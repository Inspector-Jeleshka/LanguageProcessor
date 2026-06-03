using LexicalAnalyzer;
using LexicalAnalyzer.Tokens;
using SyntaxAnalyzer.Ast;
using SyntaxAnalyzer.Semantics;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SyntaxAnalyzer;

public interface IParserState
{
	(IParserState? NextState, string? Expected) Match(IToken token);
}

public record StartState : IParserState
{
	public (IParserState? NextState, string? Expected) Match(IToken token)
	{
		if (token is ConstKeyword)
			return (new ConstState(), null);

		return (null, "ожидалось ключевое слово 'const'");
	}
}

public record ConstState : IParserState
{
	public (IParserState? NextState, string? Expected) Match(IToken token)
	{
		if (token is Space)
			return (new SpaceState(), null);

		return (null, "после 'const' обязателен значащий пробел");
	}
}

public record SpaceState : IParserState
{
	public (IParserState? NextState, string? Expected) Match(IToken token)
	{
		if (token is Identifier)
			return (new IdentifierState(), null);

		return (null, "ожидалось имя константы (идентификатор)");
	}
}

public record IdentifierState : IParserState
{
	public (IParserState? NextState, string? Expected) Match(IToken token)
	{
		if (token is Colon)
			return (new ColonState(), null);

		return (null, "ожидалось двоеточие ':'");
	}
}

public record ColonState : IParserState
{
	public (IParserState? NextState, string? Expected) Match(IToken token)
	{
		if (token is F32Keyword)
			return (new F32State(), null);

		return (null, "ожидался тип 'f32'");
	}
}

public record F32State : IParserState
{
	public (IParserState? NextState, string? Expected) Match(IToken token)
	{
		if (token is AssignmentOperator)
			return (new AssignmentState(), null);

		return (null, "ожидался оператор присваивания '='");
	}
}

public record AssignmentState : IParserState
{
	public (IParserState? NextState, string? Expected) Match(IToken token)
	{
		if (token is IntLiteral or FloatLiteral)
			return (new NumberState(), null);

		return (null, "ожидалось числовое значение");
	}
}

public record NumberState : IParserState
{
	public (IParserState? NextState, string? Expected) Match(IToken token)
	{
		if (token is Semicolon)
			return (new StartState(), null);

		return (null, "ожидалась точка с запятой ';'");
	}
}

public record ParseError(string Value, int Line, (int Start, int End) Columns, string Description)
{
	public string Location { get; } = $"Линия {Line}, {Columns.Start}-{Columns.End}";
}

public class Parser
{
	private enum ParserStateKind
	{
		Start,
		Const,
		Space,
		Identifier,
		Colon,
		F32,
		Assignment,
		Number
	}

	private enum RepairAction
	{
		Delete,
		Replace,
		Insert
	}

	private readonly List<ParseError> _errors = new();
	private readonly Dictionary<(ParserStateKind Kind, int Index), int> _costMemo = new();
	private readonly HashSet<(ParserStateKind Kind, int Index)> _inProgress = new();
	private readonly SymbolTable _symbols = new();

	private static ParserStateKind GetKind(IParserState state) => state switch
	{
		StartState => ParserStateKind.Start,
		ConstState => ParserStateKind.Const,
		SpaceState => ParserStateKind.Space,
		IdentifierState => ParserStateKind.Identifier,
		ColonState => ParserStateKind.Colon,
		F32State => ParserStateKind.F32,
		AssignmentState => ParserStateKind.Assignment,
		NumberState => ParserStateKind.Number,
		_ => ParserStateKind.Start
	};

	private static IParserState CreateState(ParserStateKind kind) => kind switch
	{
		ParserStateKind.Start => new StartState(),
		ParserStateKind.Const => new ConstState(),
		ParserStateKind.Space => new SpaceState(),
		ParserStateKind.Identifier => new IdentifierState(),
		ParserStateKind.Colon => new ColonState(),
		ParserStateKind.F32 => new F32State(),
		ParserStateKind.Assignment => new AssignmentState(),
		ParserStateKind.Number => new NumberState(),
		_ => new StartState()
	};

	private static ParserStateKind GetRecoveryKind(ParserStateKind kind) => kind switch
	{
		ParserStateKind.Start => ParserStateKind.Const,
		ParserStateKind.Const => ParserStateKind.Space,
		ParserStateKind.Space => ParserStateKind.Identifier,
		ParserStateKind.Identifier => ParserStateKind.Colon,
		ParserStateKind.Colon => ParserStateKind.F32,
		ParserStateKind.F32 => ParserStateKind.Assignment,
		ParserStateKind.Assignment => ParserStateKind.Number,
		ParserStateKind.Number => ParserStateKind.Start,
		_ => ParserStateKind.Start
	};

	private static bool ShouldSkipInMainLoop(IParserState state, IToken token)
		=> token is Space && state is not ConstState;

	private static string GetEofDescription(IParserState state) => state switch
	{
		StartState => "неожиданный конец файла, ожидалось ключевое слово 'const'",
		ConstState => "неожиданный конец файла, выражение не завершено",
		SpaceState => "неожиданный конец файла, выражение не завершено",
		IdentifierState => "неожиданный конец файла, выражение не завершено",
		ColonState => "неожиданный конец файла, выражение не завершено",
		F32State => "неожиданный конец файла, выражение не завершено",
		AssignmentState => "неожиданный конец файла, выражение не завершено",
		NumberState => "неожиданный конец файла, ожидалась ';'",
		_ => "неожиданный конец файла"
	};

	private int GetCost(IReadOnlyList<IToken> tokens, ParserStateKind kind, int index)
	{
		if (index >= tokens.Count)
			return kind == ParserStateKind.Start ? 0 : 1;

		var key = (kind, index);
		if (_costMemo.TryGetValue(key, out var cached))
			return cached;

		if (!_inProgress.Add(key))
			return int.MaxValue / 4;

		try
		{
			var token = tokens[index];

			if (token is EndOfFile)
			{
				var eofCost = kind == ParserStateKind.Start ? 0 : 1;
				_costMemo[key] = eofCost;
				return eofCost;
			}

			if (token is ErrorToken || (token is Space && kind != ParserStateKind.Const))
			{
				var skipCost = GetCost(tokens, kind, index + 1);
				_costMemo[key] = skipCost;
				return skipCost;
			}

			var state = CreateState(kind);
			var (nextState, _) = state.Match(token);

			int cost;

			if (nextState is not null)
			{
				cost = GetCost(tokens, GetKind(nextState), index + 1);
			}
			else
			{
				var recoveryKind = GetRecoveryKind(kind);

				int deleteCost = 1 + GetCost(tokens, kind, index + 1);
				int replaceCost = 1 + GetCost(tokens, recoveryKind, index + 1);
				int insertCost = 1 + GetCost(tokens, recoveryKind, index);

				cost = deleteCost;

				if (replaceCost < cost)
					cost = replaceCost;

				if (insertCost < cost)
					cost = insertCost;
			}

			_costMemo[key] = cost;
			return cost;
		}
		finally
		{
			_inProgress.Remove(key);
		}
	}

	private void Recover(IReadOnlyList<IToken> tokens, ref int i, ref IParserState state)
	{
		// Защита от бесконечного цикла
		int guard = tokens.Count * 4 + 16;

		while (i < tokens.Count && guard-- > 0)
		{
			var token = tokens[i];

			if (token is ErrorToken || (token is Space && state is not ConstState))
			{
				i++;
				continue;
			}

			if (token is EndOfFile)
				return;

			if (token is ErrorToken)
			{
				i++;
				continue;
			}

			if (token is Space && state is not ConstState)
			{
				i++;
				continue;
			}

			var (nextState, _) = state.Match(token);
			if (nextState is not null)
			{
				state = nextState;
				i++;
				return;
			}

			var currentKind = GetKind(state);
			var recoveryKind = GetRecoveryKind(currentKind);

			int deleteCost = 1 + GetCost(tokens, currentKind, i + 1);
			int replaceCost = 1 + GetCost(tokens, recoveryKind, i + 1);
			int insertCost = 1 + GetCost(tokens, recoveryKind, i);

			var bestAction = RepairAction.Delete;
			var bestCost = deleteCost;

			if (replaceCost < bestCost)
			{
				bestCost = replaceCost;
				bestAction = RepairAction.Replace;
			}

			if (insertCost < bestCost)
			{
				bestCost = insertCost;
				bestAction = RepairAction.Insert;
			}

			switch (bestAction)
			{
				case RepairAction.Delete:
					i++;
					break;

				case RepairAction.Replace:
					state = CreateState(recoveryKind);
					i++;
					break;

				case RepairAction.Insert:
					state = CreateState(recoveryKind);
					break;
			}
		}
	}

	public bool TryParse(IEnumerable<IToken> tokens, out List<ParseError> errors)
	{
		_errors.Clear();
		_costMemo.Clear();
		_inProgress.Clear();

		var list = tokens.ToList();
		IParserState state = new StartState();
		int i = 0;

		bool recoveringConst = false;

		while (i < list.Count)
		{
			var token = list[i];

			if (token is EndOfFile)
			{
				if (state is not StartState)
				{
					_errors.Add(new ParseError(
						string.Empty,
						token.Line,
						token.Columns,
						GetEofDescription(state)
					));
				}

				break;
			}

			// После ошибки в начале выражения всё до первого пробела считаем
			// неудачной попыткой написать const.
			if (recoveringConst)
			{
				if (token is Space)
				{
					recoveringConst = false;
					state = new SpaceState();
					i++;
				}
				else
				{
					i++;
				}

				continue;
			}

			// Незначащие пробелы и ErrorToken вне const-области игнорируем.
			if (ShouldSkipInMainLoop(state, token))
			{
				i++;
				continue;
			}

			var (nextState, expected) = state.Match(token);

			if (nextState is not null)
			{
				state = nextState;
				i++;
				continue;
			}

			_errors.Add(new ParseError(
				token.ToString(),
				token.Line,
				token.Columns,
				expected ?? "неизвестная ошибка"
			));

			if (state is StartState)
			{
				recoveringConst = true;
				i++;
				continue;
			}

			Recover(list, ref i, ref state);
		}

		errors = [.. _errors];
		return _errors.Count == 0;
	}

	private static bool IsIgnorable(IToken token)
		=> token is Space or ErrorToken or EndOfFile;

	private static string GetTokenText(IToken token)
	{
		//var type = token.GetType();
		//foreach (var propName in new[] { "Value", "Text", "Lexeme", "Name" })
		//{
		//	var prop = type.GetProperty(propName);
		//	if (prop?.GetValue(token) is string s && !string.IsNullOrWhiteSpace(s))
		//		return s;
		//}

		return token.ToString();
	}

	private static bool TryParseFloatLiteral(string text, out float value)
	{
		//text = text.Replace('_', ' ');
		//text = text.Replace(" ", string.Empty);

		return float.TryParse(
			text,
			NumberStyles.Float,
			CultureInfo.InvariantCulture,
			out value
		) && !float.IsNaN(value) && !float.IsInfinity(value);
	}

	private static bool IsConstStart(IToken token) => token is ConstKeyword;
	private static bool IsIdentifier(IToken token) => token is Identifier;
	private static bool IsColon(IToken token) => token is Colon;
	private static bool IsF32(IToken token) => token is F32Keyword;
	private static bool IsAssign(IToken token) => token is AssignmentOperator;
	private static bool IsLiteral(IToken token) => token is IntLiteral or FloatLiteral;
	private static bool IsSemicolon(IToken token) => token is Semicolon;

	private bool TryReadConstDeclaration(
		IReadOnlyList<IToken> tokens,
		ref int i,
		CompilationUnitNode root,
		List<ParseError> errors)
	{
		int start = i;

		if (!IsConstStart(tokens[i]))
		{
			errors.Add(new ParseError(
				GetTokenText(tokens[i]),
				tokens[i].Line,
				tokens[i].Columns,
				"ожидалось ключевое слово 'const'"
			));
			i++;
			return false;
		}

		var constToken = tokens[i++];
		if (i >= tokens.Count || !IsIdentifier(tokens[i]))
		{
			var bad = i < tokens.Count ? tokens[i] : constToken;
			errors.Add(new ParseError(
				i < tokens.Count ? GetTokenText(bad) : string.Empty,
				bad.Line,
				bad.Columns,
				"ожидалось имя константы (идентификатор)"
			));
			return false;
		}

		var idToken = (Identifier)tokens[i++];
		if (i >= tokens.Count || !IsColon(tokens[i]))
		{
			var bad = i < tokens.Count ? tokens[i] : idToken;
			errors.Add(new ParseError(
				i < tokens.Count ? GetTokenText(bad) : string.Empty,
				bad.Line,
				bad.Columns,
				"ожидалось двоеточие ':'"
			));
			return false;
		}

		i++; // ':'

		if (i >= tokens.Count || !IsF32(tokens[i]))
		{
			var bad = i < tokens.Count ? tokens[i] : idToken;
			errors.Add(new ParseError(
				i < tokens.Count ? GetTokenText(bad) : string.Empty,
				bad.Line,
				bad.Columns,
				"ожидался тип 'f32'"
			));
			return false;
		}

		i++; // f32

		if (i >= tokens.Count || !IsAssign(tokens[i]))
		{
			var bad = i < tokens.Count ? tokens[i] : idToken;
			errors.Add(new ParseError(
				i < tokens.Count ? GetTokenText(bad) : string.Empty,
				bad.Line,
				bad.Columns,
				"ожидался оператор присваивания '='"
			));
			return false;
		}

		i++; // '='

		if (i >= tokens.Count || !IsLiteral(tokens[i]))
		{
			var bad = i < tokens.Count ? tokens[i] : idToken;
			errors.Add(new ParseError(
				i < tokens.Count ? GetTokenText(bad) : string.Empty,
				bad.Line,
				bad.Columns,
				"инициализатор должен быть литералом"
			));
			return false;
		}

		var valueToken = tokens[i++];
		var literalText = GetTokenText(valueToken);
		var hasSemanticError = false;

		if (valueToken is not FloatLiteral)
		{
			errors.Add(new ParseError(
				literalText,
				valueToken.Line,
				valueToken.Columns,
				"для типа 'f32' ожидается вещественный литерал"
			));
			hasSemanticError = true;
		}

		if (!TryParseFloatLiteral(literalText, out var value))
		{
			errors.Add(new ParseError(
				literalText,
				valueToken.Line,
				valueToken.Columns,
				"значение выходит за пределы допустимого диапазона f32"
			));
			hasSemanticError = true;
		}

		if (i >= tokens.Count || !IsSemicolon(tokens[i]))
		{
			var bad = i < tokens.Count ? tokens[i] : valueToken;
			errors.Add(new ParseError(
				i < tokens.Count ? GetTokenText(bad) : string.Empty,
				bad.Line,
				bad.Columns,
				"ожидалась точка с запятой ';'"
			));
			return false;
		}

		var semiToken = tokens[i++];

		if (hasSemanticError)
			return true;

		if (!_symbols.Declare(
				new SymbolInfo(GetTokenText(idToken), "f32", value, idToken.Line, idToken.Columns),
				out var duplicateError))
		{
			errors.Add(new ParseError(
				GetTokenText(idToken),
				idToken.Line,
				idToken.Columns,
				duplicateError
			));

			return true;
		}

		var literalNode = new LiteralNode("FloatLiteral", literalText, value, valueToken.Line, valueToken.Columns);
		var constNode = new ConstDeclNode(
			GetTokenText(idToken),
			"f32",
			literalNode,
			constToken.Line,
			constToken.Columns
		);
		
		root.Add(constNode);
		return true;
	}

	public bool TryBuildAst(IEnumerable<IToken> tokens, out AstNode? ast, out List<ParseError> errors)
	{
		errors = new List<ParseError>();
		ast = null;
		_symbols.Clear();

		var list = tokens.Where(t => !IsIgnorable(t)).ToList();
		if (list.Count == 0)
		{
			errors.Add(new ParseError(string.Empty, 1, (1, 1), "пустой входной поток"));
			return false;
		}

		var root = new CompilationUnitNode(list[0].Line, list[0].Columns);

		int i = 0;
		while (i < list.Count)
		{
			if (!IsConstStart(list[i]))
			{
				errors.Add(new ParseError(
					GetTokenText(list[i]),
					list[i].Line,
					list[i].Columns,
					"ожидалось ключевое слово 'const'"
				));

				i++;
				continue;
			}

			// Разбор одного объявления.
			TryReadConstDeclaration(list, ref i, root, errors);

			// Если после объявления остались лишние токены до следующего const,
			// не останавливаемся, а продолжаем поиск следующего объявления.
			while (i < list.Count && !IsConstStart(list[i]))
			{
				errors.Add(new ParseError(
					GetTokenText(list[i]),
					list[i].Line,
					list[i].Columns,
					"после объявления константы ожидалось следующее объявление 'const' или конец файла"
				));
				i++;
			}
		}

		ast = root;
		return errors.Count == 0;
	}

	public bool TryParseWithAst(IEnumerable<IToken> tokens, out AstNode? ast, out List<ParseError> errors)
	{
		_symbols.Clear();

		var syntaxOk = TryParse(tokens, out var syntaxErrors);

		errors = [.. syntaxErrors];
		ast = null;

		if (!syntaxOk)
			return false;

		if (!TryBuildAst(tokens, out ast, out var semanticErrors))
		{
			errors.AddRange(semanticErrors);
			return false;
		}

		errors.AddRange(semanticErrors);
		return errors.Count == 0;
	}
}
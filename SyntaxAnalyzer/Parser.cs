using LexicalAnalyzer;
using LexicalAnalyzer.Tokens;
using System.Collections.Generic;
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

public record ParseError(string Value, int Line, (int Start, int End) Columns, string Description);

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
}
using LexicalAnalyzer.Tokens;

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
	private readonly List<ParseError> _errors = new();

	private static IParserState GetRecoveryState(IParserState state) => state switch
	{
		ConstState => new SpaceState(),
		SpaceState => new IdentifierState(),
		IdentifierState => new ColonState(),
		ColonState => new F32State(),
		F32State => new AssignmentState(),
		AssignmentState => new NumberState(),
		NumberState => new StartState(),
		_ => new StartState()
	};

	private static string GetEofDescription(IParserState state) => $"неожиданный конец файла, {state switch
	{
		StartState => "ожидалось ключевое слово 'const'",
		ConstState or SpaceState or IdentifierState
			or ColonState or F32State or AssignmentState => "выражение не завершено",
		NumberState => "ожидалась ';'",
		_ => ""
	}}";

	private static bool CanMatch(IParserState state, IToken token)
		=> state.Match(token).NextState is not null;

	public bool TryParse(IList<IToken> tokens, out List<ParseError> errors)
	{
		_errors.Clear();

		IParserState state = new StartState();
		var skipUntilSemicolon = false;
		var i = 0;

		while (i < tokens.Count)
		{
			var token = tokens[i];

			if (token is EndOfFile)
			{
				if (!skipUntilSemicolon && state is not StartState)
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

			if (skipUntilSemicolon)
			{
				if (token is Semicolon)
				{
					skipUntilSemicolon = false;
					state = new StartState();
				}
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
				skipUntilSemicolon = true;
				i++;
				continue;
			}

			var nextToken = (i + 1 < tokens.Count) ? tokens[i + 1] : null;
			var insertState = GetRecoveryState(state);

			var deleteWorks = nextToken != null && CanMatch(state, nextToken);
			var insertWorks = CanMatch(insertState, token);
			var replaceWorks = nextToken != null && CanMatch(insertState, nextToken);

			if (deleteWorks)
			{
				i++;
			}
			else if (insertWorks)
			{
				state = insertState;
			}
			else if (replaceWorks)
			{
				state = insertState;
				i++;
			}
			else
			{
				state = insertState;
			}
		}

		errors = [.. _errors];
		return _errors.Count == 0;
	}
}
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
	private IParserState? _recoveryOrigin;

	public bool TryParse(IEnumerable<IToken> tokens, out List<ParseError> errors)
	{
		_errors.Clear();
		_recoveryOrigin = null;

		using var enumerator = tokens.GetEnumerator();
		IParserState state = new StartState();

		while (enumerator.MoveNext())
		{
			var token = enumerator.Current;

			if (_recoveryOrigin is not null)
			{
				if (token is Semicolon or ConstKeyword)
				{
					state = _recoveryOrigin;
					_recoveryOrigin = null;
					continue;
				}
				continue;
			}

			var (nextState, expected) = state.Match(token);
			if (nextState is not null)
			{
				state = nextState;
			}
			else
			{
				_errors.Add(new ParseError(
					token.ToString(),
					token.Line,
					token.Columns,
					expected ?? "неизвестная ошибка"
				));

				_recoveryOrigin = state;
			}
		}

		if (_recoveryOrigin is not null)
		{
			_errors.Add(new ParseError(
				string.Empty,
				-1,
				(-1, -1),
				"неожиданный конец файла, выражение не завершено"
			));
		}
		else if (state is not StartState)
		{
			_errors.Add(new ParseError(
				string.Empty,
				-1,
				(-1, -1),
				"неожиданный конец файла, ожидалась ';'"
			));
		}

		errors = [.. _errors];
		return _errors.Count == 0;
	}
}
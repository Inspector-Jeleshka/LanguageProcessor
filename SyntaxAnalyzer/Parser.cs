using LexicalAnalyzer;
using LexicalAnalyzer.Tokens;

namespace SyntaxAnalyzer;

public interface IParserState
{
	IParserState Match(IToken token);
}

public interface IParserState<TParserState> : IParserState
	where TParserState : IParserState
{
	new TParserState Match(IToken token);
	IParserState IParserState.Match(IToken token) => Match(token);
}

public record ConstState : IParserState<SpaceState>
{
	public SpaceState Match(IToken token) => token switch
	{
		Space space => new SpaceState(),
		_ => throw new NotImplementedException(),
	};
}
public record SpaceState : IParserState<IdentifierState>;
public record IdentifierState : IParserState<ColonState>;
public record ColonState : IParserState<F32State>;
public record F32State : IParserState<AssignmentState>;
public record AssignmentState : IParserState<NumberState>;
public record NumberState : IParserState<SemicolonState>;
public record SemicolonState : IParserState<ConstState>;

public record ParseError(string Value, int Line, (int Start, int End) Columns, string Description);

public class Parser
{
	private IParserState _state;

	public int Line { get; private set; }
	public int Column { get; private set; }

	public bool TryParse(IEnumerable<IToken> tokens, out List<ParseError> errors)
	{
		Line = 1;
		Column = 1;

		IParserState state = new ConstState();
		state.Match()

		throw new NotImplementedException();
	}
}

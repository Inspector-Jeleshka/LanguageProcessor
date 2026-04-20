using LexicalAnalyzer;
using LexicalAnalyzer.Tokens;

namespace SyntaxAnalyzer;

//public enum ParserState
//{
//	Start,
//	Id,
//	IdEnd,
//	Type,
//	Assignment,
//	Float,
//	Int,
//	Fraction,
//	End,
//}

public record ParserState;

public record ConstState : ParserState;
public record SpaceState : ParserState;
public record IdentifierState : ParserState;
public record ColonState : ParserState;
public record F32State : ParserState;
public record AssignmentState : ParserState;
public record NumberState : ParserState;
public record SemicolonState : ParserState;

public class Parser
{
	private ParserState _state;

	public int Line { get; private set; }
	public int Column { get; private set; }

	public bool TryParse(IEnumerable<IToken> tokens)
	{
		Line = 1;
		Column = 1;

		throw new NotImplementedException();
	}
}

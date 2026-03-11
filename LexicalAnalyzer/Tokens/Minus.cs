namespace LexicalAnalyzer.Tokens;

public class Minus : IToken
{
	public int Code => 11;

	public override string ToString() => "-";
}

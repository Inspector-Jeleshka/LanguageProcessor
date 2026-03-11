namespace LexicalAnalyzer.Tokens;

public class Plus : IToken
{
	public int Code => 10;

	public override string ToString() => "+";
}

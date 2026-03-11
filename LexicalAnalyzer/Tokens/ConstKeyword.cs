namespace LexicalAnalyzer.Tokens;

public class ConstKeyword : IToken
{
	public int Code => 1;

	public override string ToString() => "const";

}

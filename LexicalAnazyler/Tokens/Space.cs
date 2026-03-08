namespace LexicalAnazyler.Tokens;

public class Space : IToken
{
	public int Code => 6;

	public override string ToString() => " ";
}

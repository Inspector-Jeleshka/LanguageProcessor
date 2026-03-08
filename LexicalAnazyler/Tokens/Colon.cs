namespace LexicalAnazyler.Tokens;

public class Colon : IToken
{
	public int Code => 7;

	public override string ToString() => ":";
}

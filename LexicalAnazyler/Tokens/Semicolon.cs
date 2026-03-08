namespace LexicalAnazyler.Tokens;

public class Semicolon : IToken
{
	public int Code => 9;

	public override string ToString() => ";";
}

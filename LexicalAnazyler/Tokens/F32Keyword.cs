namespace LexicalAnazyler.Tokens;

public class F32Keyword : IToken
{
	public int Code => 3;

	public override string ToString() => "f32";
}

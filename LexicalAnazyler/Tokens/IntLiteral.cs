namespace LexicalAnazyler.Tokens;

public class IntLiteral(int value) : IToken
{
	public int Code => 4;
	public int Value { get; } = value;

	public override string ToString() => Value.ToString();
}

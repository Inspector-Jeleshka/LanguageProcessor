namespace LexicalAnazyler.Tokens;

public class FloatLiteral(float value) : IToken
{
	public int Code => 5;
	public float Value { get; } = value;

	public override string ToString() => Value.ToString();
}

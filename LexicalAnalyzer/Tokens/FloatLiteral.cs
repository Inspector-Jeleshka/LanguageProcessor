namespace LexicalAnalyzer.Tokens;

public class FloatLiteral(int line, (int, int) columns, float value) : IToken
{
	public int Code => 5;

	public int Line => line;

	public (int, int) Columns => columns;

	public string Name => "вещественное без знака"; 
	public float Value { get; } = value;

	public override string ToString() => Value.ToString();
}

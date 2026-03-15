namespace LexicalAnalyzer.Tokens;

public class IntLiteral(int line, (int, int) columns, int value) : IToken
{
	public int Code => 4;

	public int Line => line;

	public (int, int) Columns => columns;

	public string Name => "целое без знака";
	public int Value { get; } = value;

	public override string ToString() => Value.ToString();
}

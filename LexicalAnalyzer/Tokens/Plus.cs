namespace LexicalAnalyzer.Tokens;

public class Plus(int line, (int, int) columns) : IToken
{
	public int Code => 10;

	public int Line => line;

	public (int, int) Columns => columns;

	public string Name => "оператор унарного плюса";

	public override string ToString() => "+";
}

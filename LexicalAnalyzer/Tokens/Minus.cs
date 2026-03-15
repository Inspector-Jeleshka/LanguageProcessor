namespace LexicalAnalyzer.Tokens;

public class Minus(int line, (int, int) columns) : IToken
{
	public int Code => 11;

	public int Line => line;

	public (int, int) Columns => columns;

	public string Name => "оператор унарного минуса";

	public override string ToString() => "-";
}

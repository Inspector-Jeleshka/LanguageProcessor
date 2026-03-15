namespace LexicalAnalyzer.Tokens;

public class Colon(int line, (int, int) columns) : IToken
{
	public int Code => 7;

	public int Line => line;

	public (int, int) Columns => columns;

	public string Name => "оператор ограничения типа";

	public override string ToString() => ":";
}

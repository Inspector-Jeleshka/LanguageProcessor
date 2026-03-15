namespace LexicalAnalyzer.Tokens;

public class Semicolon(int line, (int, int) columns) : IToken
{
	public int Code => 9;

	public int Line => line;

	public (int, int) Columns => columns;

	public string Name => "конец оператора";

	public override string ToString() => ";";
}

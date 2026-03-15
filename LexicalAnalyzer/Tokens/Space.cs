namespace LexicalAnalyzer.Tokens;

public class Space(int line, (int, int) columns) : IToken
{
	public int Code => 6;

	public int Line => line;

	public (int, int) Columns => columns;

	public string Name => "разделитель (пробел)";

	public override string ToString() => " ";
}

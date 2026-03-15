namespace LexicalAnalyzer.Tokens;

public class Identifier(int line, (int, int) columns, string value) : IToken
{
	public int Code => 2;
	public int Line => line;
	public (int, int) Columns => columns;
	public string Name => "идентификатор";
	public string Value { get; } = value;

	public override string ToString() => Name;
}

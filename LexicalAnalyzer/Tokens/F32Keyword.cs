namespace LexicalAnalyzer.Tokens;

public class F32Keyword(int line, (int, int) columns) : IToken
{
	public int Code => 3;

	public int Line => line;

	public (int, int) Columns => columns;

	public string Name => "ключевое слово f32";

	public override string ToString() => "f32";
}

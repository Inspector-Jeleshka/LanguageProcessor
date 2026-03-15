namespace LexicalAnalyzer.Tokens;

public class ConstKeyword(int line, (int, int) columns) : IToken
{
	public int Code => 1;

	public int Line => line;

	public (int, int) Columns => columns;

	public string Name => "ключевое слово const";

	public override string ToString() => "const";

}

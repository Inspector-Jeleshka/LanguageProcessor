namespace LexicalAnalyzer.Tokens;

public class EndOfFile(int line, (int, int) columns) : IToken
{
	public int Code => 12;

	public int Line => line;

	public (int, int) Columns => columns;

	public string Name => "идентификатор конца файла";

	public override string ToString() => "End-Of-File";
}

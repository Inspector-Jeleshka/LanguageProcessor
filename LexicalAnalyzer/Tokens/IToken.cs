namespace LexicalAnalyzer.Tokens;

public interface IToken
{
	public int Code { get; }
	public int Line { get; }
	public (int, int) Columns { get; }
	public string Name { get; }
	public string ToString();
}

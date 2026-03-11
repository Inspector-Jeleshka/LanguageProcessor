namespace LexicalAnalyzer.Tokens;

public interface IToken
{
	public int Code { get; }
	public string ToString();
}

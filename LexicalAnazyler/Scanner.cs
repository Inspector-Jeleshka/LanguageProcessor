using LexicalAnazyler.Tokens;

namespace LexicalAnazyler;

public class Scanner
{
	public int Line { get; private set; }
	public int Column { get; private set; }

	public IEnumerable<IToken> Scan(string input)
	{
		return null;
	}
}

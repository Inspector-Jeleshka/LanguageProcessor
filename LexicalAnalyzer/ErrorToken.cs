using LexicalAnalyzer.Tokens;

namespace LexicalAnalyzer;

public class ErrorToken : Exception, IToken
{
	public int Code => -1;
	public int Line { get; }
	public (int, int) Columns { get; }
	public string Name => "ошибка";
	public string Value => Message;

	public ErrorToken(int line, (int, int) columns, string message) : base(message)
		=> (Line, Columns) = (line, columns);

	public override string ToString() => Value;
}

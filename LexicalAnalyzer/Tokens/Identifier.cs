namespace LexicalAnalyzer.Tokens;

public class Identifier(string name) : IToken
{
	public int Code => 2;
	public string Name { get; } = name;

	public override string ToString() => Name;
}

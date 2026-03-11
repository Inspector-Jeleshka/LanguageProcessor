namespace LexicalAnalyzer.Tokens;

public class AssignmentOperator : IToken
{
	public int Code => 8;

	public override string ToString() => "=";

}

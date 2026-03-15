namespace LexicalAnalyzer.Tokens;

public class AssignmentOperator(int line, (int, int) columns) : IToken
{
	public int Code => 8;

	public int Line => line;

	public (int, int) Columns => columns;

	public string Name => "оператор присваивания";

	public override string ToString() => "=";

}

namespace GUI.Models;

public record SyntaxInfo(string Value, int Line, (int Start, int End) Columns, string Description)
{
	public string Location => $"Линия {Line}, {Columns.Start}-{Columns.End}";
}

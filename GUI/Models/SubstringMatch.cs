using System.Text.RegularExpressions;

namespace GUI.Models;

public class SubstringMatch
{
	private readonly Match _match;

	public string Value => _match.Value;
	public int Index => _match.Index;
	public int Length => _match.Length;
	public string Location => $"Линия {Line}, {Columns.Start}-{Columns.End}";
	public int Line { get; }
	public (int Start, int End) Columns { get; }

	public SubstringMatch(Match match, int line, (int Start, int End) columns)
		=> (_match, Line, Columns) = (match, line, columns);

	public override string ToString() => Value;
}

using System.Text.RegularExpressions;

namespace GUI.Models;

public record SubstringMatch(string Value, int Index, int Length, int Line, (int Start, int End) Columns)
{
	public string Value { get; init; } = Value;
	public int Index { get; init; } = Index;
	public int Length { get; init; } = Length;
	public string Location => $"Линия {Line}, {Columns.Start}-{Columns.End}";
	public int Line { get; init; } = Line;
	public (int Start, int End) Columns { get; init; } = Columns;

	public SubstringMatch(Match match, int line, (int Start, int End) columns)
		: this(match.Value, match.Index, match.Length, line, columns) { }

	public static SubstringMatch FromInput(string input, int index, int length)
	{
		var value = input.Substring(index, length);
		var textBeforeMatch = input[..index];
		var line = textBeforeMatch.Count(c => c == '\n') + 1;
		var lineStart = textBeforeMatch.LastIndexOf('\n');
		var columns = (index - lineStart, index - lineStart + length - 1);

		return new(value, index, length, line, columns);
	}

	public override string ToString() => Value;
}

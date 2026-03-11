namespace LexicalAnalyzer;

public class ScannerException : Exception
{
	public int Line { get; }
	public (int, int) Columns { get; }

	public ScannerException(int line, (int, int) columns, string message) : base(message)
		=> (Line, Columns) = (line, columns);
}

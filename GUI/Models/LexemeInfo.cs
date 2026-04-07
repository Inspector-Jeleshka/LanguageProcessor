using LexicalAnalyzer;
using LexicalAnalyzer.Tokens;

namespace GUI.Models;

internal class LexemeInfo
{
	public int Code { get; }
	public string Type { get; }
	public string Value { get; }
	public string Location => $"Линия {Line}, {Columns.Start}-{Columns.End}";
	public int Line { get; }
	public (int Start, int End) Columns { get; }

	public LexemeInfo(IToken token)
	{
		Code = token.Code;
		Type = token.Name;
		Value = token.ToString();
		Line = token.Line;
		Columns = token.Columns;
	}
	public LexemeInfo(ScannerException exception)
	{
		Code = -1;
		Type = "Ошибка";
		Value = exception.Message;
		Line = exception.Line;
		Columns = exception.Columns;
	}

}

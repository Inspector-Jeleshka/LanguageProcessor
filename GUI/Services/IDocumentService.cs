using System.Windows.Documents;

namespace GUI.Services;

public interface IDocumentService
{
	public FlowDocument Document { get; set; }
	public string Text { get; set; }
	public TextPointer? GetTextPointerAt(int targetLine, int targetColumn);
	public TextPointer? GetTextPointerAt(int targetIndex);
	public int GetIndexAt(int targetLine, int targetColumn);
	public (int Line, int Column) GetLineColumnAt(int targetIndex);
}

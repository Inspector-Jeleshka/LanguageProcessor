using System.Windows.Documents;

namespace GUI.Services;

public interface IDocumentService
{
	public FlowDocument Document { get; set; }
	public string Text { get; set; }
	TextPointer? FindTextPointerByIndex(int targetIndex);
}

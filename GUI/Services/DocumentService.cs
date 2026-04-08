using System.Windows.Documents;

namespace GUI.Services;

public class DocumentService : IDocumentService
{
	public FlowDocument Document { get; set; } = new FlowDocument();
	public string Text
	{
		get => new TextRange(Document.ContentStart, Document.ContentEnd).Text;
		set => new TextRange(Document.ContentStart, Document.ContentEnd).Text = value;
	}

	public TextPointer? FindTextPointerByIndex(int targetIndex)
	{
		TextPointer current = Document.ContentStart;
		int currentIndex = 0;

		while (current is not null && currentIndex <= targetIndex)
		{
			if (current.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
			{
				string textRun = current.GetTextInRun(LogicalDirection.Forward);
				int runLength = textRun.Length + "\r\n".Length;

				if (currentIndex + runLength > targetIndex)
				{
					int offset = targetIndex - currentIndex;
					return current.GetPositionAtOffset(offset);
				}

				currentIndex += runLength;
			}

			current = current.GetNextContextPosition(LogicalDirection.Forward);
		}

		return current ?? Document.ContentStart;
	}
}

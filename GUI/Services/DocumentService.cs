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

	public TextPointer? GetTextPointerAt(int targetLine, int targetColumn)
	{
		if (targetLine < 1 || targetColumn < 1)
			return null;

		foreach (var (pointer, line, column, _) in EnumerateTextPositions())
		{
			if (line == targetLine && column == targetColumn)
				return pointer.GetInsertionPosition(LogicalDirection.Forward);
		}

		return Document.ContentEnd;
	}

	public TextPointer? GetTextPointerAt(int targetIndex)
	{
		if (targetIndex < 0)
			return null;

		foreach (var (pointer, _, _, index) in EnumerateTextPositions())
		{
			if (index == targetIndex)
				return pointer.GetInsertionPosition(LogicalDirection.Forward);
		}

		return Document.ContentEnd;
	}

	public int GetIndexAt(int targetLine, int targetColumn)
	{
		foreach (var (_, line, column, index) in EnumerateTextPositions())
		{
			if (line == targetLine && column == targetColumn)
				return index;
		}

		return 0;
	}

	public (int Line, int Column) GetLineColumnAt(int targetIndex)
	{
		foreach (var (_, line, column, index) in EnumerateTextPositions())
		{
			if (index == targetIndex)
				return (line, column);
		}

		return (1, 1);
	}

	private IEnumerable<TextPosition> EnumerateTextPositions()
	{
		var navigator = Document.ContentStart;
		var currentLine = 1;
		var currentColumn = 1;
		var currentIndex = 0;

		while (navigator is not null)
		{
			var context = navigator.GetPointerContext(LogicalDirection.Forward);

			if (context == TextPointerContext.Text)
			{
				var textRun = navigator.GetTextInRun(LogicalDirection.Forward);

				for (int i = 0; i < textRun.Length; i++)
				{
					var c = textRun[i];

					if (c == '\r')
						continue;

					yield return new(navigator.GetPositionAtOffset(i), currentLine, currentColumn, currentIndex);

					if (c == '\n')
					{
						currentLine++;
						currentColumn = 1;
					}
					else
						currentColumn++;

					currentIndex++;
				}

				navigator = navigator.GetPositionAtOffset(textRun.Length, LogicalDirection.Forward);
				continue;
			}

			if (context == TextPointerContext.ElementStart &&
				navigator.GetAdjacentElement(LogicalDirection.Forward) is LineBreak)
			{
				yield return new(navigator, currentLine, currentColumn, currentIndex);

				currentLine++;
				currentColumn = 1;
				currentIndex += 2;

				navigator = navigator.GetPositionAtOffset(1, LogicalDirection.Forward);
				continue;
			}

			if (context == TextPointerContext.ElementEnd && navigator.Parent is Paragraph)
			{
				yield return new(navigator, currentLine, currentColumn, currentIndex);

				currentLine++;
				currentColumn = 1;
				currentIndex += 2;
			}

			navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
		}

		yield return new(Document.ContentEnd, currentLine, currentColumn, currentIndex);
	}
}

internal readonly record struct TextPosition(TextPointer Pointer, int Line, int Column, int Index);

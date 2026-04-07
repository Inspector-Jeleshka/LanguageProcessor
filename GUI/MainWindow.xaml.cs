using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.Input;
using GUI.Models;
using LexicalAnalyzer;
using Microsoft.Win32;

namespace GUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	private FileStream? _openFile;
	private AboutWindow? _openAboutWindow;
	private string _savedContent = string.Empty;
	private Scanner _scanner = new();
	private FileStream? OpenFile
	{
		get => _openFile;
		set
		{
			_openFile?.Dispose();
			_openFile = value;
		}
	}
	public ObservableCollection<LexemeInfo> Errors { get; } = new();

	public MainWindow()
	{
		InitializeComponent();

		DataContext = this;

		//rtb.PreviewKeyDown += (s, e) =>
		//{
		//	if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
		//	{
		//		e.Handled = true;
		//	}
		//};
	}

	private void MainWindow_Closed(object sender, EventArgs e)
	{
		OpenFile = null;
		_openAboutWindow?.Close();
	}
	private void FileCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		// Saving
		if (e.Command == ApplicationCommands.Save && OpenFile is not null)
		{
			WriteOpenFile();
			return;
		}
		else if (e.Command == ApplicationCommands.SaveAs || e.Command == ApplicationCommands.Save)
		{
			var dialog = new SaveFileDialog();
			var result = dialog.ShowDialog() ?? false;
			if (!result)
				return;

			OpenFile = null;
			OpenFile = File.Open(dialog.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);

			WriteOpenFile();
			return;
		}

		// SaveChanges
		var content = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text;
		if (content != _savedContent)
		{
			var saveChanges = MessageBox.Show("Сохранить текущие изменения в файл?", "Сохранение",
				MessageBoxButton.YesNoCancel);

			if (saveChanges == MessageBoxResult.Cancel)
				return;
			else if (saveChanges == MessageBoxResult.Yes)
			{
				if (OpenFile is null)
				{
					// SaveAs
					var dialog = new SaveFileDialog();
					var result = dialog.ShowDialog() ?? false;
					if (!result)
						return;

					OpenFile = null;
					OpenFile = File.Open(dialog.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
				}

				WriteOpenFile();
			}
		}

		if (e.Command == ApplicationCommands.New)
		{
			OpenFile = null;
			_ = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd)
			{ Text = string.Empty };
			_savedContent = string.Empty;
		}
		else if (e.Command == ApplicationCommands.Open)
		{
			try
			{
				var dialog = new OpenFileDialog();
				var result = dialog.ShowDialog() ?? false;
				if (!result)
					return;

				OpenFile = null;
				OpenFile = File.Open(dialog.FileName, FileMode.Open, FileAccess.ReadWrite);

				ReadOpenFile();
			}
			catch (UnauthorizedAccessException)
			{
				_ = MessageBox.Show("Ошибка: указанный файл доступен только для чтения", "Ошибка",
					MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		else if (e.Command == ApplicationCommands.Close)
		{
			Close();
		}
	}
	private void HelpCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		var helpContent = """
			Раздел "Файл":
			- Создать - создание нового документа (очистка области редактирования)
			- Открыть - загрузка существующего файла с диска
			- Сохранить - сохранение текущего документа
			- Сохранить как - сохранение документа с указанием нового имени/пути
			- Выход - завершение работы приложения (с проверкой несохраненных изменений)

			Раздел "Правка":
			- Отменить - отмена последнего действия (Ctrl+Z)
			- Повторить - повтор отмененного действия (Ctrl+Y)
			- Вырезать - удаление выделенного текста в буфер обмена (Ctrl+X)
			- Копировать - копирование выделенного текста в буфер обмена (Ctrl+C)
			- Вставить - вставка текста из буфера обмена (Ctrl+V)
			- Удалить - удаление выделенного текста (Del)
			- Выделить все - выделение всего текста в документе (Ctrl+A)

			Раздел "Справка":
			- Вызов справки - открытие данного окна справки (F1)
			- О программе - информация о версии, авторе, лицензии
			""";

		_ = MessageBox.Show(helpContent, "Справка", MessageBoxButton.OK, MessageBoxImage.Information);
	}
	private void ShowAboutCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (_openAboutWindow is null)
		{
			_openAboutWindow = new AboutWindow();
			_openAboutWindow.Closed += (_, _) => _openAboutWindow = null;
			_openAboutWindow.Show();

			return;
		}
		_ = _openAboutWindow.Focus();
	}
	private void ReadOpenFile()
	{
		if (OpenFile is null)
			throw new InvalidOperationException($"{nameof(OpenFile)} cannot be null");

		OpenFile.Position = 0;
		using var reader = new StreamReader(OpenFile, leaveOpen: true);

		var content = reader.ReadToEnd();
		_ = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd)
		{ Text = content };
		_savedContent = content;
	}
	private void WriteOpenFile()
	{
		if (OpenFile is null)
			throw new InvalidOperationException($"{nameof(OpenFile)} cannot be null");

		OpenFile.Position = 0;
		using var writer = new StreamWriter(OpenFile, leaveOpen: true)
		{ AutoFlush = true };

		var content = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text;
		writer.Write(content);
		OpenFile.SetLength(OpenFile.Position);
		_savedContent = content;
	}
	//[RelayCommand]
	private void RunScanner(object s, EventArgs e)
	{
		Errors.Clear();

		var content = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text;

		var tokens = _scanner.Scan(content);
		foreach (var err in _scanner.Errors)
			Errors.Add(new LexemeInfo(err));
		foreach (var token in tokens)
			Errors.Add(new LexemeInfo(token));
	}
	private void Link_Click(object sender, RoutedEventArgs e)
	{
		if (dg.SelectedItem is not LexemeInfo selectedLexeme)
			return;

		var content = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text;
		var position = FindPositionInText(content, selectedLexeme.Line, selectedLexeme.Columns.Start);

		if (position >= 0)
		{
			var cursorPosition = FindTextPointerByIndex(rtb.Document.ContentStart, position);
			if (cursorPosition is null)
				return;

			rtb.CaretPosition = cursorPosition;
			rtb.Focus();
		}
	}

	private static int FindPositionInText(string text, int line, int column)
	{
		int currentLine = 1;
		int currentColumn = 1;

		for (int i = 0; i < text.Length; i++)
		{
			if (currentLine == line && currentColumn == column)
				return i;

			if (text[i] == '\n')
			{
				currentLine++;
				currentColumn = 1;
			}
			else
				currentColumn++;
		}

		return text.Length;
	}
	private static TextPointer FindTextPointerByIndex(TextPointer start, int targetIndex)
	{
		TextPointer current = start;
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

		return current ?? start;
	}
}

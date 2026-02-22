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
	private FileStream? OpenFile
	{
		get => _openFile;
		set
		{
			_openFile?.Dispose();
			_openFile = value;
		}
	}

	public MainWindow()
	{
		InitializeComponent();
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
			WriteFile();
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

			WriteFile();
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

				WriteFile();
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
			var dialog = new OpenFileDialog();
			var result = dialog.ShowDialog() ?? false;
			if (!result)
				return;

			OpenFile = null;
			OpenFile = File.Open(dialog.FileName, FileMode.Open, FileAccess.ReadWrite);

			ReadFile();
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
	private void ReadFile()
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
	private void WriteFile()
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
}

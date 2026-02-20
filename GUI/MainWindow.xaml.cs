using System.IO;
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

	private void NewCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (OpenFile is not null)
		{
			var result = MessageBox.Show("Сохранить изменения в текущем файле?", "Сохранение",
				MessageBoxButton.YesNoCancel);

			if (result == MessageBoxResult.Cancel)
				return;

			if (result == MessageBoxResult.Yes)
			{
				var dialog = new SaveFileDialog();
				var saveResult = dialog.ShowDialog() ?? false;
				if (!saveResult)
					return;

				var file = File.OpenWrite(dialog.FileName);
				using var writer = new StreamWriter(file);
				var content = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text;
				writer.Write(content);
			}
		}

		OpenFile = null;
		_ = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd)
		{ Text = string.Empty };
	}
	private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		var dialog = new OpenFileDialog();
		var result = dialog.ShowDialog() ?? false;
		if (!result)
			return;

		OpenFile = null;
		OpenFile = File.Open(dialog.FileName, FileMode.Open, FileAccess.ReadWrite);

		using var reader = new StreamReader(OpenFile, leaveOpen: true);
		var content = reader.ReadToEnd();

		_ = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd)
		{ Text = content };
	}
	private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (OpenFile is null)
		{
			var dialog = new OpenFileDialog();
			var result = dialog.ShowDialog() ?? false;
			if (!result)
				return;

			OpenFile = File.Open(dialog.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
		}

		OpenFile.Position = 0;
		using var writer = new StreamWriter(OpenFile, leaveOpen: true)
		{ AutoFlush = true };

		var content = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text;
		writer.Write(content);
		OpenFile.SetLength(OpenFile.Position);
	}
	private void SaveAsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		var dialog = new SaveFileDialog();
		var result = dialog.ShowDialog() ?? false;
		if (!result)
			return;

		OpenFile = null;
		OpenFile = File.Open(dialog.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
		OpenFile.Position = 0;
		using var writer = new StreamWriter(OpenFile, leaveOpen: true)
		{ AutoFlush = true };

		var content = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text;
		writer.Write(content);
		OpenFile.SetLength(OpenFile.Position);
	}
	private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		OpenFile = null;
		Close();
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

		MessageBox.Show(helpContent, "Справка", MessageBoxButton.OK, MessageBoxImage.Information);
	}
}

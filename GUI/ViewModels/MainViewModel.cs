using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GUI.Models;
using GUI.Services;
using LexicalAnalyzer;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace GUI.ViewModels;

public class MainViewModel : ObservableObject
{
	private readonly IFileService _fileService;
	private readonly IFileDialogService _fileDialogService;
	private readonly IDocumentService _documentService;
	private readonly IWindowService _windowService;

	private string _currentFilePath = string.Empty;
	private string _savedContent = string.Empty;
	private readonly Scanner _scanner = new();

	public FlowDocument Document => _documentService.Document;
	public ObservableCollection<LexemeInfo> Lexemes { get; } = new();

	public ICommand NewCommand { get; }
	public ICommand OpenCommand { get; }
	public ICommand SaveCommand { get; }
	public ICommand SaveAsCommand { get; }
	public ICommand CloseCommand { get; }
	public ICommand HelpCommand { get; }
	public ICommand AboutCommand { get; }
	public ICommand RunScannerCommand { get; }
	public ICommand GoToLexemeCommand { get; }

	public bool HasChanges => _savedContent != _documentService.Text;

	public event Action<TextPointer>? NavigateToPostionRequested;

	public MainViewModel(IFileService fileService, IFileDialogService fileDialogService,
		IDocumentService documentService, IWindowService windowService)
	{
		_fileService = fileService;
		_fileDialogService = fileDialogService;
		_documentService = documentService;
		_windowService = windowService;

		NewCommand = new RelayCommand(NewDocument);
		OpenCommand = new RelayCommand(OpenDocument);
		SaveCommand = new RelayCommand(SaveDocument);
		SaveAsCommand = new RelayCommand(() => SaveDocumentAs());
		CloseCommand = new RelayCommand(CloseApplication);
		HelpCommand = new RelayCommand(ShowHelp);
		AboutCommand = new RelayCommand(_windowService.ShowAboutWindow);
		RunScannerCommand = new RelayCommand(RunScanner);
		GoToLexemeCommand = new RelayCommand<LexemeInfo>(lexeme => NavigateToLexeme(lexeme));
	}

	private bool CheckUnsavedChanges()
	{
		if (!HasChanges)
			return true;

		var saveChanges = MessageBox.Show("Сохранить текущие изменения в файл?", "Сохранение",
				MessageBoxButton.YesNoCancel);

		if (saveChanges == MessageBoxResult.Cancel)
			return false;

		if (saveChanges == MessageBoxResult.Yes)
		{
			if (string.IsNullOrEmpty(_currentFilePath) && !SaveDocumentAs())
				return false;

			SaveDocument();
		}

		return true;
	}

	private void NewDocument()
	{
		if (!CheckUnsavedChanges())
			return;

		_documentService.Text = string.Empty;
		_currentFilePath = string.Empty;
		_savedContent = string.Empty;
	}

	private void OpenDocument()
	{
		if (!CheckUnsavedChanges())
			return;

		var filePath = _fileDialogService.OpenFileDialog();
		if (string.IsNullOrEmpty(filePath))
			return;

		try
		{
			var content = _fileService.ReadAllText(filePath);
			_documentService.Text = content;
			_currentFilePath = filePath;
			_savedContent = content;
		}
		catch (UnauthorizedAccessException)
		{
			_ = MessageBox.Show("Ошибка: указанный файл доступен только для чтения", "Ошибка",
					MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}

	private void SaveDocument()
	{
		if (string.IsNullOrEmpty(_currentFilePath))
		{
			_ = SaveDocumentAs();
			return;
		}

		var content = _documentService.Text;
		_fileService.WriteAllText(_currentFilePath, content);
		_savedContent = content;
	}

	private bool SaveDocumentAs()
	{
		var path = _fileDialogService.SaveFileDialog();
		if (string.IsNullOrEmpty(path))
			return false;

		_currentFilePath = path;
		SaveDocument();
		return true;
	}

	private void CloseApplication()
	{
		if (!CheckUnsavedChanges())
			return;

		Application.Current.Shutdown();
	}

	private void ShowHelp()
	{
		var helpMessage = """
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

		_ = MessageBox.Show(helpMessage, "Справка", MessageBoxButton.OK, MessageBoxImage.Information);
	}

	private void RunScanner()
	{
		Lexemes.Clear();

		var content = _documentService.Text;
		var tokens = _scanner.Scan(content);

		foreach (var error in _scanner.Errors)
			Lexemes.Add(new(error));

		foreach (var token in tokens)
			Lexemes.Add(new(token));
	}

	private void NavigateToLexeme(LexemeInfo? lexeme)
	{
		if (lexeme is null)
			return;

		var content = _documentService.Text;
		var pos = FindPositionInText(content, lexeme.Line, lexeme.Columns.Start);

		var newCaretPosition = _documentService.FindTextPointerByIndex(pos);
		if (newCaretPosition is null)
			return;

		NavigateToPostionRequested?.Invoke(newCaretPosition);
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
}

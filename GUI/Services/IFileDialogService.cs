namespace GUI.Services;

internal interface IFileDialogService
{
	string? OpenFileDialog(string filter);
	string? SaveFileDialog(string filter);
}

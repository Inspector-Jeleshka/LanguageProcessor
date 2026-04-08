namespace GUI.Services;

public interface IFileDialogService
{
	string? OpenFileDialog(string filter = "");
	string? SaveFileDialog(string filter = "");
}

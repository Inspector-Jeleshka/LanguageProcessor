using Microsoft.Win32;

namespace GUI.Services;

internal class FileDialogService : IFileDialogService
{
	public string? OpenFileDialog(string filter)
	{
		var dialog = new OpenFileDialog { Filter = filter };
		return dialog.ShowDialog() == true ? dialog.FileName : null;
	}

	public string? SaveFileDialog(string filter)
	{
		var dialog = new SaveFileDialog { Filter = filter };
		return dialog.ShowDialog() == true ? dialog.FileName : null;
	}
}

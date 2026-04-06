namespace GUI.Services;

internal interface IFileService
{
	string ReadAllText(string path);
	void WriteAllText(string path, string text);
}

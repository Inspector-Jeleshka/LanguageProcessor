namespace GUI.Services;

public interface IFileService
{
	string ReadAllText(string path);
	void WriteAllText(string path, string text);
}

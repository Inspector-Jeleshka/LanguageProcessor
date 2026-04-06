using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GUI.Services;

internal class FileService : IFileService
{
	public string ReadAllText(string path)
	{
		return File.ReadAllText(path);
	}

	public void WriteAllText(string path, string text)
	{
		File.WriteAllText(path, text);
	}
}

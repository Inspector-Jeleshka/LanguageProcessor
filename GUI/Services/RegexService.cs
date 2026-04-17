using GUI.Models;
using System.Text.RegularExpressions;

namespace GUI.Services;

public class RegexService
{
	public List<SubstringMatch> FindSubstrings(string input, SubstringTemplate template)
	{
		var result = new List<SubstringMatch>();
		var matches = template.Template.Matches(input);
		foreach (Match match in matches)
		{
			var textBeforeMatch = input[..match.Index];
			var line = textBeforeMatch.Count(c => c == '\n') + 1;
			var lineStart = textBeforeMatch.LastIndexOf('\n');
			var columns = (match.Index - lineStart, match.Index - lineStart + match.Length - 1);

			result.Add(new(match, line, columns));
		}

		return result;
	}
}

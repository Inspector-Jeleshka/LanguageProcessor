using GUI.Models;
using System.Text.RegularExpressions;

namespace GUI.Services;

public class RegexService
{
	public List<SubstringMatch> FindSubstrings(string input, SubstringTemplate template)
	{
		if (template == SubstringTemplate.Number)
			return FindNumberSubstrings(input);

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

	public List<SubstringMatch> FindNumberSubstrings(string input)
	{
		var result = new NumberParser().Parse(input);
		return result;
	}
}

public static class SubstringHelper
{
	public static SubstringMatch FromInput(string input, string value, int index, int length)
	{
		var textBeforeMatch = input[..index];
		var line = textBeforeMatch.Count(c => c == '\n') + 1;
		var lineStart = textBeforeMatch.LastIndexOf('\n');
		var columns = (index - lineStart, index - lineStart + length - 1);

		return new(value, index, length, line, columns);
	}
}

public class NumberParser
{
	private enum State
	{
		S0,
		S1,
		S2,
		S3,
		S4
	}

	public List<SubstringMatch> Parse(string input)
	{
		var matches = new List<SubstringMatch>();

		for (int i = 0; i < input.Length; i++)
		{
			var match = TryParseNumber(input, i);
			if (match is not null)
			{
				matches.Add(match);
				i += match.Length - 1;
			}
		}

		return matches;
	}

	private SubstringMatch? TryParseNumber(string input, int startIndex)
	{
		State currentState = State.S0;
		int currentPos = startIndex;
		int numberStart = 0;
		int lastValidEnd = 0;

		while (currentPos < input.Length)
		{
			char c = input[currentPos];

			switch (currentState)
			{
				case State.S0:
					if (c == '+' || c == '-')
					{
						currentState = State.S1;
						numberStart = currentPos;
						currentPos++;
					}
					else if (char.IsDigit(c))
					{
						currentState = State.S2;
						numberStart = currentPos;
						currentPos++;
						lastValidEnd = currentPos;
					}
					else
					{
						return null;
					}
					break;

				case State.S1:
					if (char.IsDigit(c))
					{
						currentState = State.S2;
						currentPos++;
						lastValidEnd = currentPos;
					}
					else
					{
						return null;
					}
					break;

				case State.S2:
					if (char.IsDigit(c))
					{
						currentState = State.S2;
						currentPos++;
						lastValidEnd = currentPos;
					}
					else if (c == '.' || c == ',')
					{
						currentState = State.S3;
						currentPos++;
					}
					else
					{
						var index = numberStart;
						var length = lastValidEnd - numberStart;
						var value = input[index..(index + length)];
						return SubstringHelper.FromInput(input, value, index, length);
					}
					break;

				case State.S3:
					if (char.IsDigit(c))
					{
						currentState = State.S4;
						currentPos++;
						lastValidEnd = currentPos;
					}
					else
					{
						var index = numberStart;
						var length = lastValidEnd - numberStart;
						var value = input[index..(index + length)];
						return SubstringHelper.FromInput(input, value, index, length);
					}
					break;

				case State.S4:
					if (char.IsDigit(c))
					{
						currentState = State.S4;
						currentPos++;
						lastValidEnd = currentPos;
					}
					else
					{
						var index = numberStart;
						var length = lastValidEnd - numberStart;
						var value = input[index..(index + length)];
						return SubstringHelper.FromInput(input, value, index, length);
					}
					break;
			}
		}

		if (currentState == State.S2 || currentState == State.S4)
		{
			var index = numberStart;
			var length = lastValidEnd - numberStart;
			var value = input[index..(index + length)];
			return SubstringHelper.FromInput(input, value, index, length);
		}

		return null;
	}
}
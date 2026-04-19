using GUI.Models;

namespace GUI.Services;

public class SubstringSearchService : ISubstringSearchService
{
	public List<SubstringMatch> FindSubstrings(string input, SubstringTemplate template)
	{
		var matches = template.Template.Matches(input);
		var result = matches
			.Select(m => SubstringMatch.FromInput(input, m.Index, m.Length))
			.ToList();

		return result;
	}

	public List<SubstringMatch> FindNumberSubstrings(string input)
	{
		var result = NumberParser.Parse(input);
		return result;
	}
}

public static class NumberParser
{
	private enum State
	{
		S0,
		S1,
		S2,
		S3,
		S4
	}

	public static List<SubstringMatch> Parse(string input)
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

	private static SubstringMatch? TryParseNumber(string input, int startIndex)
	{
		var currentState = State.S0;
		var currentPos = startIndex;
		var numberStart = 0;
		var lastValidEnd = 0;

		while (currentPos < input.Length)
		{
			var c = input[currentPos];

			switch (currentState)
			{
				case State.S0:
					if (c is '+' or '-')
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
						return null;
					break;
				case State.S1:
					if (char.IsDigit(c))
					{
						currentState = State.S2;
						currentPos++;
						lastValidEnd = currentPos;
					}
					else
						return null;
					break;
				case State.S2:
					if (char.IsDigit(c))
					{
						currentState = State.S2;
						currentPos++;
						lastValidEnd = currentPos;
					}
					else if (c is '.' or ',')
					{
						currentState = State.S3;
						currentPos++;
					}
					else
					{
						var index = numberStart;
						var length = lastValidEnd - numberStart;
						return SubstringMatch.FromInput(input, index, length);
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
						return SubstringMatch.FromInput(input, index, length);
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
						return SubstringMatch.FromInput(input, index, length);
					}
					break;
			}
		}

		if (currentState is State.S2 or State.S4)
		{
			var index = numberStart;
			var length = lastValidEnd - numberStart;
			return SubstringMatch.FromInput(input, index, length);
		}

		return null;
	}
}
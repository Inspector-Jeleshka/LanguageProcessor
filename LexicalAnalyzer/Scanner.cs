using LexicalAnalyzer.Tokens;

namespace LexicalAnalyzer;

public class Scanner
{
	private int _state = 0;

	public int Line { get; private set; } = 1;
	public int Column { get; private set; } = 1;
	public List<ScannerException> Errors = new();

	public IEnumerable<IToken> Scan(string input)
	{
		var tokens = new List<IToken>();
		var word = string.Empty;
		var pos = 0;

		while (pos <= input.Length)
		{
			var symbol = input[pos];

			switch (_state)
			{
				case 0 when char.IsLetter(symbol):
					word = string.Empty;
					word += symbol;
					_state = 1;
					break;
				case 0 when char.IsDigit(symbol):
					word = string.Empty;
					word += symbol;
					_state = 2;
					break;
				case 0 when char.IsWhiteSpace(symbol):
					if (symbol == '\r')
						break;
					if (symbol == '\n')
					{
						Column = 1;
						Line++;
					}
					_state = 5;
					break;
				case 0 when symbol == ':':
					_state = 6;
					break;
				case 0 when symbol == '=':
					_state = 7;
					break;
				case 0 when symbol == ';':
					_state = 8;
					break;
				case 0 when symbol == '+':
					_state = 9;
					break;
				case 0 when symbol == '-':
					_state = 10;
					break;
				case 1 when char.IsLetterOrDigit(symbol):
					word += symbol;
					break;
				case 1 when word == "const":
					tokens.Add(new ConstKeyword(Line, (Column - word.Length, Column)));
					_state = 0;
					continue;
				case 1 when word == "f32":
					tokens.Add(new F32Keyword(Line, (Column - word.Length, Column)));
					_state = 0;
					continue;
				case 1:
					tokens.Add(new Identifier(Line, (Column - word.Length, Column), word));
					_state = 0;
					continue;
				case 2 when char.IsDigit(symbol):
					word += symbol;
					break;
				case 2 when symbol == '.':
					word += symbol;
					_state = 3;
					break;
				case 2:
					if (int.TryParse(word, out var intVal))
						tokens.Add(new IntLiteral(Line, (Column - word.Length, Column), intVal));
					else
						Errors.Add(new ScannerException(Line, (Column - word.Length, Column), $"Не удалось отсканировать число {word}"));
					_state = 0;
					break;
				case 3:
					if (char.IsDigit(symbol))
					{
						word += symbol;
						_state = 4;
					}
					else
					{
						Errors.Add(new ScannerException(Line, (Column - word.Length, Column), $"Не удалось отсканировать число {word}"));
						_state = 0;
					}
					break;
				case 4 when char.IsDigit(symbol):
					word += symbol;
					break;
				case 4:
					if (float.TryParse(word, out var floatVal))
						tokens.Add(new FloatLiteral(Line, (Column - word.Length, Column), floatVal));
					else
						Errors.Add(new ScannerException(Line, (Column, Column), $"Не удалось отсканировать число {word}"));
					_state = 0;
					continue;
				case 5:
					tokens.Add(new Space(Line, (Column, Column)));
					_state = 0;
					continue;
				case 6:
					tokens.Add(new Colon(Line, (Column, Column)));
					_state = 0;
					continue;
				case 7:
					tokens.Add(new AssignmentOperator(Line, (Column, Column)));
					_state = 0;
					continue;
				case 8:
					tokens.Add(new Semicolon(Line, (Column, Column)));
					_state = 0;
					continue;
				case 9:
					tokens.Add(new Plus(Line, (Column, Column)));
					_state = 0;
					continue;
				case 10:
					tokens.Add(new Minus(Line, (Column, Column)));
					_state = 0;
					continue;
			}

			pos++;
			Column++;
		}

		for (int i = 0; i < tokens.Count;)
		{
			if (i == 0 && tokens[i] is Space)
			{
				tokens.RemoveAt(i);
				continue;
			}
			if (tokens[i] is Space && tokens[i - 1] is not ConstKeyword)
			{
				tokens.RemoveAt(i);
				continue;
			}
			i++;
		}
		return tokens;
	}
}

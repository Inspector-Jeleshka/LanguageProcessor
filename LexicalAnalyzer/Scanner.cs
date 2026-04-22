using LexicalAnalyzer.Tokens;
using System.Globalization;

namespace LexicalAnalyzer;

public class Scanner
{
	private int _state;

	public int Line { get; private set; }
	public int Column { get; private set; }
	public List<ScannerException> Errors = new();

	public IEnumerable<IToken> Scan(string input)
	{
		_state = 0;
		Line = 1;
		Column = 1;
		Errors = new();
		var tokens = new List<IToken>();
		var word = string.Empty;
		var pos = 0;

		while (pos < input.Length)
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
					_state = 5;
					if (symbol == '\n')
					{
						Column = 1;
						Line++;
						pos++;
						continue;
					}
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
					tokens.Add(new ConstKeyword(Line, (Column - word.Length, Column - 1)));
					_state = 0;
					continue;
				case 1 when word == "f32":
					tokens.Add(new F32Keyword(Line, (Column - word.Length, Column - 1)));
					_state = 0;
					continue;
				case 1:
					tokens.Add(new Identifier(Line, (Column - word.Length, Column - 1), word));
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
						tokens.Add(new IntLiteral(Line, (Column - word.Length, Column - 1), intVal));
					else
						Errors.Add(new ScannerException(Line, (Column - word.Length, Column - 1), $"Не удалось отсканировать число {word}"));
					_state = 0;
					continue;
				case 3:
					if (char.IsDigit(symbol))
					{
						word += symbol;
						_state = 4;
					}
					else
					{
						Errors.Add(new ScannerException(Line, (Column - word.Length, Column - 1), $"Не удалось отсканировать число {word}"));
						_state = 0;
						continue;
					}
					break;
				case 4 when char.IsDigit(symbol):
					word += symbol;
					break;
				case 4:
					if (float.TryParse(word, CultureInfo.InvariantCulture, out var floatVal))
						tokens.Add(new FloatLiteral(Line, (Column - word.Length, Column - 1), floatVal));
					else
						Errors.Add(new ScannerException(Line, (Column - 1, Column - 1), $"Не удалось отсканировать число {word}"));
					_state = 0;
					continue;
				case 5:
					tokens.Add(new Space(Line, (Column - 1, Column - 1)));
					_state = 0;
					continue;
				case 6:
					tokens.Add(new Colon(Line, (Column - 1, Column - 1)));
					_state = 0;
					continue;
				case 7:
					tokens.Add(new AssignmentOperator(Line, (Column - 1, Column - 1)));
					_state = 0;
					continue;
				case 8:
					tokens.Add(new Semicolon(Line, (Column - 1, Column - 1)));
					_state = 0;
					continue;
				case 9:
					tokens.Add(new Plus(Line, (Column - 1, Column - 1)));
					_state = 0;
					continue;
				case 10:
					tokens.Add(new Minus(Line, (Column - 1, Column - 1)));
					_state = 0;
					continue;
				default:
					Errors.Add(new ScannerException(Line, (Column, Column), $"Неизвестный символ {symbol}"));
					_state = 0;
					break;
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
		for (int i = 0; i < Errors.Count - 1;)
		{
			if (Errors[i].Line == Errors[i + 1].Line && Errors[i].Columns.Item2 + 1 == Errors[i + 1].Columns.Item1)
			{
				var line = Errors[i].Line;
				var message = "Неизвестная последовательность";
				var newError = new ScannerException(line, (Errors[i].Columns.Item1, Errors[i + 1].Columns.Item2), message);
				Errors.RemoveAt(i + 1);
				Errors[i] = newError;
			}
			else
				i++;
		}

		var lastToken = tokens.Last();
		tokens.Add(new EndOfFile(lastToken.Line, (lastToken.Columns.Item2 + 1, lastToken.Columns.Item2 + 1)));

		return tokens;
	}
}

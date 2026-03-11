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
			if (symbol == '\r' && input[pos + 1] == '\n'
				|| symbol == '\n')
			{
				pos++;
				Column = 1;
				Line++;
				continue;
			}

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
					tokens.Add(new ConstKeyword());
					_state = 0;
					break;
				case 1 when word == "f32":
					tokens.Add(new F32Keyword());
					_state = 0;
					break;
				case 1:
					tokens.Add(new Identifier(word));
					_state = 0;
					break;
				case 2 when char.IsDigit(symbol):

					break;
				case 3:

					break;
				case 4 when char.IsDigit(symbol):
					word += symbol;
					break;
				case 4:
					_state = 0;
					if (float.TryParse(word, out var number))
					{
						tokens.Add(new FloatLiteral(number));
					}
					else
					{

					}
					continue;
				case 5:
					tokens.Add(new Space());
					_state = 0;
					continue;
				case 6:
					tokens.Add(new Colon());
					_state = 0;
					continue;
				case 7:
					tokens.Add(new AssignmentOperator());
					_state = 0;
					continue;
				case 8:
					tokens.Add(new Semicolon());
					_state = 0;
					continue;
				case 9:
					tokens.Add(new Plus());
					_state = 0;
					continue;
				case 10:
					tokens.Add(new Minus());
					_state = 0;
					continue;
			}

			pos++;
			Column++;
		}
		return tokens;
	}
}

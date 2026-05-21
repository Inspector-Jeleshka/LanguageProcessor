namespace SyntaxAnalyzer.Semantics;

public sealed record SymbolInfo(
	string Name,
	string TypeName,
	object? Value,
	int Line,
	(int Start, int End) Columns);

public sealed class SymbolTable
{
	private readonly Dictionary<string, SymbolInfo> _symbols = new(StringComparer.Ordinal);

	public void Clear() => _symbols.Clear();

	public bool Declare(SymbolInfo symbol, out string error)
	{
		if (_symbols.ContainsKey(symbol.Name))
		{
			error = $"повторное объявление идентификатора '{symbol.Name}'";
			return false;
		}

		_symbols.Add(symbol.Name, symbol);
		error = string.Empty;
		return true;
	}

	public bool TryLookup(string name, out SymbolInfo? symbol)
		=> _symbols.TryGetValue(name, out symbol);
}
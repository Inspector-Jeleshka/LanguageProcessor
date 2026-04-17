using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace GUI.Models;

public sealed partial record SubstringTemplate
{
	private readonly string _name;

	public Regex Template { get; }

	public static SubstringTemplate Empty => new(EmptyRegex, "Отсутствует");
	public static SubstringTemplate Year2000 => new(Year2000Regex, "2000-ый год");
	public static SubstringTemplate Number => new(NumberRegex, "Число целое/с плавающей точкой");
	public static SubstringTemplate WindowsPath => new(WindowsPathRegex, "Путь к файлу Windows");

	public static ImmutableList<SubstringTemplate> AllTemplates { get; } = [Empty, Year2000, Number, WindowsPath];

	[GeneratedRegex(@"$.^", RegexOptions.None)]
	private static partial Regex EmptyRegex { get; }
	[GeneratedRegex(@"20(0\d|10)", RegexOptions.None)]
	private static partial Regex Year2000Regex { get; }
	[GeneratedRegex(@"[+-]?\d+([\.,]\d+)?", RegexOptions.None)]
	private static partial Regex NumberRegex { get; }
	[GeneratedRegex(@"([A-Z]:|\.|\.\.)?(\\?(\w[\w ]+\w|\w+))+\.\w+", RegexOptions.None)]
	private static partial Regex WindowsPathRegex { get; }

	private SubstringTemplate(Regex template, string name) => (Template, _name) = (template, name);

	public override string ToString() => _name;
}


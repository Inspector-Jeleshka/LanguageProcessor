using System.Text;

namespace SyntaxAnalyzer.Ast;

public abstract class AstNode
{
	protected AstNode(string kind, int line, (int Start, int End) columns)
	{
		Kind = kind;
		Line = line;
		Columns = columns;
	}

	public string Kind { get; }
	public int Line { get; }
	public (int Start, int End) Columns { get; }
	public List<AstNode> Children { get; } = new();

	public void Add(AstNode child) => Children.Add(child);
}

public sealed class CompilationUnitNode : AstNode
{
	public CompilationUnitNode(int line, (int Start, int End) columns) : base("CompilationUnit", line, columns) { }
}

public sealed class ConstDeclNode : AstNode
{
	public ConstDeclNode(string name, string typeName, LiteralNode value, int line, (int Start, int End) columns)
		: base("ConstDecl", line, columns)
	{
		Name = name;
		TypeName = typeName;
		Value = value;

		Add(new IdentifierNode(name, line, columns));
		Add(new TypeNode(typeName, line, columns));
		Add(value);
	}

	public string Name { get; }
	public string TypeName { get; }
	public LiteralNode Value { get; }
}

public sealed class IdentifierNode : AstNode
{
	public IdentifierNode(string name, int line, (int Start, int End) columns)
		: base("Identifier", line, columns)
	{
		Name = name;
	}

	public string Name { get; }
}

public sealed class TypeNode : AstNode
{
	public TypeNode(string name, int line, (int Start, int End) columns)
		: base("Type", line, columns)
	{
		Name = name;
	}

	public string Name { get; }
}

public sealed class LiteralNode : AstNode
{
	public LiteralNode(string literalKind, string text, object? parsedValue, int line, (int Start, int End) columns)
		: base("Literal", line, columns)
	{
		LiteralKind = literalKind;
		Text = text;
		ParsedValue = parsedValue;
	}

	public string LiteralKind { get; }
	public string Text { get; }
	public object? ParsedValue { get; }
}

public static class AstPrinter
{
	public static string Print(AstNode root)
	{
		var sb = new StringBuilder();
		Write(root, "", isLast: true, sb);
		return sb.ToString();
	}

	private static void Write(AstNode node, string indent, bool isLast, StringBuilder sb)
	{
		sb.Append(indent);
		sb.Append(isLast ? "└── " : "├── ");
		sb.Append(node.Kind);

		switch (node)
		{
			case ConstDeclNode c:
				sb.Append($" [name={c.Name}, type={c.TypeName}]");
				break;
			case IdentifierNode id:
				sb.Append($" [name={id.Name}]");
				break;
			case TypeNode t:
				sb.Append($" [name={t.Name}]");
				break;
			case LiteralNode l:
				sb.Append($" [kind={l.LiteralKind}, text={l.Text}]");
				break;
		}

		sb.AppendLine();

		var childIndent = indent + (isLast ? "    " : "│   ");
		for (int i = 0; i < node.Children.Count; i++)
			Write(node.Children[i], childIndent, i == node.Children.Count - 1, sb);
	}
}
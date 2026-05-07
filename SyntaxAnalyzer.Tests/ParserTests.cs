using LexicalAnalyzer;
using LexicalAnalyzer.Tokens;

namespace SyntaxAnalyzer.Tests;

public class ParserTests
{
	// Форма описания тестов: Method_Scenario_ExpectedBehavior

	[Fact]
	public void TryParse_CorrectInput_ReturnsTrueAndEmptyErrors()
	{
		var line = 1;
		var columns = new InclusiveRange();
		List<IToken> tokens = [new ConstKeyword(line, columns++), new Space(line, columns++),
			new Identifier(line, columns++, "a"), new Colon(line, columns++), new F32Keyword(line, columns++),
			new AssignmentOperator(line, columns++), new FloatLiteral(line, columns++, 12.3f),
			new Semicolon(line, columns++), new EndOfFile(line, columns)];
		var parser = new Parser();

		var parsed = parser.TryParse(tokens, out var errors);

		Assert.True(parsed, string.Join(";\n", errors));
		Assert.Empty(errors);
	}

	[Fact]
	public void TryParse_CorrectInput2_ReturnsTrueAndEmptyErrors()
	{
		var line = 1;
		var columns = new InclusiveRange();
		List<IToken> tokens = [new ConstKeyword(line, columns++), new Space(line, columns++),
			new Identifier(line, columns++, "a"), new Colon(line, columns++), new F32Keyword(line, columns++),
			new AssignmentOperator(line, columns++), new FloatLiteral(line, columns++, 12.3f),
			new Semicolon(line, columns++), new ConstKeyword(line, columns++), new Space(line, columns++),
			new Identifier(line, columns++, "b"), new Colon(line, columns++), new F32Keyword(line, columns++),
			new AssignmentOperator(line, columns++), new FloatLiteral(line, columns++, 23.4f),
			new Semicolon(line, columns++), new EndOfFile(line, columns)];
		var parser = new Parser();

		var parsed = parser.TryParse(tokens, out var errors);

		Assert.True(parsed, string.Join(";\n", errors));
		Assert.Empty(errors);
	}

	[Fact]
	public void TryParse_AdditionalSpaces_ReturnsTrueAndEmptyErrors()
	{
		var line = 1;
		var columns = new InclusiveRange();
		List<IToken> tokens = [new ConstKeyword(line, columns++), new Space(line, columns++),
			new Identifier(line, columns++, "a"), new Colon(line, columns++),
			new Space(line, columns++), new F32Keyword(line, columns++), new Space(line, columns++),
			new AssignmentOperator(line, columns++), new Space(line, columns++),
			new FloatLiteral(line, columns++, 12.3f), new Space(line, columns++),
			new Semicolon(line, columns++), new EndOfFile(line, columns)];
		var parser = new Parser();

		var parsed = parser.TryParse(tokens, out var errors);

		Assert.True(parsed, string.Join(";\n", errors));
		Assert.Empty(errors);
	}

	[Fact]
	public void TryParse_AdditionalColon_ReturnsFalseAndSingleError()
	{
		var line = 1;
		var columns = new InclusiveRange();
		List<IToken> tokens = [new ConstKeyword(line, columns++), new Space(line, columns++),
			new Identifier(line, columns++, "a"), new Colon(line, columns++), new Colon(line, columns++),
			new F32Keyword(line, columns++), new AssignmentOperator(line, columns++),
			new FloatLiteral(line, columns++, 12.3f), new Semicolon(line, columns++), new EndOfFile(line, columns)];
		var parser = new Parser();

		var parsed = parser.TryParse(tokens, out var errors);

		Assert.False(parsed);
		Assert.Single(errors);
	}

	[Fact]
	public void TryParse_OnlySemicolon_ReturnsFalseAndSingleError()
	{
		var line = 1;
		var columns = new InclusiveRange();
		List<IToken> tokens = [new Semicolon(line, columns++), new EndOfFile(line, columns)];
		var parser = new Parser();

		var parsed = parser.TryParse(tokens, out var errors);

		Assert.False(parsed);
		Assert.Single(errors);
	}

	[Fact]
	public void TryParse_OnlyAssignment_ReturnsFalseAndSingleError()
	{
		var line = 1;
		var columns = new InclusiveRange();
		List<IToken> tokens = [new AssignmentOperator(line, columns++), new EndOfFile(line, columns)];
		var parser = new Parser();

		var parsed = parser.TryParse(tokens, out var errors);

		Assert.False(parsed);
		Assert.Single(errors);
	}

	[Fact]
	public void TryParse_ConstTypo_ReturnsFalseAndSingleError()
	{
		var line = 1;
		var columns = new InclusiveRange();
		List<IToken> tokens = [new Identifier(line, columns++, "cont"), new Space(line, columns++),
			new Identifier(line, columns++, "a"), new Colon(line, columns++), new F32Keyword(line, columns++),
			new AssignmentOperator(line, columns++), new FloatLiteral(line, columns++, 12.3f),
			new Semicolon(line, columns++), new EndOfFile(line, columns)];
		var parser = new Parser();

		var parsed = parser.TryParse(tokens, out var errors);

		Assert.False(parsed);
		Assert.Single(errors);
	}

	[Fact]
	public void TryParse_ErrorToken_ReturnsFalseAndSingleError()
	{
		var line = 1;
		var columns = new InclusiveRange();
		List<IToken> tokens = [new Identifier(line, columns++, "cont"), new ErrorToken(line, columns++, "@"), new Space(line, columns++),
			new Identifier(line, columns++, "a"), new Colon(line, columns++), new F32Keyword(line, columns++),
			new AssignmentOperator(line, columns++), new FloatLiteral(line, columns++, 12.3f),
			new Semicolon(line, columns++), new EndOfFile(line, columns)];
		var parser = new Parser();

		var parsed = parser.TryParse(tokens, out var errors);

		Assert.False(parsed);
		Assert.Single(errors);
	}

	[Fact]
	public void TryParse_ErrorTokens_ReturnsFalseAndErrors()
	{
		var line = 1;
		var columns = new InclusiveRange();
		List<IToken> tokens = [new Identifier(line, columns++, "c"), new ErrorToken(line, columns++, "@"),
			new Identifier(line, columns++, "o"), new ErrorToken(line, columns++, "@"),
			new Identifier(line, columns++, "ns"), new ErrorToken(line, columns++, "@"),
			new Identifier(line, columns++, "t"),
			new Space(line, columns++),
			new Identifier(line, columns++, "a"), new Colon(line, columns++), new Identifier(line, columns++, "f3"),
			new ErrorToken(line, columns++, "@@@@"), new IntLiteral(line, columns++, 2),
			new AssignmentOperator(line, columns++), new ErrorToken(line, columns++, "12."),
			new EndOfFile(line, columns)];
		var parser = new Parser();

		var parsed = parser.TryParse(tokens, out var errors);

		Assert.False(parsed);
		Assert.Contains("const", errors[0].Description);
		Assert.Contains("f32", errors[1].Description);
		Assert.Contains("число", errors[2].Description);
		Assert.Contains("конец", errors[3].Description);
	}

	[Fact]
	public void TryParse_ConstTypeAndKeywordTypo_ReturnsFalseAndErrors()
	{
		var line = 1;
		var columns = new InclusiveRange();
		List<IToken> tokens = [new Identifier(line, columns++, "cont"), new Space(line, columns++),
			new Identifier(line, columns++, "a"), new Colon(line, columns++), new Identifier(line, columns++, "f3"),
			new IntLiteral(line, columns++, 2),
			new AssignmentOperator(line, columns++), new FloatLiteral(line, columns++, 12.3f),
			new Semicolon(line, columns++), new EndOfFile(line, columns)];
		var parser = new Parser();

		var parsed = parser.TryParse(tokens, out var errors);

		Assert.False(parsed);
		Assert.Contains("const", errors[0].Description);
	}

	[Fact]
	public void TryParse_MissingSemicolonBetweenStatements_ReturnsFalseAndSingleError()
	{
		var line = 1;
		var columns = new InclusiveRange();
		List<IToken> tokens = [new ConstKeyword(line, columns++), new Space(line, columns++),
			new Identifier(line, columns++, "a"), new Colon(line, columns++), new F32Keyword(line, columns++),
			new AssignmentOperator(line, columns++), new FloatLiteral(line, columns++, 12.3f),
			new ConstKeyword(line, columns++), new Space(line, columns++),
			new Identifier(line, columns++, "b"), new Colon(line, columns++), new F32Keyword(line, columns++),
			new AssignmentOperator(line, columns++), new FloatLiteral(line, columns++, 23.4f),
			new Semicolon(line, columns++), new EndOfFile(line, columns)];
		var parser = new Parser();

		var parsed = parser.TryParse(tokens, out var errors);

		Assert.False(parsed);
		Assert.Single(errors);
	}

	[Fact]
	public void TryParse_MissingFirstAssignmentAndFirstSemicolon_ReturnsFalseAndTwoErrors()
	{
		var line = 1;
		var columns = new InclusiveRange();
		List<IToken> tokens = [new ConstKeyword(line, columns++), new Space(line, columns++),
			new Identifier(line, columns++, "a"), new Colon(line, columns++), new F32Keyword(line, columns++),
			new FloatLiteral(line, columns++, 12.3f), new ConstKeyword(line, columns++), new Space(line, columns++),
			new Identifier(line, columns++, "b"), new Colon(line, columns++), new F32Keyword(line, columns++),
			new AssignmentOperator(line, columns++), new FloatLiteral(line, columns++, 23.4f),
			new Semicolon(line, columns++), new EndOfFile(line, columns)];
		var parser = new Parser();

		var parsed = parser.TryParse(tokens, out var errors);

		Assert.False(parsed);
		Assert.Collection(errors, [missingAssignment => Assert.Contains("=", missingAssignment.Description),
			missingSemicolon => Assert.Contains(";", missingSemicolon.Description)]);
	}

	private record struct InclusiveRange(int Start, int End)
	{
		public InclusiveRange() : this(1, 1) { }

		public static InclusiveRange operator ++(InclusiveRange x)
			=> x with { Start = x.Start + 1, End = x.End + 1 };

		public static implicit operator (int, int)(InclusiveRange x)
			=> (x.Start, x.End);
	}
}

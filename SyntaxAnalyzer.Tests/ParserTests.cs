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
	public void TryParse_MissingSpace_ReturnsFalseAndSingleError()
	{
		var line = 1;
		var columns = new InclusiveRange();
		List<IToken> tokens = [new ConstKeyword(line, columns++),
			new Identifier(line, columns++, "a"), new Colon(line, columns++), new F32Keyword(line, columns++),
			new AssignmentOperator(line, columns++), new FloatLiteral(line, columns++, 12.3f),
			new Semicolon(line, columns++), new EndOfFile(line, columns)];
		var parser = new Parser();

		var parsed = parser.TryParse(tokens, out var errors);

		Assert.False(parsed);
		Assert.Single(errors);
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
		//Assert.Collection(errors, [first => Assert.Contains("const", first.Description),
		//	second => Assert.Contains("f32", second.Description)]);
	}

	// WIP. Пересмотреть ожидаемый результат
	//[Fact]
	//public void TryParse_TwoIdentifiersAtTheStart_ReturnsFalseAndSingleError()
	//{
	//	var line = 1;
	//	var columns = new InclusiveRange();
	//	List<IToken> tokens = [new Identifier(line, columns++, "con"), new Identifier(line, columns++, "st"),
	//		new Space(line, columns++), new Identifier(line, columns++, "a"), new Colon(line, columns++),
	//		new F32Keyword(line, columns++), new AssignmentOperator(line, columns++),
	//		new FloatLiteral(line, columns++, 12.3f), new Semicolon(line, columns++), new EndOfFile(line, columns)];
	//	var parser = new Parser();

	//	var parsed = parser.TryParse(tokens, out var errors);

	//	Assert.False(parsed);
	//	Assert.Single(errors);
	//}

	// WIP. Пересмотреть ожидаемый результат
	//[Fact]
	//public void TryParse_TwoAdditionalIdentifier_ReturnsFalseAndTwoErrors()
	//{
	//	var line = 1;
	//	var columns = new InclusiveRange();
	//	List<IToken> tokens = [new ConstKeyword(line, columns++), new Space(line, columns++),
	//		new Identifier(line, columns++, "a"), new Identifier(line, columns++, "b"),
	//		new Identifier(line, columns++, "c"), new Colon(line, columns++),
	//		new F32Keyword(line, columns++), new AssignmentOperator(line, columns++),
	//		new FloatLiteral(line, columns++, 12.3f), new Semicolon(line, columns++), new EndOfFile(line, columns)];
	//	var parser = new Parser();

	//	var parsed = parser.TryParse(tokens, out var errors);

	//	Assert.False(parsed);
	//	Assert.Collection(errors, [first => Assert.Contains(":", first.Description),
	//		second => Assert.Contains("f32", second.Description)]);
	//}

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

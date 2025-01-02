using Lazyripent2.Lexer;

namespace Lazyripent2.Tests;

[TestFixture]
public class FileParserTests
{
	#pragma warning disable 8618
	FileParser _parser;
	#pragma warning restore 8618

	[OneTimeSetUp]
	public void SetUp()
	{
		_parser = new("(){}[],.:;-+/*@" +
		"! =><" +
		"!===>=<=" +
		"identifier\"string\"0" + 
		"identifier iden tifier iden-tifier\"string\"\"s t r i n g\"\"str\"\"ing\"0 1 -1 0.0 1.0 -1.0 0.0 1.0 -1.0");
	}

	[Test, Order(1)]
	[TestCase(TokenType.LeftParenthesis), Repeat(3)]
	public void ExpectTokenNoAdvance(TokenType token)
	{
		Assert.DoesNotThrow(() => {
			_parser.ExpectToken(token, false);
		});
	}

	[Test, Order(2)]
	public void ExpectTokenAdvanceSingleCharacter()
	{
		TokenType[] tokens = [
			TokenType.LeftParenthesis,
			TokenType.RightParenthesis,
			TokenType.LeftBrace,
			TokenType.RightBrace,
			TokenType.LeftBracket,
			TokenType.RightBracket,
			TokenType.Comma,
			TokenType.Dot,
			TokenType.Colon,
			TokenType.Semicolon,
			TokenType.Minus,
			TokenType.Plus,
			TokenType.Slash,
			TokenType.Star,
			TokenType.AtSign,
		];

		Assert.DoesNotThrow(() => {
			for(int i = 0; i < tokens.Length; i++)
			{
				_parser.ExpectToken(tokens[i]);
			}
		});
	}

	[Test, Order(3)]
	public void ExpectTokenAdvanceSingleOrTwoCharacter()
	{
		TokenType[] tokens = [
			TokenType.Bang,
			TokenType.Equal,
			TokenType.Greater,
			TokenType.Less,
		];

		Assert.DoesNotThrow(() => {
			for(int i = 0; i < tokens.Length; i++)
			{
				_parser.ExpectToken(tokens[i]);
			}
		});
	}

	[Test, Order(4)]
	public void ExpectTokenAdvanceTwoCharacter()
	{
		TokenType[] tokens = [
			TokenType.BangEqual,
			TokenType.EqualEqual,
			TokenType.GreaterEqual,
			TokenType.LessEqual,
		];

		Assert.DoesNotThrow(() => {
			for(int i = 0; i < tokens.Length; i++)
			{
				_parser.ExpectToken(tokens[i]);
			}
		});
	}

	[Test, Order(5)]
	public void ExpectTokenAdvanceLiteral()
	{
		TokenType[] tokens = [
			TokenType.Identifier,
			TokenType.String,
			TokenType.Number,
		];

		Assert.DoesNotThrow(() => {
			for(int i = 0; i < tokens.Length; i++)
			{
				_parser.ExpectToken(tokens[i]);
			}
		});
	}

	[Test, Order(6)]
	[TestCase("identifier")]
	[TestCase("iden")]
	[TestCase("tifier")]
	[TestCase("iden-tifier")]
	public void ReadIdentifier(string expectedIdentifier)
	{
		Assert.DoesNotThrow(() => {
			Assert.That(expectedIdentifier, Is.EqualTo(_parser.ReadIdentifier()));
		});
	}

	[Test, Order(7)]
	[TestCase("string")]
	[TestCase("s t r i n g")]
	[TestCase("str")]
	[TestCase("ing")]
	public void ReadString(string expectedString)
	{
		Assert.DoesNotThrow(() => {
			Assert.That(expectedString, Is.EqualTo(_parser.ReadString()));
		});
	}

	[Test, Order(8)]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(-1)]
	public void ReadInteger(int expectedInt)
	{
		Assert.DoesNotThrow(() => {
			Assert.That(expectedInt, Is.EqualTo(_parser.ReadInteger()));
		});
	}

	[Test, Order(9)]
	[TestCase(0.0)]
	[TestCase(1.0)]
	[TestCase(-1.0)]
	public void ReadDouble(double expectedDouble)
	{
		Assert.DoesNotThrow(() => {
			Assert.That(expectedDouble, Is.EqualTo(_parser.ReadDouble()));
		});
	}

	[Test, Order(10)]
	[TestCase(0.0f)]
	[TestCase(1.0f)]
	[TestCase(-1.0f)]
	public void ReadFloat(float expectedFloat)
	{
		Assert.DoesNotThrow(() => {
			Assert.That(expectedFloat, Is.EqualTo(_parser.ReadFloat()));
		});
	}

	[Test, Order(11)]
	public void IsAtEnd()
	{
		Assert.DoesNotThrow(() => {
			Assert.That(_parser.IsAtEnd(), Is.True);
		});
	}
}
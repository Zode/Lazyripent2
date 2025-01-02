using System.Text;
using Lazyripent2.Lexer;

namespace Lazyripent2;

/// <summary>
/// A warpper around the lexer scanner to help with consuming the file formats
/// </summary>
public class FileParser
{
	private readonly LexerScanner? _lexer = null;
	private Token? _token = null;
	public int TokenIndex {get; private set; } = 0;
	private string _source = string.Empty;

	public FileParser(string source)
	{
		TokenIndex = 0;
		_lexer = new(source);
		_source = source;
		AdvanceToken();
	}

	public FileParser(byte[] source)
	{
		string sourceString = Encoding.UTF8.GetString(source);

		TokenIndex = 0;
		_lexer = new(sourceString);
		_source = sourceString;
		AdvanceToken();
	}

	/// <summary>
	/// Advance the token by one.
	/// </summary>
	/// <exception cref="FileParserException"></exception>
	public void AdvanceToken()
	{
		if(_lexer is null)
		{
			throw new FileParserException("Lexer is null");
		}

		_token = _lexer.Tokens[TokenIndex++];
	}

	public void SetTokenIndex(int tokenIndex)
	{
		if(_lexer is null)
		{
			throw new FileParserException("Lexer is null");
		}

		if(_token is null)
		{
			throw new FileParserException("Token is null");
		}

		tokenIndex--; //This is "off by one" from reality since the index is advances after the token is read from lexer, so we hide this outside of this class by adjusting here.
		if(tokenIndex < 0 || tokenIndex >= _lexer.Tokens.Count)
		{
			throw new IndexOutOfRangeException("New token index is out of bounds");
		}

		TokenIndex = tokenIndex;
		AdvanceToken();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="token"></param>
	/// <param name="advance">if true, advance current token also</param>
	/// <exception cref="FileParserException"></exception>
	public void ExpectToken(TokenType token, bool advance = true)
	{
		if(_lexer is null)
		{
			throw new FileParserException("Lexer is null");
		}

		if(_token is null)
		{
			throw new FileParserException("Token is null");
		}

		if(_token.Type != token)
		{
			throw new FileParserException($"Unexpected \"{_token.Type}\", expected \"{token}\" on line {_token.Line}", _token.EndIndex, _lexer, TokenIndex);
		}

		if(advance)
		{
			AdvanceToken();
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	/// <exception cref="FileParserException"></exception>
	public int ReadInteger()
	{
		if(_lexer is null)
		{
			throw new FileParserException("Lexer is null");
		}

		if(_token is null)
		{
			throw new FileParserException("Token is null");
		}

		bool isNegative = false;
		if(_token.Type == TokenType.Minus)
		{
			isNegative = true;
			AdvanceToken();
		}

		ExpectToken(TokenType.Number, false);
		if(_token.Literal is not int value)
		{
			throw new FileParserException($"Number token literal is not an integer on line {_token.Line}??", GetCaret(), _lexer, TokenIndex);
		}

		AdvanceToken();
		return isNegative ? value * -1 : value;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	/// <exception cref="FileParserException"></exception>
	public double ReadDouble()
	{
		if(_lexer is null)
		{
			throw new FileParserException("Lexer is null");
		}

		if(_token is null)
		{
			throw new FileParserException("Token is null");
		}

		bool isNegative = false;
		if(_token.Type == TokenType.Minus)
		{
			isNegative = true;
			AdvanceToken();
		}

		ExpectToken(TokenType.Number, false);
		double value;
		if(_token.Literal is int integerValue)
		{
			value = integerValue;
		}
		else
		{
			if(_token.Literal is double doubleValue)
			{
				value = doubleValue;
			}
			else
			{
				throw new FileParserException($"Number token literal is not a double or integer on line {_token.Line}??", GetCaret(), _lexer, TokenIndex);
			}
		}

		AdvanceToken();
		return isNegative ? value * -1.0f : value;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public float ReadFloat()
	{
		return (float)ReadDouble();
	}
	
	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	/// <exception cref="FileParserException"></exception>
	public string ReadString()
	{
		if(_lexer is null)
		{
			throw new FileParserException("Lexer is null");
		}

		if(_token is null)
		{
			throw new FileParserException("Token is null");
		}

		ExpectToken(TokenType.String, false);
		if(_token.Literal is not string value)
		{
			throw new FileParserException($"String token literal is not a string on line {_token.Line}??", GetCaret(), _lexer, TokenIndex);
		}

		AdvanceToken();
		return value;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	/// <exception cref="FileParserException"></exception>
	public string ReadIdentifier()
	{
		if(_lexer is null)
		{
			throw new FileParserException("Lexer is null");
		}

		if(_token is null)
		{
			throw new FileParserException("Token is null");
		}

		ExpectToken(TokenType.Identifier, false);
		string value = _token.GetLexeme(_source);
		AdvanceToken();
		return value;
	}

	/// <summary>
	/// Check if parser is at end of file
	/// </summary>
	/// <returns>true if token is EOF token</returns>
	/// <exception cref="FileParserException"></exception>
	public bool IsAtEnd()
	{
		if(_token is null)
		{
			throw new FileParserException("Token is null");
		}

		return _token.Type == TokenType.NullTerminator;
	}

	/// <summary>
	/// Return the current token
	/// </summary>
	/// <returns></returns>
	/// <exception cref="FileParserException"></exception>
	public Token GetToken()
	{
		if(_token is null)
		{
			throw new FileParserException("Token is null");
		}

		return _token;
	}

	/// <summary>
	/// Return the current token's type
	/// </summary>
	/// <returns></returns>
	/// <exception cref="FileParserException"></exception>
	public TokenType GetTokenType()
	{
		if(_token is null)
		{
			throw new FileParserException("Token is null");
		}

		return _token.Type;
	}

	/// <summary>
	/// Return the current token's lexeme
	/// </summary>
	/// <returns></returns>
	/// <exception cref="FileParserException"></exception>
	public string GetTokenLexeme()
	{
		if(_token is null)
		{
			throw new FileParserException("Token is null");
		}

		return _token.GetLexeme(_source);
	}

	/// <summary>
	/// Get the current caret position in source
	/// </summary>
	/// <returns></returns>
	/// <exception cref="FileParserException"></exception>
	public int GetCaret()
	{
		if(_token is null)
		{
			throw new FileParserException("Token is null");
		}

		return _token.EndIndex;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	/// <exception cref="FileParserException"></exception>
	public LexerScanner GetLexer()
	{
		if(_lexer is null)
		{
			throw new FileParserException("Lexer is null");
		}

		return _lexer;
	}
}
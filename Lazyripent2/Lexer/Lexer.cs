namespace Lazyripent2.Lexer;

/// <summary>
/// A generic lexer that generates tokens for a given string input
/// Has only one keyword, since it has to be flexible enough for all file formats used in this app.
/// See: Crafting Interpreters, by Rober Nystrom
/// </summary>
public class LexerScanner
{
    public string Source {get; private set;} = string.Empty;
	public List<Token> Tokens {get; private set;} = [];
    public int Line {get; private set;} = 0;
	private int _startCaretIndex = 0;
	private int _currentCaretIndex = 0;
	private bool _parseWhiteSpace = false;

    public LexerScanner(string source, bool parseWhiteSpace = false)
	{
		Source = source;
		_parseWhiteSpace = parseWhiteSpace;
		ScanTokens();
	}
	
	private void ScanTokens()
	{
		while(!IsAtEnd())
		{
			_startCaretIndex = _currentCaretIndex;
			ScanToken();
		}

		AddToken(TokenType.NullTerminator);
	}

	private bool IsAtEnd()
	{
		return _currentCaretIndex >= Source.Length;
	}

	private char Peek()
	{
		if(IsAtEnd())
		{
			return '\0';
		}
		
		return Source[_currentCaretIndex];
	}

	private char PeekNext()
	{
		if(_currentCaretIndex + 1 >= Source.Length)
		{
			return '\0';
		}

		return Source[_currentCaretIndex + 1];
	}

	private char Advance()
	{
		return Source[_currentCaretIndex++];
	}

	private bool Match(char expectedCharacter)
	{
		if(IsAtEnd() || Source[_currentCaretIndex] != expectedCharacter)
		{
			return false;
		}

		_currentCaretIndex++;
		return true;
	}

	private static bool IsDigit(char c)
	{
		return c >= '0' && c <= '9';
	}

	private static bool IsAlpha(char c)
	{
		return  (c >= 'a' && c <= 'z') ||
				(c >= 'A' && c <= 'Z') ||
				 c == '_';
	}

	private static bool IsAlphaNumeric(char c)
	{
		return IsAlpha(c) || IsDigit(c);
	}

	//this is only really used in identifier scanning,
	//so hackily add support for hyphens here since we aren't trying to be a programming/scripting language
	private bool IsAlphaNumericWithHyphen(char c)
	{
		return IsAlphaNumeric(c) || (c == '-' && IsAlphaNumeric(PeekNext()));
	}

	private void AddToken(TokenType type, object? literal = null)
	{
		Tokens.Add(new(type, _startCaretIndex, _currentCaretIndex, literal, Line));
	}

	/// <summary>
	/// </summary>
	/// <exception cref="LexerException"></exception>
	private void ScanToken()
	{
		char c = Advance();
		switch(c)
		{
			case '\0': AddToken(TokenType.NullTerminator); break;
			case '(': AddToken(TokenType.LeftParenthesis); break;
			case ')': AddToken(TokenType.RightParenthesis); break;
			case '{': AddToken(TokenType.LeftBrace); break;
			case '}': AddToken(TokenType.RightBrace); break;
			case '[': AddToken(TokenType.LeftBracket); break;
			case ']': AddToken(TokenType.RightBracket); break;
			case ',': AddToken(TokenType.Comma); break;
			case '.': AddToken(TokenType.Dot); break;
			case ':': AddToken(TokenType.Colon); break;
			case ';': AddToken(TokenType.Semicolon); break;
			case '-': AddToken(TokenType.Minus); break;
			case '+': AddToken(TokenType.Plus); break;
			case '*': AddToken(TokenType.Star); break;
			case '\\': AddToken(TokenType.BackwardsSlash); break;
			case '@': AddToken(TokenType.AtSign); break;
			case '~': AddToken(TokenType.Tilde); break;

			case '!': AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang); break;
			case '=': AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal); break;
			case '>': AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater); break;
			case '<': AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less); break;

			case '/':
				if(Match('/'))
				{
					//comment line, so ignore until new line
					while(Peek() != '\n' && !IsAtEnd())
					{
						Advance();
					}

					break;
				}

				AddToken(TokenType.Slash);

				break;

			case '#':
				//also a comment line
				while(Peek() != '\n' && !IsAtEnd())
				{
					Advance();
				}

				break;

			case ' ':
			case '\r':
			case '\t':
				if(_parseWhiteSpace)
				{
					AddToken(TokenType.Whitespace);
				}
				
				break;

			case '\n':
				Line++;
				break;

			case '"':
				ScanStringLiteral();
				break;

			default:
				if(IsDigit(c))
				{
					ScanNumberLiteral();
					break;
				}

				if(IsAlpha(c))
				{
					ScanIdentifierLiteral();
					break;
				}
				
				throw new LexerException($"Unexpected character: \"{c}\" (\\u{((int)c).ToString("X4")}) on line {Line}", _currentCaretIndex, Source);
		}
	}

	/// <summary>
	/// </summary>
	/// <exception cref="LexerException"></exception>
	private void ScanStringLiteral()
	{
		int startLine = Line;
		while(Peek() != '"' && !IsAtEnd())
		{
			if(Peek() == '\n')
			{
				Line++;
			}

			Advance(); 
		}

		if(IsAtEnd())
		{
			throw new LexerException($"Unterminated string: unexpected end of file on line {Line}, unterminated string starts on line {startLine}", _currentCaretIndex, Source);
		}

		//consume the closing double quote
		Advance();

		string value = Source[(_startCaretIndex + 1)..(_currentCaretIndex - 1)];
		AddToken(TokenType.String, value);
	}

	/// <summary>
	/// </summary>
	/// <exception cref="LexerException"></exception>
	private void ScanNumberLiteral()
	{
		while(IsDigit(Peek()))
		{
			Advance();
		}

		//check if fractional
		if(Peek() == '.' && IsDigit(PeekNext()))
		{
			//consume the .
			Advance();
			while(IsDigit(Peek()))
			{
				Advance();
			}

			if(!double.TryParse(Source[_startCaretIndex.._currentCaretIndex], out double doubleValue))
			{
				throw new LexerException($"Failed double conversion on line {Line}", _currentCaretIndex, Source);
			}

			AddToken(TokenType.Number, doubleValue);
			return;
		}

		if(!int.TryParse(Source[_startCaretIndex.._currentCaretIndex], out int integerValue))
		{
			throw new LexerException($"Failed integer conversion on line {Line}", _currentCaretIndex, Source);
		}

		AddToken(TokenType.Number, integerValue);
	}

	private void ScanIdentifierLiteral()
	{
		while(IsAlphaNumericWithHyphen(Peek()))
		{
			Advance();
		}

		AddToken(TokenType.Identifier);
	}
}
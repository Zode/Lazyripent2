using Lazyripent2.Lexer;

namespace Lazyripent2;

[Serializable]
public class FileParserException : Exception
{
	public LexerScanner? Lexer {get; private set;} = null;
	public int Caret {get; private set;} = 0;
	public int TokenIndex {get; private  set;} = 0;

	public FileParserException()
	{
	}

	public FileParserException(string message)
		: base(message)
	{
	}

	public FileParserException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public FileParserException(string message, int caret, LexerScanner lexer, int tokenIndex)
		: base(message)
	{
		Caret = caret;
		Lexer = lexer;
		TokenIndex = tokenIndex;
	}
}
namespace Lazyripent2.Lexer;

[Serializable]
public class LexerException : Exception
{
	public int Caret {get; private set;} = 0;
	public string LexerSource {get; private set;} = string.Empty;

	public LexerException()
	{
	}

	public LexerException(string message)
		: base(message)
	{
	}

	public LexerException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public LexerException(string message, int caret, string lexerSource)
		: base(message)
	{
		this.Caret = caret;
		LexerSource = lexerSource;
	}
}
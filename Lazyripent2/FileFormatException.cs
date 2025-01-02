using Lazyripent2.Lexer;

namespace Lazyripent2;

[Serializable]
public class FileFormatException : Exception
{
	public FileParser? FileParser {get; private set;} = null;

	public FileFormatException()
	{
	}

	public FileFormatException(string message)
		: base(message)
	{
	}

	public FileFormatException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public FileFormatException(string message, FileParser fileParser)
		: base(message)
	{
		FileParser = fileParser;
	}
}
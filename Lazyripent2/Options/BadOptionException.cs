namespace Lazyripent2;

[Serializable]
public class BadOptionException : Exception
{
	public BadOptionException()
	{
	}

	public BadOptionException(string message)
		: base(message)
	{
	}

	public BadOptionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
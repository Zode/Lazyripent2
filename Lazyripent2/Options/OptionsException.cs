namespace Lazyripent2;

[Serializable]
public class OptionsException : Exception
{
	public OptionsException()
	{
	}

	public OptionsException(string message)
		: base(message)
	{
	}

	public OptionsException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
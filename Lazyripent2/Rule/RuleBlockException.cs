namespace Lazyripent2.Rule;

[Serializable]
public class RuleBlockException : Exception
{
	public int Caret {get; private set;} = 0;
	public string RuleSource {get; private set;} = string.Empty;

	public RuleBlockException()
	{
	}

	public RuleBlockException(string message)
		: base(message)
	{
	}

	public RuleBlockException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public RuleBlockException(string message, int caret, string source)
		: base(message)
	{
		Caret = caret;
		RuleSource = source;
	}
}
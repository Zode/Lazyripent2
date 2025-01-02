namespace Lazyripent2.Rule;

[System.AttributeUsage(System.AttributeTargets.Field)]
public class RuleKeywordAttribute(string keyword, int consumeCount) : System.Attribute
{
    public string Keyword {get; private set;} = keyword;
	public int ConsumeCount {get; private set;} = consumeCount;
}

public static class RuleKeywordExtensions
{
	public static string GetKeyword(this RuleActionType value)
	{
		System.Reflection.FieldInfo? fieldInfo = value.GetType()?.GetField(value.ToString());
		if(fieldInfo is null)
		{
			return string.Empty;
		}

		RuleKeywordAttribute[] attributes = (RuleKeywordAttribute[])fieldInfo.GetCustomAttributes(typeof(RuleKeywordAttribute), false);
		return attributes.Length > 0 ? attributes[0].Keyword : string.Empty;
	}

	public static int GetConsumeCount(this RuleActionType value)
	{
		System.Reflection.FieldInfo? fieldInfo = value.GetType()?.GetField(value.ToString());
		if(fieldInfo is null)
		{
			return 0;
		}

		RuleKeywordAttribute[] attributes = (RuleKeywordAttribute[])fieldInfo.GetCustomAttributes(typeof(RuleKeywordAttribute), false);
		return attributes.Length > 0 ? attributes[0].ConsumeCount : 0;
	}

	public static string GetKeyword(this RuleSelectorType value)
	{
		System.Reflection.FieldInfo? fieldInfo = value.GetType()?.GetField(value.ToString());
		if(fieldInfo is null)
		{
			return string.Empty;
		}

		RuleKeywordAttribute[] attributes = (RuleKeywordAttribute[])fieldInfo.GetCustomAttributes(typeof(RuleKeywordAttribute), false);
		return attributes.Length > 0 ? attributes[0].Keyword : string.Empty;
	}

	public static int GetConsumeCount(this RuleSelectorType value)
	{
		System.Reflection.FieldInfo? fieldInfo = value.GetType()?.GetField(value.ToString());
		if(fieldInfo is null)
		{
			return 0;
		}

		RuleKeywordAttribute[] attributes = (RuleKeywordAttribute[])fieldInfo.GetCustomAttributes(typeof(RuleKeywordAttribute), false);
		return attributes.Length > 0 ? attributes[0].ConsumeCount : 0;
	}
}
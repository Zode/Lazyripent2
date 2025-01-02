namespace Lazyripent2.Fgd;

public class FgdClass
{
	public string ClassName {get; set;} = string.Empty;
	public Dictionary<string, string> DefaultKeyValues {get; private set;} = [];
	public List<string> Parents {get; private set;} = [];

	public bool AddDefaultKeyValue(string key, string value)
	{
		if(DefaultKeyValues.ContainsKey(key))
		{
			DefaultKeyValues[key] = value;
			return false;
		}

		DefaultKeyValues.Add(key, value);
		return true;
	}

	public bool AddParent(string parentClass)
	{
		if(Parents.Contains(parentClass))
		{
			return false;
		}

		Parents.Add(parentClass);
		return true;
	}
}
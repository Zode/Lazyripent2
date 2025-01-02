using System.Text;
using Newtonsoft.Json;

namespace Lazyripent2;

public class Entity
{
	public Dictionary<string, string> KeyValues {get; private set;} = [];
	[JsonIgnore] public bool Discarded {get; set;} = false;
	[JsonIgnore] public bool MarkedForDeletion {get; set;} = false;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
    public string GetValue(string key)
    {
		if(KeyValues.TryGetValue(key, out string? value))
		{
			return value;
		}

		throw new Exception($"Tried to get value for a nonexistent key \"{key}\"");
    }

	/// <summary>
	/// </summary>
	/// <param name="key"></param>
	/// <param name="value"></param>
	/// <returns>true if key was added, false if it was modified</returns>
	public bool AddKeyValue(string key, string value)
	{
		if(KeyValues.ContainsKey(key))
		{
			KeyValues[key] = value;
			return false;
		}

		KeyValues.Add(key, value);
		return true;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="key"></param>
	/// <param name="value"></param>
	/// <exception cref="Exception"></exception>
    public void SetKeyValue(string key, string value)
    {
        if(!KeyValues.ContainsKey(key))
		{
			throw new Exception($"Tried to set value for a nonexistent key \"{key}\"");
		}

		KeyValues[key] = value;
    }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="key"></param>
	/// <exception cref="Exception"></exception>
	public void RemoveKey(string key)
	{
		if(!KeyValues.ContainsKey(key))
		{
			throw new Exception($"Tried to remove a nonexistent key \"{key}\"");
		}

		KeyValues.Remove(key);
	}

    public override string ToString()
    {
		StringBuilder sb = new();

		sb.AppendLine("[Entity, keyvalues: {");
		foreach(string key in KeyValues.Keys)
		{
			sb.Append("\t\"");
			sb.Append(key);
			sb.Append("\":\"");
			sb.Append(KeyValues[key]);
			sb.AppendLine("\"");
		}
		sb.AppendLine("}");

        return sb.ToString();
    }
}
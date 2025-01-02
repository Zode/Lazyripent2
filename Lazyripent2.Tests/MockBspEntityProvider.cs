using Lazyripent2.Bsp;

namespace Lazyripent2.Tests;

public static class MockBspEntityProvider
{
	public static List<Entity> MakeBspEntities(List<Dictionary<string, string>> input)
	{
		List<Entity> entities = [];
		for(int i = 0; i < input.Count; i++)
		{
			Dictionary<string, string> keyValues = input[i];
			foreach(string key in keyValues.Keys)
			{
				Entity entity = new();
				entity.AddKeyValue(key, keyValues[key]);
				entities.Add(entity);
			}
		}

		return entities;
	}
}
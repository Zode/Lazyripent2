namespace Lazyripent2;

public static class EntityUpgrader
{
	private static readonly Dictionary<string, EntityUpgrade> _entityUpgrades = new(){
		{"angle", new("angles", "0 {0} 0")}
	};

	public static List<Entity> UpgradeKeyValues(List<Entity> entities, out int affected)
	{
		affected = 0;
		foreach(Entity entity in entities)
		{
			foreach(string key in _entityUpgrades.Keys)
			{
				if(!entity.KeyValues.ContainsKey(key))
				{
					continue;
				}

				if(entity.KeyValues.ContainsKey(_entityUpgrades[key].NewKey))
				{
					if(entity.KeyValues.ContainsKey("classname"))
					{
						Program.ShowWarning($"Could not upgrade key \"{key}\", entity \"{entity.KeyValues["classname"]}\" already has key \"{_entityUpgrades[key].NewKey}\"");
					}
					else
					{
						Program.ShowWarning($"Could not upgrade key \"{key}\", entity already has key \"{_entityUpgrades[key].NewKey}\"");
					}
					continue;
				}

				entity.AddKeyValue(_entityUpgrades[key].NewKey, string.Format(_entityUpgrades[key].ValueFormat, entity.KeyValues[key]));
				entity.RemoveKey(key);
				affected++;
			}
		}

		return entities;	
	}
}
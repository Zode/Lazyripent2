using Lazyripent2.Map;

namespace Lazyripent2.Tests;

[TestFixture]
public class EntityUpgraderTests
{
[	Test]
	public void KeyValue_Angles()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""angle"" ""-90""
		}
		");

		List<Entity> entities = EntityUpgrader.UpgradeKeyValues(mapFile.GetEntities(), out int affected);
		Assert.Multiple(() =>
		{
			Assert.That(entities[0].KeyValues.ContainsKey("angle"), Is.EqualTo(false));
			Assert.That(entities[0].KeyValues.ContainsKey("angles"), Is.EqualTo(true));
			Assert.That(entities[0].GetValue("angles"), Is.EqualTo("0 -90 0"));
		});

	}
}
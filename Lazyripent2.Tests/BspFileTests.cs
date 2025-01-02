using Lazyripent2.Bsp;

namespace Lazyripent2.Tests;

[TestFixture]
public class BspFileTests
{
	[Test]
	[TestCase(1, 1)]
	[TestCase(1, 10)]
	[TestCase(10, 1)]
	[TestCase(10, 10)]
	public void ValidBsp(int countEntities, int countKeyValues)
	{
		List<Dictionary<string, string>> mockData = [];

		for(int i = 0; i < countEntities; i++)
		{
			Dictionary<string, string> fauxEntity = [];
			for(int j = 0; j < countKeyValues; j++)
			{
				fauxEntity.Add($"imaginary{j}", "imaginary");
			}

			mockData.Add(fauxEntity);
		}

		byte[] bspData = MockBspDataProvider.MakeBspDataWithEntityLump(
			MockBspEntityProvider.MakeBspEntities(mockData));

		Assert.DoesNotThrow(() => {
			BspFile bspFile = new();
			bspFile.DeserializeFromMemory(bspData);
		});
	}
}
using System.Text;
using Lazyripent2.Ent;

namespace Lazyripent2.Tests;

[TestFixture]
public class EntFileTests
{
	[Test]
	[TestCase(1, 1)]
	[TestCase(1, 10)]
	[TestCase(10, 1)]
	[TestCase(10, 10)]
	public void ValidEnt(int countEntities, int countKeyValues)
	{
		StringBuilder sb = new();
		for(int i = 0; i < countEntities; i++)
		{
			sb.AppendLine(@"{""KeyValues"" : {");
			for(int j = 0; j < countKeyValues; j++)
			{
				if(j == countKeyValues - 1)
				{
					sb.AppendLine(@"""imaginary"" : ""imaginary""");
				}
				else
				{
					sb.AppendLine(@"""imaginary"" : ""imaginary"",");
				}
			}

			if(i == countEntities - 1)
			{
				sb.AppendLine(@"}}");
			}
			else
			{
				sb.AppendLine(@"}},");
			}
		}

		Assert.DoesNotThrow(() => {
			EntFile entFile = new();
			entFile.DeserializeFromMemory(@"[
			{
				""KeyValues"" : {
					""imaginary"" : ""imaginary""
				}
			}
			]");
		});
	}
}
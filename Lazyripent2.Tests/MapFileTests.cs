using System.Text;
using Lazyripent2.Map;

namespace Lazyripent2.Tests;

[TestFixture]
public class MapFileTests
{
	[Test]
	[TestCase(1, 1)]
	[TestCase(1, 10)]
	[TestCase(10, 1)]
	[TestCase(10, 10)]
	public void ValidMap_NoBrushFace(int countEntities, int countKeyValues)
	{
		StringBuilder sb = new();
		for(int i = 0; i < countEntities; i++)
		{
			sb.AppendLine(@"{");
			for(int j = 0; j < countKeyValues; j++)
			{
				sb.AppendLine(@"""imaginary"" ""imaginary""");
			}

			sb.AppendLine(@"}");
		}

		Assert.DoesNotThrow(() => {
			MapFile mapFile = new();
			mapFile.DeserializeFromMemory(sb.ToString());
		});
	}

	[Test]
	[TestCase(1, 0, 1)]
	[TestCase(1, 1, 1)]
	[TestCase(1, 1, 10)]
	[TestCase(1, 10, 1)]
	[TestCase(1, 10, 10)]
	[TestCase(10, 0, 1)]
	[TestCase(10, 1, 1)]
	[TestCase(10, 1, 10)]
	[TestCase(10, 10, 1)]
	[TestCase(10, 10, 10)]
	public void ValidMap_BrushFaces(int countEntities, int countKeyValues, int countBrushFaces)
	{
		StringBuilder sb = new();
		for(int i = 0; i < countEntities; i++)
		{
			sb.AppendLine(@"{");
			for(int j = 0; j < countKeyValues; j++)
			{
				sb.AppendLine(@"""imaginary"" ""imaginary""");
			}

			for(int j = 0; j < countBrushFaces; j++)
			{
				sb.AppendLine(@"
				{
					( 3008 960 448 ) ( 3008 960 0 ) ( 3008 1024 448 ) NULL [ -0 -1 -0 0 ] [ -0 -0 -1 0 ] 0 2 2
					( 3072 1024 448 ) ( 3072 1024 0 ) ( 3072 960 448 ) NULL [ -0 1 -0 -0 ] [ -0 -0 -1 0 ] 0 2 2
					( 3008 1024 448 ) ( 3008 1024 0 ) ( 3072 1024 448 ) NULL [ 1 0 0 -0 ] [ 0 0 -1 0 ] 0 2 2
					( 3072 960 448 ) ( 3072 960 0 ) ( 3008 960 448 ) NULL [ 1 -0 -0 -0 ] [ -0 -0 -1 0 ] 0 2 2
					( 3008 960 448 ) ( 3008 1024 448 ) ( 3072 960 448 ) NULL [ 1 -0 -0 -0 ] [ -0 -1 -0 0 ] 0 2 2
					( 3008 1024 0 ) ( 3008 960 0 ) ( 3072 1024 0 ) NULL [ 1 0 0 -0 ] [ 0 -1 0 0 ] 0 2 2
				}
				");
			}

			sb.AppendLine(@"}");
		}
		
		Assert.DoesNotThrow(() => {
			MapFile mapFile = new();
			mapFile.DeserializeFromMemory(sb.ToString());
		});
	}

	[Test]
	public void InvalidMap_SingleEntity_NoBrushFace_SingleIncompleteKey()
	{
		Assert.Catch(() => {
			MapFile mapFile = new();
			mapFile.DeserializeFromMemory(@"
			{
				""imaginary""
			}
			");
		});
	}

	[Test]
	public void InvalidMap_SingleEntity_SingleIncompeleteBrushFace_SingleKey()
	{
		Assert.Catch(() => {
			MapFile mapFile = new();
			mapFile.DeserializeFromMemory(@"
			{
				""imaginary"" ""imaginary""
				{
					( 3008 960 448 ) ( 3008 960 0 ) ( 3008 1024 448 ) NULL [ -0 -1 -0 0 ] [ -0 -0 -1 0 ] 0 2 2
					( 3072 1024 448 ) ( 3072 1024 0 ) ( 3072 960 448 ) NULL [ -0 1 -0 -0 ] [ -0 -0 -1 0 ] 0 2 2
					( 3008 1024 448 ) ( 3008 1024 0 ) ( 3072 102
				}
			}
			");
		});
	}
}
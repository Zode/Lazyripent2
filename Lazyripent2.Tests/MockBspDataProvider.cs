using System.Text;
using Lazyripent2.Bsp;

namespace Lazyripent2.Tests;

public static class MockBspDataProvider
{
	public static byte[] MakeBspDataWithEntityLump(List<Entity> entities)
	{
		MemoryStream ms = new();
		BinaryWriter writer = new(ms);

		writer.Write((int)30); //BSP30
		for(int i = 0; i < (int)LumpType.TotalLumpTypes; i++)
		{
			writer.Write((int)0); //lump offset
			writer.Write((int)0); //lump length
		}

		int streamPos = (int)writer.BaseStream.Position;

		byte[] lumpData = WriteBspEntities(entities);
		writer.BaseStream.Seek(sizeof(int) + (int)LumpType.Entities, SeekOrigin.Begin); //header + lump
		//write actual length and offset now
		writer.Write(streamPos);
		writer.Write(lumpData.Length);
		
		writer.BaseStream.Seek(streamPos, SeekOrigin.Begin);
		writer.Write(lumpData);

		return ms.ToArray();
	}

	private static byte[] WriteBspEntities(List<Entity> entities)
	{
		StringBuilder sb = new();
		foreach(Entity entity in entities)
		{
			sb.AppendLine("{");
			foreach(string key in entity.KeyValues.Keys)
			{
				sb.Append('"');
				sb.Append(key);
				sb.Append("\" \"");
				sb.Append(entity.GetValue(key));
				sb.AppendLine("\"");
			}

			sb.AppendLine("}");
		}

		sb.Append('\0');
		return UTF8Encoding.UTF8.GetBytes(sb.ToString());
	}
}
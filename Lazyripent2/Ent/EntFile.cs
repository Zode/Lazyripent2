using System.Text;
using Lazyripent2.Bsp;
using Newtonsoft.Json;

namespace Lazyripent2.Ent;

public class EntFile : IParseableFile
{
	private List<Entity> _entities = [];
	private string _fullPath = string.Empty;

	public void DeserializeFromFile(string fullPath)
	{
		_fullPath = fullPath;
		if(Options.Verbose)
		{
			Console.WriteLine($"Opening ent file for read: \"{fullPath}\"");
		}

		using StreamReader reader = File.OpenText(fullPath);
		JsonSerializer serializer = new();
		List<Entity>? entities = (List<Entity>?)serializer.Deserialize(reader, typeof(List<Entity>)) ?? throw new FileFormatException("Failed to deserialize entities");
        _entities = entities;

		if(_entities.Count == 0)
		{
			throw new FileFormatException($"Read entities from ent, and got 0 entities: \"{fullPath}\"");
		}

		if(Options.Verbose)
		{
			Console.WriteLine($"Ent read OK, got {_entities.Count} entities: \"{fullPath}\"");
		}
	}

	//only used for testing
	public void DeserializeFromMemory(string source)
	{
		_fullPath = "memory";
		if(Options.Verbose)
		{
			Console.WriteLine($"Opening ent file from memory");
		}

		List<Entity>? entities = (List<Entity>?)JsonConvert.DeserializeObject(source, typeof(List<Entity>)) ?? throw new FileFormatException("Failed to deserialize entities");
        _entities = entities;

		if(_entities.Count == 0)
		{
			throw new FileFormatException($"Read entities from ent, and got 0 entities");
		}

		if(Options.Verbose)
		{
			Console.WriteLine($"Ent read OK, got {_entities.Count} entities");
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="fullPath"></param>
	/// <exception cref="FileFormatException"></exception>
	public void SerializeToFile(string fullPath)
	{
		if(File.Exists(fullPath))
		{
			if(Options.Verbose)
			{
				Console.WriteLine($"Deleting existing ent file: \"{fullPath}\"");
			}

			File.Delete(fullPath);
		}

		if(Options.Verbose)
		{
			Console.WriteLine($"Opening ent file for write: \"{fullPath}\"");
		}

		using StreamWriter writer = File.CreateText(fullPath);
		JsonSerializer serializer = new()
		{
			Formatting = Formatting.Indented
		};
		
		serializer.Serialize(writer, _entities);
		if(Options.Verbose)
		{
			StringBuilder sb = new();
			foreach(Entity entity in _entities)
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

			byte[] data = UTF8Encoding.UTF8.GetBytes(sb.ToString());
			int length = data.Length;

			if(length > BspFile.MAX_MAP_ENTSTRING)
			{
				Program.ShowWarning($"estimated entity lump size ({length}) over limit ({BspFile.MAX_MAP_ENTSTRING})");
			}

			Console.WriteLine($"Estimated entity lump size usage: {(((float)length / (float)BspFile.MAX_MAP_ENTSTRING) * 100.0f):0.##}% ({length} / {BspFile.MAX_MAP_ENTSTRING})");
			Console.WriteLine($"Ent write OK: \"{fullPath}\"");
		}
	}

	public List<Entity> GetEntities()
	{
		return _entities;
	}

	public void SetEntities(List<Entity> entities)
	{
		_entities = entities;
	}

	public string GetFileNameNoExt()
	{
		return Path.GetFileNameWithoutExtension(_fullPath);
	}
}
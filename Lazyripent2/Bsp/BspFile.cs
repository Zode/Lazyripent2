using System.Text;
using Lazyripent2.Lexer;

namespace Lazyripent2.Bsp;

public class BspFile : IParseableFile
{
	private class BspLump
	{
		public Int32 Offset {get; set;} = 0;
		public Int32 Length {get; set;} = 0;
		public byte[] Data {get; set;} = [];
	}

	//MAX_MAP_ENTSTRING is the actual lump maximum size, but it may contain MAX_MAP_ENTITIES amount of entities somehow.
	public const int MAX_MAP_ENTSTRING = 2048 * 1024; //vhlt-v34 limit
	public const int MAX_KEY = 32;
	public const int MAX_VALUE = 1024;
    private BspLump[] _lumps = new BspLump[(int)LumpType.TotalLumpTypes];
	private int _dataStart = 0;
	private string _fullPath = string.Empty;
	private List<Entity> _entities = [];

	/// <summary>
	/// 
	/// </summary>
	/// <param name="fullPath"></param>
	/// <exception cref="FileParserException"></exception>
	public void DeserializeFromFile(string fullPath)
	{
		_fullPath = fullPath;

		if(Options.Verbose)
		{
			Console.WriteLine($"Opening bsp file for read: \"{fullPath}\"");
		}

		using FileStream fs = File.OpenRead(fullPath);
		using BinaryReader reader = new(fs);

		int bspVersion = reader.ReadInt32();
		if(bspVersion != 30)
		{
			throw new FileParserException($"Unsupported bsp version BSP{bspVersion}, expected BSP30");
		}

		for(int i = 0; i < (int)LumpType.TotalLumpTypes; i++)
		{
			_lumps[i] = new()
			{
				Offset = reader.ReadInt32(),
				Length = reader.ReadInt32()
			};

			if(_lumps[i].Length > ((LumpType)i).GetMaxLength())
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine($"unsupported bsp specification, lump \"{(LumpType)i}\" had a size ({_lumps[i].Length}) bigger than maximum expected ({((LumpType)i).GetMaxLength()})");
				Console.ForegroundColor = ConsoleColor.White;
				ExitCode.CheckWarningAsFatal();
			}
		}

		_dataStart = (int)reader.BaseStream.Position;
		for(int i = 0; i < (int)LumpType.TotalLumpTypes; i++)
		{
			reader.BaseStream.Seek(_lumps[i].Offset, SeekOrigin.Begin);

			_lumps[i].Data = reader.ReadBytes(_lumps[i].Length);
		}

		_entities = [];
		ParseBspEntities();

		if(Options.Verbose)
		{
			Console.WriteLine($"Bsp read OK: \"{fullPath}\"");
		}
	}

	/// <summary>
	/// only used for testing
	/// </summary>
	/// <param name="buffer"></param>
	/// <exception cref="FileParserException"></exception>
	public void DeserializeFromMemory(byte[] buffer)
	{
		_fullPath = "memory";

		if(Options.Verbose)
		{
			Console.WriteLine($"Opening bsp file from memory");
		}

		using MemoryStream ms = new(buffer);
		using BinaryReader reader = new(ms);

		int bspVersion = reader.ReadInt32();
		if(bspVersion != 30)
		{
			throw new FileParserException($"Unsupported bsp version BSP{bspVersion}, expected BSP30");
		}

		for(int i = 0; i < (int)LumpType.TotalLumpTypes; i++)
		{
			_lumps[i] = new()
			{
				Offset = reader.ReadInt32(),
				Length = reader.ReadInt32()
			};

			if(_lumps[i].Length > ((LumpType)i).GetMaxLength())
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine($"unsupported bsp specification, lump \"{(LumpType)i}\" had a size ({_lumps[i].Length}) bigger than maximum expected ({((LumpType)i).GetMaxLength()})");
				Console.ForegroundColor = ConsoleColor.White;
				ExitCode.CheckWarningAsFatal();
			}
		}

		_dataStart = (int)reader.BaseStream.Position;
		for(int i = 0; i < (int)LumpType.TotalLumpTypes; i++)
		{
			reader.BaseStream.Seek(_lumps[i].Offset, SeekOrigin.Begin);

			_lumps[i].Data = reader.ReadBytes(_lumps[i].Length);
		}

		_entities = [];
		ParseBspEntities();

		if(Options.Verbose)
		{
			Console.WriteLine($"Bsp read OK");
		}
	}

	public void SerializeToFile(string fullPath)
	{
		WriteBspEntities();

		if(File.Exists(fullPath))
		{
			if(Options.Verbose)
			{
				Console.WriteLine($"Deleting existing bsp file: \"{fullPath}\"");
			}

			File.Delete(fullPath);
		}

		if(Options.Verbose)
		{
			Console.WriteLine($"Opening bsp file for write: \"{fullPath}\"");
		}

		using FileStream fs = File.OpenWrite(fullPath);
		using BinaryWriter writer = new(fs);

		writer.Write(30); //HLBSP30
		for(int i = 0; i < (int)LumpType.TotalLumpTypes; i++)
		{
			writer.Write(_lumps[i].Offset);
			writer.Write(_lumps[i].Length);
		}

		for(int i = 0; i < (int)LumpType.TotalLumpTypes; i++)
		{
			writer.Seek(_lumps[i].Offset, SeekOrigin.Begin);
			writer.Write(_lumps[i].Data);
		}

		if(Options.Verbose)
		{
			Console.WriteLine($"Bsp write OK: \"{fullPath}\"");
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <exception cref="Exception"></exception>
	/// <exception cref="FileFormatException"></exception>
	private void ParseBspEntities()
	{
		if(Options.Verbose)
		{
			Console.WriteLine($"Parsing bsp entities");
		}

		if(_lumps[(int)LumpType.Entities] is null || _lumps[(int)LumpType.Entities].Data.Length == 0)
		{
			throw new Exception("Tried to read entities from an unloaded bsp??");
		}

		FileParser fileParser = new(_lumps[(int)LumpType.Entities].Data);

		/*
		bsp entity lump format is as follows:
		{
			"key" "value"
			...
			"key" "value"
		}
		*/
		while(!fileParser.IsAtEnd())
		{
			fileParser.ExpectToken(TokenType.LeftBrace, true);
			Entity bspEntity = new();

			KeyValueScan:
			if(fileParser.GetTokenType() == TokenType.RightBrace)
			{
				_entities.Add(bspEntity);
				fileParser.AdvanceToken();
				continue;
			}

			string key = fileParser.ReadString();
			string value = fileParser.ReadString();
			if(!bspEntity.AddKeyValue(key, value))
			{
				Program.ShowWarning($"replacing existing bsp key ({key}) with new value ({value}) for index: {_entities.Count - 1}", fileParser);
			}

			goto KeyValueScan;
		}

		if(_entities.Count == 0)
		{
			throw new FileFormatException($"Parsed entities from bsp, and got 0 entities: \"{_fullPath}\"");
		}

		if(Options.Verbose)
		{
			Console.WriteLine($"Bsp entities parse OK: {_entities.Count} entities parsed");
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

	/// <summary>
	/// 
	/// </summary>
	/// <exception cref="Exception"></exception>
	/// <exception cref="FileFormatException"></exception>
	public void WriteBspEntities()
	{
		if(Options.Verbose)
		{
			Console.WriteLine($"Writing bsp entities lump");
		}

		if(_lumps[(int)LumpType.Entities] is null || _lumps[(int)LumpType.Entities].Data.Length == 0)
		{
			throw new Exception("Tried to write entities to an unloaded bsp??");
		}

		StringBuilder sb = new();
		foreach(Entity entity in _entities)
		{
			sb.AppendLine("{");
			foreach(string key in entity.KeyValues.Keys)
			{
				if(key.Length > MAX_KEY)
				{
					throw new FileFormatException($"Entity key \"{key}\" length ({key.Length}) over limit ({MAX_KEY}): \"{_fullPath}\"");
				}

				string value = entity.GetValue(key);
				if(value.Length > MAX_VALUE)
				{
					throw new FileFormatException($"Entity key \"{key}\" value \"{value}\" length ({value.Length}) over limit ({MAX_VALUE}): \"{_fullPath}\"");
				}

				sb.Append('"');
				sb.Append(key);
				sb.Append("\" \"");
				sb.Append(value);
				sb.AppendLine("\"");
			}

			sb.AppendLine("}");
		}

		_lumps[(int)LumpType.Entities].Data = UTF8Encoding.UTF8.GetBytes(sb.ToString());
		_lumps[(int)LumpType.Entities].Length = _lumps[(int)LumpType.Entities].Data.Length;
		int length = _lumps[(int)LumpType.Entities].Length;

		if(length > MAX_MAP_ENTSTRING)
		{
			throw new FileFormatException($"Entity lump size ({length}) over limit ({MAX_MAP_ENTSTRING}): \"{_fullPath}\"");
		}

		if(Options.Verbose)
		{
			Console.WriteLine($"Entity lump size usage: {(((float)length / (float)MAX_MAP_ENTSTRING) * 100.0f):0.##}% ({length} / {MAX_MAP_ENTSTRING})");
			Console.WriteLine($"Bsp entities lump write OK");
		}

		RecalculateLumpOffsets();
	}

	private void RecalculateLumpOffsets()
	{
		if(Options.Verbose)
		{
			Console.WriteLine($"Recalculating bsp offsets");
		}

		int currentOffset = (int)_dataStart;
		for(int i = 0; i < (int)LumpType.TotalLumpTypes; i++)
		{
			_lumps[i].Offset = (int)currentOffset;
			currentOffset += _lumps[i].Length;
		}
	}

	public string GetFileNameNoExt()
	{
		return Path.GetFileNameWithoutExtension(_fullPath);
	}
}
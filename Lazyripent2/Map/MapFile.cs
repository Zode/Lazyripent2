using System.Text;
using Lazyripent2.Bsp;
using Lazyripent2.Lexer;

namespace Lazyripent2.Map;

public class MapFile : IParseableFile
{
	private List<MapEntity> _entities = [];
	private string _fullPath = string.Empty;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="fullPath"></param>
	/// <exception cref="FileFormatException"></exception>
	public void DeserializeFromFile(string fullPath)
	{
		_fullPath = fullPath;

		if(Options.Verbose)
		{
			Console.WriteLine($"Opening map file for read: \"{fullPath}\"");
		}

		using StreamReader reader = File.OpenText(fullPath);
		string source = reader.ReadToEnd();
		if(string.IsNullOrWhiteSpace(source))
		{
			throw new FileFormatException($"File contents were empty: \"{fullPath}\"");
		}

		_entities = [];
		ParseMapEntities(source);

		if(Options.Verbose)
		{
			Console.WriteLine($"Map read OK: \"{fullPath}\"");
		}
	}

	/// <summary>
	/// only used for testing
	/// </summary>
	/// <param name="source"></param>
	/// <exception cref="FileFormatException"></exception>
	public void DeserializeFromMemory(string source)
	{
		_fullPath = "memory";

		if(Options.Verbose)
		{
			Console.WriteLine($"Opening map file from memory");
		}

		if(string.IsNullOrWhiteSpace(source))
		{
			throw new FileFormatException($"Memory contents were empty");
		}

		_entities = [];
		ParseMapEntities(source);

		if(Options.Verbose)
		{
			Console.WriteLine($"Map read OK");
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
				Console.WriteLine($"Deleting existing map file: \"{fullPath}\"");
			}

			File.Delete(fullPath);
		}

		if(Options.Verbose)
		{
			Console.WriteLine($"Opening map file for write: \"{fullPath}\"");
		}

		using StreamWriter writer = File.CreateText(fullPath);
		foreach(MapEntity entity in _entities)
		{
			writer.WriteLine("{");
			foreach(string key in entity.KeyValues.Keys)
			{
				if(key.Length > BspFile.MAX_KEY)
				{
					throw new FileFormatException($"Entity key \"{key}\" length ({key.Length}) over limit ({BspFile.MAX_KEY}): \"{_fullPath}\"");
				}

				string value = entity.GetValue(key);
				if(value.Length > BspFile.MAX_VALUE)
				{
					throw new FileFormatException($"Entity key \"{key}\" value \"{value}\" length ({value.Length}) over limit ({BspFile.MAX_VALUE}): \"{_fullPath}\"");
				}

				writer.Write('"');
				writer.Write(key);
				writer.Write("\" \"");
				writer.Write(value);
				writer.WriteLine("\"");
			}

			for(int i = 0; i < entity.BrushFaces.Count; i++)
			{
				writer.WriteLine("{");
				foreach(MapBrushFace brushFace in entity.BrushFaces[i])
				{
					foreach(MapBrushFace.Plane plane in brushFace.Planes)
					{
						writer.Write("( ");
						for(int j = 0; j < 3; j++)
						{
							writer.Write(plane.Point[j]);
							writer.Write(' ');
						}

						writer.Write(") ");
					}

					writer.Write(brushFace.Texture);
					writer.Write(' ');
					for(int j = 0; j < 2; j++)
					{
						writer.Write("[ ");
						for(int k = 0; k < 3; k++)
						{
							writer.Write(brushFace.TextureInfos[j].Normal[k]);
							writer.Write(' ');
						}

						writer.Write(brushFace.TextureInfos[j].Offset);
						writer.Write(" ] ");
					}

					writer.Write(brushFace.Rotation);
					writer.Write(' ');
					writer.Write(brushFace.UScale);
					writer.Write(' ');
					writer.Write(brushFace.VScale);
					writer.Write('\n');
				}
				
				writer.WriteLine("}");
			}

			writer.WriteLine("}");
		}
		

		if(Options.Verbose)
		{
			StringBuilder sb = new();
			foreach(MapEntity entity in _entities)
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
			Console.WriteLine($"Map write OK: \"{fullPath}\"");
		}
	}

	private void ParseMapEntities(string source)
	{
		if(Options.Verbose)
		{
			Console.WriteLine($"Parsing map entities");
		}

		FileParser fileParser = new FileParser(source);
		while(!fileParser.IsAtEnd())
		{
			fileParser.ExpectToken(TokenType.LeftBrace);
			MapEntity entity = new();
			ReadKeyValues(ref fileParser, ref entity);

			while(fileParser.GetTokenType() == TokenType.LeftBrace)
			{
				ReadBrushFaces(ref fileParser, ref entity);
			}

			fileParser.ExpectToken(TokenType.RightBrace);
			_entities.Add(entity);
		}

		if(_entities.Count == 0)
		{
			throw new FileFormatException($"Parsed entities from map, and got 0 entities");
		}

		if(Options.Verbose)
		{
			Console.WriteLine($"Map entities parse OK: {_entities.Count} entities parsed");
		}
	}

	public List<Entity> GetEntities()
	{
		return _entities.Cast<Entity>().ToList();
	}

	public void SetEntities(List<Entity> entities)
	{
		_entities = entities.Cast<MapEntity>().ToList();
	}

	private void ReadKeyValues(ref FileParser fileParser, ref MapEntity entity)
	{
		while(fileParser.GetTokenType() != TokenType.LeftBrace && fileParser.GetTokenType() != TokenType.RightBrace)
		{
			string key = fileParser.ReadString();
			string value = fileParser.ReadString();

			if(!entity.AddKeyValue(key, value))
			{
				Program.ShowWarning($"Replacing existing map key \"{key}\" with new value \"{value}\" for index: {_entities.Count}", fileParser);
			}
		}
	}

	private void ReadBrushFaces(ref FileParser fileParser, ref MapEntity entity)
	{
		fileParser.ExpectToken(TokenType.LeftBrace);
		entity.BrushFaces.Add([]);
		while(fileParser.GetTokenType() != TokenType.RightBrace)
		{
			MapBrushFace brushFace = new();
			for(int i = 0; i < 3; i++)
			{
				fileParser.ExpectToken(TokenType.LeftParenthesis);
				MapBrushFace.Plane plane = new();
				for(int j = 0; j < 3; j++)
				{
					plane.Point[j] = fileParser.ReadFloat();
				}

				brushFace.Planes[i] = plane;
				fileParser.ExpectToken(TokenType.RightParenthesis);
			}
			
			brushFace.Texture = ReadBrushFaceTexture(ref fileParser);
			for(int i = 0; i < 2; i++)
			{
				fileParser.ExpectToken(TokenType.LeftBracket);
				MapBrushFace.TextureInfo textureInfo = new();
				for(int j = 0; j < 3; j++)
				{
					textureInfo.Normal[j] = fileParser.ReadFloat();
				}

				textureInfo.Offset = fileParser.ReadFloat();
				fileParser.ExpectToken(TokenType.RightBracket);
				brushFace.TextureInfos[i] = textureInfo;
			}

			brushFace.Rotation = fileParser.ReadFloat();
			brushFace.UScale = fileParser.ReadFloat();
			brushFace.VScale = fileParser.ReadFloat();
			entity.BrushFaces[^1].Add(brushFace);
		}

		fileParser.AdvanceToken(); //consume the right bracket 
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="fileParser"></param>
	/// <returns></returns>
	/// <exception cref="FileFormatException"></exception>
	private string ReadBrushFaceTexture(ref FileParser fileParser)
	{
		//goldsrc texture name can contain the following: special chacaters { ! ~ + -
		//of which the following will be recognized as separate tokens: { ! + -
		//so the hacky solve is to just consume EVERYTHING into the same string until we hit a left bracket
		//as it is safe to assume at this stage we have nothing else but the texture name ahead of us.
		//this wouldn't have been an issue if the .map format actually utilized quotes from strings.. boo!

		StringBuilder sb = new();
		while(fileParser.GetTokenType() != TokenType.LeftBracket)
		{
			if(fileParser.IsAtEnd())
			{
				throw new FileFormatException($"Unexpected end of file: \"{_fullPath}\"", fileParser);
			}

			sb.Append(fileParser.GetTokenLexeme());
			fileParser.AdvanceToken();
		}

		return sb.ToString();
	}

	public string GetFileNameNoExt()
	{
		return Path.GetFileNameWithoutExtension(_fullPath);
	}
}
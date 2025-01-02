using Lazyripent2.Bsp;
using Lazyripent2.Lexer;

namespace Lazyripent2.Fgd;

public class FgdFile
{
	private List<FgdClass> _fgdClasses = [];
	private string _fullPath = string.Empty;

	private enum KeyValueType
	{
		String,
		Numeric,
		Choices,
		Flags,
		Decal,
	}

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
			Console.WriteLine($"Opening fgd file for read: \"{fullPath}\"");
		}

		using StreamReader reader = File.OpenText(fullPath);
		string source = reader.ReadToEnd();
		if(string.IsNullOrWhiteSpace(source))
		{
			throw new FileFormatException($"File contents were empty: \"{fullPath}\"");
		}

		_fgdClasses = [];
		ParseFgdEntires(source);
		if(_fgdClasses.Count == 0)
		{
			throw new FileFormatException($"Read fgd entries from fgd, and got 0 entries: \"{fullPath}\"");
		}

		SolveFgdInheritance();

		if(Options.Verbose)
		{
			Console.WriteLine($"Fgd read OK: \"{fullPath}\"");
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
			Console.WriteLine($"Opening fgd file from memory");
		}

		if(string.IsNullOrWhiteSpace(source))
		{
			throw new FileFormatException($"Memory contents were empty");
		}

		_fgdClasses = [];
		ParseFgdEntires(source);
		if(_fgdClasses.Count == 0)
		{
			throw new FileFormatException($"Read fgd entries from fgd, and got 0 entries");
		}

		SolveFgdInheritance();

		if(Options.Verbose)
		{
			Console.WriteLine($"Fgd read");
		}
	}

	private void ParseFgdEntires(string source)
	{
		if(Options.Verbose)
		{
			Console.WriteLine($"Parsing fgd entities");
		}

		FileParser fileParser = new(source);

		while(!fileParser.IsAtEnd())
		{
			//@BaseClass @SolidClass @PointClass
			fileParser.ExpectToken(TokenType.AtSign);
			//we don't really care about the class type.
			fileParser.ReadIdentifier();

			FgdClass fgdClass = new();
			ReadClassDefinition(ref fileParser, ref fgdClass);
			ReadClassKeyValues(ref fileParser, ref fgdClass);
			_fgdClasses.Add(fgdClass);
		}

		if(Options.Verbose)
		{
			Console.WriteLine($"Fgd entities parse OK");
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="fileParser"></param>
	/// <param name="fgdClass"></param>
	/// <exception cref="FileFormatException"></exception>
	private static void ReadClassDefinition(ref FileParser fileParser, ref FgdClass fgdClass)
	{
		//we have the following options:
		//optional base(identifier), multiple identifier can be defined with a comma delimiting them.
		//optional size(numeric numeric numeric, numeric numeric numeric)
		//optional color(numeric numeric numeric)
		//optional iconsprite() sprite() studio() or decal()
		//required = identifier (this is the classname identifier)
		//optional : string (human readable)
		while(fileParser.GetTokenType() != TokenType.Equal)
		{
			fileParser.ExpectToken(TokenType.Identifier, false);
			string identifier = fileParser.ReadIdentifier();
			switch(identifier)
			{
				case "base": // base(identifier) or base(identifier, identifier, ... identifier)
					fileParser.ExpectToken(TokenType.LeftParenthesis);
					
					while(fileParser.GetTokenType() != TokenType.RightParenthesis)
					{
						if(fileParser.GetTokenType() == TokenType.Identifier)
						{
							string parentClass = fileParser.ReadIdentifier();
							if(!fgdClass.AddParent(parentClass))
							{
								throw new FileFormatException($"Tried to add existing parent class \"{parentClass}\"", fileParser);
							}

							continue;
						}

						if(fileParser.GetTokenType() == TokenType.Comma)
						{
							fileParser.AdvanceToken();
							continue;
						}

						throw new FileFormatException($"Unexpected {fileParser.GetTokenType()}, expected identifier or colon", fileParser);
					}
					
					fileParser.AdvanceToken(); //consume right parenthesis
					continue;

				case "size": // size(x y z) or size(x y z, x y z)
					fileParser.ExpectToken(TokenType.LeftParenthesis);

					for(int i = 0; i < 3; i++)
					{
						fileParser.ReadInteger();
					}

					if(fileParser.GetTokenType()!= TokenType.Comma)
					{
						fileParser.ExpectToken(TokenType.RightParenthesis);
						continue;
					}

					fileParser.AdvanceToken(); //consume comma
					for(int i = 0; i < 3; i++)
					{
						fileParser.ReadInteger();
					}

					fileParser.ExpectToken(TokenType.RightParenthesis);
					continue;

				case "color": // color(r g b)
					fileParser.ExpectToken(TokenType.LeftParenthesis);
					for(int i = 0; i < 3; i++)
					{
						fileParser.ReadInteger();
					}

					fileParser.ExpectToken(TokenType.RightParenthesis);
					continue;

				case "iconsprite":
				case "studio":
				case "sprite":
				case "decal":
					fileParser.ExpectToken(TokenType.LeftParenthesis);
					//because apparently these CAN be empty..
					if(fileParser.GetTokenType() == TokenType.String)
					{
						fileParser.AdvanceToken();
					}

					fileParser.ExpectToken(TokenType.RightParenthesis);
					continue;

				default:
					throw new FileFormatException($"Unexpected identifier type \"{identifier}\"", fileParser);
			}
		}

		fileParser.AdvanceToken(); //consume equal
		fgdClass.ClassName = fileParser.ReadIdentifier();
		
		//check for optional human readable bit (that isn't really used?)
		if(fileParser.GetTokenType() == TokenType.Colon)
		{
			fileParser.AdvanceToken();
			fileParser.ReadString();
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="fileParser"></param>
	/// <param name="fgdClass"></param>
	/// <exception cref="FileFormatException"></exception>
	/// <exception cref="NotImplementedException"></exception>
	private static void ReadClassKeyValues(ref FileParser fileParser, ref FgdClass fgdClass)
	{
		//is usually one of the following:
		//identifier(identifer) : string
		//first identifier is the key name, second identifier is one of the following: string, integer, or choices.
		//last string is just the human readable.
		//optionally it may contain the default value : numeric for integer or : string for string
		//in case of choices, this is numeric for the index followed by a listing using the following format:
		//equals left bracket
		//  numeric : string (index : human readable)
		//right bracket

		//we convert integers to strings, as that is what the bsp contains. yes it will be slower to compare. no i don't care.
		fileParser.ExpectToken(TokenType.LeftBracket);
		while(fileParser.GetTokenType() != TokenType.RightBracket)
		{
			string key = fileParser.ReadIdentifier();
			fileParser.ExpectToken(TokenType.LeftParenthesis);
			string type = fileParser.ReadIdentifier();
			KeyValueType keyValueType = type.ToLower() switch
            {
                "string" or 
				"target_source" or 
				"target_destination" or
				"color255" or 
				"studio" or 
				"sound" or 
				"sprite" => KeyValueType.String,
                "integer" => KeyValueType.Numeric,
                "choices" => KeyValueType.Choices,
                "flags" => KeyValueType.Flags,
                "decal" => KeyValueType.Decal,
                _ => throw new FileFormatException($"Unexpected key type \"{type}\"", fileParser),
            };

            fileParser.ExpectToken(TokenType.RightParenthesis);
			switch(keyValueType)
			{
				case KeyValueType.Flags:
					ReadFlagsKeyValue(ref fileParser, ref fgdClass, key);
					continue;

				case KeyValueType.Decal:
					continue;
			}

			fileParser.ExpectToken(TokenType.Colon);
			fileParser.ReadString(); //we don't care about the human readable
			if(fileParser.GetTokenType() != TokenType.Colon)
			{
				continue; //doesn't have a default value.
			}

			fileParser.AdvanceToken();
			string value = keyValueType switch
            {
                KeyValueType.String => fileParser.ReadString(),
                KeyValueType.Numeric => fileParser.ReadInteger().ToString(),
                KeyValueType.Choices => ReadChoicesKeyValue(ref fileParser, ref fgdClass),
                _ => throw new NotImplementedException($"Unsupported KeyValueType {keyValueType}"),
            };

			if(!fgdClass.AddDefaultKeyValue(key, value))
			{
				Program.ShowWarning($"Replacing existing fgd key \"{key}\" with new default value \"{value}\" for class \"{fgdClass.ClassName}\"", fileParser);
			}
		}

		fileParser.AdvanceToken(); //consume right bracket
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="fileParser"></param>
	/// <param name="fgdClass"></param>
	/// <returns></returns>
	/// <exception cref="FileFormatException"></exception>
	private static string ReadChoicesKeyValue(ref FileParser fileParser, ref FgdClass fgdClass)
	{
		string defaultValue = fileParser.GetTokenType() switch
        {
            TokenType.String => fileParser.ReadString(),
            TokenType.Number => fileParser.ReadInteger().ToString(),
            TokenType.Minus => fileParser.ReadInteger().ToString(),
            _ => throw new FileFormatException($"Unexpected type \"{fileParser.GetTokenType()}\" in fgd choices", fileParser),
        };

		fileParser.ExpectToken(TokenType.Equal);
		fileParser.ExpectToken(TokenType.LeftBracket);
		while(fileParser.GetTokenType() != TokenType.RightBracket)
		{
			if(fileParser.GetTokenType() == TokenType.Minus)
			{
				fileParser.AdvanceToken();
			}

			switch(fileParser.GetTokenType())
			{
				case TokenType.Minus:
				case TokenType.Number:
				case TokenType.String:
					fileParser.AdvanceToken(); //consume
					break;

				default:
					throw new FileFormatException($"Unexpected type \"{fileParser.GetTokenType()}\" in fgd choices", fileParser);
			}
			
			fileParser.ExpectToken(TokenType.Colon);
			fileParser.ReadString();
		}

		fileParser.AdvanceToken(); //consume the right bracket
		return defaultValue;
	}

	private static void ReadFlagsKeyValue(ref FileParser fileParser, ref FgdClass fgdClass, string key)
	{
		fileParser.ExpectToken(TokenType.Equal);
		fileParser.ExpectToken(TokenType.LeftBracket);
		int totalValue = 0;
		while(fileParser.GetTokenType() != TokenType.RightBracket)
		{
			int flagValue = fileParser.ReadInteger();
			fileParser.ExpectToken(TokenType.Colon);
			fileParser.ReadString();
			fileParser.ExpectToken(TokenType.Colon);

			if(fileParser.ReadInteger() > 0)
			{
				totalValue += flagValue;
			}
		}

		fileParser.AdvanceToken(); //consume the right bracket
		if(!fgdClass.AddDefaultKeyValue(key, totalValue.ToString()))
		{
			Program.ShowWarning($"Replacing existing fgd key \"{key}\" with new value default \"{totalValue}\" for class \"{fgdClass.ClassName}\"", fileParser);
		}
	}

	private void SolveFgdInheritance()
	{
		foreach(FgdClass fgdClass in _fgdClasses)
		{
			if(fgdClass.Parents.Count == 0)
			{
				continue;
			}

			for(int i = fgdClass.Parents.Count - 1; i >= 0; i--)
			{
				FgdClass? parentClass = _fgdClasses.Where(x => x.ClassName == fgdClass.Parents[i]).FirstOrDefault();
				if(parentClass is null)
				{
					Program.ShowWarning($"fgd class \"{fgdClass.ClassName}\" has a nonexistent base fgd class \"{fgdClass.Parents[i]}\"");
					continue;
				}

				SolveFgdInheritance(fgdClass, parentClass);
			}
		}
	}

	private void SolveFgdInheritance(FgdClass targetClass, FgdClass recursiveClass)
	{
		foreach(string key in recursiveClass.DefaultKeyValues.Keys)
		{
			if(targetClass.DefaultKeyValues.ContainsKey(key))
			{
				continue;
			}

			targetClass.AddDefaultKeyValue(key, recursiveClass.DefaultKeyValues[key]);
		}

		for(int i = recursiveClass.Parents.Count - 1; i >= 0; i--)
		{
			FgdClass? parentClass = _fgdClasses.Where(x => x.ClassName == recursiveClass.Parents[i]).FirstOrDefault();
			if(parentClass is null)
			{
				Program.ShowWarning($"fgd class \"{recursiveClass.ClassName}\" has a nonexistent base fgd class \"{recursiveClass.Parents[i]}\"");
				continue;
			}

			SolveFgdInheritance(targetClass, parentClass);
		}
	}

	public List<Entity> StripDefaultKeyvaluesFromEntities(List<Entity> entities, out int stripCount)
	{
		stripCount = 0;
		foreach(Entity entity in entities)
		{
			if(!entity.KeyValues.ContainsKey("classname"))
			{
				Program.ShowWarning($"bsp entity does not contain classname key");
				continue;
			}

			FgdClass? fgdClass = _fgdClasses.Where(x => entity.GetValue("classname") == x.ClassName).FirstOrDefault();
			if(fgdClass is null)
			{
				Program.ShowWarning($"could not find fgd entry for bsp entity class \"{entity.GetValue("classname")}\"");
				continue;
			}

			foreach(string key in fgdClass.DefaultKeyValues.Keys)
			{
				if(!entity.KeyValues.ContainsKey(key))
				{
					continue;
				}

				if(entity.GetValue(key) != fgdClass.DefaultKeyValues[key])
				{
					continue;
				}

				stripCount++;
				entity.RemoveKey(key);
			}
		}

		return entities;
	}
}
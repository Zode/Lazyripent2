using Lazyripent2.Lexer;

namespace Lazyripent2.Rule;

public class RuleFile
{
	private static readonly Dictionary<RuleBlockType, List<string>> _disallowedKeywordMap = new(){
        {RuleBlockType.Normal, []},
        {RuleBlockType.NewEntity, ["replace", "remove"]},
        {RuleBlockType.RemoveEntity, ["replace", "remove", "new", "bit-set", "bit-clear"]},
	};
	private List<RuleBlock> _ruleBlocks = [];
	private Dictionary<string, string> _store = [];

	/// <summary>
	/// 
	/// </summary>
	/// <param name="fullPath"></param>
	/// <exception cref="FileFormatException"></exception>
	public void DeserializeFromFile(string fullPath)
	{
		if(Options.Verbose)
		{
			Console.WriteLine($"Opening rule file for read: \"{fullPath}\"");
		}

		using StreamReader reader = File.OpenText(fullPath);
		string source = reader.ReadToEnd();
		if(string.IsNullOrWhiteSpace(source))
		{
			throw new FileFormatException($"File contents were empty: \"{fullPath}\"");
		}
		
		_ruleBlocks = [];
		ParseRuleBlocks(source);
		if(_ruleBlocks.Count == 0)
		{
			throw new FileFormatException($"Read rule blocks, and got 0 blocks: \"{fullPath}\"");
		}

		if(Options.Verbose)
		{
			Console.WriteLine($"Rule read OK: \"{fullPath}\"");
		}
	}

	/// <summary>
	/// only used for testing
	/// </summary>
	/// <param name="source"></param>
	/// <exception cref="FileFormatException"></exception>
	public void DeserializeFromMemory(string source)
	{
		if(Options.Verbose)
		{
			Console.WriteLine($"Reading rule file from memory");
		}

		if(string.IsNullOrWhiteSpace(source))
		{
			throw new FileFormatException($"Given memory contents were empty");
		}
		
		_ruleBlocks = [];
		ParseRuleBlocks(source);
		if(_ruleBlocks.Count == 0)
		{
			throw new FileFormatException($"Read rule blocks, and got 0 blocks");
		}

		if(Options.Verbose)
		{
			Console.WriteLine($"Rule read OK");
		}
	}

	private void ParseRuleBlocks(string source)
	{
		if(Options.Verbose)
		{
			Console.WriteLine($"Parsing rule blocks");
		}

		FileParser fileParser = new(source);
		while(!fileParser.IsAtEnd())
		{
			RuleBlock ruleBlock = new(this);
			if(fileParser.GetTokenType() == TokenType.Identifier)
			{
				ReadPreBlockIdentifiers(ref fileParser, ref ruleBlock);
			}
			
			ruleBlock.Line = fileParser.GetToken().Line;
			ruleBlock.StartIndex = fileParser.GetToken().StartIndex;
			ReadBlockContents(ref fileParser, ref ruleBlock);
			_ruleBlocks.Add(ruleBlock);
		}

		ValidateRuleBlocks(source);
		if(Options.Verbose)
		{
			Console.WriteLine($"Rule block parse OK");
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="fileParser"></param>
	/// <param name="ruleBlock"></param>
	/// <exception cref="FileFormatException"></exception>
	private static void ReadPreBlockIdentifiers(ref FileParser fileParser, ref RuleBlock ruleBlock)
	{
		//we really only have one identifier here for now:
		//map filename or map filename filename ... filename
		string keyword = fileParser.ReadIdentifier().ToLower();
		if(keyword != "map")
		{
			throw new FileFormatException($"Unknown preblock keyword \"{keyword}\" on line {fileParser.GetToken().Line}", fileParser);
		}

		while(fileParser.GetTokenType() != TokenType.LeftBrace)
		{
			ruleBlock.MapFilters.Add(ReadIdentifierOrStringOrNumber(ref fileParser));
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="fileParser"></param>
	/// <param name="ruleBlock"></param>
	/// <exception cref="FileFormatException"></exception>
	/// <exception cref="NotImplementedException"></exception>
	private static void ReadBlockContents(ref FileParser fileParser, ref RuleBlock ruleBlock)
	{
		fileParser.ExpectToken(TokenType.LeftBrace);

		//prescan
		int previousTokenIndex = fileParser.TokenIndex;
		RuleBlockType ruleBlockType = RuleBlockType.Normal;
		string hitKeyword = string.Empty;
		while(fileParser.GetTokenType() != TokenType.RightBrace)
		{
			if(fileParser.GetTokenType() != TokenType.Identifier)
			{
				fileParser.AdvanceToken();
				continue;
			}

			switch(fileParser.ReadIdentifier().ToLower())
			{
				case "new-entity":
					ruleBlockType = RuleBlockType.NewEntity;
					hitKeyword = "new-entity";
					break;

				case "remove-entity":
					hitKeyword = "remove-entity";
					ruleBlockType = RuleBlockType.RemoveEntity;
					break;
			}
		}

		//validate restrictions
		fileParser.SetTokenIndex(previousTokenIndex);
		while(fileParser.GetTokenType() != TokenType.RightBrace)
		{
			if(fileParser.GetTokenType() != TokenType.Identifier)
			{
				fileParser.AdvanceToken();
				continue;
			}

			string keyword = fileParser.ReadIdentifier().ToLower();
			if(_disallowedKeywordMap[ruleBlockType].Contains(keyword))
			{
				throw new FileFormatException($"Keyword \"{keyword}\" on line {fileParser.GetToken().Line} not allowed when rule block contains the keyword \"{hitKeyword}\"", fileParser);
			}
		}

		ruleBlock.BlockType = ruleBlockType;
		//actually start reading contents now
		fileParser.SetTokenIndex(previousTokenIndex);
		while(fileParser.GetTokenType() != TokenType.RightBrace)
		{
			string keyword = fileParser.ReadIdentifier().ToLower();
			foreach(RuleSelectorType selector in Enum.GetValues(typeof(RuleSelectorType)))
			{
				if(selector.GetKeyword() != keyword)
				{
					continue;
				}

				switch(selector.GetConsumeCount())
				{
					case 2:
						ruleBlock.Selectors.Add(new(ruleBlock, selector, ReadIdentifierOrStringOrNumber(ref fileParser), ReadIdentifierOrStringOrNumber(ref fileParser)));
						goto Handled;

					case 1:
						ruleBlock.Selectors.Add(new(ruleBlock, selector, ReadIdentifierOrStringOrNumber(ref fileParser)));
						goto Handled;

					default:
						throw new NotImplementedException($"Unsupported ConsumeCount ({selector.GetConsumeCount()}) for RuleSelectorType {selector}");
				}
			}

			foreach(RuleActionType action in Enum.GetValues(typeof(RuleActionType)))
			{
				if(action.GetKeyword() != keyword)
				{
					continue;
				}

				switch(action.GetConsumeCount())
				{
					case 2:
						ruleBlock.Actions.Add(new(ruleBlock, action, ReadIdentifierOrStringOrNumber(ref fileParser), ReadIdentifierOrStringOrNumber(ref fileParser)));
						goto Handled;

					case 1:
						ruleBlock.Actions.Add(new(ruleBlock, action, ReadIdentifierOrStringOrNumber(ref fileParser)));
						goto Handled;

					case 0:
						ruleBlock.Actions.Add(new(ruleBlock, action));
						goto Handled;

					default:
						throw new NotImplementedException($"Unsupported ConsumeCount ({action.GetConsumeCount()}) for RuleActionType {action}");
				}
			}

			throw new FileFormatException($"Unknown keyword \"{keyword}\" on line {fileParser.GetToken().Line}", fileParser);
			Handled:
			; // reloop
		}

		fileParser.AdvanceToken(); //consume right brace
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="fileParser"></param>
	/// <returns></returns>
	/// <exception cref="FileFormatException"></exception>
	private static string ReadIdentifierOrStringOrNumber(ref FileParser fileParser)
	{
		TokenType type = fileParser.GetTokenType();
        return type switch
        {
            TokenType.Minus or TokenType.Number => fileParser.ReadFloat().ToString(),
            TokenType.Identifier => fileParser.ReadIdentifier(),
            TokenType.String => fileParser.ReadString(),
            _ => throw new FileFormatException($"Unexpected {type} on line {fileParser.GetToken().Line}", fileParser),
        };
    }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="source"></param>
	/// <exception cref="RuleBlockException"></exception>
	/// <exception cref="NotImplementedException"></exception>
	private void ValidateRuleBlocks(string source)
	{
		foreach(RuleBlock ruleBlock in _ruleBlocks)
		{
			switch(ruleBlock.BlockType)
			{
				case RuleBlockType.Normal:
					if(ruleBlock.Actions.Count == 0 || ruleBlock.Selectors.Count == 0)
					{
						throw new RuleBlockException($"Rule block starting on line {ruleBlock.Line} must contain at least one action and one selector", ruleBlock.StartIndex, source);
					}

					break;

				case RuleBlockType.NewEntity:
					break;

				case RuleBlockType.RemoveEntity:
					if(ruleBlock.Selectors.Count == 0)
					{
						throw new RuleBlockException($"Rule block starting on line {ruleBlock.Line} must contain at least one selector", ruleBlock.StartIndex, source);
					}

					break;

				default:
					throw new NotImplementedException($"Unsupported RuleBlockAllowedAction {ruleBlock.BlockType}");
			}
		}
	}

	public List<Entity> ApplyRules(string levelName, List<Entity> entities)
	{
		foreach(RuleBlock ruleBlock in _ruleBlocks)
		{
			//need to clear any discard state from previous iterations
			foreach(Entity entity in entities)
			{
				entity.Discarded = false;
			}

			List<Entity> newEntities = ruleBlock.ApplyBlock(levelName, ref entities);
			entities = [..entities, ..newEntities];

			for(int i = entities.Count - 1; i >= 0; i--)
			{
				if(!entities[i].MarkedForDeletion)
				{
					continue;
				}

				entities.RemoveAt(i);
			}
		}

		return entities;
	}

	public void StoreValue(string key, string value)
	{
		if(_store.ContainsKey(key))
		{
			_store[key] = value;
			return;
		}

		_store.Add(key, value);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="key"></param>
	/// <param name="value">string passed if key exists</param>
	/// <returns>true if key exists</returns>
	public bool TryGetStoreValue(string key, out string value)
	{
		if(_store.ContainsKey(key))
		{
			value = _store[key];
			return true;
		}

		value = string.Empty;
		return false;
	}
}
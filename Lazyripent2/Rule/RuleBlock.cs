using Lazyripent2.Lexer;
using Lazyripent2.Map;

namespace Lazyripent2.Rule;

public class RuleBlock(RuleFile RuleFile)
{
	public RuleFile RuleFile {get; private set;} = RuleFile;
	public int Line {get; set;} = 0;
	public int StartIndex {get; set;} = 0;
	public RuleBlockType BlockType {get; set;} = RuleBlockType.Normal;
	public List<string> MapFilters {get; set;} = [];
	public List<RuleSelector> Selectors {get; set;} = [];
	public List<RuleAction> Actions {get; set;} = [];

	/// <summary>
	/// 
	/// </summary>
	/// <param name="entities"></param>
	/// <returns></returns>
	/// <exception cref="RuleBlockException"></exception>
	public List<Entity> ApplyBlock(string levelName, ref List<Entity> entities)
	{
		if(MapFilters.Count > 0 && !MapFilters.Contains(levelName))
		{
			return [];
		}

		List<Entity> newEntities = [];
		int processed = 0;
		switch(BlockType)
		{
			case RuleBlockType.Normal:
			case RuleBlockType.RemoveEntity:
				foreach(RuleSelector selector in Selectors)
				{
					selector.Filter(ref entities);
				}

				foreach(RuleAction action in Actions)
				{
					processed += action.Process(ref entities);
				}

				break;

			case RuleBlockType.NewEntity:
				if(Selectors.Count == 0)
				{
					newEntities.Add(new());
					Entity entity = newEntities[^1];
					foreach(RuleAction action in Actions)
					{
						action.Process(ref entity);
						processed++;
					}

					newEntities[^1] = entity;
					break;
				}
				
				for(int i = 0; i < entities.Count; i++)
				{
					if(entities[i].Discarded)
					{
						continue;
					}

					Entity matchedEntity = entities[i];
					newEntities.Add(new());
					Entity entity = newEntities[^1];
					foreach(RuleAction action in Actions)
					{
						action.Process(ref entity, matchedEntity);
						processed++;
					}

					newEntities[^1] = entity;
				}

				break;

			default:
				throw new NotImplementedException($"Unsupported RuleBlockType {BlockType}");

		}

		if(Options.Verbose)
		{
			Console.WriteLine($"Applied rule block, processed {processed} entities");
		}

		return newEntities;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="input"></param>
	/// <param name="entity"></param>
	/// <returns></returns>
	/// <exception cref="RuleBlockException"></exception>
	public string SolveForOperations(string input, Entity entity, Entity? matchedEntity = null)
	{
		if(matchedEntity is not null)
		{
			entity = matchedEntity;
		}

		//this is a horrible horrible hacky solution, but it works for now
		LexerScanner lexer = new(input, true);
		Token token;
		int tokenIndex = 0;
		
		void AdvanceToken()
		{
			token = lexer.Tokens[tokenIndex++];
		}

		Token Peek()
		{
			return lexer.Tokens[tokenIndex];
		}

		void ExpectToken(TokenType tokenToExpect, bool advance = true)
		{
			while(token.Type == TokenType.Whitespace)
			{
				AdvanceToken();
			}

			if(token.Type != tokenToExpect)
			{
				throw new RuleBlockException($"Unexpected \"{token.Type}\", expected \"{tokenToExpect}\" in rule block starting on line {Line}");
			}

			if(advance)
			{
				AdvanceToken();
			}
		}

		AdvanceToken(); // load first token in
		List<string> presolve = [];
		while(token.Type != TokenType.NullTerminator)
		{
			//escape brace?
			if(token.Type == TokenType.BackwardsSlash && Peek().Type == TokenType.LeftBrace)
			{
				AdvanceToken();
				presolve.Add(token.GetLexeme(input));
				AdvanceToken();
			}
			else if(token.Type == TokenType.LeftBrace)
			{
				AdvanceToken();
				ExpectToken(TokenType.Identifier, false);
				string ident = token.GetLexeme(input);
				AdvanceToken();
				//check if its a global.something, as that is what we only support currently
				if(token.Type == TokenType.Dot)
				{
					if(ident != "global")
					{
						throw new RuleBlockException($"Complex value error: unknown main identifier \"{ident}\" in rule block starting on line {Line}");
					}

					AdvanceToken();
					ExpectToken(TokenType.Identifier, false);
					string subIdent = token.GetLexeme(input);

					if(!RuleFile.TryGetStoreValue(subIdent, out string storeValue))
					{
						throw new RuleBlockException($"Complex value error: use of undefined global variable \"{subIdent}\" in rule block starting on line {Line}");
					}

					presolve.Add(storeValue);
					AdvanceToken();
				}
				else
				{
					if(!entity.KeyValues.ContainsKey(ident))
					{
						throw new RuleBlockException($"Complex value error: use of undefined key \"{ident}\" in rule block starting on line {Line}");
					}

					presolve.Add(entity.GetValue(ident));
				}

				ExpectToken(TokenType.RightBrace, true);
			}
			else
			{
				//just add whatever
				presolve.Add(token.GetLexeme(input));
				AdvanceToken();
			}
		}

		if(presolve.Count == 0)
		{
			throw new RuleBlockException($"Complex value error: solve resuled in 0 results in rule block starting on line {Line}");
		}

		if(presolve.Count == 1)
		{
			return presolve[0];
		}

		return string.Concat(presolve);
	}
}
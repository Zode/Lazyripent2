namespace Lazyripent2.Rule;

public class RuleSelector(RuleBlock RuleBlock, RuleSelectorType Type, string Key, string? Value = null)
{
	private readonly RuleBlock _ruleBlock = RuleBlock;
	public RuleSelectorType Type {get; private set;} = Type;
	public string Key {get; private set;} = Key;
	public string? Value {get; private set;} = Value;

	/// <summary>
	/// Apply selectors to entities
	/// </summary>
	/// <param name="entities"></param>
	/// <exception cref="RuleBlockException"></exception>
	public void Filter(ref List<Entity> entities)
	{
		#pragma warning disable 8604
		switch(Type)
		{
			case RuleSelectorType.Match:
				EnsureValueIsNotNull();

				foreach(Entity entity in entities)
				{
					if(!entity.KeyValues.ContainsKey(Key))
					{
						entity.Discarded = true;
						continue;
					}

					if(entity.GetValue(Key) != _ruleBlock.SolveForOperations(Value, entity))
					{
						entity.Discarded = true;
						continue;
					}
				}

				break;

			case RuleSelectorType.DontMatch:
				EnsureValueIsNotNull();

				foreach(Entity entity in entities)
				{
					if(!entity.KeyValues.ContainsKey(Key))
					{
						entity.Discarded = true;
						continue;
					}

					if(entity.GetValue(Key) == _ruleBlock.SolveForOperations(Value, entity))
					{
						entity.Discarded = true;
						continue;
					}
				}

				break;

			case RuleSelectorType.Have:
				foreach(Entity entity in entities)
				{
					if(!entity.KeyValues.ContainsKey(Key))
					{
						entity.Discarded = true;
						continue;
					}
				}

				break;

			case RuleSelectorType.DontHave:
				foreach(Entity entity in entities)
				{
					if(entity.KeyValues.ContainsKey(Key))
					{
						entity.Discarded = true;
						continue;
					}
				}

				break;

			default:
				throw new RuleBlockException($"Unsupported RuleSelectorType {Type} in rule block starting on line {_ruleBlock.Line}");
		}
		#pragma warning restore 8604
	}

	/// <summary>
	/// 
	/// </summary>
	/// <exception cref="RuleBlockException"></exception>
	private void EnsureValueIsNotNull()
	{
		if(Value is null)
		{
			throw new RuleBlockException($"RuleSelectorType {Type} Value was null in rule block starting on line {_ruleBlock.Line}");
		}
	}
}

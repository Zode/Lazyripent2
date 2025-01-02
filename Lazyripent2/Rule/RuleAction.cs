namespace Lazyripent2.Rule;

public class RuleAction(RuleBlock ruleBlock, RuleActionType Type, string? Key = null, string? Value = null)
{
	private readonly RuleBlock _ruleBlock = ruleBlock;
	public RuleActionType Type {get; private set;} = Type;
	public string? Key {get; private set;} = Key;
	public string? Value {get; private set;} = Value;

	/// <summary>
	/// Apply actions to entities
	/// </summary>
	/// <param name="entities"></param>
	public int Process(ref List<Entity> entities)
	{
		int processed = 0;
		for(int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			if(entity.Discarded)
			{
				continue;
			}

			Process(ref entity);
			entities[i] = entity;
			processed++;
		}

		return processed;
	}

	/// <summary>
	/// Apply actions to an entity
	/// </summary>
	/// <param name="entity"></param>
	/// <param name="matchedEntity"></param>
	/// <exception cref="RuleBlockException"></exception>
	public void Process(ref Entity entity, Entity? matchedEntity = null)
	{
		#pragma warning disable 8604
		switch(Type)
		{
			//key value operations
			case RuleActionType.Replace:
				EnsureKeyIsNotNull();
				EnsureValueIsNotNull();
				EnsureEntityHasKey(entity, Key);
				entity.SetKeyValue(Key, _ruleBlock.SolveForOperations(Value, entity, matchedEntity));
				break;

			case RuleActionType.New:
				EnsureKeyIsNotNull();
				EnsureValueIsNotNull();
				EnsureEntityDoesNotHaveKey(entity, Key);
				entity.AddKeyValue(Key, _ruleBlock.SolveForOperations(Value, entity, matchedEntity));
				break;

			case RuleActionType.BitSet:
				EnsureKeyIsNotNull();
				EnsureValueIsNotNull();
				EnsureEntityHasKey(entity, Key);
				entity.SetKeyValue(Key, SolveForBitwise(entity.GetValue(Key), RuleActionBitwiseMode.Set));
				break;

			case RuleActionType.BitClear:
				EnsureKeyIsNotNull();
				EnsureValueIsNotNull();
				EnsureEntityHasKey(entity, Key);
				entity.SetKeyValue(Key, SolveForBitwise(entity.GetValue(Key), RuleActionBitwiseMode.Clear));
				break;

			//key key operations
			case RuleActionType.Rename:
				EnsureKeyIsNotNull();
				EnsureValueIsNotNull();
				EnsureEntityHasKey(entity, Key);
				EnsureEntityDoesNotHaveKey(entity, Value);
				string keyValue = entity.GetValue(Key);
				entity.RemoveKey(Key);
				entity.AddKeyValue(Value, keyValue);
				break;

			case RuleActionType.Store:
				EnsureKeyIsNotNull();
				EnsureValueIsNotNull();
				EnsureEntityHasKey(entity, Key);
				_ruleBlock.RuleFile.StoreValue(Value, entity.GetValue(Key));
				break;

			//key operations
			case RuleActionType.Remove:
				EnsureKeyIsNotNull();
				EnsureEntityHasKey(entity, Key);
				entity.RemoveKey(Key);
				break;

			case RuleActionType.NewEntity:
				EnsureKeyIsNotNull();
				entity.AddKeyValue("classname", Key);
				break;

			//as-is operations
			case RuleActionType.RemoveEntity:
				entity.MarkedForDeletion = true;
				break;

			//math operations
			case RuleActionType.MathAddKeyValue:
			{
				EnsureKeyIsNotNull();
				EnsureValueIsNotNull();
				EnsureEntityHasKey(entity, Key);
				float value = EnsureEntityValueIsNumeric(entity, entity.GetValue(Key));
				value += ConvertValueToNumeric(Value);
				entity.SetKeyValue(Key, value.ToString());
				break;
			}

			case RuleActionType.MathSubKeyValue:
			{
				EnsureKeyIsNotNull();
				EnsureValueIsNotNull();
				EnsureEntityHasKey(entity, Key);
				float value = EnsureEntityValueIsNumeric(entity, entity.GetValue(Key));
				value -= ConvertValueToNumeric(Value);
				entity.SetKeyValue(Key, value.ToString());
				break;
			}

			case RuleActionType.MathMultKeyValue:
			{
				EnsureKeyIsNotNull();
				EnsureValueIsNotNull();
				EnsureEntityHasKey(entity, Key);
				float value = EnsureEntityValueIsNumeric(entity, entity.GetValue(Key));
				value *= ConvertValueToNumeric(Value);
				entity.SetKeyValue(Key, value.ToString());
				break;
			}

			case RuleActionType.MathDivKeyValue:
			{
				EnsureKeyIsNotNull();
				EnsureValueIsNotNull();
				EnsureEntityHasKey(entity, Key);
				float value = EnsureEntityValueIsNumeric(entity, entity.GetValue(Key));
				value /= ConvertValueToNumeric(Value);
				entity.SetKeyValue(Key, value.ToString());
				break;
			}

			default:
				throw new RuleBlockException($"Unsupported RuleActionType {Type} in rule block starting on line {_ruleBlock.Line}");
		}

		#pragma warning restore 8604
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="existingValue"></param>
	/// <param name="bitwiseMode"></param>
	/// <returns></returns>
	/// <exception cref="RuleBlockException"></exception>
	/// <exception cref="NotImplementedException"></exception>
	private string SolveForBitwise(string existingValue, RuleActionBitwiseMode bitwiseMode)
	{
		if(Value is null)
		{
			throw new RuleBlockException($"RuleActionType {Type} Value was null in rule block starting on line {_ruleBlock.Line}");
		}

		if(!Value.StartsWith('b') || Value.Length != 7)
		{
			throw new RuleBlockException($"RuleActionType {Type} Value is not a bit value in rule block starting on line {_ruleBlock.Line}");
		}

		if(!int.TryParse(existingValue, out int existingValueAsInt))
		{
			throw new RuleBlockException($"RuleActionType {Type} Target value is not a bitfield in rule block starting on line {_ruleBlock.Line}");
		}

		string stripValue = Value[1..];
		for(int i = 0; i < 6; i++)
		{
			if(stripValue[i] != '0' && stripValue[i] != '1')
			{
				throw new RuleBlockException($"RuleActionType {Type} Value is not a bit value in rule block starting on line {_ruleBlock.Line}");
			}

			switch(bitwiseMode)
			{
				case RuleActionBitwiseMode.Set:
					if(stripValue[i] == '0')
					{
						continue;
					}

					existingValueAsInt |= 0x20 >> i;
					break;

				case RuleActionBitwiseMode.Clear:
					if(stripValue[i] == '0')
					{
						continue;
					}

					existingValueAsInt &= ~(0x20 >> i);
					break;

				default:
					throw new NotImplementedException($"Unsupported RuleActionBitwiseMode {bitwiseMode}");
			}
		}

		return existingValueAsInt.ToString();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <exception cref="RuleBlockException"></exception>
	private void EnsureKeyIsNotNull()
	{
		if(Key is null)
		{
			throw new RuleBlockException($"RuleActionType {Type} Key was null in rule block starting on line {_ruleBlock.Line}");
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <exception cref="RuleBlockException"></exception>
	private void EnsureValueIsNotNull()
	{
		if(Value is null)
		{
			throw new RuleBlockException($"RuleActionType {Type} Value was null in rule block starting on line {_ruleBlock.Line}");
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="entity"></param>
	/// <param name="key"></param>
	/// <exception cref="RuleBlockException"></exception>
	private void EnsureEntityHasKey(Entity entity, string key)
	{
		if(!entity.KeyValues.ContainsKey(key))
		{
			throw new RuleBlockException($"Missing key \"{Key}\" for matched entity in rule block starting on line {_ruleBlock.Line}");
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="entity"></param>
	/// <param name="key"></param>
	/// <exception cref="RuleBlockException"></exception>
	private void EnsureEntityDoesNotHaveKey(Entity entity, string key)
	{
		if(entity.KeyValues.ContainsKey(key))
		{
			throw new RuleBlockException($"Key \"{key}\" already exists for matched entity in rule block starting on line {_ruleBlock.Line}");
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="entity"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	/// <exception cref="RuleBlockException"></exception>
	private float EnsureEntityValueIsNumeric(Entity entity, string value)
	{
		if(float.TryParse(value, out float floatValue))
		{
			return floatValue;
		}

		if(int.TryParse(value, out int intValue))
		{
			return intValue;
		}

		throw new RuleBlockException($"Value \"{value}\" is not numeric for matched entity in rule block starting on line {_ruleBlock.Line}");
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	/// <exception cref="RuleBlockException"></exception>
	private float ConvertValueToNumeric(string value)
	{
		if(float.TryParse(value, out float floatValue))
		{
			return floatValue;
		}

		if(int.TryParse(value, out int intValue))
		{
			return intValue;
		}

		throw new RuleBlockException($"Value \"{value}\" is somehow not numeric for math operation for matched entity in rule block starting on line {_ruleBlock.Line}");
	}
}

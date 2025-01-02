namespace Lazyripent2.Rule;

public enum RuleActionType
{
	[RuleKeyword("replace", 2)] Replace,
	[RuleKeyword("new", 2)] New,
	[RuleKeyword("bit-set", 2)] BitSet,
	[RuleKeyword("bit-clear", 2)] BitClear,
	[RuleKeyword("rename", 2)] Rename,
	[RuleKeyword("store", 2)] Store,
 	[RuleKeyword("remove", 1)] Remove,
	[RuleKeyword("new-entity", 1 )] NewEntity,
	[RuleKeyword("remove-entity", 0)] RemoveEntity,
	[RuleKeyword("add", 2)] MathAddKeyValue,
	[RuleKeyword("sub", 2)] MathSubKeyValue,
	[RuleKeyword("mult", 2)] MathMultKeyValue,
	[RuleKeyword("div", 2)] MathDivKeyValue,
}


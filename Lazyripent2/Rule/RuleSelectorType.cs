namespace Lazyripent2.Rule;

public enum RuleSelectorType
{
	[RuleKeyword("match", 2)] Match,
	[RuleKeyword("dont-match", 2)] DontMatch,
	[RuleKeyword("have", 1)] Have,
	[RuleKeyword("dont-have", 1)] DontHave,
}
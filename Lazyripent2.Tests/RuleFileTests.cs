using Lazyripent2.Rule;

namespace Lazyripent2.Tests;

[TestFixture]
public class RuleFileTests
{
	[Test]
	public void InvalidBlock_Empty()
	{
		Assert.Catch(() => {
			RuleFile ruleFile = new();
			ruleFile.DeserializeFromMemory(string.Empty);
		});
	}

	[Test]
	public void InvalidBlock_AllTypes_Empty()
	{
		Assert.Catch(() => {
			RuleFile ruleFile = new();
			ruleFile.DeserializeFromMemory(@"{
			}");
		});
	}

	[Test]
	public void InvalidBlock_AllTypes_Empty_WithMap()
	{
		Assert.Catch(() => {
			RuleFile ruleFile = new();
			ruleFile.DeserializeFromMemory(@"map imaginary
			{
			}");
		});
	}

	[Test]
	public void InvalidBlock_AllTypes_Empty_WithMaps()
	{
		Assert.Catch(() => {
			RuleFile ruleFile = new();
			ruleFile.DeserializeFromMemory(@"map imaginary imaginary imaginary
			{
			}");
		});
	}

	[Test]
	public void InvalidBlock_AllTypes_OnlySelectors()
	{
		Assert.Catch(() => {
			RuleFile ruleFile = new();
			ruleFile.DeserializeFromMemory(@"{
				match imaginary imaginary
			}");
		});
	}

	[Test]
	public void InvalidBlock_AllTypes_OnlyActions()
	{
		Assert.Catch(() => {
			RuleFile ruleFile = new();
			ruleFile.DeserializeFromMemory(@"{
				new imaginary imaginary
			}");
		});
	}

	[Test]
	public void ValidBlock_Normal()
	{
		Assert.DoesNotThrow(() => {
			RuleFile ruleFile = new();
			ruleFile.DeserializeFromMemory(@"{
				match imaginary imaginary
				new imaginary imaginary
				replace imaginary imaginary
				remove imaginary
			}");
		});
	}

	[Test]
	public void ValidBlock_Normal_WithMap()
	{
		Assert.DoesNotThrow(() => {
			RuleFile ruleFile = new();
			ruleFile.DeserializeFromMemory(@"map imaginary
			{
				match imaginary imaginary
				new imaginary imaginary
				replace imaginary imaginary
				remove imaginary
			}");
		});
	}

	[Test]
	public void ValidBlock_Normal_WithMaps()
	{
		Assert.DoesNotThrow(() => {
			RuleFile ruleFile = new();
			ruleFile.DeserializeFromMemory(@"map imaginary imaginary imaginary
			{
				match imaginary imaginary
				new imaginary imaginary
				replace imaginary imaginary
				remove imaginary
			}");
		});
	}

	[Test]
	public void ValidBlock_NewEntity()
	{
		Assert.DoesNotThrow(() => {
			RuleFile ruleFile = new();
			ruleFile.DeserializeFromMemory(@"{
				new-entity imaginary
				new imaginary imaginary
			}");
		});
	}

	[Test]
	public void ValidBlock_NewEntity_WithMatch()
	{
		Assert.DoesNotThrow(() => {
			RuleFile ruleFile = new();
			ruleFile.DeserializeFromMemory(@"{
				match imaginary imaginary
				new-entity imaginary
				new imaginary imaginary
			}");
		});
	}

	[Test]
	public void InvalidBlock_NewEntity()
	{
		Assert.Catch(() => {
			RuleFile ruleFile = new();
			ruleFile.DeserializeFromMemory(@"{
				new-entity
				replace imaginary imaginary
				remove imaginary
			}");
		});
	}

	[Test]
	public void ValidBlock_RemoveEntity()
	{
		Assert.DoesNotThrow(() => {
			RuleFile ruleFile = new();
			ruleFile.DeserializeFromMemory(@"{
				match imaginary imaginary
				remove-entity
			}");
		});
	}

	[Test]
	public void InvalidBlock_RemoveEntity_NotAllowedKeywords()
	{
		Assert.Catch(() => {
			RuleFile ruleFile = new();
			ruleFile.DeserializeFromMemory(@"{
				match imaginary imaginary
				remove-entity
				new imaginary imaginary
				replace imaginary imaginary
				remove imaginary
			}");
		});
	}

	[Test]
	public void InvalidBlock_RemoveEntity_NoSelectors()
	{
		Assert.Catch(() => {
			RuleFile ruleFile = new();
			ruleFile.DeserializeFromMemory(@"{
				remove-entity
			}");
		});
	}
}
using Lazyripent2.Map;
using Lazyripent2.Rule;

namespace Lazyripent2.Tests;

[TestFixture]
public class RuleProcessingTests
{
	[Test]
	public void Unknown_Keyword()
	{
		Assert.Catch<FileFormatException>(() => {
			RuleFile ruleFile = new();
			ruleFile.DeserializeFromMemory(@"
			{
				motch classname test_ignore
			}");
		});
	}

	[Test]
	public void Basic_Match_Replace()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""classname"" ""test_target""
			""target"" ""aaaa"" 
			""target2"" ""200""
		}
		{
			""classname"" ""test_ignore""
			""target"" ""aaaa""
			""target2"" ""200""
		}
		");

		RuleFile ruleFile = new();
		ruleFile.DeserializeFromMemory(@"
		{
			match classname test_target
			replace target bbbb
			replace target2 100
		}");

		List<Entity> entities = ruleFile.ApplyRules("", mapFile.GetEntities());
		
		Assert.Multiple(() =>
		{
			Assert.That(entities[0].GetValue("target"), Is.EqualTo("bbbb"));
			Assert.That(entities[0].GetValue("target2"), Is.EqualTo("100"));
			Assert.That(entities[1].GetValue("target"), Is.EqualTo("aaaa"));
			Assert.That(entities[1].GetValue("target2"), Is.EqualTo("200"));
		});
	}

	[Test]
	public void Basic_DontMatch_Replace()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""classname"" ""test_target""
			""target"" ""aaaa"" 
			""target2"" ""200""
		}
		{
			""classname"" ""test_ignore""
			""target"" ""aaaa""
			""target2"" ""200""
		}
		");

		RuleFile ruleFile = new();
		ruleFile.DeserializeFromMemory(@"
		{
			dont-match classname test_target
			replace target bbbb
			replace target2 100
		}");

		List<Entity> entities = ruleFile.ApplyRules("", mapFile.GetEntities());
		
		Assert.Multiple(() =>
		{
			Assert.That(entities[0].GetValue("target"), Is.EqualTo("aaaa"));
			Assert.That(entities[0].GetValue("target2"), Is.EqualTo("200"));
			Assert.That(entities[1].GetValue("target"), Is.EqualTo("bbbb"));
			Assert.That(entities[1].GetValue("target2"), Is.EqualTo("100"));
		});
	}

	[Test]
	public void Basic_Have_Replace()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""trigger"" ""cccc""
			""target"" ""aaaa"" 
			""target2"" ""200""
		}
		{
			""target"" ""aaaa""
			""target2"" ""200""
		}
		");

		RuleFile ruleFile = new();
		ruleFile.DeserializeFromMemory(@"
		{
			have trigger
			replace target bbbb
			replace target2 100
		}");

		List<Entity> entities = ruleFile.ApplyRules("", mapFile.GetEntities());
		
		Assert.Multiple(() =>
		{
			Assert.That(entities[0].GetValue("target"), Is.EqualTo("bbbb"));
			Assert.That(entities[0].GetValue("target2"), Is.EqualTo("100"));
			Assert.That(entities[1].GetValue("target"), Is.EqualTo("aaaa"));
			Assert.That(entities[1].GetValue("target2"), Is.EqualTo("200"));
		});
	}

	[Test]
	public void Basic_DontHave_Replace()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""trigger"" ""cccc""
			""target"" ""aaaa"" 
			""target2"" ""200""
		}
		{
			""target"" ""aaaa""
			""target2"" ""200""
		}
		");

		RuleFile ruleFile = new();
		ruleFile.DeserializeFromMemory(@"
		{
			dont-have trigger
			replace target bbbb
			replace target2 100
		}");

		List<Entity> entities = ruleFile.ApplyRules("", mapFile.GetEntities());
		
		Assert.Multiple(() =>
		{
			Assert.That(entities[0].GetValue("target"), Is.EqualTo("aaaa"));
			Assert.That(entities[0].GetValue("target2"), Is.EqualTo("200"));
			Assert.That(entities[1].GetValue("target"), Is.EqualTo("bbbb"));
			Assert.That(entities[1].GetValue("target2"), Is.EqualTo("100"));
		});
	}

	[Test]
	public void Basic_Remove()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""classname"" ""test_target""
			""target"" ""aaaa"" 
		}
		");

		RuleFile ruleFile = new();
		ruleFile.DeserializeFromMemory(@"
		{
			match classname test_target
			remove target
		}");

		List<Entity> entities = ruleFile.ApplyRules("", mapFile.GetEntities());
		
		Assert.Multiple(() =>
		{
			Assert.That(entities[0].KeyValues.ContainsKey("target"), Is.EqualTo(false));
		});
	}

	[Test]
	public void Basic_New()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""classname"" ""test_target""
		}
		");

		RuleFile ruleFile = new();
		ruleFile.DeserializeFromMemory(@"
		{
			match classname test_target
			new target aaaa
		}");

		List<Entity> entities = ruleFile.ApplyRules("", mapFile.GetEntities());
		
		Assert.Multiple(() =>
		{
			Assert.That(entities[0].KeyValues.ContainsKey("target"), Is.EqualTo(true));
			Assert.That(entities[0].GetValue("target"), Is.EqualTo("aaaa"));
		});
	}

	[Test]
	public void Basic_Rename()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""classname"" ""test_target""
			""traget"" ""aaaa""
		}
		");

		RuleFile ruleFile = new();
		ruleFile.DeserializeFromMemory(@"
		{
			match classname test_target
			rename traget target
		}");

		List<Entity> entities = ruleFile.ApplyRules("", mapFile.GetEntities());
		
		Assert.Multiple(() =>
		{
			Assert.That(entities[0].KeyValues.ContainsKey("traget"), Is.EqualTo(false));
			Assert.That(entities[0].KeyValues.ContainsKey("target"), Is.EqualTo(true));
			Assert.That(entities[0].GetValue("target"), Is.EqualTo("aaaa"));
		});
	}

	[Test]
	public void Basic_NewEntity()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""classname"" ""test_ignore""
		}
		");

		RuleFile ruleFile = new();
		ruleFile.DeserializeFromMemory(@"
		{
			new-entity test_target
			new target aaaa
		}");

		List<Entity> entities = ruleFile.ApplyRules("", mapFile.GetEntities());
		
		Assert.Multiple(() =>
		{
			Assert.That(entities, Has.Count.EqualTo(2));
			Assert.That(entities[1].KeyValues.ContainsKey("classname"), Is.EqualTo(true));
			Assert.That(entities[1].KeyValues.ContainsKey("target"), Is.EqualTo(true));
			Assert.That(entities[1].GetValue("classname"), Is.EqualTo("test_target"));
			Assert.That(entities[1].GetValue("target"), Is.EqualTo("aaaa"));
		});
	}

	[Test]
	public void Basic_Match_NewEntity()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""classname"" ""test_target""
			""target"" ""aaaa""
		}
		{
			""classname"" ""test_target""
			""target"" ""aaaa""
		}
		");

		RuleFile ruleFile = new();
		ruleFile.DeserializeFromMemory(@"
		{
			match classname test_target
			new-entity test_new
			new target ""{target}""
		}");

		List<Entity> entities = ruleFile.ApplyRules("", mapFile.GetEntities());
		
		Assert.Multiple(() =>
		{
			Assert.That(entities, Has.Count.EqualTo(4));
			Assert.That(entities[2].KeyValues.ContainsKey("classname"), Is.EqualTo(true));
			Assert.That(entities[2].KeyValues.ContainsKey("target"), Is.EqualTo(true));
			Assert.That(entities[2].GetValue("classname"), Is.EqualTo("test_new"));
			Assert.That(entities[2].GetValue("target"), Is.EqualTo("aaaa"));
			Assert.That(entities[3].KeyValues.ContainsKey("classname"), Is.EqualTo(true));
			Assert.That(entities[3].KeyValues.ContainsKey("target"), Is.EqualTo(true));
			Assert.That(entities[3].GetValue("classname"), Is.EqualTo("test_new"));
			Assert.That(entities[3].GetValue("target"), Is.EqualTo("aaaa"));
		});
	}

	[Test]
	public void Basic_RemoveEntity()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""classname"" ""test_target""
		}
		{
			""classname"" ""test_ignore""
		}
		");

		RuleFile ruleFile = new();
		ruleFile.DeserializeFromMemory(@"
		{
			match classname test_target
			remove-entity
		}");

		List<Entity> entities = ruleFile.ApplyRules("", mapFile.GetEntities());
		
		Assert.Multiple(() =>
		{
			Assert.That(entities, Has.Count.EqualTo(1));
			Assert.That(entities[0].KeyValues.ContainsKey("classname"), Is.EqualTo(true));
			Assert.That(entities[0].GetValue("classname"), Is.EqualTo("test_ignore"));
		});
	}

	[Test]
	public void BitSet()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""classname"" ""test_target""
			""bitfield"" ""1""
		}
		");

		RuleFile ruleFile = new();
		ruleFile.DeserializeFromMemory(@"
		{
			match classname test_target
			bit-set bitfield b000010
		}");

		List<Entity> entities = ruleFile.ApplyRules("", mapFile.GetEntities());
		
		Assert.That(entities[0].GetValue("bitfield"), Is.EqualTo("3"));
	}

	[Test]
	public void BitClear()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""classname"" ""test_target""
			""bitfield"" ""3""
		}
		");

		RuleFile ruleFile = new();
		ruleFile.DeserializeFromMemory(@"
		{
			match classname test_target
			bit-clear bitfield b000010
		}");

		List<Entity> entities = ruleFile.ApplyRules("", mapFile.GetEntities());
		
		Assert.That(entities[0].GetValue("bitfield"), Is.EqualTo("1"));
	}

	[Test]
	public void Store()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""classname"" ""test_target""
			""aaaas"" ""10""
		}
		");

		RuleFile ruleFile = new();
		ruleFile.DeserializeFromMemory(@"
		{
			match classname test_target
			store aaaas aaaas-aaaaed
		}");

		List<Entity> entities = ruleFile.ApplyRules("", mapFile.GetEntities());
		bool result = ruleFile.TryGetStoreValue("aaaas-aaaaed", out string stored);

		Assert.Multiple(() =>
		{
			Assert.That(result, Is.EqualTo(true));
			Assert.That(stored, Is.EqualTo("10"));
		});
	}

	[Test]
	public void Complex_Match_Replace()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""classname"" ""test_target""
			""target"" ""aaaa"" 
			""otherTarget"" ""bbbb""
		}
		");

		RuleFile ruleFile = new();
		ruleFile.DeserializeFromMemory(@"
		{
			match classname test_target
			replace target ""{otherTarget}""
		}");

		List<Entity> entities = ruleFile.ApplyRules("", mapFile.GetEntities());
		
		Assert.That(entities[0].GetValue("target"), Is.EqualTo("bbbb"));
	}

	[Test]
	public void Complex_Match_Replace_ThroughStore()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""classname"" ""test_source""
			""target"" ""aaaa""
		}
		{
			""classname"" ""test_target""
			""target"" ""bbbb"" 
		}
		");

		RuleFile ruleFile = new();
		ruleFile.DeserializeFromMemory(@"
		{
			match classname test_source
			store target source-target
		}
		{
			match classname test_target
			replace target ""{global.source-target}""
		}");

		List<Entity> entities = ruleFile.ApplyRules("", mapFile.GetEntities());
		
		Assert.Multiple(() =>
		{
			Assert.That(entities[0].GetValue("target"), Is.EqualTo("aaaa"));
			Assert.That(entities[1].GetValue("target"), Is.EqualTo("aaaa"));
		});
	}

	[Test]
	public void Complex_Match_Replace_StringConcat()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""classname"" ""test_target""
			""target"" ""aaaa"" 
			""otherTarget"" ""bbbb""
		}
		");

		RuleFile ruleFile = new();
		ruleFile.DeserializeFromMemory(@"
		{
			match classname test_target
			replace target ""cccc {otherTarget}!""
		}");

		List<Entity> entities = ruleFile.ApplyRules("", mapFile.GetEntities());
		
		Assert.That(entities[0].GetValue("target"), Is.EqualTo("cccc bbbb!"));
	}

	[Test]
	public void Complex_Match_Replace_EscapeBrace()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""classname"" ""test_target""
			""target"" ""aaaa"" 
			""otherTarget"" ""bbbb""
		}
		");

		RuleFile ruleFile = new();
		ruleFile.DeserializeFromMemory(@"
		{
			match classname test_target
			replace target ""cccc \{otherTarget}!""
		}");

		List<Entity> entities = ruleFile.ApplyRules("", mapFile.GetEntities());
		
		Assert.That(entities[0].GetValue("target"), Is.EqualTo("cccc {otherTarget}!"));
	}

	[Test]
	public void Complex_Match_Replace_EscapeBrace2()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""classname"" ""test_target""
			""target"" ""aaaa"" 
			""otherTarget"" ""bbbb""
		}
		");

		RuleFile ruleFile = new();
		ruleFile.DeserializeFromMemory(@"
		{
			match classname test_target
			replace target ""cccc \\{otherTarget}!""
		}");

		List<Entity> entities = ruleFile.ApplyRules("", mapFile.GetEntities());
		
		Assert.That(entities[0].GetValue("target"), Is.EqualTo(@"cccc \{otherTarget}!"));
	}

	[Test]
	public void Basic_Match_MathOperations_Int()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""classname"" ""test_target""
			""target"" ""aaaa"" 
			""target2"" ""aaaa"" 
			""target3"" ""aaaa"" 
			""target4"" ""aaaa"" 
			""health"" ""100""
		}
		");

		RuleFile ruleFile = new();
		ruleFile.DeserializeFromMemory(@"
		{
			match classname test_target
			add health 10
			replace target ""{health}""
			replace health 100

			sub health 10
			replace target2 ""{health}""
			replace health 100

			mult health 2
			replace target3 ""{health}""
			replace health 100

			div health 2
			replace target4 ""{health}""
		}");

		List<Entity> entities = ruleFile.ApplyRules("", mapFile.GetEntities());
		
		Assert.Multiple(() =>
		{
			Assert.That(entities[0].GetValue("target"), Is.EqualTo("110"));
			Assert.That(entities[0].GetValue("target2"), Is.EqualTo("90"));
			Assert.That(entities[0].GetValue("target3"), Is.EqualTo("200"));
			Assert.That(entities[0].GetValue("target4"), Is.EqualTo("50"));
		});
	}

	[Test]
	public void Basic_Match_MathOperations_Float()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""classname"" ""test_target""
			""target"" ""aaaa"" 
			""target2"" ""aaaa"" 
			""target3"" ""aaaa"" 
			""target4"" ""aaaa"" 
			""health"" ""100.50""
		}
		");

		RuleFile ruleFile = new();
		ruleFile.DeserializeFromMemory(@"
		{
			match classname test_target
			add health 10.1
			replace target ""{health}""
			replace health 100.50

			sub health 10.1
			replace target2 ""{health}""
			replace health 100.50

			mult health 1.25
			replace target3 ""{health}""
			replace health 100.50

			div health 1.25
			replace target4 ""{health}""
		}");

		List<Entity> entities = ruleFile.ApplyRules("", mapFile.GetEntities());
		
		Assert.Multiple(() =>
		{
			Assert.That(entities[0].GetValue("target"), Is.EqualTo("110.6"));
			Assert.That(entities[0].GetValue("target2"), Is.EqualTo("90.4"));
			Assert.That(entities[0].GetValue("target3"), Is.EqualTo("125.625"));
			Assert.That(entities[0].GetValue("target4"), Is.EqualTo("80.4"));
		});
	}

	[Test]
	public void Basic_Match_MathOperations_MixedIntFloat()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""classname"" ""test_target""
			""target"" ""aaaa"" 
			""target2"" ""aaaa"" 
			""target3"" ""aaaa"" 
			""target4"" ""aaaa"" 
			""health"" ""100""
		}
		");

		RuleFile ruleFile = new();
		ruleFile.DeserializeFromMemory(@"
		{
			match classname test_target
			add health 50.50
			replace target ""{health}""
			replace health 100

			sub health 50.50
			replace target2 ""{health}""
			replace health 100

			mult health 2.5
			replace target3 ""{health}""
			replace health 100

			div health 2.5
			replace target4 ""{health}""
		}");

		List<Entity> entities = ruleFile.ApplyRules("", mapFile.GetEntities());
		
		Assert.Multiple(() =>
		{
			Assert.That(entities[0].GetValue("target"), Is.EqualTo("150.5"));
			Assert.That(entities[0].GetValue("target2"), Is.EqualTo("49.5"));
			Assert.That(entities[0].GetValue("target3"), Is.EqualTo("250"));
			Assert.That(entities[0].GetValue("target4"), Is.EqualTo("40"));
		});
	}

	[Test]
	public void Basic_Match_MathOperations_String()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""classname"" ""test_target""
			""target"" ""aaaa"" 
			""target2"" ""aaaa"" 
			""target3"" ""aaaa"" 
			""target4"" ""aaaa"" 
			""health"" ""aaaa""
		}
		");

		RuleFile ruleFile = new();
		ruleFile.DeserializeFromMemory(@"
		{
			match classname test_target
			add health 10
			replace target ""{health}""
			replace health 100

			sub health 10
			replace target2 ""{health}""
			replace health 100

			mult health 2
			replace target3 ""{health}""
			replace health 100

			div health 2
			replace target4 ""{health}""
		}");

		Assert.Catch<RuleBlockException>(() => {
			List<Entity> entities = ruleFile.ApplyRules("", mapFile.GetEntities());
		});
	}

	[Test]
	public void MapFilter()
	{
		MapFile mapFile = new();
		mapFile.DeserializeFromMemory(@"
		{
			""classname"" ""test_target""
		}
		");

		RuleFile ruleFile = new();
		ruleFile.DeserializeFromMemory(@"
		map a c
		{
			match classname test_target
			new target aaaa
		}");

		List<Entity> entities_map_a = ruleFile.ApplyRules("a", mapFile.GetEntities());

		mapFile.DeserializeFromMemory(@"
		{
			""classname"" ""test_target""
		}
		");

		List<Entity> entities_map_b = ruleFile.ApplyRules("b", mapFile.GetEntities());

		mapFile.DeserializeFromMemory(@"
		{
			""classname"" ""test_target""
		}
		");

		List<Entity> entities_map_c = ruleFile.ApplyRules("c", mapFile.GetEntities());
		
		Assert.Multiple(() =>
		{
			Assert.That(entities_map_a[0].KeyValues.ContainsKey("target"), Is.EqualTo(true));
			Assert.That(entities_map_a[0].GetValue("target"), Is.EqualTo("aaaa"));
			Assert.That(entities_map_b[0].KeyValues.ContainsKey("target"), Is.EqualTo(false));
			Assert.That(entities_map_c[0].KeyValues.ContainsKey("target"), Is.EqualTo(true));
			Assert.That(entities_map_c[0].GetValue("target"), Is.EqualTo("aaaa"));
		});
	}
}
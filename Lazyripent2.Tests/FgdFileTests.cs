using System.Text;
using Lazyripent2.Fgd;

namespace Lazyripent2.Tests;

[TestFixture]
public class FgdFileTests
{
	[Test]
	public void ValidFgd_Class_Minimal()
	{
		Assert.DoesNotThrow(() => {
			FgdFile fgdFile = new();
			fgdFile.DeserializeFromMemory(@"
			@SolidClass = imaginary
			[

			]
			");
		});
	}

	[Test]
	public void ValidFgd_Class_Tooltip()
	{
		Assert.DoesNotThrow(() => {
			FgdFile fgdFile = new();
			fgdFile.DeserializeFromMemory(@"
			@SolidClass = imaginary : ""Imaginary""
			[

			]
			");
		});
	}

	[Test]
	public void ValidFgd_Class_Size()
	{
		Assert.DoesNotThrow(() => {
			FgdFile fgdFile = new();
			fgdFile.DeserializeFromMemory(@"
			@SolidClass size(0 0 0) = imaginary
			[

			]
			");
		});
	}

	[Test]
	public void ValidFgd_Class_Size2()
	{
		Assert.DoesNotThrow(() => {
			FgdFile fgdFile = new();
			fgdFile.DeserializeFromMemory(@"
			@SolidClass size(0 0 0, 0 0 0) = imaginary
			[

			]
			");
		});
	}

	[Test]
	public void ValidFgd_Class_Color()
	{
		Assert.DoesNotThrow(() => {
			FgdFile fgdFile = new();
			fgdFile.DeserializeFromMemory(@"
			@SolidClass color(0 0 0) = imaginary
			[

			]
			");
		});
	}

	[Test]
	[TestCase("iconsprite")]
	[TestCase("studio")]
	[TestCase("sprite")]
	[TestCase("decal")]
	public void ValidFgd_Class_Renderer_Empty(string renderType)
	{
		Assert.DoesNotThrow(() => {
			FgdFile fgdFile = new();
			fgdFile.DeserializeFromMemory($@"
			@SolidClass {renderType}() = imaginary
			[

			]
			");
		});
	}

	[Test]
	[TestCase("iconsprite")]
	[TestCase("studio")]
	[TestCase("sprite")]
	[TestCase("decal")]
	public void ValidFgd_Class_Renderer(string renderType)
	{
		Assert.DoesNotThrow(() => {
			FgdFile fgdFile = new();
			fgdFile.DeserializeFromMemory($@"
			@SolidClass {renderType}(""imaginary"") = imaginary
			[

			]
			");
		});
	}

	[Test]
	public void ValidFgd_Class_Multiple()
	{
		Assert.DoesNotThrow(() => {
			FgdFile fgdFile = new();
			fgdFile.DeserializeFromMemory(@"
			@SolidClass size(0 0 0) color(0 0 0) studio() = imaginary
			[

			]
			");
		});
	}

	[Test]
	[TestCase(1)]
	[TestCase(10)]
	public void ValidFgd_KeyValues_String_Empty(int count)
	{
		StringBuilder sb = new();
		for(int i = 0; i < count; i++)
		{
			sb.AppendLine($@"imaginary{i}(string) : ""Imaginary""");
		}


		Assert.DoesNotThrow(() => {
			FgdFile fgdFile = new();
			fgdFile.DeserializeFromMemory($@"
			@SolidClass = imaginary
			[
				{sb.ToString()}
			]
			");
		});
	}

	[Test]
	[TestCase(1)]
	[TestCase(10)]
	public void ValidFgd_KeyValues_String_Default(int count)
	{
		StringBuilder sb = new();
		for(int i = 0; i < count; i++)
		{
			sb.AppendLine($@"imaginary{i}(string) : ""Imaginary"" : ""imaginary""");
		}


		Assert.DoesNotThrow(() => {
			FgdFile fgdFile = new();
			fgdFile.DeserializeFromMemory($@"
			@SolidClass = imaginary
			[
				{sb.ToString()}
			]
			");
		});
	}

	[Test]
	[TestCase(1)]
	[TestCase(10)]
	public void ValidFgd_KeyValues_Integer_Empty(int count)
	{
		StringBuilder sb = new();
		for(int i = 0; i < count; i++)
		{
			sb.AppendLine($@"imaginary{i}(integer) : ""Imaginary""");
		}


		Assert.DoesNotThrow(() => {
			FgdFile fgdFile = new();
			fgdFile.DeserializeFromMemory($@"
			@SolidClass = imaginary
			[
				{sb.ToString()}
			]
			");
		});
	}

	[Test]
	[TestCase(1)]
	[TestCase(10)]
	public void ValidFgd_KeyValues_Integer_Default(int count)
	{
		StringBuilder sb = new();
		for(int i = 0; i < count; i++)
		{
			sb.AppendLine($@"imaginary{i}(integer) : ""Imaginary"" : 0");
		}


		Assert.DoesNotThrow(() => {
			FgdFile fgdFile = new();
			fgdFile.DeserializeFromMemory($@"
			@SolidClass = imaginary
			[
				{sb.ToString()}
			]
			");
		});
	}

	[Test]
	[TestCase(1, 1)]
	[TestCase(1, 10)]
	[TestCase(10, 1)]
	[TestCase(10, 10)]
	public void ValidFgd_KeyValues_Choices(int count, int choicesCount)
	{
		StringBuilder sb = new();
		for(int i = 0; i < count; i++)
		{
			sb.AppendLine($@"imaginary{i}(choices) : ""Imaginary"" : 0 =");
			sb.AppendLine(@"[");
			for(int j = 0; j < choicesCount; j++)
			{
				sb.AppendLine($@"	{j} : ""imaginary""");
			}
			sb.AppendLine(@"]");
		}


		Assert.DoesNotThrow(() => {
			FgdFile fgdFile = new();
			fgdFile.DeserializeFromMemory($@"
			@SolidClass = imaginary
			[
				{sb.ToString()}
			]
			");
		});
	}

	[Test]
	[TestCase(1, 1)]
	[TestCase(1, 10)]
	[TestCase(10, 1)]
	[TestCase(10, 10)]
	public void ValidFgd_Flags_Choices(int count, int flagsCount)
	{
		StringBuilder sb = new();
		for(int i = 0; i < count; i++)
		{
			sb.AppendLine($@"imaginary{i}(flags) =");
			sb.AppendLine(@"[");
			for(int j = 0; j < flagsCount; j++)
			{
				sb.AppendLine($@"	{2 << j} : ""imaginary"" : 0");
			}
			sb.AppendLine(@"]");
		}


		Assert.DoesNotThrow(() => {
			FgdFile fgdFile = new();
			fgdFile.DeserializeFromMemory($@"
			@SolidClass = imaginary
			[
				{sb.ToString()}
			]
			");
		});
	}

	[Test]
	public void ValidFgd_Class_SingleInheritance()
	{
		Assert.DoesNotThrow(() => {
			FgdFile fgdFile = new();
			fgdFile.DeserializeFromMemory(@"
			@SolidClass = imaginary
			[

			]

			@SolidClass base(imaginary) = imaginary2
			[

			] 
			");
		});
	}

	[Test]
	public void ValidFgd_Class_MultipleInheritance()
	{
		Assert.DoesNotThrow(() => {
			FgdFile fgdFile = new();
			fgdFile.DeserializeFromMemory(@"
			@SolidClass = imaginary
			[

			]

			@SolidClass = imaginary2
			[

			]

			@SolidClass = imaginary3
			[

			]

			@SolidClass = imaginary4
			[

			]

			@SolidClass = imaginary5
			[

			]

			@SolidClass base(imaginary imaginary2 imaginary3 imaginary4 imaginary5) = imaginaries
			[
			
			] 
			");
		});
	}

	[Test]
	public void ValidFgd_Class_ComplexInheritance()
	{
		Assert.DoesNotThrow(() => {
			FgdFile fgdFile = new();
			fgdFile.DeserializeFromMemory(@"
			@SolidClass = imaginary
			[

			]

			@SolidClass base(imaginary) = imaginary2
			[

			]

			@SolidClass base(imaginary imaginary2) = imaginary3
			[

			]

			@SolidClass base(imaginary imaginary2) = imaginary4
			[

			]

			@SolidClass base(imaginary4) = imaginary5
			[

			]

			@SolidClass base(imaginary3 imaginary4 imaginary5) = imaginaries
			[
			
			] 
			");
		});
	}
}
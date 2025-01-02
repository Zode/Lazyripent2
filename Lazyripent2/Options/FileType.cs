namespace Lazyripent2;

public enum FileType
{
	[FileTypeExtension(".rule")] Rule,
	[FileTypeExtension(".fgd")] Fgd,
	[FileTypeExtension(".map")] Map,
	[FileTypeExtension(".ent")] Ent,
	[FileTypeExtension(".bsp")] Bsp,
}

[System.AttributeUsage(System.AttributeTargets.Field)]
public class FileTypeExtensionAttribute(string extension) : System.Attribute
{
	public string Extension {get; private set;} = extension;
}

public static class FileTypeExtensions
{
	public static string GetExtension(this FileType value)
	{
		System.Reflection.FieldInfo? fieldInfo = value.GetType()?.GetField(value.ToString());
		if(fieldInfo is null)
		{
			return string.Empty;
		}

		FileTypeExtensionAttribute[] attributes = (FileTypeExtensionAttribute[])fieldInfo.GetCustomAttributes(typeof(FileTypeExtensionAttribute), false);
		return attributes.Length > 0 ? attributes[0].Extension : string.Empty;
	}
}
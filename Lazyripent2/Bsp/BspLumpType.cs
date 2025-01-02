namespace Lazyripent2.Bsp;

public enum LumpType
{
	//vhlt-v34 limits
	[LumpMaxLength(16384)] Entities = 0,
	[LumpMaxLength(32768)] Planes,
	[LumpMaxLength(4096)] Textures,
	[LumpMaxLength(65535)] Vertices,
	[LumpMaxLength(0x800000)] Visibility,
	[LumpMaxLength(32767)] Nodes,
	[LumpMaxLength(32767)] TexInfo,
	[LumpMaxLength(65535)] Faces,
	[LumpMaxLength(0x3000000)] Lighting,
	[LumpMaxLength(32767)] ClipNodes,
	[LumpMaxLength(8192)] Leaves,
	[LumpMaxLength(65535)] MarkSurfaces,
	[LumpMaxLength(256000)] Edges,
	[LumpMaxLength(512000)] SurfEdges,
	[LumpMaxLength(512)] Models,
	
	[LumpMaxLength(0)] TotalLumpTypes,
}

[System.AttributeUsage(System.AttributeTargets.Field)]
public class LumpMaxLengthAttribute(int maxLength) : System.Attribute
{
    public int MaxLength {get; private set;} = maxLength;
}

public static class LumpTypeExtensions
{
	public static int GetMaxLength(this LumpType value)
	{
		System.Reflection.FieldInfo? fieldInfo = value.GetType()?.GetField(value.ToString());
		if(fieldInfo is null)
		{
			return 0;
		}

		LumpMaxLengthAttribute[] attributes = (LumpMaxLengthAttribute[])fieldInfo.GetCustomAttributes(typeof(LumpMaxLengthAttribute), false);
		return attributes.Length > 0 ? attributes[0].MaxLength : 0;
	}
}
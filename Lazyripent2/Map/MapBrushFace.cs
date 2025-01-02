namespace Lazyripent2.Map;

public class MapBrushFace
{
	public record Plane
	{
		public float[] Point {get; set;} = new float[3];
	}

	public record TextureInfo
	{
		public float[] Normal {get; set;} = new float[3];
		public float Offset {get; set;} = 0.0f;
	}

	public Plane[] Planes {get; set;} = new Plane[3];
	public string Texture {get; set;} = string.Empty;
	public TextureInfo[] TextureInfos {get; set;} = new TextureInfo[2];
	public float Rotation {get; set;} = 0.0f;
	public float UScale {get; set;} = 0.0f;
	public float VScale {get; set;} = 0.0f;
}
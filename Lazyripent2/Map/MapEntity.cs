using Lazyripent2.Bsp;

namespace Lazyripent2.Map;

public class MapEntity : Entity
{
	public List<List<MapBrushFace>> BrushFaces {get; set;} = [];
}
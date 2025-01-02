using Lazyripent2.Bsp;

namespace Lazyripent2;

public interface IParseableFile
{
	public void DeserializeFromFile(string fullPath);
	public void SerializeToFile(string fullPath);
	public List<Entity> GetEntities();
	public void SetEntities(List<Entity> entities);
	public string GetFileNameNoExt();
}
namespace Lazyripent2;

public record EntityUpgrade(string NewKey, string ValueFormat)
{
	//super basic for now.
	public string NewKey {get; set;} = NewKey;
	public string ValueFormat {get; set;} = ValueFormat;
}
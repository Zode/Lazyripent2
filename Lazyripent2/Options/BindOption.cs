namespace Lazyripent2;

public record BindOption(string PrimaryBind, string SecondaryBind, string Description, int Consume, Action<object?> Action)
{
	public string PrimaryBind {get; private set;} = PrimaryBind;
	public string SecondaryBind {get; private set;} = SecondaryBind;
	public string Description {get; private set;} = Description;
	public int Consume {get; private set;} = Consume;
	public Action<object?> Action {get; private set;} = Action;
}
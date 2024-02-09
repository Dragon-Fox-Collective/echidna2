namespace Echidna2.Core;

[ComponentImplementation<RectTransform>]
public interface IRectTransform : IPropertyHaver, IHierarchy
{
	public Vector2 Size { get; }
	public Vector2 Position { get; set; }
}

public partial class RectTransform(
	[Component] IHierarchy? hierarchy = null)
	: IRectTransform, INotificationHook<IUpdate.Notification>
{
	public Vector2 Size { get; private set; }
	public Vector2 Position { get; set; }
	
	private Vector2 MaxChildSize => hierarchy.GetChildren().OfType<IRectTransform>().Where(transform => transform != this).Aggregate(Vector2.Zero, (accumulate, transform) => new Vector2(Math.Max(accumulate.X, transform.Size.X), Math.Max(accumulate.Y, transform.Size.Y)));
	
	public void OnPreNotify(IUpdate.Notification notification)
	{
		foreach (object child in hierarchy.GetChildren())
			if (child is IRectTransform rectTransform)
				rectTransform.Position = Position + Vector2.One;
	}
	public void OnPostNotify(IUpdate.Notification notification)
	{
		Size = hierarchy.GetChildren().Any() ? MaxChildSize + Vector2.One * 2 : Vector2.One;
	}
	
	public IEnumerable<string> GetPropertyList()
	{
		yield return "size";
		yield return "position";
	}
}
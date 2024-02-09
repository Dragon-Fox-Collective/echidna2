using Echidna.Mathematics;

namespace Echidna2.Core;

[ComponentImplementation<RectTransform>]
public interface IRectTransform : IPropertyHaver, IHierarchy, INotificationHook<IUpdate.Notification>
{
	public Vector3 Size { get; }
	public Vector3 Position { get; set; }
}

public partial class RectTransform(
	[Component] IHierarchy? hierarchy = null)
	: IRectTransform
{
	public Vector3 Size { get; private set; }
	public Vector3 Position { get; set; }
	
	private Vector3 MaxChildSize => hierarchy.GetChildren().OfType<IRectTransform>().Where(transform => transform != this).Aggregate(Vector3.Zero, (accumulate, transform) => new Vector3(Math.Max(accumulate.X, transform.Size.X), Math.Max(accumulate.Y, transform.Size.Y), Math.Max(accumulate.Z, transform.Size.Z)));
	
	public void OnPreNotify(IUpdate.Notification notification)
	{
		foreach (object child in hierarchy.GetChildren())
			if (child is IRectTransform rectTransform)
				rectTransform.Position = Position + Vector3.One;
	}
	public void OnPostNotify(IUpdate.Notification notification)
	{
		Size = hierarchy.GetChildren().Any() ? MaxChildSize + Vector3.One * 2 : Vector3.One;
	}
	
	public IEnumerable<string> GetPropertyList()
	{
		yield return "size";
		yield return "position";
	}
}
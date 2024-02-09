namespace Echidna2.Core;

[ComponentImplementation<RectTransform>]
public interface IRectTransform : IPropertyHaver, IHierarchy
{
	public Vector2 Size { get; }
}

public partial class RectTransform(
	[Component] IHierarchy? hierarchy = null)
	: IRectTransform
{
	public Vector2 Size { get; private set; }
	
	private Vector2 MaxChildSize => hierarchy.GetChildren().OfType<IRectTransform>().Where(transform => transform != this).Aggregate(Vector2.Zero, (accumulate, transform) => new Vector2(Math.Max(accumulate.X, transform.Size.X), Math.Max(accumulate.Y, transform.Size.Y)));
	
	public void Update(double deltaTime)
	{
		hierarchy.Update(deltaTime);
		Size = MaxChildSize + Vector2.One * 2;
	}
	
	public IEnumerable<string> GetPropertyList()
	{
		yield return "rect transform size";
	}
}
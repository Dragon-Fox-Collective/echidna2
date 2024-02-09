namespace Echidna2.Core;

[ComponentImplementation<RectTransform>]
public interface IRectTransform
{
	public Vector2 Size { get; }
}

public partial class RectTransform(
	[Component] IHierarchy? hierarchy = null)
	: IRectTransform
{
	public Vector2 Size => MaxChildSize + Vector2.One * 2;
	
	private Vector2 MaxChildSize => hierarchy.GetChildren().OfType<IRectTransform>().Where(transform => transform != this).Aggregate(Vector2.Zero, (accumulate, transform) => new Vector2(Math.Max(accumulate.X, transform.Size.X), Math.Max(accumulate.Y, transform.Size.Y)));
}
namespace Echidna2.Core;

[ComponentImplementation<RectTransform>]
public interface IRectTransform
{
	public Vector2 Size { get; }
}

public partial class RectTransform(
	[Component] IHierarchy? hierarchy = null)
	: Entity, IRectTransform
{
	public Vector2 Size => ((hierarchy.GetChildren().FirstOrDefault() as IRectTransform)?.Size ?? Vector2.Zero) + Vector2.One * 2;
}
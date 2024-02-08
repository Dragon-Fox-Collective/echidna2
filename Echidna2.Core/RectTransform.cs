namespace Echidna2.Core;

public interface IRectTransform
{
	public Vector2 Size { get; }
}

public class RectTransform(IHierarchy? hierarchy = null) : Entity, IHierarchy, IRectTransform
{
	private IHierarchy hierarchy = hierarchy ?? new Hierarchy();
	
	public void AddChild(Entity entity) => hierarchy.AddChild(entity);
	public IEnumerable<Entity> GetChildren() => hierarchy.GetChildren();
	
	public Vector2 Size => ((hierarchy.GetChildren().FirstOrDefault() as IRectTransform)?.Size ?? Vector2.Zero) + Vector2.One * 2;
}
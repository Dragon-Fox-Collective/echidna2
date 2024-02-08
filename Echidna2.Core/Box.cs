using Echidna2.SourceGenerators;

namespace Echidna2.Core;

public partial class Box(
	[Component] IHierarchy? hierarchy = null,
	[Component] IRectTransform? rectTransform = null)
	: Entity
{
	public override void Draw()
	{
		Console.WriteLine($"{GetChildren().FirstOrDefault()}");
		Console.WriteLine("\u250c" + new string('\u2500', (int)Size.X) + "\u2510");
		base.Draw();
		Console.WriteLine("\u2514" + new string('\u2500', (int)Size.X) + "\u2518");
	}
}


// AUTO GENERATED CODE
partial class Box : IHierarchy, IRectTransform
{
	private IHierarchy hierarchy = hierarchy ?? new Hierarchy();
	private IRectTransform rectTransform = rectTransform ?? new RectTransform();
	
	public void AddChild(Entity entity) => hierarchy.AddChild(entity);
	public IEnumerable<Entity> GetChildren() => hierarchy.GetChildren();
	
	public Vector2 Size => rectTransform.Size;
}
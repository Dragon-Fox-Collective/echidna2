namespace Echidna2.Core;

public partial class Box : Entity
{
	public Box(
		[Component] IHierarchy? hierarchy = null,
		[Component] IRectTransform? rectTransform = null)
	{
		this.hierarchy = hierarchy ?? new Hierarchy();
		this.rectTransform = rectTransform ?? new RectTransform(this.hierarchy);
	}
	
	public override void Draw()
	{
		Console.WriteLine("\u250c" + new string('\u2500', (int)Size.X) + "\u2510");
		hierarchy.Draw();
		Console.WriteLine("\u2514" + new string('\u2500', (int)Size.X) + "\u2518");
	}
}
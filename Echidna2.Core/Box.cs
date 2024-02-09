namespace Echidna2.Core;

public partial class Box : IPropertyHaver
{
	public Box(
		[Component] IHierarchy? hierarchy = null,
		[Component] IRectTransform? rectTransform = null)
	{
		this.hierarchy = hierarchy ?? new Hierarchy(new Named(GetType().Name));
		this.rectTransform = rectTransform ?? new RectTransform(this.hierarchy);
	}
	
	public void Draw()
	{
		Console.WriteLine("\u250c" + new string('\u2500', (int)Size.X) + "\u2510");
		hierarchy.Draw();
		Console.WriteLine("\u2514" + new string('\u2500', (int)Size.X) + "\u2518");
	}
	
	public IEnumerable<string> GetPropertyList()
	{
		yield return "box property";
		foreach (string property in rectTransform.GetPropertyList()) yield return property;
	}
}
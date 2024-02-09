namespace Echidna2.Core;

public partial class Box
{
	public Box(
		[Component] IHierarchy? hierarchy = null,
		[Component] IRectTransform? rectTransform = null)
	{
		this.hierarchy = hierarchy ?? new Hierarchy(new Named(GetType().Name));
		this.rectTransform = rectTransform ?? new RectTransform(this.hierarchy);
	}
	
	public void PreUpdate()
	{
		rectTransform.PreUpdate();
	}
	
	public void Update(double deltaTime)
	{
		rectTransform.Update(deltaTime);
	}
	
	public void AddChild(object child) => hierarchy.AddChild(child);
	public bool RemoveChild(object child) => hierarchy.RemoveChild(child);
	public IEnumerable<object> GetChildren() => hierarchy.GetChildren();
	public void PrintTree(int depth = 0) => hierarchy.PrintTree(depth);
	
	public void Draw()
	{
		Console.WriteLine(new string(' ', (int)Position.X) + "\u250c" + new string('\u2500', (int)Size.X) + "\u2510");
		rectTransform.Draw();
		Console.WriteLine(new string(' ', (int)Position.X) + "\u2514" + new string('\u2500', (int)Size.X) + "\u2518");
	}
	
	public IEnumerable<string> GetPropertyList()
	{
		yield return "box property";
		foreach (string property in rectTransform.GetPropertyList()) yield return property;
	}
}
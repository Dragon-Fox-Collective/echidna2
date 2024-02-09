namespace Echidna2.Core;

public partial class Box : INotificationHook<IDraw.Notification>
{
	public Box(
		[Component] IHierarchy? hierarchy = null,
		[Component] IRectTransform? rectTransform = null)
	{
		this.hierarchy = hierarchy ?? new Hierarchy(new Named(GetType().Name));
		this.rectTransform = rectTransform ?? new RectTransform(this.hierarchy);
	}
	
	public void AddChild(object child) => hierarchy.AddChild(child);
	public bool RemoveChild(object child) => hierarchy.RemoveChild(child);
	public IEnumerable<object> GetChildren() => hierarchy.GetChildren();
	public void PrintTree(int depth = 0) => hierarchy.PrintTree(depth);
	
	public void PreNotify(IDraw.Notification notification)
	{
		Console.WriteLine(new string('\u2502', (int)Position.X) + "\u250c" + new string('\u2500', (int)Size.X) + "\u2510");
	}
	public void Notify<T>(T notification) => rectTransform.Notify(notification);
	public void PostNotify(IDraw.Notification notification)
	{
		Console.WriteLine(new string('\u2502', (int)Position.X) + "\u2514" + new string('\u2500', (int)Size.X) + "\u2518");
	}
	
	public IEnumerable<string> GetPropertyList()
	{
		yield return "box property";
		foreach (string property in rectTransform.GetPropertyList()) yield return property;
	}
}
using Echidna2.Core;

namespace Echidna2;

public partial class ReallyCoolPrefab
{
	public ReallyCoolPrefab(
		[Component] IHierarchy? hierarchy = null,
		[Component] IRectTransform? rectTransform = null)
	{
		this.hierarchy = hierarchy ?? new Hierarchy(new Named("ReallyCoolPrefab"));
		this.rectTransform = rectTransform ?? new RectTransform(this.hierarchy);
	}
	
	public void AddChild(object child) => hierarchy.AddChild(child);
	public bool RemoveChild(object child) => hierarchy.RemoveChild(child);
	public IEnumerable<object> GetChildren() => hierarchy.GetChildren();
	public void PrintTree(int depth = 0) => hierarchy.PrintTree(depth);
	public void Notify<T>(T notification) => rectTransform.Notify(notification);
	
	public IEnumerable<string> GetPropertyList()
	{
		yield return "really cool property";
		foreach (string property in rectTransform.GetPropertyList()) yield return property;
	}
}
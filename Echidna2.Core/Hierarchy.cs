namespace Echidna2.Core;

[ComponentImplementation<Hierarchy>]
public interface IHierarchy : INamed, INotificationPropagator
{
	public void AddChild(object child);
	public bool RemoveChild(object child);
	public IEnumerable<object> GetChildren();
	public void PrintTree(int depth = 0);
}

public partial class Hierarchy : IHierarchy
{
	private List<object> children = [];
	
	private bool isNotifying = false;
	
	public Hierarchy(
		[Component] INamed? named = null)
	{
		this.named = named ?? new Named(GetType().Name);
	}
	
	public void Notify<T>(T notification)
	{
		if (isNotifying) return;
		isNotifying = true;
		
		foreach (INotificationHook<T> child in children.OfType<INotificationHook<T>>())
			child.OnPreNotify(notification);
		foreach (INotificationListener<T> child in children.OfType<INotificationListener<T>>())
			child.OnNotify(notification);
		foreach (INotificationHook<T> child in children.OfType<INotificationHook<T>>())
			child.OnPostNotify(notification);
		foreach (INotificationPropagator child in children.OfType<INotificationPropagator>())
			child.Notify(notification);
		
		isNotifying = false;
	}
	
	public void AddChild(object child) => children.Add(child);
	
	public bool RemoveChild(object child) => children.Remove(child);
	
	public IEnumerable<object> GetChildren() => children;
	
	public void PrintTree(int depth = 0)
	{
		PrintLayer(depth, named.Name);
		foreach (object child in children)
		{
			if (child is IHierarchy childHierarchy)
				childHierarchy.PrintTree(depth + 1);
			else if (child is INamed childNamed)
				PrintLayer(depth + 1, childNamed.Name);
			else
				PrintLayer(depth + 1, child.GetType().Name);
		}
	}
	
	private static void PrintLayer(int depth, string name) => Console.WriteLine((depth > 0 ? new string(' ', (depth - 1) * 2) + "\u2514 " : "") + name);
}
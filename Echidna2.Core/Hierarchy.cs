using Echidna2.Serialization;

namespace Echidna2.Core;

public class Hierarchy : INotificationPropagator, IHasChildren, ICanAddChildren, INotificationListener<InitializeNotification>
{
	public delegate void ChildAddedHandler(object child);
	public event ChildAddedHandler? ChildAdded;
	public delegate void ChildRemovedHandler(object child);
	public event ChildRemovedHandler? ChildRemoved;
	
	private List<object> children = [];
	[SerializedReference(typeof(ChildrenSerializer))]
	public IEnumerable<object> Children
	{
		get => children;
		set
		{
			ClearChildren();
			value.ForEach(AddChild);
		}
	}
	
	public Hierarchy? Parent { get; private set; }
	
	private HashSet<object> currentNotifications = [];
	
	private bool hasBeenInitialized;
	
	public void OnNotify(InitializeNotification notification)
	{
		if (hasBeenInitialized) return;
		hasBeenInitialized = true;
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		if (notification is AddedToHierarchyNotification addedToHierarchy)
		{
			Parent = addedToHierarchy.Parent;
			return;
		}
		
		if (!currentNotifications.Add(notification)) return;
		INotificationPropagator.Notify(notification, children);
		currentNotifications.Remove(notification);
	}
	
	public void AddChild(object child)
	{
		children.Add(child);
		if (hasBeenInitialized)
			INotificationPropagator.Notify(new InitializeNotification(), child);
		INotificationPropagator.Notify(new AddedToHierarchyNotification(this), child);
		ChildAdded?.Invoke(child);
	}
	
	public void QueueAddChild(object child)
	{
		INotificationPropagator.NotificationFinished += () => AddChild(child);
	}
	
	public bool RemoveChild(object child)
	{
		if (children.Remove(child))
		{
			ChildRemoved?.Invoke(child);
			return true;
		}
		return false;
	}
	
	public void QueueRemoveChild(object child)
	{
		INotificationPropagator.NotificationFinished += () =>
		{
			if (!RemoveChild(child))
				Console.WriteLine($"Warning: Tried to remove a child {child} that wasn't in the hierarchy {this}");
		};
	}
	
	public void ClearChildren()
	{
		while (children.Count > 0)
			RemoveChild(children[0]);
	}
}

public interface IHasChildren
{
	public IEnumerable<object> Children { get; }
	
	public static void PrintTree(IHasChildren hierarchy, int depth = 0)
	{
		PrintLayer(depth, hierarchy);
		foreach (object child in hierarchy.Children)
		{
			if (child is IHasChildren childHierarchy)
				PrintTree(childHierarchy, depth + 1);
			else
				PrintLayer(depth + 1, child);
		}
	}
	
	private static void PrintLayer(int depth, object obj) => PrintLayer(depth, obj is INamed named ? named.Name : obj.GetType().Name + " (no name)");
	private static void PrintLayer(int depth, string name) => Console.WriteLine((depth > 0 ? string.Concat(Enumerable.Repeat("  ", depth - 1)) + "\u2514 " : "") + name);
}

public interface ICanAddChildren
{
	public IEnumerable<object> Children { get; set; }
	public void AddChild(object child);
	public void QueueAddChild(object child);
	public bool RemoveChild(object child);
	public void QueueRemoveChild(object child);
	public void ClearChildren();
}
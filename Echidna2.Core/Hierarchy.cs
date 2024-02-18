﻿namespace Echidna2.Core;

public class Hierarchy : INotificationPropagator, IHasChildren, ICanAddChildren
{
	public delegate void ChildAddedHandler(object child);
	public event ChildAddedHandler? ChildAdded;
	public delegate void ChildRemovedHandler(object child);
	public event ChildRemovedHandler? ChildRemoved;
	
	private List<object> children = [];
	public IEnumerable<object> Children => children;
	
	private HashSet<object> currentNotifications = [];
	
	public void Notify<T>(T notification) where T : notnull
	{
		if (!currentNotifications.Add(notification)) return;
		INotificationPropagator.Notify(notification, children);
		currentNotifications.Remove(notification);
	}
	
	public void AddChild(object child)
	{
		children.Add(child);
		ChildAdded?.Invoke(child);
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
	public void AddChild(object child);
	public bool RemoveChild(object child);
}
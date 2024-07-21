namespace Echidna2.Core;

[DontExpose]
public interface INotificationListener<in T>
{
	public void OnNotify(T notification);
}

[DontExpose]
public interface INotificationHook<in T>
{
	public void OnPreNotify(T notification);
	public void OnPostNotify(T notification);
	public void OnPostPropagate(T notification);
}

[DontExpose]
public interface INotificationPredicate<in T>
{
	public bool ShouldNotificationPropagate(T notification);
}

[DontExpose]
public interface INotificationPropagator
{
	private static Stack<object> currentNotifications = [];
	private static Dictionary<object, Action?> notificationDelegates = [];
	
	public static event Action? NotificationFinished
	{
		add => notificationDelegates[currentNotifications.Peek()] += value;
		remove => notificationDelegates[currentNotifications.Peek()] -= value;
	}
	
	public void Notify<T>(T notification) where T : notnull;
	
	public static void Notify<T>(T notification, params object[] objects) where T : notnull => Notify(notification, objects as ICollection<object>);
	public static void Notify<T>(T notification, ICollection<object> objects) where T : notnull
	{
		if (objects.Any<object?>(child => child is null))
			throw new NullReferenceException($"Null child in Notify with notification {notification}, objects {objects.ToDelimString()}");
		
		bool newNotification = !currentNotifications.Contains(notification);
		if (newNotification)
		{
			currentNotifications.Push(notification);
			notificationDelegates.Add(notification, null);
		}
		
		if (!objects.OfType<INotificationPredicate<T>>().All(child => child.ShouldNotificationPropagate(notification)))
			return;
		foreach (INotificationHook<T> child in objects.OfType<INotificationHook<T>>())
			child.OnPreNotify(notification);
		foreach (INotificationListener<T> child in objects.OfType<INotificationListener<T>>())
			child.OnNotify(notification);
		foreach (INotificationHook<T> child in objects.OfType<INotificationHook<T>>())
			child.OnPostNotify(notification);
		foreach (INotificationPropagator child in objects.OfType<INotificationPropagator>())
			child.Notify(notification);
		foreach (INotificationHook<T> child in objects.OfType<INotificationHook<T>>())
			child.OnPostPropagate(notification);
		
		if (newNotification)
		{
			object poppedNotificaiton = currentNotifications.Pop();
			if (!poppedNotificaiton.Equals(notification))
				throw new InvalidOperationException($"Notification stack out of order: {poppedNotificaiton} != {notification}");
			notificationDelegates.Remove(notification, out Action? action);
			action?.Invoke();
		}
	}
}

public class InitializeNotification;

public class PreUpdateNotification;

public class UpdateNotification(double deltaTime)
{
	public double DeltaTime { get; } = deltaTime;
}

public class PostUpdateNotification;

public class AddedToHierarchyNotification(Hierarchy parent)
{
	public Hierarchy Parent { get; } = parent;
}
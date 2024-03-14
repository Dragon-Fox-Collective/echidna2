namespace Echidna2.Core;

public interface INotificationListener<in T>
{
	public void OnNotify(T notification);
}

public interface INotificationHook<in T>
{
	public void OnPreNotify(T notification);
	public void OnPostNotify(T notification);
	public void OnPostPropagate(T notification);
}

public interface INotificationPredicate<in T>
{
	public bool ShouldNotificationPropagate(T notification);
}

public interface INotificationPropagator
{
	public void Notify<T>(T notification) where T : notnull;
	
	public static void Notify<T>(T notification, params object[] objects) where T : notnull
	{
		if (objects.Any<object?>(child => child is null))
			throw new NullReferenceException($"Null child in Notify with objects {objects.ToDelimString()}");
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
	}
	
	public static void Notify<T>(T notification, IList<object> objects) where T : notnull
	{
		if (objects.Any<object?>(child => child is null))
			throw new NullReferenceException($"Null child in Notify with objects {objects.ToDelimString()}");
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
	}
}

public interface IPreUpdate : INotificationListener<IPreUpdate.Notification>
{
	public class Notification;
	void INotificationListener<Notification>.OnNotify(Notification notification) => OnPreUpdate();
	public void OnPreUpdate();
}

public interface IUpdate : INotificationListener<IUpdate.Notification>
{
	public class Notification(double deltaTime)
	{
		public double DeltaTime { get; } = deltaTime;
	}
	void INotificationListener<Notification>.OnNotify(Notification notification) => OnUpdate(notification.DeltaTime);
	public void OnUpdate(double deltaTime);
}
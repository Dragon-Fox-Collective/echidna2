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

public interface INotificationPropagator
{
	public void Notify<T>(T notification);
	
	public static void NotifySingle<T>(object? @object, T notification)
	{
		INotificationHook<T>? hook = @object as INotificationHook<T>;
		INotificationListener<T>? listener = @object as INotificationListener<T>;
		INotificationPropagator? propagator = @object as INotificationPropagator;
		
		hook?.OnPreNotify(notification);
		listener?.OnNotify(notification);
		hook?.OnPostNotify(notification);
		propagator?.Notify(notification);
		hook?.OnPostPropagate(notification);
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
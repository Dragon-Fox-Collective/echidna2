namespace Echidna2.Core;

public interface INotificationListener<in T>
{
	public void Notify(T notification);
}

public interface INotificationPropagator
{
	public void Notify<T>(T notification);
}

public interface IPreUpdate : INotificationListener<IPreUpdate.Notification>
{
	public class Notification;
	void INotificationListener<Notification>.Notify(Notification notification) => PreUpdate();
	public void PreUpdate();
}

public interface IUpdate : INotificationListener<IUpdate.Notification>
{
	public class Notification(double deltaTime)
	{
		public double DeltaTime { get; } = deltaTime;
	}
	void INotificationListener<Notification>.Notify(Notification notification) => Update(notification.DeltaTime);
	public void Update(double deltaTime);
}

public interface IDraw : INotificationListener<IDraw.Notification>
{
	public class Notification;
	void INotificationListener<Notification>.Notify(Notification notification) => Draw();
	public void Draw();
}
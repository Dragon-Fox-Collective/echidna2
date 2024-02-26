using Echidna2.Core;

namespace Echidna2.Physics;

public interface IPhysicsUpdate : INotificationListener<IPhysicsUpdate.Notification>
{
	public class Notification(double deltaTime)
	{
		public double DeltaTime { get; } = deltaTime;
	}
	void INotificationListener<Notification>.OnNotify(Notification notification) => OnPhysicsUpdate(notification.DeltaTime);
	public void OnPhysicsUpdate(double deltaTime);
}

public interface IInitializeIntoSimulation : INotificationListener<IInitializeIntoSimulation.Notification>
{
	public class Notification(WorldSimulation simulation)
	{
		public WorldSimulation Simulation { get; } = simulation;
	}
	void INotificationListener<Notification>.OnNotify(Notification notification) => OnIntializeIntoWorld(notification.Simulation);
	public void OnIntializeIntoWorld(WorldSimulation simulation);
}
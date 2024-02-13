namespace Echidna2.Core;

public abstract class Prefab<T> : IInstantiatable<T> where T : Prefab<T>
{
	public Hierarchy Hierarchy { get; } = new(new Named("Prefab Hierarchy"));
	public Hierarchy PrefabChildren { get; } = new(new Named("Prefab Children"));
	
	public bool IsSharedInstance { get; set; } = false;
	
	public abstract T Instantiate();
}

public abstract class PrefabInstance : INotificationPropagator
{
	public Hierarchy PrefabChildren { get; } = new(new Named("Prefab Instance"));
	
	public abstract void Notify<T>(T notification);
}
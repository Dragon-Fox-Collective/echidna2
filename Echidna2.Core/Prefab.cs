namespace Echidna2.Core;

public class PrefabRoot
{
	public Hierarchy Hierarchy { get; } = new(new Named("Prefab Root Hierarchy"));
	public Hierarchy PrefabChildren { get; } = new(new Named("Prefab Root Children"));
}

public abstract class Prefab : INotificationPropagator
{
	public Hierarchy Hierarchy { get; } = new(new Named("Prefab"));
	
	public abstract void Notify<T>(T notification);
}
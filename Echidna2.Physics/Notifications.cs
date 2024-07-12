namespace Echidna2.Physics;

public class PhysicsUpdateNotification(double deltaTime)
{
	public double DeltaTime { get; } = deltaTime;
}

public class InitializeIntoSimulationNotification(WorldSimulation simulation)
{
	public WorldSimulation Simulation { get; } = simulation;
}
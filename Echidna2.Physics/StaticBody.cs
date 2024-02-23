using BepuPhysics;
using Echidna2.Rendering3D;

namespace Echidna2.Physics;

public class StaticBody
{
	public PhysicsMaterial PhysicsMaterial
	{
		get => Simulation.PhysicsMaterials[Handle];
		set => Simulation.PhysicsMaterials[Handle] = value;
	}
	public CollisionFilter CollisionFilter
	{
		get => Simulation.CollisionFilters[Handle];
		set => Simulation.CollisionFilters[Handle] = value;
	}
	
	public readonly WorldSimulation Simulation;
	public readonly Transform3D Transform;
	public readonly StaticHandle Handle;
	public readonly StaticReference Reference;
	
	public StaticBody(WorldSimulation simulation, Transform3D transform, BodyShape shape)
	{
		Simulation = simulation;
		Transform = transform;
		Handle = simulation.AddStaticBody(transform, shape);
		Reference = simulation[Handle];
	}
}
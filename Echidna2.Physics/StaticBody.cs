using BepuPhysics;
using Echidna2.Rendering3D;

namespace Echidna2.Physics;

public class StaticBody(Transform3D transform, BodyShape shape)
{
	public PhysicsMaterial PhysicsMaterial = new();
	public CollisionFilter CollisionFilter = new();
	
	public StaticHandle Handle { get; private init; }
	public StaticReference Reference { get; private init; }
	
	public WorldSimulation Simulation
	{
		init
		{
			Handle = value.AddStaticBody(transform, shape, ref PhysicsMaterial, ref CollisionFilter);
			Reference = value[Handle];
		}
	}
}
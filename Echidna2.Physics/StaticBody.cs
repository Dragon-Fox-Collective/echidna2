using BepuPhysics;
using Echidna2.Rendering3D;

namespace Echidna2.Physics;

public class StaticBody(Transform3D transform, BodyShape shape)
{
	public PhysicsMaterial PhysicsMaterial = new();
	
	private StaticHandle handle;
	public StaticReference Reference { get; private init; }
	
	public WorldSimulation Simulation
	{
		init
		{
			handle = value.AddStaticBody(transform, shape, ref PhysicsMaterial);
			Reference = value[handle];
		}
	}
}
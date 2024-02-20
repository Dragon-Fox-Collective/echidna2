using BepuPhysics;
using Echidna2.Core;
using Echidna2.Rendering3D;

namespace Echidna2.Physics;

public class DynamicBody(Transform3D transform, BodyShape shape, BodyInertia inertia) : IUpdate
{
	public PhysicsMaterial PhysicsMaterial = new();
	
	private BodyHandle handle;
	public BodyReference Reference { get; private init; }
	
	public WorldSimulation Simulation
	{
		init
		{
			handle = value.AddDynamicBody(transform, shape, inertia, ref PhysicsMaterial);
			Reference = value[handle];
		}
	}
	
	public void OnUpdate(double deltaTime)
	{
		RigidPose pose = Reference.Pose;
		transform.LocalPosition = pose.Position;
		transform.LocalRotation = pose.Orientation;
	}
}
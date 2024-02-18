using BepuPhysics;
using Echidna2.Core;
using Echidna2.Rendering3D;

namespace Echidna2.Physics;

public class DynamicBody(WorldSimulation simulation, Transform3D transform, BodyShape shape, BodyInertia inertia) : IUpdate
{
	private BodyHandle handle = simulation.Simulation.Bodies.Add(BodyDescription.CreateDynamic(new RigidPose(transform.LocalPosition, transform.LocalRotation), inertia, shape.AddToShapes(simulation.Simulation.Shapes), 0.01f));
	public BodyReference Reference => simulation.Simulation.Bodies[handle];
	
	public void OnUpdate(double deltaTime)
	{
		RigidPose pose = simulation.Simulation.Bodies[handle].Pose;
		transform.LocalPosition = pose.Position;
		transform.LocalRotation = pose.Orientation;
	}
}
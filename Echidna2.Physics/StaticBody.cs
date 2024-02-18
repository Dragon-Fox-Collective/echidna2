using BepuPhysics;
using Echidna2.Rendering3D;

namespace Echidna2.Physics;

public class StaticBody(WorldSimulation simulation, Transform3D transform, BodyShape shape)
{
	private StaticHandle handle = simulation.Simulation.Statics.Add(new StaticDescription(new RigidPose(transform.LocalPosition, transform.LocalRotation), shape.AddToShapes(simulation.Simulation.Shapes)));
	public StaticReference Reference => simulation.Simulation.Statics[handle];
}
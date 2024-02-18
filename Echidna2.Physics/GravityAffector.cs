using BepuPhysics;
using Echidna2.Mathematics;

namespace Echidna2.Physics;

public class GravityAffector(DynamicBody body) : IPhysicsUpdate
{
	public void OnPhysicsUpdate(double deltaTime)
	{
		BodyReference bodyReference = body.Reference;
		bodyReference.Velocity.Linear += (System.Numerics.Vector3)(Vector3.Down * 9.8 * deltaTime);
		bodyReference.Awake = true;
	}
}
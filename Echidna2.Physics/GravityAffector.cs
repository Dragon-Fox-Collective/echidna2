using BepuPhysics;
using Echidna2.Mathematics;
using Echidna2.Serialization;

namespace Echidna2.Physics;

public class GravityAffector : IPhysicsUpdate
{
	[SerializedReference] public DynamicBody? Body;
	
	public void OnPhysicsUpdate(double deltaTime)
	{
		BodyReference bodyReference = Body!.Reference;
		bodyReference.Velocity.Linear += (System.Numerics.Vector3)(Vector3.Down * 9.8 * deltaTime);
		bodyReference.Awake = true;
	}
}
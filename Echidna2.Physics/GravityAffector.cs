using BepuPhysics;
using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Serialization;

namespace Echidna2.Physics;

public class GravityAffector : INotificationListener<PhysicsUpdateNotification>
{
	[SerializedReference] public DynamicBody? Body;
	
	public void OnNotify(PhysicsUpdateNotification notification)
	{
		BodyReference bodyReference = Body!.Reference;
		bodyReference.Velocity.Linear += (System.Numerics.Vector3)(Vector3.Down * 9.8 * notification.DeltaTime);
		bodyReference.Awake = true;
	}
}
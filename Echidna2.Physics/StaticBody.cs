using BepuPhysics;
using Echidna2.Core;
using Echidna2.Rendering3D;
using Echidna2.Serialization;

namespace Echidna2.Physics;

public class StaticBody : INotificationListener<InitializeIntoSimulationNotification>
{
	private PhysicsMaterial physicsMaterial;
	[SerializedValue] public PhysicsMaterial PhysicsMaterial
	{
		get => physicsMaterial;
		set
		{
			physicsMaterial = value;
			if (Simulation is not null)
				Simulation.PhysicsMaterials[Handle] = value;
		}
	}
	
	private CollisionFilter collisionFilter;
	[SerializedValue] public CollisionFilter CollisionFilter
	{
		get => collisionFilter;
		set
		{
			collisionFilter = value;
			if (Simulation is not null)
				Simulation.CollisionFilters[Handle] = value;
		}
	}
	
	public WorldSimulation? Simulation { get; private set; }
	[SerializedReference] public Transform3D Transform = null!;
	[SerializedValue(typeof(BodyShapeSerializer))] public BodyShape Shape = null!;
	public StaticHandle Handle { get; private set; }
	public StaticReference Reference { get; private set; }
	
	public void OnNotify(InitializeIntoSimulationNotification notification)
	{
		Simulation = notification.Simulation;
		Handle = Simulation.AddStaticBody(Transform, Shape, PhysicsMaterial, CollisionFilter);
		Reference = Simulation[Handle];
	}
}
using BepuPhysics;
using Echidna2.Rendering3D;
using Echidna2.Serialization;

namespace Echidna2.Physics;

public class StaticBody : IInitializeIntoSimulation
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
	
	public void OnIntializeIntoWorld(WorldSimulation simulation)
	{
		Simulation = simulation;
		Handle = simulation.AddStaticBody(Transform, Shape, PhysicsMaterial, CollisionFilter);
		Reference = simulation[Handle];
	}
}
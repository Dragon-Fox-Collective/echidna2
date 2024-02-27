using BepuPhysics;
using Echidna2.Rendering3D;
using Echidna2.Serialization;
using Tomlyn.Model;

namespace Echidna2.Physics;

public class StaticBody : IInitializeIntoSimulation, ITomlSerializable
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
	public BodyShape Shape = null!;
	public StaticHandle Handle { get; private set; }
	public StaticReference Reference { get; private set; }
	
	public void OnIntializeIntoWorld(WorldSimulation simulation)
	{
		Simulation = simulation;
		Handle = simulation.AddStaticBody(Transform, Shape, PhysicsMaterial, CollisionFilter);
		Reference = simulation[Handle];
	}
	
	public void Serialize(TomlTable table)
	{
		
	}
    
	public bool DeserializeValue(string id, object value)
	{
		switch (id)
		{
			case "Shape":
				Shape = BepuPhysicsSerialization.DeserializeBodyShape((TomlTable)value);
				return true;
			default:
				return false;
		}
	}
	
	public bool DeserializeReference(string id, object value, Dictionary<string, object> references)
	{
		return false;
	}
}
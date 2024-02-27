using BepuPhysics;
using BepuPhysics.Collidables;
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
    
	public void DeserializeValues(TomlTable table)
	{
		// TODO: merge this and dynamicbody. actually make a big registry for deserialization classes
		if (table.TryGetValue("Shape", out object? shapeValue))
		{
			TomlTable shapeTable = (TomlTable)shapeValue;
			string type = (string)shapeTable["Type"];
			if (type == "Sphere")
				Shape = BodyShape.Of(new Sphere((float)(double)shapeTable["Radius"]));
			else if (type == "Box")
				Shape = BodyShape.Of(new Box((float)(double)shapeTable["Width"], (float)(double)shapeTable["Length"], (float)(double)shapeTable["Height"]));
			else
				throw new InvalidOperationException($"Shape type {type} does not exist");
		}
	}
	
	public void DeserializeReferences(TomlTable table, Dictionary<string, object> references)
	{
		
	}
}
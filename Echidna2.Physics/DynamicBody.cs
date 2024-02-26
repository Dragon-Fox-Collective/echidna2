using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Rendering3D;
using Echidna2.Serialization;
using Tomlyn.Model;

namespace Echidna2.Physics;

public class DynamicBody : IUpdate, IInitializeIntoSimulation, ITomlSerializable
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
	public BodyInertia Inertia;
	public BodyHandle Handle { get; private set; }
	public BodyReference Reference { get; private set; }
	
	public Vector3 GlobalPosition
	{
		get => Transform.GlobalPosition;
		set
		{
			Transform.GlobalPosition = value;
			if (Simulation is not null)
				Reference.Pose.Position = value;
		}
	}
	public Quaternion GlobalRotation
	{
		get => Transform.GlobalRotation;
		set
		{
			Transform.GlobalRotation = value;
			if (Simulation is not null)
				Reference.Pose.Orientation = value;
		}
	}
	
	public void OnIntializeIntoWorld(WorldSimulation simulation)
	{
		Simulation = simulation;
		Handle = simulation.AddDynamicBody(Transform, Shape, Inertia, PhysicsMaterial, CollisionFilter);
		Reference = simulation[Handle];
	}
	
	public void OnUpdate(double deltaTime)
	{
		GlobalPosition = Reference.Pose.Position;
		GlobalRotation = Reference.Pose.Orientation;
	}
	
	public void Serialize(TomlTable table)
	{
		
	}
	
	public void DeserializeValues(TomlTable table)
	{
		if (table.TryGetValue("Inertia", out object? inertiaValue))
		{
			TomlTable inertiaTable = (TomlTable)inertiaValue;
			TomlTable inertiaTensorTable = (TomlTable)inertiaTable["InverseInertiaTensor"];
			Inertia = new BodyInertia
			{
				InverseMass = (float)(double)inertiaTable["InverseMass"],
				InverseInertiaTensor = new Symmetric3x3
				{
					XX = (float)(double)inertiaTensorTable["XX"],
					YX = (float)(double)inertiaTensorTable["YX"],
					YY = (float)(double)inertiaTensorTable["YY"],
					ZX = (float)(double)inertiaTensorTable["ZX"],
					ZY = (float)(double)inertiaTensorTable["ZY"],
					ZZ = (float)(double)inertiaTensorTable["ZZ"],
				}
			};
		}
		
		if (table.TryGetValue("Shape", out object? shapeValue))
		{
			TomlTable shapeTable = (TomlTable)shapeValue;
			string type = (string)shapeTable["Type"];
			if (type == "Sphere")
				Shape = BodyShape.Of(new Sphere((float)(double)shapeTable["Radius"]));
			else if (type == "Box")
				Shape = BodyShape.Of(new Box((float)(double)shapeTable["Width"], (float)(double)shapeTable["Height"], (float)(double)shapeTable["Depth"]));
			else
				throw new InvalidOperationException($"Shape type {type} does not exist");
		}
	}
	
	public void DeserializeReferences(TomlTable table, Dictionary<string, object> references)
	{
		
	}
}
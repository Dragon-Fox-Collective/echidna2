using BepuPhysics;
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
	
	public bool DeserializeValue(string id, object value)
	{
		switch (id)
		{
			case "Inertia":
				Inertia = BepuPhysicsSerialization.DeserializeBodyInertia((TomlTable)value);
				return true;
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
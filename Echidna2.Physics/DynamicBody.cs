using BepuPhysics;
using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Rendering3D;
using Echidna2.Serialization;

namespace Echidna2.Physics;

public class DynamicBody : INotificationListener<UpdateNotification>, INotificationListener<InitializeIntoSimulationNotification>
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
	[SerializedValue(typeof(BodyInertiaSerializer))] public BodyInertia Inertia;
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
	
	public void OnNotify(InitializeIntoSimulationNotification notification)
	{
		Simulation = notification.Simulation;
		Handle = Simulation.AddDynamicBody(Transform, Shape, Inertia, PhysicsMaterial, CollisionFilter);
		Reference = Simulation[Handle];
	}
	
	public void OnNotify(UpdateNotification notification)
	{
		GlobalPosition = Reference.Pose.Position;
		GlobalRotation = Reference.Pose.Orientation;
	}
}
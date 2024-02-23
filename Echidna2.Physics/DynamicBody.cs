using BepuPhysics;
using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Rendering3D;

namespace Echidna2.Physics;

public class DynamicBody : IUpdate
{
	public PhysicsMaterial PhysicsMaterial
	{
		get => Simulation.PhysicsMaterials[Handle];
		set => Simulation.PhysicsMaterials[Handle] = value;
	}
	public CollisionFilter CollisionFilter
	{
		get => Simulation.CollisionFilters[Handle];
		set => Simulation.CollisionFilters[Handle] = value;
	}
	
	public readonly WorldSimulation Simulation;
	public readonly Transform3D Transform;
	public readonly BodyHandle Handle;
	public readonly BodyReference Reference;
	
	public Vector3 GlobalPosition
	{
		get => Reference.Pose.Position;
		set
		{
			Reference.Pose.Position = value;
			Transform.GlobalPosition = value;
		}
	}
	public Quaternion GlobalRotation
	{
		get => Reference.Pose.Orientation;
		set
		{
			Reference.Pose.Orientation = value;
			Transform.GlobalRotation = value;
		}
	}
	
	public DynamicBody(WorldSimulation simulation, Transform3D transform, BodyShape shape, BodyInertia inertia)
	{
		Simulation = simulation;
		Transform = transform;
		Handle = simulation.AddDynamicBody(transform, shape, inertia);
		Reference = simulation[Handle];
	}
	
	public void OnUpdate(double deltaTime)
	{
		GlobalPosition = Reference.Pose.Position;
		GlobalRotation = Reference.Pose.Orientation;
	}
}
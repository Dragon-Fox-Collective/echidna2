using System.Numerics;
using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Memory;
using Echidna2.Core;
using Echidna2.Rendering3D;
using Echidna2.Serialization;

namespace Echidna2.Physics;

public class WorldSimulation : IUpdate, IInitialize
{
	public double PhysicsDeltaTime { get; set; } = 1 / 60.0;
	private double accumulatedTime = 0;
	
	[SerializedReference] public INotificationPropagator World = null!;
	private BufferPool bufferPool = new();
	private ThreadDispatcher threadDispatcher = new(Environment.ProcessorCount);
	private Simulation simulation = null!;
	
	public readonly CollidableProperty<PhysicsMaterial> PhysicsMaterials = new();
	public readonly CollidableProperty<CollisionFilter> CollisionFilters = new();
	
	public bool HasBeenInitialized { get; set; }
	private bool hasBeenDisposed;
	
	public void OnInitialize()
	{
		simulation = Simulation.Create(
			bufferPool,
			new NarrowPhaseCallbacks
			{
				PhysicsMaterials = PhysicsMaterials,
				CollisionFilters = CollisionFilters,
			},
			new PoseIntegratorCallbacks(),
			new SolveDescription(8, 1));
		INotificationPropagator.Notify(new IInitializeIntoSimulation.Notification(this), World);
	}
	
	public void OnUpdate(double deltaTime)
	{
		accumulatedTime += deltaTime;
		
		while (accumulatedTime >= PhysicsDeltaTime)
		{
			double physicsDeltaTime = PhysicsDeltaTime;
			accumulatedTime -= physicsDeltaTime;
			World.Notify(new IPhysicsUpdate.Notification(physicsDeltaTime));
			simulation.Timestep((float)physicsDeltaTime, threadDispatcher);
		}
	}
	
	public BodyHandle AddDynamicBody(Transform3D transform, BodyShape shape, BodyInertia inertia, PhysicsMaterial material, CollisionFilter collisionFilter)
	{
		BodyHandle handle = simulation.Bodies.Add(BodyDescription.CreateDynamic(new RigidPose(transform.LocalPosition, transform.LocalRotation), inertia, AddShape(shape), 0.01f));
		PhysicsMaterials.Allocate(handle) = material;
		CollisionFilters.Allocate(handle) = collisionFilter;
		return handle;
	}
	
	public StaticHandle AddStaticBody(Transform3D transform, BodyShape shape, PhysicsMaterial material, CollisionFilter collisionFilter)
	{
		StaticHandle handle = simulation.Statics.Add(new StaticDescription(new RigidPose(transform.LocalPosition, transform.LocalRotation), AddShape(shape)));
		PhysicsMaterials.Allocate(handle) = material;
		CollisionFilters.Allocate(handle) = collisionFilter;
		return handle;
	}
	
	public TypedIndex AddShape(BodyShape shape) => shape.AddToShapes(simulation.Shapes);
	
	public void AddJoint<TDescription>(DynamicBody a, DynamicBody b, TDescription joint) where TDescription : unmanaged, ITwoBodyConstraintDescription<TDescription>
	{
		simulation.Solver.Add(a.Handle, b.Handle, joint);
	}
	
	public BodyReference this[BodyHandle handle] => simulation.Bodies[handle];
	public StaticReference this[StaticHandle handle] => simulation.Statics[handle];
	
	public void Dispose()
	{
		hasBeenDisposed = true;
		simulation.Dispose();
		threadDispatcher.Dispose();
		bufferPool.Clear();
	}
	
	~WorldSimulation()
	{
		if (!hasBeenDisposed)
			Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
	}
	
	private struct NarrowPhaseCallbacks : INarrowPhaseCallbacks
	{
		public CollidableProperty<PhysicsMaterial> PhysicsMaterials;
		public CollidableProperty<CollisionFilter> CollisionFilters;
		
		public void Initialize(Simulation simulation)
		{
			PhysicsMaterials.Initialize(simulation);
			CollisionFilters.Initialize(simulation);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
		{
			return (a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic) && (CollisionFilters[a].Collision & CollisionFilters[b].Membership) != 0 || (CollisionFilters[b].Collision & CollisionFilters[a].Membership) != 0;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
		{
			return true;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
		{
			pairMaterial.FrictionCoefficient = (float)Math.Min(PhysicsMaterials[pair.A].Friction, PhysicsMaterials[pair.B].Friction);
			pairMaterial.MaximumRecoveryVelocity = 2f;
			pairMaterial.SpringSettings = new SpringSettings(30, 1);
			return true;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
		{
			return true;
		}
		
		public void Dispose()
		{
		}
	}
	
	private struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks
	{
		public readonly AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;
		
		public readonly bool AllowSubstepsForUnconstrainedBodies => false;
		
		public readonly bool IntegrateVelocityForKinematics => false;
		
		public void Initialize(Simulation simulation)
		{
		}
		
		public void PrepareForIntegration(float dt)
		{
		}
		
		public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity)
		{
		}
	}
}

public struct PhysicsMaterial()
{
	[SerializedValue] public double Friction = 1;
}

public struct CollisionFilter()
{
	[SerializedValue] public long Membership = long.MinValue;
	[SerializedValue] public long Collision = long.MaxValue;
}
using System.Numerics;
using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Memory;
using Echidna2.Core;

namespace Echidna2.Physics;

public class WorldSimulation : IUpdate
{
	public double PhysicsDeltaTime { get; set; } = 1f / 60f;
	private double accumulatedTime = 0;
	
	private INotificationPropagator world;
	private BufferPool bufferPool;
	private ThreadDispatcher threadDispatcher;
	public Simulation Simulation { get; }
	
	private bool hasBeenDisposed;
	
	public WorldSimulation(INotificationPropagator world)
	{
		this.world = world;
		bufferPool = new BufferPool();
		Simulation = Simulation.Create(bufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(), new SolveDescription(8, 1));
		threadDispatcher = new ThreadDispatcher(Environment.ProcessorCount);
	}
	
	public void OnUpdate(double deltaTime)
	{
		accumulatedTime += deltaTime;
		
		while (accumulatedTime >= PhysicsDeltaTime)
		{
			double physicsDeltaTime = PhysicsDeltaTime;
			accumulatedTime -= physicsDeltaTime;
			world.Notify(new IPhysicsUpdate.Notification(physicsDeltaTime));
			Simulation.Timestep((float)physicsDeltaTime, threadDispatcher);
		}
	}
	
	public void Dispose()
	{
		hasBeenDisposed = true;
		Simulation.Dispose();
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
		public void Initialize(Simulation simulation)
		{
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
		{
			return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
		{
			return true;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
		{
			pairMaterial.FrictionCoefficient = 1f;
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
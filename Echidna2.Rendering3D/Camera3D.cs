using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Rendering;

namespace Echidna2.Rendering3D;

public class Camera3D(INotificationPropagator world, Transform3D transform) : Camera(world)
{
	public double FieldOfView { get; set; } = 80;
	
	public Transform3D Transform => transform;
	
	public override Matrix4 ViewMatrix => Transform.GlobalTransform.Inverted;
	public override Matrix4 ProjectionMatrix => Matrix4.FromPerspectiveProjection(FieldOfView, Size.X / Size.Y, NearClipPlane, FarClipPlane);
}
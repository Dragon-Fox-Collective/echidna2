using Echidna2.Mathematics;
using Echidna2.Rendering;
using Echidna2.Serialization;

namespace Echidna2.Rendering3D;

public class Camera3D : Camera
{
	[SerializedValue] public double FieldOfView { get; set; } = 80;
	
	[SerializedReference] public Transform3D Transform { get; set; } = null!;
	
	public override Matrix4 ViewMatrix => Transform.GlobalTransform.Inverted;
	public override Matrix4 ProjectionMatrix => Matrix4.FromPerspectiveProjection(FieldOfView, Size.X / Size.Y, NearClipPlane, FarClipPlane);
}
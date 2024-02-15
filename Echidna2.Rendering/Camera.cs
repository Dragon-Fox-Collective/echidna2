using Echidna2.Core;
using Echidna2.Mathematics;
using Vector2i = OpenTK.Mathematics.Vector2i;

namespace Echidna2.Rendering;

public class Camera
{
	public Vector2i Size { get; set; }
	public double FarClipPlane { get; set; } = 1000;
	public double NearClipPlane { get; set; } = 0.1;
	
	public INotificationPropagator? World { get; set; }
	
	public Matrix4 ViewMatrix => Matrix4.Translation(Vector3.Out * (FarClipPlane - 1)).Inverted;
	public Matrix4 ProjectionMatrix => Matrix4.OrthographicProjection(Size.X, Size.Y, (float)NearClipPlane, (float)FarClipPlane);
	
	public void Notify<T>(T notification)
	{
		INotificationPropagator.Notify(notification, World);
	}
	
	public Vector3 ScreenToGlobal(Vector2 position)
	{
		Vector3 normalized = new (position.X / Size.X * 2 - 1, -(position.Y / Size.Y * 2 - 1), 0);
		return ViewMatrix.InverseTransformPoint(ProjectionMatrix.InverseTransformPoint(normalized));
	}
	// Not sure why the y's are being flipped in one but not the other
	public Vector2 GlobalToScreen(Vector3 position)
	{
		Vector3 normalized = ProjectionMatrix.TransformPoint(ViewMatrix.TransformPoint(position));
		return new Vector2((normalized.X + 1) * Size.X / 2, (normalized.Y + 1) * Size.Y / 2);
	}
}
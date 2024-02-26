using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Serialization;

namespace Echidna2.Rendering;

public abstract class Camera
{
	[SerializedValue] public Vector2 Size { get; set; }
	[SerializedValue] public double FarClipPlane { get; set; } = 1000;
	[SerializedValue] public double NearClipPlane { get; set; } = 0.1;
	
	[SerializedReference] public INotificationPropagator? World { get; set; }
	
	public abstract Matrix4 ViewMatrix { get; }
	public abstract Matrix4 ProjectionMatrix { get; }
	
	public void Notify<T>(T notification) where T : notnull
	{
		if (World is null)
			throw new InvalidOperationException("Camera has no world to notify.");
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
using Echidna2.Mathematics;
using Echidna2.Rendering;
using Echidna2.Serialization;

namespace Echidna2.Gui;

public class GuiCamera : Camera
{
	[SerializedValue] public Vector2 GlobalPosition;
	[SerializedValue] public double Zoom = 1;
	
	public override Matrix4 ViewMatrix => (Matrix4.FromTranslation(GlobalPosition.WithZ(FarClipPlane - 1)) * Matrix4.FromScale((Zoom, Zoom, 1))).Inverted;
	public override Matrix4 ProjectionMatrix => Matrix4.FromOrthographicProjection(Size.X, Size.Y, (float)NearClipPlane, (float)FarClipPlane);
}
using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Rendering;

namespace Echidna2.Gui;

public class GuiCamera(INotificationPropagator world) : Camera(world)
{
	public override Matrix4 ViewMatrix => Matrix4.FromTranslation(Vector3.Out * (FarClipPlane - 1)).Inverted;
	public override Matrix4 ProjectionMatrix => Matrix4.FromOrthographicProjection(Size.X, Size.Y, (float)NearClipPlane, (float)FarClipPlane);
}
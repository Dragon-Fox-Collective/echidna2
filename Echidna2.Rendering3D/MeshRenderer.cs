using System.Drawing;
using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Rendering;

namespace Echidna2.Rendering3D;

public class MeshRenderer(Transform3D transform) : INotificationListener<IDraw.Notification>
{
	public Mesh Mesh { get; set; } = Mesh.Cube;
	public Shader Shader { get; set; } = Shader.Solid;
	public Color Color { get; set; } = Color.Gray;
	
	public void OnNotify(IDraw.Notification notification)
	{
		Shader.Bind(notification.Camera.ViewMatrix, notification.Camera.ProjectionMatrix);
		Shader.SetMatrix4("distortion", Matrix4.Identity);
		Shader.SetMatrix4("transform", transform.GlobalTransform);
		Shader.SetColorRgba("color", Color);
		Mesh.Draw();
	}
}
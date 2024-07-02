using System.Drawing;
using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Rendering;

namespace Echidna2.Rendering3D;

public class MeshRenderer(Transform3D transform) : INotificationListener<Draw_Notification>
{
	public Mesh Mesh { get; set; } = Mesh.Cube;
	public Shader Shader { get; set; } = Shader.Solid;
	public Color Color { get; set; } = Color.Gray;
	
	public void OnNotify(Draw_Notification notification)
	{
		Shader.Bind();
		Shader.SetMatrix4("view", notification.Camera.ViewMatrix);
		Shader.SetMatrix4("projection", notification.Camera.ProjectionMatrix);
		Shader.SetMatrix4("distortion", Matrix4.Identity);
		Shader.SetMatrix4("transform", transform.GlobalTransform);
		Shader.SetColorRgba("color", Color);
		Mesh.Draw();
	}
}
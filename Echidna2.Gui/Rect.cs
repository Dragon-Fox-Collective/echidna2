using System.Drawing;
using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Rendering;

namespace Echidna2.Gui;

public class Rect(RectTransform rectTransform) : INotificationListener<IDraw.Notification>
{
	private static readonly Shader Shader = new(ShaderNodeUtil.MainVertexShader, File.ReadAllText("Assets/rect.frag"));
	
	public Color Color { get; set; } = Color.Gray;
	
	public void OnNotify(IDraw.Notification notification)
	{
		Shader.Bind(notification.Camera.ViewMatrix, notification.Camera.ProjectionMatrix);
		Shader.SetMatrix4("distortion", Matrix4.Scale(rectTransform.LocalSize.WithZ(1) / 2));
		Shader.SetMatrix4("transform", rectTransform.GlobalTransform);
		Shader.SetColor("color", Color);
		Mesh.Quad.Draw();
	}
}
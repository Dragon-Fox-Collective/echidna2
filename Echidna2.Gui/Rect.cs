using System.Drawing;
using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Rendering;

namespace Echidna2.Gui;

public class Rect(RectTransform rectTransform) : INotificationListener<IDraw.Notification>
{
	private static Shader shader = new(ShaderNodeUtil.MainVertexShader, File.ReadAllText("rect.frag"));
	
	private static Mesh mesh = new([
		-1.0f, -1.0f, +0.0f,
		+1.0f, -1.0f, +0.0f,
		-1.0f, +1.0f, +0.0f,
		+1.0f, +1.0f, +0.0f
	], [
		0.0f, 0.0f,
		1.0f, 0.0f,
		0.0f, 1.0f,
		1.0f, 1.0f
	], [
		0.0f, 0.0f, 0.0f,
		1.0f, 0.0f, 0.0f,
		0.0f, 1.0f, 0.0f,
		0.0f, 0.0f, 1.0f
	], [
		0, 1, 2,
		2, 1, 3
	], false);
	
	public Color Color { get; set; } = Color.Gray;
	
	public void OnNotify(IDraw.Notification notification)
	{
		shader.Bind(notification.Camera.ViewMatrix, notification.Camera.ProjectionMatrix);
		shader.SetMatrix4("distortion", Matrix4.Scale(rectTransform.LocalSize.WithZ(1) / 2));
		shader.SetMatrix4("transform", rectTransform.GlobalTransform);
		shader.SetColor("color", Color);
		mesh.Draw();
	}
}
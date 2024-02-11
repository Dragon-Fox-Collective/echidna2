using Echidna2.Core;
using Echidna.Mathematics;

namespace Echidna2.Rendering;

public partial class Rect(
	[Component] IRectTransform? rectTransform = null)
	: INotificationListener<IDraw.Notification>
{
	private static Shader shader = new(ShaderNodeUtil.MainVertexShader, File.ReadAllText("uv-coords.frag"));
	
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
	
	public void OnNotify(IDraw.Notification notification)
	{
		shader.Bind(
			Matrix4.Translation(Vector3.Out * (notification.Camera.FarClipPlane - 1)).Inverted,
			Matrix4.OrthographicProjection(notification.Camera.Size.X, notification.Camera.Size.Y, (float)notification.Camera.NearClipPlane, (float)notification.Camera.FarClipPlane));
		shader.SetMatrix4(0, rectTransform.GlobalTransform * Matrix4.Scale(rectTransform.Size.WithZ(1) / 2));
		mesh.Draw();
	}
}
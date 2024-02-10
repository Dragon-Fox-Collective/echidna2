using Echidna2.Core;
using OpenTK.Mathematics;
using Vector3 = Echidna.Mathematics.Vector3;

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
	
	private static Matrix4 viewMatrix = Matrix4.CreateTranslation(Vector3.Out).Inverted();
	
	public void OnNotify(IDraw.Notification notification)
	{
		shader.Bind(viewMatrix, Matrix4.CreateOrthographic(notification.Camera.Size.X, notification.Camera.Size.Y, (float)notification.Camera.NearClipPlane, (float)notification.Camera.FarClipPlane));
		shader.SetMatrix4(0,
			Matrix4.CreateScale(Vector3.FromXY(rectTransform.Size / 2))
			* Matrix4.CreateFromQuaternion(Quaternion.Identity)
			* Matrix4.CreateTranslation(Vector3.FromXY(rectTransform.Position, rectTransform.Depth + 1 - notification.Camera.FarClipPlane)));
		mesh.Draw();
	}
}
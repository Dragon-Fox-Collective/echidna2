using Echidna2.Core;
using OpenTK.Mathematics;

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
	
	private static Matrix4 viewMatrix = Matrix4.CreateTranslation(Vector3.UnitZ).Inverted();
	
	public void OnNotify(IDraw.Notification notification)
	{
		shader.Bind(viewMatrix, notification.ScreenSize);
		shader.SetMatrix4(0, Matrix4.CreateScale(Vector3.One * 10) * Matrix4.CreateFromQuaternion(Quaternion.Identity) * Matrix4.CreateTranslation(Vector3.Zero));
		mesh.Draw();
	}
}
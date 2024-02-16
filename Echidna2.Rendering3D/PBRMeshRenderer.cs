using System.Drawing;
using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Rendering;

namespace Echidna2.Rendering3D;

public class PBRMeshRenderer(Transform3D transform) : INotificationListener<IDraw.Notification>
{
	public Mesh Mesh { get; set; } = Mesh.Cube;
	public Shader Shader { get; set; } = Shader.PBR;
	public Color Albedo { get; set; } = Color.Gray;
	
	public void OnNotify(IDraw.Notification notification)
	{
		Shader.Bind(notification.Camera.ViewMatrix, notification.Camera.ProjectionMatrix);
		Shader.SetMatrix4("distortion", Matrix4.Identity);
		Shader.SetMatrix4("transform", transform.GlobalTransform);
		Shader.SetColorRgb("albedo", Albedo);
		Shader.SetFloat("metallic", 0.0f);
		Shader.SetFloat("roughness", 0.25f);
		Shader.SetFloat("ao", 1.0f);
		Shader.SetInt("numLights", 1);
		Shader.SetVector3("lightPositions[0]", new Vector3(0, 0, 0));
		Shader.SetVector3("lightColors[0]", new Vector3(1, 1, 1) * 300.0f);
		Shader.SetVector3("cameraPosition", notification.Camera.ViewMatrix.Inverted.Translation);
		Mesh.Draw();
	}
}
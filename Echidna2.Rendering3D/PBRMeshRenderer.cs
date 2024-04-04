using System.Drawing;
using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Rendering;
using Echidna2.Serialization;

namespace Echidna2.Rendering3D;

public class PBRMeshRenderer : INotificationListener<IDraw.Notification>
{
	[SerializedReference] public Transform3D? Transform;
	
	[SerializedValue] public Mesh Mesh { get; set; } = Mesh.Cube;
	public Shader Shader { get; set; } = Shader.PBR;
	
	[SerializedValue] public Color Albedo { get; set; } = Color.Gray;
	[SerializedValue] public double Metallic { get; set; } = 0.0;
	[SerializedValue] public double Roughness { get; set; } = 0.25;
	[SerializedValue] public double AmbientOcclusion { get; set; } = 1.0;
	
	public void OnNotify(IDraw.Notification notification)
	{
		Shader.Bind();
		Shader.SetMatrix4("view", notification.Camera.ViewMatrix);
		Shader.SetMatrix4("projection", notification.Camera.ProjectionMatrix);
		Shader.SetMatrix4("distortion", Matrix4.Identity);
		Shader.SetMatrix4("transform", Transform!.GlobalTransform);
		Shader.SetColorRgb("albedo", Albedo);
		Shader.SetFloat("metallic", (float)Metallic);
		Shader.SetFloat("roughness", (float)Roughness);
		Shader.SetFloat("ao", (float)AmbientOcclusion);
		Shader.SetInt("numLights", 1);
		Shader.SetVector3("lightPositions[0]", new Vector3(0, 0, 0));
		Shader.SetVector3("lightColors[0]", new Vector3(1, 1, 1) * 300.0f);
		Shader.SetVector3("cameraPosition", notification.Camera.ViewMatrix.Inverted.Translation);
		Mesh.Draw();
	}
}
using System.Drawing;
using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Rendering;
using Echidna2.Serialization;
using Tomlyn.Model;

namespace Echidna2.Rendering3D;

public class PBRMeshRenderer : INotificationListener<IDraw.Notification>, ITomlSerializable
{
	[SerializedReference] public Transform3D? Transform;
	
	public Mesh Mesh { get; set; } = Mesh.Cube;
	public Shader Shader { get; set; } = Shader.PBR;
	
	public Color Albedo { get; set; } = Color.Gray;
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
	
	public void Serialize(TomlTable table)
	{
		
	}
	
	public void DeserializeValues(TomlTable table)
	{
		if (table.TryGetValue("Albedo", out object? albedoValue))
		{
			TomlTable albedoTable = (TomlTable)albedoValue;
			Albedo = Color.FromArgb((int)((double)albedoTable["A"] * 255), (int)((double)albedoTable["R"] * 255), (int)((double)albedoTable["G"] * 255), (int)((double)albedoTable["B"] * 255));
		}
		
		if (table.TryGetValue("Mesh", out object? meshValue))
		{
			string type = (string)meshValue;
			if (type == "Sphere")
				Mesh = Mesh.Sphere;
			else if (type == "Cube")
				Mesh = Mesh.Cube;
			else
				throw new InvalidOperationException($"Mesh type {type} does not exist");
		}
	}
	
	public void DeserializeReferences(TomlTable table, Dictionary<string, object> references)
	{
		
	}
}
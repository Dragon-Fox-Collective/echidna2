using System.Drawing;
using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Rendering;
using Echidna2.Serialization;
using Tomlyn.Model;

namespace Echidna2.Gui;

public class Rect : INotificationListener<IDraw.Notification>, ITomlSerializable
{
	private static readonly Shader Shader = new(ShaderNodeUtil.MainVertexShader, File.ReadAllText("Assets/rect.frag"));
	
	[SerializedReference] public RectTransform RectTransform { get; set; } = null!;
	
	public Color Color { get; set; } = Color.Gray;
	
	public void OnNotify(IDraw.Notification notification)
	{
		Shader.Bind();
		Shader.SetMatrix4("view", notification.Camera.ViewMatrix);
		Shader.SetMatrix4("projection", notification.Camera.ProjectionMatrix);
		Shader.SetMatrix4("distortion", Matrix4.FromScale(RectTransform.LocalSize.WithZ(1) / 2));
		Shader.SetMatrix4("transform", RectTransform.GlobalTransform);
		Shader.SetColorRgba("color", Color);
		Mesh.Quad.Draw();
	}
	
	public void Serialize(TomlTable table)
	{
		
	}
	
	public bool DeserializeValue(string id, object value)
	{
		switch (id)
		{
			case "Color":
				Color = SystemSerialization.DeserializeColor((TomlTable)value);
				return true;
			default:
				return false;
		}
	}
	
	public bool DeserializeReference(string id, object value, Dictionary<string, object> references)
	{
		return false;
	}
}
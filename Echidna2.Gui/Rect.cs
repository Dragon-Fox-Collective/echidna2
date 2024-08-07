﻿using System.Drawing;
using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Rendering;
using Echidna2.Serialization;

namespace Echidna2.Gui;

public class Rect : INotificationListener<DrawNotification>
{
	private static readonly Shader Shader = new(ShaderNodeUtil.MainVertexShader, File.ReadAllText("Assets/rect.frag"));
	
	[SerializedReference] public RectTransform RectTransform { get; set; } = null!;
	
	[SerializedValue] public Color Color { get; set; } = Color.Gray;
	
	public void OnNotify(DrawNotification notification)
	{
		Shader.Bind();
		Shader.SetMatrix4("view", notification.Camera.ViewMatrix);
		Shader.SetMatrix4("projection", notification.Camera.ProjectionMatrix);
		Shader.SetMatrix4("distortion", Matrix4.FromScale(RectTransform.LocalSize.WithZ(1) / 2));
		Shader.SetMatrix4("transform", RectTransform.GlobalTransform);
		Shader.SetColorRgba("color", Color);
		Mesh.Quad.Draw();
	}
}
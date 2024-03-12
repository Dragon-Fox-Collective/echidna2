using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Mathematics;
using Echidna2.Rendering;
using Echidna2.Rendering3D;
using Echidna2.Serialization;
using OpenTK.Graphics.OpenGL4;

namespace Echidna2;

public interface Viewport
{
	public void AddChild(object child);
}

[SerializeExposedMembers, Prefab("Prefabs/ViewportGui.toml")]
public partial class ViewportGui : Viewport, ICanBeLaidOut, INotificationListener<IDraw.Notification>, IUpdate
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference] public RenderTarget RenderTarget { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy Hierarchy { get; set; } = null!;
	[SerializedReference] public GuiCamera Camera { get; set; } = null!;
	[SerializedReference] public RectLayout GuiRectLayout { get; set; } = null!;
	[SerializedReference] public RectTransform GuiRectTransform { get; set; } = null!;
	
	private static readonly Shader Shader = new(ShaderNodeUtil.MainVertexShader, File.ReadAllText("Assets/solid_texture.frag"));
	
	public void Notify<T>(T notification) where T : notnull
	{
		if (notification is not IDraw.Notification)
			INotificationPropagator.Notify(notification, Hierarchy, RenderTarget, GuiRectLayout);
	}
	
	public void OnNotify(IDraw.Notification notification)
	{
		Shader.Bind();
		Shader.SetMatrix4("view", notification.Camera.ViewMatrix);
		Shader.SetMatrix4("projection", notification.Camera.ProjectionMatrix);
		Shader.SetMatrix4("distortion", Matrix4.FromScale(RectTransform.LocalSize.WithZ(1) / 2));
		Shader.SetMatrix4("transform", RectTransform.GlobalTransform);
		GL.BindTexture(TextureTarget.Texture2D, RenderTarget.ColorTexture);
		Shader.SetInt("colorTexture", 0);
		Mesh.Quad.Draw();
	}
	
	public void OnUpdate(double deltaTime)
	{
		Camera.Size = RectTransform.LocalSize;
	}
}

[SerializeExposedMembers, Prefab("Prefabs/Viewport3D.toml")]
public partial class Viewport3D : Viewport, ICanBeLaidOut, INotificationListener<IDraw.Notification>, IUpdate
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference] public RenderTarget RenderTarget { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy Hierarchy { get; set; } = null!;
	[SerializedReference] public Camera3D Camera { get; set; } = null!;
	
	private static readonly Shader Shader = new(ShaderNodeUtil.MainVertexShader, File.ReadAllText("Assets/solid_texture.frag"));
	
	public void Notify<T>(T notification) where T : notnull
	{
		if (notification is not IDraw.Notification)
			INotificationPropagator.Notify(notification, Hierarchy, RenderTarget);
	}
	
	public void OnNotify(IDraw.Notification notification)
	{
		Shader.Bind();
		Shader.SetMatrix4("view", notification.Camera.ViewMatrix);
		Shader.SetMatrix4("projection", notification.Camera.ProjectionMatrix);
		Shader.SetMatrix4("distortion", Matrix4.FromScale(RectTransform.LocalSize.WithZ(1) / 2));
		Shader.SetMatrix4("transform", RectTransform.GlobalTransform);
		GL.BindTexture(TextureTarget.Texture2D, RenderTarget.ColorTexture);
		Shader.SetInt("colorTexture", 0);
		Mesh.Quad.Draw();
	}
	
	public void OnUpdate(double deltaTime)
	{
		Camera.Size = RectTransform.LocalSize;
	}
}
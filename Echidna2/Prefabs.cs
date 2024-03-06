using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Mathematics;
using Echidna2.Rendering;
using Echidna2.Serialization;
using OpenTK.Graphics.OpenGL4;
using Color = System.Drawing.Color;

namespace Echidna2;


[SerializeExposedMembers, Prefab("Prefabs/RectWithHierarchy.toml")]
public partial class RectWithHierarchy : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren, ICanAddChildren
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectLayout RectLayout { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Rect Rect { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect, RectLayout, PrefabChildren);
	}
}


[SerializeExposedMembers, Prefab("Prefabs/ButtonRect.toml")]
public partial class ButtonRect : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren, ICanAddChildren, IInitialize
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectLayout RectLayout { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Rect Rect { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Button Button { get; set; } = null!;
	
	public void OnInitialize()
	{
		Button.MouseDown += () => Rect.Color = Color.DarkGray;
		Button.MouseUp += () => Rect.Color = Color.LightGray;
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect, Button, RectLayout, PrefabChildren);
	}
}


[SerializeExposedMembers, Prefab("Prefabs/Named.toml")]
public partial class HLayoutWithHierarchy : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren, ICanAddChildren
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public HorizontalLayout Layout { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Layout, PrefabChildren);
	}
}


[SerializeExposedMembers, Prefab("Prefabs/VLayoutWithHierarchy.toml")]
public partial class VLayoutWithHierarchy : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren, ICanAddChildren
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public VerticalLayout Layout { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Layout, PrefabChildren);
	}
}


[SerializeExposedMembers, Prefab("Prefabs/TextRect.toml")]
public partial class TextRect : INotificationPropagator, ICanBeLaidOut, INamed
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Text Text { get; set; } = null!;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Text);
	}
}


[SerializeExposedMembers, Prefab("Prefabs/HierarchyDisplay.toml")]
public partial class HierarchyDisplay : INotificationPropagator, ICanBeLaidOut, INamed, IInitialize
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference] public IHasChildren HierarchyToDisplay { get; set; } = null!;
	[SerializedReference] private Hierarchy DisplayElements { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public FullLayout Layout { get; set; } = null!;
	
	public void OnInitialize()
	{
		DisplayElements.AddChild(BoxOfHierarchy(HierarchyToDisplay));
	}
	
	private static FullLayoutWithHierarchy BoxOfHierarchy(object obj)
	{
		FullLayoutWithHierarchy box = FullLayoutWithHierarchy.Instantiate();
		box.Name = $"Box for {obj}";
		box.AnchorPreset = AnchorPreset.Full;
		box.LeftMargin = 10;
		VLayoutWithHierarchy layout = VLayoutWithHierarchy.Instantiate();
		layout.Name = $"Layout for box for {obj}";
		layout.AnchorPreset = AnchorPreset.Full;
		box.AddChild(layout);
		
		TextRect text = TextRect.Instantiate();
		text.TextString = obj is INamed named ? named.Name : obj.GetType().Name + " (no name)";
		text.AnchorPreset = AnchorPreset.Full;
		text.LocalScale = Vector2.One * 0.5;
		text.MinimumSize = (0, 25);
		text.Justification = TextJustification.Left;
		layout.AddChild(text);
		
		if (obj is IHasChildren hasChildren)
			foreach (object child in hasChildren.Children)
				layout.AddChild(BoxOfHierarchy(child));
		
		return box;
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Layout, DisplayElements);
	}
}


[SerializeExposedMembers, Prefab("Prefabs/FullLayoutWithHierarchy.toml")]
public partial class FullLayoutWithHierarchy : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren, ICanAddChildren
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public FullLayout Layout { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Layout, PrefabChildren);
	}
}


[SerializeExposedMembers, Prefab("Prefabs/ViewportGui.toml")]
public partial class ViewportGui : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren, ICanAddChildren, INotificationListener<IDraw.Notification>, IUpdate
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference] public RenderTarget RenderTarget { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy Hierarchy { get; set; } = null!;
	[SerializedReference] public IHasCamera CameraHaver { get; set; } = null!;
	[SerializedReference] public RectLayout GuiRectLayout { get; set; } = null!;
	[SerializedReference] public RectTransform GuiRectTransform { get; set; } = null!;
	
	private static readonly Shader Shader = new(ShaderNodeUtil.MainVertexShader, File.ReadAllText("Assets/solid_texture.frag"));
	
	public void Notify<T>(T notification) where T : notnull
	{
		if (notification is IInitialize.Notification or IDispose.Notification or IPreUpdate.Notification or IUpdate.Notification or IDrawPass.Notification)
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
		CameraHaver.HavedCamera.Size = RectTransform.LocalSize;
		GuiRectTransform.LocalSize = RectTransform.LocalSize;
	}
}
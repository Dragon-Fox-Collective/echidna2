using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Mathematics;
using Echidna2.Rendering;
using Echidna2.Rendering3D;
using Echidna2.Serialization;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Echidna2;

[SerializeExposedMembers, Prefab("Prefabs/HierarchyDisplay.toml")]
public partial class HierarchyDisplay : INotificationPropagator, ICanBeLaidOut, INamed, IInitialize
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference] public IHasChildren? HierarchyToDisplay { get; set; } = null!;
	[SerializedReference] private Hierarchy DisplayElements { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public FullLayout Layout { get; set; } = null!;
	
	public void OnInitialize()
	{
		DisplayElements.AddChild(BoxOfHierarchy(HierarchyToDisplay));
	}
	
	private static FullLayoutWithHierarchy BoxOfHierarchy(object? obj)
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
		text.TextString = obj switch
		{
			INamed named => named.Name,
			not null => obj.GetType().Name + " (no name)",
			_ => "(no prefab loaded)"
		};
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

[SerializeExposedMembers, Prefab("Prefabs/Cube.toml")]
public partial class Cube : INotificationPropagator, INamed, IHasChildren, ICanAddChildren
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Transform3D Transform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public PBRMeshRenderer MeshRenderer { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, MeshRenderer);
	}
}

[SerializeExposedMembers, Prefab("Prefabs/Editor.toml")]
public partial class Editor : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren, ICanAddChildren
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectLayout RectLayout { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	[SerializedReference] public Viewport Viewport { get; set; } = null!;
	[SerializedReference] public HierarchyDisplay HierarchyDisplay { get; set; } = null!;
	[SerializedValue] public string PrefabPath { get; set; } = "";
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, RectLayout, PrefabChildren);
	}
}

[SerializeExposedMembers, Prefab("Prefabs/EditorViewportGui.toml")]
public partial class EditorViewportGui : Viewport, INotificationPropagator, ICanBeLaidOut, INamed, IMouseDown, IMouseMoved, IMouseUp, IMouseWheelScrolled
{
	[SerializedReference, ExposeMembersInClass] public ViewportGui Viewport { get; set; } = null!;
	
	[SerializedValue] public double ZoomFactor = 0.1;
	
	private bool isDragging;
	
	public void Notify<T>(T notification) where T : notnull
	{
		if (notification is IMouseDown.Notification || notification is IMouseMoved.Notification || notification is IMouseUp.Notification)
			INotificationPropagator.Notify(new EditorNotification<T>(notification), Viewport);
		else
			INotificationPropagator.Notify(notification, Viewport);
	}
	
	public void OnMouseDown(MouseButton button, Vector2 position, Vector3 globalPosition)
	{
		if (button == MouseButton.Left && RectTransform.ContainsGlobalPoint(globalPosition.XY))
			isDragging = true;
	}
	
	public void OnMouseMoved(Vector2 position, Vector2 delta, Vector3 globalPosition)
	{
		if (isDragging)
			Camera.GlobalPosition += Camera.Zoom * -delta;
	}
	
	public void OnMouseUp(MouseButton button, Vector2 position, Vector3 globalPosition)
	{
		isDragging = false;
	}
	
	public void OnMouseWheelScrolled(Vector2 offset, Vector2 position, Vector3 globalPosition)
	{
		if (RectTransform.ContainsGlobalPoint(globalPosition.XY))
		{
			double zoomAmount = 1 + Math.Abs(offset.Y) * ZoomFactor;
			if (offset.Y > 0)
				Camera.Zoom *= zoomAmount;
			else
				Camera.Zoom /= zoomAmount;
		}
	}
}
using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Mathematics;
using Echidna2.Rendering;
using Echidna2.Rendering3D;
using Echidna2.Serialization;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Echidna2;

[SerializeExposedMembers, Prefab("Prefabs/HierarchyDisplay.toml")]
public partial class HierarchyDisplay : INotificationPropagator, ICanBeLaidOut
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference] private Hierarchy DisplayElements { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public FullLayout Layout { get; set; } = null!;
	
	private IHasChildren? hierarchyToDisplay = null;
	[SerializedReference] public IHasChildren? HierarchyToDisplay
	{
		get => hierarchyToDisplay;
		set
		{
			DisplayElements.ClearChildren();
			hierarchyToDisplay = value;
			if (hierarchyToDisplay is not null)
				DisplayElements.AddChild(BoxOfHierarchy(hierarchyToDisplay));
		}
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
public partial class Cube
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
public partial class Editor : ICanBeLaidOut
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectLayout RectLayout { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	[SerializedReference] public Viewport Viewport { get; set; } = null!;
	[SerializedReference] public ObjectPanel ObjectPanel { get; set; } = null!;
	[SerializedValue] public string PrefabPath { get; set; } = "";
	
	private object? prefab;
	public object? Prefab
	{
		get => prefab;
		set
		{
			Viewport.ClearChildren();
			prefab = value;
			ObjectPanel.Prefab = value;
			if (prefab is not null)
				Viewport.AddChild(prefab);
		}
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, RectLayout, PrefabChildren);
	}
}

[SerializeExposedMembers, Prefab("Prefabs/EditorViewportGui.toml")]
public partial class EditorViewportGui : IMouseDown, IMouseMoved, IMouseUp, IMouseWheelScrolled
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

[SerializeExposedMembers, Prefab("Prefabs/EditorViewport3D.toml")]
public partial class EditorViewport3D : IMouseDown, IMouseMoved, IMouseUp, IMouseWheelScrolled, IKeyDown, IKeyUp
{
	[SerializedReference, ExposeMembersInClass] public Viewport3D Viewport { get; set; } = null!;
	[SerializedReference] public Transform3D CameraPivot { get; set; } = null!;
	
	[SerializedValue] public double OrbitFactor = 0.01;
	[SerializedValue] public double ZoomFactor = 0.1;
	[SerializedValue] public double PanFactor = 0.01;
	
	private double cameraPitch = Math.PI / 4;
	private double cameraYaw = 0;
	private double cameraDistance = 5;
	
	private bool isOrbiting;
	private bool isPanning;
	private bool isModifierPressed;
	
	public void Notify<T>(T notification) where T : notnull
	{
		if (notification is IMouseDown.Notification || notification is IMouseMoved.Notification || notification is IMouseUp.Notification)
			INotificationPropagator.Notify(new EditorNotification<T>(notification), Viewport);
		else
			INotificationPropagator.Notify(notification, Viewport);
	}
	
	public void OnUpdate(double deltaTime)
	{
		Viewport.Camera.Transform.GlobalPosition = CameraPivot.GlobalPosition + cameraDistance * new Vector3(
			Math.Cos(cameraPitch) * Math.Cos(cameraYaw),
			Math.Cos(cameraPitch) * Math.Sin(cameraYaw),
			Math.Sin(cameraPitch)
		);
		Viewport.Camera.Transform.LookAt(CameraPivot, Math.Cos(cameraPitch) >= 0 ? Vector3.Up : Vector3.Down, Vector3.North);
	}
	
	public void OnMouseDown(MouseButton button, Vector2 position, Vector3 globalPosition)
	{
		if (button == MouseButton.Left && RectTransform.ContainsGlobalPoint(globalPosition.XY))
			if (isModifierPressed)
				isPanning = true;
			else
				isOrbiting = true;
	}
	
	public void OnMouseMoved(Vector2 position, Vector2 delta, Vector3 globalPosition)
	{
		if (isOrbiting)
		{
			cameraYaw -= delta.X * OrbitFactor;
			cameraPitch -= delta.Y * OrbitFactor;
		}
		else if (isPanning)
		{
			CameraPivot.GlobalPosition +=
				-delta.X * PanFactor * cameraDistance * Viewport.Camera.Transform.GlobalTransform.Right +
				-delta.Y * PanFactor * cameraDistance * Viewport.Camera.Transform.GlobalTransform.Forward;
		}
	}
	
	public void OnMouseUp(MouseButton button, Vector2 position, Vector3 globalPosition)
	{
		if (button == MouseButton.Left)
		{
			isOrbiting = false;
			isPanning = false;
		}
	}
	
	public void OnMouseWheelScrolled(Vector2 offset, Vector2 position, Vector3 globalPosition)
	{
		if (RectTransform.ContainsGlobalPoint(globalPosition.XY))
		{
			double zoomAmount = 1 + Math.Abs(offset.Y) * ZoomFactor;
			if (offset.Y > 0)
				cameraDistance *= zoomAmount;
			else
				cameraDistance /= zoomAmount;
		}
	}
	
	public void OnKeyDown(Keys key)
	{
		if (key is Keys.LeftShift)
			isModifierPressed = true;
	}
	
	public void OnKeyUp(Keys key)
	{
		if (key is Keys.LeftShift)
			isModifierPressed = false;
	}
}

[SerializeExposedMembers, Prefab("Prefabs/ObjectPanel.toml")]
public partial class ObjectPanel
{
	[SerializedReference, ExposeMembersInClass] public FullRectWithHierarchy Rect { get; set; } = null!;
	[SerializedReference] public HierarchyDisplay HierarchyDisplay { get; set; } = null!;
	
	private object? prefab;
	public object? Prefab
	{
		get => prefab;
		set
		{
			prefab = value;
			HierarchyDisplay.HierarchyToDisplay = prefab as IHasChildren;
		}
	}
}
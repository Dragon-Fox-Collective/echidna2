﻿using System.Drawing;
using System.Reflection;
using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Mathematics;
using Echidna2.Rendering;
using Echidna2.Rendering3D;
using Echidna2.Serialization;
using JetBrains.Annotations;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Echidna2;

[UsedImplicitly, SerializeExposedMembers, Prefab("Prefabs/HierarchyDisplay.toml")]
public partial class HierarchyDisplay : INotificationPropagator, ICanBeLaidOut
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference] private Hierarchy DisplayElements { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public FullLayout Layout { get; set; } = null!;
	
	public event Action<object>? ItemSelected;
	
	private IHasChildren? hierarchyToDisplay = null;
	[SerializedReference] public IHasChildren? HierarchyToDisplay
	{
		get => hierarchyToDisplay;
		set
		{
			DisplayElements.ClearChildren();
			hierarchyToDisplay = value;
			if (hierarchyToDisplay is not null)
			{
				FullLayoutWithHierarchy box = BoxOfHierarchy(hierarchyToDisplay);
				box.LeftMargin = 0;
				DisplayElements.AddChild(box);
			}
		}
	}
	
	private FullLayoutWithHierarchy BoxOfHierarchy(object obj)
	{
		FullLayoutWithHierarchy box = FullLayoutWithHierarchy.Instantiate();
		box.Name = $"Box for {obj}";
		box.AnchorPreset = AnchorPreset.Full;
		box.LeftMargin = 10;
		VLayoutWithHierarchy layout = VLayoutWithHierarchy.Instantiate();
		layout.Name = $"Layout for box for {obj}";
		layout.AnchorPreset = AnchorPreset.Full;
		box.AddChild(layout);
		
		ButtonRect button = ButtonRect.Instantiate();
		button.AnchorPreset = AnchorPreset.Full;
		button.Margin = 5;
		button.MouseUp += () => ItemSelected?.Invoke(obj);
		layout.AddChild(button);
		
		TextRect text = TextRect.Instantiate();
		text.TextString = INamed.GetName(obj);
		text.Justification = TextJustification.Left;
		text.AnchorPreset = AnchorPreset.Full;
		text.MinimumSize = (0, 25);
		text.LocalScale = Vector2.One * 0.5;
		button.AddChild(text);
		
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

[UsedImplicitly, SerializeExposedMembers, Prefab("Prefabs/Cube.toml")]
public partial class Cube : INotificationPropagator
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

[UsedImplicitly, SerializeExposedMembers, Prefab("Prefabs/Editor.toml")]
public partial class Editor : INotificationPropagator, ICanBeLaidOut
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectLayout RectLayout { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	[SerializedReference] public Viewport Viewport { get; set; } = null!;
	[SerializedReference] public ComponentPanel ComponentPanel { get; set; } = null!;
	[SerializedValue] public string PrefabPath { get; set; } = "";
	
	private ObjectPanel? objectPanel;
	[SerializedReference] public ObjectPanel ObjectPanel
	{
		get => objectPanel!;
		set
		{
			if (objectPanel is not null)
				objectPanel.ItemSelected -= OnObjectSelected;
			
			objectPanel = value;
			
			if (objectPanel is not null)
				objectPanel.ItemSelected += OnObjectSelected;
		}
	}
	
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
	
	private Dictionary<Type, Func<IFieldEditor>> editorInstantiators = new()
	{
		{ typeof(string), StringFieldEditor.Instantiate },
		{ typeof(double), DoubleFieldEditor.Instantiate }
	};
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, RectLayout, PrefabChildren);
	}
	
	public void OnObjectSelected(object obj)
	{
		Console.WriteLine("Selected " + obj);
		ComponentPanel.SelectedObject = obj;
	}
	
	public void RegisterFieldEditor<TFieldType, TFieldEditor>() where TFieldEditor : IFieldEditor<TFieldType> => editorInstantiators.Add(typeof(TFieldType), TFieldEditor.Instantiate);
	public IFieldEditor InstantiateFieldEditor(Type type) => editorInstantiators[type]();
	public bool HasRegisteredFieldEditor(Type type) => editorInstantiators.ContainsKey(type);
}

[UsedImplicitly, SerializeExposedMembers, Prefab("Prefabs/EditorViewportGui.toml")]
public partial class EditorViewportGui : INotificationPropagator, IMouseDown, IMouseMoved, IMouseUp, IMouseWheelScrolled
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

[UsedImplicitly, SerializeExposedMembers, Prefab("Prefabs/EditorViewport3D.toml")]
public partial class EditorViewport3D : INotificationPropagator, IUpdate, IMouseDown, IMouseMoved, IMouseUp, IMouseWheelScrolled, IKeyDown, IKeyUp
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

[UsedImplicitly, SerializeExposedMembers, Prefab("Prefabs/ObjectPanel.toml")]
public partial class ObjectPanel : INotificationPropagator
{
	[SerializedReference, ExposeMembersInClass] public FullRectWithHierarchy Rect { get; set; } = null!;
	[SerializedReference] public HierarchyDisplay HierarchyDisplay { get; set; } = null!;
	
	public event Action<object> ItemSelected
	{
		add => HierarchyDisplay.ItemSelected += value;
		remove => HierarchyDisplay.ItemSelected -= value;
	}
	
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
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect);
	}
}

[UsedImplicitly, SerializeExposedMembers, Prefab("Prefabs/ComponentPanel.toml")]
public partial class ComponentPanel : INotificationPropagator, IEditorInitialize
{
	[SerializedReference, ExposeMembersInClass] public FullRectWithHierarchy Rect { get; set; } = null!;
	[SerializedReference] public TextRect NameText { get; set; } = null!;
	[SerializedReference] public ICanAddChildren Fields { get; set; } = null!;
	
	private Editor editor = null!;
	
	private object? selectedObject;
	public object? SelectedObject
	{
		get => selectedObject;
		set
		{
			selectedObject = value;
			if (selectedObject is not null)
			{
				if (selectedObject is INamed named)
					named.NameChanged += name => NameText.TextString = name;
				NameText.TextString = INamed.GetName(selectedObject);
			}
			else
			{
				NameText.TextString = "(no object selected)";
			}
			RefreshFields();
		}
	}
	
	public void RefreshFields()
	{
		Fields.ClearChildren();
		if (selectedObject is null) return;
		
		foreach (MemberInfo memberInfo in selectedObject.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(member => member.GetCustomAttribute<SerializedValueAttribute>() is not null))
		{
			HLayoutWithHierarchy layout = HLayoutWithHierarchy.Instantiate();
			Fields.AddChild(layout);
			
			TextRect text = TextRect.Instantiate();
			text.TextString = memberInfo.Name;
			text.LocalScale = (0.5, 0.5);
			text.MinimumSize = (150, 25);
			text.Justification = TextJustification.Left;
			layout.AddChild(text);
			
			if (memberInfo is FieldInfo fieldInfo)
			{
				if (editor.HasRegisteredFieldEditor(fieldInfo.FieldType))
				{
					IFieldEditor fieldEditor = editor.InstantiateFieldEditor(fieldInfo.FieldType);
					fieldEditor.Load(fieldInfo.GetValue(selectedObject)!);
					fieldEditor.ValueChanged += value => fieldInfo.SetValue(selectedObject, value);
					layout.AddChild(fieldEditor);
				}
			}
			else if (memberInfo is PropertyInfo propertyInfo)
			{
				if (editor.HasRegisteredFieldEditor(propertyInfo.PropertyType))
				{
					IFieldEditor fieldEditor = editor.InstantiateFieldEditor(propertyInfo.PropertyType);
					fieldEditor.Load(propertyInfo.GetValue(selectedObject)!);
					fieldEditor.ValueChanged += value => propertyInfo.SetValue(selectedObject, value);
					layout.AddChild(fieldEditor);
				}
			}
		}
	}
	
	public void OnEditorInitialize(Editor editor) => this.editor = editor;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect);
	}
}
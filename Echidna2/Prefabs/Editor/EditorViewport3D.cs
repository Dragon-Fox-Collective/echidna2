using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Mathematics;
using Echidna2.Rendering;
using Echidna2.Rendering3D;
using Echidna2.Serialization;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Echidna2.Prefabs.Editor;

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
		if (button == MouseButton.Left && this.ContainsGlobalPoint(globalPosition.XY))
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
		if (this.ContainsGlobalPoint(globalPosition.XY))
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
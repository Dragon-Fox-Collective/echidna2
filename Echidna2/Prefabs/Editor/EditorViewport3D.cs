using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Mathematics;
using Echidna2.Rendering;
using Echidna2.Rendering3D;
using Echidna2.Serialization;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Echidna2.Prefabs.Editor;

public partial class EditorViewport3D : Viewport, INotificationPropagator, INotificationListener<UpdateNotification>, INotificationListener<MouseDownNotification>, INotificationListener<MouseMovedNotification>, INotificationListener<MouseUpNotification>, INotificationListener<MouseWheelScrolledNotification>, INotificationListener<KeyDownNotification>, INotificationListener<KeyUpNotification>
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
		if (notification is MouseDownNotification or MouseMovedNotification or MouseUpNotification)
			INotificationPropagator.Notify(Activator.CreateInstance(Project.Singleton.Assembly.GetType("Notifications.Editor.EditorNotificationNotification`1").MakeGenericType(typeof(T)), notification), Viewport);
		else
			INotificationPropagator.Notify(notification, Viewport);
	}
	
	public void OnNotify(UpdateNotification notification)
	{
		Viewport.Camera.Transform.GlobalPosition = CameraPivot.GlobalPosition + cameraDistance * new Vector3(
			Math.Cos(cameraPitch) * Math.Cos(cameraYaw),
			Math.Cos(cameraPitch) * Math.Sin(cameraYaw),
			Math.Sin(cameraPitch)
		);
		Viewport.Camera.Transform.LookAt(CameraPivot, Math.Cos(cameraPitch) >= 0 ? Vector3.Up : Vector3.Down, Vector3.North);
	}
	
	public void OnNotify(MouseDownNotification notification)
	{
		if (notification.Button == MouseButton.Left && !notification.Clipped && this.ContainsGlobalPoint(notification.GlobalPosition.XY))
			if (isModifierPressed)
				isPanning = true;
			else
				isOrbiting = true;
	}
	
	public void OnNotify(MouseMovedNotification notification)
	{
		if (isOrbiting)
		{
			cameraYaw -= notification.GlobalDelta.X * OrbitFactor;
			cameraPitch -= notification.GlobalDelta.Y * OrbitFactor;
		}
		else if (isPanning)
		{
			CameraPivot.GlobalPosition +=
				-notification.GlobalDelta.X * PanFactor * cameraDistance * Viewport.Camera.Transform.GlobalTransform.Right +
				-notification.GlobalDelta.Y * PanFactor * cameraDistance * Viewport.Camera.Transform.GlobalTransform.Forward;
		}
	}
	
	public void OnNotify(MouseUpNotification notification)
	{
		if (notification.Button == MouseButton.Left)
		{
			isOrbiting = false;
			isPanning = false;
		}
	}
	
	public void OnNotify(MouseWheelScrolledNotification notification)
	{
		if (this.ContainsGlobalPoint(notification.GlobalPosition.XY))
		{
			double zoomAmount = 1 + Math.Abs(notification.Offset.Y) * ZoomFactor;
			if (notification.Offset.Y > 0)
				cameraDistance /= zoomAmount;
			else
				cameraDistance *= zoomAmount;
		}
	}
	
	public void OnNotify(KeyDownNotification notification)
	{
		if (notification.Key is Keys.LeftShift)
			isModifierPressed = true;
	}
	
	public void OnNotify(KeyUpNotification notification)
	{
		if (notification.Key is Keys.LeftShift)
			isModifierPressed = false;
	}
}
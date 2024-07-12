using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Mathematics;
using Echidna2.Rendering;
using Echidna2.Serialization;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Echidna2.Prefabs.Editor;

public partial class EditorViewportGui : INotificationPropagator, INotificationListener<MouseDownNotification>, INotificationListener<MouseMovedNotification>, INotificationListener<MouseUpNotification>, INotificationListener<MouseWheelScrolledNotification>
{
	[SerializedReference, ExposeMembersInClass] public ViewportGui Viewport { get; set; } = null!;
	
	[SerializedValue] public double ZoomFactor = 0.1;
	
	private bool isDragging;
	
	public void Notify<T>(T notification) where T : notnull
	{
		if (notification is MouseDownNotification || notification is MouseMovedNotification || notification is MouseUpNotification)
			INotificationPropagator.Notify(Activator.CreateInstance(Project.Singleton.Assembly.GetType("Notifications.Editor.EditorNotificationNotification`1").MakeGenericType(typeof(T)), notification), Viewport);
		else
			INotificationPropagator.Notify(notification, Viewport);
	}
	
	public void OnNotify(MouseDownNotification notification)
	{
		if (notification is { Button: MouseButton.Left, Clipped: false } && this.ContainsGlobalPoint(notification.GlobalPosition.XY))
			isDragging = true;
	}
	
	public void OnNotify(MouseMovedNotification notification)
	{
		if (isDragging)
			Camera.GlobalPosition += Camera.Zoom * -notification.GlobalDelta.XY;
	}
	
	public void OnNotify(MouseUpNotification notification)
	{
		isDragging = false;
	}
	
	public void OnNotify(MouseWheelScrolledNotification notification)
	{
		if (this.ContainsGlobalPoint(notification.GlobalPosition.XY))
		{
			double zoomAmount = 1 + Math.Abs(notification.Offset.Y) * ZoomFactor;
			if (notification.Offset.Y > 0)
				Camera.Zoom /= zoomAmount;
			else
				Camera.Zoom *= zoomAmount;
		}
	}
}
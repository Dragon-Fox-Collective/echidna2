using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Rendering;
using Echidna2.Serialization;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Echidna2.Gui;

public class Button : INotificationListener<MouseDownNotification>, INotificationListener<MouseUpNotification>, INotificationListener<MouseMovedNotification>
{
	public event Action<MouseUpNotification>? Clicked;
	public event Action<MouseMovedNotification>? Dragged;
	public event Action<MouseDownNotification>? MouseDown;
	public event Action<MouseDownNotification>? MouseDownOutside;
	public event Action<MouseUpNotification>? MouseUp;
	public event Action<MouseUpNotification>? MouseUpOutside;
	public event Action<MouseMovedNotification>? MouseEnter;
	public event Action<MouseMovedNotification>? MouseExit;
	
	[SerializedReference] public IRectTransform RectTransform { get; set; } = null!;
	
	private bool wasPressed;
	private bool wasInside;
	
	private Vector3 lastGlobalPosition;
	
	public void OnNotify(MouseDownNotification notification)
	{
		if (notification.Button != MouseButton.Left) return;
		
		if (!notification.Clipped && RectTransform.ContainsGlobalPoint(notification.GlobalPosition.XY))
		{
			wasPressed = true;
			MouseDown?.Invoke(notification);
		}
		else
		{
			MouseDownOutside?.Invoke(notification);
		}
	}
	
	public void OnNotify(MouseUpNotification notification)
	{
		if (notification.Button != MouseButton.Left) return;
		
		if (wasPressed)
		{
			wasPressed = false;
			MouseUp?.Invoke(notification);
			
			if (RectTransform.ContainsGlobalPoint(notification.GlobalPosition.XY))
				Clicked?.Invoke(notification);
		}
		else
		{
			MouseUpOutside?.Invoke(notification);
		}
	}
	
	public void OnNotify(MouseMovedNotification notification)
	{
		if (wasPressed)
			Dragged?.Invoke(notification);
		
		if (!notification.Clipped && RectTransform.ContainsGlobalPoint(notification.GlobalPosition.XY))
		{
			if (!wasInside)
			{
				wasInside = true;
				MouseEnter?.Invoke(notification);
			}
		}
		else
		{
			if (wasInside)
			{
				wasInside = false;
				MouseExit?.Invoke(notification);
			}
		}
		
		lastGlobalPosition = notification.GlobalPosition;
	}
}
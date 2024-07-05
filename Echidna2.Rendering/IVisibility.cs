using Echidna2.Core;
using Echidna2.Serialization;

namespace Echidna2.Rendering;

public interface IVisibility
{
	public bool IsSelfVisible { get; set; }
	public bool IsVisible { get; }
}

public static class IVisibilityExtensions
{
	public static void ToggleVisibility(this IVisibility visibility) => visibility.IsSelfVisible = !visibility.IsSelfVisible;
	public static void Show(this IVisibility visibility) => visibility.IsSelfVisible = true;
	public static void Hide(this IVisibility visibility) => visibility.IsSelfVisible = false;
}

public class Visibility : IVisibility,
	INotificationPredicate<Draw_Notification>,
	INotificationPredicate<ParentVisibilityChanged_Notification>,
	INotificationPredicate<MouseDown_Notification>,
	INotificationPredicate<MouseUp_Notification>,
	INotificationPredicate<MouseMoved_Notification>
{
	[SerializedReference] public INotificationPropagator NotificationPropagator = null!;
	
	private bool isSelfVisible = true;
	private bool isParentVisible = true;
	public bool IsVisible => isSelfVisible && isParentVisible;
	[SerializedValue] public bool IsSelfVisible
	{
		get => isSelfVisible;
		set
		{
			bool wasVisible = IsVisible;
			isSelfVisible = value;
			if (wasVisible != IsVisible)
				NotificationPropagator.Notify(new ParentVisibilityChanged_Notification(IsVisible));
		}
	}
	
	public bool ShouldNotificationPropagate(Draw_Notification notification) => IsVisible;
	public bool ShouldNotificationPropagate(MouseDown_Notification notification) => IsVisible;
	public bool ShouldNotificationPropagate(MouseUp_Notification notification) => IsVisible;
	public bool ShouldNotificationPropagate(MouseMoved_Notification notification) => IsVisible;
	
	public bool ShouldNotificationPropagate(ParentVisibilityChanged_Notification notification)
	{
		isParentVisible = notification.Visible;
		return isSelfVisible;
	}
}

public class ParentVisibilityChanged_Notification(bool visible)
{
	public bool Visible { get; } = visible;
}
public interface IParentVisibilityChanged : INotificationListener<ParentVisibilityChanged_Notification>
{
	void INotificationListener<ParentVisibilityChanged_Notification>.OnNotify(ParentVisibilityChanged_Notification notification) => OnParentVisibilityChanged(notification.Visible);
	public void OnParentVisibilityChanged(bool visible);
}
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
	INotificationPredicate<DrawNotification>,
	INotificationPredicate<ParentVisibilityChangedNotification>,
	INotificationPredicate<MouseDownNotification>,
	INotificationPredicate<MouseUpNotification>,
	INotificationPredicate<MouseMovedNotification>
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
				NotificationPropagator.Notify(new ParentVisibilityChangedNotification(IsVisible));
		}
	}
	
	public bool ShouldNotificationPropagate(DrawNotification notification) => IsVisible;
	public bool ShouldNotificationPropagate(MouseDownNotification notification) => IsVisible;
	public bool ShouldNotificationPropagate(MouseUpNotification notification) => IsVisible;
	public bool ShouldNotificationPropagate(MouseMovedNotification notification) => IsVisible;
	
	public bool ShouldNotificationPropagate(ParentVisibilityChangedNotification notification)
	{
		isParentVisible = notification.Visible;
		return isSelfVisible;
	}
}

public class ParentVisibilityChangedNotification(bool visible)
{
	public bool Visible { get; } = visible;
}
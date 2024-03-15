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
	INotificationPredicate<IDraw.Notification>,
	INotificationPredicate<IParentVisibilityChanged.Notification>,
	INotificationPredicate<IMouseDown.Notification>,
	INotificationPredicate<IMouseUp.Notification>,
	INotificationPredicate<IMouseMoved.Notification>
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
				NotificationPropagator.Notify(new IParentVisibilityChanged.Notification(IsVisible));
		}
	}
	
	public bool ShouldNotificationPropagate(IDraw.Notification notification) => IsVisible;
	public bool ShouldNotificationPropagate(IMouseDown.Notification notification) => IsVisible;
	public bool ShouldNotificationPropagate(IMouseUp.Notification notification) => IsVisible;
	public bool ShouldNotificationPropagate(IMouseMoved.Notification notification) => IsVisible;
	
	public bool ShouldNotificationPropagate(IParentVisibilityChanged.Notification notification)
	{
		isParentVisible = notification.Visible;
		return isSelfVisible;
	}
}

public interface IParentVisibilityChanged : INotificationListener<IParentVisibilityChanged.Notification>
{
	public class Notification(bool visible)
	{
		public bool Visible { get; } = visible;
	}
	void INotificationListener<Notification>.OnNotify(Notification notification) => OnParentVisibilityChanged(notification.Visible);
	public void OnParentVisibilityChanged(bool visible);
}
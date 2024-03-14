using Echidna2.Core;
using Echidna2.Serialization;

namespace Echidna2.Rendering;

public interface IVisibility
{
	public bool SelfIsVisible { get; set; }
	public bool IsVisible { get; }
}

public static class IVisibleExtensions
{
	public static void ToggleVisibility(this IVisibility visibility) => visibility.SelfIsVisible = !visibility.SelfIsVisible;
	public static void Show(this IVisibility visibility) => visibility.SelfIsVisible = true;
	public static void Hide(this IVisibility visibility) => visibility.SelfIsVisible = false;
}

public class Visibility : IVisibility, INotificationPredicate<IDraw.Notification>, INotificationPredicate<IParentVisibilityChanged.Notification>
{
	[SerializedReference] public INotificationPropagator NotificationPropagator = null!;
	
	private bool selfIsVisible = true;
	private bool parentIsVisible = true;
	public bool IsVisible => selfIsVisible && parentIsVisible;
	public bool SelfIsVisible
	{
		get => selfIsVisible;
		set
		{
			bool wasVisible = IsVisible;
			selfIsVisible = value;
			if (wasVisible != IsVisible)
				NotificationPropagator.Notify(new IParentVisibilityChanged.Notification(IsVisible));
		}
	}
	
	public bool ShouldNotificationPropagate(IDraw.Notification notification) => IsVisible;
	
	public bool ShouldNotificationPropagate(IParentVisibilityChanged.Notification notification)
	{
		parentIsVisible = notification.Visible;
		return selfIsVisible;
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
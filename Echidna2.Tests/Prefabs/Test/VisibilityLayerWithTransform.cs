using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Rendering;

namespace Echidna2.Prefabs.Test;

public partial class VisibilityLayerWithTransform
{
	[ExposeMembersInClass] public RectTransform RectTransform { get; set; }
	[ExposeMembersInClass] public Visibility Visibility { get; set; }
	[ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; }
	
	public VisibilityLayerWithTransform()
	{
		RectTransform = new RectTransform();
		PrefabChildren = new Hierarchy();
		Visibility = new Visibility
		{
			NotificationPropagator = PrefabChildren,
		};
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Visibility, PrefabChildren);
	}
}
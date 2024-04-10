using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Rendering;
using Echidna2.Rendering3D;
using Echidna2.Serialization;

namespace Echidna2.TestPrefabs;

public partial class ButtonWithTransform : INotificationPropagator
{
	[ExposeMembersInClass] public RectTransform RectTransform { get; set; }
	[ExposeMembersInClass] public Button Button { get; set; }
	
	public ButtonWithTransform()
	{
		RectTransform = new RectTransform
		{
			LocalSize = (10, 10),
		};
		Button = new Button
		{
			RectTransform = RectTransform,
		};
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Button);
	}
}

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

public partial class TransformPrefab
{
	[SerializedReference, ExposeMembersInClass] public Transform3D Transform { get; set; } = null!;
}

public partial class SubcomponentPrefab
{
	[SerializedReference, ExposeMembersInClass] public SubcomponentSubcomponent Subcomponent { get; set; } = null!;
}

public partial class SubcomponentSubcomponent
{
	[SerializedReference, ExposeMembersInClass] public object? Reference { get; set; } = null!;
}
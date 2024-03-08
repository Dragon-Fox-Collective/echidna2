using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Rendering;
using Echidna2.Serialization;
using Color = System.Drawing.Color;

namespace Echidna2;


[SerializeExposedMembers, Prefab("Prefabs/RectWithHierarchy.toml")]
public partial class RectWithHierarchy : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren, ICanAddChildren
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectLayout Layout { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Rect Rect { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect, Layout, PrefabChildren);
	}
}


[SerializeExposedMembers, Prefab("Prefabs/FullRectWithHierarchy.toml")]
public partial class FullRectWithHierarchy : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren, ICanAddChildren
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public FullLayout Layout { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Rect Rect { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect, Layout, PrefabChildren);
	}
}


[SerializeExposedMembers, Prefab("Prefabs/RectLayoutWithHierarchy.toml")]
public partial class RectLayoutWithHierarchy : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren, ICanAddChildren
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectLayout Layout { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Layout, PrefabChildren);
	}
}


[SerializeExposedMembers, Prefab("Prefabs/ButtonRect.toml")]
public partial class ButtonRect : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren, ICanAddChildren, IInitialize
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectLayout Layout { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Rect Rect { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Button Button { get; set; } = null!;
	
	public void OnInitialize()
	{
		Button.MouseDown += () => Rect.Color = Color.DarkGray;
		Button.MouseUp += () => Rect.Color = Color.LightGray;
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect, Button, Layout, PrefabChildren);
	}
}


[SerializeExposedMembers, Prefab("Prefabs/Named.toml")]
public partial class HLayoutWithHierarchy : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren, ICanAddChildren
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public HorizontalLayout Layout { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Layout, PrefabChildren);
	}
}


[SerializeExposedMembers, Prefab("Prefabs/VLayoutWithHierarchy.toml")]
public partial class VLayoutWithHierarchy : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren, ICanAddChildren
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public VerticalLayout Layout { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Layout, PrefabChildren);
	}
}


[SerializeExposedMembers, Prefab("Prefabs/TextRect.toml")]
public partial class TextRect : INotificationPropagator, ICanBeLaidOut, INamed
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Text Text { get; set; } = null!;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Text);
	}
}


[SerializeExposedMembers, Prefab("Prefabs/FullLayoutWithHierarchy.toml")]
public partial class FullLayoutWithHierarchy : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren, ICanAddChildren
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public FullLayout Layout { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Layout, PrefabChildren);
	}
}
using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Mathematics;
using Echidna2.Rendering;
using Echidna2.Serialization;
using Color = System.Drawing.Color;

namespace Echidna2;


[SerializeExposedMembers]
public partial class RectWithHierarchy : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren, ICanAddChildren
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectLayout RectLayout { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Rect Rect { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect, RectLayout, PrefabChildren);
	}
}


[SerializeExposedMembers]
public partial class ButtonRect : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren, ICanAddChildren, IInitialize
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectLayout RectLayout { get; set; } = null!;
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
		INotificationPropagator.Notify(notification, Rect, Button, RectLayout, PrefabChildren);
	}
}


[SerializeExposedMembers]
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


[SerializeExposedMembers]
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


[SerializeExposedMembers]
public partial class HierarchyDisplay : INotificationPropagator, ICanBeLaidOut, INamed, IInitialize
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference] public IHasChildren HierarchyToDisplay { get; set; } = null!;
	[SerializedReference] private Hierarchy DisplayElements { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public FullLayout Layout { get; set; } = null!;
	
	public void OnInitialize()
	{
		DisplayElements.AddChild(BoxOfHierarchy(HierarchyToDisplay));
	}
	
	private static FullLayoutWithHierarchy BoxOfHierarchy(object obj)
	{
		FullLayoutWithHierarchy box = FullLayoutWithHierarchy.Instantiate();
		box.Name = $"Box for {obj}";
		box.AnchorPreset = AnchorPreset.Full;
		box.LeftMargin = 10;
		VLayoutWithHierarchy layout = VLayoutWithHierarchy.Instantiate();
		layout.Name = $"Layout for box for {obj}";
		layout.AnchorPreset = AnchorPreset.Full;
		box.AddChild(layout);
		
		TextRect text = new()
		{
			TextString = obj is INamed named ? named.Name : obj.GetType().Name + " (no name)",
			AnchorPreset = AnchorPreset.Full,
			LocalScale = Vector2.One * 0.5,
			MinimumSize = (0, 25),
			Justification = TextJustification.Left,
		};
		layout.AddChild(text);
		
		if (obj is IHasChildren hasChildren)
			foreach (object child in hasChildren.Children)
				layout.AddChild(BoxOfHierarchy(child));
		
		return box;
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Layout, DisplayElements);
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


[SerializeExposedMembers]
public partial class DisplayOnlyLayer : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren, ICanAddChildren
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectLayout Layout { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	
	public void Notify<T>(T notification) where T : notnull
	{
		if (notification is IPreUpdate.Notification or IUpdate.Notification or IDraw.Notification)
			INotificationPropagator.Notify(notification, Layout, PrefabChildren);
	}
}

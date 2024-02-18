using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Mathematics;
using Echidna2.Rendering;
using Color = System.Drawing.Color;

namespace Echidna2;

public partial class RectWithHierarchy : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren, ICanAddChildren
{
	[ExposeMembersInClass] public Named Named { get; set; }
	[ExposeMembersInClass] public RectTransform RectTransform { get; set; }
	[ExposeMembersInClass] public RectLayout RectLayout { get; set; }
	[ExposeMembersInClass] public Rect Rect { get; set; }
	[ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; }
	
	public RectWithHierarchy()
	{
		Named = new Named("Rect With Hierarchy");
		PrefabChildren = new Hierarchy();
		RectTransform = new RectTransform();
		RectLayout = new RectLayout(RectTransform, PrefabChildren);
		Rect = new Rect(RectTransform);
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect, RectLayout, PrefabChildren);
	}
}


public partial class ButtonRect : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren, ICanAddChildren
{
	[ExposeMembersInClass] public Named Named { get; set; }
	[ExposeMembersInClass] public RectTransform RectTransform { get; set; }
	[ExposeMembersInClass] public RectLayout RectLayout { get; set; }
	[ExposeMembersInClass] public Rect Rect { get; set; }
	[ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; }
	[ExposeMembersInClass] public Button Button { get; set; }
	
	public ButtonRect()
	{
		Named = new Named("Rect With Hierarchy");
		PrefabChildren = new Hierarchy();
		RectTransform = new RectTransform();
		RectLayout = new RectLayout(RectTransform, PrefabChildren);
		Rect = new Rect(RectTransform);
		Button = new Button(RectTransform);
		
		Rect.Color = Color.LightGray;
		Button.MouseDown += () => Rect.Color = Color.DarkGray;
		Button.MouseUp += () => Rect.Color = Color.LightGray;
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect, Button, RectLayout, PrefabChildren);
	}
}


public partial class HLayoutWithHierarchy : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren, ICanAddChildren
{
	[ExposeMembersInClass] public Named Named { get; set; }
	[ExposeMembersInClass] public RectTransform RectTransform { get; set; }
	[ExposeMembersInClass] public HorizontalLayout Layout { get; set; }
	[ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; }
	
	public HLayoutWithHierarchy()
	{
		Named = new Named("Horizontal Layout");
		PrefabChildren = new Hierarchy();
		RectTransform = new RectTransform();
		Layout = new HorizontalLayout(RectTransform, PrefabChildren);
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Layout, PrefabChildren);
	}
}


public partial class VLayoutWithHierarchy : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren, ICanAddChildren
{
	[ExposeMembersInClass] public Named Named { get; set; }
	[ExposeMembersInClass] public RectTransform RectTransform { get; set; }
	[ExposeMembersInClass] public VerticalLayout Layout { get; set; }
	[ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; }
	
	public VLayoutWithHierarchy()
	{
		Named = new Named("Vertical Layout");
		PrefabChildren = new Hierarchy();
		RectTransform = new RectTransform();
		Layout = new VerticalLayout(RectTransform, PrefabChildren);
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Layout, PrefabChildren);
	}
}


public partial class TextRect : INotificationPropagator, ICanBeLaidOut, INamed
{
	[ExposeMembersInClass] public Named Named { get; set; }
	[ExposeMembersInClass] public RectTransform RectTransform { get; set; }
	[ExposeMembersInClass] public Text Text { get; set; }
	
	public TextRect()
	{
		Named = new Named("Text");
		RectTransform = new RectTransform();
		Text = new Text(RectTransform);
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Text);
	}
}


public partial class HierarchyDisplay : INotificationPropagator, ICanBeLaidOut, INamed
{
	[ExposeMembersInClass] public Named Named { get; set; }
	[ExposeMembersInClass] public RectTransform RectTransform { get; set; }
	public IHasChildren HierarchyToDisplay { get; set; }
	private Hierarchy DisplayElements { get; set; }
	[ExposeMembersInClass] public FullLayout Layout { get; set; }
	
	public HierarchyDisplay(IHasChildren hierarchyToDisplay)
	{
		Named = new Named("Hierarchy Display");
		RectTransform = new RectTransform();
		HierarchyToDisplay = hierarchyToDisplay;
		DisplayElements = new Hierarchy();
		Layout = new FullLayout(RectTransform, DisplayElements);
		
		DisplayElements.AddChild(BoxOfHierarchy(HierarchyToDisplay));
	}
	
	private static FullLayoutWithHierarchy BoxOfHierarchy(object obj)
	{
		FullLayoutWithHierarchy box = new() { Name = $"Box for {obj}", AnchorPreset = AnchorPreset.Full, LeftMargin = 10 };
		VLayoutWithHierarchy layout = new() { Name = $"Layout for box for {obj}", AnchorPreset = AnchorPreset.Full };
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


public partial class FullLayoutWithHierarchy : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren, ICanAddChildren
{
	[ExposeMembersInClass] public Named Named { get; set; }
	[ExposeMembersInClass] public RectTransform RectTransform { get; set; }
	[ExposeMembersInClass] public FullLayout Layout { get; set; }
	[ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; }
	
	public FullLayoutWithHierarchy()
	{
		Named = new Named("Vertical Layout");
		PrefabChildren = new Hierarchy();
		RectTransform = new RectTransform();
		Layout = new FullLayout(RectTransform, PrefabChildren);
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Layout, PrefabChildren);
	}
}


public partial class DisplayOnlyLayer : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren, ICanAddChildren
{
	[ExposeMembersInClass] public Named Named { get; set; }
	[ExposeMembersInClass] public RectTransform RectTransform { get; set; }
	[ExposeMembersInClass] public RectLayout Layout { get; set; }
	[ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; }
	
	public DisplayOnlyLayer()
	{
		Named = new Named("Display Only Layer");
		PrefabChildren = new Hierarchy();
		RectTransform = new RectTransform();
		Layout = new RectLayout(RectTransform, PrefabChildren);
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		if (notification is IPreUpdate.Notification or IUpdate.Notification or IDraw.Notification)
			INotificationPropagator.Notify(notification, Layout, PrefabChildren);
	}
}

using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Mathematics;
using Echidna2.Rendering;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using Color = System.Drawing.Color;
using Image = SixLabors.ImageSharp.Image;

Console.WriteLine("Hello, World!");



RectWithHierarchy root = new() { Name = "Root", IsGlobal = true, Color = Color.Magenta };

VLayoutWithHierarchy toolbarBox = new() { Name = "Toolbar Box", AnchorPreset = AnchorPreset.Full };
root.PrefabChildren.AddChild(toolbarBox);

RectWithHierarchy toolbar = new() { Name = "Toolbar", MinimumSize = (0, 50) };
toolbarBox.PrefabChildren.AddChild(toolbar);

TextRect toolbarText = new() { Name = "Toolbar Text", TextString = "This is a toolbar.", AnchorPreset = AnchorPreset.Full };
toolbar.PrefabChildren.AddChild(toolbarText);

HLayoutWithHierarchy mainPanels = new() { Name = "Main Panels", VerticalExpand = true };
toolbarBox.PrefabChildren.AddChild(mainPanels);

VLayoutWithHierarchy hierarchyBox = new() { Name = "Hierarchy Box" };
mainPanels.PrefabChildren.AddChild(hierarchyBox);

RectWithHierarchy hierarchy = new() { Name = "Hierarchy", VerticalExpand = true, MinimumSize = (200, 0) };
hierarchyBox.PrefabChildren.AddChild(hierarchy);

RectWithHierarchy fileBrowser = new() { Name = "File Browser", VerticalExpand = true, MinimumSize = (200, 0) };
hierarchyBox.PrefabChildren.AddChild(fileBrowser);

VLayoutWithHierarchy sceneBox = new() { Name = "Scene Box", HorizontalExpand = true };
mainPanels.PrefabChildren.AddChild(sceneBox);

RectWithHierarchy console = new() { Name = "Console", MinimumSize = (100, 200) };
sceneBox.PrefabChildren.AddChild(console);

RectWithHierarchy scene = new() { Name = "Scene", VerticalExpand = true };
sceneBox.PrefabChildren.AddChild(scene);

VLayoutWithHierarchy inspectorBox = new() { Name = "Inspector Box" };
mainPanels.PrefabChildren.AddChild(inspectorBox);

RectWithHierarchy inspector = new() { Name = "Inspector", VerticalExpand = true, MinimumSize = (200, 0) };
inspectorBox.PrefabChildren.AddChild(inspector);

RectButton inspectorButton = new() { Name = "Inspector Button", AnchorPreset = AnchorPreset.Center, MinimumSize = (150, 50) };
inspectorButton.Clicked += () => Console.WriteLine("Inspector Button Clicked!");
inspector.PrefabChildren.AddChild(inspectorButton);



RectWithHierarchy sceneHierarchy = new() { Name = "Scene Hierarchy", AnchorPreset = AnchorPreset.Full };
TextRect sceneText = new() { Name = "Some Text", TextString = "This is some text.", AnchorPreset = AnchorPreset.Full };
sceneHierarchy.PrefabChildren.AddChild(sceneText);

HierarchyDisplay hierarchyDisplay = new(sceneHierarchy) { Name = "Hierarchy Display", AnchorPreset = AnchorPreset.Full };
hierarchy.PrefabChildren.AddChild(hierarchyDisplay);



IHasChildren.PrintTree(root);

Window window = new(new GameWindow(
	new GameWindowSettings(),
	new NativeWindowSettings
	{
		ClientSize = (1280, 720),
		Title = "Echidna Engine",
		Icon = CreateWindowIcon("Echidna.png"),
	}
))
{
	Camera = new Camera { World = root }
};
window.Resize += size => root.Size = size;
window.Run();
return;



static WindowIcon CreateWindowIcon(string path)
{
	Image<Rgba32> image = (Image<Rgba32>)Image.Load(new DecoderOptions(), path);
	Span<byte> imageBytes = stackalloc byte[32 * 32 * 4];
	image.Frames.RootFrame.CopyPixelDataTo(imageBytes);
	return new WindowIcon(new OpenTK.Windowing.Common.Input.Image(image.Width, image.Height, imageBytes.ToArray()));
}


partial class RectWithHierarchy : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren
{
	[ExposeMembersInClass] public Named Named { get; set; }
	[ExposeMembersInClass] public RectTransform RectTransform { get; set; }
	public RectLayout RectLayout { get; set; }
	[ExposeMembersInClass] public Rect Rect { get; set; }
	public Hierarchy PrefabChildren { get; set; }
	
	public IEnumerable<object> Children => PrefabChildren.Children;
	
	public RectWithHierarchy()
	{
		Named = new Named("Rect With Hierarchy");
		PrefabChildren = new Hierarchy();
		RectTransform = new RectTransform();
		RectLayout = new RectLayout(RectTransform, PrefabChildren);
		Rect = new Rect(RectTransform);
	}
	
	public void Notify<T>(T notification)
	{
		INotificationPropagator.Notify(notification, Rect, RectLayout, PrefabChildren);
	}
}


partial class RectButton : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren
{
	[ExposeMembersInClass] public Named Named { get; set; }
	[ExposeMembersInClass] public RectTransform RectTransform { get; set; }
	public RectLayout RectLayout { get; set; }
	[ExposeMembersInClass] public Rect Rect { get; set; }
	public Hierarchy PrefabChildren { get; set; }
	[ExposeMembersInClass] public Button Button { get; set; }
	
	public IEnumerable<object> Children => PrefabChildren.Children;
	
	public RectButton()
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
	
	public void Notify<T>(T notification)
	{
		INotificationPropagator.Notify(notification, Rect, Button, RectLayout, PrefabChildren);
	}
}


partial class HLayoutWithHierarchy : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren
{
	[ExposeMembersInClass] public Named Named { get; set; }
	[ExposeMembersInClass] public RectTransform RectTransform { get; set; }
	public HorizontalLayout Layout { get; set; }
	public Hierarchy PrefabChildren { get; set; }
	
	public IEnumerable<object> Children => PrefabChildren.Children;
	
	public HLayoutWithHierarchy()
	{
		Named = new Named("Horizontal Layout");
		PrefabChildren = new Hierarchy();
		RectTransform = new RectTransform();
		Layout = new HorizontalLayout(RectTransform, PrefabChildren);
	}
	
	public void Notify<T>(T notification)
	{
		INotificationPropagator.Notify(notification, Layout, PrefabChildren);
	}
}


partial class VLayoutWithHierarchy : INotificationPropagator, ICanBeLaidOut, INamed, IHasChildren
{
	[ExposeMembersInClass] public Named Named { get; set; }
	[ExposeMembersInClass] public RectTransform RectTransform { get; set; }
	public VerticalLayout Layout { get; set; }
	public Hierarchy PrefabChildren { get; set; }
	
	public IEnumerable<object> Children => PrefabChildren.Children;
	
	public VLayoutWithHierarchy()
	{
		Named = new Named("Vertical Layout");
		PrefabChildren = new Hierarchy();
		RectTransform = new RectTransform();
		Layout = new VerticalLayout(RectTransform, PrefabChildren);
	}
	
	public void Notify<T>(T notification)
	{
		INotificationPropagator.Notify(notification, Layout, PrefabChildren);
	}
}


partial class TextRect : INotificationPropagator, ICanBeLaidOut, INamed
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
	
	public void Notify<T>(T notification)
	{
		INotificationPropagator.Notify(notification, Text);
	}
}


partial class HierarchyDisplay : INotificationPropagator, ICanBeLaidOut, INamed
{
	[ExposeMembersInClass] public Named Named { get; set; }
	[ExposeMembersInClass] public RectTransform RectTransform { get; set; }
	public IHasChildren HierarchyToDisplay { get; set; }
	private Hierarchy DisplayElements { get; set; }
	public RectLayout DisplayLayout { get; set; }
	
	public HierarchyDisplay(IHasChildren hierarchyToDisplay)
	{
		Named = new Named("Hierarchy Display");
		RectTransform = new RectTransform();
		HierarchyToDisplay = hierarchyToDisplay;
		DisplayElements = new Hierarchy();
		DisplayLayout = new RectLayout(RectTransform, DisplayElements);
		
		DisplayElements.AddChild(BoxOfHierarchy(HierarchyToDisplay));
	}
	
	private static VLayoutWithHierarchy BoxOfHierarchy(object obj)
	{
		VLayoutWithHierarchy box = new() { Name = $"Box for {obj}", AnchorPreset = AnchorPreset.Full };
		
		TextRect text = new()
		{
			TextString = obj is INamed named ? named.Name : obj.GetType().Name + " (no name)",
			AnchorPreset = AnchorPreset.Full,
			Scale = Vector2.One * 0.5,
			MinimumSize = (0, 25),
			Justification = TextJustification.Left,
		};
		box.PrefabChildren.AddChild(text);
		
		if (obj is IHasChildren hasChildren)
			foreach (object child in hasChildren.Children)
				box.PrefabChildren.AddChild(BoxOfHierarchy(child));
		
		return box;
	}
	
	public void Notify<T>(T notification)
	{
		INotificationPropagator.Notify(notification, DisplayLayout, DisplayElements);
	}
}
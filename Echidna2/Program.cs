using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Rendering;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

Console.WriteLine("Hello, World!");



RectWithHierarchyPrefab rectPrefab = new();
rectPrefab.RectTransform = new RectTransform();
rectPrefab.RectLayout = new RectLayout(rectPrefab.RectTransform, rectPrefab.PrefabChildren);
rectPrefab.Rect = new Rect(rectPrefab.RectTransform);


RectWithHierarchy root = new(rectPrefab) { IsGlobal = true };

RectWithHierarchy someRect = new(rectPrefab) { AnchorPreset = AnchorPreset.Center, MinimumSize = (200, 200) };
root.PrefabChildren.AddChild(someRect);


// VerticalLayout toolbarBox = new() { Name = "Toolbar Box", AnchorPreset = AnchorPreset.Full };
// root.Hierarchy.AddChild(toolbarBox);
//
// HorizontalLayout mainPanels = new() { Name = "Main Panels", VerticalExpand = true };
// toolbarBox.AddChild(mainPanels);
//
// VerticalLayout hierarchyBox = new() { Name = "Hierarchy Box" };
// mainPanels.AddChild(hierarchyBox);
//
// Rect hierarchy = new() { Name = "Hierarchy", VerticalExpand = true, MinimumSize = (200, 0) };
// hierarchyBox.AddChild(hierarchy);
//
// Rect fileBrowser = new() { Name = "File Browser", VerticalExpand = true, MinimumSize = (200, 0) };
// hierarchyBox.AddChild(fileBrowser);
//
// VerticalLayout sceneBox = new() { Name = "Scene Box", HorizontalExpand = true };
// mainPanels.AddChild(sceneBox);
//
// Rect console = new() { Name = "Console", MinimumSize = (100, 200) };
// sceneBox.AddChild(console);
//
// Rect scene = new() { Name = "Scene", VerticalExpand = true };
// sceneBox.AddChild(scene);
//
// VerticalLayout inspectorBox = new() { Name = "Inspector Box" };
// mainPanels.AddChild(inspectorBox);
//
// Rect inspector = new() { Name = "Inspector", VerticalExpand = true, MinimumSize = (200, 0) };
// inspectorBox.AddChild(inspector);
//
// Rect inspectorButtonRect = new() { Name = "Inspector Button", AnchorPreset = AnchorPreset.Center, MinimumSize = (150, 50) };
// Button inspectorButton = new(rectTransform: inspectorButtonRect);
// inspector.AddChild(inspectorButton);
//
// Rect toolbar = new() { Name = "Toolbar", MinimumSize = (0, 50) };
// toolbarBox.AddChild(toolbar);
//
// Text toolbarText = new() { Name = "Toolbar Text", TextString = "This is a toolbar.", AnchorPreset = AnchorPreset.Full };
// toolbar.AddChild(toolbarText);




// PrefabRoot prefabRoot = new();
// RectTransform prefabTransform = new();
// prefabRoot.Hierarchy.AddChild(prefabTransform);
// Button prefabButton = new(rectTransform: prefabTransform);
// prefabRoot.Hierarchy.AddChild(prefabButton);
//
// Hierarchy prefabProperties = new();
// Rect prefabBackground = new(rectTransform: prefabTransform);
// prefabProperties.AddChild(prefabBackground);
// RectLayout prefabLayout = new(
// 	rectTransform: prefabTransform,
// 	hierarchy: prefabRoot.Hierarchy);
// prefabProperties.AddChild(prefabLayout);
//
//
//
//
// Prefab buttonPrefab = prefabRoot.Instantiate();
// root.AddChild(buttonPrefab);
// Text buttonText = new() { TextString = "Button" };
// buttonPrefab.Hierarchy.AddChild(buttonText);



root.PrefabChildren.PrintTree();

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


partial class RectWithHierarchyPrefab : Prefab<RectWithHierarchyPrefab>
{
	[ExposeMembersInClass] public RectTransform RectTransform { get; set; }
	public RectLayout RectLayout;
	public Rect Rect;
	
	public override RectWithHierarchyPrefab Instantiate()
	{
		if (IsSharedInstance) return this;
		
		RectWithHierarchyPrefab instance = new();
		instance.RectTransform = new RectTransform().CopyFrom(RectTransform);
		return instance;
	}
}

[PrefabOf<RectWithHierarchyPrefab>]
partial class RectWithHierarchy : PrefabInstance
{
	public override void Notify<T>(T notification)
	{
		INotificationPropagator.NotifySingle(Rect, notification);
		INotificationPropagator.NotifySingle(RectLayout, notification);
		INotificationPropagator.NotifySingle(PrefabChildren, notification);
	}
}
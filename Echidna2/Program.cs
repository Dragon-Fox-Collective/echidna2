using Echidna2;
using Echidna2.Core;
using Echidna2.Gui;
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
root.AddChild(toolbarBox);

RectWithHierarchy toolbar = new() { Name = "Toolbar", MinimumSize = (0, 50) };
toolbarBox.AddChild(toolbar);

TextRect toolbarText = new() { Name = "Toolbar Text", TextString = "This is a toolbar.", AnchorPreset = AnchorPreset.Full };
toolbar.AddChild(toolbarText);

HLayoutWithHierarchy mainPanels = new() { Name = "Main Panels", VerticalExpand = true };
toolbarBox.AddChild(mainPanels);

VLayoutWithHierarchy hierarchyBox = new() { Name = "Hierarchy Box" };
mainPanels.AddChild(hierarchyBox);

RectWithHierarchy hierarchy = new() { Name = "Hierarchy", VerticalExpand = true, MinimumSize = (200, 0) };
hierarchyBox.AddChild(hierarchy);

RectWithHierarchy fileBrowser = new() { Name = "File Browser", VerticalExpand = true, MinimumSize = (200, 0) };
hierarchyBox.AddChild(fileBrowser);

VLayoutWithHierarchy sceneBox = new() { Name = "Scene Box", HorizontalExpand = true };
mainPanels.AddChild(sceneBox);

DisplayOnlyLayer scene = new() { Name = "Scene", VerticalExpand = true };
sceneBox.AddChild(scene);

RectWithHierarchy console = new() { Name = "Console", MinimumSize = (100, 200) };
sceneBox.AddChild(console);

VLayoutWithHierarchy inspectorBox = new() { Name = "Inspector Box" };
mainPanels.AddChild(inspectorBox);

RectWithHierarchy inspector = new() { Name = "Inspector", VerticalExpand = true, MinimumSize = (200, 0) };
inspectorBox.AddChild(inspector);

ButtonRect inspectorButton = new() { Name = "Inspector Button", AnchorPreset = AnchorPreset.Center, MinimumSize = (150, 50) };
inspectorButton.Clicked += () => Console.WriteLine("Inspector Button Clicked!");
inspector.AddChild(inspectorButton);



RectWithHierarchy someHierarchy = new() { Name = "Some Hierarchy", AnchorPreset = AnchorPreset.Full, Color = Color.DarkCyan, ClipChildren = true };
RectWithHierarchy someRect = new() { Name = "Some Rect", AnchorPreset = AnchorPreset.Center, MinimumSize = (200, 200) };
someHierarchy.AddChild(someRect);
TextRect someText = new() { Name = "Some Text", TextString = "This is some text.", AnchorPreset = AnchorPreset.TopCenter };
someRect.AddChild(someText);
TextRect someOtherText = new() { Name = "Some Other Text", TextString = "This is some other text.", AnchorPreset = AnchorPreset.BottomCenter };
someHierarchy.AddChild(someOtherText);
ButtonRect someButton = new() { Name = "Some Button", AnchorPreset = AnchorPreset.CenterLeft, MinimumSize = (150, 50) };
someHierarchy.AddChild(someButton);

scene.AddChild(someHierarchy);

HierarchyDisplay hierarchyDisplay = new(someHierarchy) { Name = "Hierarchy Display", AnchorPreset = AnchorPreset.Full, TopMargin = 10 };
hierarchy.AddChild(hierarchyDisplay);



IHasChildren.PrintTree(root);

Window window = new(new GameWindow(
	new GameWindowSettings(),
	new NativeWindowSettings
	{
		ClientSize = (1280, 720),
		Title = "Echidna Engine",
		Icon = Window.CreateWindowIcon("Assets/Echidna.png"),
	}
))
{
	Camera = new GuiCamera(root)
};
window.Resize += size => root.LocalSize = size;
window.Run();
return;

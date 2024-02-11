using Echidna2.Core;
using Echidna2.Rendering;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

Console.WriteLine("Hello, World!");


Rect root = new() { Name = "Root", IsGlobal = true };


VerticalLayout toolbarBox = new() { Name = "ToolbarBox", AnchorPreset = AnchorPreset.Full };
root.AddChild(toolbarBox);

HorizontalLayout mainPanels = new() { Name = "MainPanels", VerticalExpand = true };
toolbarBox.AddChild(mainPanels);

VerticalLayout hierarchyBox = new() { Name = "HierarchyBox" };
mainPanels.AddChild(hierarchyBox);

Rect hierarchy = new() { Name = "Hierarchy", VerticalExpand = true, MinimumSize = (200, 0) };
hierarchyBox.AddChild(hierarchy);

Rect fileBrowser = new() { Name = "FileBrowser", VerticalExpand = true, MinimumSize = (200, 0) };
hierarchyBox.AddChild(fileBrowser);

VerticalLayout sceneBox = new() { Name = "SceneBox", HorizontalExpand = true };
mainPanels.AddChild(sceneBox);

Rect console = new() { Name = "Console", MinimumSize = (100, 200) };
sceneBox.AddChild(console);

Rect scene = new() { Name = "Scene", VerticalExpand = true };
sceneBox.AddChild(scene);

VerticalLayout inspectorBox = new() { Name = "InspectorBox" };
mainPanels.AddChild(inspectorBox);

Rect inspector = new() { Name = "Inspector", VerticalExpand = true, MinimumSize = (200, 0) };
inspectorBox.AddChild(inspector);

Rect toolbar = new() { Name = "Toolbar", MinimumSize = (0, 50) };
toolbarBox.AddChild(toolbar);


root.PrintTree();

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
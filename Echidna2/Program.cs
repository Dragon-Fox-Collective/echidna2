using Echidna2.Core;
using Echidna2.Rendering;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

Console.WriteLine("Hello, World!");


Hierarchy world = new() { Name = "Root" };

Rect rect1 = new() { Name = "Rect1", Size = (500, 300), IsGlobal = true, Position = (100, 0) };
world.AddChild(rect1);
HorizontalLayout layout = new() { Name = "Layout", AnchorPreset = AnchorPreset.Full };
rect1.AddChild(layout);
Rect rect2 = new() { Name = "Rect2", MinimumSize = (100, 100), VerticalSizing = LayoutSizing.FitCenter };
layout.AddChild(rect2);
Rect rect3 = new() { Name = "Rect3", MinimumSize = (200, 150), VerticalSizing = LayoutSizing.FitBottom, HorizontalExpand = true };
layout.AddChild(rect3);
Rect rect4 = new() { Name = "Rect4", MinimumSize = (50, 50) };
layout.AddChild(rect4);


world.PrintTree();

new Window(new GameWindow(
	new GameWindowSettings(),
	new NativeWindowSettings
	{
		ClientSize = (1080, 720),
		Title = "Echidna Engine",
		Icon = CreateWindowIcon("Echidna.png"),
	}
))
{
	Camera = new Camera { World = world }
}.Run();
return;


static WindowIcon CreateWindowIcon(string path)
{
	Image<Rgba32> image = (Image<Rgba32>)Image.Load(new DecoderOptions(), path);
	Span<byte> imageBytes = stackalloc byte[32 * 32 * 4];
	image.Frames.RootFrame.CopyPixelDataTo(imageBytes);
	return new WindowIcon(new OpenTK.Windowing.Common.Input.Image(image.Width, image.Height, imageBytes.ToArray()));
}
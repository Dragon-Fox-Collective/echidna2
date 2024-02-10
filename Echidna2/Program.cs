using Echidna2.Core;
using Echidna2.Rendering;
using OpenTK.Windowing.Desktop;

Console.WriteLine("Hello, World!");


Hierarchy world = new() { Name = "Root" };

Rect rect1 = new() { Name = "Rect1" };
world.AddChild(rect1);
Rect rect2 = new() { Name = "Rect2", AnchorPreset = AnchorPreset.TallLeft, AnchorOffsetRight = 100 };
rect1.AddChild(rect2);
Rect rect3 = new() { Name = "Rect3", AnchorPreset = AnchorPreset.WideBottom, AnchorOffsetTop = 100 };
rect2.AddChild(rect3);


world.PrintTree();

new Window(new GameWindow(
	new GameWindowSettings(),
	new NativeWindowSettings { ClientSize = (1080, 720) }
))
{
	Camera = new Camera { World = world }
}.Run();
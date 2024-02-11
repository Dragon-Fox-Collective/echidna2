using Echidna2.Core;
using Echidna2.Rendering;
using OpenTK.Windowing.Desktop;

Console.WriteLine("Hello, World!");


Hierarchy world = new() { Name = "Root" };

Rect rect1 = new() { Name = "Rect1", Size = (500, 500) };
world.AddChild(rect1);
HorizontalLayout layout = new() { Name = "Layout", AnchorPreset = AnchorPreset.Full };
rect1.AddChild(layout);
Rect rect2 = new() { Name = "Rect2", MinimumSize = (100, 100) };
layout.AddChild(rect2);


world.PrintTree();

new Window(new GameWindow(
	new GameWindowSettings(),
	new NativeWindowSettings { ClientSize = (1080, 720) }
))
{
	Camera = new Camera { World = world }
}.Run();
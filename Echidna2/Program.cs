using Echidna2;
using Echidna2.Core;
using Echidna2.Rendering;
using OpenTK.Windowing.Desktop;

Console.WriteLine("Hello, World!");


Hierarchy world = new() { Name = "Root" };
world.AddChild(new DebugEntity());

world.AddChild(new Rect { Position = (100, 0, 0) });


new Window(new GameWindow(
	new GameWindowSettings(),
	new NativeWindowSettings { ClientSize = (1080, 720) }
))
{
	Camera = new Camera { World = world }
}.Run();
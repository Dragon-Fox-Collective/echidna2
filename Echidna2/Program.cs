using Echidna2;
using Echidna2.Core;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

Console.WriteLine("Hello, World!");

Vector2i size = (1080, 720);
GameWindow gameWindow = new(
	new GameWindowSettings(),
	new NativeWindowSettings
	{
		ClientSize = size,
	}
);

Hierarchy hierarchy = new();
hierarchy.AddChild(new DebugEntity());

Box box = new();
hierarchy.AddChild(box);

Box box1 = new();
hierarchy.AddChild(box1);
Box box2 = new();
box1.AddChild(box2);

// gameWindow.Load += world.Initialize;
// gameWindow.Unload += world.Dispose;
gameWindow.UpdateFrame += args => hierarchy.Update(args.Time);
gameWindow.RenderFrame += _ => hierarchy.Draw();
// gameWindow.MouseMove += args => world.MouseMove(args.Position, args.Delta);
// gameWindow.KeyDown += args => world.KeyDown(args.Key);
// gameWindow.KeyUp += args => world.KeyUp(args.Key);
gameWindow.Run();
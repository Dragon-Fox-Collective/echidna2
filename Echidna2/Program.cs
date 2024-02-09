using Echidna2;
using Echidna2.Core;
using OpenTK.Windowing.Desktop;

Console.WriteLine("Hello, World!");

GameWindow gameWindow = new(
	new GameWindowSettings(),
	new NativeWindowSettings
	{
		ClientSize = (1080, 720),
	}
);

ReallyCoolPrefab world = new();
world.AddChild(new DebugEntity());

Box pseudoBox = new(hierarchy: null, rectTransform: world);
world.AddChild(pseudoBox);

world.AddChild(new Box());

Box bigBox = new();
world.AddChild(bigBox);
bigBox.AddChild(new Box());

world.PrintTree();
world.Draw();
Console.WriteLine(string.Join(", ", world.GetPropertyList()));

// gameWindow.Load += world.Initialize;
// gameWindow.Unload += world.Dispose;
gameWindow.UpdateFrame += args =>
{
	world.PreUpdate();
	world.Update(args.Time);
};
gameWindow.RenderFrame += _ => world.Draw();
// gameWindow.MouseMove += args => world.MouseMove(args.Position, args.Delta);
// gameWindow.KeyDown += args => world.KeyDown(args.Key);
// gameWindow.KeyUp += args => world.KeyUp(args.Key);
// gameWindow.Run();
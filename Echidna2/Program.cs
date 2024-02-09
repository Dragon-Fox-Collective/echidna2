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

Hierarchy worldHierarchy = new() { Name = "ReallyCoolPrefab hierarchy" };
RectTransform worldRectTransform = new(new Hierarchy { Name = "ReallyCoolPrefab rect hierarchy" });
ReallyCoolPrefab world = new(hierarchy: worldHierarchy, rectTransform: worldRectTransform);
world.AddChild(new DebugEntity());

Box pseudoBox = new(hierarchy: null, rectTransform: worldRectTransform);
world.AddChild(pseudoBox);

worldRectTransform.AddChild(new Box());

Box bigBox = new();
worldRectTransform.AddChild(bigBox);
bigBox.AddChild(new Box());

world.PrintTree();
worldRectTransform.PrintTree();
Console.WriteLine("ReallyCoolPrefab's properties: " + string.Join(", ", world.GetPropertyList()));
world.PreUpdate();
world.Update(0);
world.Draw();

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
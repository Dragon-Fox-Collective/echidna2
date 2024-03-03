using Echidna2;
using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Rendering;
using Echidna2.Serialization;
using OpenTK.Windowing.Desktop;

Console.WriteLine("Hello, World!");



RectWithHierarchy root = TomlSerializer.Deserialize<RectWithHierarchy>($"{AppContext.BaseDirectory}/Prefabs/Editor.toml");



IHasChildren.PrintTree(root);

Window window = new(new GameWindow(
	new GameWindowSettings(),
	new NativeWindowSettings
	{
		ClientSize = (1280, 720),
		Title = "Echidna Engine",
		Icon = Window.CreateWindowIcon($"{AppContext.BaseDirectory}/Assets/Echidna.png"),
	}
))
{
	Camera = new GuiCamera
	{
		World = root,
	}
};
window.Resize += size =>
{
	root.LocalSize = size;
	window.Camera.Size = size;
};
window.Run();

using Echidna2;
using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Rendering;
using Echidna2.Serialization;
using OpenTK.Windowing.Desktop;

Console.WriteLine("Hello, World!");



RectWithHierarchy root = TomlSerializer.Deserialize<RectWithHierarchy>($"{AppContext.BaseDirectory}/Prefabs/Editor.toml");

// inspectorButton.Clicked += () => Console.WriteLine("Inspector Button Clicked!");



// Scene sceneInstance = TomlSerializer.Deserialize<Scene>($"{AppContext.BaseDirectory}/Prefabs/SomeScene.toml");
// RectWithHierarchy someHierarchy = new() { Name = "Some Hierarchy", AnchorPreset = AnchorPreset.Full, Color = Color.DarkCyan, ClipChildren = true };
// RectWithHierarchy someRect = new() { Name = "Some Rect", AnchorPreset = AnchorPreset.Center, MinimumSize = (200, 200) };
// someHierarchy.AddChild(someRect);
// TextRect someText = new() { Name = "Some Text", TextString = "This is some text.", AnchorPreset = AnchorPreset.TopCenter };
// someRect.AddChild(someText);
// TextRect someOtherText = new() { Name = "Some Other Text", TextString = "This is some other text.", AnchorPreset = AnchorPreset.BottomCenter };
// someHierarchy.AddChild(someOtherText);
// ButtonRect someButton = new() { Name = "Some Button", AnchorPreset = AnchorPreset.CenterLeft, MinimumSize = (150, 50) };
// someHierarchy.AddChild(someButton);
//
// scene.AddChild(someHierarchy);
//
// HierarchyDisplay hierarchyDisplay = new(someHierarchy) { Name = "Hierarchy Display", AnchorPreset = AnchorPreset.Full, TopMargin = 10 };
// hierarchy.AddChild(hierarchyDisplay);



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

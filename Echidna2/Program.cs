using System.Runtime.Loader;
using Echidna2;
using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Prefabs.Editor;
using Echidna2.Rendering;
using Echidna2.Serialization;
using OpenTK.Windowing.Desktop;

Console.WriteLine("Hello, World!");
Compilation.CompileCSProj("Prefabs/Editor/Editor.toml");

AssemblyLoadContext projectAssemblyLoadContext = new("EchidnaProject");
using FileStream projectAssemblyFileStream = new(Compilation.CompilationDllPath, FileMode.Open, FileAccess.Read);
TomlDeserializer.ProjectAssembly = projectAssemblyLoadContext.LoadFromStream(projectAssemblyFileStream);


Editor root = (Editor)TomlDeserializer.Deserialize(AppContext.BaseDirectory + "Prefabs/Editor/Editor.toml").RootObject;
root.Notify(new IEditorInitialize.Notification(root));

PrefabRoot prefab = TomlDeserializer.Deserialize(AppContext.BaseDirectory + root.PrefabPath);
TomlSerializer.Serialize(prefab, AppContext.BaseDirectory + root.PrefabPath);
prefab = TomlDeserializer.Deserialize(AppContext.BaseDirectory + root.PrefabPath);

root.PrefabRoot = prefab;
IHasChildren.PrintTree(root);

Window window = new(new GameWindow(
	new GameWindowSettings(),
	new NativeWindowSettings
	{
		ClientSize = (1280, 720),
		Title = "Echidna Engine",
		Icon = Window.CreateWindowIcon(AppContext.BaseDirectory +  "Assets/Echidna.png"),
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

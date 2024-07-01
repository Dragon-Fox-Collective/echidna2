﻿using System.Runtime.Loader;
using Echidna2;
using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Prefabs.Editor;
using Echidna2.Rendering;
using Echidna2.Rendering3D;
using Echidna2.Serialization;
using OpenTK.Windowing.Desktop;

// :/ cubemap no longer static constructed because nothing references it since the skybox is a toml now
object staticConstructorLoad;
staticConstructorLoad = CubeMap.Skybox;
staticConstructorLoad = Mesh.Cube;
staticConstructorLoad = Shader.Quad;

Console.WriteLine("Hello, World!");
Compilation.Compile().Wait();

AssemblyLoadContext projectAssemblyLoadContext = new("EchidnaProject");
using FileStream projectAssemblyFileStream = new(Compilation.CompilationDllPath, FileMode.Open, FileAccess.Read);
TomlDeserializer.ProjectAssembly = projectAssemblyLoadContext.LoadFromStream(projectAssemblyFileStream);


Editor root = (Editor)Editor.Instantiate("Prefabs/Editor/Editor");
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
		Icon = Window.CreateWindowIcon(AppContext.BaseDirectory + "/Assets/Echidna.png"),
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

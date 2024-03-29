﻿using System.Runtime.Loader;
using Echidna2;
using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Rendering;
using Echidna2.Serialization;
using OpenTK.Windowing.Desktop;

Console.WriteLine("Hello, World!");
Compilation.CompileCSProj("Prefabs/Editor.toml");

AssemblyLoadContext projectAssemblyLoadContext = new("EchidnaProject");
using FileStream proejctAssemblyFileStream = new(Compilation.CompilationDllPath, FileMode.Open, FileAccess.Read);
TomlSerializer.ProjectAssembly = projectAssemblyLoadContext.LoadFromStream(proejctAssemblyFileStream);


Editor root = TomlSerializer.Deserialize<Editor>($"{AppContext.BaseDirectory}/Prefabs/Editor.toml");
root.Notify(new IEditorInitialize.Notification(root));

IHasChildren prefab = TomlSerializer.Deserialize<IHasChildren>($"{AppContext.BaseDirectory}/{root.PrefabPath}");
root.Prefab = prefab;


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

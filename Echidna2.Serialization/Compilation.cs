using System.Diagnostics;
using System.Runtime.Loader;
using Echidna2.Serialization.TomlFiles;
using Tomlyn;
using Tomlyn.Model;

namespace Echidna2.Serialization;

public static class Compilation
{
	public static string CompilationFolder => $"{AppContext.BaseDirectory}/.echidna";
	public static string CompilationBinFolder => $"{CompilationFolder}/bin/Debug/net8.0";
	public static string CompilationDllPath => $"{CompilationBinFolder}/EchidnaProject.dll";
	
	public static async Task<Project> Compile()
	{
		RecreateDirectory();
		CreateCSProj();
		
		Project project = new();
		
		foreach (string prefabPath in Directory.EnumerateFiles(".", "*.prefab.toml", SearchOption.AllDirectories))
			project.AddPrefab(CreateCSPrefabFile(prefabPath[2..]));
		
		foreach (string prefabPath in Directory.EnumerateFiles(".", "*.interface.toml", SearchOption.AllDirectories))
			project.AddInterface(CreateCSInterfaceFile(prefabPath[2..]));
		
		foreach (string prefabPath in Directory.EnumerateFiles(".", "*.notification.toml", SearchOption.AllDirectories))
			project.AddNotification(CreateCSNotificationFile(prefabPath[2..]));
		
		await CompileCSProj();
		
		AssemblyLoadContext projectAssemblyLoadContext = new("EchidnaProject");
		await using FileStream projectAssemblyFileStream = new(CompilationDllPath, FileMode.Open, FileAccess.Read);
		project.Assembly = projectAssemblyLoadContext.LoadFromStream(projectAssemblyFileStream);
		
		return project;
	}
	
	public static void RecreateDirectory()
	{
		if (Directory.Exists(CompilationFolder))
		{
			DirectoryInfo directory = new(CompilationFolder);
			directory.EnumerateFiles().ForEach(file => file.Delete());
			directory.EnumerateDirectories().ForEach(dir => dir.Delete(true));
		}
		else
			Directory.CreateDirectory(CompilationFolder);
	}
	
	public static void CreateCSProj()
	{
		string csprojString =
"""
<Project Sdk="Microsoft.NET.Sdk">

	<PropertyGroup>
		<TargetFramework>net8.0</TargetFramework>
		<ImplicitUsings>enable</ImplicitUsings>
		<Nullable>enable</Nullable>
		<DebugType>embedded</DebugType>
	</PropertyGroup>

	<ItemGroup>
		<Reference Include="..\Echidna2.dll" />
		<Reference Include="..\Echidna2.Core.dll" />
		<Reference Include="..\Echidna2.Gui.dll" />
		<Reference Include="..\Echidna2.Mathematics.dll" />
		<Reference Include="..\Echidna2.Rendering.dll" />
		<Reference Include="..\Echidna2.Rendering3D.dll" />
		<Reference Include="..\Echidna2.Serialization.dll" />
		<Reference Include="..\Echidna2.SourceGenerators.dll" />
		<Analyzer Include="..\Echidna2.SourceGenerators.dll" />
	</ItemGroup>

	<ItemGroup>
		<PackageReference Include="OpenTK" Version="4.8.2" />
		<PackageReference Include="Tomlyn" Version="0.17.0" />
	</ItemGroup>

</Project>
""";
		File.WriteAllText($"{CompilationFolder}/EchidnaProject.csproj", csprojString);
	}
	
	public static Prefab CreateCSPrefabFile(string prefabPath)
	{
		Prefab prefab = Prefab.FromToml(prefabPath);
		if (!prefab.NeedsCustomClass) return prefab;
		string classPath = GetPrefabClassPath(prefabPath);
		Directory.CreateDirectory(Path.GetDirectoryName(classPath));
		File.WriteAllText(classPath, prefab.StringifyCS());
		return prefab;
	}
	
	public static Interface CreateCSInterfaceFile(string prefabPath)
	{
		Interface @interface = Interface.FromToml(prefabPath);
		string classPath = GetPrefabClassPath(prefabPath);
		Directory.CreateDirectory(Path.GetDirectoryName(classPath));
		File.WriteAllText(classPath, @interface.StringifyCS());
		return @interface;
	}
	
	public static Notification CreateCSNotificationFile(string prefabPath)
	{
		Notification notification = Notification.FromToml(prefabPath);
		string classPath = GetPrefabClassPath(prefabPath);
		Directory.CreateDirectory(Path.GetDirectoryName(classPath));
		File.WriteAllText(classPath, notification.StringifyCS());
		return notification;
	}
	
	public static string GetPrefabClassPath(string prefabPath, string? id = null) => $"{CompilationFolder}/{Path.GetDirectoryName(prefabPath)}/{GetPrefabClassName(prefabPath, id)}.cs";
	
	public static string GetPrefabClassName(string prefabPath, string? id = null) => id is not null and not "This" ? $"{GetPrefabFileNameWithoutExtension(prefabPath)}_{id}" : GetPrefabFileNameWithoutExtension(prefabPath);
	
	public static string GetPrefabFileNameWithoutExtension(string prefabPath) => Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(prefabPath));
	
	public static string GetPrefabClassNamespace(string prefabPath) => Path.GetDirectoryName(prefabPath).Replace('/', '.').Replace('\\', '.');
	
	public static async Task CompileCSProj()
	{
		Process process = new();
		process.StartInfo.FileName = "dotnet";
		process.StartInfo.Arguments = "build";
		process.StartInfo.WorkingDirectory = CompilationFolder;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		process.Start();
		
		await Task.WhenAny(
			process.WaitForExitAsync(),
			DebugProcess(process));
		
		await PrintProcessOutput(process);
		if (process.ExitCode != 0)
			throw new InvalidOperationException($"Build failed with exit code {process.ExitCode}");
	}
	
	public static async Task RunCSProj()
	{
		Process process = new();
		process.StartInfo.FileName = "dotnet";
		process.StartInfo.Arguments = CompilationDllPath;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		process.Start();
		
		await Task.WhenAny(
			process.WaitForExitAsync(),
			DebugProcess(process));
		
		await PrintProcessOutput(process);
		if (process.ExitCode != 0)
			throw new InvalidOperationException($"Build failed with exit code {process.ExitCode}");
	}
	
	private static async Task PrintProcessOutput(Process process)
	{
		Console.Write(await process.StandardOutput.ReadToEndAsync());
		Console.ForegroundColor = ConsoleColor.Red;
		Console.Write(await process.StandardError.ReadToEndAsync());
		Console.ResetColor();
	}
	
	private static async Task DebugProcess(Process process)
	{
		while (!process.HasExited)
		{
			await PrintProcessOutput(process);
			await Task.Delay(250);
		}
	}
	
	public static string GetComponentBaseTypeName(string prefabPath, string id, TomlTable componentTable)
	{
		if (componentTable.TryGetValue("Prefab", out object basePrefab))
		{
			string basePrefabPath = $"{((string)basePrefab).Replace(".", "/")}.prefab.toml";
			TomlTable baseComponentTable = (TomlTable)Toml.ToModel(File.ReadAllText(basePrefabPath))["This"];
			return GetComponentBaseTypeName(basePrefabPath, "This", baseComponentTable);
		}
		
		if (componentTable.TryGetValue("Component", out object typeName))
			return ((string)typeName).Split(",")[0];
		
		return GetPrefabFileNameWithoutExtension(prefabPath) + (id is null or "This" ? "" : "_" + id);
	}
}
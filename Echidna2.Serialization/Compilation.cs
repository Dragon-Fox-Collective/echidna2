using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using Tomlyn;
using Tomlyn.Model;

namespace Echidna2.Serialization;

public class Compilation
{
	public static string CompilationFolder => $"{AppContext.BaseDirectory}/.echidna";
	public static string CompilationBinFolder => $"{CompilationFolder}/bin/Debug/net8.0";
	public static string CompilationDllPath => $"{CompilationBinFolder}/EchidnaProject.dll";
	
	private static string CSFileHeader =>
"""
using Echidna2;
using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Rendering;
using Echidna2.Rendering3D;
using Echidna2.Serialization;
using Echidna2.SourceGenerators;


""";
	
	public static async Task CompileCSProj(string prefabRootPath)
	{
		RecreateDirectory();
		CreateCSProj();
		
		foreach (string prefabPath in Directory.EnumerateFiles(prefabRootPath, "*.toml", SearchOption.AllDirectories))
		{
			CreateCSFiles(prefabPath);
		}
		await CompileCSProj();
		
		foreach (string prefabPath in Directory.EnumerateFiles(prefabRootPath, "*.toml", SearchOption.AllDirectories))
		{
			Dictionary<string, (bool, Dictionary<string, string>)> serializedEvents = GetSerializedEvents(prefabPath);
			CreateCSFiles(prefabPath, serializedEvents);
		}
		await CompileCSProj();
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
		string csprojString = @"
<Project Sdk=""Microsoft.NET.Sdk"">

    <PropertyGroup>
        <TargetFramework>net8.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <DebugType>embedded</DebugType>
    </PropertyGroup>

    <ItemGroup>
	    <Reference Include=""..\Echidna2.dll"" />
	    <Reference Include=""..\Echidna2.Core.dll"" />
	    <Reference Include=""..\Echidna2.Gui.dll"" />
	    <Reference Include=""..\Echidna2.Mathematics.dll"" />
	    <Reference Include=""..\Echidna2.Rendering.dll"" />
	    <Reference Include=""..\Echidna2.Rendering3D.dll"" />
	    <Reference Include=""..\Echidna2.Serialization.dll"" />
	    <Reference Include=""..\Echidna2.SourceGenerators.dll"" />
	    <Analyzer Include=""..\Echidna2.SourceGenerators.dll"" />
    </ItemGroup>

</Project>
";
		File.WriteAllText($"{CompilationFolder}/EchidnaProject.csproj", csprojString);
	}
	
	public static void CreateCSFiles(string prefabPath, Dictionary<string, (bool, Dictionary<string, string>)>? serializedEvents = null)
	{
		TomlTable table = Toml.ToModel(File.ReadAllText(prefabPath));
		foreach ((string id, object value) in table.Where(TomlSerializer.IsValidComponentId))
		{
			TomlTable valueTable = (TomlTable)value;
			if (!valueTable.TryGetValue("ScriptContent", out object scriptContent))
				continue;
			
			string baseType = GetComponentBaseTypeName(prefabPath, id, valueTable);
			string className = $"{Path.GetFileNameWithoutExtension(prefabPath)}_{id}";
			(bool baseTypeIsIInitialize, Dictionary<string, string>? events) = serializedEvents?.GetValueOrDefault(id) ?? (false, null)!;
			bool shouldAddInitialize = events != null!;
			
			string scriptString = CSFileHeader;
			scriptString += $"public class {className} : {baseType}{(shouldAddInitialize && !baseTypeIsIInitialize ? ", IInitialize" : "")}\n";
			scriptString += "{\n";
			
			scriptString += "\t" + ((string)scriptContent).Split("\n").Join("\n\t").Trim() + "\n";
			
			if (shouldAddInitialize)
			{
				scriptString += "\n";
				scriptString += $"\tpublic {(baseTypeIsIInitialize ? "override " : "")}void OnInitialize() {{\n";
				if (baseTypeIsIInitialize)
					scriptString += "\t\tbase.OnInitialize();\n";
				foreach ((string eventName, string eventContent) in events!)
					scriptString += $"\t\t{eventName} += () => {{ {eventContent} }};\n";
				scriptString += "\t}\n";
			}
			
			scriptString += "}\n";
			
			File.WriteAllText($"{CompilationFolder}/{className}.cs", scriptString);
		}
		
		if (table.TryGetValue("This", out object? _))
			CreateThisCSFile(prefabPath, table);
	}
	
	public static void CreateThisCSFile(string prefabPath, TomlTable table)
	{
		List<TomlTable> components = table.TryGetValue("Components", out object? componentsArray) ? ((TomlArray)componentsArray).OfType<TomlTable>().ToList() : [];
		List<TomlTable> fields = table.TryGetValue("Fields", out object? fieldsArray) ? ((TomlArray)fieldsArray).OfType<TomlTable>().ToList() : [];
		
		string className = Path.GetFileNameWithoutExtension(prefabPath);
		string scriptString = CSFileHeader;
		scriptString += $"public partial class {className}";
		if (components.Count != 0)
			scriptString += ": INotificationPropagator\n";
		else
			scriptString += "\n";
		scriptString += "{\n";
		
		if (components.Count != 0)
		{
			foreach (TomlTable componentTable in components)
				scriptString += $"\t[SerializedReference, ExposeMembersInClass] public {componentTable["Type"]} {componentTable["Name"]} {{ get; set; }} = null!;\n";
			scriptString += "\n";
			
			scriptString += "\tpublic void Notify<T>(T notification) where T : notnull\n";
			scriptString += "\t{\n";
			foreach (TomlTable componentTable in components)
				scriptString += $"\t\tINotificationPropagator.Notify(notification, {componentTable["Name"]});\n";
			scriptString += "\t}\n\n";
		}
		
		if (fields.Count != 0)
		{
			foreach (TomlTable fieldTable in fields)
				scriptString += $"\t[SerializedValue] public {fieldTable["Type"]} {fieldTable["Name"]} {{ get; set; }} = default!;\n";
			scriptString += "\n";
		}
		
		scriptString += "}\n";
		
		File.WriteAllText($"{CompilationFolder}/{className}.cs", scriptString);
	}
	
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
		string className = $"{Path.GetFileNameWithoutExtension(prefabPath)}_{id}";
		if (File.Exists(className + ".cs"))
			return className;
		
		if (componentTable.TryGetValue("Prefab", out object basePrefab))
		{
			string basePrefabPath = $"{Path.GetDirectoryName(prefabPath)}/{(string)basePrefab}";
			(string baseId, object baseComponentTable) = Toml.ToModel(File.ReadAllText(basePrefabPath)).First();
			return GetComponentBaseTypeName(basePrefabPath, baseId, (TomlTable)baseComponentTable);
		}
		
		if (!componentTable.TryGetValue("Component", out object typeName))
			throw new InvalidOperationException($"Component table {id} of {prefabPath} does not contain a Component key or Prefab key");
		
		return ((string)typeName).Split(",")[0];
	}
	
	public static Type GetComponentBaseType(string prefabPath, string? id, TomlTable componentTable, Assembly projectAssembly)
	{
		string className = Path.GetFileNameWithoutExtension(prefabPath) + (id is not null ? "_" + id : "");
		if (File.Exists($"{CompilationFolder}/{className}.cs"))
			return projectAssembly.GetType(className) ?? throw new NullReferenceException($"Type {className} has a file but does not exist as a type");
		
		if (componentTable.TryGetValue("Prefab", out object basePrefab))
		{
			string basePrefabPath = $"{Path.GetDirectoryName(prefabPath)}/{(string)basePrefab}";
			if (!File.Exists(basePrefabPath))
				throw new FileNotFoundException($"Could not fine file '{basePrefabPath}' referenced by prefab '{prefabPath}'");
			TomlTable baseTable = Toml.ToModel(File.ReadAllText(basePrefabPath));
			string? baseId;
			object baseComponentTable;
			if (baseTable.TryGetValue("This", out object? baseThis))
			{
				baseId = null;
				baseComponentTable = baseThis;
			}
			else
			{
				(baseId, baseComponentTable) = baseTable.First(TomlSerializer.IsValidComponentId);
			}
			return GetComponentBaseType(basePrefabPath, baseId, (TomlTable)baseComponentTable, projectAssembly);
		}
		
		if (componentTable.TryGetValue("Component", out object typeName))
			return Type.GetType((string)typeName) ?? throw new NullReferenceException($"Type {(string)typeName} does not exist");
		
		throw new InvalidOperationException($"Component table {id} of {prefabPath} does not contain a Component key or Prefab key");
	}
	
	public static Dictionary<string, (bool baseTypeIsIInitialize, Dictionary<string, string> events)> GetSerializedEvents(string prefabPath)
	{
		AssemblyLoadContext assemblyLoadContext = new("EchidnaProject", true);
		// AssemblyName assemblyName = new("EchidnaProject");
		// AssemblyDependencyResolver resolver = new(CompilationDllPath);
		using FileStream fileStream = new(CompilationDllPath, FileMode.Open, FileAccess.Read);
		Assembly assembly = assemblyLoadContext.LoadFromStream(fileStream);
		
		TomlTable table = Toml.ToModel(File.ReadAllText(prefabPath));
		Dictionary<string, (bool baseTypeIsIInitialize, Dictionary<string, string> events)> serializedEvents = new();
		foreach ((string id, object value) in table.Where(TomlSerializer.IsValidComponentId))
		{
			TomlTable valueTable = (TomlTable)value;
			Type baseType = GetComponentBaseType(prefabPath, id, valueTable, assembly);
			Dictionary<string, string> events = baseType
				.GetEvents()
				.Where(@event => @event.GetCustomAttributes<SerializedEventAttribute>().Any())
				.Where(@event => valueTable.ContainsKey(@event.Name))
				.ToDictionary(@event => @event.Name, @event => (string)valueTable[@event.Name]);
			if (events.Count != 0)
				serializedEvents.Add(id, (baseType.GetInterface("IInitialize") != null, events));
		}
		
		assemblyLoadContext.Unload();
		return serializedEvents;
	}
}
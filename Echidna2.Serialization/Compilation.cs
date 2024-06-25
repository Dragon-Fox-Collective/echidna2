using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using Tomlyn;
using Tomlyn.Model;
using static Echidna2.Serialization.SerializationPredicates;

namespace Echidna2.Serialization;

public static class Compilation
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
using OpenTK.Graphics.OpenGL4;
using System.Drawing;


""";
	
	public static async Task CompileCSProj(string prefabRootPath)
	{
		RecreateDirectory();
		CreateCSProj();
		
		foreach (string prefabPath in Directory.EnumerateFiles(prefabRootPath, "*.toml", SearchOption.AllDirectories))
			CreateCSFiles(prefabPath);
		
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
	</ItemGroup>

</Project>
""";
		File.WriteAllText($"{CompilationFolder}/EchidnaProject.csproj", csprojString);
	}
	
	public static void CreateCSFiles(string prefabPath)
	{
		TomlTable table = Toml.ToModel(File.ReadAllText(prefabPath));
		foreach ((string id, object componentTable) in table)
			if (ComponentNeedsCustomClass(id, (TomlTable)componentTable))
				CreateCSFile(prefabPath, id, (TomlTable)componentTable);
	}
	
	public static void CreateCSFile(string prefabPath, string id, TomlTable table)
	{
		List<TomlTable> components = table.TryGetValue("Components", out object? componentsArray) ? ((TomlArray)componentsArray).OfType<TomlTable>().ToList() : [];
		List<TomlTable> properties = table.TryGetValue("Properties", out object? propertiesArray) ? ((TomlArray)propertiesArray).OfType<TomlTable>().ToList() : [];
		List<TomlTable> events = table.TryGetValue("Events", out object? eventsArray) ? ((TomlArray)eventsArray).OfType<TomlTable>().ToList() : [];
		List<TomlTable> functions = table.TryGetValue("Functions", out object? functionsArray) ? ((TomlArray)functionsArray).OfType<TomlTable>().ToList() : [];
		List<string> interfaces = table.TryGetValue("Interfaces", out object? interfacesArray) ? ((TomlArray)interfacesArray).OfType<string>().ToList() : [];
		
		if (components.Count != 0)
			interfaces.Add("INotificationPropagator");
		foreach (TomlTable @event in events.Where(EventIsNotification))
			interfaces.Add($"INotificationListener<I{@event["Name"]}.Notification>");
		
		bool thisIsSubclass = id != "This";
		string className = thisIsSubclass ? $"{Path.GetFileNameWithoutExtension(prefabPath)}_{id}" : Path.GetFileNameWithoutExtension(prefabPath);
		if (thisIsSubclass)
			interfaces.Insert(0, GetComponentBaseTypeName(prefabPath, id, table));
		
		string scriptString = CSFileHeader;
		scriptString += $"public partial class {className}";
		if (interfaces.Count != 0)
			scriptString += " : " + string.Join(", ", interfaces) + "\n";
		else
			scriptString += "\n";
		scriptString += "{\n";
		
		if (components.Count != 0)
		{
			foreach (TomlTable component in components)
			{
				scriptString += $"\tprivate {component["Type"]} _{component["Name"]} = default!;\n";
				scriptString += $"\t[SerializedReference, ExposeMembersInClass] public {component["Type"]} {component["Name"]}\n";
				scriptString += "\t{\n";
				scriptString += $"\t\tget => _{component["Name"]};\n";
				scriptString += "\t\tset\n";
				scriptString += "\t\t{\n";
				
				scriptString += $"\t\t\tif (_{component["Name"]} is not null)\n";
				scriptString += $"\t\t\t\tUnsetup_{component["Name"]}();\n";
				scriptString += "\n";
				
				scriptString += $"\t\t\t_{component["Name"]} = value;\n";
				scriptString += "\n";
				
				scriptString += $"\t\t\tif (_{component["Name"]} is not null)\n";
				scriptString += $"\t\t\t\tSetup_{component["Name"]}();\n";
				
				scriptString += "\t\t}\n";
				scriptString += "\t}\n";
				
				scriptString += $"\tprotected virtual void Setup_{component["Name"]}()\n";
				scriptString += "\t{\n";
				foreach (TomlTable @event in events.Where(EventIsReferenceAndTargets(component)))
					scriptString += $"\t\t{component["Name"]}.{@event["Name"]} += {@event["Target"]}_{@event["Name"]};\n";
				scriptString += "\t}\n";
				
				scriptString += $"\tprotected virtual void Unsetup_{component["Name"]}()\n";
				scriptString += "\t{\n";
				foreach (TomlTable @event in events.Where(EventIsReferenceAndTargets(component)))
					scriptString += $"\t\t{component["Name"]}.{@event["Name"]} -= {@event["Target"]}_{@event["Name"]};\n";
				scriptString += "\t}\n";
				
				foreach (TomlTable @event in events.Where(EventIsReferenceAndTargets(component)))
				{
					if (((TomlArray)@event["Args"]).OfType<TomlTable>().Any(arg => arg.ContainsKey("CastTo")))
					{
						scriptString += $"\tprivate void {@event["Target"]}_{@event["Name"]}({string.Join(", ", ((TomlArray)@event["Args"]).OfType<TomlTable>().Select(arg => $"{arg["Type"]} {arg["Name"]}"))})";
						scriptString += $" => {@event["Target"]}_{@event["Name"]}({string.Join(", ", ((TomlArray)@event["Args"]).OfType<TomlTable>().Select(arg => arg.TryGetValue("CastTo", out object? castTo) ? $"({castTo}){arg["Name"]}!" : $"{arg["Name"]}"))});\n";
						scriptString += $"\tprivate void {@event["Target"]}_{@event["Name"]}({string.Join(", ", ((TomlArray)@event["Args"]).OfType<TomlTable>().Select(arg => arg.TryGetValue("CastTo", out object? castTo) ? $"{castTo} {arg["Name"]}" : $"{arg["Type"]} {arg["Name"]}"))})\n";
					}
					else
						scriptString += $"\tprivate void {@event["Target"]}_{@event["Name"]}({string.Join(", ", ((TomlArray)@event["Args"]).OfType<TomlTable>().Select(arg => $"{arg["Type"]} {arg["Name"]}"))})\n";
					scriptString += "\t{\n";
					scriptString += "\t\t" + ((string)@event["Content"]).Split("\n").Join("\n\t\t") + "\n";
					scriptString += "\t}\n";
				}
			}
			scriptString += "\n";
			
			scriptString += "\tpublic void Notify<T>(T notification) where T : notnull\n";
			scriptString += "\t{\n";
			foreach (TomlTable componentTable in components)
				scriptString += $"\t\tINotificationPropagator.Notify(notification, {componentTable["Name"]});\n";
			scriptString += "\t}\n\n";
		}
		
		foreach (TomlTable property in properties.Where(PropertyIsValue))
		{
			scriptString += $"\tprivate {property["Type"]} _{property["Name"]} = default!;\n";
			scriptString += $"\t[SerializedValue] public {property["Type"]} {property["Name"]}\n";
			scriptString += "\t{\n";
			if (property.TryGetValue("GetterContent", out object? getterContent))
			{
				scriptString += "\t\tget\n";
				scriptString += "\t\t{\n";
				scriptString += "\t\t\t" + ((string)getterContent).Split("\n").Join("\n\t\t\t") + "\n";
				scriptString += "\t\t}\n";
			}
			else
				scriptString += $"\t\tget => _{property["Name"]};\n";
			if (property.TryGetValue("SetterContent", out object? setterContent))
			{
				scriptString += "\t\tset\n";
				scriptString += "\t\t{\n";
				scriptString += "\t\t\t" + ((string)setterContent).Split("\n").Join("\n\t\t\t") + "\n";
				scriptString += "\t\t}\n";
			}
			else
				scriptString += $"\t\tset => _{property["Name"]} = value;\n";
			scriptString += "\t}\n";
			scriptString += "\n";
		}
		
		foreach (TomlTable property in properties.Where(PropertyIsReference))
		{
			scriptString += $"\tprivate {property["Type"]} _{property["Name"]} = default!;\n";
			scriptString += $"\t[SerializedReference] public {property["Type"]} {property["Name"]}\n";
			scriptString += "\t{\n";
			scriptString += $"\t\tget => _{property["Name"]};\n";
			scriptString += "\t\tset\n";
			scriptString += "\t\t{\n";
			
			scriptString += $"\t\t\tif (_{property["Name"]} is not null)\n";
			scriptString += $"\t\t\t\tUnsetup_{property["Name"]}();\n";
			scriptString += "\n";
			
			scriptString += $"\t\t\t_{property["Name"]} = value;\n";
			scriptString += "\n";
			
			scriptString += $"\t\t\tif (_{property["Name"]} is not null)\n";
			scriptString += $"\t\t\t\tSetup_{property["Name"]}();\n";
			
			scriptString += "\t\t}\n";
			scriptString += "\t}\n";
			
			scriptString += $"\tprotected virtual void Setup_{property["Name"]}()\n";
			scriptString += "\t{\n";
			foreach (TomlTable @event in events.Where(EventIsReferenceAndTargets(property)))
				scriptString += $"\t\t{@event["Target"]}.{@event["Name"]} += {@event["Target"]}_{@event["Name"]};\n";
			scriptString += "\t}\n";
			
			scriptString += $"\tprotected virtual void Unsetup_{property["Name"]}()\n";
			scriptString += "\t{\n";
			foreach (TomlTable @event in events.Where(EventIsReferenceAndTargets(property)))
				scriptString += $"\t\t{@event["Target"]}.{@event["Name"]} -= {@event["Target"]}_{@event["Name"]};\n";
			scriptString += "\t}\n";
			
			foreach (TomlTable @event in events.Where(EventIsReferenceAndTargets(property)))
			{
				if (((TomlArray)@event["Args"]).OfType<TomlTable>().Any(arg => arg.ContainsKey("CastTo")))
				{
					scriptString += $"\tprivate void {@event["Target"]}_{@event["Name"]}({string.Join(", ", ((TomlArray)@event["Args"]).OfType<TomlTable>().Select(arg => $"{arg["Type"]} {arg["Name"]}"))})";
					scriptString += $" => {@event["Target"]}_{@event["Name"]}({string.Join(", ", ((TomlArray)@event["Args"]).OfType<TomlTable>().Select(arg => arg.TryGetValue("CastTo", out object? castTo) ? $"({castTo}){arg["Name"]}!" : $"{arg["Name"]}"))});\n";
					scriptString += $"\tprivate void {@event["Target"]}_{@event["Name"]}({string.Join(", ", ((TomlArray)@event["Args"]).OfType<TomlTable>().Select(arg => arg.TryGetValue("CastTo", out object? castTo) ? $"{castTo} {arg["Name"]}" : $"{arg["Type"]} {arg["Name"]}"))})\n";
				}
				else
					scriptString += $"\tprivate void {@event["Target"]}_{@event["Name"]}({string.Join(", ", ((TomlArray)@event["Args"]).OfType<TomlTable>().Select(arg => $"{arg["Type"]} {arg["Name"]}"))})\n";
				scriptString += "\t{\n";
				scriptString += "\t\t" + ((string)@event["Content"]).Split("\n").Join("\n\t\t") + "\n";
				scriptString += "\t}\n";
			}
			
			scriptString += "\n";
		}
		
		foreach (TomlTable property in properties.Where(PropertyIsEvent))
		{
			scriptString += $"\tpublic event Action<{property["Type"]}>? {property["Name"]};\n";
		}
		scriptString += "\n";
		
		foreach (TomlTable @event in events.Where(EventIsSelf))
		{
			scriptString += $"\tprotected override void Setup_{@event["Target"]}()\n";
			scriptString += "\t{\n";
			scriptString += $"\t\tbase.Setup_{@event["Target"]}();\n";
			scriptString += $"\t\t{@event["Target"]}.{@event["Name"]} += {@event["Target"]}_{@event["Name"]};\n";
			scriptString += "\t}\n";
			
			scriptString += $"\tprotected override void Unsetup_{@event["Target"]}()\n";
			scriptString += "\t{\n";
			scriptString += $"\t\tbase.Setup_{@event["Target"]}();\n";
			scriptString += $"\t\t{@event["Target"]}.{@event["Name"]} -= {@event["Target"]}_{@event["Name"]};\n";
			scriptString += "\t}\n";
			
			if (((TomlArray)@event["Args"]).OfType<TomlTable>().Any(arg => arg.ContainsKey("CastTo")))
			{
				scriptString += $"\tprivate void {@event["Target"]}_{@event["Name"]}({string.Join(", ", ((TomlArray)@event["Args"]).OfType<TomlTable>().Select(arg => $"{arg["Type"]} {arg["Name"]}"))})";
				scriptString += $" => {@event["Target"]}_{@event["Name"]}({string.Join(", ", ((TomlArray)@event["Args"]).OfType<TomlTable>().Select(arg => arg.TryGetValue("CastTo", out object? castTo) ? $"({castTo}){arg["Name"]}!" : $"{arg["Name"]}"))});\n";
				scriptString += $"\tprivate void {@event["Target"]}_{@event["Name"]}({string.Join(", ", ((TomlArray)@event["Args"]).OfType<TomlTable>().Select(arg => arg.TryGetValue("CastTo", out object? castTo) ? $"{castTo} {arg["Name"]}" : $"{arg["Type"]} {arg["Name"]}"))})\n";
			}
			else
				scriptString += $"\tprivate void {@event["Target"]}_{@event["Name"]}({string.Join(", ", ((TomlArray)@event["Args"]).OfType<TomlTable>().Select(arg => $"{arg["Type"]} {arg["Name"]}"))})\n";
			scriptString += "\t{\n";
			scriptString += "\t\t" + ((string)@event["Content"]).Split("\n").Join("\n\t\t") + "\n";
			scriptString += "\t}\n";
		}
		
		foreach (TomlTable @event in events.Where(EventIsNotification))
		{
			scriptString += $"\tpublic void OnNotify(I{@event["Name"]}.Notification notification)\n";
			scriptString += "\t{\n";
			scriptString += "\t\t" + ((string)@event["Content"]).Split("\n").Join("\n\t\t") + "\n";
			scriptString += "\t}\n";
			scriptString += "\n";
		}
		
		foreach (TomlTable function in functions)
		{
			scriptString += $"\tpublic void {function["Name"]}({string.Join(", ", ((TomlArray)function["Args"]).OfType<TomlTable>().Select(arg => $"{arg["Type"]} {arg["Name"]}"))})\n";
			scriptString += "\t{\n";
			scriptString += "\t\t" + ((string)function["Content"]).Split("\n").Join("\n\t\t") + "\n";
			scriptString += "\t}\n";
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
		if (componentTable.TryGetValue("Prefab", out object basePrefab))
		{
			string basePrefabPath = $"{Path.GetDirectoryName(prefabPath)}/{(string)basePrefab}";
			(string baseId, object baseComponentTable) = Toml.ToModel(File.ReadAllText(basePrefabPath)).First();
			return GetComponentBaseTypeName(basePrefabPath, baseId, (TomlTable)baseComponentTable);
		}
		
		if (componentTable.TryGetValue("Component", out object typeName))
			return ((string)typeName).Split(",")[0];
		
		return Path.GetFileNameWithoutExtension(prefabPath) + (id is null or "This" ? "" : "_" + id);
	}
	
	public static Type GetComponentBaseType(string prefabPath, string? id, TomlTable componentTable, Assembly projectAssembly)
	{
		string className = Path.GetFileNameWithoutExtension(prefabPath) + (id is null or "This" ? "" : "_" + id);
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
				(baseId, baseComponentTable) = baseTable.First(IdIsValidComponentId);
			}
			return GetComponentBaseType(basePrefabPath, baseId, (TomlTable)baseComponentTable, projectAssembly);
		}
		
		if (componentTable.TryGetValue("Component", out object typeName))
			return Type.GetType((string)typeName) ?? throw new NullReferenceException($"Type {(string)typeName} does not exist");
		
		throw new InvalidOperationException($"Component table '{id}' of '{prefabPath}' does not contain a Component key, Prefab key, or class file ('{className}.cs')");
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
		foreach ((string id, object value) in table.Where(IdIsValidComponentId))
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
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
	
	public static void CompileCSProj(params string[] prefabPaths)
	{
		RecreateDirectory();
		CreateCSProj();
		
		foreach (string prefabPath in prefabPaths)
		{
			CreateCSFiles(prefabPath);
		}
		CompileCSProj();
		
		foreach (string prefabPath in prefabPaths)
		{
			Dictionary<string, (bool, Dictionary<string, string>)> serializedEvents = GetSerializedEvents(prefabPath);
			CreateCSFiles(prefabPath, serializedEvents);
		}
		CompileCSProj();
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
	    <Reference Include=""Echidna2""><HintPath>..\Echidna2.dll</HintPath></Reference>
	    <Reference Include=""Echidna2.Core""><HintPath>..\Echidna2.Core.dll</HintPath></Reference>
	    <Reference Include=""Echidna2.Gui""><HintPath>..\Echidna2.Gui.dll</HintPath></Reference>
	    <Reference Include=""Echidna2.Rendering""><HintPath>..\Echidna2.Rendering.dll</HintPath></Reference>
	    <Reference Include=""Echidna2.Serialization""><HintPath>..\Echidna2.Serialization.dll</HintPath></Reference>
    </ItemGroup>

</Project>
";
		File.WriteAllText($"{CompilationFolder}/EchidnaProject.csproj", csprojString);
	}
	
	public static void CreateCSFiles(string prefabPath, Dictionary<string, (bool, Dictionary<string, string>)>? serializedEvents = null)
	{
		TomlTable table = Toml.ToModel(File.ReadAllText(prefabPath));
		foreach ((string id, object value) in table)
		{
			TomlTable valueTable = (TomlTable)value;
			if (!valueTable.TryGetValue("ScriptContent", out object scriptContent))
				continue;
			
			string baseType = GetComponentBaseTypeName(prefabPath, id, valueTable);
			string className = $"{Path.GetFileNameWithoutExtension(prefabPath)}_{id}";
			(bool baseTypeIsIInitialize, Dictionary<string, string>? events) = serializedEvents?.GetValueOrDefault(id) ?? (false, null)!;
			bool shouldAddInitialize = events != null!;
			
			string scriptString = "";
			scriptString += "using Echidna2;\n";
			scriptString += "using Echidna2.Core;\n";
			scriptString += "using Echidna2.Gui;\n";
			scriptString += "using Echidna2.Rendering;\n";
			scriptString += "using Echidna2.Serialization;\n";
			scriptString += "\n";
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
	}
	
	public static void CompileCSProj()
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
		process.WaitForExit();
		Console.WriteLine(process.StandardOutput.ReadToEnd());
		Console.WriteLine(process.StandardError.ReadToEnd());
	}
	
	public static void RunCSProj()
	{
		Process process = new();
		process.StartInfo.FileName = "dotnet";
		process.StartInfo.Arguments = CompilationDllPath;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		process.Start();
		process.WaitForExit();
		Console.WriteLine(process.StandardOutput.ReadToEnd());
		Console.WriteLine(process.StandardError.ReadToEnd());
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
	
	public static Type GetComponentBaseType(string prefabPath, string id, TomlTable componentTable, Assembly projectAssembly)
	{
		string className = $"{Path.GetFileNameWithoutExtension(prefabPath)}_{id}";
		if (File.Exists(className + ".cs"))
			return projectAssembly.GetType(className) ?? throw new NullReferenceException($"Type {className} does not exist");
		
		if (componentTable.TryGetValue("Prefab", out object basePrefab))
		{
			string basePrefabPath = $"{Path.GetDirectoryName(prefabPath)}/{(string)basePrefab}";
			(string baseId, object baseComponentTable) = Toml.ToModel(File.ReadAllText(basePrefabPath)).First();
			return GetComponentBaseType(basePrefabPath, baseId, (TomlTable)baseComponentTable, projectAssembly);
		}
		
		if (!componentTable.TryGetValue("Component", out object typeName))
			throw new InvalidOperationException($"Component table {id} of {prefabPath} does not contain a Component key or Prefab key");
		
		return Type.GetType((string)typeName) ?? throw new NullReferenceException($"Type {(string)typeName} does not exist");
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
		foreach ((string id, object value) in table)
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
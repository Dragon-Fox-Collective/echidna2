using System.Diagnostics;
using Tomlyn;
using Tomlyn.Model;

namespace Echidna2.Serialization;

public class Compilation
{
	public static void RecreateDirectory()
	{
		Directory.Delete($"{AppContext.BaseDirectory}/.echidna", true);
		Directory.CreateDirectory($"{AppContext.BaseDirectory}/.echidna");
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
	    <Reference Include=""Echidna2.Rendering""><HintPath>..\Echidna2.Rendering.dll</HintPath></Reference>
	    <Reference Include=""Echidna2.Serialization""><HintPath>..\Echidna2.Serialization.dll</HintPath></Reference>
    </ItemGroup>

</Project>
";
		File.WriteAllText($"{AppContext.BaseDirectory}/.echidna/EchidnaProject.csproj", csprojString);
	}
	
	public static void CreateCSFiles(string prefabPath)
	{
		TomlTable table = Toml.ToModel(File.ReadAllText(prefabPath));
		foreach ((string id, object value) in table)
		{
			TomlTable valueTable = (TomlTable)value;
			if (!valueTable.TryGetValue("ScriptContent", out object scriptContent))
				continue;
			
			string baseType = GetComponentBaseType(prefabPath, id, valueTable);
			
			string className = $"{Path.GetFileNameWithoutExtension(prefabPath)}_{id}";
			
			string scriptString = "";
			scriptString += "using Echidna2;\n";
			scriptString += "using Echidna2.Rendering;\n";
			scriptString += "using Echidna2.Serialization;\n";
			scriptString += "\n";
			scriptString += $"public class {className} : {baseType}\n";
			scriptString += "{\n";
			scriptString += scriptContent;
			scriptString += "}\n";
			
			File.WriteAllText($"{AppContext.BaseDirectory}/.echidna/{className}.cs", scriptString);
		}
	}
	
	public static void CompileCSProj()
	{
		Process process = new();
		process.StartInfo.FileName = "dotnet";
		process.StartInfo.Arguments = "build";
		process.StartInfo.WorkingDirectory = $"{AppContext.BaseDirectory}/.echidna";
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
		process.StartInfo.Arguments = $"{AppContext.BaseDirectory}/.echidna/bin/Debug/net8.0/EchidnaProject.dll";
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		process.Start();
		process.WaitForExit();
		Console.WriteLine(process.StandardOutput.ReadToEnd());
		Console.WriteLine(process.StandardError.ReadToEnd());
	}
	
	public static string GetComponentBaseType(string prefabPath, string id, TomlTable componentTable)
	{
		string className = $"{Path.GetFileNameWithoutExtension(prefabPath)}_{id}";
		if (File.Exists(className))
			return className;
		
		if (componentTable.TryGetValue("Prefab", out object basePrefab))
		{
			string basePrefabPath = $"{Path.GetDirectoryName(prefabPath)}/{(string)basePrefab}";
			(string baseId, object baseComponentTable) = Toml.ToModel(File.ReadAllText(prefabPath)).First();
			return GetComponentBaseType(basePrefabPath, baseId, (TomlTable)baseComponentTable);
		}
		
		if (!componentTable.TryGetValue("Component", out object typeName))
			throw new InvalidOperationException($"Component table {id} of {prefabPath} does not contain a Component key or Prefab key");
		
		return ((string)typeName).Split(",")[0];
	}
	
	public static void CompileCSProj(string prefabPath)
	{
		RecreateDirectory();
		CreateCSProj();
		CreateCSFiles(prefabPath);
		CompileCSProj();
		// https://stackoverflow.com/questions/6258160/unloading-the-assembly-loaded-with-assembly-loadfrom
	}
}
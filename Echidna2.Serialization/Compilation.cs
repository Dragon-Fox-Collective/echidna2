using System.Diagnostics;
using Tomlyn;
using Tomlyn.Model;

namespace Echidna2.Serialization;

public class Compilation
{
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
			
			string className = $"{Path.GetFileNameWithoutExtension(prefabPath)}_{id}";
			
			string scriptString = "";
			scriptString += "using Echidna2.Rendering;\n";
			scriptString += "using Echidna2.Serialization;\n";
			scriptString += "\n";
			scriptString += $"public class {className}\n";
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
	
	public static void CompileCSProj(string prefabPath)
	{
		Directory.CreateDirectory($"{AppContext.BaseDirectory}/.echidna");
		CreateCSProj();
		CreateCSFiles(prefabPath);
		CompileCSProj();
	}
}
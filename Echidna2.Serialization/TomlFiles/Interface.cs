using Tomlyn;
using Tomlyn.Model;

namespace Echidna2.Serialization.TomlFiles;

public class Interface
{
	public string Path => $"{DottedPath.Replace(".", "/")}.prefab.toml";
	public string DottedPath => $"{Namespace.Value}.{ClassName}";
	public Usings Usings = new();
	public Namespace Namespace = new();
	public bool DontExpose = false;
	public string ClassName = "";
	public List<Generic> Generics = [];
	public bool GenericsInSeparateInterface = false;
	public List<Property> Properties = [];
	public List<Function> Functions = [];
	
	public static Interface FromToml(string prefabPath)
	{
		Interface @interface = new();
		TomlTable table = Toml.ToModel(File.ReadAllText(prefabPath));
		@interface.Usings = Usings.FromToml(table.GetList<string>("Using"));
		@interface.Namespace = Namespace.FromToml(prefabPath);
		@interface.DontExpose = !table.GetCasted("Expose", true);
		@interface.ClassName = Compilation.GetPrefabClassName(prefabPath);
		@interface.Generics = table.GetList<TomlTable>("Generics").Select(Generic.FromToml).ToList();
		@interface.GenericsInSeparateInterface = table.GetCasted("GenericsInSeparateInterface", false);
		@interface.Properties = table.GetList<TomlTable>("Properties").Select(propTable => Property.FromToml(propTable, [])).ToList();
		@interface.Functions = table.GetList<TomlTable>("Functions").Select(Function.FromToml).ToList();
		return @interface;
	}
	
	public string StringifyCS()
	{
		string scriptString = "";
		
		scriptString += Usings.StringifyCS();
		scriptString += Namespace.StringifyCS();
		
		if (DontExpose) scriptString += "[DontExpose]\n";
		
		scriptString += $"public interface {ClassName}";
		
		if (Generics.Count != 0 && !GenericsInSeparateInterface)
			scriptString += $"<{Generics.Select(generic => generic.StringifyCS()).Join(", ")}>";
		
		scriptString += "\n";
		scriptString += "{\n";
		
		scriptString += Properties.Select(property => property.StringifyCSAbstract() + "\n").Join("\n");
		
		scriptString += Functions.Select(function => function.StringifyCSAbstract() + "\n").Join("\n");
		
		scriptString += "}\n";
		scriptString += "\n";
		
		if (GenericsInSeparateInterface)
		{
			if (DontExpose)
				scriptString += "[DontExpose]\n";
			
			scriptString += $"public interface {ClassName}";
			scriptString += $"<{Generics.Select(generic => generic.StringifyCS()).Join(", ")}>";
			scriptString += $" : {ClassName}";
			scriptString += "\n";
			scriptString += "{\n";
			
			scriptString += Functions.Where(function => function.Args.Any(arg => arg.HasCast)).Select(function => function.StringifyCSAbstractOverride(ClassName) + "\n").Join();
			
			scriptString += "}\n";
			scriptString += "\n";
		}
		
		return scriptString;
	}
}
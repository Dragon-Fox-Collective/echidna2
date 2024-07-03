using Tomlyn;
using Tomlyn.Model;

namespace Echidna2.Serialization.TomlFiles;

public class Notification
{
	public Usings Usings = new();
	public Namespace Namespace = new();
	public string ClassName = "";
	public List<Generic> Generics = [];
	public List<Arg> Args = [];
	
	public static Notification FromToml(string prefabPath)
	{
		Notification notification = new();
		TomlTable table = Toml.ToModel(File.ReadAllText(prefabPath));
		notification.Usings = Usings.FromToml(table.GetList<string>("Using"));
		notification.Namespace = Namespace.FromToml(prefabPath);
		notification.ClassName = Compilation.GetPrefabClassName(prefabPath) + "_Notification";
		notification.Generics = table.GetList<TomlTable>("Generics").Select(Generic.FromToml).ToList();
		notification.Args = table.GetList<TomlTable>("Args").Select(Arg.FromToml).ToList();
		return notification;
	}
	
	public string StringifyCS()
	{
		string scriptString = "";
		scriptString += Usings.StringifyCS();
		scriptString += Namespace.StringifyCS();
		
		scriptString += $"public class {ClassName}";
		
		if (Generics.Count != 0)
			scriptString += $"<{Generics.Select(generic => generic.StringifyCS()).Join(", ")}>";
		
		scriptString += $"({Args.Select(arg => $"{arg.Type} _{arg.Name}").Join(", ")})";
		
		scriptString += "\n";
		scriptString += "{\n";
		
		scriptString += Args.Select(arg => $"\tpublic {arg.Type} {arg.Name} => _{arg.Name};\n").Join();
		
		scriptString += "}\n";
		return scriptString;
	}
}
using Tomlyn;
using Tomlyn.Model;
using static Echidna2.Serialization.SerializationPredicates;

namespace Echidna2.Serialization.TomlFiles;

public class Prefab
{
	public Usings Usings = new();
	public Namespace Namespace = new();
	public List<Component> Components = [];
	
	public bool NeedsCustomClass => Components.Any(c => c.NeedsCustomClass);
	
	public static Prefab FromToml(string prefabPath)
	{
		Prefab prefab = new();
		TomlTable table = Toml.ToModel(File.ReadAllText(prefabPath));
		prefab.Usings = Usings.FromToml(table.GetList<string>("Using"));
		prefab.Namespace = Namespace.FromToml(prefabPath);
		prefab.Components = [];
		foreach ((string id, object componentTable) in table.Where(IdIsValidComponentId))
			prefab.Components.Add(Component.FromToml(prefabPath, id, (TomlTable)componentTable));
		return prefab;
	}
	
	public string StringifyCS()
	{
		string scriptString = "";
		scriptString += Usings.StringifyCS();
		scriptString += Namespace.StringifyCS();
		scriptString += Components.Select(component => component.StringifyCS()).Join();
		return scriptString;
	}
}
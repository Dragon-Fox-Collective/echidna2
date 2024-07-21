using Tomlyn;
using Tomlyn.Model;
using static Echidna2.Serialization.SerializationPredicates;

namespace Echidna2.Serialization.TomlFiles;

public class Prefab
{
	public string Path => $"{DottedPath.Replace(".", "/")}.prefab.toml";
	public string DottedPath => $"{Namespace.Value}.{ThisComponent.ClassName}";
	public Usings Usings = new();
	public Namespace Namespace = new();
	public List<Component> Components = [];
	public Component ThisComponent => Components.First(c => c.IsRoot);
	public List<string> FavoriteFields = [];
	
	public bool NeedsCustomClass => Components.Any(c => c.NeedsCustomClass);
	
	public static Prefab FromToml(string prefabPath)
	{
		Prefab prefab = new();
		TomlTable table = Toml.ToModel(File.ReadAllText(prefabPath));
		prefab.Usings = Usings.FromToml(table.GetList<string>("Using"));
		prefab.FavoriteFields = table.GetList<string>("FavoriteFields");
		prefab.Namespace = Namespace.FromToml(prefabPath);
		prefab.Components = [];
		foreach ((string id, object componentTable) in table.Where(IdIsValidComponentId))
			prefab.Components.Add(Component.FromToml(prefabPath, id, (TomlTable)componentTable));
		return prefab;
	}
	
	public TomlTable ToToml()
	{
		TomlTable table = new();
		table.Add("FavoriteFields", FavoriteFields);
		table.Add("Using", Usings.ToToml());
		foreach (Component component in Components)
			table.Add(component.Id, component.ToToml());
		return table;
	}
	
	public void Save()
	{
		File.WriteAllText(Path, CustomTomlStringifier.Stringify(ToToml()));
	}
	
	public string StringifyCS()
	{
		string scriptString = "";
		scriptString += Usings.StringifyCS();
		scriptString += Namespace.StringifyCS();
		scriptString += Components.Select(component => component.StringifyCS()).Join();
		return scriptString;
	}
	
	public Component this[string id] => Components.First(c => c.Id == id);
	
	public override string ToString() => Path;
}
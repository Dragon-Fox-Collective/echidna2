using Tomlyn.Model;

namespace Echidna2.Serialization.TomlFiles;

public class Component
{
	public string Id = "";
	public ComponentSource Source = new NoSource();
	public bool IsRoot => Id == "This";
	public bool IsSubclass => Source is not NoSource && NeedsCustomClass;
	private string baseClassName = "";
	public string ClassName = "";
	public List<string> Interfaces = [];
	public List<Property> Properties = [];
	public IEnumerable<Property> Components => Properties.Where(property => property.PropertyType == PropertyType.Component);
	public List<EventListener> EventsListeners = [];
	public List<Function> Functions = [];
	public Dictionary<string, object> Values = [];
	
	public bool NeedsCustomClass => Components.Any() || Properties.Count != 0 || EventsListeners.Count != 0 || Functions.Count != 0 || Interfaces.Count > 1;
	
	public static Component FromToml(string prefabPath, string id, TomlTable table)
	{
		Component component = new();
		component.Id = id;
		component.Source =
			table.TryGetValue("Component", out object? typeValue) ? new TypeSource((string)typeValue) :
			table.TryGetValue("Prefab", out object? prefabPathValue) ? new PrefabSource((string)prefabPathValue) :
			new NoSource();
		component.ClassName = Compilation.GetPrefabClassName(prefabPath, id);
		component.Interfaces = table.GetList<string>("Interfaces");
		component.EventsListeners = table.GetList<TomlTable>("Events").Select(EventListener.FromToml).ToList();
		component.Properties = table.GetList<TomlTable>("Properties").Select(propTable => Property.FromToml(propTable, component.EventsListeners)).ToList();
		component.Functions = table.GetList<TomlTable>("Functions").Select(Function.FromToml).ToList();
		component.Values = table.GetDict("Values");
		if (component.IsSubclass)
			component.baseClassName = Compilation.GetComponentBaseTypeName(prefabPath, id, table);
		return component;
	}
	
	public TomlTable ToToml()
	{
		TomlTable table = new();
		if (Source is TypeSource typeSource) table.Add("Component", typeSource.Type);
		if (Source is PrefabSource prefabSource) table.Add("Prefab", prefabSource.Path);
		if (Interfaces.Count != 0) table.Add("Interfaces", Interfaces);
		if (EventsListeners.Count != 0) table.Add("Events", EventsListeners.Select(@event => @event.ToToml()).ToList());
		if (Properties.Count != 0) table.Add("Properties", Properties.Select(prop => prop.ToToml()).ToList());
		if (Functions.Count != 0) table.Add("Functions", Functions.Select(func => func.ToToml()).ToList());
		if (Values.Count != 0) table.Add("Values", Values);
		return table;
	}
	
	public string StringifyCS()
	{
		if (!NeedsCustomClass) return "";
		
		List<string> interfaces = Interfaces.ToList();
		if (Components.Any())
			interfaces.Add("INotificationPropagator");
		foreach (EventListener @event in EventsListeners.Where(@event => @event.EventType == EventType.Notification))
			interfaces.Add($"INotificationListener<{@event.Name}Notification>");
		if (IsSubclass)
			interfaces.Insert(0, baseClassName);
		
		string scriptString = "";
		scriptString += $"public partial class {ClassName}";
		if (interfaces.Count != 0) scriptString += " : " + interfaces.Join(", ");
		scriptString += "\n";
		scriptString += "{\n";
		scriptString += Properties.Select(property => property.StringifyCS()).Join();
		if (Components.Any()) scriptString += "\n" + StringifyCSNotify();
		scriptString += "\n";
		scriptString += EventsListeners.Where(@event => @event.EventType is EventType.Self).GroupBy(@event => @event.Target).Select(group => StringifyCSEventListenersSelf(group.Key, group.ToArray()) + "\n").Join();
		scriptString += EventsListeners.Where(@event => @event.EventType is EventType.Notification).Select(@event => @event.StringifyCS() + "\n").Join();
		scriptString += "\n";
		scriptString += Functions.Select(function => function.StringifyCS()).Join();
		scriptString += "}\n";
		scriptString += "\n";
		return scriptString;
	}
	public string StringifyCSNotify()
	{
		string scriptString = "";
		scriptString += "\tpublic void Notify<T>(T notification) where T : notnull\n";
		scriptString += "\t{\n";
		scriptString += $"\t\tINotificationPropagator.Notify(notification, {Components.Select(component => component.Name).Join(", ")});\n";
		scriptString += "\t}\n";
		scriptString += "\n";
		return scriptString;
	}
	
	private static string StringifyCSEventListenersSelf(string target, ICollection<EventListener> eventListeners)
	{
		string scriptString = "";
		scriptString += $"\tprotected override void Setup_{target}()\n";
		scriptString += "\t{\n";
		scriptString += $"\t\tbase.Setup_{target}();\n";
		foreach (EventListener @event in eventListeners)
			scriptString += @event.StringifyCSAdd();
		scriptString += "\t}\n";
		
		scriptString += $"\tprotected override void Unsetup_{target}()\n";
		scriptString += "\t{\n";
		scriptString += $"\t\tbase.Unsetup_{target}();\n";
		foreach (EventListener @event in eventListeners)
			scriptString += @event.StringifyCSSub();
		scriptString += "\t}\n";
		
		foreach (EventListener @event in eventListeners)
			scriptString += @event.StringifyCS();
		
		scriptString += "\n";
		
		return scriptString;
	}
	
	public override string ToString() => Source is NoSource ? ClassName : $"{Source} {ClassName}";
}

public class ComponentSource;

public class NoSource : ComponentSource;

public class PrefabSource(string path) : ComponentSource
{
	public string Path = path;
	
	public override string ToString() => Path.Split(".").Last();
}

public class TypeSource(string type) : ComponentSource
{
	public string Type = type;
	
	public override string ToString() => Type.Split(",").First().Split(".").Last();
}
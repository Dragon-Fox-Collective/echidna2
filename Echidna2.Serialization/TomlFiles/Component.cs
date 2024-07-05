using Tomlyn.Model;

namespace Echidna2.Serialization.TomlFiles;

public class Component
{
	public string Id = "";
	public ComponentSource Source = new NoSource();
	public bool IsRoot => Id == "This";
	public bool IsSubclass => Id != "This";
	public string ClassName = "";
	public List<string> Interfaces = [];
	public List<Property> Properties = [];
	public IEnumerable<Property> Components => Properties.Where(property => property.PropertyType == PropertyType.Component);
	public List<EventListener> EventsListeners = [];
	public List<Function> Functions = [];
	public Dictionary<string, object> Values = [];
	
	public bool NeedsCustomClass => IsSubclass
		? Components.Any() || Properties.Count != 0 || EventsListeners.Count != 0 || Functions.Count != 0 || Interfaces.Count > 1
		: Source is NoSource;
	
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
		
		if (component.Components.Any())
			component.Interfaces.Add("INotificationPropagator");
		
		foreach (EventListener @event in component.EventsListeners.Where(@event => @event.EventType == EventType.Notification))
			component.Interfaces.Add($"INotificationListener<{@event.Name}_Notification>");
		
		if (component.IsSubclass)
			component.Interfaces.Insert(0, Compilation.GetComponentBaseTypeName(prefabPath, id, table));
		
		return component;
	}
	
	public string StringifyCS()
	{
		if (!NeedsCustomClass) return "";
		
		string scriptString = "";
		scriptString += $"public partial class {ClassName}";
		if (Interfaces.Count != 0) scriptString += " : " + Interfaces.Join(", ");
		scriptString += "\n";
		scriptString += "{\n";
		scriptString += Properties.Select(property => property.StringifyCS()).Join();
		if (Components.Any()) scriptString += "\n" + StringifyCSNotify();
		scriptString += "\n";
		scriptString += EventsListeners.Where(@event => @event.EventType is EventType.Self or EventType.Notification).Select(@event => @event.StringifyCS() + "\n").Join();
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
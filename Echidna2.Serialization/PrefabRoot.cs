using Echidna2.Serialization.TomlFiles;
using TooManyExtensions;

namespace Echidna2.Serialization;

public class PrefabRoot
{
	public List<PrefabInstance> ChildPrefabs = [];
	public List<object> Components = [];
	
	public List<ComponentPair> ComponentPairs = [];
	
	public List<(ComponentPair Component, IMemberWrapper Member)> FavoriteFields = [];
	
	public object RootObject = null!;
	public Component RootComponent => Prefab.ThisComponent;
	public ComponentPair RootPair => new(RootObject, RootComponent);
	public Prefab Prefab = null!;
	
	public bool Owns(object component) => Components.Contains(component) || ChildPrefabs.Any(child => child.PrefabRoot.RootObject == component);
	public bool Owns(ComponentPair pair) => Owns(pair.Object);
	
	public IEnumerable<IMemberWrapper> GetAllSerializedFieldsOf(ComponentPair rootComponent)
	{
		List<IMemberWrapper> foundFields = [];
		List<ComponentPair> foundComponents = [];
		List<ComponentPair> componentsToSearch = [rootComponent];
		
		while (componentsToSearch.TryPop(out ComponentPair component))
		{
			if (foundComponents.Contains(component)) continue;
			foundComponents.Add(component);
			
			foundFields.AddRange(component.Component.Properties
				.Where(prop => prop.PropertyType is PropertyType.Component or PropertyType.Reference or PropertyType.Value)
				.Select(prop => IMemberWrapper.Wrap(component.Object.GetType().GetMember(prop.Name).FirstOrNone().Expect($"Component '{component}' has no field '{prop.Name}' (out of {component.Object.GetType().GetMembers().ToDelimString()})"))));
			
			if (component.SourceComponent.TrySome(out ComponentPair source))
				componentsToSearch.Add(source);
			
			componentsToSearch.AddRange(component.Component.Properties
				.Where(prop => prop.ExposeProperties)
				.Select(prop => IMemberWrapper.Wrap(component.Object.GetType().GetMember(prop.Name).FirstOrNone().Expect($"Component '{component}' has no field '{prop.Name}' (out of {component.Object.GetType().GetMembers().ToDelimString()})")))
				.Select(member => member.GetValue(component.Object))
				.WhereNotNull()
				.Select(obj => new ComponentPair(obj, this[obj])));
		}
		
		return foundFields;
	}
	
	public IEnumerable<ComponentPair> GetAllComponentsOf(ComponentPair rootComponent)
	{
		List<ComponentPair> foundComponents = [];
		List<ComponentPair> componentsToSearch = [rootComponent];
		
		while (componentsToSearch.TryPop(out ComponentPair component))
		{
			if (foundComponents.Contains(component)) continue;
			foundComponents.Add(component);
			
			componentsToSearch.AddRange(component.Component.Properties
				.Where(prop => prop.PropertyType is PropertyType.Component)
				.Select(prop => (string)component.Component.Values.Get(prop.Name).Expect($"Component '{component.Component}' has no value for property {prop.Name} (out of {component.Component.Values.ToDelimString()})"))
				.Select(id => ComponentPairs.First(pair => pair.Component.Id == id, $"Prefab '{Prefab}' has no pair for id '{id}' (out of {ComponentPairs.ToDelimString()})").Unwrap()));
		}
		
		return foundComponents;
	}
	
	public IEnumerable<ComponentPair> GetAllOwnedComponentsOf(ComponentPair rootComponent) => GetAllComponentsOf(rootComponent).Where(Owns);
	
	public object AddComponent(string componentPath, object connectedObject)
	{
		object component = Project.Instantiate<object>(componentPath);
		string id = Enumerable.Range(0, (int)Math.Ceiling(128 / 3.0)).Select(_ => Random.Shared.Choose(["0", "1", "2", "3", "4", "5", "6", "7"])).Join(); // GUIDs stay mad
		Component data = new();
		data.Id = id;
		data.Source = new PrefabSource(componentPath);
		Components.Add(component);
		ComponentPairs.Add(new ComponentPair(component, data));
		Prefab.Components.Add(data);
		
		Property property = new();
		property.Name = componentPath.SplitLast('.').Item2;
		property.Type = componentPath;
		property.PropertyType = PropertyType.Component;
		property.ExposeProperties = true;
		
		Component connectedData = this[connectedObject];
		connectedData.Properties.Add(property);
		connectedData.Values.Add(property.Name, id);
		
		Prefab.Save();
		
		return component;
	}
	
	public Option<Component> GetComponent(object obj) => ComponentPairs.FirstOrNone(pair => pair.Object == obj)
		.Map(pair => pair.Component)
		.OrElse(() => ChildPrefabs.Select(child => child.PrefabRoot.GetComponent(obj)).FirstSome());
	public Component this[object obj] => GetComponent(obj)
		.Expect($"Prefab '{Prefab}' has no pair for object '{obj}' (out of {ComponentPairs.ToDelimString()})");
}

public class PrefabInstance(PrefabRoot prefabRoot)
{
	public PrefabRoot PrefabRoot => prefabRoot;
}
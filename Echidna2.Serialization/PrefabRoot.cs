using System.Reflection;
using Echidna2.Serialization.TomlFiles;
using TooManyExtensions;

namespace Echidna2.Serialization;

public class PrefabRoot
{
	public List<PrefabInstance> ChildPrefabs = [];
	public List<object> Components = [];
	
	public List<(object, Component)> ComponentPairs = [];
	
	public List<(object Component, MemberInfo Field)> FavoriteFields = [];
	
	public object RootObject = null!;
	public Prefab Prefab = null!;
	
	public void SerializeChange(object component, string member, object value)
	{
		Component connectedData = ComponentPairs.First(pair => pair.Item1 == component).Item2;
		connectedData.Values[member] = value;
	}
	
	public bool Owns(object component) => Components.Contains(component) || ChildPrefabs.Any(child => child.PrefabRoot.RootObject == component);
	
	public IEnumerable<object> GetAllComponentsOf(object rootComponent)
	{
		List<object> foundComponents = [];
		List<object> componentsToSearch = [rootComponent];
		
		while (componentsToSearch.TryPop(out object? component))
		{
			if (foundComponents.Contains(component)) continue;
			foundComponents.Add(component);
			
			Component componentData = ComponentPairs.First(pair => pair.Item1 == component, $"Prefab '{Prefab}' has no pair for object '{component}' (out of {ComponentPairs.ToDelimString()})").Unwrap().Item2;
			componentsToSearch.AddRange(componentData.Properties
				.Where(prop => prop.PropertyType is PropertyType.Component)
				.Select(prop => (string)componentData.Values.Get(prop.Name).Expect($"Component '{componentData}' has no value for property {prop.Name} (out of {componentData.Values.ToDelimString()})"))
				.Select(id => ComponentPairs.First(pair => pair.Item2.Id == id, $"Prefab '{Prefab}' has no pair for id '{id}' (out of {ComponentPairs.ToDelimString()})").Unwrap().Item1));
		}
		
		return foundComponents;
	}
	
	public IEnumerable<object> GetAllOwnedComponentsOf(object rootComponent) => GetAllComponentsOf(rootComponent).Where(Owns);
	
	public object AddComponent(string componentPath, object connectedObject)
	{
		object component = Project.Instantiate<object>(componentPath);
		string id = Enumerable.Range(0, (int)Math.Ceiling(128 / 3.0)).Select(_ => Random.Shared.Choose(["0", "1", "2", "3", "4", "5", "6", "7"])).Join(); // GUIDs stay mad
		Component data = new();
		data.Id = id;
		data.Source = new PrefabSource(componentPath);
		Components.Add(component);
		ComponentPairs.Add((component, data));
		Prefab.Components.Add(data);
		
		Property property = new();
		property.Name = componentPath.SplitLast('.').Item2;
		property.Type = componentPath;
		property.PropertyType = PropertyType.Component;
		
		Component connectedData = ComponentPairs.First(pair => pair.Item1 == connectedObject).Item2;
		connectedData.Properties.Add(property);
		connectedData.Values.Add(property.Name, id);
		
		Prefab.Save();
		
		return component;
	}
}

public class PrefabInstance(PrefabRoot prefabRoot)
{
	public PrefabRoot PrefabRoot => prefabRoot;
}
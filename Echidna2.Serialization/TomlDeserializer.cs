using System.Reflection;
using Echidna2.Serialization.TomlFiles;
using Tomlyn.Model;

namespace Echidna2.Serialization;

public partial class Project
{
	/// <param name="prefab"></param>
	/// <param name="rootTypeName">Like Editor_06 instead of Editor</param>
	private PrefabRoot Deserialize(Prefab prefab, string? rootTypeName = null)
	{
		List<(object Component, Component ComponentTable)> components = [];
		
		PrefabRoot prefabRoot = new();
		prefabRoot.Prefab = prefab;
		
		foreach (Component componentTable in prefab.Components)
		{
			string? typeName = (componentTable.IsRoot ? rootTypeName : null) ?? (componentTable.NeedsCustomClass ? componentTable.ClassName : null);
			
			object component;
			switch (componentTable.Source)
			{
				case TypeSource source:
					component = DeserializeComponent(componentTable.Id, typeName ?? source.Type, typeName != null);
					prefabRoot.Components.Add(component);
					prefabRoot.ComponentPairs.Add((component, componentTable));
					break;
				
				case PrefabSource source:
					PrefabRoot componentPrefabRoot = Deserialize(Prefabs[source.Path], typeName);
					component = componentPrefabRoot.RootObject;
					PrefabInstance componentPrefabInstance = new(componentPrefabRoot);
					prefabRoot.ChildPrefabs.Add(componentPrefabInstance);
					prefabRoot.ComponentPairs.Add((component, componentTable));
					break;
				
				default:
					component = DeserializeComponent(componentTable.Id, typeName ?? componentTable.ClassName, true);
					prefabRoot.Components.Add(component);
					prefabRoot.ComponentPairs.Add((component, componentTable));
					break;
			}
			
			if (componentTable.IsRoot)
				prefabRoot.RootObject = component;
			
			components.Add((component, componentTable));
		}
		
		foreach ((object component, Component componentTable) in components)
			DeserializeReference(component, componentTable.Values, GetReferenceFrom);
		
		foreach ((object component, Component componentTable) in components)
			DeserializeValue(component, componentTable.Values);
		
		prefabRoot.FavoriteFields = prefab.FavoriteFields.Select(refPath =>
			{
				(string componentPath, string fieldPath) = refPath.SplitLast('.');
				object component = componentPath.IsEmpty() ? prefabRoot.RootObject : GetReferenceFrom(componentPath);
				MemberInfo? field = component.GetType().GetMember(fieldPath).FirstOrDefault();
				if (field == null)
				{
					Console.WriteLine($"WARN: Field '{refPath}' of '{prefab}' does not exist");
					return default;
				}
				return (component, field);
			})
				.Where(pair => pair != default)
				.ToList();
		
		return prefabRoot;
		
		
		object GetReferenceFrom(string refPath)
		{
			while (true)
			{
				if (refPath.IsEmpty()) throw new InvalidOperationException("Reference path part is empty");
				
				(string id, string rest) = refPath.SplitFirst('.');
				(object value, Component valueTable) = components.FirstOrDefault(pair => pair.ComponentTable.Id == id);
				if (value == null) throw new InvalidOperationException($"Component of id '{id}' in prefab '{prefab}' does not exist");
				
				if (rest.IsEmpty()) return value;
				
				refPath = (string)rest.Split('.')
					.Aggregate<string, object>(valueTable, (current, name) => ((TomlTable)current)[name]);
			}
		}
	}
	
	private object DeserializeComponent(string id, string typeName, bool useProjectAssembly)
	{
		Type? type = useProjectAssembly ? Assembly.DefinedTypes.FirstOrDefault(type => type.Name == typeName) : Type.GetType(typeName);
		if (type == null)
			throw new InvalidOperationException($"Type '{typeName}' of id '{id}' does not exist");
		
		ConstructorInfo? constructor = type.GetConstructor([]);
		if (constructor == null)
			throw new InvalidOperationException($"Type '{type}' of id '{id}' does not have a parameterless constructor");
		
		return constructor.Invoke([]);
	}
	
	private void DeserializeReference(object component, Dictionary<string, object> values, Func<string, object> getReferenceFrom)
	{
		MemberInfo[] members = component.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
			.Where(member => member.GetCustomAttribute<SerializedReferenceAttribute>() is not null)
			.Where(member => values.ContainsKey(member.Name))
			.ToArray();
		
		foreach (MemberInfo member in members)
		{
			IMemberWrapper wrapper = IMemberWrapper.Wrap(member);
			ReferenceSerializer serializer = wrapper.GetCustomAttribute<SerializedReferenceAttribute>()!.GetSerializer(wrapper.FieldType);
			object serializedValue = values[member.Name];
			object deserializedValue = serializer.Deserialize(wrapper.GetValue(component), serializedValue, getReferenceFrom);
			wrapper.SetValue(component, deserializedValue);
		}
	}
	
	private object DeserializeValue(object component, Dictionary<string, object> values)
	{
		MemberInfo[] members = component.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
			.Where(member => member.GetCustomAttribute<SerializedValueAttribute>() != null)
			.Where(member => values.ContainsKey(member.Name))
			.ToArray();
		
		foreach (MemberInfo member in members)
		{
			IMemberWrapper wrapper = IMemberWrapper.Wrap(member);
			Serializer serializer = wrapper.GetCustomAttribute<SerializedValueAttribute>()!.GetSerializer(wrapper.FieldType, data => DeserializeValue(wrapper.GetValue(component)!, data.ToDictionary()));
			object serializedValue = values[member.Name];
			object deserializedValue = serializer.Deserialize(wrapper.GetValue(component), serializedValue);
			wrapper.SetValue(component, deserializedValue);
		}
		
		return component;
	}
	
	public static PrefabRoot Deserialize(string path) => Singleton!.Deserialize(Singleton.Prefabs[path]);
	public static T Instantiate<T>(string path) => (T)Singleton!.Deserialize(Singleton.Prefabs[path]).RootObject;
}
using System.Reflection;
using Echidna2.Serialization.TomlFiles;
using TooManyExtensions;

namespace Echidna2.Serialization;

public partial class Project
{
	/// <param name="prefab"></param>
	/// <param name="rootTypeName">Like Editor_06 instead of Editor</param>
	private PrefabRoot Deserialize(Prefab prefab, string? rootTypeName = null)
	{
		List<ComponentPair> components = [];
		
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
					prefabRoot.ComponentPairs.Add(new ComponentPair(component, componentTable));
					break;
				
				case PrefabSource source:
					PrefabRoot componentPrefabRoot = Deserialize(Prefabs[source.Path], typeName);
					component = componentPrefabRoot.RootObject;
					PrefabInstance componentPrefabInstance = new(componentPrefabRoot);
					prefabRoot.ChildPrefabs.Add(componentPrefabInstance);
					prefabRoot.ComponentPairs.Add(new ComponentPair(component, componentTable));
					break;
				
				default:
					component = DeserializeComponent(componentTable.Id, typeName ?? componentTable.ClassName, true);
					prefabRoot.Components.Add(component);
					prefabRoot.ComponentPairs.Add(new ComponentPair(component, componentTable));
					break;
			}
			
			if (componentTable.IsRoot)
				prefabRoot.RootObject = component;
			
			components.Add(new ComponentPair(component, componentTable));
		}
		
		foreach (ComponentPair pair in components)
			DeserializeReference(pair.Object, pair.Component.Values, GetReferenceFrom);
		
		foreach (ComponentPair pair in components)
			DeserializeValue(pair.Object, pair.Component.Values);
		
		prefabRoot.FavoriteFields = prefab.FavoriteFields.Select(refPath =>
			{
				(string componentPath, string fieldPath) = refPath.SplitLast('.');
				ComponentPair pair = componentPath.IsEmpty() ? prefabRoot.RootPair : GetReferenceAndComponentFrom(componentPath);
				MemberInfo? field = pair.Object.GetType().GetMember(fieldPath).FirstOrDefault();
				if (field == null)
				{
					Console.WriteLine($"WARN: Field '{refPath}' of '{prefab}' does not exist");
					return Option.None<(ComponentPair, IMemberWrapper)>();
				}
				return Option.Some((pair, IMemberWrapper.Wrap(field)));
			})
				.WhereSome()
				.ToList();
		
		return prefabRoot;
		
		
		ComponentPair GetReferenceAndComponentFrom(string id)
		{
			if (id.IsEmpty()) throw new InvalidOperationException("Reference id is empty");
			ComponentPair pair = components.FirstOrNone(pair => pair.Component.Id == id).Expect($"Component of id '{id}' in prefab '{prefab}' does not exist");
			return pair;
		}
		
		object GetReferenceFrom(string refPath) => GetReferenceAndComponentFrom(refPath).Object;
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
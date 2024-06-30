using System.Reflection;
using Tomlyn;
using Tomlyn.Model;
using TooManyExtensions;
using static Echidna2.Serialization.SerializationPredicates;

namespace Echidna2.Serialization;

public static class TomlDeserializer
{
	public static Assembly ProjectAssembly = null!;
	
	public static PrefabRoot Deserialize(string path, string? overridenTypeName = null)
	{
		Dictionary<string, (object Component, TomlTable ComponentTable)> components = [];
		
		PrefabRoot prefabRoot = new();
		prefabRoot.PrefabPath = path;
		
		TomlTable table = Toml.ToModel(File.ReadAllText(path));
		
		if (!table.ContainsKey("This"))
			throw new InvalidOperationException($"'This' root object not found in '{path}'");
		
		foreach ((string id, object value) in table.Where(IdIsValidComponentId))
		{
			TomlTable componentTable = (TomlTable)value;
			
			string? scriptName = id == "This" ? overridenTypeName : ComponentNeedsCustomClass(id, componentTable) ? $"{Path.GetFileNameWithoutExtension(path)}_{id}" : null;
			
			object component;
			if (componentTable.Remove("Component", out object? typeName))
			{
				component = DeserializeComponent(id, scriptName ?? (string)typeName, scriptName != null);
				prefabRoot.Components.Add(component);
			}
			else if (componentTable.Remove("Prefab", out object? prefabPath))
			{
				PrefabRoot componentPrefabRoot = Deserialize(Path.Join(Path.GetDirectoryName(path), (string)prefabPath), scriptName);
				PrefabInstance componentPrefabInstance = new(componentPrefabRoot);
				prefabRoot.ChildPrefabs.Add(componentPrefabInstance);
				component = componentPrefabRoot.RootObject;
			}
			else
			{
				component = DeserializeComponent(id, Path.GetFileNameWithoutExtension(path), true);
			}
			
			if (id == "This")
				prefabRoot.RootObject = component;
			
			components.Add(id, (component, componentTable));
		}
		
		foreach ((string id, (object component, TomlTable componentTable)) in components)
			if (componentTable.TryGetValue("Values", out object? values))
				DeserializeReference(prefabRoot, id, new ComponentPath(component), component, (TomlTable)values, GetReferenceFrom);
		
		foreach ((string id, (object component, TomlTable componentTable)) in components)
			if (componentTable.TryGetValue("Values", out object? values))
				DeserializeValue(prefabRoot, id, new ComponentPath(component), component, (TomlTable)values);
		
		foreach ((string id, (object _, TomlTable componentTable)) in components)
			if (componentTable.TryGetValue("Values", out object? values) && ((TomlTable)values).Count != 0)
				Console.WriteLine($"WARN: Unused table {id} {((TomlTable)values).ToDelimString()} of prefab '{path}' leftover");
		
		if (prefabRoot.RootObject == null)
			throw new InvalidOperationException($"Root object was not deserialized in '{path}'");
		
		prefabRoot.FavoriteFields = ((TomlTable)table["This"]).TryGetValue("FavoriteFields", out object? fields)
			? ((TomlArray)fields).Select(refPath =>
			{
				(string componentPath, string fieldPath) = ((string)refPath).SplitLast('.');
				object component = componentPath.IsEmpty() ? prefabRoot.RootObject : GetReferenceFrom(componentPath);
				MemberInfo? field = component.GetType().GetMember(fieldPath).FirstOrDefault();
				if (field == null)
				{
					Console.WriteLine($"WARN: Field {refPath} of {path} does not exist");
					return default;
				}
				return (component, field);
			})
				.Where(pair => pair != default)
				.ToList()
			: [];
		
		return prefabRoot;
		
		
		object? GetReferenceFrom(string refPath)
		{
			while (true)
			{
				if (refPath.IsEmpty()) return null;
				
				(string id, string rest) = refPath.SplitFirst('.');
				(object value, TomlTable valueTable) = components[id];
				
				if (rest.IsEmpty()) return value;
				
				refPath = (string)rest.Split('.')
					.Aggregate<string, object>(valueTable, (current, name) => ((TomlTable)current)[name]);
			}
		}
	}
	
	private static object DeserializeComponent(string id, string typeName, bool useProjectAssembly)
	{
		Type? type = useProjectAssembly ? ProjectAssembly.DefinedTypes.FirstOrDefault(type => type.Name == typeName) : Type.GetType(typeName);
		if (type == null)
			throw new InvalidOperationException($"Type '{typeName}' of id '{id}' does not exist");
		
		ConstructorInfo? constructor = type.GetConstructor([]);
		if (constructor == null)
			throw new InvalidOperationException($"Type '{type}' of id '{id}' does not have a parameterless constructor");
		
		return constructor.Invoke([]);
	}
	
	private static void DeserializeReference(PrefabRoot prefabRoot, string id, IMemberPath path, object component, TomlTable componentTable, Func<string, object> getReferenceFrom)
	{
		List<string> usedValues = [];
		
		MemberInfo[] members = component.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
			.Where(member => member.GetCustomAttribute<SerializedReferenceAttribute>() is not null)
			.Where(member => componentTable.ContainsKey(member.Name))
			.ToArray();
		
		foreach (MemberInfo member in members)
		{
			IMemberWrapper wrapper = IMemberWrapper.Wrap(member);
			MemberPath memberPath = new(wrapper, path);
			ReferenceSerializer serializer = wrapper.GetCustomAttribute<SerializedReferenceAttribute>()!.GetSerializer(wrapper.FieldType);
			object serializedValue = componentTable[member.Name];
			object deserializedValue = serializer.Deserialize(wrapper.GetValue(component), serializedValue, getReferenceFrom);
			wrapper.SetValue(component, deserializedValue);
			prefabRoot.RegisterChange(memberPath);
			usedValues.Add(member.Name);
		}
		
		foreach (string usedValue in usedValues)
		{
			if (componentTable[usedValue] is TomlTable usedTable && usedTable.Count != 0)
				Console.WriteLine($"WARN: Unused table {usedValue} {usedTable.ToDelimString()} of {id} leftover");
			componentTable.Remove(usedValue);
		}
	}
	
	private static object DeserializeValue(PrefabRoot prefabRoot, string id, IMemberPath path, object component, TomlTable componentTable)
	{
		List<string> usedValues = [];
		
		MemberInfo[] members = component.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
			.Where(member => member.GetCustomAttribute<SerializedValueAttribute>() != null)
			.Where(member => componentTable.ContainsKey(member.Name))
			.ToArray();
		
		foreach (MemberInfo member in members)
		{
			IMemberWrapper wrapper = IMemberWrapper.Wrap(member);
			MemberPath memberPath = new(wrapper, path);
			Serializer serializer = wrapper.GetCustomAttribute<SerializedValueAttribute>()!.GetSerializer(wrapper.FieldType, data => DeserializeValue(prefabRoot, wrapper.Name, memberPath, wrapper.GetValue(component)!, data), wrapper.Name, id);
			object serializedValue = componentTable[member.Name];
			object deserializedValue = serializer.Deserialize(wrapper.GetValue(component), serializedValue);
			wrapper.SetValue(component, deserializedValue);
			prefabRoot.RegisterChange(memberPath);
			usedValues.Add(member.Name);
		}
		
		foreach (string usedValue in usedValues)
		{
			if (componentTable[usedValue] is TomlTable usedTable && usedTable.Count != 0)
				Console.WriteLine($"WARN: Unused table {usedValue} {usedTable.ToDelimString()} of {id} leftover");
			componentTable.Remove(usedValue);
		}
		
		return component;
	}
}
using System.Drawing;
using System.Reflection;
using Tomlyn;
using Tomlyn.Model;

namespace Echidna2.Serialization;

public static class TomlSerializer
{
	public static TomlTable Serialize(PrefabRoot prefabRoot, string path)
	{
		Dictionary<object, string> references = new();
		Dictionary<object, TomlTable> tables = new();
		
		foreach (object subcomponent in prefabRoot.Components.Concat(prefabRoot.ChildPrefabs.Select(prefab => prefab.PrefabRoot.RootObject)))
		{
			string id = references.Count.ToString();
			references.Add(subcomponent, id);
			
			TomlTable subcomponentTable = new();
			SerializeComponentType(prefabRoot, new ComponentPath(subcomponent), subcomponent, subcomponentTable, path);
			tables.Add(subcomponent, subcomponentTable);
		}
		
		foreach ((object subcomponent, TomlTable subcomponentTable) in tables)
			SerializeReference(prefabRoot, new ComponentPath(subcomponent), subcomponentTable, valueId => references[valueId]);
		
		foreach ((object subcomponent, TomlTable subcomponentTable) in tables)
			SerializeValue(prefabRoot, new ComponentPath(subcomponent), subcomponentTable);
		
		TomlTable prefabTable = new();
		foreach ((string id, TomlTable table) in references.Select(pair => (pair.Value, tables[pair.Key])))
			prefabTable.Add(id, table);
		
		File.WriteAllText(path, Toml.FromModel(prefabTable));
		return prefabTable;
	}
	
	private static string GetPathRelativeTo(string path, string relativeTo)
	{
		Uri pathUri = new(path);
		Uri relativeToUri = new(relativeTo);
		Uri relativeUri = relativeToUri.MakeRelativeUri(pathUri);
		return Uri.UnescapeDataString(relativeUri.ToString());
	}
	
	private static void SerializeComponentType(PrefabRoot prefabRoot, IMemberPath path, object component, TomlTable table, string relativeTo)
	{
		if (prefabRoot.GetPrefabInstance(path) is { } componentPrefabInstance)
			table.Add("Prefab", GetPathRelativeTo(componentPrefabInstance.PrefabRoot.PrefabPath, relativeTo));
		else
			table.Add("Component", component.GetType().AssemblyQualifiedName!.Split(',')[..2].Join(","));
	}
	
	private static void SerializeReference(PrefabRoot prefabRoot, IMemberPath path, TomlTable table, Func<object, string> getReferenceTo)
	{
		if (prefabRoot.GetPrefabInstance(path) is { } componentPrefabInstance)
		{
			foreach ((MemberPath memberPath, object value) in componentPrefabInstance.SerializedChanges)
			{
				IMemberWrapper wrapper = memberPath.Wrapper;
				SerializedReferenceAttribute? attribute = wrapper.Member.GetCustomAttribute<SerializedReferenceAttribute>();
				if (attribute is null) continue;
				ReferenceSerializer serializer = attribute.GetSerializer(wrapper.FieldType);
				table.Add(wrapper.Name, serializer.Serialize(value, getReferenceTo));
			}
		}
		else
		{
			foreach ((MemberPath memberPath, object value) in prefabRoot.SerializedData
				         .Where(pair => pair.Key.Root.Equals(path.Root)))
			{
				IMemberWrapper wrapper = memberPath.Wrapper;
				SerializedReferenceAttribute? attribute = wrapper.Member.GetCustomAttribute<SerializedReferenceAttribute>();
				if (attribute is null) continue;
				ReferenceSerializer serializer = attribute.GetSerializer(wrapper.FieldType);
				table.Add(wrapper.Name, serializer.Serialize(value, getReferenceTo));
			}
		}
	}
	
	private static void SerializeValue(PrefabRoot prefabRoot, IMemberPath path, TomlTable table)
	{
		if (prefabRoot.GetPrefabInstance(path) is { } componentPrefabInstance)
		{
			foreach ((MemberPath memberPath, object value) in componentPrefabInstance.SerializedChanges)
			{
				IMemberWrapper wrapper = memberPath.Wrapper;
				SerializedValueAttribute? attribute = wrapper.Member.GetCustomAttribute<SerializedValueAttribute>();
				if (attribute is null) continue;
				Serializer serializer = attribute.GetSerializer(wrapper.FieldType, null, null, null);
				table.Add(wrapper.Name, serializer.Serialize(value));
			}
		}
		else
		{
			foreach ((MemberPath memberPath, object value) in prefabRoot.SerializedData
				         .Where(pair => pair.Key.Root.Equals(path.Root)))
			{
				IMemberWrapper wrapper = memberPath.Wrapper;
				SerializedValueAttribute? attribute = wrapper.Member.GetCustomAttribute<SerializedValueAttribute>();
				if (attribute is null) continue;
				Serializer serializer = attribute.GetSerializer(wrapper.FieldType, null, null, null);
				table.Add(wrapper.Name, serializer.Serialize(value));
			}
		}
	}
}

public static class TomlDeserializer
{
	public static Assembly ProjectAssembly = null!;
	
	public static PrefabRoot Deserialize(string path, string? overridenTypeName = null)
	{
		Dictionary<string, object> references = new();
		List<(string id, object component, TomlTable componentTable)> components = [];
		
		PrefabRoot prefabRoot = new();
		prefabRoot.PrefabPath = path;
		
		bool doneFirstObject = false;
		TomlTable table = Toml.ToModel(File.ReadAllText(path));
		
		foreach ((string id, object value) in table)
		{
			TomlTable componentTable = (TomlTable)value;
			
			string? scriptName = doneFirstObject ? null : overridenTypeName;
			if (componentTable.Remove("ScriptContent", out object? _) && overridenTypeName == null)
				scriptName = $"{Path.GetFileNameWithoutExtension(path)}_{id}";
			
			object component;
			if (componentTable.Remove("Component", out object? typeName))
			{
				component = DeserializeComponent(id, scriptName ?? (string)typeName, scriptName != null);
				prefabRoot.Components.Add(component);
			}
			else if (componentTable.Remove("Prefab", out object? prefabPath))
			{
				PrefabRoot componentPrefabRoot = Deserialize($"{Path.GetDirectoryName(path)}/{(string)prefabPath}", scriptName);
				PrefabInstance componentPrefabInstance = new(componentPrefabRoot);
				prefabRoot.ChildPrefabs.Add(componentPrefabInstance);
				component = componentPrefabRoot.RootObject;
			}
			else
				throw new InvalidOperationException("Component table does not contain a Component or Prefab key");
			
			RemoveEvents(component, componentTable); // Should've been handled by Compilation
			
			references.Add(id, component);
			components.Add((id, component, componentTable));
			
			if (!doneFirstObject)
				prefabRoot.RootObject = component;
			doneFirstObject = true;
		}
		
		foreach ((string id, object component, TomlTable componentTable) in components)
			DeserializeReference(prefabRoot, id, new ComponentPath(component), component, componentTable, valueId => references[valueId]);
		
		foreach ((string id, object component, TomlTable componentTable) in components)
			DeserializeValue(prefabRoot, id, new ComponentPath(component), component, componentTable);
		
		foreach ((string id, object _, TomlTable componentTable) in components)
			if (componentTable.Count != 0)
				Console.WriteLine($"WARN: Unused table {id} {componentTable.ToDelimString()} of prefab leftover");
		
		if (prefabRoot.RootObject == null)
			throw new InvalidOperationException("No objects were deserialized");
		
		return prefabRoot;
	}
	
	private static object DeserializeComponent(string id, string typeName, bool useProjectAssembly)
	{
		Type? type = useProjectAssembly ? ProjectAssembly.GetType(typeName) : Type.GetType(typeName);
		if (type == null)
			throw new InvalidOperationException($"Type {typeName} of id {id} does not exist");
		
		ConstructorInfo? constructor = type.GetConstructor([]);
		if (constructor == null)
			throw new InvalidOperationException($"Type {type} of id {id} does not have a parameterless constructor");
		
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
	
	public static void RemoveEvents(object component, TomlTable componentTable)
	{
		List<string> usedValues = [];
		
		EventInfo[] members = component.GetType().GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
			.Where(member => member.GetCustomAttribute<SerializedEventAttribute>() != null)
			.Where(member => componentTable.ContainsKey(member.Name))
			.ToArray();
		
		usedValues.AddRange(members.Select(member => member.Name));
		
		foreach (string usedValue in usedValues)
			componentTable.Remove(usedValue);
	}
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SerializedValueAttribute(Type? serializerType = null) : Attribute
{
	private static Dictionary<Type, Serializer> defaultSerializers = new()
	{
		{ typeof(double), new DirectSerializer<double>() },
		{ typeof(string), new DirectSerializer<string>() },
		{ typeof(bool), new DirectSerializer<bool>() },
		{ typeof(Color), new ColorSerializer() },
	};
	
	public static void AddDefaultSerializer(Type type, Serializer serializer) => defaultSerializers[type] = serializer;
	
	private Serializer? serializer = (Serializer)serializerType?.GetConstructor([])!.Invoke([])!;
	
	public Serializer GetSerializer(Type type, Func<TomlTable, object> subcomponentDeserializer, string fieldName, string componentId)
	{
		if (serializer is not null)
			return serializer;
		
		if (defaultSerializers.TryGetValue(type, out serializer))
			return serializer;
		
		if (type.IsEnum)
			return serializer = new EnumSerializer(type);
		
		if (type
		    .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
		    .Any(member => member.GetCustomAttribute<SerializedValueAttribute>() is not null))
			return new SubComponentSerializer(subcomponentDeserializer, fieldName, componentId);
		
		throw new InvalidOperationException($"No serializer found for type {type}");
	}
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SerializedReferenceAttribute(Type? serializerType = null) : Attribute
{
	private static readonly ReferenceSerializer DefaultReferenceSerializer = new DefaultReferenceSerializer();
	
	private ReferenceSerializer? serializer = (ReferenceSerializer)serializerType?.GetConstructor([])!.Invoke([])!;
	
	public ReferenceSerializer GetSerializer(Type type)
	{
		if (serializer is not null)
			return serializer;

		return serializer = DefaultReferenceSerializer;
	}
}

[AttributeUsage(AttributeTargets.Event)]
public class SerializedEventAttribute : Attribute;
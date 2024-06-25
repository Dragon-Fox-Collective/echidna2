using System.Drawing;
using System.Reflection;
using Tomlyn;
using Tomlyn.Model;
using TooManyExtensions;
using static Echidna2.Serialization.SerializationPredicates;

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
			SerializeReference(prefabRoot, new ComponentPath(subcomponent), subcomponentTable, GetReferenceTo);
		
		foreach ((object subcomponent, TomlTable subcomponentTable) in tables)
			SerializeValue(prefabRoot, new ComponentPath(subcomponent), subcomponentTable);
		
		TomlTable prefabTable = new();
		
		TomlArray favoriteFields = [];
		foreach ((object component, MemberInfo field) in prefabRoot.FavoriteFields)
		{
			string componentId = GetReferenceTo(component);
			favoriteFields.Add($"{componentId}.{field.Name}");
		}
		prefabTable.Add("FavoriteFields", favoriteFields);
		
		foreach ((string id, TomlTable table) in references.Select(pair => (pair.Value, tables[pair.Key])))
			prefabTable.Add(id, table);
		
		File.WriteAllText(path, Toml.FromModel(prefabTable));
		return prefabTable;
		
		string GetReferenceTo(object component)
		{
			return references[component];
		}
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
		Dictionary<string, (object component, TomlTable componentTable)> components = [];
		
		PrefabRoot prefabRoot = new();
		prefabRoot.PrefabPath = path;
		
		bool doneFirstObject = false;
		TomlTable table = Toml.ToModel(File.ReadAllText(path));
		
		if (table.Remove("This", out object? root))
		{
			TomlTable rootTable = (TomlTable)root;
			prefabRoot.RootObject = DeserializeComponent("This", Path.GetFileNameWithoutExtension(path), true);
			components.Add("This", (prefabRoot.RootObject, rootTable));
			doneFirstObject = true;
		}
		
		foreach ((string id, object value) in table.Where(IdIsValidComponentId))
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
				PrefabRoot componentPrefabRoot = Deserialize(Path.Join(Path.GetDirectoryName(path), (string)prefabPath), scriptName);
				PrefabInstance componentPrefabInstance = new(componentPrefabRoot);
				prefabRoot.ChildPrefabs.Add(componentPrefabInstance);
				component = componentPrefabRoot.RootObject;
			}
			else
				throw new InvalidOperationException("Component table does not contain a Component or Prefab key");
			
			RemoveEvents(component, componentTable); // Should've been handled by Compilation
			
			components.Add(id, (component, componentTable));
			
			if (!doneFirstObject)
				prefabRoot.RootObject = component;
			doneFirstObject = true;
		}
		
		foreach ((string id, (object component, TomlTable componentTable)) in components)
			DeserializeReference(prefabRoot, id, new ComponentPath(component), component, componentTable, GetReferenceFrom);
		
		foreach ((string id, (object component, TomlTable componentTable)) in components)
			DeserializeValue(prefabRoot, id, new ComponentPath(component), component, componentTable);
		
		foreach ((string id, (object _, TomlTable componentTable)) in components)
			if (componentTable.Count != 0)
				Console.WriteLine($"WARN: Unused table {id} {componentTable.ToDelimString()} of prefab leftover");
		
		if (prefabRoot.RootObject == null)
			throw new InvalidOperationException("No objects were deserialized");
		
		prefabRoot.FavoriteFields = table.TryGetValue("FavoriteFields", out object? fields)
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
		{ typeof(double), new NumberSerializer<double>() },
		{ typeof(int), new NumberSerializer<int>() },
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
using System.Reflection;
using Echidna2.SourceGenerators;
using Tomlyn;
using Tomlyn.Model;

namespace Echidna2.Serialization;

[DontExpose]
public interface ITomlSerializable
{
	public void SerializeReferences(TomlTable table, Func<object, string> getReferenceTo);
	public bool DeserializeValue(string id, object value);
	public bool DeserializeReference(string id, object value, Dictionary<string, object> references);
}

public static class TomlSerializer
{
	public static void Serialize(PrefabRoot prefabRoot, string path)
	{
		object component = prefabRoot.RootObject;
		Dictionary<object, string> references = new();
		Dictionary<object, TomlTable> tables = new();
		GetReferenceTo(component);
		
		TomlTable prefabTable = new();
		foreach ((string id, TomlTable table) in references.Select(pair => (pair.Value, tables[pair.Key])))
			prefabTable.Add(id, table);
		
		Console.WriteLine(Toml.FromModel(prefabTable));
		// SerializeValues(component, table);
		// SerializeEvents(component, table);
		// File.WriteAllText(path, Toml.FromModel(table));
		return;
		
		string GetReferenceTo(object subcomponent)
		{
			if (!references.TryGetValue(subcomponent, out string? id))
			{
                id = references.Count.ToString();
                references.Add(subcomponent, id);
				tables.Add(subcomponent, SerializeReferences(prefabRoot, subcomponent, GetReferenceTo, path));
			}
			return id;
		}
	}
	
	private static string GetPathRelativeTo(string path, string relativeTo)
	{
		Uri pathUri = new(path);
		Uri relativeToUri = new(relativeTo);
		Uri relativeUri = relativeToUri.MakeRelativeUri(pathUri);
		return Uri.UnescapeDataString(relativeUri.ToString());
	}
	
	private static TomlTable SerializeReferences(PrefabRoot prefabRoot, object component, Func<object, string> getReferenceTo, string relativeTo)
	{
		TomlTable table = new();
		
		if (prefabRoot.GetPrefabRoot(component) is { } componentPrefabRoot)
		{
			table.Add("Prefab", GetPathRelativeTo(componentPrefabRoot.PrefabPath, relativeTo));
			foreach ((MemberInfo member, object value) in componentPrefabRoot.Changes)
				table.Add(member.Name, value);
		}
		else
		{
			table.Add("Component", component.GetType().AssemblyQualifiedName!.Split(',')[..2].Join(","));
			
			foreach (MemberInfo member in component.GetType()
				         .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				         .Where(member => member.GetCustomAttribute<SerializedReferenceAttribute>() is not null))
			{
				object? subcomponent = member switch
				{
					FieldInfo field => field.GetValue(component),
					PropertyInfo property => property.GetValue(component),
					_ => throw new InvalidOperationException($"Member {member} is not a field or property")
				};
				if (subcomponent is null) continue;
				string subcomponentReference = getReferenceTo(subcomponent);
				table.Add(member.Name, subcomponentReference);
			}
			
			if (component is ITomlSerializable serializable)
				serializable.SerializeReferences(table, getReferenceTo);
			
			return table;
		}
		
		return table;
	}
}

public static class TomlDeserializer
{
	public static Assembly ProjectAssembly = null!;
	
	// TODO: Make this not a horrible maze
	// TODO: Make the serialization for external classes automatic instead of calling static methods manually
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
				prefabRoot.ChildPrefabs.Add(componentPrefabRoot);
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
			DeserializeReference(id, component, componentTable, references);
		
		foreach ((string id, object component, TomlTable componentTable) in components)
			DeserializeValue(id, component, componentTable);
		
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
	
	private static T DeserializeValue<T>(string id, T component, TomlTable componentTable) where T : notnull
	{
		List<string> usedValues = [];
		
		MemberInfo[] members = component.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
			.Where(member => member.GetCustomAttribute<SerializedValueAttribute>() != null)
			.Where(member => componentTable.ContainsKey(member.Name))
			.ToArray();
		
		foreach (MemberInfo member in members)
		{
			if (member is FieldInfo field)
			{
				object? newValue;
				if (componentTable[member.Name] is TomlTable valueTable)
				{
					newValue = DeserializeValue(member.Name, field.GetValue(component)!, valueTable);
					if (valueTable.Count != 0)
						Console.WriteLine($"WARN: Unused table {member.Name} {valueTable.ToDelimString()} of {id} leftover");
				}
				else
				{
					newValue = componentTable[member.Name];
					if (field.FieldType.IsEnum)
						newValue = Enum.Parse(field.FieldType, (string)newValue);
				}
				field.SetValue(component, newValue);
				usedValues.Add(member.Name);
			}
			
			else if (member is PropertyInfo property)
			{
				object? newValue;
				if (componentTable[member.Name] is TomlTable valueTable)
				{
					newValue = DeserializeValue(member.Name, property.GetValue(component)!, valueTable);
					if (valueTable.Count != 0)
						Console.WriteLine($"WARN: Unused table {member.Name} {valueTable.ToDelimString()} of {id} leftover");
				}
				else
				{
					newValue = componentTable[member.Name];
					if (property.PropertyType.IsEnum)
						newValue = Enum.Parse(property.PropertyType, (string)newValue);
				}
				property.SetValue(component, newValue);
				usedValues.Add(member.Name);
			}
			
			else
				throw new InvalidOperationException($"Member {member} is not a field or property");
		}
		
		if (component is ITomlSerializable serializable)
			foreach ((string key, object value) in componentTable)
				if (serializable.DeserializeValue(key, value))
					usedValues.Add(key);
		
		foreach (string usedValue in usedValues)
		{
			if (componentTable[usedValue] is TomlTable usedTable && usedTable.Count != 0)
				Console.WriteLine($"WARN: Unused table {usedValue} {usedTable.ToDelimString()} of {id} leftover");
			componentTable.Remove(usedValue);
		}
		
		return component;
	}
	
	private static void DeserializeReference<T>(string id, T component, TomlTable componentTable, Dictionary<string, object> references) where T : notnull
	{
		List<string> usedValues = [];
		
		MemberInfo[] members = component.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
			.Where(member => member.GetCustomAttribute<SerializedReferenceAttribute>() != null)
			.Where(member => componentTable.ContainsKey(member.Name))
			.ToArray();
		
		foreach (MemberInfo member in members)
		{
			if (member is FieldInfo field)
			{
				if (componentTable[member.Name] is TomlTable valueTable)
				{
					object newValue = DeserializeValue(member.Name, field.GetValue(component)!, valueTable);
					DeserializeReference(member.Name, newValue, valueTable, references);
					field.SetValue(component, newValue);
					if (valueTable.Count != 0)
						Console.WriteLine($"WARN: Unused table {member.Name} {valueTable.ToDelimString()} of {id} leftover");
					usedValues.Add(member.Name);
				}
				else
				{
					string newValueId = (string)componentTable[member.Name];
					object newValue = references[newValueId];
					field.SetValue(component, newValue);
					usedValues.Add(member.Name);
				}
			}
			
			else if (member is PropertyInfo property)
			{
				if (componentTable[member.Name] is TomlTable valueTable)
				{
					object newValue = DeserializeValue(member.Name, property.GetValue(component)!, valueTable);
					DeserializeReference(member.Name, newValue, valueTable, references);
					property.SetValue(component, newValue);
					if (valueTable.Count != 0)
						Console.WriteLine($"WARN: Unused table {member.Name} {valueTable.ToDelimString()} of {id} leftover");
					usedValues.Add(member.Name);
				}
				else
				{
					string newValueId = (string)componentTable[member.Name];
					object newValue = references[newValueId];
					property.SetValue(component, newValue);
					usedValues.Add(member.Name);
				}
			}
			
			else
				throw new InvalidOperationException($"Member {member} is not a field or property");
		}
		
		if (component is ITomlSerializable serializable)
			foreach ((string key, object value) in componentTable)
				if (serializable.DeserializeReference(key, value, references))
					usedValues.Add(key);
		
		foreach (string usedValue in usedValues)
		{
			if (componentTable[usedValue] is TomlTable usedTable && usedTable.Count != 0)
				Console.WriteLine($"WARN: Unused table {usedValue} {usedTable.ToDelimString()} of {id} leftover");
			componentTable.Remove(usedValue);
		}
	}
	
	public static void RemoveEvents<T>(T component, TomlTable componentTable) where T : notnull
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
public class SerializedValueAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SerializedValueAttribute<TSerializer, TSerialized, TDeserialized> : Attribute where TSerializer : Serializer<TSerialized, TDeserialized>;

public interface Serializer<TSerialized, TDeserialized>
{
	public static abstract TSerialized Serialize(TDeserialized value);
	public static abstract TDeserialized Deserialize(TSerialized value);
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SerializedReferenceAttribute : Attribute;

[AttributeUsage(AttributeTargets.Event)]
public class SerializedEventAttribute : Attribute;
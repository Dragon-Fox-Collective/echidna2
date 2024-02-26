using System.Reflection;
using Tomlyn;
using Tomlyn.Model;

namespace Echidna2.Serialization;

public interface ITomlSerializable
{
	public void Serialize(TomlTable table);
	public void DeserializeValues(TomlTable table);
	public void DeserializeReferences(TomlTable table, Dictionary<string, object> references);
}

public static class TomlSerializer
{
	public static T Deserialize<T>(string path) where T : class
	{
		Dictionary<string, object> references = new();
		
		bool doneFirstObject = false;
		T? returnValue = null;
		TomlTable table = Toml.ToModel(File.ReadAllText(path));
		
		foreach ((string id, object value) in table)
		{
			TomlTable componentTable = (TomlTable)value;

			object component;
			if (componentTable.TryGetValue("Component", out object? typeName))
				component = DeserializeComponent(id, componentTable, (string)typeName);
			else if (componentTable.TryGetValue("Prefab", out object? prefabPath))
				component = DeserializePrefab(componentTable, $"{Path.GetDirectoryName(path)}/{prefabPath}");
			else
				throw new InvalidOperationException("Component table does not contain a Component or Prefab key");
			
			references.Add(id, component);
			
			if (!doneFirstObject)
				returnValue = (T)component;
			doneFirstObject = true;
		}
		
		foreach ((string key, object component) in references)
			DeserializeReference(component, (TomlTable)table[key], references);
		
		if (returnValue == null)
			throw new InvalidOperationException("No objects were deserialized");
		
		return returnValue;
	}
	
	private static object DeserializeComponent(string id, TomlTable componentTable, string typeName)
	{
		Type? type = Type.GetType(typeName);
		if (type == null)
			throw new InvalidOperationException($"Type {componentTable["type"]} of id {id} does not exist");
		
		ConstructorInfo? constructor = type.GetConstructor([]);
		if (constructor == null)
			throw new InvalidOperationException($"Type {type} of id {id} does not have a parameterless constructor");
		
		object component = constructor.Invoke([]);
		
		component = DeserializeValue(component, componentTable);
		
		return component;
	}
	
	private static object DeserializePrefab(TomlTable componentTable, string prefabPath)
	{
		object prefab = Deserialize<object>(prefabPath);
		return DeserializeValue(prefab, componentTable);
	}
	
	private static T DeserializeValue<T>(T component, TomlTable componentTable) where T : notnull
	{
		MemberInfo[] members = component.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
			.Where(member => member.GetCustomAttribute<SerializedValueAttribute>() != null)
			.Where(member => componentTable.ContainsKey(member.Name))
			.ToArray();
		
		// string unusedValues = componentTable.Keys
		// 	.Where(key => key != "Type")
		// 	.Where(key => members.All(member => member.Name != key))
		// 	.ToDelimString();
		// if (unusedValues != "[]")
		// 	Console.WriteLine("INFO: Unused values: " + unusedValues);
		
		foreach (MemberInfo member in members)
		{
			if (member is FieldInfo field)
			{
				object? newValue;
				if (componentTable[member.Name] is TomlTable valueTable)
					newValue = DeserializeValue(field.GetValue(component)!, valueTable);
				else
					newValue = componentTable[member.Name];
				field.SetValue(component, newValue);
			}
			
			else if (member is PropertyInfo property)
			{
				object? newValue;
				if (componentTable[member.Name] is TomlTable valueTable)
					newValue = DeserializeValue(property.GetValue(component)!, valueTable);
				else
					newValue = componentTable[member.Name];
				property.SetValue(component, newValue);
			}
			
			else
				throw new InvalidOperationException($"Member {member} is not a field or property");
		}
		
		if (component is ITomlSerializable serializable)
			serializable.DeserializeValues(componentTable);
		
		return component;
	}
	
	private static void DeserializeReference<T>(T component, TomlTable componentTable, Dictionary<string, object> references) where T : notnull
	{
		component.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
			.Where(member => member.GetCustomAttribute<SerializedReferenceAttribute>() != null)
			.Where(member => componentTable.ContainsKey(member.Name))
			.ForEach(member =>
			{
				if (member is FieldInfo field)
				{
					if (componentTable[member.Name] is TomlTable valueTable)
					{
						object newValue = DeserializeValue(field.GetValue(component)!, valueTable);
						DeserializeReference(newValue, valueTable, references);
						field.SetValue(component, newValue);
					}
					else
					{
						string newValueId = (string)componentTable[member.Name];
						object newValue = references[newValueId];
						field.SetValue(component, newValue);
					}
				}
				
				else if (member is PropertyInfo property)
				{
					if (componentTable[member.Name] is TomlTable valueTable)
					{
						object newValue = DeserializeValue(property.GetValue(component)!, valueTable);
						DeserializeReference(newValue, valueTable, references);
						property.SetValue(component, newValue);
					}
					else
					{
						string newValueId = (string)componentTable[member.Name];
						object newValue = references[newValueId];
						property.SetValue(component, newValue);
					}
				}
				
				else
					throw new InvalidOperationException($"Member {member} is not a field or property");
			});
		
		if (component is ITomlSerializable serializable)
			serializable.DeserializeReferences(componentTable, references);
	}
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SerializedValueAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SerializedReferenceAttribute : Attribute;
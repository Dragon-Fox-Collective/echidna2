﻿using System.Reflection;
using Tomlyn;
using Tomlyn.Model;

namespace Echidna2.Serialization;

public interface ITomlSerializable
{
	public void Serialize(TomlTable table);
	public bool DeserializeValue(string id, object value);
	public bool DeserializeReference(string id, object value, Dictionary<string, object> references);
}

public static class TomlSerializer
{
	// TODO: Make this not a horrible maze
	// TODO: Make the serialization for external classes automatic instead of calling static methods manually
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
			if (componentTable.Remove("Component", out object? typeName))
				component = DeserializeComponent(id, componentTable, (string)typeName);
			else if (componentTable.Remove("Prefab", out object? prefabPath))
				component = DeserializePrefab(id, componentTable, $"{Path.GetDirectoryName(path)}/{(string)prefabPath}");
			else
				throw new InvalidOperationException("Component table does not contain a Component or Prefab key");
			
			references.Add(id, component);
			
			if (!doneFirstObject)
				returnValue = (T)component;
			doneFirstObject = true;
		}
		
		foreach ((string key, object component) in references)
			DeserializeReference(key, component, (TomlTable)table[key], references);
		
		if (returnValue == null)
			throw new InvalidOperationException("No objects were deserialized");
		
		return returnValue;
	}
	
	private static object DeserializeComponent(string id, TomlTable componentTable, string typeName)
	{
		Type? type = Type.GetType(typeName);
		if (type == null)
			throw new InvalidOperationException($"Type {typeName} of id {id} does not exist");
		
		ConstructorInfo? constructor = type.GetConstructor([]);
		if (constructor == null)
			throw new InvalidOperationException($"Type {type} of id {id} does not have a parameterless constructor");
		
		object component = constructor.Invoke([]);
		
		component = DeserializeValue(id, component, componentTable);
		
		return component;
	}
	
	private static object DeserializePrefab(string id, TomlTable componentTable, string prefabPath)
	{
		object prefab = Deserialize<object>(prefabPath);
		return DeserializeValue(id, prefab, componentTable);
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
						Console.WriteLine($"WARN: Unused table {member.Name} {valueTable.ToDelimString()} leftover");
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
						Console.WriteLine($"WARN: Unused table {member.Name} {valueTable.ToDelimString()} leftover");
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
				Console.WriteLine($"WARN: Unused table {usedValue} {usedTable.ToDelimString()} leftover");
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
						Console.WriteLine($"WARN: Unused table {member.Name} {valueTable.ToDelimString()} leftover");
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
				Console.WriteLine($"WARN: Unused table {usedValue} {usedTable.ToDelimString()} leftover");
			componentTable.Remove(usedValue);
		}
		
		if (componentTable.Count != 0)
			Console.WriteLine($"WARN: Unused table {id} {componentTable.ToDelimString()} leftover");
	}
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SerializedValueAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SerializedReferenceAttribute : Attribute;
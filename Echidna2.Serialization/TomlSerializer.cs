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
			string id = subcomponent == prefabRoot.RootObject ? "This" : references.Count.ToString();
			references.Add(subcomponent, id);
			
			TomlTable subcomponentTable = new();
			SerializeComponentType(prefabRoot, new ComponentPath(subcomponent), subcomponent, subcomponentTable, path);
			subcomponentTable.Add("Values", new TomlTable());
			tables.Add(subcomponent, subcomponentTable);
		}
		
		foreach ((object subcomponent, TomlTable subcomponentTable) in tables)
			SerializeReference(prefabRoot, new ComponentPath(subcomponent), (TomlTable)subcomponentTable["Values"], GetReferenceTo);
		
		foreach ((object subcomponent, TomlTable subcomponentTable) in tables)
			SerializeValue(prefabRoot, new ComponentPath(subcomponent), (TomlTable)subcomponentTable["Values"]);
		
		TomlTable prefabTable = new();
		
		foreach ((string id, TomlTable table) in references.Select(pair => (pair.Value, tables[pair.Key])))
			prefabTable.Add(id, table);
		
		TomlArray favoriteFields = [];
		foreach ((object component, MemberInfo field) in prefabRoot.FavoriteFields)
		{
			string componentId = GetReferenceTo(component);
			favoriteFields.Add($"{componentId}.{field.Name}");
		}
		tables[prefabRoot.RootObject].Add("FavoriteFields", favoriteFields);
		
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
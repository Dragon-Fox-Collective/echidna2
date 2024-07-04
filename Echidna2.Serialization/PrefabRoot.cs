using System.Reflection;
using Echidna2.Serialization.TomlFiles;

namespace Echidna2.Serialization;

public class PrefabRoot : IPrefabChangeRegistry
{
	public List<PrefabInstance> ChildPrefabs = [];
	public List<object> Components = [];
	
	public Dictionary<MemberPath, object> SerializedData = new();
	
	public List<(object Component, MemberInfo Field)> FavoriteFields = [];
	
	public object RootObject = null!;
	public Prefab Prefab = null!;
	
	public PrefabInstance? GetPrefabInstance(IMemberPath path)
	{
		return ChildPrefabs.FirstOrDefault(child => child.PrefabRoot.RootObject == path.Root.Component);
	}
	
	public IPrefabChangeRegistry? GetChangeRegistry(IMemberPath path)
	{
		return Components.Contains(path.Root.Component) ? this : GetPrefabInstance(path);
	}
	
	public void RegisterChange(MemberPath path)
	{
		GetChangeRegistry(path)?.RegisterChangeInSelf(path.HighestMemberPath);
	}
	
	public void RegisterChangeInSelf(MemberPath path) => SerializedData[path] = path.Value;
	
	public bool Owns(object component) => Components.Contains(component) || ChildPrefabs.Any(child => child.PrefabRoot.RootObject == component);
}

public class PrefabInstance(PrefabRoot prefabRoot) : IPrefabChangeRegistry
{
	public PrefabRoot PrefabRoot => prefabRoot;
	public Dictionary<MemberPath, object> SerializedChanges = new();
	
	public void RegisterChangeInSelf(MemberPath path)
	{
		if (path.Root.Component != prefabRoot.RootObject)
			throw new ArgumentException("Component must be the root object of the prefab");
		SerializedChanges[path] = path.Value;
	}
}

public interface IPrefabChangeRegistry
{
	public void RegisterChangeInSelf(MemberPath path);
}
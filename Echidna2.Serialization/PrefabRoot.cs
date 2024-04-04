using System.Reflection;

namespace Echidna2.Serialization;

public class PrefabRoot
{
	public List<PrefabRoot> ChildPrefabs = [];
	public List<object> Components = [];
	public Dictionary<MemberInfo, object> Changes = new();
	public object RootObject = null!;
	public string PrefabPath = null!;
	
	public PrefabRoot? GetPrefabRoot(object component)
	{
		foreach (PrefabRoot childPrefab in ChildPrefabs)
		{
			if (childPrefab.RootObject == component)
				return childPrefab;
			if (childPrefab.GetPrefabRoot(component) is { } childPrefabRoot)
				return childPrefabRoot;
		}
		return null;
	}
	
	public PrefabRoot? GetOwningPrefabRoot(object component)
	{
		return Components.Contains(component) ? this : ChildPrefabs.FirstOrDefault(childPrefab => childPrefab.RootObject == component);
	}
	
	public void RegisterChange(object component, MemberInfo member, object value)
	{
		// FIXME: Changes to child prefab components are regestered to the child prefab, causing conflicts when the parent prefab changes it
		PrefabRoot? owningPrefabRoot = GetOwningPrefabRoot(component);
		if (owningPrefabRoot is null) return;
		owningPrefabRoot.Changes[member] = value;
	}
}
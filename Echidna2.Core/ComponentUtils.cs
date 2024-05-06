using System.Reflection;
using Echidna2.Serialization;

namespace Echidna2.Core;

public static class ComponentUtils
{
	public static IEnumerable<object> GetAllComponents(object component, bool searchChildren = false) => GetAllComponentsOfTypeIncludingDuplicates(component, null, searchChildren).Distinct();
	
	public static IEnumerable<object> GetAllComponentsOfType(object component, Type type, bool searchChildren = false) => GetAllComponentsOfTypeIncludingDuplicates(component, type, searchChildren).Distinct();
	
	private static IEnumerable<object> GetAllComponentsOfTypeIncludingDuplicates(object component, Type? type, bool searchChildren = false)
	{
		if (type is null || component.GetType().IsAssignableTo(type))
			yield return component;
		
		foreach (MemberInfo member in component.GetType()
			         .GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			         .Where(member => member.GetCustomAttribute<SerializedReferenceAttribute>() is not null))
		{
			object? child = IMemberWrapper.Wrap(member).GetValue(component);
			if (child is not null)
				foreach (object t in GetAllComponentsOfTypeIncludingDuplicates(child, type))
					yield return t;
		}
		
		if (searchChildren && component is IHasChildren hasChildren)
			foreach (object child in hasChildren.Children)
			foreach (object t in GetAllComponentsOfTypeIncludingDuplicates(child, type))
				yield return t;
	}
}
using System.Reflection;
using Echidna2.Serialization;

namespace Echidna2.Core;

public static class ComponentUtils
{
	public static IEnumerable<object> GetAllComponents(object component) => GetAllComponentsOfTypeIncludingDuplicates(component, null).Distinct();
	
	public static IEnumerable<object> GetAllComponentsOfType(object component, Type type) => GetAllComponentsOfTypeIncludingDuplicates(component, type).Distinct();
	
	private static IEnumerable<object> GetAllComponentsOfTypeIncludingDuplicates(object component, Type? type)
	{
		if (type is null || component.GetType().IsAssignableTo(type))
			yield return component;
		
		if (component is IHasChildren hasChildren)
			foreach (object child in hasChildren.Children)
			foreach (object t in GetAllComponentsOfTypeIncludingDuplicates(child, type))
				yield return t;
		
		foreach (MemberInfo member in component.GetType()
			         .GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			         .Where(member => member.GetCustomAttribute<SerializedReferenceAttribute>() is not null))
		{
			object? child = IMemberWrapper.Wrap(member).GetValue(component);
			if (child is not null)
				foreach (object t in GetAllComponentsOfTypeIncludingDuplicates(child, type))
					yield return t;
		}
	}
}
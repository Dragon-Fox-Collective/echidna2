using System.Reflection;
using Echidna2.Serialization;

namespace Echidna2.Core;

public static class ComponentUtils
{
	public static IEnumerable<object> GetAllComponents(object component, bool searchChildren = false) => GetAllComponentsOfTypeIncludingDuplicates(component, null, null, (c, _) => c, searchChildren).Distinct();
	
	public static IEnumerable<object> GetAllComponentsOfType(object component, Type type, bool searchChildren = false) => GetAllComponentsOfTypeIncludingDuplicates(component, type, null, (c, _) => c, searchChildren).Distinct();
	
	public static IEnumerable<(object Component, ReferencePath? Reference)> GetAllReferencesToComponentsOfType(object component, Type type, bool searchChildren = false) => GetAllComponentsOfTypeIncludingDuplicates(component, type, null, (c, r) => (c, r), searchChildren)
		.DistinctBy(zip => zip.c);
	
	private static IEnumerable<T> GetAllComponentsOfTypeIncludingDuplicates<T>(object component, Type? type, ReferencePath? reference, Func<object, ReferencePath?, T> zipper, bool searchChildren = false)
	{
		if (type is null || component.GetType().IsAssignableTo(type))
			yield return zipper(component, reference);
		
		foreach (MemberInfo member in component.GetType()
			         .GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			         .Where(member => member.GetCustomAttribute<SerializedReferenceAttribute>() is not null))
		{
			object? child = IMemberWrapper.Wrap(member).GetValue(component);
			if (child is not null)
				foreach (T t in GetAllComponentsOfTypeIncludingDuplicates(child, type, new ReferencePath(component, reference), zipper, searchChildren))
					yield return t;
		}
		
		if (searchChildren && component is IHasChildren hasChildren)
			foreach (object child in hasChildren.Children)
			foreach (T t in GetAllComponentsOfTypeIncludingDuplicates(child, type, new ReferencePath(component, reference), zipper, searchChildren))
				yield return t;
	}
	
	public class ReferencePath(object component, ReferencePath? reference)
	{
		public object Component => component;
		public ReferencePath? Reference => reference;
		
		public override string ToString() => $"{(Reference is not null ? Reference + "\n\t" : null)}{(Component is INamed named ? $"{named.Name} ({Component.GetType().Name})" : Component.GetType().Name)}";
	}
}
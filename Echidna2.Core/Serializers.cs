using Echidna2.Serialization;
using Tomlyn.Model;

namespace Echidna2.Core;

public class ChildrenSerializer : ReferenceSerializer<TomlArray, IEnumerable<object>>
{
	public TomlArray Serialize(IEnumerable<object> value, Func<object, string> getReferenceTo)
	{
		TomlArray children = [];
		foreach (string reference in value.Select(getReferenceTo))
			children.Add(reference);
		return children;
	}
	
	public IEnumerable<object> Deserialize(IEnumerable<object>? value, TomlArray data, Func<string, object> getReferenceFrom)
	{
		return data.Select(childId => getReferenceFrom((string)childId!));
	}
}
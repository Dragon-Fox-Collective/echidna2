using Echidna2.Serialization;

namespace Echidna2.Core;

public interface INamed
{
	public string Name { get; set; }
}

public class Named : INamed
{
	[SerializedValue] public string Name { get; set; } = "Named";
}
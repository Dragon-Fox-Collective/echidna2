using Echidna2.Serialization;

namespace Echidna2.Core;

public interface INamed
{
	public string Name { get; set; }
	
	public static string? GetName(object? obj) => obj switch
	{
		INamed named => named.Name,
		not null => obj.GetType().Name + " (no name)",
		_ => null
	};
}

public class Named : INamed
{
	[SerializedValue] public string Name { get; set; } = "Named";
}
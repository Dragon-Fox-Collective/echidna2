using System.Diagnostics.CodeAnalysis;
using Echidna2.Serialization;
using JetBrains.Annotations;

namespace Echidna2.Core;

public interface INamed
{
	public string Name { get; set; }
	public event Action<string>? NameChanged;
	
	[return: NotNullIfNotNull("obj")]
	public static string? GetName(object? obj) => obj switch
	{
		INamed named => named.Name,
		not null => obj.GetType().Name + " (no name)",
		_ => null
	};
}

[UsedImplicitly]
public class Named : INamed
{
	private string name = "Named";
	[SerializedValue] public string Name
	{
		get => name;
		set
		{
			name = value;
			NameChanged?.Invoke(value);
		}
	}
	[SerializedEvent] public event Action<string>? NameChanged;
}
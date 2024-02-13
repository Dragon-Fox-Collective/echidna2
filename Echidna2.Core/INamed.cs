namespace Echidna2.Core;

public interface INamed
{
	public string Name { get; set; }
}

public class Named(string name) : INamed
{
	public string Name { get; set; } = name;
}
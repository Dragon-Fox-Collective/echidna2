namespace Echidna2.Core;

[ComponentImplementation<Named>]
public interface INamed
{
	public string Name { get; set; }
}

public class Named(string name) : INamed
{
	public Named() : this("Named") { }
	
	public string Name { get; set; } = name;
}
namespace Echidna2.Core;

[AttributeUsage(AttributeTargets.Class)]
public class PrefabAttribute(string path) : Attribute
{
	public string Path { get; } = path;
}
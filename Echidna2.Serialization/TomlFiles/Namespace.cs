namespace Echidna2.Serialization.TomlFiles;

public class Namespace
{
	public string Value = "";
	
	public static Namespace FromToml(string prefabPath)
	{
		return new Namespace { Value = Compilation.GetPrefabClassNamespace(prefabPath) };
	}
	
	public string StringifyCS()
	{
		return $"namespace {Value};\n\n";
	}
	
	public override string ToString() => Value;
}
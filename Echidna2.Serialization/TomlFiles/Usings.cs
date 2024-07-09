using Tomlyn.Model;

namespace Echidna2.Serialization.TomlFiles;

public class Usings
{
	public static readonly string[] GlobalImports = ["Echidna2.Core", "Echidna2.Mathematics", "Echidna2.Serialization"];
	
	public List<string> Value = GlobalImports.ToList();
	
	public static Usings FromToml(IEnumerable<string> array)
	{
		Usings usings = new();
		usings.Value.AddRange(array);
		return usings;
	}
	
	public TomlArray ToToml()
	{
		TomlArray array = [];
		array.AddRange(Value);
		return array;
	}
	
    public string StringifyCS()
	{
		return Value.Select(u => $"using {u};\n").Join() + "\n";
	}
}
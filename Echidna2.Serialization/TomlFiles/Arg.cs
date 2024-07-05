using Tomlyn.Model;

namespace Echidna2.Serialization.TomlFiles;

public class Arg
{
	public string Name = "";
	public string Type = "";
	
	public bool HasCast => !CastTo.IsEmpty();
	public string CastTo = "";
	
	public static Arg FromToml(TomlTable table)
	{
		Arg arg = new();
		arg.Name = table.GetCasted<string>("Name");
		arg.Type = table.GetCasted<string>("Type");
		arg.CastTo = table.GetString("CastTo");
		return arg;
	}
	
	public string StringifyCS() => $"{Type} {Name}";
	public string StringifyCSCast() => HasCast ? $"({CastTo}){Name}!" : Name;
	public string StringifyCSCasted() => HasCast ? $"{CastTo} {Name}" : StringifyCS();
}
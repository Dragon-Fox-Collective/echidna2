using Tomlyn.Model;

namespace Echidna2.Serialization.TomlFiles;

public class Generic
{
	public GenericType GenericType;
	public string Name = "";
	
	public static Generic FromToml(TomlTable table)
	{
		Generic generic = new();
		generic.GenericType = table.GetString("GenericType") switch
		{
			"In" => GenericType.In,
			"Out" => GenericType.Out,
			_ => GenericType.None,
		};
		generic.Name = table.GetCasted<string>("Name");
		return generic;
	}
	
	public string StringifyCS()
	{
		return GenericType switch
		{
			GenericType.None => "",
			GenericType.In => "in ",
			GenericType.Out => "out ",
			_ => throw new IndexOutOfRangeException(),
		} + Name;
	}
}

public enum GenericType
{
	None,
	In,
	Out,
}
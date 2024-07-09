using Tomlyn.Model;

namespace Echidna2.Serialization.TomlFiles;

public class Function
{
	public FunctionType FunctionType;
	public bool IsStatic;
	public string ReturnType = "";
	public string Name = "";
	public List<Arg> Args = [];
	public string Content = "";
	
	public static Function FromToml(TomlTable table)
	{
		Function function = new();
		function.FunctionType = Enum.Parse<FunctionType>(table.GetCasted<string>("FunctionType", "Public"));
		function.IsStatic = table.GetCasted("Static", false);
		function.ReturnType = table.GetCasted<string>("ReturnType");
		function.Name = table.GetCasted<string>("Name");
		function.Args = table.GetList<TomlTable>("Args").Select(Arg.FromToml).ToList();
		function.Content = table.GetString("Content");
		return function;
	}
	
	public TomlTable ToToml()
	{
		TomlTable table = new();
		table.Add("FunctionType", FunctionType.ToString());
		table.Add("Static", IsStatic);
		table.Add("ReturnType", ReturnType);
		table.Add("Name", Name);
		table.Add("Args", Args.Select(arg => arg.ToToml()).ToList());
		table.Add("Content", Content);
		return table;
	}
	
	public string StringifyCS()
	{
		string scriptString = "";
		scriptString += "\t";
		
		if (FunctionType == FunctionType.Public) scriptString += "public ";
		else if (FunctionType == FunctionType.Private) scriptString += "private ";
		
		if (IsStatic) scriptString += "static ";
		
		scriptString += $"{ReturnType} {Name}({Args.Select(arg => arg.StringifyCS()).Join(", ")})\n";
		scriptString += "\t{\n";
		scriptString += "\t\t" + Content.Indent().Indent() + "\n";
		scriptString += "\t}\n";
		scriptString += "\n";
		return scriptString;
	}
	
	public string StringifyCSAbstract()
	{
		string scriptString = "";
		scriptString += "\t";
		
		if (IsStatic) scriptString += "static ";
		
		scriptString += $"{ReturnType} {Name}({Args.Select(arg => arg.StringifyCS()).Join(", ")});\n";
		return scriptString;
	}
	
	public string StringifyCSAbstractOverride(string className)
	{
		string scriptString = "";
		
		scriptString += "\t";
		if (IsStatic) scriptString += "static ";
		scriptString += $"{ReturnType} {className}.{Name}({Args.Select(arg => arg.StringifyCS()).Join(", ")}) => {Name}({Args.Select(arg => arg.StringifyCSCast()).Join(", ")});\n";
		
		scriptString += "\t";
		if (IsStatic) scriptString += "static ";
		scriptString += $"{ReturnType} {Name}({Args.Select(arg => arg.StringifyCSCasted()).Join(", ")});\n";
		
		return scriptString;
	}
}

public enum FunctionType
{
	Public,
	Private,
}
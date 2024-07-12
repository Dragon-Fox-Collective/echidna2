using Tomlyn.Model;

namespace Echidna2.Serialization.TomlFiles;

public class EventListener
{
	public EventType EventType;
	public string Name = "";
	public string Target = "";
	public List<Arg> Args = [];
	public string Content = "";
	public bool UsesFunction => !Function.IsEmpty();
	public string Function = "";
	private string FunctionName => UsesFunction ? Function : $"{Target}_{Name}";
	private string TargetName => $"{Target}.{Name}";
	
	public static EventListener FromToml(TomlTable table)
	{
		EventListener eventListener = new();
		eventListener.EventType = Enum.Parse<EventType>(table.GetCasted<string>("EventType"));
		eventListener.Name = table.GetCasted<string>("Name");
		eventListener.Target = table.GetString("Target");
		eventListener.Args = table.GetList<TomlTable>("Args").Select(Arg.FromToml).ToList();
		eventListener.Content = table.GetString("Content");
		eventListener.Function = table.GetString("Function");
		return eventListener;
	}
	
	public TomlTable ToToml()
	{
		TomlTable table = new();
		table.Add("EventType", EventType.ToString());
		table.Add("Name", Name);
		table.Add("Target", Target);
		table.Add("Args", Args.Select(arg => arg.ToToml()).ToList());
		table.Add("Content", Content);
		table.Add("Function", Function);
		return table;
	}
	
	public string StringifyCSAdd() => $"\t\t{TargetName} += {FunctionName};\n";
	public string StringifyCSSub() => $"\t\t{TargetName} -= {FunctionName};\n";
	
	public string StringifyCS() => EventType switch
	{
		EventType.Self => StringifyCSReference(),
		EventType.Notification => StringifyCSNotification(),
		EventType.Reference => StringifyCSReference(),
		_ => throw new ArgumentOutOfRangeException(EventType.ToString()),
	};
	
	private string StringifyCSNotification()
	{
		string scriptString = "";
		scriptString += $"\tpublic void OnNotify({Name}Notification notification)\n";
		scriptString += "\t{\n";
		scriptString += "\t\t" + Content.Indent().Indent() + "\n";
		scriptString += "\t}\n";
		return scriptString;
	}
	private string StringifyCSReference()
	{
		string scriptString = "";
		if (Args.Any(arg => arg.HasCast))
		{
			scriptString += $"\tprivate void {FunctionName}({Args.Select(arg => arg.StringifyCS()).Join(", ")})";
			scriptString += $" => {FunctionName}({Args.Select(arg => arg.StringifyCSCast()).Join(", ")});\n";
			if (UsesFunction) return scriptString;
			scriptString += $"\tprivate void {FunctionName}({Args.Select(arg => arg.StringifyCSCasted()).Join(", ")})\n";
		}
		else
		{
			if (UsesFunction) return scriptString;
			scriptString += $"\tprivate void {FunctionName}({Args.Select(arg => arg.StringifyCS()).Join(", ")})\n";
		}
		scriptString += "\t{\n";
		scriptString += "\t\t" + Content.Indent().Indent() + "\n";
		scriptString += "\t}\n";
		return scriptString;
	}
}

public enum EventType
{
	Self,
	Notification,
	Reference,
}
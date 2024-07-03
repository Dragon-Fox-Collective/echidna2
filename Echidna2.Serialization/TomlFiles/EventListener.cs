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
	
	public string StringifyCSAdd() => $"\t\t{Target}.{Name} += {(UsesFunction ? Function : $"{Target}_{Name}")};\n";
	public string StringifyCSSub() => $"\t\t{Target}.{Name} -= {(UsesFunction ? Function : $"{Target}_{Name}")};\n";
	
	public string StringifyCS()
	{
		if (UsesFunction) return "";
		return EventType switch
		{
			EventType.Self => StringifyCSSelf(),
			EventType.Notification => StringifyCSNotification(),
			EventType.Reference => StringifyCSReference(),
			_ => throw new ArgumentOutOfRangeException(EventType.ToString()),
		} + "\n";
	}
	private string StringifyCSSelf()
	{
		string scriptString = "";
		scriptString += $"\tprotected override void Setup_{Target}()\n";
		scriptString += "\t{\n";
		scriptString += $"\t\tbase.Setup_{Target}();\n";
		scriptString += $"\t\t{Target}.{Name} += {Target}_{Name};\n";
		scriptString += "\t}\n";
		
		scriptString += $"\tprotected override void Unsetup_{Target}()\n";
		scriptString += "\t{\n";
		scriptString += $"\t\tbase.Unsetup_{Target}();\n";
		scriptString += $"\t\t{Target}.{Name} -= {Target}_{Name};\n";
		scriptString += "\t}\n";
		
		if (Args.Any(arg => arg.HasCast))
		{
			scriptString += $"\tprivate void {Target}_{Name}({Args.Select(arg => arg.StringifyCS()).Join(", ")})";
			scriptString += $" => {Target}_{Name}({Args.Select(arg => arg.StringifyCSCast()).Join(", ")});\n";
			scriptString += $"\tprivate void {Target}_{Name}({Args.Select(arg => arg.StringifyCSCasted()).Join(", ")})\n";
		}
		else
			scriptString += $"\tprivate void {Target}_{Name}({Args.Select(arg => arg.StringifyCS()).Join(", ")})\n";
		scriptString += "\t{\n";
		scriptString += "\t\t" + Content.Indent().Indent() + "\n";
		scriptString += "\t}\n";
		return scriptString;
	}
	private string StringifyCSNotification()
	{
		string scriptString = "";
		scriptString += $"\tpublic void OnNotify({Name}_Notification notification)\n";
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
			scriptString += $"\tprivate void {Target}_{Name}({Args.Select(arg => arg.StringifyCS()).Join(", ")})";
			scriptString += $" => {Target}_{Name}({Args.Select(arg => arg.StringifyCSCast()).Join(", ")});\n";
			scriptString += $"\tprivate void {Target}_{Name}({Args.Select(arg => arg.StringifyCSCasted()).Join(", ")})\n";
		}
		else
			scriptString += $"\tprivate void {Target}_{Name}({Args.Select(arg => arg.StringifyCS()).Join(", ")})\n";
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
using Tomlyn.Model;

namespace Echidna2.Serialization.TomlFiles;

public class Property
{
	public string Name = "";
	public string Type = "";
	public PropertyType PropertyType;
	public bool ExposeProperties = false;
	public List<EventListener> EventListeners = [];
	
	public bool HasCustomGetter => !GetterContent.IsEmpty();
	public string GetterContent = "";
	public bool HasCustomSetter => !SetterContent.IsEmpty();
	public string SetterContent = "";
	
	public bool HasCustomAdder => !AdderContent.IsEmpty();
	public string AdderContent = "";
	public bool HasCustomRemover => !RemoverContent.IsEmpty();
	public string RemoverContent = "";
	
	public static Property FromToml(TomlTable table, IEnumerable<EventListener> eventListeners)
	{
		Property property = new();
		property.Type = table.GetCasted<string>("Type");
		property.Name = table.GetCasted<string>("Name");
		property.PropertyType = Enum.Parse<PropertyType>(table.GetCasted<string>("PropertyType"));
		property.ExposeProperties = table.GetCasted("ExposeProperties", property.PropertyType == PropertyType.Component);
		property.EventListeners = eventListeners.Where(eventListener => eventListener.EventType == EventType.Reference && eventListener.Target == property.Name).ToList();
		property.GetterContent = table.GetString("GetterContent");
		property.SetterContent = table.GetString("SetterContent");
		property.AdderContent = table.GetString("AdderContent");
		property.RemoverContent = table.GetString("RemoverContent");
		return property;
	}
	
	public TomlTable ToToml()
	{
		TomlTable table = new();
		table.Add("Type", Type);
		table.Add("Name", Name);
		table.Add("PropertyType", PropertyType.ToString());
		table.Add("ExposeProperties", ExposeProperties);
		table.Add("GetterContent", GetterContent);
		table.Add("SetterContent", SetterContent);
		table.Add("AdderContent", AdderContent);
		table.Add("RemoverContent", RemoverContent);
		return table;
	}
	
	public string StringifyCS()
	{
		return PropertyType switch
		{
			PropertyType.Component => StringifyCSComponent(),
			PropertyType.Reference => StringifyCSReference(),
			PropertyType.Value => StringifyCSValue(),
			PropertyType.Public => StringifyCSPublic(),
			PropertyType.Private => StringifyCSPrivate(),
			PropertyType.Event => StringifyCSEvent(),
			_ => throw new ArgumentOutOfRangeException(PropertyType.ToString())
		} + "\n";
	}
	private string StringifyCSComponent()
	{
		string scriptString = "";
		scriptString += $"\tprivate {Type} _{Name} = default!;\n";
		scriptString += $"\t[SerializedReference{(ExposeProperties ? ", ExposeMembersInClass" : "")}] public {Type} {Name}\n";
		scriptString += StringifyCSReferenceGetterAndSetter();
		return scriptString;
	}
	private string StringifyCSReference()
	{
		string scriptString = "";
		scriptString += $"\tprivate {Type} _{Name} = default!;\n";
		scriptString += $"\t[SerializedReference] public {Type} {Name}\n";
		scriptString += StringifyCSReferenceGetterAndSetter();
		return scriptString;
	}
	private string StringifyCSValue()
	{
		string scriptString = "";
		scriptString += $"\tprivate {Type} _{Name} = default!;\n";
		scriptString += $"\t[SerializedValue] public {Type} {Name}\n";
		scriptString += StringifyCSValueGetterAndSetter();
		return scriptString;
	}
	private string StringifyCSPublic()
	{
		string scriptString = "";
		scriptString += $"\tprivate {Type} _{Name} = default!;\n";
		scriptString += $"\tpublic {Type} {Name}\n";
		scriptString += StringifyCSValueGetterAndSetter();
		return scriptString;
	}
	private string StringifyCSPrivate()
	{
		string scriptString = "";
		scriptString += $"\tprivate {Type} _{Name} = default!;\n";
		scriptString += $"\tprivate {Type} {Name}\n";
		scriptString += StringifyCSValueGetterAndSetter();
		return scriptString;
	}
	private string StringifyCSEvent()
	{
		string scriptString = "";
		scriptString += $"\tpublic event Action<{Type}>? {Name}";
		if (HasCustomAdder || HasCustomRemover)
		{
			scriptString += "\n";
			scriptString += "\t{\n";
			if (HasCustomAdder)
			{
				scriptString += "\t\tadd\n";
				scriptString += "\t\t{\n";
				scriptString += "\t\t\t" + AdderContent.Indent().Indent().Indent() + "\n";
				scriptString += "\t\t}\n";
			}
			if (HasCustomRemover)
			{
				scriptString += "\t\tremove\n";
				scriptString += "\t\t{\n";
				scriptString += "\t\t\t" + RemoverContent.Indent().Indent().Indent() + "\n";
				scriptString += "\t\t}\n";
			}
			scriptString += "\t}\n";
		}
		else
		{
			scriptString += ";\n";
		}
		return scriptString;
	}

	private string StringifyCSReferenceGetterAndSetter()
	{
		string scriptString = "";
		scriptString += "\t{\n";
		scriptString += $"\t\tget => _{Name};\n";
		scriptString += "\t\tset\n";
		scriptString += "\t\t{\n";
		
		scriptString += $"\t\t\tif (_{Name} is not null)\n";
		scriptString += $"\t\t\t\tUnsetup_{Name}();\n";
		scriptString += "\n";
		
		scriptString += $"\t\t\t_{Name} = value;\n";
		scriptString += "\n";
		
		scriptString += $"\t\t\tif (_{Name} is not null)\n";
		scriptString += $"\t\t\t\tSetup_{Name}();\n";
		
		scriptString += "\t\t}\n";
		scriptString += "\t}\n";
		
		scriptString += $"\tprotected virtual void Setup_{Name}()\n";
		scriptString += "\t{\n";
		scriptString += EventListeners.Select(eventListener => eventListener.StringifyCSAdd()).Join();
		scriptString += "\t}\n";
		
		scriptString += $"\tprotected virtual void Unsetup_{Name}()\n";
		scriptString += "\t{\n";
		scriptString += EventListeners.Select(eventListener => eventListener.StringifyCSSub()).Join();
		scriptString += "\t}\n";
		
		scriptString += EventListeners.Select(eventListener => eventListener.StringifyCS()).Join();
		return scriptString;
	}
	private string StringifyCSValueGetterAndSetter()
	{
		string scriptString = "";
		scriptString += "\t{\n";
		if (HasCustomGetter)
		{
			scriptString += "\t\tget\n";
			scriptString += "\t\t{\n";
			scriptString += "\t\t\t" + GetterContent.Indent().Indent().Indent() + "\n";
			scriptString += "\t\t}\n";
		}
		else
			scriptString += $"\t\tget => _{Name};\n";
		if (HasCustomSetter)
		{
			scriptString += "\t\tset\n";
			scriptString += "\t\t{\n";
			scriptString += "\t\t\t" + SetterContent.Indent().Indent().Indent() + "\n";
			scriptString += "\t\t}\n";
		}
		else
			scriptString += $"\t\tset => _{Name} = value;\n";
		scriptString += "\t}\n";
		return scriptString;
	}
	
	public string StringifyCSAbstract()
	{
		return PropertyType switch
		{
			PropertyType.Component => StringifyCSComponentAbstract(),
			PropertyType.Reference => StringifyCSReferenceAbstract(),
			PropertyType.Value => StringifyCSValueAbstract(),
			PropertyType.Public => StringifyCSPublicAbstract(),
			PropertyType.Private => StringifyCSPrivateAbstract(),
			PropertyType.Event => StringifyCSEventAbstract(),
			_ => throw new ArgumentOutOfRangeException(PropertyType.ToString())
		};
	}
	private string StringifyCSComponentAbstract()
	{
		return $"\t[SerializedReference{(ExposeProperties ? ", ExposeMembersInClass" : "")}] {Type} {Name} {{ get; set; }}\n";
	}
	private string StringifyCSReferenceAbstract()
	{
		return $"\t[SerializedReference] {Type} {Name} {{ get; set; }}\n";
	}
	private string StringifyCSValueAbstract()
	{
		return $"\t[SerializedValue] {Type} {Name} {{ get; set; }}\n";
	}
	private string StringifyCSPublicAbstract()
	{
		return $"\t{Type} {Name} {{ get; set; }}\n";
	}
	private string StringifyCSPrivateAbstract()
	{
		return $"\t{Type} {Name} {{ get; set; }}\n";
	}
	private string StringifyCSEventAbstract()
	{
		return $"\tevent Action<{Type}>? {Name};\n";
	}
}

public enum PropertyType
{
	Component,
	Reference,
	Value,
	Public,
	Private,
	Event,
}
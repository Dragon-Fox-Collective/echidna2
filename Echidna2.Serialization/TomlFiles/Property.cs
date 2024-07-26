using Tomlyn.Model;

namespace Echidna2.Serialization.TomlFiles;

public class Property
{
	public string Name = "";
	public string Type = "";
	public PropertyType PropertyType;
	public string PropertySerializer = "";
	public bool HasPropertySerializer => !PropertySerializer.IsEmpty();
	public bool Expose = false;
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
	
	private string AttributesString
	{
		get
		{
			List<string> attributes = [];
			if (PropertyType is PropertyType.Component or PropertyType.Reference)
				attributes.Add(HasPropertySerializer ? $"SerializedReference(typeof({PropertySerializer}))" : "SerializedReference");
			if (PropertyType is PropertyType.Value)
				attributes.Add(HasPropertySerializer ? $"SerializedValue(typeof({PropertySerializer}))" : "SerializedValue");
			if (ExposeProperties) attributes.Add("ExposeMembersInClass");
			if (!Expose) attributes.Add("DontExpose");
			
			return attributes.Count != 0 ? attributes.ToDelimString() + " " : "";
		}
	}
	
	public static Property FromToml(TomlTable table, IEnumerable<EventListener> eventListeners)
	{
		Property property = new();
		property.Type = table.GetCasted<string>("Type");
		property.Name = table.GetCasted<string>("Name");
		property.PropertyType = Enum.Parse<PropertyType>(table.GetCasted<string>("PropertyType"));
		property.PropertySerializer = table.GetString("PropertySerializer");
		property.Expose = table.GetCasted("Expose", true);
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
		if (!Expose) table.Add("Expose", Expose);
		if (ExposeProperties ^ (PropertyType == PropertyType.Component)) table.Add("ExposeProperties", ExposeProperties);
		if (HasCustomGetter) table.Add("GetterContent", GetterContent);
		if (HasCustomSetter) table.Add("SetterContent", SetterContent);
		if (HasCustomAdder) table.Add("AdderContent", AdderContent);
		if (HasCustomRemover) table.Add("RemoverContent", RemoverContent);
		return table;
	}
	
	public string StringifyCS()
	{
		return (PropertyType is PropertyType.Event ? StringifyCSEvent() : StringifyCSValue()) + "\n";
	}
	private string StringifyCSValue()
	{
		string scriptString = "";
		scriptString += $"\tprivate {Type} _{Name} = default!;\n";
		scriptString += $"\t{AttributesString}public {Type} {Name}\n";
		if (PropertyType is PropertyType.Component or PropertyType.Reference)
			scriptString += StringifyCSReferenceGetterAndSetter();
		else if (PropertyType is PropertyType.Value or PropertyType.Public or PropertyType.Private)
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
		if (HasCustomGetter)
		{
			scriptString += "\t\tget\n";
			scriptString += "\t\t{\n";
			scriptString += "\t\t\t" + GetterContent.Indent().Indent().Indent() + "\n";
			scriptString += "\t\t}\n";
		}
		else
			scriptString += $"\t\tget => _{Name};\n";
		
		scriptString += "\t\tset\n";
		scriptString += "\t\t{\n";
		
		scriptString += $"\t\t\tif (_{Name} is not null)\n";
		scriptString += $"\t\t\t\tUnsetup_{Name}();\n";
		scriptString += "\n";
		
		if (HasCustomSetter)
			scriptString += "\t\t\t" + SetterContent.Indent().Indent().Indent() + "\n";
		else
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
		return PropertyType is PropertyType.Event ? StringifyCSEventAbstract() : StringifyCSValueAbstract();
	}
	private string StringifyCSValueAbstract()
	{
		return $"\t{AttributesString} public {Type} {Name} {{ get; set; }}\n";
	}
	private string StringifyCSEventAbstract()
	{
		return $"\tevent Action<{Type}>? {Name};\n";
	}
	
	public override string ToString() => $"{PropertyType} {Type} {Name}";
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
using Tomlyn.Model;

namespace Echidna2.Serialization;

public static class SerializationPredicates
{
	public static bool IdIsValidComponentId(KeyValuePair<string, object> pair) => pair.Key is "This" || pair.Key.All(char.IsDigit);
	
	public static bool EventIsNotification(TomlTable @event) => (string)@event["EventType"] == "Notification";
	public static bool EventIsSelf(TomlTable @event) => (string)@event["EventType"] == "Self";
	public static bool EventIsReference(TomlTable @event) => (string)@event["EventType"] == "Reference";
	public static Func<TomlTable, bool> EventIsReferenceAndTargets(TomlTable property) => @event => EventIsReference(@event) && (string)@event["Target"] == (string)property["Name"];
	
	public static bool PropertyIsValue(TomlTable property) => (string)property["PropertyType"] == "Value";
	public static bool PropertyIsReference(TomlTable property) => (string)property["PropertyType"] == "Reference";
	public static bool PropertyIsPrivate(TomlTable property) => (string)property["PropertyType"] == "Private";
	public static bool PropertyIsPublic(TomlTable property) => (string)property["PropertyType"] == "Public";
	public static bool PropertyIsEvent(TomlTable property) => (string)property["PropertyType"] == "Event";
	
	public static bool ComponentHasValidIdAndNeedsCustomClass(KeyValuePair<string, object> pair) => IdIsValidComponentId(pair) && ComponentNeedsCustomClass(pair.Key, (TomlTable)pair.Value);
	public static bool ComponentNeedsCustomClass(string id, TomlTable component) => id is "This" ? !component.Keys.Any(key => key is "Component" or "Prefab") : component.Keys.Any(key => key is "Components" or "Properties" or "Events" or "Functions" or "Interfaces");
}
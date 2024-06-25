using Tomlyn.Model;

namespace Echidna2.Serialization;

public static class SerializationPredicates
{
	public static bool IdIsValidComponentId(KeyValuePair<string, object> pair) => pair.Key.All(char.IsDigit);
	
	public static bool EventIsNotification(TomlTable @event) => (string)@event["EventType"] == "Notification";
	public static bool EventIsReference(TomlTable @event) => (string)@event["EventType"] == "Reference";
	public static Func<TomlTable, bool> EventIsReferenceAndTargets(TomlTable property) => @event => EventIsReference(@event) && (string)@event["Target"] == (string)property["Name"];
	
	public static bool PropertyIsValue(TomlTable property) => (string)property["PropertyType"] == "Value";
	public static bool PropertyIsReference(TomlTable property) => (string)property["PropertyType"] == "Reference";
	public static bool PropertyIsEvent(TomlTable property) => (string)property["PropertyType"] == "Event";
}
using System.Collections;
using System.Globalization;
using Tomlyn.Model;

namespace Echidna2.Serialization;

public static class CustomTomlStringifier
{
	public static string Stringify(TomlTable table) => StringifyTable(null, table, false);
	
	private static string StringifyTable(string? totalId, object table, bool isArrayMember)
	{
		Dictionary<string, object> values = Dictionaryify(totalId, table);
		
		string tableString = "";
		
		string totalIdPrefix = totalId is null ? "" : $"{totalId}.";
		
		List<KeyValuePair<string, object>> thisLevelValues = values.Where(pair => pair.Value is not TomlTable and not IDictionary && !ObjectIsArrayOfTables(pair.Value)).ToList();
		if (thisLevelValues.Count != 0)
		{
			if (totalId is not null) tableString += isArrayMember ? $"[[{totalId}]]\n" : $"[{totalId}]\n";
			foreach ((string id, object value) in thisLevelValues)
				tableString += $"{id} = {StringifyValue($"{totalIdPrefix}{id}", value)}\n";
			tableString += "\n";
		}
		
		List<KeyValuePair<string, object>> nextLevelTables = values.Except(thisLevelValues).ToList();
		foreach ((string id, object value) in nextLevelTables)
		{
			if (ObjectIsArrayOfTables(value))
			{
				foreach (object element in (IEnumerable)value)
					tableString += StringifyTable($"{totalIdPrefix}{id}", element, true);
			}
			else
				tableString += StringifyTable($"{totalIdPrefix}{id}", value, false);
		}
		
		return tableString;
	}
	
	private static string StringifyValue(string totalId, object value)
	{
		return value switch
		{
			bool b => b.ToString().ToLower(),
			string s => $"\"{s}\"",
			int i => i.ToString(),
			long l => l.ToString(),
			float f => f.ToString("0.0################", CultureInfo.InvariantCulture),
			double d => d.ToString("0.0################", CultureInfo.InvariantCulture),
			IEnumerable l => $"[{l.Cast<object>().Select(v => $"\n{StringifyValue(totalId, v)},").Join().Indent()}\n]",
			_ => throw new NotImplementedException($"Cannot stringify value of type {value.GetType()} (totalId: '{totalId}', value: '{value}')")
		};
	}
	
	private static Dictionary<string, object> Dictionaryify(string? totalId, object table)
	{
		return table switch
		{
			TomlTable t => t.ToDictionary(),
			IDictionary d => d.Keys.Cast<string>().Zip(d.Values.Cast<object>(), (a, b) => new KeyValuePair<string, object>(a, b)).ToDictionary(),
			_ => throw new NotImplementedException($"Cannot stringify table of type {table.GetType()} (totalId: '{totalId}', value: '{table}')")
		};
	}
	
	private static bool ObjectIsArrayOfTables(object obj)
	{
		if (obj is not IEnumerable enumerable) return false;
		List<object> objects = enumerable.Cast<object>().ToList();
		if (objects.Count == 0) return false;
		object first = objects.First();
		if (first is IDictionary or TomlTable) return true;
		if (first is IEnumerable and not string) throw new InvalidOperationException("Cannot stringify array of arrays");
		return false;
	}
}
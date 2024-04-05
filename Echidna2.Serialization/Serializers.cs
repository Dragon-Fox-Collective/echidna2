using System.Drawing;
using System.Reflection;
using Tomlyn.Model;

namespace Echidna2.Serialization;

public interface Serializer
{
	public object Serialize(object value);
	public object Deserialize(object? value, object data);
}

public interface Serializer<TSerialized, TDeserialized> : Serializer where TSerialized : notnull where TDeserialized : notnull
{
	object Serializer.Serialize(object value) => Serialize((TDeserialized)value);
	object Serializer.Deserialize(object? value, object data) => Deserialize(value is null ? default : (TDeserialized)value, (TSerialized)data);
	public TSerialized Serialize(TDeserialized value);
	public TDeserialized Deserialize(TDeserialized? value, TSerialized data);
}

public class DirectSerializer<T> : Serializer<T, T> where T : notnull
{
	public T Serialize(T value) => value;
	public T Deserialize(T? value, T data) => data;
}

public class EnumSerializer(Type type) : Serializer<string, Enum>
{
	public string Serialize(Enum value) => value.ToString();
	public Enum Deserialize(Enum? value, string data) => (Enum)Enum.Parse(type, data);
}

public class SubComponentSerializer(Func<TomlTable, object> deserializeValue, string fieldName, string componentId) : Serializer<TomlTable, object>
{
	public TomlTable Serialize(object value)
	{
		TomlTable table = new();
		
		foreach (MemberInfo member in value.GetType()
			         .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
			         .Where(member => member.GetCustomAttribute<SerializedValueAttribute>() is not null))
		{
			IMemberWrapper wrapper = IMemberWrapper.Wrap(member);
			SerializedValueAttribute attribute = wrapper.Member.GetCustomAttribute<SerializedValueAttribute>()!;
			Serializer serializer = attribute.GetSerializer(wrapper.FieldType, null, null, null);
			table.Add(wrapper.Name, serializer.Serialize(wrapper.GetValue(value)!));
		}
		
		return table;
	}
	
	public object Deserialize(object? value, TomlTable data)
	{
		object deserializedValue = deserializeValue(data);
		if (data.Count != 0)
			Console.WriteLine($"WARN: Unused table {fieldName} {data.ToDelimString()} of {componentId} leftover");
		return deserializedValue;
	}
}

public class ColorSerializer : Serializer<TomlTable, Color>
{
	public TomlTable Serialize(Color color)
	{
		TomlTable table = new();
		table.Add("A", color.A / 255.0);
		table.Add("R", color.R / 255.0);
		table.Add("G", color.G / 255.0);
		table.Add("B", color.B / 255.0);
		return table;
	}
	
	public Color Deserialize(Color value, TomlTable data)
	{
		Color color = Color.FromArgb(
			(int)((double)data["A"] * 255),
			(int)((double)data["R"] * 255),
			(int)((double)data["G"] * 255),
			(int)((double)data["B"] * 255));
		data.Remove("A");
		data.Remove("R");
		data.Remove("G");
		data.Remove("B");
		return color;
	}
}

public interface ReferenceSerializer
{
	public object Serialize(object value, Func<object, string> getReferenceTo);
	public object Deserialize(object? value, object data, Func<string, object> getReferenceFrom);
}

public interface ReferenceSerializer<TSerialized, TDeserialized> : ReferenceSerializer where TSerialized : notnull where TDeserialized : notnull
{
	object ReferenceSerializer.Serialize(object value, Func<object, string> getReferenceTo) => Serialize((TDeserialized)value, getReferenceTo);
	object ReferenceSerializer.Deserialize(object? value, object data, Func<string, object> getReferenceFrom) => Deserialize(value is null ? default : (TDeserialized)value, (TSerialized)data, getReferenceFrom);
	public TSerialized Serialize(TDeserialized value, Func<object, string> getReferenceTo);
	public TDeserialized Deserialize(TDeserialized? value, TSerialized data, Func<string, object> getReferenceFrom);
}

public class DefaultReferenceSerializer : ReferenceSerializer<string, object>
{
	public string Serialize(object value, Func<object, string> getReferenceTo) => getReferenceTo(value);
	public object Deserialize(object? value, string data, Func<string, object> getReferenceFrom) => getReferenceFrom(data);
}
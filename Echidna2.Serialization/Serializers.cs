using System.Drawing;
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
using System.Drawing;
using Tomlyn.Model;

namespace Echidna2.Serialization;

public class ColorSerializer : Serializer<TomlTable, Color>
{
	public static TomlTable Serialize(Color color)
	{
		TomlTable table = new();
		table.Add("A", color.A / 255.0);
		table.Add("R", color.R / 255.0);
		table.Add("G", color.G / 255.0);
		table.Add("B", color.B / 255.0);
		return table;
	}
	
	public static Color Deserialize(TomlTable table)
	{
		Color color = Color.FromArgb(
			(int)((double)table["A"] * 255),
			(int)((double)table["R"] * 255),
			(int)((double)table["G"] * 255),
			(int)((double)table["B"] * 255));
		table.Remove("A");
		table.Remove("R");
		table.Remove("G");
		table.Remove("B");
		return color;
	}
}
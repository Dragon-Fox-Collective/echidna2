using System.Drawing;
using Tomlyn.Model;

namespace Echidna2.Serialization;

public static class SystemSerialization
{
	public static Color DeserializeColor(TomlTable table)
	{
		Color color = Color.FromArgb((int)((double)table["A"] * 255), (int)((double)table["R"] * 255), (int)((double)table["G"] * 255), (int)((double)table["B"] * 255));
		table.Remove("A");
		table.Remove("R");
		table.Remove("G");
		table.Remove("B");
		return color;
	}
}
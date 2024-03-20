using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Echidna2.Rendering;

public static class KeysExtensions
{
	public static void ManipulateText(this Keys key, KeyModifiers modifiers, ref string value, ref int cursorPosition)
	{
		if (key is Keys.Left)
			cursorPosition = Math.Max(0, cursorPosition - 1);
		else if (key is Keys.Right)
			cursorPosition = Math.Min(value.Length, cursorPosition + 1);
		else if (key is Keys.Backspace)
			value = cursorPosition == 0 ? value : value.Remove(--cursorPosition, 1);
		else if (key is Keys.Delete)
			value = cursorPosition == value.Length ? value : value.Remove(cursorPosition, 1);
		else if (key is >= Keys.A and <= Keys.Z && !modifiers.HasFlag(KeyModifiers.Shift))
			value = value.Insert(cursorPosition++, key.ToString().ToLower());
		else if (key is >= Keys.A and <= Keys.Z && modifiers.HasFlag(KeyModifiers.Shift))
			value = value.Insert(cursorPosition++, key.ToString());
		else if (key is Keys.Space)
			value = value.Insert(cursorPosition++, " ");
		else if (key is Keys.D0 or Keys.KeyPad0)
			value = value.Insert(cursorPosition++, "0");
		else if (key is Keys.D1 or Keys.KeyPad1)
			value = value.Insert(cursorPosition++, "1");
		else if (key is Keys.D2 or Keys.KeyPad2)
			value = value.Insert(cursorPosition++, "2");
		else if (key is Keys.D3 or Keys.KeyPad3)
			value = value.Insert(cursorPosition++, "3");
		else if (key is Keys.D4 or Keys.KeyPad4)
			value = value.Insert(cursorPosition++, "4");
		else if (key is Keys.D5 or Keys.KeyPad5)
			value = value.Insert(cursorPosition++, "5");
		else if (key is Keys.D6 or Keys.KeyPad6)
			value = value.Insert(cursorPosition++, "6");
		else if (key is Keys.D7 or Keys.KeyPad7)
			value = value.Insert(cursorPosition++, "7");
		else if (key is Keys.D8 or Keys.KeyPad8)
			value = value.Insert(cursorPosition++, "8");
		else if (key is Keys.D9 or Keys.KeyPad9)
			value = value.Insert(cursorPosition++, "9");
		else if (key is Keys.KeyPadDivide)
			value = value.Insert(cursorPosition++, "/");
		else if (key is Keys.KeyPadMultiply)
			value = value.Insert(cursorPosition++, "*");
		else if (key is Keys.KeyPadSubtract)
			value = value.Insert(cursorPosition++, "-");
		else if (key is Keys.KeyPadAdd)
			value = value.Insert(cursorPosition++, "+");
		else if (key is Keys.KeyPadDecimal)
			value = value.Insert(cursorPosition++, ".");
		else if (key is Keys.Home or Keys.PageUp)
			cursorPosition = 0;
		else if (key is Keys.End or Keys.PageDown)
			cursorPosition = value.Length;
		else if (key
		         is not Keys.LeftShift
		         and not Keys.RightShift
		         and not Keys.LeftControl
		         and not Keys.RightControl
		         and not Keys.LeftAlt
		         and not Keys.RightAlt
		         and not Keys.Menu
		         and not Keys.Enter
		         and not Keys.KeyPadEnter
		         and not Keys.Escape
		         and not Keys.Tab)
			Console.WriteLine($"Unhandled key: {key}");
	}
}
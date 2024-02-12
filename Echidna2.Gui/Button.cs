using Echidna2.Mathematics;
using Echidna2.Rendering;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Echidna2.Gui;

public class Button(RectTransform rectTransform)
	: IMouseDown
{
	public void OnMouseDown(MouseButton button, Vector2 position)
	{
		Console.WriteLine($"Button clicked! {button} {position}");
	}
}
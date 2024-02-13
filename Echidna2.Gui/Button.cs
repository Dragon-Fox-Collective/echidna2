using Echidna2.Mathematics;
using Echidna2.Rendering;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Echidna2.Gui;

public class Button(RectTransform rectTransform) : IMouseDown
{
	public void OnMouseDown(MouseButton button, Vector2 position, Vector3 globalPosition)
	{
		if (rectTransform.ContainsGlobalPoint(globalPosition.XY))
			Console.WriteLine($"Button clicked! {button} {position} {globalPosition.XY}");
	}
}
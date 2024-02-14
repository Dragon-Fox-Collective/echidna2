using Echidna2.Mathematics;
using Echidna2.Rendering;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Echidna2.Gui;

public class Button(RectTransform rectTransform) : IMouseDown
{
	public event Action? Clicked;
	
	public void OnMouseDown(MouseButton button, Vector2 position, Vector3 globalPosition)
	{
		if (rectTransform.ContainsGlobalPoint(globalPosition.XY))
			Clicked?.Invoke();
	}
}
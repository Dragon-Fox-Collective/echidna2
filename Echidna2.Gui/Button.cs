using Echidna2.Mathematics;
using Echidna2.Rendering;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Echidna2.Gui;

public class Button(RectTransform rectTransform) : IMouseDown, IMouseUp
{
	public event Action? Clicked;
	public event Action? MouseDown;
	public event Action? MouseUp;
	
	private bool wasPressed;
	
	public void OnMouseDown(MouseButton button, Vector2 position, Vector3 globalPosition)
	{
		if (rectTransform.ContainsGlobalPoint(globalPosition.XY))
		{
			wasPressed = true;
			MouseDown?.Invoke();
		}
	}
	
	public void OnMouseUp(MouseButton button, Vector2 position, Vector3 globalPosition)
	{
		if (wasPressed)
		{
			wasPressed = false;
			MouseUp?.Invoke();
			
			if (rectTransform.ContainsGlobalPoint(globalPosition.XY))
				Clicked?.Invoke();
		}
	}
}
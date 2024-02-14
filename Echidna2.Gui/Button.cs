using Echidna2.Mathematics;
using Echidna2.Rendering;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Echidna2.Gui;

public class Button(RectTransform rectTransform) : IMouseDown, IMouseUp, IMouseMoved
{
	public event Action? Clicked;
	public event Action? Dragged;
	public event Action? MouseDown;
	public event Action? MouseUp;
	public event Action? MouseEnter;
	public event Action? MouseExit;
	
	private bool wasPressed;
	private bool wasInside;
	
	public void OnMouseDown(MouseButton button, Vector2 position, Vector3 globalPosition)
	{
		Console.WriteLine($"Mouse down at {globalPosition}");
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
	
	public void OnMouseMoved(Vector2 position, Vector2 delta, Vector3 globalPosition)
	{
		if (wasPressed)
			Dragged?.Invoke();
		
		if (rectTransform.ContainsGlobalPoint(globalPosition.XY))
		{
			if (!wasInside)
			{
				wasInside = true;
				MouseEnter?.Invoke();
			}
		}
		else
		{
			if (wasInside)
			{
				wasInside = false;
				MouseExit?.Invoke();
			}
		}
	}
}
using Echidna2.Mathematics;
using Echidna2.Rendering;
using Echidna2.Serialization;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Echidna2.Gui;

public interface IButton
{
	public event Action? Clicked;
}

public class Button : IButton, IMouseDown, IMouseUp, IMouseMoved
{
	public event Action? Clicked;
	public event Action<Vector2>? Dragged;
	public event Action? MouseDown;
	public event Action? MouseDownOutside;
	public event Action? MouseUp;
	public event Action? MouseUpOutside;
	public event Action? MouseEnter;
	public event Action? MouseExit;
	
	[SerializedReference] public IRectTransform RectTransform { get; set; } = null!;
	
	private bool wasPressed;
	private bool wasInside;
	
	private Vector3 lastGlobalPosition;
	
	public void OnMouseDown(MouseButton button, Vector2 position, Vector3 globalPosition, bool clipped)
	{
		if (button != MouseButton.Left) return;
		
		if (!clipped && RectTransform.ContainsGlobalPoint(globalPosition.XY))
		{
			wasPressed = true;
			MouseDown?.Invoke();
		}
		else
		{
			MouseDownOutside?.Invoke();
		}
	}
	
	public void OnMouseUp(MouseButton button, Vector2 position, Vector3 globalPosition, bool clipped)
	{
		if (button != MouseButton.Left) return;
		
		if (wasPressed)
		{
			wasPressed = false;
			MouseUp?.Invoke();
			
			if (RectTransform.ContainsGlobalPoint(globalPosition.XY))
				Clicked?.Invoke();
		}
		else
		{
			MouseUpOutside?.Invoke();
		}
	}
	
	public void OnMouseMoved(Vector2 position, Vector2 delta, Vector3 globalPosition, bool clipped)
	{
		if (wasPressed)
			Dragged?.Invoke(globalPosition.XY - lastGlobalPosition.XY);
		
		if (!clipped && RectTransform.ContainsGlobalPoint(globalPosition.XY))
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
		
		lastGlobalPosition = globalPosition;
	}
}
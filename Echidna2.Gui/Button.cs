﻿using Echidna2.Mathematics;
using Echidna2.Rendering;
using Echidna2.Serialization;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Echidna2.Gui;

public class Button : IMouseDown, IMouseUp, IMouseMoved
{
	[SerializedEvent] public event Action? Clicked;
	public event Action? Dragged;
	public event Action? MouseDown;
	public event Action? MouseUp;
	public event Action? MouseEnter;
	public event Action? MouseExit;
	
	[SerializedReference] public RectTransform RectTransform { get; set; } = null!;
	
	private bool wasPressed;
	private bool wasInside;
	
	public void OnMouseDown(MouseButton button, Vector2 position, Vector3 globalPosition)
	{
		if (RectTransform.ContainsGlobalPoint(globalPosition.XY))
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
			
			if (RectTransform.ContainsGlobalPoint(globalPosition.XY))
				Clicked?.Invoke();
		}
	}
	
	public void OnMouseMoved(Vector2 position, Vector2 delta, Vector3 globalPosition)
	{
		if (wasPressed)
			Dragged?.Invoke();
		
		if (RectTransform.ContainsGlobalPoint(globalPosition.XY))
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
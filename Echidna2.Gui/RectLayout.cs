﻿using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Rendering;
using Echidna2.Serialization;

namespace Echidna2.Gui;

public class RectLayout : INotificationHook<UpdateNotification>, INotificationHook<DrawNotification>, INotificationHook<IMouseNotification>
{
	[SerializedValue] public bool ClipChildren;
	
	[SerializedValue] public bool LayOutChildren = true;
	
	private List<IMouseNotification> clippedClicks = [];
	
	private List<(RectTransform child, RectTransform.LocalTransformChangedHandler handler)> localTransformChangedHandlers = [];
	
	[SerializedReference] public RectTransform RectTransform { get; set; } = null!;
	private Hierarchy? hierarchy;
	[SerializedReference] public Hierarchy Hierarchy
	{
		get => hierarchy!;
		set
		{
			if (hierarchy is not null)
			{
				Hierarchy.ChildAdded -= AddLayoutChild;
				Hierarchy.ChildRemoved -= RemoveLayoutChild;
				ChildrenThatCanBeLaidOut.ForEach(RemoveLayoutChild);
			}
			
			hierarchy = value;
			
			if (hierarchy is not null)
			{
				Hierarchy.ChildAdded += AddLayoutChild;
				Hierarchy.ChildRemoved += RemoveLayoutChild;
				ChildrenThatCanBeLaidOut.ForEach(AddLayoutChild);
			}
		}
	}
	public IEnumerable<ICanBeLaidOut> ChildrenThatCanBeLaidOut => Hierarchy.Children.OfType<ICanBeLaidOut>().Where(child => child is not IVisibility visible || visible.IsVisible);
	
	public void AddLayoutChild(object child)
	{
		if (child is ICanBeLaidOut childLaidOut)
		{
			RectTransform childRect = childLaidOut.RectTransform;
			RectTransform.LocalTransformChangedHandler handler = () => childRect.GlobalTransform = RectTransform.GlobalTransform * childRect.LocalTransform;
			localTransformChangedHandlers.Add((childRect, handler));
			childRect.LocalTransformChanged += handler;
		}
	}
	
	public void RemoveLayoutChild(object child)
	{
		if (child is ICanBeLaidOut childLaidOut)
		{
			RectTransform childRect = childLaidOut.RectTransform;
			(RectTransform child, RectTransform.LocalTransformChangedHandler handler) tuple = localTransformChangedHandlers.First(tuple => tuple.child == childRect);
			childRect.LocalTransformChanged -= tuple.handler;
			localTransformChangedHandlers.Remove(tuple);
		}
	}
	
	public virtual void OnPreNotify(UpdateNotification notification)
	{
		foreach (ICanBeLaidOut child in ChildrenThatCanBeLaidOut)
		{
			RectTransform childRect = child.RectTransform;
			if (LayOutChildren)
			{
				double left = (RectTransform.LocalSize.X * (childRect.AnchorLeft - 0.5) + Math.Min(childRect.AnchorOffsetLeft, childRect.AnchorLeft.Lerp(childRect.AnchorOffsetLeft, childRect.AnchorOffsetRight) - childRect.AnchorLeft * childRect.MinimumSize.X)) / childRect.LocalScale.X;
				double right = (RectTransform.LocalSize.X * (childRect.AnchorRight - 0.5) + Math.Max(childRect.AnchorOffsetRight, childRect.AnchorRight.Lerp(childRect.AnchorOffsetLeft, childRect.AnchorOffsetRight) + (1 - childRect.AnchorRight) * childRect.MinimumSize.X)) / childRect.LocalScale.X;
				double bottom = (RectTransform.LocalSize.Y * (childRect.AnchorBottom - 0.5) + Math.Min(childRect.AnchorOffsetBottom, childRect.AnchorBottom.Lerp(childRect.AnchorOffsetBottom, childRect.AnchorOffsetTop) - childRect.AnchorBottom * childRect.MinimumSize.Y)) / childRect.LocalScale.Y;
				double top = (RectTransform.LocalSize.Y * (childRect.AnchorTop - 0.5) + Math.Max(childRect.AnchorOffsetTop, childRect.AnchorTop.Lerp(childRect.AnchorOffsetBottom, childRect.AnchorOffsetTop) + (1 - childRect.AnchorTop) * childRect.MinimumSize.Y)) / childRect.LocalScale.Y;
				childRect.LocalSize = new Vector2(right - left, top - bottom);
				childRect.LocalPosition = new Vector2(right + left, top + bottom) / 2;
			}
			childRect.Depth = RectTransform.Depth + 1;
			
			// Console.WriteLine($"{(child as INamed)?.Name ?? "No name"} {childRect.AnchorPreset} {left} {right} {bottom} {top} {childRect.MinimumSize} {childRect.LocalSize} {childRect.LocalPosition} {childRect.Depth}");
		}
	}
	public virtual void OnPostNotify(UpdateNotification notification)
	{
		
	}
	public virtual void OnPostPropagate(UpdateNotification notification)
	{
		
	}
	
	public void OnPreNotify(DrawNotification notification)
	{
		if (ClipChildren)
		{
			Vector2 screenPosition = notification.Camera.GlobalToScreen(RectTransform.GlobalPosition.WithZ(1));
			Vector2 screenSize = RectTransform.GlobalSize;
			ScissorStack.PushScissor(screenPosition.X - screenSize.X / 2, screenPosition.Y - screenSize.Y / 2, screenSize.X, screenSize.Y);
		}
	}
	public void OnPostNotify(DrawNotification notification)
	{
		
	}
	public void OnPostPropagate(DrawNotification notification)
	{
		if (ClipChildren)
			ScissorStack.PopScissor();
	}
	
	public void OnPreNotify(IMouseNotification notification)
	{
		if (!notification.Clipped && ClipChildren && !RectTransform.ContainsGlobalPoint(notification.GlobalPosition.XY))
		{
			notification.Clipped = true;
			clippedClicks.Add(notification);
		}
	}
	public void OnPostNotify(IMouseNotification notification)
	{
		
	}
	public void OnPostPropagate(IMouseNotification notification)
	{
		if (clippedClicks.Remove(notification))
			notification.Clipped = false;
	}
}

public interface ICanBeLaidOut
{
	public RectTransform RectTransform { get; set; }
}
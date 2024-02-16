using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Rendering;

namespace Echidna2.Gui;

public class RectLayout : INotificationHook<IUpdate.Notification>, INotificationHook<IDraw.Notification>
{
	public bool ClipChildren;
	
	private List<(RectTransform child, RectTransform.LocalTransformChangedHandler handler)> localTransformChangedHandlers = [];
	
	protected RectTransform RectTransform;
	protected Hierarchy Hierarchy;
	
	public RectLayout(RectTransform rectTransform, Hierarchy hierarchy)
	{
		RectTransform = rectTransform;
		Hierarchy = hierarchy;
		
		Hierarchy.ChildAdded += child =>
		{
			if (child is ICanBeLaidOut childLaidOut)
			{
				RectTransform childRect = childLaidOut.RectTransform;
				RectTransform.LocalTransformChangedHandler handler = () => childRect.GlobalTransform = RectTransform.GlobalTransform * childRect.LocalTransform;
				localTransformChangedHandlers.Add((childRect, handler));
				childRect.LocalTransformChanged += handler;
			}
		};
		Hierarchy.ChildRemoved += child =>
		{
			if (child is ICanBeLaidOut childLaidOut)
			{
				RectTransform childRect = childLaidOut.RectTransform;
				(RectTransform child, RectTransform.LocalTransformChangedHandler handler) tuple = localTransformChangedHandlers.First(tuple => tuple.child == childRect);
				childRect.LocalTransformChanged -= tuple.handler;
				localTransformChangedHandlers.Remove(tuple);
			}
		};
	}
	
	public virtual void OnPreNotify(IUpdate.Notification notification)
	{
		foreach (ICanBeLaidOut child in Hierarchy.Children.OfType<ICanBeLaidOut>())
		{
			RectTransform childRect = child.RectTransform;
			double left = RectTransform.LocalSize.X * (childRect.AnchorLeft - 0.5) + childRect.AnchorOffsetLeft;
			double right = RectTransform.LocalSize.X * (childRect.AnchorRight - 0.5) + childRect.AnchorOffsetRight;
			double bottom = RectTransform.LocalSize.Y * (childRect.AnchorBottom - 0.5) + childRect.AnchorOffsetBottom;
			double top = RectTransform.LocalSize.Y * (childRect.AnchorTop - 0.5) + childRect.AnchorOffsetTop;
			childRect.LocalSize = new Vector2(Math.Max(right - left, childRect.MinimumSize.X), Math.Max(top - bottom, childRect.MinimumSize.Y));
			childRect.LocalPosition = new Vector2(left + right, bottom + top) / 2;
			childRect.Depth = RectTransform.Depth + 1;
			
			// Console.WriteLine($"{childRect.AnchorPreset} {left} {right} {bottom} {top} {childRect.Size} {childRect.Position} {childRect.Depth}");
		}
	}
	public virtual void OnPostNotify(IUpdate.Notification notification)
	{
		
	}
	public virtual void OnPostPropagate(IUpdate.Notification notification)
	{
		
	}
	
	public void OnPreNotify(IDraw.Notification notification)
	{
		if (ClipChildren)
		{
			Vector2 screenPosition = notification.Camera.GlobalToScreen(RectTransform.GlobalPosition.WithZ(1));
			Vector2 screenSize = RectTransform.GlobalSize;
			ScissorStack.PushScissor(screenPosition.X - screenSize.X / 2, screenPosition.Y - screenSize.Y / 2, screenSize.X, screenSize.Y);
		}
	}
	public void OnPostNotify(IDraw.Notification notification)
	{
		
	}
	public void OnPostPropagate(IDraw.Notification notification)
	{
		if (ClipChildren)
			ScissorStack.PopScissor();
	}
}

public interface ICanBeLaidOut
{
	public RectTransform RectTransform { get; set; }
}
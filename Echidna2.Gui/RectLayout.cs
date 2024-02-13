using Echidna2.Core;
using Echidna2.Mathematics;

namespace Echidna2.Gui;

public class RectLayout : INotificationHook<IUpdate.Notification>
{
	private List<(RectTransform child, RectTransform.LocalTransformChangedHandler handler)> localTransformChangedHandlers = [];
	
	private RectTransform rectTransform;
	private Hierarchy hierarchy;
	
	public RectLayout(RectTransform rectTransform, Hierarchy hierarchy)
	{
		this.rectTransform = rectTransform;
		this.hierarchy = hierarchy;
		hierarchy.ChildAdded += child =>
		{
			if (child is ICanBeLaidOut childLaidOut)
			{
				RectTransform childRect = childLaidOut.RectTransform;
				RectTransform.LocalTransformChangedHandler handler = () => childRect.GlobalTransform = rectTransform.GlobalTransform * childRect.LocalTransform;
				localTransformChangedHandlers.Add((childRect, handler));
				childRect.LocalTransformChanged += handler;
			}
		};
		hierarchy.ChildRemoved += child =>
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
	
	public void OnPreNotify(IUpdate.Notification notification)
	{
		Console.WriteLine($"Laying out {string.Join(", ", hierarchy.GetChildren())}");
		foreach (ICanBeLaidOut child in hierarchy.GetChildren().OfType<ICanBeLaidOut>())
		{
			RectTransform childRect = child.RectTransform;
			double left = rectTransform.Size.X * (childRect.AnchorLeft - 0.5) + childRect.AnchorOffsetLeft;
			double right = rectTransform.Size.X * (childRect.AnchorRight - 0.5) + childRect.AnchorOffsetRight;
			double bottom = rectTransform.Size.Y * (childRect.AnchorBottom - 0.5) + childRect.AnchorOffsetBottom;
			double top = rectTransform.Size.Y * (childRect.AnchorTop - 0.5) + childRect.AnchorOffsetTop;
			childRect.Size = new Vector2(Math.Max(right - left, childRect.MinimumSize.X), Math.Max(top - bottom, childRect.MinimumSize.Y));
			childRect.Position = new Vector2(left + right, bottom + top) / 2;
			childRect.Depth = rectTransform.Depth + 1;
			
			Console.WriteLine($"{childRect.AnchorPreset} {left} {right} {bottom} {top} {childRect.Size} {childRect.Position} {childRect.Depth}");
		}
	}
	public void OnPostNotify(IUpdate.Notification notification)
	{
		
	}
	public void OnPostPropagate(IUpdate.Notification notification)
	{
		
	}
}

public interface ICanBeLaidOut
{
	public RectTransform RectTransform { get; set; }
}
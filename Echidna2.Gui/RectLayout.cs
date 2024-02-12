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
			if (child is RectTransform childRect)
			{
				RectTransform.LocalTransformChangedHandler handler = () => childRect.GlobalTransform = rectTransform.GlobalTransform * childRect.LocalTransform;
				localTransformChangedHandlers.Add((childRect, handler));
				childRect.LocalTransformChanged += handler;
			}
		};
		hierarchy.ChildRemoved += child =>
		{
			if (child is RectTransform childRect)
			{
				(RectTransform child, RectTransform.LocalTransformChangedHandler handler) tuple = localTransformChangedHandlers.First(tuple => tuple.child == childRect);
				childRect.LocalTransformChanged -= tuple.handler;
				localTransformChangedHandlers.Remove(tuple);
			}
		};
	}
	
	public void OnPreNotify(IUpdate.Notification notification)
	{
		foreach (RectTransform child in hierarchy.GetChildren().OfType<RectTransform>())
		{
			double left = rectTransform.Size.X * (child.AnchorLeft - 0.5) + child.AnchorOffsetLeft;
			double right = rectTransform.Size.X * (child.AnchorRight - 0.5) + child.AnchorOffsetRight;
			double bottom = rectTransform.Size.Y * (child.AnchorBottom - 0.5) + child.AnchorOffsetBottom;
			double top = rectTransform.Size.Y * (child.AnchorTop - 0.5) + child.AnchorOffsetTop;
			child.Size = new Vector2(Math.Max(right - left, child.MinimumSize.X), Math.Max(top - bottom, child.MinimumSize.Y));
			child.Position = new Vector2(left + right, bottom + top) / 2;
			child.Depth = rectTransform.Depth + 1;
			
			// Console.WriteLine($"{child.AnchorPreset} {left} {right} {bottom} {top} {child.Size} {child.Position} {child.Depth}");
		}
	}
	public void OnPostNotify(IUpdate.Notification notification)
	{
		
	}
	public void OnPostPropagate(IUpdate.Notification notification)
	{
		
	}
}
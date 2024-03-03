using Echidna2.Core;
using Echidna2.Serialization;

namespace Echidna2.Gui;

public class VerticalLayout : RectLayout
{
	[SerializedValue] public VerticalLayoutDirection LayoutDirection = VerticalLayoutDirection.TopToBottom;
	
	public override void OnPreNotify(IUpdate.Notification notification)
	{
		List<RectTransform> laidOutChildren = Hierarchy.Children.OfType<ICanBeLaidOut>().Select(child => child.RectTransform).ToList();
		
		foreach (RectTransform child in laidOutChildren)
		{
			child.AnchorPreset = child.HorizontalSizing switch
			{
				LayoutSizing.Stretch => AnchorPreset.WideBottom,
				LayoutSizing.FitBegin => AnchorPreset.BottomLeft,
				LayoutSizing.FitCenter => AnchorPreset.BottomCenter,
				LayoutSizing.FitEnd => AnchorPreset.BottomRight,
				_ => throw new IndexOutOfRangeException()
			};
		}
		
		base.OnPreNotify(notification);
		
		if (laidOutChildren.Count != 0)
			RectTransform.MinimumSize = (
				laidOutChildren.Max(child => child.MinimumSize.X),
				laidOutChildren.Sum(child => child.MinimumSize.Y));
		else
			RectTransform.MinimumSize = (0, 0);
		
		double remainingHeight = RectTransform.LocalSize.Y - RectTransform.MinimumSize.Y;
		double totalExpand = laidOutChildren.Where(child => child.VerticalExpand).Sum(child => child.VerticalExpandFactor);
		
		double directionModifier = LayoutDirection switch
		{
			VerticalLayoutDirection.TopToBottom => -1,
			VerticalLayoutDirection.BottomToTop => +1,
			_ => throw new IndexOutOfRangeException()
		};
		double y = -directionModifier * RectTransform.LocalSize.Y / 2;
		foreach (RectTransform child in laidOutChildren)
		{
			double minimumHeight = child.MinimumSize.Y;
			double extraHeight = 0;
			if (child.VerticalExpand && totalExpand > 0)
				extraHeight += remainingHeight * child.VerticalExpandFactor / totalExpand;
			child.LocalPosition = (
				child.HorizontalSizing switch {
					LayoutSizing.Stretch => 0,
					LayoutSizing.FitBegin => -RectTransform.LocalSize.X / 2 + child.MinimumSize.X / 2,
					LayoutSizing.FitCenter => 0,
					LayoutSizing.FitEnd => +RectTransform.LocalSize.X / 2 - child.MinimumSize.X / 2,
					_ => throw new IndexOutOfRangeException()
				},
				y + directionModifier * child.VerticalSizing switch {
					LayoutSizing.Stretch => minimumHeight / 2 + extraHeight / 2,
					LayoutSizing.FitBegin => minimumHeight / 2,
					LayoutSizing.FitCenter => minimumHeight / 2 + extraHeight / 2,
					LayoutSizing.FitEnd => minimumHeight / 2 + extraHeight,
					_ => throw new IndexOutOfRangeException()
				});
			if (child.VerticalSizing == LayoutSizing.Stretch)
				child.LocalSize = (child.LocalSize.X, minimumHeight + extraHeight);
			y += directionModifier * (minimumHeight + extraHeight);
		}
	}
}

public enum VerticalLayoutDirection
{
	TopToBottom,
	BottomToTop
}
using Echidna2.Core;
using Echidna2.Serialization;

namespace Echidna2.Gui;

public class VerticalLayout : RectLayout
{
	[SerializedValue] public double Spacing = 0;
	
	[SerializedValue] public VerticalLayoutDirection LayoutDirection = VerticalLayoutDirection.TopToBottom;
	
	public override void OnPreNotify(IUpdate.Notification notification)
	{
		List<RectTransform> laidOutChildren = ChildrenThatCanBeLaidOut.Select(child => child.RectTransform).ToList();
		
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
				laidOutChildren.Max(child => child.MinimumSizeToParent.X),
				laidOutChildren.Sum(child => child.MinimumSizeToParent.Y) + Spacing * (laidOutChildren.Count - 1));
		else
			RectTransform.MinimumSize = (0, 0);
		
		double totalExpandHeight = RectTransform.LocalSize.Y - RectTransform.MinimumSize.Y;
		double totalExpandWeight = laidOutChildren.Where(child => child.VerticalExpand).Sum(child => child.VerticalExpandWeight);
		
		double directionModifier = LayoutDirection switch
		{
			VerticalLayoutDirection.TopToBottom => -1,
			VerticalLayoutDirection.BottomToTop => +1,
			_ => throw new IndexOutOfRangeException()
		};
		double y = -directionModifier * RectTransform.LocalSize.Y / 2;
		foreach (RectTransform child in laidOutChildren)
		{
			double minimumHeight = child.MinimumSizeToParent.Y;
			double expandHeight = 0;
			if (child.VerticalExpand && totalExpandWeight > 0)
				expandHeight += totalExpandHeight * child.VerticalExpandWeight / totalExpandWeight;
			child.LocalPosition = (
				child.HorizontalSizing switch {
					LayoutSizing.Stretch => 0,
					LayoutSizing.FitBegin => -RectTransform.LocalSize.X / 2 + child.MinimumSizeToParent.X / 2,
					LayoutSizing.FitCenter => 0,
					LayoutSizing.FitEnd => +RectTransform.LocalSize.X / 2 - child.MinimumSizeToParent.X / 2,
					_ => throw new IndexOutOfRangeException()
				},
				y + directionModifier * child.VerticalSizing switch {
					LayoutSizing.Stretch => minimumHeight / 2 + expandHeight / 2,
					LayoutSizing.FitBegin => minimumHeight / 2,
					LayoutSizing.FitCenter => minimumHeight / 2 + expandHeight / 2,
					LayoutSizing.FitEnd => minimumHeight / 2 + expandHeight,
					_ => throw new IndexOutOfRangeException()
				});
			if (child.VerticalSizing == LayoutSizing.Stretch)
				child.LocalSize = (child.LocalSize.X, (minimumHeight + expandHeight) / child.LocalScale.Y);
			y += directionModifier * (minimumHeight + expandHeight + Spacing);
		}
	}
}

public enum VerticalLayoutDirection
{
	TopToBottom,
	BottomToTop
}
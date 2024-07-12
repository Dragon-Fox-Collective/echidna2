using Echidna2.Core;
using Echidna2.Serialization;

namespace Echidna2.Gui;

public class HorizontalLayout : RectLayout
{
	[SerializedValue] public double Spacing = 0;
	
	[SerializedValue] public HorizontalLayoutDirection LayoutDirection = HorizontalLayoutDirection.LeftToRight;
	
	public override void OnPreNotify(UpdateNotification notification)
	{
		List<RectTransform> laidOutChildren = ChildrenThatCanBeLaidOut.Select(child => child.RectTransform).ToList();
		
		foreach (RectTransform child in laidOutChildren)
		{
			child.AnchorPreset = child.VerticalSizing switch
			{
				LayoutSizing.Stretch => AnchorPreset.TallLeft,
				LayoutSizing.FitBegin => AnchorPreset.BottomLeft,
				LayoutSizing.FitCenter => AnchorPreset.CenterLeft,
				LayoutSizing.FitEnd => AnchorPreset.TopLeft,
				_ => throw new IndexOutOfRangeException()
			};
		}
		
		base.OnPreNotify(notification);
		
		if (laidOutChildren.Count != 0)
			RectTransform.MinimumSize = (
				laidOutChildren.Sum(child => child.MinimumSizeToParent.X) + Spacing * (laidOutChildren.Count - 1),
				laidOutChildren.Max(child => child.MinimumSizeToParent.Y));
		else
			RectTransform.MinimumSize = (0, 0);
		
		double totalExpandWidth = RectTransform.LocalSize.X - RectTransform.MinimumSize.X;
		double totalExpandWeight = laidOutChildren.Where(child => child.HorizontalExpand).Sum(child => child.HorizontalExpandWeight);
		
		double directionModifier = LayoutDirection switch
		{
			HorizontalLayoutDirection.LeftToRight => +1,
			HorizontalLayoutDirection.RightToLeft => -1,
			_ => throw new IndexOutOfRangeException()
		};
		double x = -directionModifier * RectTransform.LocalSize.X / 2;
		foreach (RectTransform child in laidOutChildren)
		{
			double minimumWidth = child.MinimumSizeToParent.X;
			double expandWidth = 0;
			if (child.HorizontalExpand && totalExpandWeight > 0)
				expandWidth += totalExpandWidth * child.HorizontalExpandWeight / totalExpandWeight;
			child.LocalPosition = (
				x + directionModifier * child.HorizontalSizing switch {
					LayoutSizing.Stretch => minimumWidth / 2 + expandWidth / 2,
					LayoutSizing.FitBegin => minimumWidth / 2,
					LayoutSizing.FitCenter => minimumWidth / 2 + expandWidth / 2,
					LayoutSizing.FitEnd => minimumWidth / 2 + expandWidth,
					_ => throw new IndexOutOfRangeException()
				},
				child.VerticalSizing switch {
					LayoutSizing.Stretch => 0,
					LayoutSizing.FitBegin => -RectTransform.LocalSize.Y / 2 + child.MinimumSizeToParent.Y / 2,
					LayoutSizing.FitCenter => 0,
					LayoutSizing.FitEnd => +RectTransform.LocalSize.Y / 2 - child.MinimumSizeToParent.Y / 2,
					_ => throw new IndexOutOfRangeException()
				});
			if (child.HorizontalSizing == LayoutSizing.Stretch)
				child.LocalSize = ((minimumWidth + expandWidth) / child.LocalScale.X, child.LocalSize.Y);
			x += directionModifier * (minimumWidth + expandWidth + Spacing);
		}
	}
}

public enum HorizontalLayoutDirection
{
	LeftToRight,
	RightToLeft
}
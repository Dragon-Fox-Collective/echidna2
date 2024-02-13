using Echidna2.Core;

namespace Echidna2.Gui;

public class HorizontalLayout(RectTransform rectTransform, Hierarchy hierarchy) : RectLayout(rectTransform, hierarchy)
{
	public override void OnPreNotify(IUpdate.Notification notification)
	{
		List<RectTransform> laidOutChildren = Hierarchy.Children.OfType<ICanBeLaidOut>().Select(child => child.RectTransform).ToList();
		
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
				laidOutChildren.Sum(child => child.MinimumSize.X),
				laidOutChildren.Max(child => child.MinimumSize.Y));
		else
			RectTransform.MinimumSize = (0, 0);
		
		double remainingWidth = RectTransform.Size.X - RectTransform.MinimumSize.X;
		double totalExpand = laidOutChildren.Where(child => child.HorizontalExpand).Sum(child => child.HorizontalExpandFactor);
		
		double x = -RectTransform.Size.X / 2;
		foreach (RectTransform child in laidOutChildren)
		{
			double minimumWidth = child.MinimumSize.X;
			double extraWidth = 0;
			if (child.HorizontalExpand && totalExpand > 0)
				extraWidth += remainingWidth * child.HorizontalExpandFactor / totalExpand;
			child.Position = (
				child.HorizontalSizing switch {
					LayoutSizing.Stretch => x + minimumWidth / 2 + extraWidth / 2,
					LayoutSizing.FitBegin => x + minimumWidth / 2,
					LayoutSizing.FitCenter => x + minimumWidth / 2 + extraWidth / 2,
					LayoutSizing.FitEnd => x + minimumWidth / 2 + extraWidth,
					_ => throw new IndexOutOfRangeException()
				},
				child.VerticalSizing switch {
					LayoutSizing.Stretch => 0,
					LayoutSizing.FitBegin => -RectTransform.Size.Y / 2 + child.MinimumSize.Y / 2,
					LayoutSizing.FitCenter => 0,
					LayoutSizing.FitEnd => +RectTransform.Size.Y / 2 - child.MinimumSize.Y / 2,
					_ => throw new IndexOutOfRangeException()
				});
			if (child.HorizontalSizing == LayoutSizing.Stretch)
				child.Size = (minimumWidth + extraWidth, child.Size.Y);
			x += minimumWidth + extraWidth;
		}
	}
}
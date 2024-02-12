using Echidna2.Core;

namespace Echidna2.Gui;

public class HorizontalLayout(RectTransform rectTransform, Hierarchy hierarchy)
{
	public void OnPreNotify(IUpdate.Notification notification)
	{
		foreach (RectTransform child in hierarchy.GetChildren().OfType<RectTransform>())
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
		
		// FIXME
		// rectTransform.OnPreNotify(notification);
		
		if (hierarchy.GetChildren().OfType<RectTransform>().Any())
			rectTransform.MinimumSize = (
				hierarchy.GetChildren().OfType<RectTransform>().Sum(child => child.MinimumSize.X),
				hierarchy.GetChildren().OfType<RectTransform>().Max(child => child.MinimumSize.Y));
		else
			rectTransform.MinimumSize = (0, 0);
		
		double remainingWidth = rectTransform.Size.X - rectTransform.MinimumSize.X;
		double totalExpand = hierarchy.GetChildren().OfType<RectTransform>().Where(child => child.HorizontalExpand).Sum(child => child.HorizontalExpandFactor);
		
		double x = -rectTransform.Size.X / 2;
		foreach (RectTransform child in hierarchy.GetChildren().OfType<RectTransform>())
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
					LayoutSizing.FitBegin => -rectTransform.Size.Y / 2 + child.MinimumSize.Y / 2,
					LayoutSizing.FitCenter => 0,
					LayoutSizing.FitEnd => +rectTransform.Size.Y / 2 - child.MinimumSize.Y / 2,
					_ => throw new IndexOutOfRangeException()
				});
			if (child.HorizontalSizing == LayoutSizing.Stretch)
				child.Size = (minimumWidth + extraWidth, child.Size.Y);
			x += minimumWidth + extraWidth;
		}
	}
}
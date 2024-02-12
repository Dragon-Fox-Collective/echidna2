using Echidna2.Core;

namespace Echidna2.Gui;

public class VerticalLayout(RectTransform rectTransform, Hierarchy hierarchy)
{
	public void OnPreNotify(IUpdate.Notification notification)
	{
		foreach (RectTransform child in hierarchy.GetChildren().OfType<RectTransform>())
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
		
		// FIXME
		// rectTransform.OnPreNotify(notification);
		
		if (hierarchy.GetChildren().OfType<RectTransform>().Any())
			rectTransform.MinimumSize = (
				hierarchy.GetChildren().OfType<RectTransform>().Max(child => child.MinimumSize.X),
				hierarchy.GetChildren().OfType<RectTransform>().Sum(child => child.MinimumSize.Y));
		else
			rectTransform.MinimumSize = (0, 0);
		
		double remainingHeight = rectTransform.Size.Y - rectTransform.MinimumSize.Y;
		double totalExpand = hierarchy.GetChildren().OfType<RectTransform>().Where(child => child.VerticalExpand).Sum(child => child.VerticalExpandFactor);
		
		double y = -rectTransform.Size.Y / 2;
		foreach (RectTransform child in hierarchy.GetChildren().OfType<RectTransform>())
		{
			double minimumHeight = child.MinimumSize.Y;
			double extraHeight = 0;
			if (child.VerticalExpand && totalExpand > 0)
				extraHeight += remainingHeight * child.VerticalExpandFactor / totalExpand;
			child.Position = (
				child.HorizontalSizing switch {
					LayoutSizing.Stretch => 0,
					LayoutSizing.FitBegin => -rectTransform.Size.X / 2 + child.MinimumSize.X / 2,
					LayoutSizing.FitCenter => 0,
					LayoutSizing.FitEnd => +rectTransform.Size.X / 2 - child.MinimumSize.X / 2,
					_ => throw new IndexOutOfRangeException()
				},
				child.VerticalSizing switch {
					LayoutSizing.Stretch => y + minimumHeight / 2 + extraHeight / 2,
					LayoutSizing.FitBegin => y + minimumHeight / 2,
					LayoutSizing.FitCenter => y + minimumHeight / 2 + extraHeight / 2,
					LayoutSizing.FitEnd => y + minimumHeight / 2 + extraHeight,
					_ => throw new IndexOutOfRangeException()
				});
			if (child.VerticalSizing == LayoutSizing.Stretch)
				child.Size = (child.Size.X, minimumHeight + extraHeight);
			y += minimumHeight + extraHeight;
		}
	}
}
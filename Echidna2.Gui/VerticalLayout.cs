using Echidna2.Core;

namespace Echidna2.Gui;

public partial class VerticalLayout(
	[Component] IRectTransform? rectTransform = null)
{
	public void OnPreNotify(IUpdate.Notification notification)
	{
		foreach (IRectTransform child in GetChildren().OfType<IRectTransform>())
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
		
		rectTransform.OnPreNotify(notification);
		
		if (GetChildren().OfType<IRectTransform>().Any())
			MinimumSize = (
				GetChildren().OfType<IRectTransform>().Max(child => child.MinimumSize.X),
				GetChildren().OfType<IRectTransform>().Sum(child => child.MinimumSize.Y));
		else
			MinimumSize = (0, 0);
		
		double remainingHeight = Size.Y - MinimumSize.Y;
		double totalExpand = GetChildren().OfType<IRectTransform>().Where(child => child.VerticalExpand).Sum(child => child.VerticalExpandFactor);
		
		double y = -Size.Y / 2;
		foreach (IRectTransform child in GetChildren().OfType<IRectTransform>())
		{
			double minimumHeight = child.MinimumSize.Y;
			double extraHeight = 0;
			if (child.VerticalExpand && totalExpand > 0)
				extraHeight += remainingHeight * child.VerticalExpandFactor / totalExpand;
			child.Position = (
				child.HorizontalSizing switch {
					LayoutSizing.Stretch => 0,
					LayoutSizing.FitBegin => -Size.X / 2 + child.MinimumSize.X / 2,
					LayoutSizing.FitCenter => 0,
					LayoutSizing.FitEnd => +Size.X / 2 - child.MinimumSize.X / 2,
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
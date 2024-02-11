namespace Echidna2.Core;

public partial class HorizontalLayout(
	[Component] IRectTransform? rectTransform = null)
{
	public void OnPreNotify(IUpdate.Notification notification)
	{
		foreach (IRectTransform child in rectTransform.GetChildren().OfType<IRectTransform>())
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
		
		rectTransform.OnPreNotify(notification);
		
		MinimumSize = (
			rectTransform.GetChildren().OfType<IRectTransform>().Sum(child => child.MinimumSize.X),
			rectTransform.GetChildren().OfType<IRectTransform>().Max(child => child.MinimumSize.Y));
		
		double remainingWidth = Size.X - MinimumSize.X;
		double totalExpand = rectTransform.GetChildren().OfType<IRectTransform>().Where(child => child.HorizontalExpand).Sum(child => child.HorizontalExpandFactor);
		
		double x = -Size.X / 2;
		foreach (IRectTransform child in rectTransform.GetChildren().OfType<IRectTransform>())
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
					LayoutSizing.FitBegin => -Size.Y / 2 + child.MinimumSize.Y / 2,
					LayoutSizing.FitCenter => 0,
					LayoutSizing.FitEnd => +Size.Y / 2 - child.MinimumSize.Y / 2,
					_ => throw new IndexOutOfRangeException()
				});
			if (child.HorizontalSizing == LayoutSizing.Stretch)
				child.Size = (minimumWidth + extraWidth, child.Size.Y);
			x += minimumWidth + extraWidth;
		}
	}
}
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
				LayoutSizing.Expand => AnchorPreset.TallLeft,
				_ => throw new IndexOutOfRangeException()
			};
		}
		
		rectTransform.OnPreNotify(notification);
		
		double x = -Size.X / 2;
		foreach (IRectTransform child in rectTransform.GetChildren().OfType<IRectTransform>())
		{
			child.Position = (
				x + child.MinimumSize.X / 2,
				child.VerticalSizing switch {
					LayoutSizing.Stretch => 0,
					LayoutSizing.FitBegin => -Size.Y / 2 + child.MinimumSize.Y / 2,
					LayoutSizing.FitCenter => 0,
					LayoutSizing.FitEnd => +Size.Y / 2 - child.MinimumSize.Y / 2,
					LayoutSizing.Expand => 0,
					_ => throw new IndexOutOfRangeException()
				});
			x += child.MinimumSize.X;
		}
	}
}
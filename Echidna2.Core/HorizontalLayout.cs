namespace Echidna2.Core;

public partial class HorizontalLayout(
	[Component] IRectTransform? rectTransform = null)
{
	public void OnPreNotify(IUpdate.Notification notification)
	{
		foreach (IRectTransform child in rectTransform.GetChildren().OfType<IRectTransform>())
		{
			child.AnchorPreset = AnchorPreset.TallLeft;
		}
		
		rectTransform.OnPreNotify(notification);
		
		double x = -Size.X / 2;
		foreach (IRectTransform child in rectTransform.GetChildren().OfType<IRectTransform>())
		{
			child.Position = child.Position with { X = x + child.MinimumSize.X / 2 };
			x += child.MinimumSize.X;
		}
	}
}
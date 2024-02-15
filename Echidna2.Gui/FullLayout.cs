using Echidna2.Core;
using Echidna2.Mathematics;

namespace Echidna2.Gui;

public class FullLayout(RectTransform rectTransform, Hierarchy hierarchy) : RectLayout(rectTransform, hierarchy)
{
	public double LeftMargin = 0;
	public double RightMargin = 0;
	public double BottomMargin = 0;
	public double TopMargin = 0;
	
	public double Margin
	{
		set => LeftMargin = RightMargin = BottomMargin = TopMargin = value;
	}
	
	public double HorizontalMargin
	{
		set => LeftMargin = RightMargin = value;
	}
	
	public double VerticalMargin
	{
		set => BottomMargin = TopMargin = value;
	}
	
	public override void OnPreNotify(IUpdate.Notification notification)
	{
		List<RectTransform> laidOutChildren = Hierarchy.Children.OfType<ICanBeLaidOut>().Select(child => child.RectTransform).ToList();
		
		foreach (RectTransform child in laidOutChildren)
			child.AnchorPreset = AnchorPreset.Full;
		
		Vector2 exteriorSize = RectTransform.LocalSize;
		Vector2 interiorSize = (Math.Max(exteriorSize.X - LeftMargin - RightMargin, 0), Math.Max(exteriorSize.Y - BottomMargin - TopMargin, 0));
		RectTransform.LocalSize = exteriorSize;
		
		base.OnPreNotify(notification);
		
		RectTransform.LocalSize = exteriorSize;
		
		if (laidOutChildren.Count != 0)
			RectTransform.MinimumSize = (
				laidOutChildren.Max(child => child.MinimumSize.X) + LeftMargin + RightMargin,
				laidOutChildren.Max(child => child.MinimumSize.Y) + BottomMargin + TopMargin);
		else
			RectTransform.MinimumSize = (0, 0);
		
		foreach (RectTransform child in laidOutChildren)
		{
			child.LocalPosition = (LeftMargin - RightMargin, BottomMargin - TopMargin);
			child.LocalSize = interiorSize;
		}
	}
}
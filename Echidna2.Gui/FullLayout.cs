using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Serialization;

namespace Echidna2.Gui;

public class FullLayout : RectLayout
{
	[SerializedValue] public double LeftMargin = 0;
	[SerializedValue] public double RightMargin = 0;
	[SerializedValue] public double BottomMargin = 0;
	[SerializedValue] public double TopMargin = 0;
	
	[SerializedValue] public double Margin
	{
		set => LeftMargin = RightMargin = BottomMargin = TopMargin = value;
	}
	
	[SerializedValue] public double HorizontalMargin
	{
		set => LeftMargin = RightMargin = value;
	}
	
	[SerializedValue] public double VerticalMargin
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
		RectTransform.LocalSize = interiorSize;
		
		base.OnPreNotify(notification);
		
		RectTransform.LocalSize = exteriorSize;
		
		if (laidOutChildren.Count != 0)
			RectTransform.MinimumSize = (
				laidOutChildren.Max(child => child.MinimumSize.X) + LeftMargin + RightMargin,
				laidOutChildren.Max(child => child.MinimumSize.Y) + BottomMargin + TopMargin);
		else
			RectTransform.MinimumSize = (LeftMargin + RightMargin, BottomMargin + TopMargin);
		
		foreach (RectTransform child in laidOutChildren)
		{
			child.LocalPosition = (LeftMargin - RightMargin, BottomMargin - TopMargin);
			child.LocalSize = interiorSize;
		}
	}
}
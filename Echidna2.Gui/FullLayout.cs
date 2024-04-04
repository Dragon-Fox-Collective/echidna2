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
	
	private double margin = 0;
	[SerializedValue] public double Margin
	{
		get => margin;
		set => margin = LeftMargin = RightMargin = BottomMargin = TopMargin = value;
	}
	
	private double horizontalMargin = 0;
	[SerializedValue] public double HorizontalMargin
	{
		get => horizontalMargin;
		set => horizontalMargin = LeftMargin = RightMargin = value;
	}
	
	private double verticalMargin = 0;
	[SerializedValue] public double VerticalMargin
	{
		get => verticalMargin;
		set => verticalMargin = BottomMargin = TopMargin = value;
	}
	
	public override void OnPreNotify(IUpdate.Notification notification)
	{
		List<RectTransform> laidOutChildren = ChildrenThatCanBeLaidOut.Select(child => child.RectTransform).ToList();
		
		foreach (RectTransform child in laidOutChildren)
			child.AnchorPreset = AnchorPreset.Full;
		
		Vector2 exteriorSize = RectTransform.LocalSize;
		Vector2 interiorSize = (Math.Max(exteriorSize.X - LeftMargin - RightMargin, 0), Math.Max(exteriorSize.Y - BottomMargin - TopMargin, 0));
		RectTransform.LocalSize = interiorSize;
		
		base.OnPreNotify(notification);
		
		RectTransform.LocalSize = exteriorSize;
		
		if (laidOutChildren.Count != 0)
			RectTransform.MinimumSize = (
				Math.Max(RectTransform.MinimumSize.X, laidOutChildren.Max(child => child.MinimumSize.X) + LeftMargin + RightMargin),
				Math.Max(RectTransform.MinimumSize.Y, laidOutChildren.Max(child => child.MinimumSize.Y) + BottomMargin + TopMargin));
		else
			RectTransform.MinimumSize = (
				Math.Max(RectTransform.MinimumSize.X, LeftMargin + RightMargin),
				Math.Max(RectTransform.MinimumSize.Y, BottomMargin + TopMargin));
		
		foreach (RectTransform child in laidOutChildren)
		{
			child.LocalPosition = (LeftMargin - RightMargin, BottomMargin - TopMargin);
			child.LocalSize = interiorSize;
		}
	}
}
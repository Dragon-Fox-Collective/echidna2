using Echidna2.Core;

namespace Echidna2.Gui;

public class VerticalLayout(RectTransform rectTransform, Hierarchy hierarchy) : RectLayout(rectTransform, hierarchy)
{
	public override void OnPreNotify(IUpdate.Notification notification)
	{
		List<RectTransform> laidOutChildren = Hierarchy.Children.OfType<ICanBeLaidOut>().Select(child => child.RectTransform).ToList();
		
		foreach (RectTransform child in laidOutChildren)
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
		
		base.OnPreNotify(notification);
		
		if (laidOutChildren.Count != 0)
			RectTransform.MinimumSize = (
				laidOutChildren.Max(child => child.MinimumSize.X),
				laidOutChildren.Sum(child => child.MinimumSize.Y));
		else
			RectTransform.MinimumSize = (0, 0);
		
		double remainingHeight = RectTransform.Size.Y - RectTransform.MinimumSize.Y;
		double totalExpand = laidOutChildren.Where(child => child.VerticalExpand).Sum(child => child.VerticalExpandFactor);
		
		Console.WriteLine($"{remainingHeight} {totalExpand}");
		
		double y = -RectTransform.Size.Y / 2;
		foreach (RectTransform child in laidOutChildren)
		{
			double minimumHeight = child.MinimumSize.Y;
			double extraHeight = 0;
			if (child.VerticalExpand && totalExpand > 0)
				extraHeight += remainingHeight * child.VerticalExpandFactor / totalExpand;
			child.Position = (
				child.HorizontalSizing switch {
					LayoutSizing.Stretch => 0,
					LayoutSizing.FitBegin => -RectTransform.Size.X / 2 + child.MinimumSize.X / 2,
					LayoutSizing.FitCenter => 0,
					LayoutSizing.FitEnd => +RectTransform.Size.X / 2 - child.MinimumSize.X / 2,
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
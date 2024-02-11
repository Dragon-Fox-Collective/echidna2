using Echidna.Mathematics;

namespace Echidna2.Core;

[ComponentImplementation<RectTransform>]
public interface IRectTransform : IHierarchy, INotificationHook<IUpdate.Notification>
{
	public Vector2 Position { get; set; }
	public Vector2 Size { get; set; }
	
	public Matrix4 LocalTransform { get; set; }
	public Matrix4 GlobalTransform { get; set; }
	
	public int Depth { get; set; }
	
	public Vector2 MinimumSize { get; set; }
	
	public AnchorPreset AnchorPreset { get; set; }
	
	public double AnchorLeft { get; set; }
	public double AnchorRight { get; set; }
	public double AnchorBottom { get; set; }
	public double AnchorTop { get; set; }
	
	public double AnchorOffsetLeft { get; set; }
	public double AnchorOffsetRight { get; set; }
	public double AnchorOffsetBottom { get; set; }
	public double AnchorOffsetTop { get; set; }
}

[Flags]
public enum AnchorPreset
{
	LeftHalf = 1 << 1,
	LeftOne = 1 << 2,
	RightHalf = 1 << 3,
	RightOne = 1 << 4,
	BottomHalf = 1 << 5,
	BottomOne = 1 << 6,
	TopHalf = 1 << 7,
	TopOne = 1 << 8,
	
	BottomLeft = 0,
	BottomCenter = LeftHalf | RightHalf,
	BottomRight = LeftOne | RightOne,
	CenterLeft = BottomHalf | TopHalf,
	Center = LeftHalf | RightHalf | BottomHalf | TopHalf,
	CenterRight = LeftOne | RightOne | BottomHalf | TopHalf,
	TopLeft = BottomOne | TopOne,
	TopCenter = LeftHalf | RightHalf | BottomOne | TopOne,
	TopRight = LeftOne | RightOne | BottomOne | TopOne,
	
	TallLeft = TopOne,
	TallCenter = LeftHalf | RightHalf | TopOne,
	TallRight = LeftOne | RightOne | TopOne,
	
	WideBottom = RightOne,
	WideCenter = TopHalf | BottomHalf | RightOne,
	WideTop = TopOne | BottomOne | RightOne,
	
	Full = RightOne | TopOne,
}

public partial class RectTransform(
	[Component] IHierarchy? hierarchy = null)
	: IRectTransform
{
	public Vector2 Position { get; set; }
	public Vector2 Size { get; set; }
	
	public Matrix4 LocalTransform { get; set; } = Matrix4.Identity;
	public Matrix4 GlobalTransform { get; set; } = Matrix4.Identity;
	
	public int Depth { get; set; }
	
	public Vector2 MinimumSize { get; set; }
	
	private AnchorPreset anchorPreset;
	public AnchorPreset AnchorPreset
	{
		get => anchorPreset;
		set
		{
			anchorPreset = value;
			
			if (anchorPreset.HasFlag(AnchorPreset.LeftHalf))
				AnchorLeft = 0.5;
			else if (anchorPreset.HasFlag(AnchorPreset.LeftOne))
				AnchorLeft = 1;
			else
				AnchorLeft = 0;
			
			if (anchorPreset.HasFlag(AnchorPreset.RightHalf))
				AnchorRight = 0.5;
			else if (anchorPreset.HasFlag(AnchorPreset.RightOne))
				AnchorRight = 1;
			else
				AnchorRight = 0;
			
			if (anchorPreset.HasFlag(AnchorPreset.BottomHalf))
				AnchorBottom = 0.5;
			else if (anchorPreset.HasFlag(AnchorPreset.BottomOne))
				AnchorBottom = 1;
			else
				AnchorBottom = 0;
			
			if (anchorPreset.HasFlag(AnchorPreset.TopHalf))
				AnchorTop = 0.5;
			else if (anchorPreset.HasFlag(AnchorPreset.TopOne))
				AnchorTop = 1;
			else
				AnchorTop = 0;
		}
	}
	
	public double AnchorLeft { get; set; }
	public double AnchorRight { get; set; }
	public double AnchorBottom { get; set; }
	public double AnchorTop { get; set; }
	
	public double AnchorOffsetLeft { get; set; }
	public double AnchorOffsetRight { get; set; }
	public double AnchorOffsetBottom { get; set; }
	public double AnchorOffsetTop { get; set; }
	
	public void OnPreNotify(IUpdate.Notification notification)
	{
		foreach (IRectTransform child in GetChildren().OfType<IRectTransform>())
		{
			double left = Size.X * (child.AnchorLeft - 0.5) + child.AnchorOffsetLeft;
			double right = Size.X * (child.AnchorRight - 0.5) + child.AnchorOffsetRight;
			double bottom = Size.Y * (child.AnchorBottom - 0.5) + child.AnchorOffsetBottom;
			double top = Size.Y * (child.AnchorTop - 0.5) + child.AnchorOffsetTop;
			child.Size = new Vector2(Math.Max(right - left, child.MinimumSize.X), Math.Max(top - bottom, child.MinimumSize.Y));
			child.Position = new Vector2(left + right, bottom + top) / 2;
			child.Depth = Depth + 1;
			child.LocalTransform = Matrix4.Translation(child.Position.WithZ(child.Depth));
			child.GlobalTransform = GlobalTransform * child.LocalTransform;
			// FIXME: Root doesn't get its transforms updated. A use for an event?
			
			// Console.WriteLine($"{child.AnchorPreset} {left} {right} {bottom} {top} {child.Size} {child.Position} {child.Depth}");
		}
	}
	public void OnPostNotify(IUpdate.Notification notification)
	{
		
	}
	public void OnPostPropagate(IUpdate.Notification notification)
	{
		
	}
}
using Echidna2.Mathematics;

namespace Echidna2.Gui;

public class RectTransform
{
	public delegate void LocalTransformChangedHandler();
	public event LocalTransformChangedHandler? LocalTransformChanged;
	
	private Vector2 position;
	public Vector2 Position
	{
		get => position;
		set
		{
			position = value;
			RecalculateLocalTransform();
		}
	}
	public Vector2 Size { get; set; }
	
	private Matrix4 localTransform = Matrix4.Identity;
	public Matrix4 LocalTransform
	{
		get => localTransform;
		set
		{
			localTransform = value;
			LocalTransformChanged?.Invoke();
			if (IsGlobal)
				GlobalTransform = localTransform;
		}
	}
	public Matrix4 GlobalTransform { get; set; } = Matrix4.Identity;
	
	private int depth;
	public int Depth
	{
		get => depth;
		set
		{
			depth = value;
			RecalculateLocalTransform();
		}
	}
	
	public Vector2 MinimumSize { get; set; }
	
	private AnchorPreset anchorPreset;
	public AnchorPreset AnchorPreset
	{
		get => anchorPreset;
		set
		{
			anchorPreset = value;
			
			if (anchorPreset.HasFlagFast(AnchorPreset.LeftHalf))
				AnchorLeft = 0.5;
			else if (anchorPreset.HasFlagFast(AnchorPreset.LeftOne))
				AnchorLeft = 1;
			else
				AnchorLeft = 0;
			
			if (anchorPreset.HasFlagFast(AnchorPreset.RightHalf))
				AnchorRight = 0.5;
			else if (anchorPreset.HasFlagFast(AnchorPreset.RightOne))
				AnchorRight = 1;
			else
				AnchorRight = 0;
			
			if (anchorPreset.HasFlagFast(AnchorPreset.BottomHalf))
				AnchorBottom = 0.5;
			else if (anchorPreset.HasFlagFast(AnchorPreset.BottomOne))
				AnchorBottom = 1;
			else
				AnchorBottom = 0;
			
			if (anchorPreset.HasFlagFast(AnchorPreset.TopHalf))
				AnchorTop = 0.5;
			else if (anchorPreset.HasFlagFast(AnchorPreset.TopOne))
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
	
	public LayoutSizing HorizontalSizing { get; set; }
	public bool HorizontalExpand { get; set; }
	public double HorizontalExpandFactor { get; set; } = 1;
	public LayoutSizing VerticalSizing { get; set; }
	public bool VerticalExpand { get; set; }
	public double VerticalExpandFactor { get; set; } = 1;
	
	public bool IsGlobal { get; set; } = false;
	
	private void RecalculateLocalTransform()
	{
		LocalTransform = Matrix4.Translation(Position.WithZ(Depth));
	}
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

public static class AnchorPresetExtensions
{
	public static bool HasFlagFast(this AnchorPreset anchorPreset, AnchorPreset flag) => (anchorPreset & flag) == flag;
}

public enum LayoutSizing
{
	Stretch,
	FitBegin,
	FitBottom = FitBegin,
	FitLeft = FitBegin,
	FitCenter,
	FitEnd,
	FitTop = FitEnd,
	FitRight = FitEnd,
}
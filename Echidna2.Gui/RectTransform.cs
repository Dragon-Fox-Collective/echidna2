using Echidna2.Mathematics;
using Echidna2.Serialization;

namespace Echidna2.Gui;

public class RectTransform
{
	public delegate void LocalTransformChangedHandler();
	public event LocalTransformChangedHandler? LocalTransformChanged;
	
	private Vector2 localPosition;
	[SerializedValue] public Vector2 LocalPosition
	{
		get => localPosition;
		set
		{
			localPosition = value;
			RecalculateLocalTransform();
		}
	}
	public Vector2 GlobalPosition => GlobalTransform.Translation.XY;
	
	[SerializedValue] public Vector2 LocalSize { get; set; }
	public Vector2 GlobalSize => GlobalTransform.InverseTransformDirection(LocalSize);
	
	private Vector2 localScale = Vector2.One;
	[SerializedValue] public Vector2 LocalScale
	{
		get => localScale;
		set
		{
			localScale = value;
			RecalculateLocalTransform();
		}
	}
	
	private Matrix4 localTransform = Matrix4.Identity;
	[SerializedValue] public Matrix4 LocalTransform
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
	[SerializedValue] public int Depth
	{
		get => depth;
		set
		{
			depth = value;
			RecalculateLocalTransform();
		}
	}
	
	[SerializedValue] public Vector2 MinimumSize { get; set; }
	
	private AnchorPreset anchorPreset;
	[SerializedValue] public AnchorPreset AnchorPreset
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
	
	[SerializedValue] public double AnchorLeft { get; set; }
	[SerializedValue] public double AnchorRight { get; set; }
	[SerializedValue] public double AnchorBottom { get; set; }
	[SerializedValue] public double AnchorTop { get; set; }
	
	[SerializedValue] public double AnchorOffsetLeft { get; set; }
	[SerializedValue] public double AnchorOffsetRight { get; set; }
	[SerializedValue] public double AnchorOffsetBottom { get; set; }
	[SerializedValue] public double AnchorOffsetTop { get; set; }
	
	[SerializedValue] public LayoutSizing HorizontalSizing { get; set; }
	[SerializedValue] public bool HorizontalExpand { get; set; }
	[SerializedValue] public double HorizontalExpandFactor { get; set; } = 1;
	[SerializedValue] public LayoutSizing VerticalSizing { get; set; }
	[SerializedValue] public bool VerticalExpand { get; set; }
	[SerializedValue] public double VerticalExpandFactor { get; set; } = 1;
	
	[SerializedValue] public bool IsGlobal { get; set; } = false;
	
	private void RecalculateLocalTransform()
	{
		LocalTransform = Matrix4.FromTranslation(LocalPosition.WithZ(Depth)) * Matrix4.FromScale(LocalScale.WithZ(1));
	}
	
	public bool ContainsGlobalPoint(Vector2 point) => ContainsLocalPoint(GlobalTransform.InverseTransformPoint(point));
	public bool ContainsLocalPoint(Vector2 point) => point.X >= -LocalSize.X / 2 && point.X <= +LocalSize.X / 2 && point.Y >= -LocalSize.Y / 2 && point.Y <= +LocalSize.Y / 2;
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
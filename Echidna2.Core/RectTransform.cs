using Echidna.Mathematics;

namespace Echidna2.Core;

[ComponentImplementation<RectTransform>]
public interface IRectTransform : IHierarchy, INotificationHook<IUpdate.Notification>
{
	public delegate void LocalTransformChangedHandler();
	public event LocalTransformChangedHandler? LocalTransformChanged;
	
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
	
	public LayoutSizing HorizontalSizing { get; set; }
	public bool HorizontalExpand { get; set; }
	public double HorizontalExpandFactor { get; set; }
	public LayoutSizing VerticalSizing { get; set; }
	public bool VerticalExpand { get; set; }
	public double VerticalExpandFactor { get; set; }
	
	public bool IsGlobal { get; set; }
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

public partial class RectTransform : IRectTransform
{
	public event IRectTransform.LocalTransformChangedHandler? LocalTransformChanged;
	
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
	
	public LayoutSizing HorizontalSizing { get; set; }
	public bool HorizontalExpand { get; set; }
	public double HorizontalExpandFactor { get; set; } = 1;
	public LayoutSizing VerticalSizing { get; set; }
	public bool VerticalExpand { get; set; }
	public double VerticalExpandFactor { get; set; } = 1;
	
	public bool IsGlobal { get; set; } = false;
	
	private List<(IRectTransform child, IRectTransform.LocalTransformChangedHandler handler)> localTransformChangedHandlers = [];
	
	public RectTransform(
		[Component] IHierarchy? hierarchy = null)
	{
		this.hierarchy = hierarchy ?? new Hierarchy();
		ChildAdded += (child) =>
		{
			if (child is IRectTransform rectTransform)
			{
				IRectTransform.LocalTransformChangedHandler handler = () => rectTransform.GlobalTransform = GlobalTransform * rectTransform.LocalTransform;
				localTransformChangedHandlers.Add((rectTransform, handler));
				rectTransform.LocalTransformChanged += handler;
			}
		};
		ChildRemoved += (child) =>
		{
			if (child is IRectTransform rectTransform)
			{
				(IRectTransform child, IRectTransform.LocalTransformChangedHandler handler) tuple = localTransformChangedHandlers.First(tuple => tuple.child == rectTransform);
				rectTransform.LocalTransformChanged -= tuple.handler;
				localTransformChangedHandlers.Remove(tuple);
			}
		};
	}
	
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
			
			// Console.WriteLine($"{child.AnchorPreset} {left} {right} {bottom} {top} {child.Size} {child.Position} {child.Depth}");
		}
	}
	public void OnPostNotify(IUpdate.Notification notification)
	{
		
	}
	public void OnPostPropagate(IUpdate.Notification notification)
	{
		
	}
	
	private void RecalculateLocalTransform()
	{
		LocalTransform = Matrix4.Translation(Position.WithZ(Depth));
	}
}
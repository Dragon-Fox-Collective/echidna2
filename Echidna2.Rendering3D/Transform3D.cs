using Echidna2.Mathematics;

namespace Echidna2.Rendering3D;

public class Transform3D
{
	public delegate void LocalTransformChangedHandler();
	public event LocalTransformChangedHandler? LocalTransformChanged;
	
	private Vector3 localPosition = Vector3.Zero;
	public Vector3 LocalPosition
	{
		get => localPosition;
		set
		{
			localPosition = value;
			RecalculateLocalTransform();
		}
	}
	public Vector3 GlobalPosition => GlobalTransform.TransformPoint(localPosition);
	
	private Quaternion localRotation = Quaternion.Identity;
	public Quaternion LocalRotation
	{
		get => localRotation;
		set
		{
			localRotation = value;
			RecalculateLocalTransform();
		}
	}
	
	private Vector3 localScale = Vector3.One;
	public Vector3 LocalScale
	{
		get => localScale;
		set
		{
			localScale = value;
			RecalculateLocalTransform();
		}
	}
	
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
	
	public bool IsGlobal { get; set; } = false;
	
	private void RecalculateLocalTransform()
	{
		LocalTransform = Matrix4.FromTranslation(LocalPosition) * Matrix4.FromRotation(LocalRotation) * Matrix4.FromScale(LocalScale);
	}
}
using Echidna2.Mathematics;

namespace Echidna2.Rendering3D;

public class Transform3D
{
	public delegate void TransformChangedHandler();
	public event TransformChangedHandler? TransformChanged;
	
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
	public Vector3 GlobalPosition
	{
		get => GlobalTransform.Translation;
		set => LocalPosition = Parent?.GlobalTransform.InverseTransformPoint(value) ?? value;
	}
	
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
	public Quaternion GlobalRotation
	{
		get => GlobalTransform.Rotation;
		set => LocalRotation = Parent?.GlobalTransform.InverseTransformRotation(value) ?? value;
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
	
	public Matrix4 LocalTransform { get; private set; } = Matrix4.Identity;
	public Matrix4 GlobalTransform { get; private set; } = Matrix4.Identity;
	
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
	
	private Transform3D? parent;
	public Transform3D? Parent
	{
		get => parent;
		set
		{
			if (parent is not null)
				parent.TransformChanged -= RecalculateGlobalTransform;
			parent = value;
			if (parent is not null)
				parent.TransformChanged += RecalculateGlobalTransform;
		}
	}
	
	private void RecalculateLocalTransform()
	{
		LocalTransform = Matrix4.FromTranslation(LocalPosition) * Matrix4.FromRotation(LocalRotation) * Matrix4.FromScale(LocalScale);
		RecalculateGlobalTransform();
	}
	private void RecalculateGlobalTransform()
	{
		GlobalTransform = Parent?.GlobalTransform * LocalTransform ?? LocalTransform;
		TransformChanged?.Invoke();
	}
}
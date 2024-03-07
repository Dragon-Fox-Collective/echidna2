using Echidna2.Mathematics;
using Echidna2.Serialization;

namespace Echidna2.Rendering3D;

public class Transform3D
{
	public delegate void TransformChangedHandler();
	public event TransformChangedHandler? TransformChanged;
	
	private Vector3 localPosition = Vector3.Zero;
	[SerializedValue] public Vector3 LocalPosition
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
	[SerializedValue] public Quaternion LocalRotation
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
	[SerializedValue] public Vector3 LocalScale
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
	
	private Transform3D? parent;
	[SerializedReference] public Transform3D? Parent
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
	
	public void LookAt(Transform3D target, Vector3 up, Vector3 fallbackUp = default) => GlobalRotation = Quaternion.LookAt(GlobalPosition, target.GlobalPosition, up, fallbackUp);
	public void LookAt(Vector3 target, Vector3 up, Vector3 fallbackUp = default) => GlobalRotation = Quaternion.LookAt(GlobalPosition, target, up, fallbackUp);
	public void LookToward(Vector3 direction, Vector3 up, Vector3 fallbackUp = default) => GlobalRotation = Quaternion.LookToward(direction, up, fallbackUp);
	
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
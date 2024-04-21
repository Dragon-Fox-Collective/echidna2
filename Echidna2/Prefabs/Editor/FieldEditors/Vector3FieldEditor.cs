using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Prefabs.Gui;
using Echidna2.Serialization;
using JetBrains.Annotations;

namespace Echidna2.Prefabs.Editor.FieldEditors;

[UsedImplicitly, Prefab("Prefabs/Editor/FieldEditors/Vector3FieldEditor.toml")]
public partial class Vector3FieldEditor : INotificationPropagator, IFieldEditor<Vector3>
{
	[SerializedReference, ExposeMembersInClass] public HLayoutWithHierarchy Layout { get; set; } = null!;
	
	private DoubleFieldEditor? xFieldEditor;
	[SerializedReference] public DoubleFieldEditor XFieldEditor
	{
		get => xFieldEditor!;
		set
		{
			if (xFieldEditor is not null)
				xFieldEditor.ValueChanged -= UpdateX;
			
			xFieldEditor = value;
			
			if (xFieldEditor is not null)
				xFieldEditor.ValueChanged += UpdateX;
		}
	}
	
	private DoubleFieldEditor? yFieldEditor;
	[SerializedReference] public DoubleFieldEditor YFieldEditor
	{
		get => yFieldEditor!;
		set
		{
			if (yFieldEditor is not null)
				yFieldEditor.ValueChanged -= UpdateY;
			
			yFieldEditor = value;
			
			if (yFieldEditor is not null)
				yFieldEditor.ValueChanged += UpdateY;
		}
	}
	
	private DoubleFieldEditor? zFieldEditor;
	[SerializedReference] public DoubleFieldEditor ZFieldEditor
	{
		get => zFieldEditor!;
		set
		{
			if (zFieldEditor is not null)
				zFieldEditor.ValueChanged -= UpdateZ;
			
			zFieldEditor = value;
			
			if (zFieldEditor is not null)
				zFieldEditor.ValueChanged += UpdateZ;
		}
	}
	
	private Vector3 value;
	public Vector3 Value
	{
		get => value;
		set
		{
			this.value = value;
			XFieldEditor.Load(value.X);
			YFieldEditor.Load(value.Y);
			ZFieldEditor.Load(value.Z);
		}
	}
	
	public event Action<object?>? ValueChanged;
	
	public void UpdateX(object? x) => UpdateX((double)x!);
	public void UpdateX(double x)
	{
		value = value with { X = x };
		ValueChanged?.Invoke(value);
	}
	public void UpdateY(object? y) => UpdateY((double)y!);
	public void UpdateY(double y)
	{
		value = value with { Y = y };
		ValueChanged?.Invoke(value);
	}
	public void UpdateZ(object? z) => UpdateZ((double)z!);
	public void UpdateZ(double z)
	{
		value = value with { Z = z };
		ValueChanged?.Invoke(value);
	}
	
	public void Load(Vector3 value) => Value = value;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Layout);
	}
}
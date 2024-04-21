using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Prefabs.Gui;
using Echidna2.Serialization;
using JetBrains.Annotations;

namespace Echidna2.Prefabs.Editor.FieldEditors;

[UsedImplicitly, Prefab("Prefabs/Editor/FieldEditors/Vector2FieldEditor.toml")]
public partial class Vector2FieldEditor : INotificationPropagator, IFieldEditor<Vector2>
{
	[SerializedReference, ExposeMembersInClass] public HLayoutWithHierarchy Layout { get; set; } = null!;
	
	private DoubleFieldEditor? xFieldEditor;
	[SerializedReference]
	public DoubleFieldEditor XFieldEditor
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
	[SerializedReference]
	public DoubleFieldEditor YFieldEditor
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
	
	private Vector2 value;
	public Vector2 Value
	{
		get => value;
		set
		{
			this.value = value;
			XFieldEditor.Load(value.X);
			YFieldEditor.Load(value.Y);
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
	
	public void Load(Vector2 value) => Value = value;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Layout);
	}
}
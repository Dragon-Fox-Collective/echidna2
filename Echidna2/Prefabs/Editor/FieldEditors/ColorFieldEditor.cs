using System.Drawing;
using Echidna2.Core;
using Echidna2.Prefabs.Gui;
using Echidna2.Serialization;
using JetBrains.Annotations;

namespace Echidna2.Prefabs.Editor.FieldEditors;

[UsedImplicitly, Prefab("Prefabs/Editor/FieldEditors/ColorFieldEditor.toml")]
public partial class ColorFieldEditor : INotificationPropagator, IFieldEditor<Color>
{
	[SerializedReference, ExposeMembersInClass] public HLayoutWithHierarchy Layout { get; set; } = null!;
	
	private DoubleFieldEditor? rFieldEditor;
	[SerializedReference] public DoubleFieldEditor RFieldEditor
	{
		get => rFieldEditor!;
		set
		{
			if (rFieldEditor is not null)
				rFieldEditor.ValueChanged -= UpdateR;
			
			rFieldEditor = value;
			
			if (rFieldEditor is not null)
				rFieldEditor.ValueChanged += UpdateR;
		}
	}
	
	private DoubleFieldEditor? gFieldEditor;
	[SerializedReference] public DoubleFieldEditor GFieldEditor
	{
		get => gFieldEditor!;
		set
		{
			if (gFieldEditor is not null)
				gFieldEditor.ValueChanged -= UpdateG;
			
			gFieldEditor = value;
			
			if (gFieldEditor is not null)
				gFieldEditor.ValueChanged += UpdateG;
		}
	}
	
	private DoubleFieldEditor? bFieldEditor;
	[SerializedReference] public DoubleFieldEditor BFieldEditor
	{
		get => bFieldEditor!;
		set
		{
			if (bFieldEditor is not null)
				bFieldEditor.ValueChanged -= UpdateB;
			
			bFieldEditor = value;
			
			if (bFieldEditor is not null)
				bFieldEditor.ValueChanged += UpdateB;
		}
	}
	
	private DoubleFieldEditor? aFieldEditor;
	[SerializedReference] public DoubleFieldEditor AFieldEditor
	{
		get => aFieldEditor!;
		set
		{
			if (aFieldEditor is not null)
				aFieldEditor.ValueChanged -= UpdateA;
			
			aFieldEditor = value;
			
			if (aFieldEditor is not null)
				aFieldEditor.ValueChanged += UpdateA;
		}
	}
	
	private Color value;
	public Color Value
	{
		get => value;
		set
		{
			this.value = value;
			RFieldEditor.Load(value.R / 255.0);
			GFieldEditor.Load(value.G / 255.0);
			BFieldEditor.Load(value.B / 255.0);
			AFieldEditor.Load(value.A / 255.0);
		}
	}
	
	public event Action<object?>? ValueChanged;
	
	public void UpdateR(object? r) => UpdateR((double)r!);
	public void UpdateR(double r)
	{
		r = Math.Clamp(r, 0.0, 1.0);
		value = Color.FromArgb(value.A, (int)(r * 255.0), value.G, value.B);
		ValueChanged?.Invoke(value);
	}
	public void UpdateG(object? g) => UpdateG((double)g!);
	public void UpdateG(double g)
	{
		g = Math.Clamp(g, 0.0, 1.0);
		value = Color.FromArgb(value.A, value.R, (int)(g * 255.0), value.B);
		ValueChanged?.Invoke(value);
	}
	public void UpdateB(object? b) => UpdateB((double)b!);
	public void UpdateB(double b)
	{
		b = Math.Clamp(b, 0.0, 1.0);
		value = Color.FromArgb(value.A, value.R, value.G, (int)(b * 255.0));
		ValueChanged?.Invoke(value);
	}
	public void UpdateA(object? a) => UpdateA((double)a!);
	public void UpdateA(double a)
	{
		a = Math.Clamp(a, 0.0, 1.0);
		value = Color.FromArgb((int)(a * 255.0), value.R, value.G, value.B);
		ValueChanged?.Invoke(value);
	}
	
	public void Load(Color value) => Value = value;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Layout);
	}
}
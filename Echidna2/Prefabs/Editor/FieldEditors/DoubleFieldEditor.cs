using System.Globalization;
using Echidna2.Core;
using Echidna2.Serialization;
using JetBrains.Annotations;

namespace Echidna2.Prefabs.Editor.FieldEditors;

[UsedImplicitly, Prefab("Prefabs/Editor/FieldEditors/DoubleFieldEditor.toml")]
public partial class DoubleFieldEditor : INotificationPropagator, IFieldEditor<double>
{
	private StringFieldEditor? stringFieldEditor;
	[SerializedReference, ExposeMembersInClass]
	public StringFieldEditor StringFieldEditor
	{
		get => stringFieldEditor!;
		set
		{
			if (stringFieldEditor is not null)
				stringFieldEditor.ValueChanged -= UpdateValue;
			
			stringFieldEditor = value;
			
			if (stringFieldEditor is not null)
				stringFieldEditor.ValueChanged += UpdateValue;
		}
	}
	
	private double value;
	public double Value
	{
		get => value;
		set
		{
			this.value = value;
			StringFieldEditor.Load(value.ToString(CultureInfo.CurrentCulture));
		}
	}
	
	public event Action<object?>? ValueChanged;
	
	public void Load(double value) => Value = value;
	
	public void UpdateValue(object? value) => UpdateValue((string)value!);
	public void UpdateValue(string stringValue)
	{
		if (double.TryParse(stringValue, NumberStyles.Any, CultureInfo.CurrentCulture, out double result))
		{
			value = result;
			ValueChanged?.Invoke(value);
		}
		else
		{
			StringFieldEditor.Load(value.ToString(CultureInfo.CurrentCulture));
		}
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, StringFieldEditor);
	}
}
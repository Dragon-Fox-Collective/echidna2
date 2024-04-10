using System.Drawing;
using System.Globalization;
using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Rendering;
using Echidna2.Serialization;
using JetBrains.Annotations;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Echidna2;

[DontExpose]
public interface IFieldEditor
{
	public void Load(object value);
	public event Action<object>? ValueChanged;
	public static virtual IFieldEditor Instantiate() => throw new NotImplementedException();
}

[DontExpose]
public interface IFieldEditor<in T> : IFieldEditor
{
	public void Load(T value);
	void IFieldEditor.Load(object value) => Load((T)value);
}

[UsedImplicitly, Prefab("Editors/StringFieldEditor.toml")]
public partial class StringFieldEditor : INotificationPropagator, IInitialize, IFieldEditor<string>, ITextInput
{
	[SerializedReference, ExposeMembersInClass] public FullRectWithHierarchy Rect { get; set; } = null!;
	[SerializedReference] public TextRect Text { get; set; } = null!;
	[SerializedReference] public Button Button { get; set; } = null!;
	
	public event Action<object>? ValueChanged;
	
	private string value = "";
	public string Value
	{
		get => value;
		set
		{
			this.value = value;
			BufferValue = value;
			ValueChanged?.Invoke(value);
		}
	}
	
	private string bufferValue = "";
	private string BufferValue
	{
		get => bufferValue;
		set
		{
			bufferValue = value;
			Text.TextString = $"{value}";
		}
	}
	
	private int cursorPosition;
	
	public bool HasBeenInitialized { get; set; }
	
	private bool isFocused;
	public bool IsFocused
	{
		get => isFocused;
		private set
		{
			if (value == isFocused) return;
			isFocused = value;
			
			if (isFocused)
			{
				Text.Color = Color.DeepSkyBlue;
				cursorPosition = Value.Length;
			}
			else
			{
				Text.Color = Color.White;
			}
		}
	}
	
	public void OnInitialize()
	{
		Button.MouseDown += () => IsFocused = true;
		Button.MouseDownOutside += () => IsFocused = false;
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect, Button);
	}
	
	public void Load(string value) => Value = value;
	
	public void OnTextInput(Keys key, KeyModifiers modifiers)
	{
		if (!IsFocused) return;
		
		if (key is Keys.Enter)
		{
			Value = BufferValue;
			IsFocused = false;
		}
		else if (key is Keys.Escape)
		{
			BufferValue = Value;
			IsFocused = false;
		}
		else
		{
			string tempValue = BufferValue;
			key.ManipulateText(modifiers, ref tempValue, ref cursorPosition);
			BufferValue = tempValue;
		}
	}
	
	// FIXME: These don't do anything. They're not generated automatically for some reason.
	public Color Color { get; set; }
	public void OnNotify(IDraw.Notification notification) { }
}

[UsedImplicitly, Prefab("Editors/DoubleFieldEditor.toml")]
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
	
	public event Action<object>? ValueChanged;
	
	public void Load(double value) => Value = value;
	
	public void UpdateValue(object obj) => UpdateValue((string)obj);
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
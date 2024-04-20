using System.Drawing;
using System.Globalization;
using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Rendering;
using Echidna2.Serialization;
using JetBrains.Annotations;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Echidna2.Mathematics;

namespace Echidna2;

[DontExpose]
public interface IFieldEditor
{
	public void Load(object? value);
	public event Action<object?>? ValueChanged;
	public static virtual IFieldEditor Instantiate() => throw new NotImplementedException();
}

[DontExpose]
public interface IFieldEditor<in T> : IFieldEditor
{
	public void Load(T value);
	void IFieldEditor.Load(object? value) => Load((T)value);
	// Trying to cast an Action<object> to an Action<T> doesn't work when T is a value type, so don't include ValueChanged
}

[UsedImplicitly, Prefab("Editors/StringFieldEditor.toml")]
public partial class StringFieldEditor : INotificationPropagator, IInitialize, IFieldEditor<string>, ITextInput
{
	[SerializedReference, ExposeMembersInClass] public FullRectWithHierarchy Rect { get; set; } = null!;
	[SerializedReference] public TextRect Text { get; set; } = null!;
	[SerializedReference] public Button Button { get; set; } = null!;
	
	public event Action<object?>? ValueChanged;
	
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
			Text.TextString = value;
		}
	}
	
	private int cursorPosition;
	
	[DontExpose] public bool HasBeenInitialized { get; set; }
	
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
		Value = "";
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
	
	public event Action<object?>? ValueChanged;
	
	public void Load(double value) => Value = value;
	
	public void UpdateValue(object? value) => UpdateValue((string)value!);
	public void UpdateValue(string stringValue)
	{
		if (double.TryParse(stringValue, NumberStyles.Any, CultureInfo.CurrentCulture, out double result))
		{
			value = result;
			if (value != result)
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

[UsedImplicitly, Prefab("Editors/ReferenceFieldEditor.toml")]
public partial class ReferenceFieldEditor : IFieldEditor, INotificationPropagator, IInitialize
{
	[SerializedReference, ExposeMembersInClass] public FullRectWithHierarchy Rect { get; set; } = null!;
	[SerializedReference] public TextRect Text { get; set; } = null!;
	[SerializedReference] public Button Button { get; set; } = null!;
	
	public event Action<object?>? ValueChanged = null;
	
	private object? value = null;
	public object? Value
	{
		get => value;
		set
		{
			this.value = value;
			Text.TextString = INamed.GetName(value) ?? "null";
			ValueChanged?.Invoke(value);
		}
	}
	
	[DontExpose] public bool HasBeenInitialized { get; set; }
	
	public void OnInitialize()
	{
		Button.MouseDown += () =>
		{
			AddComponentWindow window = AddComponentWindow.Instantiate();
			QueueAddChild(window);
		};
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect, Button);
	}
	
	public void Load(object? value) => Value = value;
}


[UsedImplicitly, Prefab("Editors/Vector2FieldEditor.toml")]
public partial class Vector2FieldEditor : INotificationPropagator, IFieldEditor<Vector2>
{
	[SerializedReference, ExposeMembersInClass] public HLayoutWithHierarchy Layout { get; set; } = null!;
	public DoubleFieldEditor? XEditor;
	public DoubleFieldEditor? YEditor;
	[DontExpose] public bool HasBeenInitialized { get; set; }
	[SerializedReference]
	private DoubleFieldEditor XFieldEditor
	{
		get => XEditor!;
		set
		{
			if (XEditor is not null)
				XEditor.ValueChanged -= UpdateX;
			
			XEditor = value;
			
			if (XEditor is not null)
				XEditor.ValueChanged += UpdateX;
		}
	}
	
	[SerializedReference]
	private DoubleFieldEditor YFieldEditor
	{
		get => YEditor!;
		set
		{
			if (YEditor is not null)
				YEditor.ValueChanged -= UpdateY;
			
			YEditor = value;
			
			if (YEditor is not null)
				YEditor.ValueChanged += UpdateY;
		}
	}
	
	private Vector2 value;
	public Vector2 Value
	{
		get => value;
		set
		{
			this.value = value;
			ValueChanged?.Invoke(value);
			XFieldEditor.Load(value.X);
			YFieldEditor.Load(value.Y);
		}
	}
	
	public event Action<object?>? ValueChanged;
	
	public void UpdateX(object? x) => UpdateX((double)x!);
	public void UpdateX(double x){
		value = new Vector2(x, value.Y);
	}
	public void UpdateY(object? y) => UpdateY((double)y!);
	public void UpdateY(double y){
		value = new Vector2(value.X, y);
	}
	
	public void Load(Vector2 value) => Value = value;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Layout);
	}
}

[UsedImplicitly, Prefab("Editors/Vector3FieldEditor.toml")]
public partial class Vector3FieldEditor : INotificationPropagator, IFieldEditor<Vector3>
{
	[SerializedReference, ExposeMembersInClass] public HLayoutWithHierarchy Layout { get; set; } = null!;
	public DoubleFieldEditor? XEditor;
	public DoubleFieldEditor? YEditor;
	public DoubleFieldEditor? ZEditor;
	[DontExpose] public bool HasBeenInitialized { get; set; }
	[SerializedReference]
	private DoubleFieldEditor XFieldEditor
	{
		get => XEditor!;
		set
		{
			if (XEditor is not null)
				XEditor.ValueChanged -= UpdateX;
			
			XEditor = value;
			
			if (XEditor is not null)
				XEditor.ValueChanged += UpdateX;
		}
	}
	
	[SerializedReference]
	private DoubleFieldEditor YFieldEditor
	{
		get => YEditor!;
		set
		{
			if (YEditor is not null)
				YEditor.ValueChanged -= UpdateY;
			
			YEditor = value;
			
			if (YEditor is not null)
				YEditor.ValueChanged += UpdateY;
		}
	}
	[SerializedReference]
	private DoubleFieldEditor ZFieldEditor
	{
		get => ZEditor!;
		set
		{
			if (ZEditor is not null)
				ZEditor.ValueChanged -= UpdateZ;
			
			ZEditor = value;
			
			if (ZEditor is not null)
				ZEditor.ValueChanged += UpdateZ;
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
		value = new Vector3(x, value.Y, value.Z);
		ValueChanged?.Invoke(value);
	}
	public void UpdateY(object? y) => UpdateY((double)y!);
	public void UpdateY(double y)
	{
		value = new Vector3(value.X, y, value.Z);
		ValueChanged?.Invoke(value);
	}
	public void UpdateZ(object? z) => UpdateZ((double)z!);
	public void UpdateZ(double z)
	{
		value = new Vector3(value.X, value.Y, z);
		ValueChanged?.Invoke(value);
	}
	
	public void Load(Vector3 value) => Value = value;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Layout);
	}
}

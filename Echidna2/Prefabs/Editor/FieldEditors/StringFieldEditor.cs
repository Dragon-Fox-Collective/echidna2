using System.Drawing;
using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Prefabs.Gui;
using Echidna2.Rendering;
using Echidna2.Serialization;
using JetBrains.Annotations;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Echidna2.Prefabs.Editor.FieldEditors;

[UsedImplicitly, Prefab("Prefabs/Editor/FieldEditors/StringFieldEditor.toml")]
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
}
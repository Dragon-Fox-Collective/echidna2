using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Prefabs.Gui;
using Echidna2.Serialization;
using JetBrains.Annotations;

namespace Echidna2.Prefabs.Editor.FieldEditors;

[UsedImplicitly, Prefab("Prefabs/Editor/FieldEditors/ReferenceFieldEditor.toml")]
public partial class ReferenceFieldEditor : IFieldEditor, INotificationPropagator, IInitialize
{
	[SerializedReference, ExposeMembersInClass] public FullRectWithHierarchy Rect { get; set; } = null!;
	[SerializedReference] public TextRect Text { get; set; } = null!;
	[SerializedReference] public Button Button { get; set; } = null!;
	
	public Type ComponentType = null!; // TODO: remove this and use a constructor of some kind instead
	
	public event Action<object?>? ValueChanged;
	
	public event Action<AddComponentWindow>? WindowOpened;
	
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
			window.ComponentType = ComponentType;
			window.Field = this;
			QueueAddChild(window);
			WindowOpened?.Invoke(window);
		};
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect, Button);
	}
	
	public void Load(object? value) => Value = value;
}
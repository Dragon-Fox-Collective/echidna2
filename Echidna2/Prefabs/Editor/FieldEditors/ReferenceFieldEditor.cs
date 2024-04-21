﻿using Echidna2.Core;
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
	
	public event Action<object?>? ValueChanged;
	
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
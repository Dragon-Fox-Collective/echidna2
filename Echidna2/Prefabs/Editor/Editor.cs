﻿using System.Drawing;
using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Mathematics;
using Echidna2.Prefabs.Editor.FieldEditors;
using Echidna2.Serialization;

namespace Echidna2.Prefabs.Editor;

public partial class Editor : INotificationPropagator, ICanBeLaidOut
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectLayout RectLayout { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	[SerializedReference] public Viewport Viewport { get; set; } = null!;
	[SerializedReference] public object ComponentPanel { get; set; } = null!;
	[SerializedValue] public string PrefabPath { get; set; } = "";
	
	private ObjectPanel? objectPanel;
	[SerializedReference] public ObjectPanel ObjectPanel
	{
		get => objectPanel!;
		set
		{
			if (objectPanel is not null)
				objectPanel.ItemSelected -= OnObjectSelected;
			
			objectPanel = value;
			
			if (objectPanel is not null)
				objectPanel.ItemSelected += OnObjectSelected;
		}
	}
	
	private PrefabRoot? prefabRoot;
	public PrefabRoot? PrefabRoot
	{
		get => prefabRoot;
		set
		{
			Viewport.ClearChildren();
			prefabRoot = value;
			ObjectPanel.PrefabRoot = value;
			if (prefabRoot is not null)
				Viewport.AddChild(prefabRoot.RootObject);
		}
	}
	
	private Dictionary<Type, Func<IFieldEditor>> editorInstantiators = new()
	{
		{ typeof(string), Instantiator("Prefabs/Editor/FieldEditors/StringFieldEditor") },
		{ typeof(double), Instantiator("Prefabs/Editor/FieldEditors/DoubleFieldEditor") },
		{ typeof(Vector2), Instantiator("Prefabs/Editor/FieldEditors/Vector2FieldEditor") },
		{ typeof(Vector3), Instantiator("Prefabs/Editor/FieldEditors/Vector3FieldEditor") },
		{ typeof(Quaternion), Instantiator("Prefabs/Editor/FieldEditors/QuaternionFieldEditor") },
		{ typeof(Color), Instantiator("Prefabs/Editor/FieldEditors/ColorFieldEditor") },
	};
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, RectLayout, PrefabChildren);
	}
	
	private void OnObjectSelected(object obj)
	{
		Console.WriteLine("Selected " + obj);
		ComponentPanel.GetType().GetMethod("set_SelectedObject").Invoke(ComponentPanel, [obj]); // lol. lmao even. revert to an acutal assignment asap
	}
	
	public void RegisterFieldEditor<TFieldType, TFieldEditor>() where TFieldEditor : IFieldEditor<TFieldType> => editorInstantiators.Add(typeof(TFieldType), TFieldEditor.Instantiate);
	public IFieldEditor InstantiateFieldEditor(Type type) => editorInstantiators[type]();
	public bool HasRegisteredFieldEditor(Type type) => editorInstantiators.ContainsKey(type);
	private static Func<IFieldEditor> Instantiator(string path) => () => (IFieldEditor)Instantiate(path);
	public static object Instantiate(string path) => TomlDeserializer.Deserialize(AppContext.BaseDirectory + path + ".prefab.toml").RootObject;
	
	public void SerializePrefab()
	{
		if (PrefabRoot is null)
			return;
		
		TomlSerializer.Serialize(PrefabRoot, AppContext.BaseDirectory + PrefabPath);
	}
}
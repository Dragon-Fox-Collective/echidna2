using System.Drawing;
using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Mathematics;
using Echidna2.Prefabs.Editor.FieldEditors;
using Echidna2.Serialization;
using JetBrains.Annotations;

namespace Echidna2.Prefabs.Editor;

[UsedImplicitly, Prefab("Prefabs/Editor/Editor.toml")]
public partial class Editor : INotificationPropagator, ICanBeLaidOut
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectLayout RectLayout { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	[SerializedReference] public Viewport Viewport { get; set; } = null!;
	[SerializedReference] public ComponentPanel ComponentPanel { get; set; } = null!;
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
		{ typeof(string), StringFieldEditor.Instantiate },
		{ typeof(double), DoubleFieldEditor.Instantiate },
		{ typeof(Vector2), Vector2FieldEditor.Instantiate },
		{ typeof(Vector3), Vector3FieldEditor.Instantiate },
		{ typeof(Quaternion), QuaternionFieldEditor.Instantiate },
		{ typeof(Color), Instantiator("Prefabs/Editor/FieldEditors/ColorFieldEditor.toml") },
	};
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, RectLayout, PrefabChildren);
	}
	
	private void OnObjectSelected(object obj)
	{
		Console.WriteLine("Selected " + obj);
		ComponentPanel.SelectedObject = obj;
	}
	
	public void RegisterFieldEditor<TFieldType, TFieldEditor>() where TFieldEditor : IFieldEditor<TFieldType> => editorInstantiators.Add(typeof(TFieldType), TFieldEditor.Instantiate);
	public IFieldEditor InstantiateFieldEditor(Type type) => editorInstantiators[type]();
	public bool HasRegisteredFieldEditor(Type type) => editorInstantiators.ContainsKey(type);
	private static Func<IFieldEditor> Instantiator(string path) => () => (IFieldEditor)TomlDeserializer.Deserialize(AppContext.BaseDirectory + path).RootObject;
	
	public void SerializePrefab()
	{
		if (PrefabRoot is null)
			return;
		
		TomlSerializer.Serialize(PrefabRoot, AppContext.BaseDirectory + PrefabPath);
	}
}
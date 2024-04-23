using Echidna2.Core;
using Echidna2.Prefabs.Editor.FieldEditors;
using Echidna2.Prefabs.Gui;
using Echidna2.Serialization;
using JetBrains.Annotations;

namespace Echidna2.Prefabs.Editor;

[UsedImplicitly, Prefab("Prefabs/Editor/AddComponentWindow.toml")]
public partial class AddComponentWindow : INotificationPropagator, IInitialize
{
	[SerializedReference, ExposeMembersInClass] public FullRectWindow Window { get; set; } = null!;
	
	public Type ComponentType = null!;
	public ReferenceFieldEditor Field = null!;
	public PrefabRoot? PrefabRoot => Field.ComponentPanel.Editor.PrefabRoot;
	
	[DontExpose] public bool HasBeenInitialized { get; set; }
	
	public void OnInitialize()
	{
		Window.CloseWindowRequest += () => Hierarchy.Parent.QueueRemoveChild(this);
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Window);
	}
	
	public void AddNewComponent()
	{
		object component = Activator.CreateInstance(ComponentType)!;
		PrefabRoot.Components.Add(component);
		Field.Value = component;
	}
}
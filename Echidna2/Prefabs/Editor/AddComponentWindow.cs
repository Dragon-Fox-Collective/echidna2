using System.Drawing;
using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Prefabs.Editor.FieldEditors;
using Echidna2.Prefabs.Gui;
using Echidna2.Serialization;
using JetBrains.Annotations;

namespace Echidna2.Prefabs.Editor;

[UsedImplicitly, Prefab("Prefabs/Editor/AddComponentWindow.toml")]
public partial class AddComponentWindow : INotificationPropagator, IInitialize, IEditorInitialize
{
	[SerializedReference, ExposeMembersInClass] public FullRectWindow Window { get; set; } = null!;
	
	[SerializedReference] public ICanAddChildren ComponentList { get; set; } = null!;
	
	public Type ComponentType = null!;
	public ReferenceFieldEditor Field = null!;
	
	private Editor editor = null!;
	public PrefabRoot? PrefabRoot => editor.PrefabRoot;
	
	[DontExpose] public bool HasBeenInitialized { get; set; }
	
	private (object Component, ButtonText Text)? selectedComponent;
	private (object Component, ButtonText Text)? SelectedComponent
	{
		get => selectedComponent;
		set
		{
			if (selectedComponent is not null)
				selectedComponent.Value.Text.Color = Color.White;
			
			selectedComponent = value;
			
			if (selectedComponent is not null)
				selectedComponent.Value.Text.Color = Color.DeepSkyBlue;
		}
	}
	
	public void OnInitialize()
	{
		Window.CloseWindowRequest += () => Hierarchy.Parent.QueueRemoveChild(this);
		
		foreach ((object component, ComponentUtils.ReferencePath? reference) in ComponentUtils.GetAllReferencesToComponentsOfType(PrefabRoot.RootObject, ComponentType, true))
		{
			ButtonText text = ButtonText.Instantiate();
			
			if (reference is null)
				text.TextString = $"Root {component.GetType().Name}";
			else if (reference.Component is IHasChildren hasChildren && hasChildren.Children.Contains(component))
				text.TextString = $"{component.GetType().Name} under\n\t{reference}";
			else
				text.TextString = $"{component.GetType().Name} on\n\t{reference}";
			
			text.LocalScale = (0.5, 0.5);
			text.Justification = TextJustification.Left;
			
			text.Clicked += () => SelectedComponent = (component, text);
			if (component == Field.Value)
				SelectedComponent = (component, text);
			
			ComponentList.AddChild(text);
		}
	}
	
	public void OnEditorInitialize(Editor editor)
	{
		this.editor = editor;
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Window);
	}
	
	[UsedImplicitly]
	public void AddNewComponent()
	{
		object component = Activator.CreateInstance(ComponentType)!;
		PrefabRoot.Components.Add(component);
		Field.Value = component;
	}
}
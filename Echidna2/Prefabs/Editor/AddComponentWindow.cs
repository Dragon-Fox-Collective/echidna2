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
	
	private ButtonText nullText = null!;
	
	private (object Component, ButtonText Text)? selectedComponent;
	private (object Component, ButtonText Text)? SelectedComponent
	{
		get => selectedComponent;
		set
		{
			(selectedComponent?.Text ?? nullText).Color = Color.White;
			selectedComponent = value;
			(selectedComponent?.Text ?? nullText).Color = Color.DeepSkyBlue;
		}
	}
	
	public void OnInitialize()
	{
		Window.CloseWindowRequest += () => Hierarchy.Parent.QueueRemoveChild(this);
		
		nullText = ButtonText.Instantiate();
		nullText.TextString = "Default value";
		nullText.LocalScale = (0.5, 0.5);
		nullText.Justification = TextJustification.Left;
		nullText.Clicked += () => SelectedComponent = null;
		if (Field.Value is null)
			SelectedComponent = null;
		
		foreach ((object component, ComponentUtils.ReferencePath? reference) in ComponentUtils.GetAllReferencesToComponentsOfType(PrefabRoot.RootObject, ComponentType, true)
			         .Where(pair => PrefabRoot.Components.Contains(pair.Component))
			         .Concat(PrefabRoot.Components
				         .Where(component => component.GetType().IsAssignableTo(ComponentType))
				         .Select(component => (component, (ComponentUtils.ReferencePath?) null)))
			         .DistinctBy(pair => pair.Item1))
		{
			ButtonText text = ButtonText.Instantiate();
			
			if (reference is null)
				text.TextString = $"Unparented {component.GetType().Name}";
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
		
		ComponentList.AddChild(nullText);
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
	
	[UsedImplicitly]
	public void UseSelectedComponent()
	{
		Field.Value = SelectedComponent?.Component ?? throw new NotImplementedException("Figure out how to use a default value");
	}
}
using System.Reflection;
using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Prefabs.Editor.FieldEditors;
using Echidna2.Prefabs.Gui;
using Echidna2.Serialization;
using JetBrains.Annotations;

namespace Echidna2.Prefabs.Editor;

[UsedImplicitly, Prefab("Prefabs/Editor/ComponentPanel.toml")]
public partial class ComponentPanel : INotificationPropagator, IEditorInitialize
{
	[SerializedReference, ExposeMembersInClass] public FullRectWithHierarchy Rect { get; set; } = null!;
	[SerializedReference] public TextRect PrefabNameText { get; set; } = null!;
	[SerializedReference] public TextRect ComponentNameText { get; set; } = null!;
	[SerializedReference] public ICanAddChildren Fields { get; set; } = null!;
	[SerializedReference] public ICanAddChildren Components { get; set; } = null!;
	
	private Editor editor = null!;
	
	private object? selectedObject;
	public object? SelectedObject
	{
		get => selectedObject;
		set
		{
			selectedObject = value;
			if (selectedObject is not null)
			{
				if (selectedObject is INamed named)
					named.NameChanged += name => PrefabNameText.TextString = name;
				PrefabNameText.TextString = INamed.GetName(selectedObject);
			}
			else
			{
				PrefabNameText.TextString = "No object selected";
			}
			
			SelectedComponent = null;
			
			INotificationPropagator.NotificationFinished += RefreshComponents;
		}
	}
	
	private object? selectedComponent;
	public object? SelectedComponent
	{
		get => selectedComponent;
		set
		{
			selectedComponent = value;
			INotificationPropagator.NotificationFinished += RefreshFields;
		}
	}
	
	public void RefreshFields()
	{
		if (selectedComponent is null)
			RefreshFavoritedFields();
		else
			RefreshSelectedComponentFields();
	}
	
	private void RefreshFavoritedFields()
	{
		Fields.ClearChildren();
		if (editor.PrefabRoot is null) return;
		if (selectedObject is null) return;
		
		ComponentNameText.TextString = "Favorites";
		
		List<object> availableComponents = ComponentUtils.GetAllComponents(selectedObject).ToList();
		IEnumerable<(object Component, MemberInfo Member)> availableFields = editor.PrefabRoot.FavoriteFields
			.Where(zip => availableComponents.Contains(zip.Component));
		
		if (editor.PrefabRoot.ChildPrefabs.FirstOrDefault(prefab => prefab.PrefabRoot.RootObject == selectedObject) is { } selectedPrefab)
			availableFields = availableFields.Concat(selectedPrefab.PrefabRoot.FavoriteFields);
		
		foreach ((object component, MemberInfo member) in availableFields)
			AddField(member, component);
	}

	private void RefreshSelectedComponentFields()
	{
		Fields.ClearChildren();
		if (selectedComponent is null) return;

		ComponentNameText.TextString = selectedComponent.GetType().Name;

		foreach (MemberInfo member in selectedComponent.GetType()
			         .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
			         .Where(member => member.GetCustomAttribute<SerializedValueAttribute>() is not null ||
			                          member.GetCustomAttribute<SerializedReferenceAttribute>() is not null))
			AddField(member, selectedComponent);
	}

	private void AddField(MemberInfo member, object component)
	{
		HLayoutWithHierarchy layout = HLayoutWithHierarchy.Instantiate();
		Fields.AddChild(layout);
		
		FullLayoutWithHierarchy textClipper = FullLayoutWithHierarchy.Instantiate();
		textClipper.ClipChildren = true;
		layout.AddChild(textClipper);
		
		TextRect text = TextRect.Instantiate();
		text.TextString = member.Name;
		text.LocalScale = (0.5, 0.5);
		text.MinimumSize = (300, 0);
		text.Justification = TextJustification.Left;
		textClipper.AddChild(text);
		
		IMemberWrapper wrapper = IMemberWrapper.Wrap(member);
		IFieldEditor? fieldEditor =
			member.GetCustomAttribute<SerializedValueAttribute>() is not null ? editor.HasRegisteredFieldEditor(wrapper.FieldType) ? editor.InstantiateFieldEditor(wrapper.FieldType) : null :
			member.GetCustomAttribute<SerializedReferenceAttribute>() is not null ? NewReferenceFieldEditorOfType(wrapper.FieldType) :
			null;
		if (fieldEditor is not null)
		{
			fieldEditor.Load(wrapper.GetValue(component));
			fieldEditor.ValueChanged += value =>
			{
				wrapper.SetValue(component, value);
				editor.PrefabRoot?.RegisterChange(new MemberPath(wrapper, new ComponentPath(component)));
				editor.SerializePrefab();
			};
			layout.AddChild(fieldEditor);
		}
		
		ButtonRect favoriteButton = ButtonRect.Instantiate();
		favoriteButton.MinimumSize = (25, 25);
		favoriteButton.Clicked += () =>
		{
			if (editor.PrefabRoot is null) return;
			
			if (!editor.PrefabRoot.FavoriteFields.Remove((component, member)))
				editor.PrefabRoot.FavoriteFields.Add((component, member));
			
			editor.SerializePrefab();
		};
		layout.AddChild(favoriteButton);
		
		ReferenceFieldEditor NewReferenceFieldEditorOfType(Type type)
		{
			ReferenceFieldEditor referenceFieldEditor = ReferenceFieldEditor.Instantiate();
			referenceFieldEditor.ComponentType = type;
			INotificationPropagator.Notify(new IEditorInitialize.Notification(editor), referenceFieldEditor);
			return referenceFieldEditor;
		}
	}
	
	public void RefreshComponents()
	{
		Components.ClearChildren();
		if (selectedObject is null) return;
		
		ButtonRect favoritesButton = (ButtonRect)TomlDeserializer.Deserialize(AppContext.BaseDirectory + "Prefabs/Editor/ComponentSelectionButton.toml").RootObject;
		favoritesButton.Clicked += () => SelectedComponent = null;
		favoritesButton.MinimumSize = (40, 40);
		Components.AddChild(favoritesButton);
		
		foreach (object component in ComponentUtils.GetAllComponents(selectedObject))
		{
			ButtonRect button = (ButtonRect)TomlDeserializer.Deserialize(AppContext.BaseDirectory + "Prefabs/Editor/ComponentSelectionButton.toml").RootObject;
			button.Clicked += () => SelectedComponent = component;
			Components.AddChild(button);
		}
	}
	
	public void OnEditorInitialize(Editor editor) => this.editor = editor;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect);
	}
}
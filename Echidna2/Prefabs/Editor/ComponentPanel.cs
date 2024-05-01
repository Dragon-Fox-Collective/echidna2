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
			
			SelectedComponent = selectedObject;
			
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
		
		List<object> availableComponents = GetAllComponentsReferencedBy(selectedObject);
		foreach ((object component, MemberInfo member) in editor.PrefabRoot.FavoritedFields
			         .Where(zip => availableComponents.Contains(zip.Component)))
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
		
		TextRect text = TextRect.Instantiate();
		text.TextString = member.Name;
		text.LocalScale = (0.5, 0.5);
		text.MinimumSize = (150, 25);
		text.Justification = TextJustification.Left;
		layout.AddChild(text);
		
		IMemberWrapper wrapper = IMemberWrapper.Wrap(member);
		IFieldEditor? fieldEditor =
			member.GetCustomAttribute<SerializedValueAttribute>() is not null ? editor.HasRegisteredFieldEditor(wrapper.FieldType) ? editor.InstantiateFieldEditor(wrapper.FieldType) : null :
			member.GetCustomAttribute<SerializedReferenceAttribute>() is not null ? ReferenceFieldEditor.Instantiate() :
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
			
			if (!editor.PrefabRoot.FavoritedFields.Remove((component, member)))
				editor.PrefabRoot.FavoritedFields.Add((component, member));
			
			editor.SerializePrefab();
		};
		layout.AddChild(favoriteButton);
	}
	
	public void RefreshComponents()
	{
		Components.ClearChildren();
		if (selectedObject is null) return;
		
		ButtonRect favoritesButton = (ButtonRect)TomlDeserializer.Deserialize(AppContext.BaseDirectory + "Prefabs/Editor/ComponentSelectionButton.toml").RootObject;
		favoritesButton.Clicked += () => SelectedComponent = null;
		favoritesButton.MinimumSize = (40, 40);
		Components.AddChild(favoritesButton);
		
		foreach (object component in GetAllComponentsReferencedBy(selectedObject))
		{
			ButtonRect button = (ButtonRect)TomlDeserializer.Deserialize(AppContext.BaseDirectory + "Prefabs/Editor/ComponentSelectionButton.toml").RootObject;
			button.Clicked += () => SelectedComponent = component;
			Components.AddChild(button);
		}
	}
	
	private static List<object> GetAllComponentsReferencedBy(object component)
	{
		List<object> thingsToSearch = [component];
		List<object> thingsSearched = [];
		
		while (!thingsToSearch.IsEmpty())
		{
			object thing = thingsToSearch.Pop();

			thingsToSearch.AddRange(
				thing.GetType().GetMembers()
					.Where(member => member.GetCustomAttribute<SerializedReferenceAttribute>() is not null)
					.Select(member => IMemberWrapper.Wrap(member).GetValue(thing))
					.WhereNotNull()
					.Where(nextThing => !thingsSearched.Contains(nextThing) && !thingsToSearch.Contains(nextThing))
					.Distinct()
			);

			thingsSearched.AddIfDistinct(thing);
		}
		
		return thingsSearched;
	}
	
	public void OnEditorInitialize(Editor editor) => this.editor = editor;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect);
	}
}
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
				ComponentNameText.TextString = selectedObject.GetType().Name;
			}
			else
			{
				PrefabNameText.TextString = "No object selected";
				ComponentNameText.TextString = "No component selected";
			}
			
			INotificationPropagator.NotificationFinished += () =>
			{
				RefreshFields();
				RefreshComponents();
			};
		}
	}
	
	public void RefreshFields()
	{
		Fields.ClearChildren();
		if (selectedObject is null) return;
		
		foreach (MemberInfo member in selectedObject.GetType()
			         .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
			         .Where(member => member.GetCustomAttribute<SerializedValueAttribute>() is not null ||
			                          member.GetCustomAttribute<SerializedReferenceAttribute>() is not null))
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
				fieldEditor.Load(wrapper.GetValue(selectedObject));
				fieldEditor.ValueChanged += value =>
				{
					wrapper.SetValue(selectedObject, value);
					editor.PrefabRoot?.RegisterChange(new MemberPath(wrapper, new ComponentPath(selectedObject)));
					editor.SerializePrefab();
				};
				layout.AddChild(fieldEditor);
			}
		}
	}
	
	public void RefreshComponents()
	{
		Components.ClearChildren();
		if (selectedObject is null) return;
		
		List<object> thingsToSearch = [selectedObject];
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
		
		foreach (object component in thingsSearched)
		{
			ButtonRect button = (ButtonRect)TomlDeserializer.Deserialize(AppContext.BaseDirectory + "Prefabs/Editor/ComponentSelectionButton.toml").RootObject;
			button.Clicked += () => SelectedObject = component;
			Components.AddChild(button);
		}
	}
	
	public void OnEditorInitialize(Editor editor) => this.editor = editor;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect);
	}
}
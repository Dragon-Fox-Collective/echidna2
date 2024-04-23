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
	[SerializedReference] public TextRect NameText { get; set; } = null!;
	[SerializedReference] public ICanAddChildren Fields { get; set; } = null!;
	
	public Editor Editor { get; private set; } = null!;
	
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
					named.NameChanged += name => NameText.TextString = name;
				NameText.TextString = INamed.GetName(selectedObject);
			}
			else
			{
				NameText.TextString = "(no object selected)";
			}
			RefreshFields();
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
				member.GetCustomAttribute<SerializedValueAttribute>() is not null ? Editor.HasRegisteredFieldEditor(wrapper.FieldType) ? Editor.InstantiateFieldEditor(wrapper.FieldType) : null :
				member.GetCustomAttribute<SerializedReferenceAttribute>() is not null ? NewReferenceFieldEditorOfType(wrapper.FieldType) :
				null;
			if (fieldEditor is not null)
			{
				fieldEditor.Load(wrapper.GetValue(selectedObject));
				fieldEditor.ValueChanged += value =>
				{
					wrapper.SetValue(selectedObject, value);
					Editor.PrefabRoot?.RegisterChange(new MemberPath(wrapper, new ComponentPath(selectedObject)));
					Editor.SerializePrefab();
				};
				layout.AddChild(fieldEditor);
			}
		}
		return;
		
		ReferenceFieldEditor NewReferenceFieldEditorOfType(Type type)
		{
			ReferenceFieldEditor referenceFieldEditor = ReferenceFieldEditor.Instantiate();
			referenceFieldEditor.ComponentType = type;
			referenceFieldEditor.ComponentPanel = this;
			return referenceFieldEditor;
		}
	}
	
	public void OnEditorInitialize(Editor editor) => this.Editor = editor;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect);
	}
}
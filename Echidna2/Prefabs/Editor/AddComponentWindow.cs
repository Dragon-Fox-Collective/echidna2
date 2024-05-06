using System.Reflection;
using Echidna2.Core;
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
	
	public void OnInitialize()
	{
		Window.CloseWindowRequest += () => Hierarchy.Parent.QueueRemoveChild(this);
		
		foreach (object component in GetComponentsRecursive(PrefabRoot.RootObject, ComponentType))
		{
			TextRect text = TextRect.Instantiate();
			text.TextString = INamed.GetName(component);
			text.MinimumSize = (0, 20);
			text.LocalScale = (0.5, 0.5);
			ComponentList.AddChild(text);
		}
	}
	
	private IEnumerable<object> GetComponentsRecursive(object component, Type type) => GetComponentsRecursiveIncludingDuplicates(component, type).Distinct();
	
	private IEnumerable<object> GetComponentsRecursiveIncludingDuplicates(object component, Type type)
	{
		if (component.GetType().IsAssignableTo(type))
			yield return component;
		
		if (component is IHasChildren hasChildren)
			foreach (object child in hasChildren.Children)
				foreach (object t in GetComponentsRecursiveIncludingDuplicates(child, type))
					yield return t;
		
		foreach (MemberInfo member in component.GetType()
			         .GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			         .Where(member => member.GetCustomAttribute<SerializedReferenceAttribute>() is not null))
		{
			object? child = IMemberWrapper.Wrap(member).GetValue(component);
			if (child is not null)
				foreach (object t in GetComponentsRecursiveIncludingDuplicates(child, type))
					yield return t;
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
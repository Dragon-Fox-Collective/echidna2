using Echidna2.Core;
using Echidna2.Prefabs.Gui;
using Echidna2.Serialization;
using JetBrains.Annotations;

namespace Echidna2.Prefabs.Editor;

[UsedImplicitly, Prefab("Prefabs/Editor/ObjectPanel.toml")]
public partial class ObjectPanel : INotificationPropagator
{
	[SerializedReference, ExposeMembersInClass] public FullRectWithHierarchy Rect { get; set; } = null!;
	[SerializedReference] public HierarchyDisplay HierarchyDisplay { get; set; } = null!;
	
	public event Action<object> ItemSelected
	{
		add => HierarchyDisplay.ItemSelected += value;
		remove => HierarchyDisplay.ItemSelected -= value;
	}
	
	private PrefabRoot? prefabRoot;
	public PrefabRoot? PrefabRoot
	{
		get => prefabRoot;
		set
		{
			prefabRoot = value;
			HierarchyDisplay.HierarchyToDisplay = prefabRoot?.RootObject as IHasChildren;
		}
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect);
	}
}
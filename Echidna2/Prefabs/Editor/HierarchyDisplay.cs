using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Mathematics;
using Echidna2.Prefabs.Gui;
using Echidna2.Serialization;

namespace Echidna2.Prefabs.Editor;

public partial class HierarchyDisplay : INotificationPropagator, ICanBeLaidOut
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference] private Hierarchy DisplayElements { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public FullLayout Layout { get; set; } = null!;
	
	public event Action<object>? ItemSelected;
	
	private IHasChildren? hierarchyToDisplay = null;
	[SerializedReference] public IHasChildren? HierarchyToDisplay
	{
		get => hierarchyToDisplay;
		set
		{
			DisplayElements.ClearChildren();
			hierarchyToDisplay = value;
			if (hierarchyToDisplay is not null)
			{
				FullLayoutWithHierarchy box = BoxOfHierarchy(hierarchyToDisplay);
				box.LeftMargin = 0;
				DisplayElements.AddChild(box);
			}
		}
	}
	
	private FullLayoutWithHierarchy BoxOfHierarchy(object obj)
	{
		FullLayoutWithHierarchy box = (FullLayoutWithHierarchy)TomlDeserializer.Instantiate("Prefabs/Gui/FullLayoutWithHierarchy");
		box.Name = $"Box for {obj}";
		box.AnchorPreset = AnchorPreset.Full;
		box.LeftMargin = 10;
		VLayoutWithHierarchy layout = (VLayoutWithHierarchy)TomlDeserializer.Instantiate("Prefabs/Gui/VLayoutWithHierarchy");
		layout.Name = $"Layout for box for {obj}";
		layout.AnchorPreset = AnchorPreset.Full;
		box.AddChild(layout);
		
		ButtonRect button = (ButtonRect)TomlDeserializer.Instantiate("Prefabs/Gui/ButtonRect");
		button.AnchorPreset = AnchorPreset.Full;
		button.Margin = 5;
		button.MouseUp += () => ItemSelected?.Invoke(obj);
		layout.AddChild(button);
		
		TextRect text = (TextRect)TomlDeserializer.Instantiate("Prefabs/Gui/TextRect");
		text.TextString = INamed.GetName(obj);
		if (obj is INamed named)
			named.NameChanged += name => text.TextString = name;
		text.Justification = TextJustification.Left;
		text.AnchorPreset = AnchorPreset.Full;
		text.LocalScale = Vector2.One * 0.5;
		button.AddChild(text);
		
		if (obj is IHasChildren hasChildren)
			foreach (object child in hasChildren.Children)
				layout.AddChild(BoxOfHierarchy(child));
		
		return box;
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Layout, DisplayElements);
	}
}
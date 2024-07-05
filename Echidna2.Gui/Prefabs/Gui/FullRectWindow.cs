using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Serialization;

namespace Echidna2.Prefabs.Gui;

public partial class FullRectWindow : INotificationPropagator, IInitialize
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public FullLayout Layout { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Rect Rect { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	[SerializedReference] public Button Button { get; set; } = null!;
	
	public event Action? CloseWindowRequest;
	
	[DontExpose] public bool HasBeenInitialized { get; set; }
	
	public void OnInitialize()
	{
		Button.MouseDownOutside += CloseWindow;
	}
	
	public void CloseWindow()
	{
		CloseWindowRequest?.Invoke();
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect, Layout, PrefabChildren, Button);
	}
}
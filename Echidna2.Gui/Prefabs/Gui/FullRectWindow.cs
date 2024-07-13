using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Rendering;
using Echidna2.Serialization;

namespace Echidna2.Prefabs.Gui;

public partial class FullRectWindow : INotificationPropagator
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public FullLayout Layout { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Rect Rect { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	
	private Button? _Button = default!;
	[SerializedReference, ExposeMembersInClass] public Button Button
	{
		get => _Button;
		set
		{
			if (_Button is not null)
				Unsetup_Button();
			
			_Button = value;
			
			if (_Button is not null)
				Setup_Button();
		}
	}
	protected virtual void Setup_Button()
	{
		Button.MouseDownOutside += CloseWindow;
	}
	protected virtual void Unsetup_Button()
	{
		Button.MouseDownOutside -= CloseWindow;
	}
	
	public event Action? CloseWindowRequest;
	
	private void CloseWindow(MouseDownNotification notification) => CloseWindow();
	public void CloseWindow()
	{
		CloseWindowRequest?.Invoke();
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect, Layout, PrefabChildren, Button);
	}
}
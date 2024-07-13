using System.Drawing;
using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Rendering;
using Echidna2.Serialization;

namespace Echidna2.Prefabs.Gui;

public partial class ButtonRect : INotificationPropagator, ICanBeLaidOut, INotificationListener<InitializeNotification>
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
		Button.MouseDown += SetDarkColor;
		Button.MouseUp += SetLightColor;
	}
	protected virtual void Unsetup_Button()
	{
	}
	
	public void OnNotify(InitializeNotification notification)
	{
		lightColor = Rect.Color;
		darkColor = Color.FromArgb(lightColor.A / 2, lightColor.R / 2, lightColor.G / 2, lightColor.B / 2);
	}
	
	private Color lightColor;
	private Color darkColor;
	
	private void SetLightColor(MouseUpNotification notification)
	{
		Rect.Color = lightColor;
	}
	private void SetDarkColor(MouseDownNotification notification)
	{
		Rect.Color = darkColor;
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect, Button, Layout, PrefabChildren);
	}
}
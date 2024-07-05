using System.Drawing;
using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Serialization;

namespace Echidna2.Prefabs.Gui;

public partial class ButtonRect : INotificationPropagator, IInitialize, ICanBeLaidOut
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
	}
	protected virtual void Unsetup_Button()
	{
	}
	
	[DontExpose] public bool HasBeenInitialized { get; set; }
	
	public virtual void OnInitialize()
	{
		Color color = Rect.Color;
		Color darkColor = Color.FromArgb(color.A / 2, color.R / 2, color.G / 2, color.B / 2);
		Button.MouseDown += () => Rect.Color = darkColor;
		Button.MouseUp += () => Rect.Color = color;
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect, Button, Layout, PrefabChildren);
	}
}
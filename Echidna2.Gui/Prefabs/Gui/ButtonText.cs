using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Serialization;

namespace Echidna2.Prefabs.Gui;

public partial class ButtonText : INotificationPropagator
{
	[SerializedReference, ExposeMembersInClass] public TextRect TextRect { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Button Button { get; set; } = null!;
	
	[SerializedReference] public RectTransform RectTransform
	{
		get => TextRect.RectTransform;
		set => TextRect.RectTransform = value;
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, TextRect, Button);
	}
}
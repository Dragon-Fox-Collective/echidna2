using Echidna2.Core;
using Echidna2.Gui;

namespace Echidna2.Prefabs.Test;

public partial class ButtonWithTransform : INotificationPropagator
{
	[ExposeMembersInClass] public RectTransform RectTransform { get; set; }
	[ExposeMembersInClass] public Button Button { get; set; }
	
	public ButtonWithTransform()
	{
		RectTransform = new RectTransform
		{
			LocalSize = (10, 10),
		};
		Button = new Button
		{
			RectTransform = RectTransform,
		};
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Button);
	}
}
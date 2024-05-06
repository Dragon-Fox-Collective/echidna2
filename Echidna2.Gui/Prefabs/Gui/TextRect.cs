using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Serialization;
using JetBrains.Annotations;

namespace Echidna2.Prefabs.Gui;

[UsedImplicitly, Prefab("Prefabs/Gui/TextRect.toml")]
public partial class TextRect : INotificationPropagator, ICanBeLaidOut, IInitialize
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Text Text { get; set; } = null!;
	
	[DontExpose] public bool HasBeenInitialized { get; set; }
	
	/// <summary>
	/// Sets the text line height and also sets the minimum height of the rect transform.
	/// </summary>
	[SerializedValue] public double LineHeight
	{
		get => Text.LineHeight;
		set
		{
			Text.LineHeight = value;
			RecalculateMinimumSize();
		}
	}
	
	[SerializedValue] public string TextString
	{
		get => Text.TextString;
		set
		{
			Text.TextString = value;
			RecalculateMinimumSize();
		}
	}
	
	public void OnInitialize()
	{
		RecalculateMinimumSize();
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Text);
	}
	
	private void RecalculateMinimumSize()
	{
		MinimumSize = MinimumSize with { Y = ScaledLineHeight * (Text.TextString.Count(c => c == '\n') + 1) };
	}
}
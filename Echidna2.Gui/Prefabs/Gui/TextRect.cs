﻿using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Serialization;

namespace Echidna2.Prefabs.Gui;

public partial class TextRect : INotificationPropagator, ICanBeLaidOut, INotificationListener<InitializeNotification>
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Text Text { get; set; } = null!;
	
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
	
	public void OnNotify(InitializeNotification notification)
	{
		RecalculateMinimumSize();
	}
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Text);
	}
	
	private void RecalculateMinimumSize()
	{
		MinimumSize = MinimumSize with { Y = LineHeight * (Text.TextString.Count(c => c == '\n') + 1) };
	}
}
﻿using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Serialization;

namespace Echidna2.Prefabs.Gui;

public partial class RectWithHierarchy : INotificationPropagator, ICanBeLaidOut
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectTransform RectTransform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public RectLayout Layout { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Rect Rect { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect, Layout, PrefabChildren);
	}
}
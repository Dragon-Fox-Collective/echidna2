﻿using Echidna2.Core;
using Echidna2.Mathematics;
using Vector2i = OpenTK.Mathematics.Vector2i;

namespace Echidna2.Rendering;

public class Camera
{
	public Vector2i Size { get; set; }
	public double FarClipPlane { get; set; } = 1000;
	public double NearClipPlane { get; set; } = 0.1;
	
	public INotificationPropagator? World { get; set; }
	
	public Matrix4 ViewMatrix => Matrix4.Translation(Vector3.Out * (FarClipPlane - 1)).Inverted;
	public Matrix4 ProjectionMatrix => Matrix4.OrthographicProjection(Size.X, Size.Y, (float)NearClipPlane, (float)FarClipPlane);
	
	public void RenderPreUpdate()
	{
		INotificationPropagator.NotifySingle(World, new IPreUpdate.Notification());
	}
	
	public void RenderUpdate(double deltaTime)
	{
		INotificationPropagator.NotifySingle(World, new IUpdate.Notification(deltaTime));
	}
	
	public void Render()
	{
		INotificationPropagator.NotifySingle(World, new IDraw.Notification(this));
	}
}
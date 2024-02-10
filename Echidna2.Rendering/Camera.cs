using Echidna2.Core;
using OpenTK.Mathematics;

namespace Echidna2.Rendering;

public class Camera
{
	public Vector2i Size { get; set; }
	public double FarClipPlane { get; set; } = 1000;
	public double NearClipPlane { get; set; } = 0.1;
	
	public INotificationPropagator? World { get; set; }
	
	public void RenderPreUpdate()
	{
		World?.Notify(new IPreUpdate.Notification());
	}
	
	public void RenderUpdate(double deltaTime)
	{
		World?.Notify(new IUpdate.Notification(deltaTime));
	}
	
	public void Render()
	{
		World?.Notify(new IDraw.Notification(this));
	}
}
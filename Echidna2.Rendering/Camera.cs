using Echidna2.Core;

namespace Echidna2.Rendering;

public class Camera
{
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
		World?.Notify(new IDraw.Notification());
	}
}
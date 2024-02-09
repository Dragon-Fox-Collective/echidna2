using Echidna2.Core;
using OpenTK.Mathematics;

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
	
	public void Render(Vector2i screenSize)
	{
		World?.Notify(new IDraw.Notification(screenSize));
	}
}
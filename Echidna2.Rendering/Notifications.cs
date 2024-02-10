using Echidna2.Core;

namespace Echidna2.Rendering;

public interface IDraw : INotificationListener<IDraw.Notification>
{
	public class Notification(Camera camera)
	{
		public Camera Camera { get; } = camera;
	}
	void INotificationListener<Notification>.OnNotify(Notification notification) => OnDraw();
	public void OnDraw();
}
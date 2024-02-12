using Echidna2.Core;
using Echidna2.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

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

public interface IMouseMoved : INotificationListener<IMouseMoved.Notification>
{
	public class Notification(Vector2 position, Vector2 delta)
	{
		public Vector2 Position { get; } = position;
		public Vector2 Delta { get; } = delta;
	}
	void INotificationListener<Notification>.OnNotify(Notification notification) => OnMouseMoved(notification.Position, notification.Delta);
	public void OnMouseMoved(Vector2 position, Vector2 delta);
}

public interface IMouseDown : INotificationListener<IMouseDown.Notification>
{
	public class Notification(MouseButton button, Vector2 position)
	{
		public MouseButton Button { get; } = button;
		public Vector2 Position { get; } = position;
	}
	void INotificationListener<Notification>.OnNotify(Notification notification) => OnMouseDown(notification.Button, notification.Position);
	public void OnMouseDown(MouseButton button, Vector2 position);
}

public interface IMouseUp : INotificationListener<IMouseUp.Notification>
{
	public class Notification(MouseButton button, Vector2 position)
	{
		public MouseButton Button { get; } = button;
		public Vector2 Position { get; } = position;
	}
	void INotificationListener<Notification>.OnNotify(Notification notification) => OnMouseDown(notification.Button, notification.Position);
	public void OnMouseDown(MouseButton button, Vector2 position);
}
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
	public class Notification(Vector2 position, Vector2 delta, Vector3 globalPosition)
	{
		public Vector2 Position { get; } = position;
		public Vector2 Delta { get; } = delta;
		public Vector3 GlobalPosition { get; } = globalPosition;
	}
	void INotificationListener<Notification>.OnNotify(Notification notification) => OnMouseMoved(notification.Position, notification.Delta, notification.GlobalPosition);
	public void OnMouseMoved(Vector2 position, Vector2 delta, Vector3 globalPosition);
}

public interface IMouseDown : INotificationListener<IMouseDown.Notification>
{
	public class Notification(MouseButton button, Vector2 position, Vector3 globalPosition)
	{
		public MouseButton Button { get; } = button;
		public Vector2 Position { get; } = position;
		public Vector3 GlobalPosition { get; } = globalPosition;
	}
	void INotificationListener<Notification>.OnNotify(Notification notification) => OnMouseDown(notification.Button, notification.Position, notification.GlobalPosition);
	public void OnMouseDown(MouseButton button, Vector2 position, Vector3 globalPosition);
}

public interface IMouseUp : INotificationListener<IMouseUp.Notification>
{
	public class Notification(MouseButton button, Vector2 position, Vector3 globalPosition)
	{
		public MouseButton Button { get; } = button;
		public Vector2 Position { get; } = position;
		public Vector3 GlobalPosition { get; } = globalPosition;
	}
	void INotificationListener<Notification>.OnNotify(Notification notification) => OnMouseUp(notification.Button, notification.Position, notification.GlobalPosition);
	public void OnMouseUp(MouseButton button, Vector2 position, Vector3 globalPosition);
}

public interface IMouseWheelScrolled : INotificationListener<IMouseWheelScrolled.Notification>
{
	public class Notification(Vector2 offset, Vector2 position, Vector3 globalPosition)
	{
		public Vector2 Offset { get; } = offset;
		public Vector2 Position { get; } = position;
		public Vector3 GlobalPosition { get; } = globalPosition;
	}
	void INotificationListener<Notification>.OnNotify(Notification notification) => OnMouseWheelScrolled(notification.Offset, notification.Position, notification.GlobalPosition);
	public void OnMouseWheelScrolled(Vector2 offset, Vector2 position, Vector3 globalPosition);
}

public interface IKeyDown : INotificationListener<IKeyDown.Notification>
{
	public class Notification(Keys key)
	{
		public Keys Key { get; } = key;
	}
	void INotificationListener<Notification>.OnNotify(Notification notification) => OnKeyDown(notification.Key);
	public void OnKeyDown(Keys key);
}

public interface IKeyUp : INotificationListener<IKeyUp.Notification>
{
	public class Notification(Keys key)
	{
		public Keys Key { get; } = key;
	}
	void INotificationListener<Notification>.OnNotify(Notification notification) => OnKeyUp(notification.Key);
	public void OnKeyUp(Keys key);
}

public interface IDispose : INotificationListener<IDispose.Notification>
{
	public class Notification;
	void INotificationListener<Notification>.OnNotify(Notification notification) => OnDispose();
	public void OnDispose();
}

public interface IDrawPass : INotificationListener<IDrawPass.Notification>
{
	public class Notification;
	void INotificationListener<Notification>.OnNotify(Notification notification) => OnDrawPass();
	public void OnDrawPass();
}
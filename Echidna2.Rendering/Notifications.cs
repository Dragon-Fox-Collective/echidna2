using Echidna2.Core;
using Echidna2.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Echidna2.Rendering;

public class Draw_Notification(Camera camera)
{
	public Camera Camera { get; } = camera;
}
[DontExpose]
public interface IDraw : INotificationListener<Draw_Notification>
{
	void INotificationListener<Draw_Notification>.OnNotify(Draw_Notification notification) => OnDraw();
	public void OnDraw();
}

public interface IMouseNotification
{
	public Vector2 Position { get; }
	public Vector3 GlobalPosition { get; }
	public bool Clipped { get; set; }
}

public class MouseMoved_Notification(Vector2 position, Vector2 delta, Vector3 globalPosition) : IMouseNotification
{
	public Vector2 Position { get; } = position;
	public Vector2 Delta { get; } = delta;
	public Vector3 GlobalPosition { get; } = globalPosition;
	public bool Clipped { get; set; } = false;
}
[DontExpose]
public interface IMouseMoved : INotificationListener<MouseMoved_Notification>
{
	void INotificationListener<MouseMoved_Notification>.OnNotify(MouseMoved_Notification notification) => OnMouseMoved(notification.Position, notification.Delta, notification.GlobalPosition, notification.Clipped);
	public void OnMouseMoved(Vector2 position, Vector2 delta, Vector3 globalPosition, bool clipped);
}

public class MouseDown_Notification(MouseButton button, Vector2 position, Vector3 globalPosition) : IMouseNotification
{
	public MouseButton Button { get; } = button;
	public Vector2 Position { get; } = position;
	public Vector3 GlobalPosition { get; } = globalPosition;
	public bool Clipped { get; set; } = false;
}
[DontExpose]
public interface IMouseDown : INotificationListener<MouseDown_Notification>
{
	void INotificationListener<MouseDown_Notification>.OnNotify(MouseDown_Notification notification) => OnMouseDown(notification.Button, notification.Position, notification.GlobalPosition, notification.Clipped);
	public void OnMouseDown(MouseButton button, Vector2 position, Vector3 globalPosition, bool clipped);
}

public class MouseUp_Notification(MouseButton button, Vector2 position, Vector3 globalPosition) : IMouseNotification
{
	public MouseButton Button { get; } = button;
	public Vector2 Position { get; } = position;
	public Vector3 GlobalPosition { get; } = globalPosition;
	public bool Clipped { get; set; } = false;
}
[DontExpose]
public interface IMouseUp : INotificationListener<MouseUp_Notification>
{
	void INotificationListener<MouseUp_Notification>.OnNotify(MouseUp_Notification notification) => OnMouseUp(notification.Button, notification.Position, notification.GlobalPosition, notification.Clipped);
	public void OnMouseUp(MouseButton button, Vector2 position, Vector3 globalPosition, bool clipped);
}

public class MouseWheelScrolled_Notification(Vector2 offset, Vector2 position, Vector3 globalPosition) : IMouseNotification
{
	public Vector2 Offset { get; } = offset;
	public Vector2 Position { get; } = position;
	public Vector3 GlobalPosition { get; } = globalPosition;
	public bool Clipped { get; set; } = false;
}
[DontExpose]
public interface IMouseWheelScrolled : INotificationListener<MouseWheelScrolled_Notification>
{
	void INotificationListener<MouseWheelScrolled_Notification>.OnNotify(MouseWheelScrolled_Notification notification) => OnMouseWheelScrolled(notification.Offset, notification.Position, notification.GlobalPosition, notification.Clipped);
	public void OnMouseWheelScrolled(Vector2 offset, Vector2 position, Vector3 globalPosition, bool clipped);
}

public class KeyDown_Notification(Keys key)
{
	public Keys Key { get; } = key;
}
[DontExpose]
public interface IKeyDown : INotificationListener<KeyDown_Notification>
{
	void INotificationListener<KeyDown_Notification>.OnNotify(KeyDown_Notification notification) => OnKeyDown(notification.Key);
	public void OnKeyDown(Keys key);
}

public class KeyUp_Notification(Keys key)
{
	public Keys Key { get; } = key;
}
[DontExpose]
public interface IKeyUp : INotificationListener<KeyUp_Notification>
{
	void INotificationListener<KeyUp_Notification>.OnNotify(KeyUp_Notification notification) => OnKeyUp(notification.Key);
	public void OnKeyUp(Keys key);
}

public class TextInput_Notification(Keys key, KeyModifiers modifiers)
{
	public Keys Key { get; } = key;
	public KeyModifiers Modifiers { get; } = modifiers;
}
[DontExpose]
public interface ITextInput : INotificationListener<TextInput_Notification>
{
	void INotificationListener<TextInput_Notification>.OnNotify(TextInput_Notification notification) => OnTextInput(notification.Key, notification.Modifiers);
	public void OnTextInput(Keys key, KeyModifiers modifiers);
}

public class Dispose_Notification;
public interface IDispose : INotificationListener<Dispose_Notification>
{
	void INotificationListener<Dispose_Notification>.OnNotify(Dispose_Notification notification) => OnDispose();
	public void OnDispose();
}

public class DrawPass_Notification;
public interface IDrawPass : INotificationListener<DrawPass_Notification>
{
	void INotificationListener<DrawPass_Notification>.OnNotify(DrawPass_Notification notification) => OnDrawPass();
	public void OnDrawPass();
}
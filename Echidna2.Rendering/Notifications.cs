using Echidna2.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Echidna2.Rendering;

public class DrawNotification(Camera camera)
{
	public Camera Camera { get; } = camera;
}

public interface IMouseNotification
{
	public Vector3 GlobalPosition { get; }
	public bool Clipped { get; set; }
}

public class MouseMovedNotification(Vector3 delta, Vector3 globalPosition) : IMouseNotification
{
	public Vector3 GlobalDelta { get; } = delta;
	public Vector3 GlobalPosition { get; } = globalPosition;
	public bool Clipped { get; set; } = false;
}

public class MouseDownNotification(MouseButton button, Vector3 globalPosition) : IMouseNotification
{
	public MouseButton Button { get; } = button;
	public Vector3 GlobalPosition { get; } = globalPosition;
	public bool Clipped { get; set; } = false;
}

public class MouseUpNotification(MouseButton button, Vector3 globalPosition) : IMouseNotification
{
	public MouseButton Button { get; } = button;
	public Vector3 GlobalPosition { get; } = globalPosition;
	public bool Clipped { get; set; } = false;
}

public class MouseWheelScrolledNotification(Vector2 offset, Vector3 globalPosition) : IMouseNotification
{
	public Vector2 Offset { get; } = offset;
	public Vector3 GlobalPosition { get; } = globalPosition;
	public bool Clipped { get; set; } = false;
}

public class KeyDownNotification(Keys key)
{
	public Keys Key { get; } = key;
}

public class KeyUpNotification(Keys key)
{
	public Keys Key { get; } = key;
}

public class TextInputNotification(Keys key, KeyModifiers modifiers)
{
	public Keys Key { get; } = key;
	public KeyModifiers Modifiers { get; } = modifiers;
}

public class DisposeNotification;

public class DrawPassNotification;
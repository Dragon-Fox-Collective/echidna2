using Echidna2.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Vector2 = Echidna2.Mathematics.Vector2;

namespace Echidna2.Rendering;

public class Window
{
	public delegate void ResizeHandler(Vector2 size);
	public event ResizeHandler? Resize;
	
	private GameWindow window;
	
	public Camera? Camera { get; set; }
	
	private Vector2 mousePosition;
	
	public Window(GameWindow window)
	{
		this.window = window;
		
		window.Load += () =>
		{
			GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
			GL.Enable(EnableCap.DepthTest);
		};
		// window.Unload += world.Dispose;
		window.UpdateFrame += args => OnUpdate(args.Time);
		window.RenderFrame += _ => OnDraw();
		window.MouseMove += args => OnMouseMoved(args.Position, args.Delta);
		window.MouseDown += args => OnMouseDown(args.Button);
		window.MouseUp += args => OnMouseUp(args.Button);
		// window.KeyDown += args => world.KeyDown(args.Key);
		// window.KeyUp += args => world.KeyUp(args.Key);
		window.Resize += args => OnResize(args.Size);
	}
	
	private void OnUpdate(double deltaTime)
	{
		Camera?.Notify(new IPreUpdate.Notification());
		Camera?.Notify(new IUpdate.Notification(deltaTime));
	}
	
	private void OnDraw()
	{
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
		Camera?.Notify(new IDraw.Notification(Camera));
		window.SwapBuffers();
	}
	
	private void OnMouseMoved(Vector2 position, Vector2 delta)
	{
		mousePosition = position;
		Camera?.Notify(new IMouseMoved.Notification(position, delta));
	}
	
	private void OnMouseDown(MouseButton button)
	{
		Camera?.Notify(new IMouseDown.Notification(button, mousePosition));
	}
	
	private void OnMouseUp(MouseButton button)
	{
		Camera?.Notify(new IMouseUp.Notification(button, mousePosition));
	}
	
	private void OnResize(Vector2i size)
	{
		GL.Viewport(0, 0, size.X, size.Y);
		if (Camera is not null) Camera.Size = size;
		Resize?.Invoke(size.ToVector2());
	}
	
	public void Run() => window.Run();
}
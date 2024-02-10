using Echidna2.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;

namespace Echidna2.Rendering;

public class Window : IUpdate, IDraw
{
	private GameWindow window;
	
	public Camera? Camera { get; set; }
	
	public Window(GameWindow window)
	{
		this.window = window;
		
		window.Load += () =>
		{
			GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
			GL.Enable(EnableCap.DepthTest);
		};
		// window.Unload += world.Dispose;
		window.UpdateFrame += args =>
		{
			PreUpdate();
			OnUpdate(args.Time);
		};
		window.RenderFrame += _ => OnDraw();
		// gameWindow.MouseMove += args => world.MouseMove(args.Position, args.Delta);
		// gameWindow.KeyDown += args => world.KeyDown(args.Key);
		// gameWindow.KeyUp += args => world.KeyUp(args.Key);
		window.Resize += args =>
		{
			GL.Viewport(0, 0, args.Size.X, args.Size.Y);
			if (Camera is not null) Camera.Size = args.Size;
		};
	}
	
	public void PreUpdate() => Camera?.RenderPreUpdate();
	
	public void OnUpdate(double deltaTime) => Camera?.RenderUpdate(deltaTime);
	
	public void OnDraw()
	{
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
		Camera?.Render();
		window.SwapBuffers();
	}
	
	public void Run() => window.Run();
}
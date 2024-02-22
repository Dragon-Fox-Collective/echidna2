using Echidna2.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;
using Vector2 = Echidna2.Mathematics.Vector2;

namespace Echidna2.Rendering;

public class Window
{
	public delegate void ResizeHandler(Vector2 size);
	public event ResizeHandler? Resize;
	
	public GameWindow GameWindow;
	
	public Camera? Camera { get; set; }
	public PostProcessing? PostProcessing { get; set; }
	
	private Vector2 mousePosition;
	
	public Window(GameWindow gameWindow)
	{
		GameWindow = gameWindow;
		
		gameWindow.Load += () =>
		{
			GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
			GL.Enable(EnableCap.DepthTest);
		};
		// window.Unload += world.Dispose;
		gameWindow.UpdateFrame += args => OnUpdate(args.Time);
		gameWindow.RenderFrame += _ => OnDraw();
		gameWindow.MouseMove += args => OnMouseMoved(args.Position, new Vector2(args.DeltaX, -args.DeltaY));
		gameWindow.MouseDown += args => OnMouseDown(args.Button);
		gameWindow.MouseUp += args => OnMouseUp(args.Button);
		gameWindow.KeyDown += args => OnKeyDown(args.Key, args.IsRepeat);
		gameWindow.KeyUp += args => OnKeyUp(args.Key);
		gameWindow.Resize += args => OnResize(args.Size);
	}
	
	private void OnUpdate(double deltaTime)
	{
		Camera?.Notify(new IPreUpdate.Notification());
		Camera?.Notify(new IUpdate.Notification(deltaTime));
	}
	
	private void OnDraw()
	{
		if (Camera is null) return;
		GL.Viewport(0, 0, (int)Camera.Size.X, (int)Camera.Size.Y);
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
		PostProcessing?.BeginRender();
		Camera.Notify(new IDraw.Notification(Camera));
		PostProcessing?.EndRender();
		PostProcessing?.Render();
		GameWindow.SwapBuffers();
	}
	
	private void OnMouseMoved(Vector2 position, Vector2 delta)
	{
		mousePosition = position;
		Camera?.Notify(new IMouseMoved.Notification(mousePosition, delta, Camera.ScreenToGlobal(mousePosition)));
	}
	
	private void OnMouseDown(MouseButton button)
	{
		Camera?.Notify(new IMouseDown.Notification(button, mousePosition, Camera.ScreenToGlobal(mousePosition)));
	}
	
	private void OnMouseUp(MouseButton button)
	{
		Camera?.Notify(new IMouseUp.Notification(button, mousePosition, Camera.ScreenToGlobal(mousePosition)));
	}
	
	private void OnKeyDown(Keys key, bool isRepeat)
	{
		if (!isRepeat)
			Camera?.Notify(new IKeyDown.Notification(key));
	}
	
	private void OnKeyUp(Keys key)
	{
		Camera?.Notify(new IKeyUp.Notification(key));
	}
	
	private void OnResize(Vector2i size)
	{
		Resize?.Invoke(size.ToVector2());
	}
	
	public void Run() => GameWindow.Run();
	
	public static WindowIcon CreateWindowIcon(string path)
	{
		Image<Rgba32> image = (Image<Rgba32>)Image.Load(new DecoderOptions(), path);
		Span<byte> imageBytes = stackalloc byte[32 * 32 * 4];
		image.Frames.RootFrame.CopyPixelDataTo(imageBytes);
		return new WindowIcon(new OpenTK.Windowing.Common.Input.Image(image.Width, image.Height, imageBytes.ToArray()));
	}
}
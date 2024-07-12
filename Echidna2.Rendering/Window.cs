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
	
	private Vector3 globalMousePosition;
	
	public Window(GameWindow gameWindow)
	{
		GameWindow = gameWindow;
		
		gameWindow.Load += OnInitialize;
		gameWindow.Unload += OnDispose;
		gameWindow.UpdateFrame += args => OnUpdate(args.Time);
		gameWindow.RenderFrame += _ => OnDraw();
		gameWindow.MouseMove += args => OnMouseMoved(args.Position);
		gameWindow.MouseDown += args => OnMouseDown(args.Button);
		gameWindow.MouseUp += args => OnMouseUp(args.Button);
		gameWindow.MouseWheel += args => OnMouseWheel(args.Offset);
		gameWindow.KeyDown += args => OnKeyDown(args.Key, args.Modifiers, args.IsRepeat);
		gameWindow.KeyUp += args => OnKeyUp(args.Key);
		gameWindow.Resize += args => OnResize(args.Size);
	}
	
	private void OnInitialize()
	{
		GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
		GL.Enable(EnableCap.DepthTest);
		GL.Enable(EnableCap.DepthClamp);
		Camera?.Notify(new InitializeNotification());
	}
	
	private void OnDispose()
	{
		Camera?.Notify(new DisposeNotification());
	}
	
	private void OnUpdate(double deltaTime)
	{
		Camera?.Notify(new PreUpdateNotification());
		Camera?.Notify(new UpdateNotification(deltaTime));
		Camera?.Notify(new PostUpdateNotification());
	}
	
	private void OnDraw()
	{
		if (Camera is null) return;
		Camera.Notify(new DrawPassNotification());
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
		GL.Viewport(0, 0, (int)Camera.Size.X, (int)Camera.Size.Y);
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
		PostProcessing?.BeginRender();
		Camera.Notify(new DrawNotification(Camera));
		PostProcessing?.EndRender();
		PostProcessing?.Render();
		GameWindow.SwapBuffers();
	}
	
	private void OnMouseMoved(Vector2 position)
	{
		Vector3 newGlobalMousePosition = Camera.ScreenToGlobal(position);
		Vector3 delta = newGlobalMousePosition - globalMousePosition;
		globalMousePosition = newGlobalMousePosition;
		Camera?.Notify(new MouseMovedNotification(delta, globalMousePosition));
	}
	
	private void OnMouseDown(MouseButton button)
	{
		Camera?.Notify(new MouseDownNotification(button, globalMousePosition));
	}
	
	private void OnMouseUp(MouseButton button)
	{
		Camera?.Notify(new MouseUpNotification(button, globalMousePosition));
	}
	
	private void OnMouseWheel(Vector2 offset)
	{
		Camera?.Notify(new MouseWheelScrolledNotification(offset, globalMousePosition));
	}
	
	private void OnKeyDown(Keys key, KeyModifiers modifiers, bool isRepeat)
	{
		if (!isRepeat)
			Camera?.Notify(new KeyDownNotification(key));
		Camera?.Notify(new TextInputNotification(key, modifiers));
	}
	
	private void OnKeyUp(Keys key)
	{
		Camera?.Notify(new KeyUpNotification(key));
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
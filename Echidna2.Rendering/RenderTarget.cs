using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Serialization;
using OpenTK.Graphics.OpenGL4;

namespace Echidna2.Rendering;

public class RenderTarget : IInitialize, INotificationHook<DrawPass_Notification>, IDispose
{
	private int colorTexture;
	private int depthTexture;
	private int frameBufferObject;
	
	private Vector2 currentSize;
	
	public int ColorTexture => colorTexture;
	
	[SerializedReference] public IHasCamera CameraHaver = null!;
	public Camera Camera => CameraHaver.HavedCamera;
	
	[DontExpose] public bool HasBeenInitialized { get; set; }
	
	public void OnInitialize()
	{
		colorTexture = GL.GenTexture();
		depthTexture = GL.GenTexture();
		frameBufferObject = GL.GenFramebuffer();
		
		GenerateTextures();
		GenerateFrameBufferObject();
	}
	
	public void GenerateTextures()
	{
		GL.BindTexture(TextureTarget.Texture2D, colorTexture);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
		GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, (int)Camera.Size.X, (int)Camera.Size.Y, 0, PixelFormat.Rgb, PixelType.UnsignedByte, (IntPtr)null);
		
		GL.BindTexture(TextureTarget.Texture2D, depthTexture);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
		GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, (int)Camera.Size.X, (int)Camera.Size.Y, 0, PixelFormat.DepthComponent, PixelType.UnsignedByte, (IntPtr)null);
		
		GL.BindTexture(TextureTarget.Texture2D, 0);
	}
	
	public void GenerateFrameBufferObject()
	{
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferObject);
		GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTexture, 0);
		GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthTexture, 0);
		if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
			throw new Exception("Framebuffer is not complete!");
		
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
	}
	
	public void OnPreNotify(DrawPass_Notification notification)
	{
		
	}
	public void OnPostNotify(DrawPass_Notification notification)
	{
		
	}
	public void OnPostPropagate(DrawPass_Notification notification)
	{
		if (currentSize != Camera.Size)
		{
			currentSize = Camera.Size;
			GenerateTextures();
		}
		
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferObject);
		GL.Viewport(0, 0, (int)Camera.Size.X, (int)Camera.Size.Y);
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
		Camera.Notify(new Draw_Notification(Camera));
	}
	
	public void OnDispose()
	{
		GL.DeleteTexture(colorTexture);
		GL.DeleteTexture(depthTexture);
		GL.DeleteFramebuffer(frameBufferObject);
	}
}
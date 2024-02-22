using Echidna2.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace Echidna2.Rendering;

public class PostProcessing(Shader shader, Vector2 size, Camera camera)
{
	public Shader Shader { get; set; } = shader;
	
	public Vector2 Size
	{
		get => size;
		set
		{
			size = value;
			if (hasBeenInitialized)
				RegenerateTextures();
		}
	}
	
	public Camera Camera { get; set; } = camera;
	
	private int colorTexture;
	private int depthTexture;
	private int frameBufferObject;
	private int renderBufferObject;
	
	private bool hasBeenInitialized;
	private bool hasBeenDisposed;
	
	private void Initialize()
	{
		hasBeenInitialized = true;
		
		colorTexture = GL.GenTexture();
		depthTexture = GL.GenTexture();
		frameBufferObject = GL.GenFramebuffer();
		renderBufferObject = GL.GenRenderbuffer();
		
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferObject);
		
		RegenerateTextures();
		
		GL.BindTexture(TextureTarget.Texture2D, colorTexture);
		GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTexture, 0);
		GL.BindTexture(TextureTarget.Texture2D, depthTexture);
		GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthTexture, 0);
		GL.BindTexture(TextureTarget.Texture2D, 0);
		
		if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
			throw new Exception("Framebuffer is not complete!");
		
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
	}
	
	private void RegenerateTextures()
	{
		GL.BindTexture(TextureTarget.Texture2D, colorTexture);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
		GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, (int)Camera.Size.X, (int)Camera.Size.Y, 0, PixelFormat.Rgb, PixelType.UnsignedByte, (IntPtr)null);
		
		GL.BindTexture(TextureTarget.Texture2D, depthTexture);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
		GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, (int)Camera.Size.X, (int)Camera.Size.Y, 0, PixelFormat.DepthComponent, PixelType.UnsignedByte, (IntPtr)null);
		
		GL.BindTexture(TextureTarget.Texture2D, 0);
	}
	
	public void BeginRender()
	{
		if (!hasBeenInitialized)
			Initialize();
		
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferObject);
		GL.Viewport(0, 0, (int)Camera.Size.X, (int)Camera.Size.Y);
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
	}
	
	public void EndRender()
	{
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
		GL.Viewport(0, 0, (int)Size.X, (int)Size.Y);
	}
	
	public void Render()
	{
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
		GL.Disable(EnableCap.DepthTest);
		Shader.Bind();
		GL.ActiveTexture(TextureUnit.Texture0);
		GL.BindTexture(TextureTarget.Texture2D, colorTexture);
		Shader.SetInt("colorTexture", 0);
		GL.ActiveTexture(TextureUnit.Texture1);
		GL.BindTexture(TextureTarget.Texture2D, depthTexture);
		Shader.SetInt("depthTexture", 1);
		Shader.SetFloat("nearClipPlane", (float)Camera.NearClipPlane);
		Shader.SetFloat("farClipPlane", (float)Camera.FarClipPlane);
		Mesh.Quad.Draw();
		GL.Enable(EnableCap.DepthTest);
	}
	
	public void Dispose()
	{
		hasBeenDisposed = true;
		
		GL.DeleteTexture(colorTexture);
		GL.DeleteTexture(depthTexture);
		GL.DeleteFramebuffer(frameBufferObject);
		GL.DeleteRenderbuffer(renderBufferObject);
	}
	
	~PostProcessing()
	{
		if (!hasBeenDisposed)
			Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
	}
}
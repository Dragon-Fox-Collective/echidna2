using Echidna2.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace Echidna2.Rendering;

public class PostProcessing(Shader shader, Shader blurShader, Vector2 size, Camera camera)
{
	public Shader Shader { get; set; } = shader;
	public Shader BlurShader { get; set; } = blurShader;
	
	public Vector2 Size
	{
		get => size;
		set
		{
			size = value;
			if (hasBeenInitialized)
				GenerateTextures();
		}
	}
	
	public Camera Camera { get; set; } = camera;
	
	private int colorTexture;
	private int brightTexture;
	private int depthTexture;
	
	private int frameBufferObject;
	private int renderBufferObject;
	
	private int[] pingPongFrameBufferObjects = [-1, -1];
	private int[] pingPongTextures = [-1, -1];
	private const int BlurAmount = 5;
	
	private bool hasBeenInitialized;
	private bool hasBeenDisposed;
	
	private void Initialize()
	{
		hasBeenInitialized = true;
		
		colorTexture = GL.GenTexture();
		brightTexture = GL.GenTexture();
		depthTexture = GL.GenTexture();
		
		frameBufferObject = GL.GenFramebuffer();
		renderBufferObject = GL.GenRenderbuffer();
		
		GL.GenFramebuffers(2, pingPongFrameBufferObjects);
		GL.GenTextures(2, pingPongTextures);
		
		GenerateTextures();
		GenerateFrameBufferObjects();
	}
	
	private void GenerateTextures()
	{
		GL.BindTexture(TextureTarget.Texture2D, colorTexture);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
		GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, (int)Camera.Size.X, (int)Camera.Size.Y, 0, PixelFormat.Rgb, PixelType.UnsignedByte, (IntPtr)null);
		
		GL.BindTexture(TextureTarget.Texture2D, brightTexture);
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
		
		for (int i = 0; i < 2; i++)
		{
			GL.BindTexture(TextureTarget.Texture2D, pingPongTextures[i]);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, (int)Camera.Size.X, (int)Camera.Size.Y, 0, PixelFormat.Rgb, PixelType.UnsignedByte, (IntPtr)null);
		}
		
		GL.BindTexture(TextureTarget.Texture2D, 0);
	}
	
	public void GenerateFrameBufferObjects()
	{
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferObject);
		GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTexture, 0);
		GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, brightTexture, 0);
		GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthTexture, 0);
		if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
			throw new Exception("Framebuffer is not complete!");
		
		for (int i = 0; i < 2; i++)
		{
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, pingPongFrameBufferObjects[i]);
			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, pingPongTextures[i], 0);
			if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
				throw new Exception("Framebuffer is not complete!");
		}
		
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
	}
	
	public void BeginRender()
	{
		if (!hasBeenInitialized)
			Initialize();
		
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferObject);
		GL.DrawBuffers(2, [DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1]);
		GL.Viewport(0, 0, (int)Camera.Size.X, (int)Camera.Size.Y);
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
	}
	
	public void EndRender()
	{
		GL.Viewport(0, 0, (int)Size.X, (int)Size.Y);
	}
	
	public void Render()
	{
		GL.Disable(EnableCap.DepthTest);
		RenderBlurShader();
		RenderMainShader();
		GL.Enable(EnableCap.DepthTest);
	}
	
	public void RenderBlurShader()
	{
		GL.DrawBuffers(1, [DrawBuffersEnum.ColorAttachment0]);
		
		BlurShader.Bind();
		
		bool horizontal = true;
		for (int i = 0; i < BlurAmount * 2; i++, horizontal = !horizontal)
		{
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, pingPongFrameBufferObjects[horizontal ? 1 : 0]);
			BlurShader.SetInt("horizontal", horizontal ? 1 : 0);
			GL.BindTexture(TextureTarget.Texture2D, i == 0 ? brightTexture : pingPongTextures[horizontal ? 0 : 1]);
			Mesh.Quad.Draw();
		}
	}
	
	public void RenderMainShader()
	{
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
		GL.DrawBuffers(1, [DrawBuffersEnum.ColorAttachment0]);
		
		Shader.Bind();
		
		GL.ActiveTexture(TextureUnit.Texture0);
		GL.BindTexture(TextureTarget.Texture2D, colorTexture);
		Shader.SetInt("colorTexture", 0);
		
		GL.ActiveTexture(TextureUnit.Texture1);
		GL.BindTexture(TextureTarget.Texture2D, brightTexture);
		Shader.SetInt("brightTexture", 1);
		
		GL.ActiveTexture(TextureUnit.Texture2);
		GL.BindTexture(TextureTarget.Texture2D, pingPongTextures[BlurAmount % 2]);
		Shader.SetInt("bloomTexture", 2);
		
		GL.ActiveTexture(TextureUnit.Texture3);
		GL.BindTexture(TextureTarget.Texture2D, depthTexture);
		Shader.SetInt("depthTexture", 3);
		
		Shader.SetFloat("nearClipPlane", (float)Camera.NearClipPlane);
		Shader.SetFloat("farClipPlane", (float)Camera.FarClipPlane);
		
		Mesh.Quad.Draw();
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
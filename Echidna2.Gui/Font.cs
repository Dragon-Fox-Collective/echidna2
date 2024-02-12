using OpenTK.Graphics.OpenGL4;
using StbSharp.MonoGame.Test;

namespace Echidna2.Gui;

public class Font(string path)
{
	public const int TextureSize = 1024;
	
	private int textureHandle;
	private int vertexBufferObject;
	private int vertexArrayObject;
	public static readonly float[] Vertices =
	[
		0.0f, 1.0f, 0.0f,   0.0f, 0.0f,   0.0f, 0.0f, 0.0f,
		0.0f, 0.0f, 0.0f,   0.0f, 1.0f,   1.0f, 0.0f, 0.0f,
		1.0f, 0.0f, 0.0f,   1.0f, 1.0f,   0.0f, 0.0f, 1.0f,
		
		0.0f, 1.0f, 0.0f,   0.0f, 0.0f,   0.0f, 0.0f, 0.0f,
		1.0f, 0.0f, 0.0f,   1.0f, 1.0f,   0.0f, 0.0f, 1.0f,
		1.0f, 1.0f, 0.0f,   1.0f, 0.0f,   0.0f, 1.0f, 0.0f
	];
	
	public FontBakerResult? FontResult { get; private set; }
	
	private bool hasBeenInitialized;
	private bool hasBeenDisposed;
	
	public void Bind(TextureUnit unit = TextureUnit.Texture0)
	{
		if (!hasBeenInitialized)
			Initialize();
		
		GL.ActiveTexture(unit);
		GL.BindTexture(TextureTarget.Texture2D, textureHandle);
		GL.BindVertexArray(vertexArrayObject);
		GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
	}
	
	private void Initialize()
	{
		hasBeenInitialized = true;
		FontResult = GenerateFont(path);
		textureHandle = GenerateFontTexture(FontResult);
		GenerateFontMesh();
	}
	
	private static FontBakerResult GenerateFont(string path)
	{
		FontBaker fontBaker = new();
		
		fontBaker.Begin(TextureSize, TextureSize);
		fontBaker.Add(File.ReadAllBytes(path), 32, new[]
		{
			CharacterRange.BasicLatin
		});
		return fontBaker.End();
	}
	
	private static int GenerateFontTexture(FontBakerResult font)
	{
		GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
		
		int handle = GL.GenTexture();
		GL.BindTexture(TextureTarget.Texture2D, handle);
		
		GL.TexImage2D(TextureTarget.Texture2D, 0, (PixelInternalFormat)PixelFormat.Red, font.Width, font.Height, 0, PixelFormat.Red, PixelType.UnsignedByte, font.Bitmap);
		
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
		
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
		
		GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
		
		return handle;
	}
	
	private void GenerateFontMesh()
	{
		vertexBufferObject = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
		
		vertexArrayObject = GL.GenVertexArray();
		GL.BindVertexArray(vertexArrayObject);
		
		int[] widths = [3, 2, 3];
		int stride = widths.Sum();
		for (int attribute = 0, offset = 0; attribute < widths.Length; offset += widths[attribute], attribute++)
		{
			GL.EnableVertexAttribArray(attribute);
			GL.VertexAttribPointer(attribute, widths[attribute], VertexAttribPointerType.Float, false, stride * sizeof(float), offset * sizeof(float));
		}
		
		GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 6 * stride, IntPtr.Zero, BufferUsageHint.DynamicDraw);
	}
	
	public void Dispose()
	{
		hasBeenDisposed = true;
		GL.DeleteTexture(textureHandle);
	}
	
	~Font()
	{
		if (!hasBeenDisposed)
			Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
	}
}
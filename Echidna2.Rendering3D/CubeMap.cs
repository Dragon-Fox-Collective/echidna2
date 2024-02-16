using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace Echidna2.Rendering3D;

public class CubeMap(string rightPath, string leftPath, string forwardPath, string backPath, string upPath, string downPath)
{
	public static readonly CubeMap Skybox = new(
		"Assets/Skybox/right.png",
		"Assets/Skybox/left.png",
		"Assets/Skybox/front.png",
		"Assets/Skybox/back.png",
		"Assets/Skybox/top.png",
		"Assets/Skybox/bottom.png"
	);
	
	private int handle;
	
	private bool hasBeenInitialized;
	private bool hasBeenDisposed;
	
	public void Bind(TextureUnit unit = TextureUnit.Texture0)
	{
		if  (!hasBeenInitialized)
			Initialize();
		
		GL.ActiveTexture(unit);
		GL.BindTexture(TextureTarget.TextureCubeMap, handle);
	}
	
	private void Initialize()
	{
		hasBeenInitialized = true;
		
		handle = GL.GenTexture();
		Bind();
		
		StbImage.stbi_set_flip_vertically_on_load(1);
		for (int i = 0; i < 6; i++)
		{
			string path = i switch
			{
				0 => rightPath,
				1 => leftPath,
				2 => forwardPath,
				3 => backPath,
				4 => upPath,
				5 => downPath,
				_ => throw new InvalidOperationException("For loop broke")
			};
			using Stream textureStream = File.OpenRead(path);
			ImageResult image = ImageResult.FromStream(textureStream, ColorComponents.RedGreenBlueAlpha);
			GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
		}
		
		GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
		GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
		
		GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
		GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
		GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
	}
	
	public void Dispose()
	{
		hasBeenDisposed = true;
		GL.DeleteTexture(handle);
	}
	
	~CubeMap()
	{
		if (!hasBeenDisposed)
			Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
	}
}
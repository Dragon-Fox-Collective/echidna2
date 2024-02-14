using System.Drawing;
using Echidna2.Mathematics;
using OpenTK.Graphics.OpenGL4;
using Vector4 = OpenTK.Mathematics.Vector4;

namespace Echidna2.Rendering;

public class Shader(string vertexSource, string fragmentSource)
{
	private int handle;
	
	private readonly Dictionary<string, int> uniforms = new();
	
	private bool hasBeenInitialized;
	private bool hasBeenDisposed;
	
	public void Bind(Matrix4 viewMatrix, Matrix4 projectionMatrix)
	{
		if (!hasBeenInitialized)
			Initialize();
		
		GL.UseProgram(handle);
		SetMatrix4("view", viewMatrix);
		SetMatrix4("projection", projectionMatrix);
	}
	
	private void Initialize()
	{
		hasBeenInitialized = true;
		
		int vertexShader = CompileShader(vertexSource, ShaderType.VertexShader);
		int fragmentShader = CompileShader(fragmentSource, ShaderType.FragmentShader);
		
		handle = GL.CreateProgram();
		GL.AttachShader(handle, vertexShader);
		GL.AttachShader(handle, fragmentShader);
		GL.LinkProgram(handle);
		
		GL.GetProgram(handle, GetProgramParameterName.LinkStatus, out int success);
		if (success == 0)
		{
			string infoLog = GL.GetProgramInfoLog(handle);
			Console.WriteLine(infoLog);
		}
		
		GL.DetachShader(handle, vertexShader);
		GL.DeleteShader(vertexShader);
		GL.DetachShader(handle, fragmentShader);
		GL.DeleteShader(fragmentShader);
		
		GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, out int numberOfUniforms);
		for (int i = 0; i < numberOfUniforms; i++)
		{
			string key = GL.GetActiveUniform(handle, i, out _, out _);
			int location = GL.GetUniformLocation(handle, key);
			uniforms.Add(key, location);
		}
	}
	
	private static int CompileShader(string source, ShaderType type)
	{
		int shader = GL.CreateShader(type);
		GL.ShaderSource(shader, source);
		GL.CompileShader(shader);
		
		GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
		if (success == 0)
		{
			string infoLog = GL.GetShaderInfoLog(shader);
			Console.WriteLine(infoLog);
		}
		
		return shader;
	}
	
	public int GetAttribLocation(string attribName) => GL.GetAttribLocation(handle, attribName);
	public int GetUniformLocation(string uniformName) => uniforms[uniformName];
	public void SetInt(string name, int data) => GL.Uniform1(GetUniformLocation(name), data);
	public void SetFloat(string name, float data) => GL.Uniform1(GetUniformLocation(name), data);
	public void SetMatrix4(string name, Matrix4 data)
	{
		OpenTK.Mathematics.Matrix4 openTKData = data;
		GL.UniformMatrix4(GetUniformLocation(name), false, ref openTKData);
	}
	public void SetMatrix4(int location, Matrix4 data)
	{
		OpenTK.Mathematics.Matrix4 openTKData = data;
		GL.UniformMatrix4(location, false, ref openTKData);
	}
	public void SetVector3(string name, Vector3 data) => GL.Uniform3(GetUniformLocation(name), data);
	public void SetVector4(string name, Vector4 data) => GL.Uniform4(GetUniformLocation(name), data);
	public void SetColor(string name, Color data) => GL.Uniform4(GetUniformLocation(name), data);
	
	public void Dispose()
	{
		hasBeenDisposed = true;
		GL.DeleteProgram(handle);
	}
	
	~Shader()
	{
		if (!hasBeenDisposed)
			Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
	}
}
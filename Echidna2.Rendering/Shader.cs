﻿using System.Drawing;
using Echidna2.Mathematics;
using Echidna2.Serialization;
using OpenTK.Graphics.OpenGL4;
using Vector4 = OpenTK.Mathematics.Vector4;

namespace Echidna2.Rendering;

public class Shader(string vertexSource, string fragmentSource)
{
	static Shader() => SerializedValueAttribute.AddDefaultSerializer(typeof(Shader), new ShaderSerializer());
	
	public static readonly Shader Solid = new(ShaderNodeUtil.MainVertexShader, File.ReadAllText($"{AppContext.BaseDirectory}/Assets/solid.frag"));
	public static readonly Shader PBR = new(ShaderNodeUtil.MainVertexShader, File.ReadAllText($"{AppContext.BaseDirectory}/Assets/pbr.frag"));
	public static readonly Shader Skybox = new(ShaderNodeUtil.SkyboxVertexShader, File.ReadAllText($"{AppContext.BaseDirectory}/Assets/cubemap.frag"));
	public static readonly Shader Quad = new(File.ReadAllText($"{AppContext.BaseDirectory}/Assets/quad.vert"), File.ReadAllText($"{AppContext.BaseDirectory}/Assets/solid_texture.frag"));
	
	private int handle;
	
	private readonly Dictionary<string, int> uniforms = new();
	
	private bool hasBeenInitialized;
	private bool hasBeenDisposed;
	
	public void Bind()
	{
		if (!hasBeenInitialized)
			Initialize();
		
		GL.UseProgram(handle);
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
	public int GetUniformLocation(string uniformName) => uniforms.GetValueOrDefault(uniformName, -1);
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
	public void SetColorRgba(string name, Color data) => GL.Uniform4(GetUniformLocation(name), data);
	public void SetColorRgb(string name, Color data) => GL.Uniform3(GetUniformLocation(name), data.R / 255f, data.G / 255f, data.B / 255f);
	
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
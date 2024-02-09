using Echidna2.Core;
using OpenTK.Graphics.OpenGL4;

namespace Echidna2.Rendering;

public class Mesh(float[] positions, float[] texCoords, float[] colors, uint[] indices, bool cullBackFaces = true)
{
	internal const int Dims = 3;
	
	public int NumVertices => positions.Length / Dims;
	
	internal bool IsDirty = true;

	public float[] Positions
	{
		get => positions;
		set
		{
			positions = value;
			IsDirty = true;
		}
	}

	public float[] TexCoords
	{
		get => texCoords;
		set
		{
			texCoords = value;
			IsDirty = true;
		}
	}

	public float[] Colors
	{
		get => colors;
		set
		{
			colors = value;
			IsDirty = true;
		}
	}

	public uint[] Indices
	{
		get => indices;
		set
		{
			indices = value;
			IsDirty = true;
		}
	}
	
	internal float[] Data = Array.Empty<float>();
	
	internal int VertexBufferObject;
	internal int ElementBufferObject;
	internal int VertexArrayObject;
	
	internal readonly bool CullBackFaces = cullBackFaces;
	
	internal bool HasBeenDisposed;
	
	public void Initialize()
	{
		VertexBufferObject = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
		
		VertexArrayObject = GL.GenVertexArray();
		GL.BindVertexArray(VertexArrayObject);
		
		int[] widths = [3, 2, 3];
		int stride = widths.Sum();
		for (int attribute = 0, offset = 0; attribute < widths.Length; offset += widths[attribute], attribute++)
		{
			GL.EnableVertexAttribArray(attribute);
			GL.VertexAttribPointer(attribute, widths[attribute], VertexAttribPointerType.Float, false, stride * sizeof(float), offset * sizeof(float));
		}
		
		ElementBufferObject = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
	}
	
	public void CleanIfDirty()
	{
		if (IsDirty)
			Clean();
	}
	
	public void Clean()
	{
		IsDirty = false;
		RegenerateData();
		BindData(VertexBufferObject, Data);
		BindIndices(ElementBufferObject, Indices);
	}
	
	private void RegenerateData()
	{
		float[][] datasets = [Positions, TexCoords, Colors];
		int[] widths = [3, 2, 3];
		int stride = widths.Sum();
		Data = new float[datasets.Sum(data => data.Length)];
		
		for (int i = 0; i < NumVertices; i++)
		for (int dataset = 0, offset = 0; dataset < datasets.Length; offset += widths[dataset], dataset++)
		for (int x = 0; x < widths[dataset]; x++)
			Data[i * stride + offset + x] = datasets[dataset][i * widths[dataset] + x];
	}
	
	private static void BindData(int vertexBufferObject, float[] data)
	{
		GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
		GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.StaticDraw);
	}
	
	private static void BindIndices(int elementBufferObject, uint[] indices)
	{
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
		GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
	}
	
	public void Dispose()
	{
		HasBeenDisposed = true;
		GL.DeleteBuffer(VertexBufferObject);
		GL.DeleteVertexArray(VertexArrayObject);
		GL.DeleteBuffer(ElementBufferObject);
	}
	
	~Mesh()
	{
		if (!HasBeenDisposed)
			Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
	}
}
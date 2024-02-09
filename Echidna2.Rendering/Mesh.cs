using OpenTK.Graphics.OpenGL4;

namespace Echidna2.Rendering;

public class Mesh(float[] positions, float[] texCoords, float[] colors, uint[] indices, bool cullBackFaces = true)
{
	private const int Dims = 3;
	
	public int NumVertices => positions.Length / Dims;
	
	private bool isDirty = true;
	private bool hasBeenInitialized;
	private bool hasBeenDisposed;
	
	public float[] Positions
	{
		get => positions;
		set
		{
			positions = value;
			isDirty = true;
		}
	}
	
	public float[] TexCoords
	{
		get => texCoords;
		set
		{
			texCoords = value;
			isDirty = true;
		}
	}
	
	public float[] Colors
	{
		get => colors;
		set
		{
			colors = value;
			isDirty = true;
		}
	}
	
	public uint[] Indices
	{
		get => indices;
		set
		{
			indices = value;
			isDirty = true;
		}
	}
	
	private float[] data = Array.Empty<float>();
	
	private int vertexBufferObject;
	private int elementBufferObject;
	private int vertexArrayObject;
	
	public void Draw()
	{
		if (isDirty)
			Clean();
		
		if (cullBackFaces)
			GL.Enable(EnableCap.CullFace);
		else
			GL.Disable(EnableCap.CullFace);
		
		GL.Disable(EnableCap.Blend);
		
		GL.BindVertexArray(vertexArrayObject);
		GL.DrawElements(PrimitiveType.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);
	}
	
	private void Clean()
	{
		isDirty = false;
		if (!hasBeenInitialized)
			Initialize();
		RegenerateData();
		BindData(vertexBufferObject, data);
		BindIndices(elementBufferObject, Indices);
	}
	
	private void Initialize()
	{
		hasBeenInitialized = true;
		
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
		
		elementBufferObject = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
	}
	
	private void RegenerateData()
	{
		float[][] datasets = [Positions, TexCoords, Colors];
		int[] widths = [3, 2, 3];
		int stride = widths.Sum();
		data = new float[datasets.Sum(data => data.Length)];
		
		for (int i = 0; i < NumVertices; i++)
		for (int dataset = 0, offset = 0; dataset < datasets.Length; offset += widths[dataset], dataset++)
		for (int x = 0; x < widths[dataset]; x++)
			data[i * stride + offset + x] = datasets[dataset][i * widths[dataset] + x];
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
		hasBeenDisposed = true;
		GL.DeleteBuffer(vertexBufferObject);
		GL.DeleteVertexArray(vertexArrayObject);
		GL.DeleteBuffer(elementBufferObject);
	}
	
	~Mesh()
	{
		if (!hasBeenDisposed)
			Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
	}
}
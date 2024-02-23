using ObjLoader.Loader.Loaders;
using OpenTK.Graphics.OpenGL4;

namespace Echidna2.Rendering;

public class Mesh(float[] positions, float[] normals, float[] texCoords, float[] colors, uint[] indices)
{
	private const int Dims = 3;
	
	public static readonly Mesh Triangle = new([
		+0.5f, -0.5f, +0.0f,
		-0.5f, -0.5f, +0.0f,
		+0.0f, +0.5f, +0.0f,
	], [
		+0.0f, +0.0f, +1.0f,
		+0.0f, +0.0f, +1.0f,
		+0.0f, +0.0f, +1.0f,
	], [
		1.0f, 0.0f,
		0.0f, 0.0f,
		0.5f, 1.0f,
	], [
		1.0f, 0.0f, 0.0f,
		0.0f, 1.0f, 0.0f,
		0.0f, 0.0f, 1.0f,
	], [
		0, 1, 2,
	]) { CullBackFaces = false };
	public static readonly Mesh Quad = new([
		-1.0f, -1.0f, +0.0f,
		+1.0f, -1.0f, +0.0f,
		-1.0f, +1.0f, +0.0f,
		+1.0f, +1.0f, +0.0f,
	], [
		+0.0f, +0.0f, +1.0f,
		+0.0f, +0.0f, +1.0f,
		+0.0f, +0.0f, +1.0f,
		+0.0f, +0.0f, +1.0f,
	], [
		0.0f, 0.0f,
		1.0f, 0.0f,
		0.0f, 1.0f,
		1.0f, 1.0f,
	], [
		0.0f, 0.0f, 0.0f,
		1.0f, 0.0f, 0.0f,
		0.0f, 1.0f, 0.0f,
		0.0f, 0.0f, 1.0f,
	], [
		0, 1, 2,
		2, 1, 3,
	]) { CullBackFaces = false };
	public static readonly Mesh Cube = FromObj($"{AppContext.BaseDirectory}/Assets/cube.obj");
	public static readonly Mesh Sphere = FromObj($"{AppContext.BaseDirectory}/Assets/sphere.obj");
	
	public int NumVertices => Positions.Length / Dims;
	
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
	
	public float[] Normals
	{
		get => normals;
		set
		{
			normals = value;
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
	
	public bool CullBackFaces { get; set; } = true;
	
	private float[] data = Array.Empty<float>();
	
	private int vertexBufferObject;
	private int elementBufferObject;
	private int vertexArrayObject;
	
	public struct ObjVertex
	{
		public float X;
		public float Y;
		public float Z;
		public float NormalX;
		public float NormalY;
		public float NormalZ;
		public float U;
		public float V;
	}
	public static Mesh FromObj(string filename)
	{
		using Stream fileStream = File.OpenRead(filename);
		LoadResult result = new ObjLoaderFactory().Create().Load(fileStream);
			
		List<ObjVertex> uniqueVertices = []; // this is probably not going to hold up, figure out a set with insertion order
		uint[] faces = result.Groups
			.SelectMany(group => group.Faces
				.SelectMany(face => Enumerable.Range(0, face.Count)
					.Select(i =>
					{
						ObjLoader.Loader.Data.VertexData.Vertex vertex = result.Vertices[face[i].VertexIndex - 1];
						ObjLoader.Loader.Data.VertexData.Normal normal = result.Normals[face[i].NormalIndex - 1];
						ObjLoader.Loader.Data.VertexData.Texture texCoord = result.Textures[face[i].TextureIndex - 1];
						uniqueVertices.Add(new ObjVertex
						{
							X = vertex.X,
							Y = vertex.Y,
							Z = vertex.Z,
							NormalX = normal.X,
							NormalY = normal.Y,
							NormalZ = normal.Z,
							U = texCoord.X,
							V = texCoord.Y,
						});
						return (uint)uniqueVertices.Count - 1;
					})
				)
			).ToArray();
			
		return new Mesh(
			uniqueVertices.SelectMany(vertex => EnumerableOf.Of(vertex.X, vertex.Y, vertex.Z)).ToArray(),
			uniqueVertices.SelectMany(vertex => EnumerableOf.Of(vertex.NormalX, vertex.NormalY, vertex.NormalZ)).ToArray(),
			uniqueVertices.SelectMany(vertex => EnumerableOf.Of(vertex.U, vertex.V)).ToArray(),
			uniqueVertices.SelectMany(_ => EnumerableOf.Of(1f, 1f, 1f)).ToArray(),
			faces);
	}
	
	public void Draw()
	{
		if (isDirty)
			Clean();
		
		if (CullBackFaces)
			GL.Enable(EnableCap.CullFace);
		else
			GL.Disable(EnableCap.CullFace);
		
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
		
		int[] widths = [3, 3, 2, 3];
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
		float[][] datasets = [Positions, Normals, TexCoords, Colors];
		int[] widths = [3, 3, 2, 3];
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
namespace Echidna2.Rendering;

public static class RenderingSerialization
{
	public static Mesh DeserializeMesh(string type)
	{
		return type switch
		{
			"Sphere" => Mesh.Sphere,
			"Cube" => Mesh.Cube,
			_ => throw new InvalidOperationException($"Mesh type {type} does not exist")
		};
	}
}
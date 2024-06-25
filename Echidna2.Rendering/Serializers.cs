using Echidna2.Serialization;

namespace Echidna2.Rendering;

public class MeshSerializer : Serializer<string, Mesh>
{
	public string Serialize(Mesh value) => throw new InvalidOperationException("Cannot serialize mesh");
	
	public Mesh Deserialize(Mesh? value, string data) => data switch
	{
		"Sphere" => Mesh.Sphere,
		"Cube" => Mesh.Cube,
		_ => throw new InvalidOperationException($"Mesh type {data} does not exist")
	};
}

public class ShaderSerializer : Serializer<string, Shader>
{
	public string Serialize(Shader value) => throw new InvalidOperationException("Cannot serialize shader");
	
	public Shader Deserialize(Shader? value, string data) => data switch
	{
		"Solid" => Shader.Solid,
		"PBR" => Shader.PBR,
		"Skybox" => Shader.Skybox,
		"Quad" => Shader.Quad,
		_ => throw new InvalidOperationException($"Shader type {data} does not exist")
	};
}
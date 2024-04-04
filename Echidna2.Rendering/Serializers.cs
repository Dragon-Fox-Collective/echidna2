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
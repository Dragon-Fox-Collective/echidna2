using Echidna2.Serialization;

namespace Echidna2.Rendering3D;

public class CubeMapSerializer : Serializer<string, CubeMap>
{
	public string Serialize(CubeMap value) => throw new InvalidOperationException("Cannot serialize cubemap");
	
	public CubeMap Deserialize(CubeMap? value, string data) => data switch
	{
		"Skybox" => CubeMap.Skybox,
		_ => throw new InvalidOperationException($"Cubemap type {data} does not exist")
	};
}
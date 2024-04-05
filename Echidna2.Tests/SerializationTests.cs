using Echidna2.Mathematics;
using Echidna2.Rendering3D;
using Echidna2.Serialization;
using Echidna2.TestPrefabs;

namespace Echidna2.Tests;

public class SerializationTests
{
	[Fact]
	public void DeserializingComponent_SetsValues()
	{
		// Arrange
		
		// Act
		Transform3D transform = (Transform3D)TomlDeserializer.Deserialize(AppContext.BaseDirectory + "Prefabs/TransformTest.toml").RootObject;
		
		// Assert
		Assert.Equal(new Vector3(0.0, 0.0, 2.5), transform.LocalPosition);
	}
	
	[Fact]
	public void DeserializingComponent_SetsReferences()
	{
		// Arrange
		
		// Act
		TransformPrefab transform = (TransformPrefab)TomlDeserializer.Deserialize(AppContext.BaseDirectory + "Prefabs/TransformPrefabTest.toml").RootObject;
		
		// Assert
		Assert.NotNull(transform.Transform);
	}
}
using Echidna2.Mathematics;
using Echidna2.Rendering3D;
using Echidna2.Serialization;
using Echidna2.Prefabs.Test;
using Tomlyn.Model;

namespace Echidna2.SerializationTests;

public static class SerializationTests
{
	[Fact]
	public static void DeserializingComponent_SetsValues()
	{
		// Arrange
		
		// Act
		Transform3D transform = (Transform3D)TomlDeserializer.Deserialize(AppContext.BaseDirectory + "Prefabs/Test/TransformTest.toml").RootObject;
		
		// Assert
		Assert.Equal(new Vector3(0.0, 0.0, 2.5), transform.LocalPosition);
	}
	
	[Fact]
	public static void DeserializingComponent_SetsReferences()
	{
		// Arrange
		
		// Act
		TransformPrefab transform = (TransformPrefab)TomlDeserializer.Deserialize(AppContext.BaseDirectory + "Prefabs/Test/TransformPrefabTest.toml").RootObject;
		
		// Assert
		Assert.NotNull(transform.Transform);
	}
	
	[Fact]
	public static void ReserializingComponent_WithReferencesOnSubComponent_SetsReferencesOnlyOnSubComponent()
	{
		// Arrange
		
		// Act
		PrefabRoot prefab = TomlDeserializer.Deserialize(AppContext.BaseDirectory + "Prefabs/Test/SubcomponentPrefabTest.toml");
		TomlTable table = TomlSerializer.Serialize(prefab, AppContext.BaseDirectory + "Prefabs/Test/SubcomponentPrefabTest_Result.toml");
		
		// Assert
		Assert.False(((TomlTable)table["0"]).ContainsKey("Reference"));
		Assert.True(((TomlTable)table["1"]).ContainsKey("Reference"));
		Assert.False(((TomlTable)table["2"]).ContainsKey("Reference"));
	}
}
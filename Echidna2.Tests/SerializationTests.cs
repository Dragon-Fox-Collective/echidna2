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
		Transform3D transform = Project.Instantiate<Transform3D>(AppContext.BaseDirectory + "Prefabs.Test.TransformTest");
		
		// Assert
		Assert.Equal(new Vector3(0.0, 0.0, 2.5), transform.LocalPosition);
	}
	
	[Fact]
	public static void DeserializingComponent_SetsReferences()
	{
		// Arrange
		
		// Act
		TransformPrefab transform = Project.Instantiate<TransformPrefab>(AppContext.BaseDirectory + "Prefabs.Test.TransformPrefabTest");
		
		// Assert
		Assert.NotNull(transform.Transform);
	}
	
	[Fact]
	public static void DeserializingComponent_WithDottedPaths_SetsReferences()
	{
		// Arrange
		
		// Act
		PrefabRoot prefab = Project.Deserialize(AppContext.BaseDirectory + "Prefabs.Test.SubcomponentDottedPrefabTest");
		
		// Assert
		Assert.NotNull(((SubcomponentPrefab)prefab.RootObject).Subcomponent);
		Assert.Null(((SubcomponentPrefab)prefab.RootObject).Reference);
		Assert.Equal(prefab.Components[2], ((SubcomponentPrefab)prefab.RootObject).Subcomponent);
	}
	
	[Fact]
	public static void ReserializingComponent_WithReferencesOnSubComponent_SetsReferencesOnlyOnSubComponent()
	{
		// Arrange
		
		// Act
		PrefabRoot prefab = Project.Deserialize(AppContext.BaseDirectory + "Prefabs.Test.SubcomponentPrefabTest");
		TomlTable table = prefab.Prefab.ToToml();
		
		// Assert
		Assert.False(((TomlTable)table["0"]).ContainsKey("Reference"));
		Assert.True(((TomlTable)table["1"]).ContainsKey("Reference"));
		Assert.False(((TomlTable)table["2"]).ContainsKey("Reference"));
	}
}
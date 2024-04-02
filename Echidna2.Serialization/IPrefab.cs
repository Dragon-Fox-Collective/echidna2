namespace Echidna2.Serialization;

public interface IPrefab
{
	public string PrefabPath { get; }
}

public static class PrefabExtensions
{
	public static T Instantiate<T>(this T prefab) where T : class, IPrefab => TomlDeserializer.Deserialize<T>(prefab.PrefabPath);
}
using Echidna2.Core;
using Echidna2.Rendering3D;
using Echidna2.Serialization;
using JetBrains.Annotations;

namespace Echidna2.Prefabs;

[UsedImplicitly, Prefab("Prefabs/Cube.toml")]
public partial class Cube : INotificationPropagator
{
	[SerializedReference, ExposeMembersInClass] public Named Named { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Transform3D Transform { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public PBRMeshRenderer MeshRenderer { get; set; } = null!;
	[SerializedReference, ExposeMembersInClass] public Hierarchy PrefabChildren { get; set; } = null!;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, MeshRenderer);
	}
}
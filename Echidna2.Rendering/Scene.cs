using Echidna2.Core;
using Echidna2.Serialization;

namespace Echidna2.Rendering;

[SerializeExposedMembers]
public partial class Scene : IHasCamera
{
	[SerializedReference, ExposeMembersInClass] public Hierarchy Hierarchy { get; set; } = null!;
	[SerializedReference] public IHasCamera CameraHaver { get; set; } = null!;
	public Camera HavedCamera => CameraHaver.HavedCamera;
}
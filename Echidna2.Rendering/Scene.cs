using Echidna2.Core;
using Echidna2.Serialization;

namespace Echidna2.Rendering;

public partial class Scene : INotificationPropagator, IHasChildren, ICanAddChildren, IHasCamera
{
	[SerializedReference, ExposeMembersInClass] public Hierarchy Hierarchy { get; set; } = null!;
	[SerializedReference] public IHasCamera CameraHaver { get; set; } = null!;
	public Camera HavedCamera => CameraHaver.HavedCamera;
}
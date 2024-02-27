using Echidna2.Core;
using Echidna2.Serialization;

namespace Echidna2.Rendering;

public partial class Scene : INotificationPropagator, IHasChildren, ICanAddChildren
{
	[SerializedReference, ExposeMembersInClass] public Hierarchy Hierarchy { get; set; } = null!;
	[SerializedReference] public IHasCamera CameraHaver { get; set; } = null!;
}
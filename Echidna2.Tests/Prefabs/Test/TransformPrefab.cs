using Echidna2.Core;
using Echidna2.Rendering3D;
using Echidna2.Serialization;

namespace Echidna2.Prefabs.Test;

public partial class TransformPrefab
{
	[SerializedReference, ExposeMembersInClass] public Transform3D Transform { get; set; } = null!;
}
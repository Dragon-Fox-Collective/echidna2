using Echidna2.Core;
using Echidna2.Serialization;

namespace Echidna2.Prefabs.Test;

public partial class SubcomponentPrefab
{
	[SerializedReference, ExposeMembersInClass] public SubcomponentSubcomponent Subcomponent { get; set; } = null!;
}
using Echidna2.Core;
using Echidna2.Serialization;

namespace Echidna2.Prefabs.Test;

public partial class SubcomponentSubcomponent
{
	[SerializedReference, ExposeMembersInClass] public object? Reference { get; set; } = null!;
}
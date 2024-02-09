using JetBrains.Annotations;

namespace Echidna2.Core;

[AttributeUsage(AttributeTargets.Interface)]
public class ComponentImplementationAttribute<[UsedImplicitly] T> : Attribute;
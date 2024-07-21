using Echidna2.Serialization.TomlFiles;
using TooManyExtensions;

namespace Echidna2.Serialization;

public readonly struct ComponentPair(object obj, Component component)
{
	public object Object => obj;
	public Component Component => component;
	
	public Option<ComponentPair> SourceComponent => component.SourceComponent.TrySome(out Component? source) ? Option.Some<ComponentPair>(new ComponentPair(obj, source)) : Option.None<ComponentPair>();
	
	public override string ToString() => $"({obj}, {component})";
}
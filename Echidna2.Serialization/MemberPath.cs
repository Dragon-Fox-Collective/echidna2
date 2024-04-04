namespace Echidna2.Serialization;

public interface IMemberPath : IEquatable<IMemberPath>
{
	public ComponentPath Root { get; }
	public object Value { get; }
}

public class MemberPath(IMemberWrapper wrapper, IMemberPath parent) : IMemberPath
{
	public IMemberWrapper Wrapper => wrapper;
	public IMemberPath Parent => parent;
	
	public ComponentPath Root => parent.Root;
	public MemberPath HighestMemberPath => parent is MemberPath memberParent ? memberParent.HighestMemberPath : this;
	public object Value => wrapper.GetValue(parent.Value)!;
	
	public override bool Equals(object? other) => Equals(other as IMemberPath);
	public override int GetHashCode() => (wrapper, parent).GetHashCode();
	public bool Equals(IMemberPath? other)
	{
		if (other is null)
			return false;
		if (ReferenceEquals(this, other))
			return true;
		if (other.GetType() != GetType())
			return false;
		MemberPath otherPath = (MemberPath)other;
		return wrapper.Equals(otherPath.Wrapper) && parent.Equals(otherPath.Parent);
	}
	
	public override string ToString() => $"{parent}.{wrapper.Name}";
}

public class ComponentPath(object component) : IMemberPath
{
	public object Component => component;
	
	public ComponentPath Root => this;
	public object Value => component;
	
	public override bool Equals(object? other) => Equals(other as IMemberPath);
	public override int GetHashCode() => component.GetHashCode();
	public bool Equals(IMemberPath? other)
	{
		if (other is null)
			return false;
		if (ReferenceEquals(this, other))
			return true;
		if (other.GetType() != GetType())
			return false;
		ComponentPath otherPath = (ComponentPath)other;
		return component == otherPath.Component;
	}
	
	public override string ToString() => component.GetType().Name;
}
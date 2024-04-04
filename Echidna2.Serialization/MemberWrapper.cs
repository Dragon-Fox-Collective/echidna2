using System.Reflection;

namespace Echidna2.Serialization;

public interface IMemberWrapper : IEquatable<IMemberWrapper>
{
	public object? GetValue(object obj);
	public void SetValue(object obj, object? value);
	public Type FieldType { get; }
	public bool CanRead { get; }
	public bool CanWrite { get; }
	public string Name { get; }
	public T? GetCustomAttribute<T>() where T : Attribute;
	public MemberInfo Member { get; }
	
	public static IMemberWrapper Wrap(MemberInfo member) => member switch
	{
		FieldInfo field => new FieldWrapper(field),
		PropertyInfo property => new PropertyWrapper(property),
		_ => throw new ArgumentException("Member must be a field or property"),
	};
}

public class FieldWrapper(FieldInfo field) : IMemberWrapper
{
	private readonly FieldInfo field = field;
	
	public object? GetValue(object obj) => field.GetValue(obj);
	public void SetValue(object obj, object? value) => field.SetValue(obj, value);
	public Type FieldType => field.FieldType;
	public bool CanRead => true;
	public bool CanWrite => !field.IsInitOnly;
	public string Name => field.Name;
	public T? GetCustomAttribute<T>() where T : Attribute => field.GetCustomAttribute<T>();
	public MemberInfo Member => field;
	
	public override string? ToString() => field.ToString();
	
	public override bool Equals(object? other) => Equals(other as IMemberWrapper);
	public override int GetHashCode() => field.GetHashCode();
	public bool Equals(IMemberWrapper? other)
	{
		if (other is null)
			return false;
		if (ReferenceEquals(this, other))
			return true;
		if (other.GetType() != GetType())
			return false;
		FieldWrapper otherWrapper = (FieldWrapper)other;
		return field == otherWrapper.field;
	}
}

public class PropertyWrapper(PropertyInfo property) : IMemberWrapper
{
	private readonly PropertyInfo property = property;
	
	public object? GetValue(object obj) => property.GetValue(obj);
	public void SetValue(object obj, object? value) => property.SetValue(obj, value);
	public Type FieldType => property.PropertyType;
	public bool CanRead => property.CanRead;
	public bool CanWrite => property.CanWrite;
	public string Name => property.Name;
	public T? GetCustomAttribute<T>() where T : Attribute => property.GetCustomAttribute<T>();
	public override string? ToString() => property.ToString();
	public MemberInfo Member => property;
	
	public override bool Equals(object? other) => Equals(other as IMemberWrapper);
	public override int GetHashCode() => property.GetHashCode();
	public bool Equals(IMemberWrapper? other)
	{
		if (other is null)
			return false;
		if (ReferenceEquals(this, other))
			return true;
		if (other.GetType() != GetType())
			return false;
		PropertyWrapper otherWrapper = (PropertyWrapper)other;
		return property == otherWrapper.property;
	}
}
using System.Drawing;
using System.Reflection;
using Tomlyn.Model;

namespace Echidna2.Serialization;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SerializedValueAttribute(Type? serializerType = null) : Attribute
{
	private static Dictionary<Type, Serializer> defaultSerializers = new()
	{
		{ typeof(double), new NumberSerializer<double>() },
		{ typeof(int), new NumberSerializer<int>() },
		{ typeof(string), new DirectSerializer<string>() },
		{ typeof(bool), new DirectSerializer<bool>() },
		{ typeof(Color), new ColorSerializer() },
	};
	
	public static void AddDefaultSerializer(Type type, Serializer serializer) => defaultSerializers[type] = serializer;
	
	private Serializer? serializer = (Serializer)serializerType?.GetConstructor([])!.Invoke([])!;
	
	public Serializer GetSerializer(Type type, Func<TomlTable, object> subcomponentDeserializer)
	{
		if (serializer is not null)
			return serializer;
		
		if (defaultSerializers.TryGetValue(type, out serializer))
			return serializer;
		
		if (type.IsEnum)
			return serializer = new EnumSerializer(type);
		
		if (type
		    .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
		    .Any(member => member.GetCustomAttribute<SerializedValueAttribute>() is not null))
			return new SubComponentSerializer(subcomponentDeserializer);
		
		throw new InvalidOperationException($"No serializer found for type {type}");
	}
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SerializedReferenceAttribute(Type? serializerType = null) : Attribute
{
	private static readonly ReferenceSerializer DefaultReferenceSerializer = new DefaultReferenceSerializer();
	
	private ReferenceSerializer? serializer = (ReferenceSerializer)serializerType?.GetConstructor([])!.Invoke([])!;
	
	public ReferenceSerializer GetSerializer(Type type)
	{
		if (serializer is not null)
			return serializer;

		return serializer = DefaultReferenceSerializer;
	}
}
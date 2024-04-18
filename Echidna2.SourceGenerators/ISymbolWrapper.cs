using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Echidna2.SourceGenerators;

public interface ISymbolWrapper
{
	public ImmutableArray<AttributeData> GetAttributes();
	public ITypeSymbol Type { get; }
	public ISymbol Symbol { get; }
}

public static class SymbolWrapper
{
	public static ISymbolWrapper Wrap(ISymbol member) => member switch
	{
		IFieldSymbol field => new FieldSymbolWrapper(field),
		IPropertySymbol property => new PropertySymbolWrapper(property),
		_ => throw new ArgumentException($"Member must be a field or property, not {member.GetType()}"),
	};
}

public class FieldSymbolWrapper(IFieldSymbol field) : ISymbolWrapper
{
	public ImmutableArray<AttributeData> GetAttributes() => field.GetAttributes();
	public ITypeSymbol Type => field.Type;
	public ISymbol Symbol => field;
}

public class PropertySymbolWrapper(IPropertySymbol property) : ISymbolWrapper
{
	public ImmutableArray<AttributeData> GetAttributes() => property.GetAttributes();
	public ITypeSymbol Type => property.Type;
	public ISymbol Symbol => property;
}
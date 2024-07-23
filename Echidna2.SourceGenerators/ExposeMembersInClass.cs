using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Echidna2.Core
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class ExposeMembersInClassAttribute : Attribute;
	
	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Event)]
	public class DontExposeAttribute : Attribute;
}

namespace Echidna2.SourceGenerators
{
	[Generator]
	public class ExposeMembersInClassPropertySourceGenerator : IIncrementalGenerator
	{
		public void Initialize(IncrementalGeneratorInitializationContext initContext)
		{
			IncrementalValueProvider<Compilation> compilationProvider = initContext.CompilationProvider;
			IncrementalValuesProvider<PropertyDeclarationSyntax> syntaxProvider = initContext.SyntaxProvider.CreateSyntaxProvider(
				(node, _) => NodeIsPropertyWithExposeMembersInClassAttribute(node),
				(syntaxContext, _) => (PropertyDeclarationSyntax)syntaxContext.Node
			);
			IncrementalValueProvider<(Compilation Left, ImmutableArray<PropertyDeclarationSyntax> Right)> tuple = compilationProvider.Combine(syntaxProvider.Collect());
			initContext.RegisterImplementationSourceOutput(tuple, (context, provider) => OnExecute(context, provider.Left, provider.Right));
		}
		
		private static bool NodeIsPropertyWithExposeMembersInClassAttribute(SyntaxNode node)
		{
			if (node is not PropertyDeclarationSyntax property) return false;
			if (property.AttributeLists.Count == 0) return false;
			return property.AttributeLists.SelectMany(attributeList => attributeList.Attributes).Any(attribute => attribute.Name.ToString() == "ExposeMembersInClass");
		}
		
		private static void OnExecute(SourceProductionContext context, Compilation compilation, ImmutableArray<PropertyDeclarationSyntax> nodes)
		{
			// context.AddSource("debug.g.cs", "// " + string.Join(", ", nodes).Replace("\n", "\n// "));
			foreach (PropertyDeclarationSyntax node in nodes.Distinct())
			{
				if (context.CancellationToken.IsCancellationRequested)
					return;
				
				SemanticModel model = compilation.GetSemanticModel(node.SyntaxTree);
				if (model.GetDeclaredSymbol(node) is not IPropertySymbol symbol) continue;
				
				AttributeData? attributeData = symbol.GetAttributes().SingleOrDefault(attribute => attribute.AttributeClass?.Name == "ExposeMembersInClassAttribute");
				if (attributeData is null) continue;
				
				string generatedCode = GenerateCode(symbol);
				context.AddSource($"{symbol.ContainingType}_{symbol.Name}.g.cs", generatedCode);
			}
		}
		
		private static string GenerateCode(IPropertySymbol symbol)
		{
			INamedTypeSymbol classType = symbol.ContainingType;
			INamedTypeSymbol prefabType = (INamedTypeSymbol)symbol.Type;
			string propertyName = symbol.Name;
			INamedTypeSymbol[] interfaces = GetAllExposedInterfaces(prefabType).ToArray();
			
			string source = "#nullable enable\n";
			if (!classType.ContainingNamespace.IsGlobalNamespace)
				source += $"namespace {symbol.ContainingNamespace};\n";
			source += "\n";
			source += $"partial class {classType.Name}{(interfaces.Length != 0 ? " : " + string.Join<INamedTypeSymbol>(", ", interfaces) : "")}\n";
			source += "{\n";
			
			foreach (ISymbol member in GetAllUnimplementedMembersExposeRecursive(classType, prefabType))
				source += GetMemberDeclaration(member, propertyName);
			
			source += "}";
			return source;
		}
		
		
		public static IEnumerable<ISymbol> GetAllUnimplementedMembersExposeRecursive(INamedTypeSymbol classType, INamedTypeSymbol prefabType) =>
			GetAllUnimplementedMembersExposeRecursiveIncludingConflicts(classType, prefabType).Distinct(SymbolSignatureEqualityComparer.Default);
		
		public static IEnumerable<ISymbol> GetAllUnimplementedMembersExposeRecursiveIncludingConflicts(INamedTypeSymbol classType, INamedTypeSymbol prefabType)
		{
			foreach (ISymbol member in FilterOutNotExposedSymbols(GetAllUnimplementedMembers(classType, prefabType)))
				yield return member;
			
			foreach (ISymbol member2 in FilterForExposingSymbols(GetAllPublicInstanceMembers(prefabType))
				         .SelectMany(symbol => GetAllUnimplementedMembersExposeRecursiveIncludingConflicts(classType, (INamedTypeSymbol)SymbolWrapper.Wrap(symbol).Type)))
				yield return member2;
		}
		
		
		public static IEnumerable<INamedTypeSymbol> GetAllExposedInterfaces(ITypeSymbol type) =>
			GetAllExposedInterfacesIncludingConflicts(type).Distinct<INamedTypeSymbol>(SymbolSignatureEqualityComparer.Default);
		
		public static IEnumerable<INamedTypeSymbol> GetAllExposedInterfacesIncludingConflicts(ITypeSymbol type)
		{
			if (type.TypeKind == TypeKind.Interface && !type.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "DontExposeAttribute"))
				yield return (INamedTypeSymbol)type;
			
			foreach (INamedTypeSymbol inter in type.AllInterfaces.Where(inter => !inter.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "DontExposeAttribute")))
				yield return inter;
			
			foreach (INamedTypeSymbol member2 in FilterForExposingSymbols(type.GetMembers())
				         .SelectMany(symbol => GetAllExposedInterfacesIncludingConflicts(SymbolWrapper.Wrap(symbol).Type)))
				yield return member2;
		}
		
		
		public static IEnumerable<ISymbol> FilterForExposingSymbols(IEnumerable<ISymbol> symbols) => symbols
			.Where(symbol => symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "ExposeMembersInClassAttribute"));
		
		public static IEnumerable<ISymbol> FilterOutNotExposedSymbols(IEnumerable<ISymbol> symbols) => symbols
			.Where(symbol => !symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "DontExposeAttribute"));
		
		
		public static IEnumerable<ISymbol> GetAllPublicInstanceMembers(INamedTypeSymbol type)
		{
			if (type.SpecialType == SpecialType.System_Object)
				yield break;
			
			foreach (ISymbol member in type.GetMembers().Where(member => member is { IsOverride: false, IsStatic: false, DeclaredAccessibility: Accessibility.Public }))
				yield return member;
			
			if (type.BaseType is not null)
				foreach (ISymbol member in GetAllPublicInstanceMembers(type.BaseType))
					yield return member;
		}
		
		
		public static IEnumerable<ISymbol> GetAllPublicInstanceMembersWithoutSpecialMethods(INamedTypeSymbol interfaceType) =>
			GetAllPublicInstanceMembers(interfaceType).Where(member =>
				!member.Name.StartsWith("get_")
				&& !member.Name.StartsWith("set_")
				&& !member.Name.StartsWith("add_")
				&& !member.Name.StartsWith("remove_")
				&& member.Name != ".ctor");
		
		public static IEnumerable<ISymbol> GetAllUnimplementedMembers(INamedTypeSymbol classType, INamedTypeSymbol interfaceType) =>
			GetAllPublicInstanceMembersWithoutSpecialMethods(interfaceType).Where(member => !classType.GetMembers(member.Name).Any());
		
		
		public static string GetMemberDeclaration(ISymbol member, string propertyName)
		{
			AttributeData[] inheritedAttributes = member.GetAttributes().Where(attr => attr.AttributeClass?.GetAttributes().Any(subattr => subattr.AttributeClass?.Name == "AttributeUsageAttribute" && (bool)(subattr.NamedArguments.FirstOrDefault(pair => pair.Key == "Inherited").Value.Value ?? true)) ?? false).ToArray();
			string attributes = inheritedAttributes.Any() ? "[" + string.Join(",", inheritedAttributes.Select(attr => $"{attr.AttributeClass}({string.Join(", ", attr.ConstructorArguments.Select(arg => arg.Value is null ? "null" : arg.Kind is TypedConstantKind.Type ? $"typeof({arg.Value})" : arg.Value.ToString()).Concat(attr.NamedArguments.Select(arg => $"{arg.Key} = {arg.Value}")))})")) + "] " : "";
			string accessibility = member.DeclaredAccessibility.ToString().ToLower();
			return member switch
			{
				IMethodSymbol method =>
					$"\t{attributes}{accessibility} {method.ReturnType} {method.Name}"
					+ (method.IsGenericMethod ? "<" + string.Join(", ", method.TypeArguments) + ">" : "")
					+ "(" + string.Join(", ", method.Parameters.Select(parameter => $"{(parameter.IsParams ? "params " : "")}{parameter.Type} {parameter.Name}{(parameter.HasExplicitDefaultValue ? $" = {parameter.ExplicitDefaultValue ?? "default"}" : "")}")) + ")"
					+ string.Join("", method.TypeArguments.OfType<ITypeParameterSymbol>().Select(typeParameter => (typeParameter, typeParameter.ConstraintTypes.Select(constraint => constraint.ToString()).Concat([
						typeParameter.HasConstructorConstraint ? "new()" : null,
						typeParameter.HasNotNullConstraint ? "notnull" : null,
						typeParameter.HasReferenceTypeConstraint ? "class" : null,
						typeParameter.HasUnmanagedTypeConstraint ? "unmanaged" : null,
						typeParameter.HasValueTypeConstraint ? "struct" : null,
					]).Where(constraint => constraint != null).ToArray())).Select(args => $" where {args.typeParameter} : {string.Join(", ", args.Item2)}"))
					+ " => "
					+ $"{propertyName}.{method.Name}({string.Join(", ", method.Parameters.Select(parameter => parameter.Name))});\n",
				IPropertySymbol { GetMethod.DeclaredAccessibility: Accessibility.Public, SetMethod.DeclaredAccessibility: Accessibility.Public } property =>
					$"\t{attributes}{accessibility} {property.Type} {property.Name} {{ get => {propertyName}.{property.Name}; set => {propertyName}.{property.Name} = value; }}\n",
				IPropertySymbol { GetMethod.DeclaredAccessibility: Accessibility.Public } property =>
					$"\t{attributes}{accessibility} {property.Type} {property.Name} => {propertyName}.{property.Name};\n",
				IPropertySymbol { SetMethod.DeclaredAccessibility: Accessibility.Public } property =>
					$"\t{attributes}{accessibility} {property.Type} {property.Name} {{ set => {propertyName}.{property.Name} = value; }}\n",
				IEventSymbol @event =>
					$"\t{attributes}{accessibility} event {@event.Type} {@event.Name} {{ add => {propertyName}.{@event.Name} += value; remove => {propertyName}.{@event.Name} -= value; }}\n",
				IFieldSymbol field =>
					$"\t{attributes}{accessibility} {field.Type} {field.Name} {{ get => {propertyName}.{field.Name}; set => {propertyName}.{field.Name} = value; }}\n",
				_ => ""
			};
		}
	}
	
	public class SymbolSignatureEqualityComparer : IEqualityComparer<ISymbol>
	{
		public static SymbolSignatureEqualityComparer Default { get; } = new();
		
		public bool Equals(ISymbol? x, ISymbol? y)
		{
			if (x is null || y is null) return x is null && y is null;
			if (x.Name != y.Name) return false;
			return true;
		}
		public int GetHashCode(ISymbol obj) => obj.Name.GetHashCode();
	}
}
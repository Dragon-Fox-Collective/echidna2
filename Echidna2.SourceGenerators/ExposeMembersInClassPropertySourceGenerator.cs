using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Echidna2.SourceGenerators;

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
		context.AddSource("debug.g.cs", "// " + string.Join(", ", nodes).Replace("\n", "\n// "));
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
	
	private static string GenerateCode(IPropertySymbol symbol)//, PropertyDeclarationSyntax node)
	{
		INamedTypeSymbol classType = symbol.ContainingType;
		INamedTypeSymbol prefabType = (INamedTypeSymbol)symbol.Type;
		string propertyName = symbol.Name;
		INamedTypeSymbol[] interfaces = GetAllExposedInterfaces(prefabType)
			.Where(inter =>
				inter.Name != "INotificationListener"
				&& inter.Name != "INotificationHook"
				&& inter.Name != "INotificationPredicate")
			.ToArray();
		
		string source = "#nullable enable\n";
		if (!classType.ContainingNamespace.IsGlobalNamespace)
			source += $"namespace {symbol.ContainingNamespace};\n";
		source += "\n";
		source += $"partial class {classType.Name}{(interfaces.Length != 0 ? " : " + string.Join<INamedTypeSymbol>(", ", interfaces) : "")}\n";
		source += "{\n";
		
		foreach (ISymbol member in GetAllUnimplementedMembersExposeRecursive(classType, prefabType))
			source += ComponentSourceGenerator.GetMemberDeclaration(member, propertyName);
		
		source += "}";
		return source;
	}
	
	public static IEnumerable<ISymbol> GetAllUnimplementedMembersExposeRecursive(INamedTypeSymbol classType, INamedTypeSymbol prefabType) =>
		GetAllUnimplementedMembersExposeRecursiveIncludingConflicts(classType, prefabType).Distinct(SymbolSignatureEqualityComparer.Default);
	
	public static IEnumerable<ISymbol> GetAllUnimplementedMembersExposeRecursiveIncludingConflicts(INamedTypeSymbol classType, INamedTypeSymbol prefabType)
	{
		foreach (ISymbol member in ComponentSourceGenerator.GetAllUnimplementedMembers(classType, prefabType))
		{
			yield return member;
			
			if (member is IPropertySymbol property && property.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "ExposeMembersInClassAttribute"))
				foreach (ISymbol member2 in GetAllUnimplementedMembersExposeRecursiveIncludingConflicts(classType, (INamedTypeSymbol)property.Type))
					yield return member2;
			else if (member is IFieldSymbol field && field.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "ExposeMembersInClassAttribute"))
				foreach (ISymbol member2 in GetAllUnimplementedMembersExposeRecursiveIncludingConflicts(classType, (INamedTypeSymbol)field.Type))
					yield return member2;
		}
	}
	
	public static IEnumerable<INamedTypeSymbol> GetAllExposedInterfaces(ITypeSymbol type) =>
		GetAllExposedInterfacesIncludingConflicts(type).Distinct<INamedTypeSymbol>(SymbolSignatureEqualityComparer.Default);
	
	public static IEnumerable<INamedTypeSymbol> GetAllExposedInterfacesIncludingConflicts(ITypeSymbol type)
	{
		foreach (INamedTypeSymbol inter in type.AllInterfaces)
			yield return inter;
		
		foreach (ISymbol member in type.GetMembers())
		{
			if (member is IPropertySymbol property && property.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "ExposeMembersInClassAttribute"))
				foreach (INamedTypeSymbol member2 in GetAllExposedInterfaces(property.Type))
					yield return member2;
			else if (member is IFieldSymbol field && field.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "ExposeMembersInClassAttribute"))
				foreach (INamedTypeSymbol member2 in GetAllExposedInterfaces(field.Type))
					yield return member2;
		}
	}
}
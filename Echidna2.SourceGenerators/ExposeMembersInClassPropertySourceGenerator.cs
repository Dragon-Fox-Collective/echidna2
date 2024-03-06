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
		string propertyName = symbol.Name;
		
		string source = "#nullable enable\n";
		if (!classType.ContainingNamespace.IsGlobalNamespace)
			source += $"namespace {symbol.ContainingNamespace};\n";
		source += "\n";
		source += $"partial class {classType.Name}\n";
		source += "{\n";
		
		foreach (ISymbol member in GetAllUnimplementedMembersExposeRecursive(classType, (INamedTypeSymbol)symbol.Type))
			source += ComponentSourceGenerator.GetMemberDeclaration(member, propertyName);
		
		source += "}";
		return source;
	}
	
	public static IEnumerable<ISymbol> GetAllUnimplementedMembersExposeRecursive(INamedTypeSymbol classType, INamedTypeSymbol prefabType)
	{
		foreach (ISymbol member in ComponentSourceGenerator.GetAllUnimplementedMembers(classType, prefabType))
		{
			yield return member;
			
			if (member is IPropertySymbol property && classType.ContainingAssembly.Equals(property.Type.ContainingAssembly, SymbolEqualityComparer.Default) && property.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "ExposeMembersInClassAttribute"))
				foreach (ISymbol member2 in GetAllUnimplementedMembersExposeRecursive(classType, (INamedTypeSymbol)property.Type))
					yield return member2;
			else if (member is IFieldSymbol field && classType.ContainingAssembly.Equals(field.Type.ContainingAssembly, SymbolEqualityComparer.Default) && field.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "ExposeMembersInClassAttribute"))
				foreach (ISymbol member2 in GetAllUnimplementedMembersExposeRecursive(classType, (INamedTypeSymbol)field.Type))
					yield return member2;
		}
	}
}
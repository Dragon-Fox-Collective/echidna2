using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Echidna2.SourceGenerators;

[Generator]
public class PrefabOfSourceGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext initContext)
	{
		IncrementalValueProvider<Compilation> compilationProvider = initContext.CompilationProvider;
		IncrementalValuesProvider<ClassDeclarationSyntax> syntaxProvider = initContext.SyntaxProvider.CreateSyntaxProvider(
			(node, _) => NodeIsClassWithPrefabOfAttribute(node),
			(syntaxContext, _) => (ClassDeclarationSyntax)syntaxContext.Node
		);
		IncrementalValueProvider<(Compilation Left, ImmutableArray<ClassDeclarationSyntax> Right)> tuple = compilationProvider.Combine(syntaxProvider.Collect());
		initContext.RegisterImplementationSourceOutput(tuple, (context, provider) => OnExecute(context, provider.Left, provider.Right));
	}
	
	private static bool NodeIsClassWithPrefabOfAttribute(SyntaxNode node)
	{
		if (node is not ClassDeclarationSyntax clazz) return false;
		if (clazz.AttributeLists.Count == 0) return false;
		return clazz.AttributeLists.SelectMany(attributeList => attributeList.Attributes).Any(attribute => attribute.Name.ToString().StartsWith("PrefabOf<"));
	}
	
	private static void OnExecute(SourceProductionContext context, Compilation compilation, ImmutableArray<ClassDeclarationSyntax> nodes)
	{
		context.AddSource("debug.g.cs", "// " + string.Join(", ", nodes).Replace("\n", "\n// "));
		foreach (ClassDeclarationSyntax node in nodes.Distinct())
		{
			if (context.CancellationToken.IsCancellationRequested)
				return;
			
			SemanticModel model = compilation.GetSemanticModel(node.SyntaxTree);
			if (model.GetDeclaredSymbol(node) is not INamedTypeSymbol symbol) continue;
			
			AttributeData? attributeData = symbol.GetAttributes().SingleOrDefault(attribute => attribute.AttributeClass?.Name == "PrefabOfAttribute");
			if (attributeData is null) continue;
			
			string generatedCode = GenerateCode(symbol);
			context.AddSource($"{symbol}.g.cs", generatedCode);
		}
	}
	
	private static string GenerateCode(INamedTypeSymbol symbol)//, ClassDeclarationSyntax node)
	{
		string className = symbol.Name;
		INamedTypeSymbol? prefabType = symbol.GetAttributes().First(attribute => attribute.AttributeClass?.Name == "PrefabOfAttribute").AttributeClass?.TypeArguments[0] as INamedTypeSymbol;
		if (prefabType is null) throw new InvalidOperationException("PrefabOfAttribute must have a type argument");
		string propertyName = "prefab";
		
		string source = "#nullable enable\n";
		if (!symbol.ContainingNamespace.IsGlobalNamespace)
			source += $"namespace {symbol.ContainingNamespace};\n";
		source += "\n";
		source += $"partial class {className}({prefabType} {propertyName})\n";
		source += "{\n";
		
		// if (prefabType.AllInterfaces.Any(@interface => @interface.Name == "IInstantiatable")) // doesn't work
		if (prefabType.GetMembers("Instantiate").Any())
			source += $"\tprivate {prefabType} {propertyName} = {propertyName}.Instantiate();\n";
		
		foreach (ISymbol member in ExposeMembersInClassPropertySourceGenerator.GetAllUnimplementedMembersExposeRecursive(symbol, prefabType))
			source += ComponentSourceGenerator.GetMemberDeclaration(member, propertyName);
		
		source += "}";
		return source;
	}
}
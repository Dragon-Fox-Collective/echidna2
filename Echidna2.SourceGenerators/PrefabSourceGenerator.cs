using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Echidna2.SourceGenerators;

[Generator]
public class PrefabSourceGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext initContext)
	{
		IncrementalValueProvider<Compilation> compilationProvider = initContext.CompilationProvider;
		IncrementalValuesProvider<ClassDeclarationSyntax> syntaxProvider = initContext.SyntaxProvider.CreateSyntaxProvider(
			(node, _) => NodeIsClassWithPrefabAttribute(node),
			(syntaxContext, _) => (ClassDeclarationSyntax)syntaxContext.Node
		);
		IncrementalValueProvider<(Compilation Left, ImmutableArray<ClassDeclarationSyntax> Right)> tuple = compilationProvider.Combine(syntaxProvider.Collect());
		initContext.RegisterImplementationSourceOutput(tuple, (context, provider) => OnExecute(context, provider.Left, provider.Right));
	}
	
	private static bool NodeIsClassWithPrefabAttribute(SyntaxNode node)
	{
		if (node is not ClassDeclarationSyntax clazz) return false;
		if (clazz.AttributeLists.Count == 0) return false;
		return clazz.AttributeLists.SelectMany(attributeList => attributeList.Attributes).Any(attribute => attribute.Name.ToString() == "Prefab");
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
			
			AttributeData? attributeData = symbol.GetAttributes().SingleOrDefault(attribute => attribute.AttributeClass?.Name == "PrefabAttribute");
			if (attributeData is null) continue;
			
			string generatedCode = GenerateCode(symbol);
			context.AddSource($"{symbol}.g.cs", generatedCode);
		}
	}
	
	private static string GenerateCode(INamedTypeSymbol symbol)//, ClassDeclarationSyntax node)
	{
		string className = symbol.Name;
		string path = (string?)symbol.GetAttributes().First(attribute => attribute.AttributeClass?.Name == "PrefabAttribute").ConstructorArguments[0].Value ?? "";
		
		string source = "#nullable enable\n";
		if (!symbol.ContainingNamespace.IsGlobalNamespace)
			source += $"namespace {symbol.ContainingNamespace};\n";
		source += "\n";
		source += $"partial class {className}\n";
		source += "{\n";
		
		source += $"\tpublic static {className} Instantiate() => Echidna2.Serialization.TomlSerializer.Deserialize<{className}>($\"{{AppContext.BaseDirectory}}/{path}\");\n";
		
		source += "}";
		return source;
	}
}
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Echidna2.SourceGenerators;

[Generator]
public class SerializeExposedMembersSourceGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext initContext)
	{
		IncrementalValueProvider<Compilation> compilationProvider = initContext.CompilationProvider;
		IncrementalValuesProvider<ClassDeclarationSyntax> syntaxProvider = initContext.SyntaxProvider.CreateSyntaxProvider(
			(node, _) => NodeIsClassWithSerializeExposedMembersAttribute(node),
			(syntaxContext, _) => (ClassDeclarationSyntax)syntaxContext.Node
		);
		IncrementalValueProvider<(Compilation Left, ImmutableArray<ClassDeclarationSyntax> Right)> tuple = compilationProvider.Combine(syntaxProvider.Collect());
		initContext.RegisterImplementationSourceOutput(tuple, (context, provider) => OnExecute(context, provider.Left, provider.Right));
	}
	
	private static bool NodeIsClassWithSerializeExposedMembersAttribute(SyntaxNode node)
	{
		if (node is not ClassDeclarationSyntax clazz) return false;
		if (clazz.AttributeLists.Count == 0) return false;
		return clazz.AttributeLists.SelectMany(attributeList => attributeList.Attributes).Any(attribute => attribute.Name.ToString() == "SerializeExposedMembers");
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
			
			AttributeData? attributeData = symbol.GetAttributes().SingleOrDefault(attribute => attribute.AttributeClass?.Name == "SerializeExposedMembersAttribute");
			if (attributeData is null) continue;
			
			string generatedCode = GenerateCode(symbol);
			context.AddSource($"{symbol}.g.cs", generatedCode);
		}
	}
	
	private static string GenerateCode(INamedTypeSymbol symbol)//, ClassDeclarationSyntax node)
	{
		string className = symbol.Name;
		ISymbol[] childSerializables = symbol.GetMembers()
			.Where(member => !member.Name.EndsWith("__BackingField"))
			.Where(member => member switch
			{
				IPropertySymbol property => property.Type.AllInterfaces.Any(inter => inter.Name == "ITomlSerializable"),
				IFieldSymbol field => field.Type.AllInterfaces.Any(inter => inter.Name == "ITomlSerializable"),
				_ => false
			})
			.ToArray();
		
		string source = "#nullable enable\n";
		if (!symbol.ContainingNamespace.IsGlobalNamespace)
			source += $"namespace {symbol.ContainingNamespace};\n";
		source += "\n";
		source += $"partial class {className} : Echidna2.Serialization.ITomlSerializable\n";
		source += "{\n";
		
		source += $"\tprivate IEnumerable<Echidna2.Serialization.ITomlSerializable> ChildSerializables => EnumerableOf.Of<Echidna2.Serialization.ITomlSerializable?>({string.Join(", ", childSerializables.Select(child => child.Name))}).Where(child => child != null)!;\n";
		source += "\tpublic void SerializeReferences(Tomlyn.Model.TomlTable table, Func<object, string> getReferenceTo) => ChildSerializables.ForEach(serializable => serializable.SerializeReferences(table, getReferenceTo));\n";
		source += "\tpublic bool DeserializeReference(string id, object value, Dictionary<string, object> references) => ChildSerializables.Any(serializable => serializable.DeserializeReference(id, value, references));\n";
		
		source += "}";
		return source;
	}
}
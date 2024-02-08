using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Echidna2.SourceGenerators;

[Generator]
public class ComponentSourceGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		context.SyntaxProvider.CreateSyntaxProvider(
			(node, token) => node is ConstructorDeclarationSyntax,
			(syntaxContext, token) => (ConstructorDeclarationSyntax)syntaxContext.Node);
		
		
	}
}
using System.Collections.Immutable;
using Echidna2.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Echidna2.SourceGenerators;

[Generator]
public class ComponentSourceGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext initContext)
	{
		IncrementalValueProvider<Compilation> compilationProvider = initContext.CompilationProvider;
		IncrementalValuesProvider<ParameterSyntax> syntaxProvider = initContext.SyntaxProvider.CreateSyntaxProvider(
			(node, _) => NodeIsConstructorParameterWithComponentAttribute(node),
			(syntaxContext, _) => (ParameterSyntax)syntaxContext.Node
		);
		IncrementalValueProvider<(Compilation Left, ImmutableArray<ParameterSyntax> Right)> tuple = compilationProvider.Combine(syntaxProvider.Collect());
		initContext.RegisterImplementationSourceOutput(tuple, (context, provider) => OnExecute(context, provider.Left, provider.Right));
	}
	
	private static bool NodeIsConstructorParameterWithComponentAttribute(SyntaxNode node)
	{
		if (node is not ParameterSyntax parameter) return false;
		if (parameter.Parent?.Parent is not ClassDeclarationSyntax and not ConstructorDeclarationSyntax) return false;
		if (parameter.AttributeLists.Count == 0) return false;
		return parameter.AttributeLists.SelectMany(attributeList => attributeList.Attributes).Any(attribute => attribute.Name.ToString() == "Component");
	}
	
	private static void OnExecute(SourceProductionContext context, Compilation compilation, ImmutableArray<ParameterSyntax> nodes)
	{
		context.AddSource("debug.g.cs", "// " + string.Join(", ", nodes).Replace("\n", "\n// "));
		foreach (ParameterSyntax node in nodes.Distinct())
		{
			if (context.CancellationToken.IsCancellationRequested)
				return;
			
			SemanticModel model = compilation.GetSemanticModel(node.SyntaxTree);
			if (model.GetDeclaredSymbol(node) is not IParameterSymbol symbol) continue;
			
			AttributeData? attributeData = symbol.GetAttributes().SingleOrDefault(attribute => attribute.AttributeClass?.Name == nameof(ComponentAttribute));
			if (attributeData is null) continue;
			
			string generatedCode = GenerateCode(symbol, node);
			context.AddSource($"{symbol.ContainingType}_{symbol.Type.Name}.g.cs", generatedCode);
		}
	}
	
	private static string GenerateCode(IParameterSymbol symbol, ParameterSyntax node)
	{
		bool isPrimaryConstructorParameter = node.Parent?.Parent is ClassDeclarationSyntax;
		string parameterName = symbol.Name;
		string parameterFieldName = parameterName;//parameterName.Substring(0, 1).ToUpperInvariant() + parameterName.Substring(1);
		INamedTypeSymbol classType = symbol.ContainingType;
		INamedTypeSymbol interfaceType = (INamedTypeSymbol)symbol.Type.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
		ITypeSymbol? interfaceImplementationType = interfaceType.GetAttributes().Where(implAttribute => implAttribute.AttributeClass?.Name == "ComponentImplementationAttribute").Select(implAttribute => implAttribute.AttributeClass?.TypeArguments[0]).FirstOrDefault();
		IMethodSymbol? interfaceImplementationConstructor = interfaceImplementationType?.GetMembers(".ctor").OfType<IMethodSymbol>().FirstOrDefault(method => method.Parameters.All(parameter => parameter.IsOptional));
		
		string source = "#nullable enable\n";
		source += $"namespace {symbol.ContainingNamespace};\n";
		source += "\n";
		source += $"partial class {classType.Name} : {interfaceType}\n";
		source += "{\n";
		
		if (interfaceImplementationType is not null && interfaceImplementationConstructor is not null)
		{
			if (isPrimaryConstructorParameter)
				source += $"\tprivate {interfaceType} {parameterFieldName} = {parameterName} ?? new {interfaceImplementationType}();\n\n";
			else
				source += $"\tprivate {interfaceType} {parameterFieldName};\n\n";
		}
		
		foreach (ISymbol member in GetAllUnimplementedInterfaceMembers(classType, interfaceType))
			source += GetMemberDeclaration(member, parameterFieldName);
		
		source += "}";
		return source;
	}
	
	public static IEnumerable<ISymbol> GetAllUnimplementedMembers(INamedTypeSymbol classType, INamedTypeSymbol interfaceType) =>
		interfaceType.GetMembers().Where(member => member.DeclaredAccessibility == Accessibility.Public && !member.IsStatic && !member.Name.StartsWith("get_") && !member.Name.StartsWith("set_") && !member.Name.StartsWith("add_") && !member.Name.StartsWith("remove_") && member.Name != "Serialize" && member.Name != "DeserializeValue" && member.Name != "DeserializeReference" && !classType.GetMembers(member.Name).Any());
	
	private static IEnumerable<ISymbol> GetAllUnimplementedInterfaceMembers(INamedTypeSymbol classType, INamedTypeSymbol interfaceType) =>
		interfaceType.AllInterfaces.Append(interfaceType).SelectMany(inter => GetAllUnimplementedMembers(classType, inter));
	
	public static string GetMemberDeclaration(ISymbol member, string propertyName)
	{
		AttributeData[] inheritedAttributes = member.GetAttributes().Where(attr => attr.AttributeClass?.GetAttributes().Any(subattr => subattr.AttributeClass?.Name == "AttributeUsageAttribute" && (bool)(subattr.NamedArguments.FirstOrDefault(pair => pair.Key == "Inherited").Value.Value ?? true)) ?? false).ToArray();
		string attributes = inheritedAttributes.Any() ? "[" + string.Join(",", inheritedAttributes.Select(attr => $"{attr.AttributeClass}({string.Join(", ", attr.ConstructorArguments.Select(arg => arg.Value?.ToString() ?? "null").Concat(attr.NamedArguments.Select(arg => $"{arg.Key} = {arg.Value}")))})")) + "] " : "";
		string accessibility = member.DeclaredAccessibility.ToString().ToLower();
		return member switch
		{
			IMethodSymbol method =>
				$"\t{attributes}{accessibility} {method.ReturnType} {method.Name}{(method.IsGenericMethod ? "<" + string.Join(", ", method.TypeArguments) + ">" : "")}({string.Join(", ", method.Parameters.Select(parameter => $"{(parameter.IsParams ? "params " : "")}{parameter.Type} {parameter.Name}{(parameter.HasExplicitDefaultValue ? $" = {parameter.ExplicitDefaultValue ?? "default"}" : "")}"))}) => {propertyName}.{method.Name}({string.Join(", ", method.Parameters.Select(parameter => parameter.Name))});\n",
			IPropertySymbol { GetMethod: not null, SetMethod: not null } property =>
				$"\t{attributes}{accessibility} {property.Type} {property.Name} {{ get => {propertyName}.{property.Name}; set => {propertyName}.{property.Name} = value; }}\n",
			IPropertySymbol { GetMethod: not null } property =>
				$"\t{attributes}{accessibility} {property.Type} {property.Name} => {propertyName}.{property.Name};\n",
			IPropertySymbol { SetMethod: not null } property =>
				$"\t{attributes}{accessibility} {property.Type} {property.Name} {{ set => {propertyName}.{property.Name} = value; }}\n",
			IEventSymbol @event =>
				$"\t{attributes}{accessibility} event {@event.Type} {@event.Name} {{ add => {propertyName}.{@event.Name} += value; remove => {propertyName}.{@event.Name} -= value; }}\n",
			IFieldSymbol field =>
				$"\t{attributes}{accessibility} {field.Type} {field.Name} {{ get => {propertyName}.{field.Name}; set => {propertyName}.{field.Name} = value; }}\n",
			_ => ""
		};
	}
}
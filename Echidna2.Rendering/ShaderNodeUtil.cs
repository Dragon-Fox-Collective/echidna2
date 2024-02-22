using JetBrains.Annotations;
using OpenTK.Mathematics;

namespace Echidna2.Rendering;

public static class ShaderNodeUtil
{
	private static readonly InOutVariable<Vector2> TexCoordVariable = new("TexCoords");
	private static readonly InOutVariable<Vector3> LocalPositionVariable = new("LocalPosition");
	private static readonly InOutVariable<Vector3> GlobalPositionVariable = new("GlobalPosition");
	private static readonly InOutVariable<Vector3> VertexColorVariable = new("VertexColor");
	private static readonly InOutVariable<Vector3> NormalVariable = new("Normal");
	private static readonly InOutVariable<Vector3> CubeMapTexCoordVariable = new("TexCoords");
	
	private static readonly UniformVariable<CubeSampler> SkyboxVariable = new("skybox");
	
	
	public static readonly VertexShader MainVertexShader = new()
	{
		Position = new PositionOutput(
			new Vector4TimesMatrix4(
				new Vector4TimesMatrix4(
					new Vector4TimesMatrix4(
						new Vector4TimesMatrix4(
							new Vector3ToVector4(
								new PositionInput().Output,
								new FloatValue(1.0f).Output
								).Output,
							new DistortionInput().Output
							).Output,
						new TransformInput().Output
						).Output,
					new ViewInput().Output
					).Output,
				new ProjectionInput().Output
				).Output),
		InOutBindings =
		[
			new InOutBinding<Vector3>(GlobalPositionVariable,
				new Vector4XYZ(
					new Vector4TimesMatrix4(
						new Vector4TimesMatrix4(
							new Vector3ToVector4(
								new PositionInput().Output,
								new FloatValue(1.0f).Output
								).Output,
							new DistortionInput().Output
							).Output,
						new TransformInput().Output
						).Output
					).Output
				),
			new InOutBinding<Vector3>(LocalPositionVariable,
				new Vector4XYZ(
					new Vector4TimesMatrix4(
						new Vector3ToVector4(
							new PositionInput().Output,
							new FloatValue(1.0f).Output
							).Output,
						new DistortionInput().Output
						).Output
					).Output
				),
			new InOutBinding<Vector2>(TexCoordVariable, new TexCoordInput().Output),
			new InOutBinding<Vector3>(VertexColorVariable, new VertexColorInput().Output),
			new InOutBinding<Vector3>(NormalVariable,
				new Vector4XYZ(
					new Vector4TimesMatrix4(
						new Vector3ToVector4(
							new NormalInput().Output,
							new FloatValue(1.0f).Output
							).Output,
						new Matrix3ToMatrix4(
							new Matrix4ToMatrix3(
								new Matrix4TimesMatrix4(
									new DistortionInput().Output,
									new TransformInput().Output
									).Output
								).Output
							).Output
						).Output
					).Output
				),
		],
	};
	
	public static readonly VertexShader SkyboxVertexShader = new()
	{
		Position = new PositionOutput(
			new Vector4XYWW(
				new Vector4TimesMatrix4(
					new Vector4TimesMatrix4(
						new Vector3ToVector4(
							new PositionInput().Output,
							new FloatValue(1.0f).Output
							).Output,
						new Matrix3ToMatrix4(
							new Matrix4ToMatrix3(
								new ViewInput().Output
								).Output
							).Output
						).Output,
					new ProjectionInput().Output
					).Output
				).Output),
		InOutBindings =
		[
			new InOutBinding<Vector3>(CubeMapTexCoordVariable, new PositionInput().Output),
		],
	};
	
	public static readonly FragmentShader CubeMapFragmentShader = new()
	{
		FragColor = new FragColorOutput(
			new CubeSampler(
				SkyboxVariable.Output,
				CubeMapTexCoordVariable.Output
				).Output
			),
		BrightColor = new BrightColorOutput(new ShaderNodeSlot<Vector4>("vec4(0, 0, 0, 1)")),
		InOutVariables =
		[
			CubeMapTexCoordVariable,
		],
		UniformVariables = [
			SkyboxVariable,
		],
	};
    
	public static string GetShaderTypeName(Type type)
	{
		return type switch
		{
			not null when type == typeof(Vector2) => "vec2",
			not null when type == typeof(Vector3) => "vec3",
			not null when type == typeof(Vector4) => "vec4",
			not null when type == typeof(CubeSampler) => "samplerCube",
			_ => throw new ArgumentException($"Invalid type {type}", nameof(type))
		};
	}
}

public class VertexShader
{
	public PositionOutput? Position { get; init; }
	public InOutBinding[] InOutBindings { get; init; } = Array.Empty<InOutBinding>();
	public UniformBinding[] UniformVariables { get; init; } = Array.Empty<UniformBinding>();
	
	public static implicit operator string(VertexShader shader) => shader.ToString();
	
	public override string ToString() => $$"""

#version 430 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;
layout (location = 3) in vec3 aColor;

{{string.Join("\n", InOutBindings.Select(binding => binding.Variable.Out))}}

layout (location = 0) uniform mat4 distortion;
layout (location = 1) uniform mat4 transform;
layout (location = 2) uniform mat4 view;
layout (location = 3) uniform mat4 projection;

{{string.Join("\n", UniformVariables.Select(variable => variable.Variable.Uniform))}}

void main()
{
    {{Position?.Output.ToString() ?? ""}}
    {{string.Join("\n    ", InOutBindings.Select(binding => binding.Code))}}
}

""";
}

public class FragmentShader
{
	public FragColorOutput? FragColor { get; init; }
	public BrightColorOutput? BrightColor { get; init; }
	public InOutVariable[] InOutVariables { get; init; } = Array.Empty<InOutVariable>();
	public UniformVariable[] UniformVariables { get; init; } = Array.Empty<UniformVariable>();
	
	public static implicit operator string(FragmentShader shader) => shader.ToString();
    
	public override string ToString() => $$"""

#version 430 core

{{string.Join("\n", InOutVariables.Select(variable => variable.In))}}

layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec4 BrightColor;

{{string.Join("\n", UniformVariables.Select(variable => variable.Uniform))}}

void main()
{
    {{FragColor?.Output.ToString() ?? ""}}
    {{BrightColor?.Output.ToString() ?? ""}}
}

""";
}

public interface ShaderNode;

public class ShaderNodeSlot<[UsedImplicitly] T>(string code)
{
	public override string ToString() => code;
}

public class DistortionInput : ShaderNode
{
	public readonly ShaderNodeSlot<Matrix4> Output = new("distortion");
}

public class TransformInput : ShaderNode
{
	public readonly ShaderNodeSlot<Matrix4> Output = new("transform");
}

public class ViewInput : ShaderNode
{
	public readonly ShaderNodeSlot<Matrix4> Output = new("view");
}

public class ProjectionInput : ShaderNode
{
	public readonly ShaderNodeSlot<Matrix4> Output = new("projection");
}

public class PositionInput : ShaderNode
{
	public readonly ShaderNodeSlot<Vector3> Output = new("aPosition");
}

public class NormalInput : ShaderNode
{
	public readonly ShaderNodeSlot<Vector3> Output = new("aNormal");
}

public class TexCoordInput : ShaderNode
{
	public readonly ShaderNodeSlot<Vector2> Output = new("aTexCoords");
}

public class VertexColorInput : ShaderNode
{
	public readonly ShaderNodeSlot<Vector3> Output = new("aColor");
}

public class PositionOutput(ShaderNodeSlot<Vector4> position) : ShaderNode
{
	public readonly ShaderNodeSlot<Vector4> Output = new($"gl_Position = {position};");
}

public class FragColorOutput(ShaderNodeSlot<Vector4> fragColor) : ShaderNode
{
	public readonly ShaderNodeSlot<Vector4> Output = new($"FragColor = {fragColor};");
}

public class BrightColorOutput(ShaderNodeSlot<Vector4> brightColor) : ShaderNode
{
	public readonly ShaderNodeSlot<Vector4> Output = new($"BrightColor = {brightColor};");
}

public class Vector4TimesMatrix4(ShaderNodeSlot<Vector4> left, ShaderNodeSlot<Matrix4> right) : ShaderNode
{
	public readonly ShaderNodeSlot<Vector4> Output = new($"({left} * {right})");
}

public class Matrix4TimesMatrix4(ShaderNodeSlot<Matrix4> left, ShaderNodeSlot<Matrix4> right) : ShaderNode
{
	public readonly ShaderNodeSlot<Matrix4> Output = new($"({left} * {right})");
}

public class Vector3ToVector4(ShaderNodeSlot<Vector3> xyz, ShaderNodeSlot<float> w) : ShaderNode
{
	public readonly ShaderNodeSlot<Vector4> Output = new($"vec4({xyz}, {w})");
}

public class Matrix3ToMatrix4(ShaderNodeSlot<Matrix3> mat3) : ShaderNode
{
	public readonly ShaderNodeSlot<Matrix4> Output = new($"mat4({mat3})");
}

public class Matrix4ToMatrix3(ShaderNodeSlot<Matrix4> mat4) : ShaderNode
{
	public readonly ShaderNodeSlot<Matrix3> Output = new($"mat3({mat4})");
}

public class Vector4XYZ(ShaderNodeSlot<Vector4> xyzw) : ShaderNode
{
	public readonly ShaderNodeSlot<Vector3> Output = new($"{xyzw}.xyz");
}

public class Vector4XYWW(ShaderNodeSlot<Vector4> xyzw) : ShaderNode
{
	public readonly ShaderNodeSlot<Vector4> Output = new($"{xyzw}.xyww");
}

public class FloatValue(float value) : ShaderNode
{
	public readonly ShaderNodeSlot<float> Output = new($"{value}");
}

public class CubeSampler(ShaderNodeSlot<CubeSampler> texture, ShaderNodeSlot<Vector3> texCoord) : ShaderNode
{
	public readonly ShaderNodeSlot<Vector4> Output = new($"texture({texture}, {texCoord})");
}

public abstract class InOutVariable(string name)
{
	public readonly string Name = name;
	protected abstract string Type { get; }
	
	public string In => $"in {Type} {Name};";
	public string Out => $"out {Type} {Name};";
}

public class InOutVariable<T>(string name) : InOutVariable(name)
{
	protected override string Type => ShaderNodeUtil.GetShaderTypeName(typeof(T));
	public readonly ShaderNodeSlot<T> Output = new($"{name}");
}

public abstract class InOutBinding(InOutVariable variable)
{
	public readonly InOutVariable Variable = variable;
	public abstract string Code { get; }
}

public class InOutBinding<T>(InOutVariable<T> variable, ShaderNodeSlot<T> input) : InOutBinding(variable)
{
	public override string Code => $"{Variable.Name} = {input};";
}

public abstract class UniformVariable(string name)
{
	public readonly string Name = name;
	protected abstract string Type { get; }
	
	public string Uniform => $"uniform {Type} {Name};";
}

public class UniformVariable<T>(string name) : UniformVariable(name)
{
	protected override string Type => ShaderNodeUtil.GetShaderTypeName(typeof(T));
	public readonly ShaderNodeSlot<T> Output = new($"{name}");
}

public abstract class UniformBinding(UniformVariable variable)
{
	public readonly UniformVariable Variable = variable;
	public abstract string Code { get; }
}

public class UniformBinding<T>(UniformVariable<T> variable, ShaderNodeSlot<T> input) : UniformBinding(variable)
{
	public override string Code => $"{Variable.Name} = {input};";
}
using System.Drawing;
using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Rendering;
using Echidna2.Serialization;
using OpenTK.Graphics.OpenGL4;
using StbSharp.MonoGame.Test;

namespace Echidna2.Gui;

public interface IHasText
{
	public string TextString { get; set; }
}

public class Text : INotificationListener<IDraw.Notification>, IHasText
{
	public static readonly Font CascadiaCode = new("Assets/CascadiaCode.ttf");
	private static readonly Shader Shader = new(ShaderNodeUtil.MainVertexShader, File.ReadAllText("Assets/font.frag"));
	
	[SerializedReference] public RectTransform RectTransform { get; set; } = null!;
	
	[SerializedValue] public string TextString { get; set; } = "";
	[SerializedValue] public Color Color { get; set; } = Color.White;
	[SerializedValue] public double LineHeight { get; set; } = 30;
	
	public double ScaledLineHeight => LineHeight * RectTransform.LocalScale.Y;
	
	[SerializedValue] public TextJustification Justification { get; set; } = TextJustification.Center;
	[SerializedValue] public TextAlignment Alignment { get; set; } = TextAlignment.Center;
	
	public void OnNotify(IDraw.Notification notification)
	{
		if (TextString.Length == 0) return;
		
		string text = TextString.Replace("\t", "    ");
		
		GL.Disable(EnableCap.CullFace);
		GL.Enable(EnableCap.Blend);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		
		Shader.Bind();
		Shader.SetMatrix4("view", notification.Camera.ViewMatrix);
		Shader.SetMatrix4("projection", notification.Camera.ProjectionMatrix);
		
		CascadiaCode.Bind();
		Shader.SetInt("texture0", 0);
		Shader.SetMatrix4("transform", RectTransform.GlobalTransform);
		Shader.SetColorRgba("color", Color);
		
		foreach ((int lineNumber, string line) in text.Split("\n").Enumerate())
			DrawTextLine(line, lineNumber);
	}
	
	private void DrawTextLine(string text, int lineNumber)
	{
		int midLineOffset = 0;
		
		Vector2 relativeRectSize = RectTransform.GlobalSize;
		
		Vector2 size = (
			text.Select(c => CascadiaCode.FontResult!.Glyphs[c]).Sum(glyph => glyph.XAdvance),
			LineHeight);
		
		Shader.SetMatrix4("distortion", Matrix4.FromTranslation(new Vector3(
			Justification switch
			{
				TextJustification.Left => -relativeRectSize.X / 2,
				TextJustification.Center => -size.X / 2,
				TextJustification.Right => relativeRectSize.X / 2 - size.X,
				_ => throw new IndexOutOfRangeException()
			},
			-1 * Alignment switch
			{
				TextAlignment.Top => relativeRectSize.Y / 2 - size.Y,
				TextAlignment.Center => -size.Y / 2,
				TextAlignment.Bottom => -relativeRectSize.Y,
				_ => throw new IndexOutOfRangeException()
			} + midLineOffset - LineHeight * lineNumber,
			0)));
		
		float xStart = 0;
		foreach (GlyphInfo glyph in text.Select(c => CascadiaCode.FontResult!.Glyphs[c]))
		{
			float x = xStart + glyph.XOffset;
			float y = -glyph.Height - glyph.YOffset;
			float w = glyph.Width;
			float h = glyph.Height;
			
			float u = (float)glyph.X / Font.TextureSize;
			float v = (float)glyph.Y / Font.TextureSize;
			float uw = (float)glyph.Width / Font.TextureSize;
			float vh = (float)glyph.Height / Font.TextureSize;
			
			const int stride = Mesh.DataStride;
			const int xPos = 0;
			const int yPos = 1;
			const int uPos = 6;
			const int vPos = 7;
			
			float[] vertices = Font.Vertices;
			
			vertices[0 * stride + xPos] = x;
			vertices[0 * stride + yPos] = y + h;
			vertices[0 * stride + uPos] = u;
			vertices[0 * stride + vPos] = v;
			
			vertices[1 * stride + xPos] = x;
			vertices[1 * stride + yPos] = y;
			vertices[1 * stride + uPos] = u;
			vertices[1 * stride + vPos] = v + vh;
			
			vertices[2 * stride + xPos] = x + w;
			vertices[2 * stride + yPos] = y;
			vertices[2 * stride + uPos] = u + uw;
			vertices[2 * stride + vPos] = v + vh;
			
			vertices[3 * stride + xPos] = x;
			vertices[3 * stride + yPos] = y + h;
			vertices[3 * stride + uPos] = u;
			vertices[3 * stride + vPos] = v;
			
			vertices[4 * stride + xPos] = x + w;
			vertices[4 * stride + yPos] = y;
			vertices[4 * stride + uPos] = u + uw;
			vertices[4 * stride + vPos] = v + vh;
			
			vertices[5 * stride + xPos] = x + w;
			vertices[5 * stride + yPos] = y + h;
			vertices[5 * stride + uPos] = u + uw;
			vertices[5 * stride + vPos] = v;
			
			GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices.Length * sizeof(float), vertices);
			
			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
			
			xStart += glyph.XAdvance;
		}
	}
}

public enum TextJustification
{
	Left,
	Center,
	Right,
}

public enum TextAlignment
{
	Top,
	Center,
	Bottom,
}
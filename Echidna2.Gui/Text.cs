using System.Drawing;
using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Rendering;
using OpenTK.Graphics.OpenGL4;
using StbSharp.MonoGame.Test;

namespace Echidna2.Gui;

public class Text(RectTransform rectTransform) : INotificationListener<IDraw.Notification>
{
	private static Font font = new("CascadiaCode.ttf");
	private static Shader shader = new(ShaderNodeUtil.MainVertexShader, File.ReadAllText("font.frag"));
	
	public string TextString { get; set; } = "";
	public Color Color { get; set; } = Color.White;
	
	public void OnNotify(IDraw.Notification notification)
	{
		GL.Disable(EnableCap.CullFace);
		GL.Enable(EnableCap.Blend);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		
		shader.Bind(notification.Camera.ViewMatrix, notification.Camera.ProjectionMatrix);
		
		font.Bind();
		shader.SetInt("texture0", 0);
		
		Vector2 size = (
			TextString.Select(c => font.FontResult!.Glyphs[c]).Sum(glyph => glyph.XAdvance),
			TextString.Select(c => font.FontResult!.Glyphs[c]).Max(glyph => glyph.Height));
		
		shader.SetMatrix4("distortion", Matrix4.Translation(new Vector3(-size.X / 2, size.Y / 2, 0)));
		shader.SetMatrix4("transform", rectTransform.GlobalTransform);
		shader.SetColor("color", Color);
		
		float xStart = 0;
		foreach (GlyphInfo glyph in TextString.Select(c => font.FontResult!.Glyphs[c]))
		{
			float x = xStart + glyph.XOffset;
			float y = -glyph.Height - glyph.YOffset;
			float w = glyph.Width;
			float h = glyph.Height;
			
			float u = (float)glyph.X / Font.TextureSize;
			float v = (float)glyph.Y / Font.TextureSize;
			float uw = (float)glyph.Width / Font.TextureSize;
			float vh = (float)glyph.Height / Font.TextureSize;
			
			float[] vertices = Font.Vertices;
			
			vertices[00] = x;
			vertices[01] = y + h;
			vertices[03] = u;
			vertices[04] = v;
			
			vertices[08] = x;
			vertices[09] = y;
			vertices[11] = u;
			vertices[12] = v + vh;
			
			vertices[16] = x + w;
			vertices[17] = y;
			vertices[19] = u + uw;
			vertices[20] = v + vh;
			
			vertices[24] = x;
			vertices[25] = y + h;
			vertices[27] = u;
			vertices[28] = v;
			
			vertices[32] = x + w;
			vertices[33] = y;
			vertices[35] = u + uw;
			vertices[36] = v + vh;
			
			vertices[40] = x + w;
			vertices[41] = y + h;
			vertices[43] = u + uw;
			vertices[44] = v;
			
			GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices.Length * sizeof(float), vertices);
			
			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
			
			xStart += glyph.XAdvance;
		}
	}
}
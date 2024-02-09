using Echidna2.Core;
using OpenTK.Graphics.OpenGL4;

namespace Echidna2.Rendering;

public partial class Rect(
	[Component] IRectTransform? rectTransform = null)
	: IDraw
{
	private Mesh mesh = new([
		-1.0f, +0.0f, -1.0f,
		+1.0f, +0.0f, -1.0f,
		-1.0f, +0.0f, +1.0f,
		+1.0f, +0.0f, +1.0f
	], [
		0.0f, 0.0f,
		1.0f, 0.0f,
		0.0f, 1.0f,
		1.0f, 1.0f
	], [
		0.0f, 0.0f, 0.0f,
		1.0f, 0.0f, 0.0f,
		0.0f, 1.0f, 0.0f,
		0.0f, 0.0f, 1.0f
	], [
		0, 1, 2,
		2, 1, 3
	], false);
	
	public void Draw()
	{
		if (mesh.CullBackFaces)
			GL.Enable(EnableCap.CullFace);
		else
			GL.Disable(EnableCap.CullFace);
		
		GL.Disable(EnableCap.Blend);
		
		GL.BindVertexArray(mesh.VertexArrayObject);
		GL.DrawElements(PrimitiveType.Triangles, mesh.Indices.Length, DrawElementsType.UnsignedInt, 0);
	}
}
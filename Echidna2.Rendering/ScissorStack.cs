using System.Drawing;
using OpenTK.Graphics.OpenGL4;

namespace Echidna2.Rendering;

public static class ScissorStack
{
	private static List<Rectangle> scissors = [];
	
	public static void PushScissor(double x, double y, double width, double height) => PushScissor(new Rectangle((int)x, (int)y, (int)width, (int)height));
	public static void PushScissor(int x, int y, int width, int height) => PushScissor(new Rectangle(x, y, width, height));
	public static void PushScissor(Rectangle scissor)
	{
		if (scissors.Count == 0)
		{
			GL.Enable(EnableCap.ScissorTest);
		}
		else
		{
			Rectangle parentScissor = scissors.Last();
			scissor = Rectangle.Intersect(parentScissor, scissor);
		}
		scissors.Add(scissor);
		GL.Scissor(scissor.X, scissor.Y, scissor.Width, scissor.Height);
	}
	
	public static void PopScissor()
	{
		scissors.RemoveAt(scissors.Count - 1);
		if (scissors.Count == 0)
		{
			GL.Disable(EnableCap.ScissorTest);
		}
		else
		{
			Rectangle scissor = scissors.Last();
			GL.Scissor(scissor.X, scissor.Y, scissor.Width, scissor.Height);
		}
	}
}
namespace Echidna2.Core;

public struct Vector2(double x, double y)
{
	public static Vector2 Zero => new(0, 0);
	public static Vector2 One => new(1, 1);
	
	public double X = x, Y = y;
	
	public override string ToString() => $"({X}, {Y})";
	
	public static Vector2 operator+(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
	public static Vector2 operator-(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
	
	public static Vector2 operator*(Vector2 a, double b) => new(a.X * b, a.Y * b);
	public static Vector2 operator*(double a, Vector2 b) => new(a * b.X, a * b.Y);
}
using System.Collections;
using Vector2System = System.Numerics.Vector2;
using Vector2OpenTK = OpenTK.Mathematics.Vector2;

namespace Echidna2.Mathematics;

public struct Vector2(double x, double y) : IEquatable<Vector2>, IEnumerable<double>
{
	public double X = x;
	public double Y = y;
	
	public Vector3 WithZ(double z) => new(X, Y, z);
	
	public double Length => Math.Sqrt(LengthSquared);
	public double LengthSquared => X * X + Y * Y;
	public Vector2 Normalized => this / Length;
	public bool IsNaN => double.IsNaN(X) || double.IsNaN(Y);
	
	public override int GetHashCode() => HashCode.Combine(X, Y);
	public override bool Equals(object? obj) => obj is Vector2 other && other == this;
	public bool Equals(Vector2 other) => other == this;
	public override string ToString() => $"<{X}, {Y}>";
	
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	public IEnumerator<double> GetEnumerator()
	{
		yield return X;
		yield return Y;
	}
	
	public static readonly Vector2 Right = new(1, 0);
	public static readonly Vector2 East = new(1, 0);
	public static readonly Vector2 Left = new(-1, 0);
	public static readonly Vector2 West = new(-1, 0);
	public static readonly Vector2 Up = new(0, 1);
	public static readonly Vector2 North = new(0, 1);
	public static readonly Vector2 Forward = new(0, 1);
	public static readonly Vector2 Down = new(0, -1);
	public static readonly Vector2 South = new(0, -1);
	public static readonly Vector2 Backward = new(0, -1);
	public static readonly Vector2 One = new(1, 1);
	public static readonly Vector2 Zero = new(0, 0);
	
	public double Cross(Vector2 other) => Cross(this, other);
	public double Dot(Vector2 other) => Dot(this, other);
	public Vector2 RotatedBy(double angle) => new(X * Math.Cos(angle) - Y * Math.Sin(angle), X * Math.Sin(angle) + Y * Math.Cos(angle));
	public double DistanceTo(Vector2 other) => (other - this).Length;
	public double DistanceToSquared(Vector2 other) => (other - this).LengthSquared;
	
	public static Vector2 operator +(Vector2 a) => a;
	public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
	public static Vector2 operator -(Vector2 a) => new(-a.X, -a.Y);
	public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
	public static Vector2 operator *(Vector2 vector, double scalar) => new(vector.X * scalar, vector.Y * scalar);
	public static Vector2 operator *(double scalar, Vector2 vector) => new(vector.X * scalar, vector.Y * scalar);
	public static Vector2 operator /(Vector2 vector, double scalar) => new(vector.X / scalar, vector.Y / scalar);
	public static bool operator ==(Vector2 a, Vector2 b) => Math.Abs(a.X - b.X) < double.Epsilon && Math.Abs(a.Y - b.Y) < double.Epsilon;
	public static bool operator !=(Vector2 a, Vector2 b) => !(a == b);
	
	public static implicit operator Vector2System(Vector2 vector) => new((float)vector.X, (float)vector.Y);
	public static implicit operator Vector2(Vector2System vector) => new(vector.X, vector.Y);
	public static implicit operator Vector2OpenTK(Vector2 vector) => new((float)vector.X, (float)vector.Y);
	public static implicit operator Vector2(Vector2OpenTK vector) => new(vector.X, vector.Y);
	public static implicit operator Vector2((double X, double Y) vector) => new(vector.X, vector.Y);
	
	public static double Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;
	public static double Dot(Vector2 a, Vector2 b) => a.X * b.X + a.Y * b.Y;
	public static double AngleBetween(Vector2 a, Vector2 b) => Math.Acos(Dot(a, b) / (a.Length * b.Length));
	
	public static Vector2 Sum<T>(IEnumerable<T> source, Func<T, Vector2> selector) => source.Aggregate(Zero, (current, item) => current + selector(item));
}
using System.Runtime.CompilerServices;
using QuaternionSystem = System.Numerics.Quaternion;
using QuaternionOpenTK = OpenTK.Mathematics.Quaternion;

namespace Echidna2.Mathematics;

public struct Quaternion(double x, double y, double z, double w) : IEquatable<Quaternion>
{
	public double X = x;
	public double Y = y;
	public double Z = z;
	public double W = w;
	
	public Vector3 XYZ => new(X, Y, Z);
	
	public override int GetHashCode() => HashCode.Combine(X, Y, Z, W);
	public override bool Equals(object? obj) => obj is Quaternion other && other == this;
	public bool Equals(Quaternion other) => other == this;
	public override string ToString() => $"<{X}, {Y}, {Z}, {W}>";
	
	public static Quaternion FromVector(Vector3 xyz, double w) => new(xyz.X, xyz.Y, xyz.Z, w);
	
	public static Quaternion FromAxisAngle(Vector3 axis, float angle)
	{
		double halfAngle = DegreesToRadians(angle) * 0.5f;
		double sin = Math.Sin(halfAngle);
		double cos = Math.Cos(halfAngle);
		return new Quaternion(axis.X * sin, axis.Y * sin, axis.Z * sin, cos);
	}
	
	/// <summary>
	/// Rotates by roll, then pitch, then yaw.
	/// </summary>
	public static Quaternion FromEulerAngles(double pitch, double roll, double yaw)
	{
		double halfRoll = DegreesToRadians(roll) * 0.5;
		double sinRoll = Math.Sin(halfRoll);
		double cosRoll = Math.Cos(halfRoll);
		
		double halfPitch = DegreesToRadians(pitch) * 0.5;
		double sinPitch = Math.Sin(halfPitch);
		double cosPitch = Math.Cos(halfPitch);
		
		double halfYaw = DegreesToRadians(yaw) * 0.5;
		double sinYaw = Math.Sin(halfYaw);
		double cosYaw = Math.Cos(halfYaw);
		
		return new Quaternion(
			cosYaw * sinPitch * cosRoll + sinYaw * cosPitch * sinRoll,
			cosYaw * cosPitch * sinRoll + sinYaw * sinPitch * cosRoll,
			sinYaw * cosPitch * cosRoll - cosYaw * sinPitch * sinRoll,
			cosYaw * cosPitch * cosRoll + sinYaw * sinPitch * sinRoll);
	}
	
	/// <summary>
	/// Rotates by roll (y), then pitch (x), then yaw (z).
	/// </summary>
	public static Quaternion FromEulerAngles(Vector3 angles) => FromEulerAngles(angles.X, angles.Y, angles.Z);
	
	public static Quaternion LookToward(Vector3 forward, Vector3 up)
	{
		// https://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/
		// maybe https://gamedev.net/forums/topic/648857-how-to-implement-lookrotation/5113120/ ?
		// https://www.andre-gaschler.com/rotationconverter/
		
		forward = forward.Normalized;
		Vector3 right = forward.Cross(up).Normalized;
		up = right.Cross(forward);
		
		double m00 = right.X;
		double m01 = right.Y;
		double m02 = right.Z;
		double m20 = up.X;
		double m21 = up.Y;
		double m22 = up.Z;
		double m10 = forward.X;
		double m11 = forward.Y;
		double m12 = forward.Z;
		
		double trace = m00 + m11 + m22;
		if (trace > 0)
		{
			double s = Math.Sqrt(trace + 1) * 2;
			return new Quaternion(
				(m12 - m21) / s,
				(m20 - m02) / s,
				(m01 - m10) / s,
				0.25f * s);
		}
		else if (m00 > m11 && m00 > m22)
		{
			double s = Math.Sqrt(1 + m00 - m11 - m22) * 2;
			return new Quaternion(
				0.25f * s,
				(m01 + m10) / s,
				(m02 + m20) / s,
				(m12 - m21) / s);
		}
		else if (m11 > m22)
		{
			double s = Math.Sqrt(1 + m11 - m00 - m22) * 2;
			return new Quaternion(
				(m01 + m10) / s,
				0.25f * s,
				(m12 + m21) / s,
				(m20 - m02) / s);
		}
		else
		{
			double s = Math.Sqrt(1 + m22 - m00 - m11) * 2;
			return new Quaternion(
				(m02 + m20) / s,
				(m12 + m21) / s,
				0.25f * s,
				(m01 - m10) / s);
		}
	}
	public static Quaternion LookAt(Vector3 position, Vector3 target, Vector3 up) => LookToward(target - position, up);
	
	public static readonly Quaternion Identity = new(0, 0, 0, 1);
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 operator *(Quaternion quaternion, Vector3 vector)
	{
		double x2 = quaternion.X + quaternion.X;
		double y2 = quaternion.Y + quaternion.Y;
		double z2 = quaternion.Z + quaternion.Z;
		
		double wx2 = quaternion.W * x2;
		double wy2 = quaternion.W * y2;
		double wz2 = quaternion.W * z2;
		double xx2 = quaternion.X * x2;
		double xy2 = quaternion.X * y2;
		double xz2 = quaternion.X * z2;
		double yy2 = quaternion.Y * y2;
		double yz2 = quaternion.Y * z2;
		double zz2 = quaternion.Z * z2;
		
		return new Vector3(
			vector.X * (1.0f - yy2 - zz2) + vector.Y * (xy2 - wz2) + vector.Z * (xz2 + wy2),
			vector.X * (xy2 + wz2) + vector.Y * (1.0f - xx2 - zz2) + vector.Z * (yz2 - wx2),
			vector.X * (xz2 - wy2) + vector.Y * (yz2 + wx2) + vector.Z * (1.0f - xx2 - yy2)
		);
	}
	public static Quaternion operator *(Quaternion a, Quaternion b) => FromVector(b.W * a.XYZ + a.W * b.XYZ + a.XYZ.Cross(b.XYZ), a.W * b.W - a.XYZ.Dot(b.XYZ));
	public static bool operator ==(Quaternion a, Quaternion b) => Math.Abs(a.X - b.X) < double.Epsilon && Math.Abs(a.Y - b.Y) < double.Epsilon && Math.Abs(a.Z - b.Z) < double.Epsilon && Math.Abs(a.W - b.W) < double.Epsilon;
	public static bool operator !=(Quaternion a, Quaternion b) => !(a == b);
	
	public static implicit operator QuaternionSystem(Quaternion quaternion) => new((float)quaternion.X, (float)quaternion.Y, (float)quaternion.Z, (float)quaternion.W);
	public static implicit operator Quaternion(QuaternionSystem quaternion) => new(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
	public static implicit operator QuaternionOpenTK(Quaternion quaternion) => new((float)quaternion.X, (float)quaternion.Y, (float)quaternion.Z, (float)quaternion.W);
	public static implicit operator Quaternion(QuaternionOpenTK quaternion) => new(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
	public static implicit operator Quaternion((float pitch, float roll, float yaw) eulers) => FromEulerAngles(eulers);
	public static implicit operator Quaternion((float X, float Y, float Z, float W) quaternion) => new(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
	
	public static double DegreesToRadians(double angle) => angle / 180 * Math.PI;
	public static double RadiansToDegrees(double angle) => angle * 180 / Math.PI;
}
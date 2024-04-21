using Echidna2.Serialization;
using JetBrains.Annotations;
using QuaternionSystem = System.Numerics.Quaternion;
using QuaternionOpenTK = OpenTK.Mathematics.Quaternion;

namespace Echidna2.Mathematics;

public struct Quaternion(double x, double y, double z, double w) : IEquatable<Quaternion>
{
	[SerializedValue] public double X = x;
	[SerializedValue] public double Y = y;
	[SerializedValue] public double Z = z;
	[SerializedValue] public double W = w;
	
	public Vector3 XYZ => new(X, Y, Z);
	
	public override int GetHashCode() => HashCode.Combine(X, Y, Z, W);
	public override bool Equals(object? obj) => obj is Quaternion other && other == this;
	public bool Equals(Quaternion other) => other == this;
	public override string ToString() => $"<{X}, {Y}, {Z}, {W}>";
	
	public static Quaternion FromVector(Vector3 xyz, double w) => new(xyz.X, xyz.Y, xyz.Z, w);
	
	public static Quaternion FromAxisAngle(Vector3 axis, float angle)
	{
		double halfAngle = angle * 0.5f;
		double sin = Math.Sin(halfAngle);
		double cos = Math.Cos(halfAngle);
		return new Quaternion(axis.X * sin, axis.Y * sin, axis.Z * sin, cos);
	}
	
	/// <summary>
	/// Rotates by yaw, then pitch, then roll.
	/// </summary>
	public static Quaternion FromEulerAngles(double pitch, double yaw, double roll)
	{
		double halfRoll = roll * 0.5;
		double sinRoll = Math.Sin(halfRoll);
		double cosRoll = Math.Cos(halfRoll);
		
		double halfPitch = pitch * 0.5;
		double sinPitch = Math.Sin(halfPitch);
		double cosPitch = Math.Cos(halfPitch);
		
		double halfYaw = yaw * 0.5;
		double sinYaw = Math.Sin(halfYaw);
		double cosYaw = Math.Cos(halfYaw);
		
		return new Quaternion(
			cosYaw * sinPitch * cosRoll + sinYaw * cosPitch * sinRoll,
			cosYaw * cosPitch * sinRoll - sinYaw * sinPitch * cosRoll,
			sinYaw * cosPitch * cosRoll + cosYaw * sinPitch * sinRoll,
			cosYaw * cosPitch * cosRoll - sinYaw * sinPitch * sinRoll);
	}
	
	/// <summary>
	/// Rotates by roll (y), then pitch (x), then yaw (z).
	/// </summary>
	public static Quaternion FromEulerAngles(Vector3 angles) => FromEulerAngles(angles.X, angles.Y, angles.Z);
	
	/// <summary>
	/// Returns a rotation by roll (y), then pitch (x), then yaw (z).
	/// </summary>
	[Pure]
	public Vector3 ToEulerAngles()
	{
		Vector3 angles;
		
		double sinrCosp = 2 * (W * X + Y * Z);
		double cosrCosp = 1 - 2 * (X * X + Y * Y);
		angles.X = Math.Atan2(sinrCosp, cosrCosp);
		
		double sinp = Math.Sqrt(1 + 2 * (W * Y - X * Z));
		double cosp = Math.Sqrt(1 - 2 * (W * Y - X * Z));
		angles.Y = 2 * Math.Atan2(sinp, cosp) - Math.PI / 2;
		
		double sinyCosp = 2 * (W * Z + X * Y);
		double cosyCosp = 1 - 2 * (Y * Y + Z * Z);
		angles.Z = Math.Atan2(sinyCosp, cosyCosp);
		return angles;
	}
	
	public static Quaternion LookToward(Vector3 forward, Vector3 up, Vector3 fallbackUp = default)
	{
		// https://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/
		// maybe https://gamedev.net/forums/topic/648857-how-to-implement-lookrotation/5113120/ ?
		// https://www.andre-gaschler.com/rotationconverter/
		// https://stackoverflow.com/questions/52413464/look-at-quaternion-using-up-vector
		
		if (up.Cross(forward).IsNaN)
			up = fallbackUp;
		
		forward = -forward.Normalized;
		Vector3 right = up.Cross(forward).Normalized;
		up = forward.Cross(right);
		
		if (forward.IsNaN || right.IsNaN || up.IsNaN)
			return Identity;
		
		return new Matrix4(
			right.X, up.X, forward.X, 0,
			right.Y, up.Y, forward.Y, 0,
			right.Z, up.Z, forward.Z, 0,
			0, 0, 0, 1).Rotation;
	}
	public static Quaternion LookAt(Vector3 position, Vector3 target, Vector3 up, Vector3 fallbackUp = default) => LookToward(target - position, up);
	
	public static readonly Quaternion Identity = new(0, 0, 0, 1);
	
	public static Vector3 operator *(Quaternion quaternion, Vector3 vector) => Matrix4.FromRotation(quaternion) * vector;
	public static Vector2 operator *(Quaternion quaternion, Vector2 vector) => Matrix4.FromRotation(quaternion) * vector;
	public static Quaternion operator *(Quaternion a, Quaternion b) => FromVector(b.W * a.XYZ + a.W * b.XYZ + a.XYZ.Cross(b.XYZ), a.W * b.W - a.XYZ.Dot(b.XYZ));
	public static bool operator ==(Quaternion a, Quaternion b) => Math.Abs(a.X - b.X) < double.Epsilon && Math.Abs(a.Y - b.Y) < double.Epsilon && Math.Abs(a.Z - b.Z) < double.Epsilon && Math.Abs(a.W - b.W) < double.Epsilon;
	public static bool operator !=(Quaternion a, Quaternion b) => !(a == b);
	
	public static implicit operator QuaternionSystem(Quaternion quaternion) => new((float)-quaternion.X, (float)-quaternion.Y, (float)-quaternion.Z, (float)quaternion.W);
	public static implicit operator Quaternion(QuaternionSystem quaternion) => new(-quaternion.X, -quaternion.Y, -quaternion.Z, quaternion.W);
	public static implicit operator QuaternionOpenTK(Quaternion quaternion) => new((float)quaternion.X, (float)quaternion.Y, (float)quaternion.Z, (float)quaternion.W);
	public static implicit operator Quaternion(QuaternionOpenTK quaternion) => new(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
	public static implicit operator Quaternion((float pitch, float roll, float yaw) eulers) => FromEulerAngles(eulers);
	public static implicit operator Quaternion((float X, float Y, float Z, float W) quaternion) => new(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
}
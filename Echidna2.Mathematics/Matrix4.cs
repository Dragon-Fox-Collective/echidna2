using Matrix4System = System.Numerics.Matrix4x4;
using Matrix4OpenTK = OpenTK.Mathematics.Matrix4;

namespace Echidna2.Mathematics;

public struct Matrix4(
	double m11, double m12, double m13, double m14,
	double m21, double m22, double m23, double m24,
	double m31, double m32, double m33, double m34,
	double m41, double m42, double m43, double m44)
	: IEquatable<Matrix4>
{
	public double M11 = m11;
	public double M12 = m12;
	public double M13 = m13;
	public double M14 = m14;
	public double M21 = m21;
	public double M22 = m22;
	public double M23 = m23;
	public double M24 = m24;
	public double M31 = m31;
	public double M32 = m32;
	public double M33 = m33;
	public double M34 = m34;
	public double M41 = m41;
	public double M42 = m42;
	public double M43 = m43;
	public double M44 = m44;
	
	public Matrix4 Inverted
	{
		get
		{
			double det = Determinant;
			if (Math.Abs(det) < double.Epsilon)
				throw new InvalidOperationException("Matrix is singular and cannot be inverted");
			double m11 = M22 * M33 * M44 + M23 * M34 * M42 + M24 * M32 * M43 - M22 * M34 * M43 - M23 * M32 * M44 - M24 * M33 * M42;
			double m12 = M12 * M34 * M43 + M13 * M32 * M44 + M14 * M33 * M42 - M12 * M33 * M44 - M13 * M34 * M42 - M14 * M32 * M43;
			double m13 = M12 * M23 * M44 + M13 * M24 * M42 + M14 * M22 * M43 - M12 * M24 * M43 - M13 * M22 * M44 - M14 * M23 * M42;
			double m14 = M12 * M24 * M33 + M13 * M22 * M34 + M14 * M23 * M32 - M12 * M23 * M34 - M13 * M24 * M32 - M14 * M22 * M33;
			double m21 = M21 * M34 * M43 + M23 * M31 * M44 + M24 * M33 * M41 - M21 * M33 * M44 - M23 * M34 * M41 - M24 * M31 * M43;
			double m22 = M11 * M33 * M44 + M13 * M34 * M41 + M14 * M31 * M43 - M11 * M34 * M43 - M13 * M31 * M44 - M14 * M33 * M41;
			double m23 = M11 * M24 * M43 + M13 * M21 * M44 + M14 * M23 * M41 - M11 * M23 * M44 - M13 * M24 * M41 - M14 * M21 * M43;
			double m24 = M11 * M23 * M34 + M13 * M24 * M31 + M14 * M21 * M33 - M11 * M24 * M33 - M13 * M21 * M34 - M14 * M23 * M31;
			double m31 = M21 * M32 * M44 + M22 * M34 * M41 + M24 * M31 * M42 - M21 * M34 * M42 - M22 * M31 * M44 - M24 * M32 * M41;
			double m32 = M11 * M34 * M42 + M12 * M31 * M44 + M14 * M32 * M41 - M11 * M32 * M44 - M12 * M34 * M41 - M14 * M31 * M42;
			double m33 = M11 * M22 * M44 + M12 * M24 * M41 + M14 * M21 * M42 - M11 * M24 * M42 - M12 * M21 * M44 - M14 * M22 * M41;
			double m34 = M11 * M24 * M32 + M12 * M21 * M34 + M14 * M22 * M31 - M11 * M22 * M34 - M12 * M24 * M31 - M14 * M21 * M32;
			double m41 = M21 * M33 * M42 + M22 * M31 * M43 + M23 * M32 * M41 - M21 * M32 * M43 - M22 * M33 * M41 - M23 * M31 * M42;
			double m42 = M11 * M32 * M43 + M12 * M33 * M41 + M13 * M31 * M42 - M11 * M33 * M42 - M12 * M31 * M43 - M13 * M32 * M41;
			double m43 = M11 * M23 * M42 + M12 * M21 * M43 + M13 * M22 * M41 - M11 * M22 * M43 - M12 * M23 * M41 - M13 * M21 * M42;
			double m44 = M11 * M22 * M33 + M12 * M23 * M31 + M13 * M21 * M32 - M11 * M23 * M32 - M12 * M21 * M33 - M13 * M22 * M31;
			return new Matrix4(
				m11 / det, m12 / det, m13 / det, m14 / det,
				m21 / det, m22 / det, m23 / det, m24 / det,
				m31 / det, m32 / det, m33 / det, m34 / det,
				m41 / det, m42 / det, m43 / det, m44 / det);
		}
	}
	public double Determinant =>
		M11 * M22 * M33 * M44
		+ M11 * M23 * M34 * M42
		+ M11 * M24 * M32 * M43
		+ M12 * M21 * M34 * M43
		+ M12 * M23 * M31 * M44
		+ M12 * M24 * M33 * M41
		+ M13 * M21 * M32 * M44
		+ M13 * M22 * M34 * M41
		+ M13 * M24 * M31 * M42
		+ M14 * M21 * M33 * M42
		+ M14 * M22 * M31 * M43
		+ M14 * M23 * M32 * M41
		- M11 * M22 * M34 * M43
		- M11 * M23 * M32 * M44
		- M11 * M24 * M33 * M42
		- M12 * M21 * M33 * M44
		- M12 * M23 * M34 * M41
		- M12 * M24 * M31 * M43
		- M13 * M21 * M34 * M42
		- M13 * M22 * M31 * M44
		- M13 * M24 * M32 * M41
		- M14 * M21 * M32 * M43
		- M14 * M22 * M33 * M41
		- M14 * M23 * M31 * M42;
	public Matrix4 Transposed => new(
		M11, M21, M31, M41,
		M12, M22, M32, M42,
		M13, M23, M33, M43,
		M14, M24, M34, M44);
	
	public Vector3 Translation => new(M14, M24, M34);
	
	public static Matrix4 FromTranslation(Vector3 translation) => new(
		1, 0, 0, translation.X,
		0, 1, 0, translation.Y,
		0, 0, 1, translation.Z,
		0, 0, 0, 1);
	
	public static Matrix4 FromRotation(Quaternion rotation)
	{
		double x = rotation.X;
		double y = rotation.Y;
		double z = rotation.Z;
		double w = rotation.W;
		double x2 = x + x;
		double y2 = y + y;
		double z2 = z + z;
		double xx = x * x2;
		double xy = x * y2;
		double xz = x * z2;
		double yy = y * y2;
		double yz = y * z2;
		double zz = z * z2;
		double wx = w * x2;
		double wy = w * y2;
		double wz = w * z2;
		return new Matrix4(
			1 - (yy + zz), xy + wz, xz - wy, 0,
			xy - wz, 1 - (xx + zz), yz + wx, 0,
			xz + wy, yz - wx, 1 - (xx + yy), 0,
			0, 0, 0, 1);
	}
	
	public static Matrix4 FromScale(Vector3 scale) => new(
		scale.X, 0, 0, 0,
		0, scale.Y, 0, 0,
		0, 0, scale.Z, 0,
		0, 0, 0, 1);
	
	public static Matrix4 FromOrthographicProjection(double width, double height, double zNear, double zFar) => new(
		2 / width, 0, 0, 0,
		0, 2 / height, 0, 0,
		0, 0, 1 / (zNear - zFar), zNear / (zNear - zFar),
		0, 0, 0, 1);
	
	public static Matrix4 FromPerspectiveProjection(double fieldOfView, double aspectRatio, double zNear, double zFar)
	{
		double yMax = zNear * Math.Tan(fieldOfView * Math.PI / 360);
		double yMin = -yMax;
		double xMin = yMin * aspectRatio;
		double xMax = yMax * aspectRatio;
		return new Matrix4(
			2 * zNear / (xMax - xMin), 0, (xMax + xMin) / (xMax - xMin), 0,
			0, 2 * zNear / (yMax - yMin), (yMax + yMin) / (yMax - yMin), 0,
			0, 0, (zFar + zNear) / (zNear - zFar), 2 * zFar * zNear / (zNear - zFar),
			0, 0, -1, 0);
	}
	
	public override int GetHashCode() => HashCode.Combine(HashCode.Combine(M11, M12, M13, M14, M21, M22, M23, M24), HashCode.Combine(M31, M32, M33, M34, M41, M42, M43, M44));
	public override bool Equals(object? obj) => obj is Matrix4 other && other == this;
	public bool Equals(Matrix4 other) => other == this;
	public override string ToString() => $"[[{M11}, {M12}, {M13}, {M14}]\n [{M21}, {M22}, {M23}, {M24}]\n [{M31}, {M32}, {M33}, {M34}]\n [{M41}, {M42}, {M43}, {M44}]]";
	
	public static readonly Matrix4 Identity = new(
		1, 0, 0, 0,
		0, 1, 0, 0,
		0, 0, 1, 0,
		0, 0, 0, 1);
	
	public static Matrix4 operator *(Matrix4 a, Matrix4 b) =>
		new(
			a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41,
			a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42,
			a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43,
			a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44,
			a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41,
			a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42,
			a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43,
			a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44,
			a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41,
			a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42,
			a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43,
			a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44,
			a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41,
			a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42,
			a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43,
			a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44);
	public static bool operator ==(Matrix4 a, Matrix4 b) =>
		Math.Abs(a.M11 - b.M11) < double.Epsilon
		&& Math.Abs(a.M12 - b.M12) < double.Epsilon
		&& Math.Abs(a.M13 - b.M13) < double.Epsilon
		&& Math.Abs(a.M14 - b.M14) < double.Epsilon
		&& Math.Abs(a.M21 - b.M21) < double.Epsilon
		&& Math.Abs(a.M22 - b.M22) < double.Epsilon
		&& Math.Abs(a.M23 - b.M23) < double.Epsilon
		&& Math.Abs(a.M24 - b.M24) < double.Epsilon
		&& Math.Abs(a.M31 - b.M31) < double.Epsilon
		&& Math.Abs(a.M32 - b.M32) < double.Epsilon
		&& Math.Abs(a.M33 - b.M33) < double.Epsilon
		&& Math.Abs(a.M34 - b.M34) < double.Epsilon
		&& Math.Abs(a.M41 - b.M41) < double.Epsilon
		&& Math.Abs(a.M42 - b.M42) < double.Epsilon
		&& Math.Abs(a.M43 - b.M43) < double.Epsilon
		&& Math.Abs(a.M44 - b.M44) < double.Epsilon;
	public static bool operator !=(Matrix4 a, Matrix4 b) => !(a == b);
	
	public static implicit operator Matrix4System(Matrix4 matrix) => new(
		(float)matrix.M11, (float)matrix.M12, (float)matrix.M13, (float)matrix.M14,
		(float)matrix.M21, (float)matrix.M22, (float)matrix.M23, (float)matrix.M24,
		(float)matrix.M31, (float)matrix.M32, (float)matrix.M33, (float)matrix.M34,
		(float)matrix.M41, (float)matrix.M42, (float)matrix.M43, (float)matrix.M44);
	public static implicit operator Matrix4(Matrix4System matrix) => new(
		matrix.M11, matrix.M12, matrix.M13, matrix.M14,
		matrix.M21, matrix.M22, matrix.M23, matrix.M24,
		matrix.M31, matrix.M32, matrix.M33, matrix.M34,
		matrix.M41, matrix.M42, matrix.M43, matrix.M44);
	public static implicit operator Matrix4OpenTK(Matrix4 matrix) => new(
		(float)matrix.M11, (float)matrix.M12, (float)matrix.M13, (float)matrix.M14,
		(float)matrix.M21, (float)matrix.M22, (float)matrix.M23, (float)matrix.M24,
		(float)matrix.M31, (float)matrix.M32, (float)matrix.M33, (float)matrix.M34,
		(float)matrix.M41, (float)matrix.M42, (float)matrix.M43, (float)matrix.M44);
	public static implicit operator Matrix4(Matrix4OpenTK matrix) => new(
		matrix.M11, matrix.M12, matrix.M13, matrix.M14,
		matrix.M21, matrix.M22, matrix.M23, matrix.M24,
		matrix.M31, matrix.M32, matrix.M33, matrix.M34,
		matrix.M41, matrix.M42, matrix.M43, matrix.M44);
	public static implicit operator Matrix4((
		double M11, double M12, double M13, double M14,
		double M21, double M22, double M23, double M24,
		double M31, double M32, double M33, double M34,
		double M41, double M42, double M43, double M44
		) matrix) => new(
		matrix.M11, matrix.M12, matrix.M13, matrix.M14,
		matrix.M21, matrix.M22, matrix.M23, matrix.M24,
		matrix.M31, matrix.M32, matrix.M33, matrix.M34,
		matrix.M41, matrix.M42, matrix.M43, matrix.M44);
	
	public Vector3 TransformPoint(Vector3 point) => new(
		M11 * point.X + M12 * point.Y + M13 * point.Z + M14,
		M21 * point.X + M22 * point.Y + M23 * point.Z + M24,
		M31 * point.X + M32 * point.Y + M33 * point.Z + M34);
	public Vector3 InverseTransformPoint(Vector3 point) => Inverted.TransformPoint(point);
	
	public Vector2 TransformPoint(Vector2 point) => new(
		M11 * point.X + M12 * point.Y + M14,
		M21 * point.X + M22 * point.Y + M24);
	public Vector2 InverseTransformPoint(Vector2 point) => Inverted.TransformPoint(point);
	
	public Vector3 TransformDirection(Vector3 direction) => new(
		M11 * direction.X + M12 * direction.Y + M13 * direction.Z,
		M21 * direction.X + M22 * direction.Y + M23 * direction.Z,
		M31 * direction.X + M32 * direction.Y + M33 * direction.Z);
	public Vector3 InverseTransformDirection(Vector3 direction) => Inverted.TransformDirection(direction);
	
	public Vector2 TransformDirection(Vector2 direction) => new(
		M11 * direction.X + M12 * direction.Y,
		M21 * direction.X + M22 * direction.Y);
	public Vector2 InverseTransformDirection(Vector2 direction) => Inverted.TransformDirection(direction);
}
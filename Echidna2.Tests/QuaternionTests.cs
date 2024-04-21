using Echidna2.Mathematics;

namespace Echidna2.MathematicsTests;

public static class QuaternionTests
{
	public static IEnumerable<(Vector3 Vector, Quaternion Quaternion)> Data =>
		new[]
		{
			(new Vector3(0.0, 0.0, 0.0), new Quaternion(0.0, 0.0, 0.0, 1.0)),
			(new Vector3(Math.PI / 2, 0.0, 0.0), new Quaternion(0.7071, 0.0, 0.0, 0.7071)),
			(new Vector3(0.0, Math.PI / 2, 0.0), new Quaternion(0.0, 0.7071, 0.0, 0.7071)),
			(new Vector3(0.0, 0.0, Math.PI / 2), new Quaternion(0.0, 0.0, 0.7071, 0.7071)),
		};
	
	public static IEnumerable<object[]> FromDoublesData => Data
		.Select(data => new object[] { data.Vector.X, data.Vector.Y, data.Vector.Z, data.Quaternion });
	[Theory, MemberData(nameof(FromDoublesData))]
	public static void FromEulerAngles_Doubles(double pitch, double roll, double yaw, Quaternion expected)
	{
		// Arrange
		
		// Act
		Quaternion actual = Quaternion.FromEulerAngles(pitch, roll, yaw);
		
		// Assert
		Assert.Equal(expected.X, actual.X, 4);
		Assert.Equal(expected.Y, actual.Y, 4);
		Assert.Equal(expected.Z, actual.Z, 4);
		Assert.Equal(expected.W, actual.W, 4);
	}
	
	public static IEnumerable<object[]> FromVectorData => Data
		.Select(data => new object[] { data.Vector, data.Quaternion });
	[Theory, MemberData(nameof(FromVectorData))]
	public static void FromEulerAngles_Vector(Vector3 angles, Quaternion expected)
	{
		// Arrange
		
		// Act
		Quaternion actual = Quaternion.FromEulerAngles(angles);
		
		// Assert
		Assert.Equal(expected.X, actual.X, 4);
		Assert.Equal(expected.Y, actual.Y, 4);
		Assert.Equal(expected.Z, actual.Z, 4);
		Assert.Equal(expected.W, actual.W, 4);
	}
	
	public static IEnumerable<object[]> ToVectorData => Data
		.Select(data => new object[] { data.Quaternion, data.Vector });
	[Theory, MemberData(nameof(ToVectorData))]
	public static void ToEulerAngles_Vector(Quaternion rotation, Vector3 expected)
	{
		// Arrange
		
		// Act
		Vector3 actual = rotation.ToEulerAngles();
		
		// Assert
		Assert.Equal(expected.X, actual.X, 1);
		Assert.Equal(expected.Y, actual.Y, 1);
		Assert.Equal(expected.Z, actual.Z, 1);
	}
}
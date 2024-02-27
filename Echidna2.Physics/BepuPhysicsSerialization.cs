using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using Tomlyn.Model;

namespace Echidna2.Physics;

public static class BepuPhysicsSerialization
{
	public static BodyInertia DeserializeBodyInertia(TomlTable table)
	{
		TomlTable inertiaTensorTable = (TomlTable)table["InverseInertiaTensor"];
		BodyInertia bodyInertia = new()
		{
			InverseMass = (float)(double)table["InverseMass"],
			InverseInertiaTensor = new Symmetric3x3
			{
				XX = (float)(double)inertiaTensorTable["XX"],
				YX = (float)(double)inertiaTensorTable["YX"],
				YY = (float)(double)inertiaTensorTable["YY"],
				ZX = (float)(double)inertiaTensorTable["ZX"],
				ZY = (float)(double)inertiaTensorTable["ZY"],
				ZZ = (float)(double)inertiaTensorTable["ZZ"],
			}
		};
		table.Remove("InverseInertiaTensor");
		table.Remove("InverseMass");
		return bodyInertia;
	}
	
	public static BodyShape DeserializeBodyShape(TomlTable table)
	{
		string type = (string)table["Type"];
		table.Remove("Type");
		switch (type)
		{
			case "Sphere":
				BodyShape sphereShape = BodyShape.Of(new Sphere((float)(double)table["Radius"]));
				table.Remove("Radius");
				return sphereShape;
			case "Box":
				BodyShape boxShape = BodyShape.Of(new Box((float)(double)table["Width"], (float)(double)table["Length"], (float)(double)table["Height"]));
				table.Remove("Width");
				table.Remove("Length");
				table.Remove("Height");
				return boxShape;
			default:
				throw new InvalidOperationException($"Shape type {type} does not exist");
		}
	}
}
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using Echidna2.Serialization;
using Tomlyn.Model;

namespace Echidna2.Physics;

public class BodyShapeSerializer : Serializer<TomlTable, BodyShape>
{
	public TomlTable Serialize(BodyShape value)
	{
		TomlTable table = new();
		switch (value)
		{
			case BodyShape<Sphere> sphere:
				table.Add("Type", "Sphere");
				table.Add("Radius", sphere.Shape.Radius);
				break;
			case BodyShape<Box> box:
				table.Add("Type", "Box");
				table.Add("Width", box.Shape.Width);
				table.Add("Length", box.Shape.Length);
				table.Add("Height", box.Shape.Height);
				break;
			default:
				throw new InvalidOperationException($"Shape type {value.GetType().GenericTypeArguments[0]} does not exist");
		}
		return table;
	}
	
	public BodyShape Deserialize(BodyShape? value, TomlTable data)
	{
		string type = (string)data["Type"];
		switch (type)
		{
			case "Sphere":
				BodyShape sphereShape = BodyShape.Of(new Sphere((float)(double)data["Radius"]));
				return sphereShape;
			case "Box":
				BodyShape boxShape = BodyShape.Of(new Box((float)(double)data["Width"], (float)(double)data["Length"], (float)(double)data["Height"]));
				return boxShape;
			default:
				throw new InvalidOperationException($"Shape type {type} does not exist");
		}
	}
}

public class BodyInertiaSerializer : Serializer<TomlTable, BodyInertia>
{
	public TomlTable Serialize(BodyInertia value)
	{
		TomlTable table = new();
		TomlTable inertiaTensorTable = new();
		inertiaTensorTable.Add("XX", value.InverseInertiaTensor.XX);
		inertiaTensorTable.Add("YX", value.InverseInertiaTensor.YX);
		inertiaTensorTable.Add("YY", value.InverseInertiaTensor.YY);
		inertiaTensorTable.Add("ZX", value.InverseInertiaTensor.ZX);
		inertiaTensorTable.Add("ZY", value.InverseInertiaTensor.ZY);
		inertiaTensorTable.Add("ZZ", value.InverseInertiaTensor.ZZ);
		table.Add("InverseInertiaTensor", inertiaTensorTable);
		table.Add("InverseMass", value.InverseMass);
		return table;
	}
	
	public BodyInertia Deserialize(BodyInertia value, TomlTable data)
	{
		TomlTable inertiaTensorTable = (TomlTable)data["InverseInertiaTensor"];
		BodyInertia bodyInertia = new()
		{
			InverseMass = (float)(double)data["InverseMass"],
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
		return bodyInertia;
	}
}
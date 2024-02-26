using BepuPhysics.Collidables;

namespace Echidna2.Physics;

public abstract class BodyShape
{
	public abstract TypedIndex AddToShapes(Shapes shapes);
	public static BodyShape Of<TShape>(TShape shape) where TShape : unmanaged, IShape => new BodyShape<TShape>(shape);
}

public class BodyShape<TShape>(TShape shape) : BodyShape
	where TShape : unmanaged, IShape
{
	public TShape Shape => shape;
	public override TypedIndex AddToShapes(Shapes shapes) => shapes.Add(shape);
}
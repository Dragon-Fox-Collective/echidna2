namespace Echidna2.Core;

public interface IEntity
{
	public void Update(double deltaTime);
	public void Draw();
}

public class Entity : IEntity
{
	public virtual void Update(double deltaTime) { }
	public virtual void Draw() { }
}
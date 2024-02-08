namespace Echidna2.Core;

public interface IHierarchy
{
	public void AddChild(Entity entity);
	public IEnumerable<Entity> GetChildren();
}

public class Hierarchy : Entity, IHierarchy
{
	private List<Entity> entities = [];
	
	public override void Update(double deltaTime)
	{
		foreach (Entity entity in entities)
			entity.Update(deltaTime);
	}
	
	public override void Draw()
	{
		foreach (Entity entity in entities)
			entity.Draw();
	}
	
	public void AddChild(Entity entity) => entities.Add(entity);
	
	public IEnumerable<Entity> GetChildren() => entities;
}
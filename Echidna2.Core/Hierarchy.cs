namespace Echidna2.Core;

[ComponentImplementation<Hierarchy>]
public interface IHierarchy : IEntity
{
	public void AddChild(IEntity entity);
	public IEnumerable<IEntity> GetChildren();
}

public class Hierarchy : Entity, IHierarchy
{
	private List<IEntity> entities = [];
	
	public override void Update(double deltaTime)
	{
		foreach (IEntity entity in entities)
			entity.Update(deltaTime);
	}
	
	public override void Draw()
	{
		foreach (IEntity entity in entities)
			entity.Draw();
	}
	
	public void AddChild(IEntity entity) => entities.Add(entity);
	
	public IEnumerable<IEntity> GetChildren() => entities;
}
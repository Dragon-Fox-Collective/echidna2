namespace Echidna2.Core;

[ComponentImplementation<Hierarchy>]
public interface IHierarchy : IUpdate, IDraw
{
	public void AddChild(object child);
	public IEnumerable<object> GetChildren();
}

public class Hierarchy : IHierarchy
{
	private List<object> children = [];
	
	public void Update(double deltaTime)
	{
		foreach (IUpdate child in children.OfType<IUpdate>())
			child.Update(deltaTime);
	}
	
	public void Draw()
	{
		foreach (IDraw child in children.OfType<IDraw>())
			child.Draw();
	}
	
	public void AddChild(object child) => children.Add(child);
	
	public IEnumerable<object> GetChildren() => children;
}
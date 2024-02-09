namespace Echidna2.Core;

[ComponentImplementation<Hierarchy>]
public interface IHierarchy : IUpdate, IDraw
{
	public void AddChild(object child);
	public IEnumerable<object> GetChildren();
	public void PrintTree(int depth = 0);
}

public partial class Hierarchy : IHierarchy
{
	private List<object> children = [];
	
	private bool updatedThisFrame = false;
	
	public Hierarchy(
		[Component] INamed? named = null)
	{
		this.named = named ?? new Named(GetType().Name);
	}
	
	public void PreUpdate()
	{
		updatedThisFrame = false;
		foreach (IUpdate child in children.OfType<IUpdate>())
			child.PreUpdate();
	}
	
	public void Update(double deltaTime)
	{
		if (updatedThisFrame) return;
		updatedThisFrame = true;
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
	
	public void PrintTree(int depth = 0)
	{
		PrintLayer(depth, named.Name);
		foreach (object child in children)
		{
			if (child is IHierarchy childHierarchy)
				childHierarchy.PrintTree(depth + 1);
			else if (child is INamed childNamed)
				PrintLayer(depth + 1, childNamed.Name);
			else
				PrintLayer(depth + 1, child.GetType().Name);
		}
	}
	
	private static void PrintLayer(int depth, string name) => Console.WriteLine(new string(' ', depth * 2) + (depth > 0 ? "\u2514 " : "") + name);
}
namespace Echidna2.Core;

public interface IUpdate
{
	public void PreUpdate();
	public void Update(double deltaTime);
}
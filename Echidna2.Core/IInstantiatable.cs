namespace Echidna2.Core;

public interface IInstantiatable<out T>
{
	public bool IsSharedInstance { get; set; }
	
	public T Instantiate();
}
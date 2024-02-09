using Echidna2.Core;

namespace Echidna2;

public class DebugEntity : IUpdate
{
	public void PreUpdate() { }
	
	public void OnUpdate(double deltaTime)
	{
		Console.WriteLine($"Updating {deltaTime}");
		// Console.WriteLine(Environment.StackTrace);
	}
}
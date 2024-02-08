using Echidna2.Core;

namespace Echidna2;

public class DebugEntity : Entity
{
	public override void Update(double deltaTime)
	{
		Console.WriteLine($"Updating {deltaTime}");
	}
}
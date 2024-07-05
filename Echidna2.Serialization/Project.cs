using System.Reflection;
using Echidna2.Serialization.TomlFiles;

namespace Echidna2.Serialization;

public partial class Project
{
	public static Project? Singleton { get; set; }
	
	public Project()
	{
		Singleton ??= this;
	}
	
	public Dictionary<string, Prefab> Prefabs = new();
	public Dictionary<string, Interface> Interfaces = new();
	public Dictionary<string, Notification> Notifications = new();
	public Assembly Assembly = null!;
	
	public void AddPrefab(Prefab prefab) => Prefabs.Add($"{prefab.Namespace}.{prefab.ThisComponent.ClassName}", prefab);
	public void AddInterface(Interface @interface) => Interfaces.Add($"{@interface.Namespace}.{@interface.ClassName}", @interface);
	public void AddNotification(Notification notification) => Notifications.Add($"{notification.Namespace}.{notification.ClassName}", notification);
}
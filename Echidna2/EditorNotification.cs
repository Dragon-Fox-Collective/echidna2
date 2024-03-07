namespace Echidna2;

public class EditorNotification<T>(T notification) where T : notnull
{
	public T Notification => notification;
}
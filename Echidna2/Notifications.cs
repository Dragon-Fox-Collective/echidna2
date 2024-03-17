using Echidna2.Core;

namespace Echidna2;

public class EditorNotification<T>(T notification) where T : notnull
{
	public T Notification => notification;
}

public interface IEditorInitialize : INotificationListener<IEditorInitialize.Notification>
{
	public class Notification(Editor editor)
	{
		public Editor Editor { get; } = editor;
	}
	void INotificationListener<Notification>.OnNotify(Notification notification) => OnEditorInitialize(notification.Editor);
	public void OnEditorInitialize(Editor editor);
}
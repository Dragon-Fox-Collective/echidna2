using Echidna2.Core;
using Echidna2.Gui;
using Echidna2.Serialization;
using JetBrains.Annotations;

namespace Echidna2;

public interface IFieldEditor
{
	public void Load(object value);
	public static virtual IFieldEditor Instantiate() => throw new NotImplementedException();
}

public interface IFieldEditor<in T> : IFieldEditor
{
	public void Load(T value);
	void IFieldEditor.Load(object value) => Load((T)value);
}

[UsedImplicitly, SerializeExposedMembers, Prefab("Editors/DoubleFieldEditor.toml")]
public partial class DoubleFieldEditor : IFieldEditor<double>
{
	[SerializedReference, ExposeMembersInClass] public FullLayoutWithHierarchy Rect { get; set; } = null!;
	[SerializedReference] public IHasText Text { get; set; } = null!;
	
	private double value;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, Rect);
	}
	
	public void Load(double value)
	{
		this.value = value;
		Text.TextString = $"{value}";
	}
}
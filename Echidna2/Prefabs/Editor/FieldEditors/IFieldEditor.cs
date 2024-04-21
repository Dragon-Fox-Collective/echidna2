using Echidna2.Core;

namespace Echidna2.Prefabs.Editor.FieldEditors;

[DontExpose]
public interface IFieldEditor
{
	public void Load(object? value);
	public event Action<object?>? ValueChanged;
	public static virtual IFieldEditor Instantiate() => throw new NotImplementedException();
}

[DontExpose]
public interface IFieldEditor<in T> : IFieldEditor
{
	public void Load(T value);
	void IFieldEditor.Load(object? value) => Load((T)value!);
	// Trying to cast an Action<object> to an Action<T> doesn't work when T is a value type, so don't include ValueChanged
}
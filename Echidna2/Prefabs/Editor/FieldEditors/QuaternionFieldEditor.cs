using Echidna2.Core;
using Echidna2.Mathematics;
using Echidna2.Serialization;
using JetBrains.Annotations;

namespace Echidna2.Prefabs.Editor.FieldEditors;

[UsedImplicitly, Prefab("Prefabs/Editor/FieldEditors/QuaternionFieldEditor.toml")]
public partial class QuaternionFieldEditor : INotificationPropagator, IFieldEditor<Quaternion>
{
	private Vector3FieldEditor? vectorFieldEditor;
	[SerializedReference, ExposeMembersInClass] public Vector3FieldEditor VectorFieldEditor
	{
		get => vectorFieldEditor!;
		set
		{
			if (vectorFieldEditor is not null)
				vectorFieldEditor.ValueChanged -= UpdateVector;
			
			vectorFieldEditor = value;
			
			if (vectorFieldEditor is not null)
				vectorFieldEditor.ValueChanged += UpdateVector;
		}
	}
	
	private Quaternion value;
	public Quaternion Value
	{
		get => value;
		set
		{
			this.value = value;
			VectorFieldEditor.Load(value.ToEulerAngles());
		}
	}
	
	public event Action<object?>? ValueChanged;
	
	public void UpdateVector(object? vector) => UpdateVector((Vector3)vector!);
	public void UpdateVector(Vector3 vector)
	{
		value = Quaternion.FromEulerAngles(vector);
		ValueChanged?.Invoke(value);
	}
	
	public void Load(Quaternion value) => Value = value;
	
	public void Notify<T>(T notification) where T : notnull
	{
		INotificationPropagator.Notify(notification, VectorFieldEditor);
	}
}
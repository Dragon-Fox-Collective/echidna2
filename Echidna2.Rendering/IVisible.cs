namespace Echidna2.Rendering;

public interface IVisible
{
	public void ToggleVisibility();
	public bool IsVisible { get; set; }
	public void Show();
	public void Hide();
}
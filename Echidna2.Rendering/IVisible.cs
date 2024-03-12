namespace Echidna2.Rendering;

public interface IVisible
{
	public bool IsVisible { get; set; }
}

public static class IVisibleExtensions
{
	public static void ToggleVisibility(this IVisible visible) => visible.IsVisible = !visible.IsVisible;
	public static void Show(this IVisible visible) => visible.IsVisible = true;
	public static void Hide(this IVisible visible) => visible.IsVisible = false;
}
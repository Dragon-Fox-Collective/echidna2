using Echidna2.Rendering;
using Echidna2.TestPrefabs;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Echidna2.NotificationTests;

public class ButtonNotificationTests
{
	[Fact]
	public void ClickingInButton_ActivatesButton()
	{
		// Arrange
		ButtonWithTransform button = new();
		bool buttonClicked = false;
		button.Clicked += () => buttonClicked = true;
		
		// Act
		button.Notify(new IMouseDown.Notification(MouseButton.Left, (0, 0), (0, 0, 0)));
		button.Notify(new IMouseUp.Notification(MouseButton.Left, (0, 0), (0, 0, 0)));
		
		// Assert
		Assert.True(buttonClicked, "Button wasn't clicked");
	}
	
	[Fact]
	public void ClickingInButton_WhenButtonIsVisible_ActivatesButton()
	{
		// Arrange
		ButtonWithTransform button = new();
		bool buttonClicked = false;
		button.Clicked += () => buttonClicked = true;
		
		VisibilityLayerWithTransform visibilityLayer = new();
		visibilityLayer.IsSelfVisible = true;
		visibilityLayer.AddChild(button);
		
		// Act
		visibilityLayer.Notify(new IMouseDown.Notification(MouseButton.Left, (0, 0), (0, 0, 0)));
		visibilityLayer.Notify(new IMouseUp.Notification(MouseButton.Left, (0, 0), (0, 0, 0)));
		
		// Assert
		Assert.True(buttonClicked, "Button wasn't clicked");
	}
	
	[Fact]
	public void ClickingInButton_WhenButtonIsInvisible_DoesntActivateButton()
	{
		// Arrange
		ButtonWithTransform button = new();
		bool buttonClicked = false;
		button.Clicked += () => buttonClicked = true;
		
		VisibilityLayerWithTransform visibilityLayer = new();
		visibilityLayer.IsSelfVisible = false;
		visibilityLayer.AddChild(button);
		
		// Act
		visibilityLayer.Notify(new IMouseDown.Notification(MouseButton.Left, (0, 0), (0, 0, 0)));
		visibilityLayer.Notify(new IMouseUp.Notification(MouseButton.Left, (0, 0), (0, 0, 0)));
		
		// Assert
		Assert.False(buttonClicked, "Button was clicked");
	}
}
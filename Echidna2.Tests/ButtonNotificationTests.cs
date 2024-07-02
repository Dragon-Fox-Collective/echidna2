using Echidna2.Rendering;
using Echidna2.Prefabs.Test;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Echidna2.NotificationTests;

public static class ButtonNotificationTests
{
	[Fact]
	public static void ClickingInButton_ActivatesButton()
	{
		// Arrange
		ButtonWithTransform button = new();
		bool buttonClicked = false;
		button.Clicked += () => buttonClicked = true;
		
		// Act
		button.Notify(new MouseDown_Notification(MouseButton.Left, (0, 0), (0, 0, 0)));
		button.Notify(new MouseUp_Notification(MouseButton.Left, (0, 0), (0, 0, 0)));
		
		// Assert
		Assert.True(buttonClicked, "Button wasn't clicked");
	}
	
	[Fact]
	public static void ClickingInButton_WhenButtonIsVisible_ActivatesButton()
	{
		// Arrange
		ButtonWithTransform button = new();
		bool buttonClicked = false;
		button.Clicked += () => buttonClicked = true;
		
		VisibilityLayerWithTransform visibilityLayer = new();
		visibilityLayer.IsSelfVisible = true;
		visibilityLayer.AddChild(button);
		
		// Act
		visibilityLayer.Notify(new MouseDown_Notification(MouseButton.Left, (0, 0), (0, 0, 0)));
		visibilityLayer.Notify(new MouseUp_Notification(MouseButton.Left, (0, 0), (0, 0, 0)));
		
		// Assert
		Assert.True(buttonClicked, "Button wasn't clicked");
	}
	
	[Fact]
	public static void ClickingInButton_WhenButtonIsInvisible_DoesntActivateButton()
	{
		// Arrange
		ButtonWithTransform button = new();
		bool buttonClicked = false;
		button.Clicked += () => buttonClicked = true;
		
		VisibilityLayerWithTransform visibilityLayer = new();
		visibilityLayer.IsSelfVisible = false;
		visibilityLayer.AddChild(button);
		
		// Act
		visibilityLayer.Notify(new MouseDown_Notification(MouseButton.Left, (0, 0), (0, 0, 0)));
		visibilityLayer.Notify(new MouseUp_Notification(MouseButton.Left, (0, 0), (0, 0, 0)));
		
		// Assert
		Assert.False(buttonClicked, "Button was clicked");
	}
}
using Godot;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		var startButton = GetNode<Button>("StartButton");
		startButton.Pressed += OnStartButtonPressed;
		
		var bg = GetNode<ColorRect>("Background");
		bg.Color = new Color(0.29f, 0.0f, 0.51f, 1f);
		
		var title = GetNode<Label>("Title");
		title.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f, 1f));
		
		var subtitle = GetNode<Label>("Subtitle");
		subtitle.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f, 1f));
	}

	private void OnStartButtonPressed()
	{
		GetTree().ChangeSceneToFile("res://Basketball.tscn");
	}
}

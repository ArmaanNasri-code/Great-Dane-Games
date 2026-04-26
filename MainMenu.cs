using Godot;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		var startButton = GetNode<Button>("StartButton");
		startButton.Pressed += OnStartButtonPressed;

		var bg = GetNode<ColorRect>("Background");
		bg.Color = new Color(0.08f, 0f, 0.15f, 1f);

		var title = GetNode<Label>("Title");
		title.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f, 1f));

		var subtitle = GetNode<Label>("Subtitle");
		subtitle.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f, 0.85f));

		// Style the button
		var normalStyle = new StyleBoxFlat();
		normalStyle.BgColor = new Color(0f, 0f, 0f, 0.75f);
		normalStyle.BorderColor = new Color(1f, 0.84f, 0f, 1f);
		normalStyle.SetBorderWidthAll(3);
		normalStyle.SetCornerRadiusAll(4);
		startButton.AddThemeStyleboxOverride("normal", normalStyle);

		var hoverStyle = new StyleBoxFlat();
		hoverStyle.BgColor = new Color(1f, 0.84f, 0f, 0.2f);
		hoverStyle.BorderColor = new Color(1f, 0.84f, 0f, 1f);
		hoverStyle.SetBorderWidthAll(3);
		hoverStyle.SetCornerRadiusAll(4);
		startButton.AddThemeStyleboxOverride("hover", hoverStyle);

		var pressedStyle = new StyleBoxFlat();
		pressedStyle.BgColor = new Color(1f, 0.84f, 0f, 0.4f);
		pressedStyle.BorderColor = new Color(1f, 0.84f, 0f, 1f);
		pressedStyle.SetBorderWidthAll(3);
		pressedStyle.SetCornerRadiusAll(4);
		startButton.AddThemeStyleboxOverride("pressed", pressedStyle);

		startButton.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f, 1f));
		startButton.AddThemeColorOverride("font_hover_color", new Color(1f, 1f, 1f, 1f));

		// Player label nodes
		var p1Label = GetNode<Label>("Player1Label");
		p1Label?.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f, 0.75f));
		var p2Label = GetNode<Label>("Player2Label");
		p2Label?.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f, 0.75f));
	}

	private void OnStartButtonPressed()
	{
		GetTree().ChangeSceneToFile("res://Basketball.tscn");
	}
}

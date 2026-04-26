using Godot;

public partial class GameOver : Node2D
{
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("p1_action") || Input.IsActionJustPressed("p2_action"))
        {
            GameManager.Instance.Player1Rounds = 0;
            GameManager.Instance.Player2Rounds = 0;
            GameManager.Instance.CurrentRound = 1;
            GetTree().ChangeSceneToFile("res://MainMenu.tscn");
        }
        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(0, 0, 1280, 720), new Color(0.29f, 0f, 0.51f, 1f));
        int p1 = GameManager.Instance?.Player1Rounds ?? 0;
        int p2 = GameManager.Instance?.Player2Rounds ?? 0;
        int winner = p1 > p2 ? 1 : 2;
        DrawString(ThemeDB.FallbackFont, new Vector2(300, 200), $"GREAT DANE {winner} WINS!", HorizontalAlignment.Left, -1, 64, new Color(1f, 0.84f, 0f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(400, 350), $"Final: Dane 1 [{p1}] - [{p2}] Dane 2", HorizontalAlignment.Left, -1, 36, new Color(1f, 1f, 1f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(350, 500), "Press F or J to play again!", HorizontalAlignment.Left, -1, 32, new Color(1f, 0.84f, 0f, 1f));
    }
}

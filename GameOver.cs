using Godot;

public partial class GameOver : Node2D
{
    private float timer = 0f;

    public override void _Process(double delta)
    {
        timer += (float)delta;
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
        int p1 = GameManager.Instance?.Player1Rounds ?? 0;
        int p2 = GameManager.Instance?.Player2Rounds ?? 0;
        int winner = p1 > p2 ? 1 : 2;

        // Background
        DrawRect(new Rect2(0, 0, 1280, 720), new Color(0.08f, 0f, 0.15f, 1f));
        // Gold bars
        DrawRect(new Rect2(0, 0, 1280, 8), new Color(1f, 0.84f, 0f, 1f));
        DrawRect(new Rect2(0, 712, 1280, 8), new Color(1f, 0.84f, 0f, 1f));

        // Header label
        DrawRect(new Rect2(440, 60, 400, 50), new Color(0f, 0f, 0f, 0.6f));
        DrawRect(new Rect2(440, 60, 400, 5), new Color(1f, 0.84f, 0f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(490, 98), "FINAL RESULTS", HorizontalAlignment.Left, -1, 26, new Color(1f, 0.84f, 0f, 1f));

        // Winner banner
        DrawString(ThemeDB.FallbackFont, new Vector2(280, 210),
            $"GREAT DANE {winner} WINS!", HorizontalAlignment.Left, -1, 72, new Color(1f, 0.84f, 0f, 1f));
        DrawLine(new Vector2(200, 230), new Vector2(1080, 230), new Color(1f, 0.84f, 0f, 0.3f), 2f);

        // Score boxes
        DrawRect(new Rect2(240, 270, 300, 130), new Color(0.29f, 0f, 0.51f, 0.85f));
        DrawRect(new Rect2(240, 270, 300, 5), new Color(0.29f, 0f, 0.51f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(310, 312), "DANE 1", HorizontalAlignment.Left, -1, 24, new Color(1f,1f,1f,0.85f));
        DrawString(ThemeDB.FallbackFont, new Vector2(345, 378), $"{p1}", HorizontalAlignment.Left, -1, 64, new Color(1f, 0.84f, 0f, 1f));

        DrawString(ThemeDB.FallbackFont, new Vector2(605, 352), "VS", HorizontalAlignment.Left, -1, 38, new Color(1f,1f,1f,0.4f));

        DrawRect(new Rect2(740, 270, 300, 130), new Color(0.29f, 0f, 0.51f, 0.85f));
        DrawRect(new Rect2(740, 270, 300, 5), new Color(1f, 0.84f, 0f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(810, 312), "DANE 2", HorizontalAlignment.Left, -1, 24, new Color(1f,1f,1f,0.85f));
        DrawString(ThemeDB.FallbackFont, new Vector2(845, 378), $"{p2}", HorizontalAlignment.Left, -1, 64, new Color(1f, 0.84f, 0f, 1f));

        // Play again box
        DrawRect(new Rect2(340, 460, 600, 60), new Color(1f, 0.84f, 0f, 0.12f));
        DrawRect(new Rect2(340, 460, 600, 3), new Color(1f, 0.84f, 0f, 0.5f));
        DrawString(ThemeDB.FallbackFont, new Vector2(370, 500), "Thanks for playing Great Dane Games!", HorizontalAlignment.Left, -1, 22, new Color(1f,1f,1f,0.85f));

        // Pulsing prompt
        if ((int)(timer * 2) % 2 == 0)
            DrawString(ThemeDB.FallbackFont, new Vector2(430, 600), "Press F or J to play again!",
                HorizontalAlignment.Left, -1, 26, new Color(1f, 0.84f, 0f, 0.95f));
    }
}

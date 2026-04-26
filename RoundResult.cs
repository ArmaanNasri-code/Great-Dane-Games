using Godot;

public partial class RoundResult : Node2D
{
    private float timer = 0f;
    private bool ready = false;

    public override void _Ready()
    {
        if (GameManager.Instance != null)
        {
            int winner = GameManager.Instance.RoundWinner;
            if (winner == 1) GameManager.Instance.Player1Rounds++;
            else GameManager.Instance.Player2Rounds++;
            GameManager.Instance.CurrentRound++;
        }
        ready = true;
    }

    public override void _Process(double delta)
    {
        if (!ready) return;
        timer += (float)delta;
        QueueRedraw();

        if (timer > 0.5f && (Input.IsActionJustPressed("p1_action") || Input.IsActionJustPressed("p2_action")))
        {
            int p1 = GameManager.Instance.Player1Rounds;
            int p2 = GameManager.Instance.Player2Rounds;
            int round = GameManager.Instance.CurrentRound;

            // Someone has 2 wins - game over
            if (p1 >= 2 || p2 >= 2)
                GetTree().ChangeSceneToFile("res://GameOver.tscn");
            // Just finished round 1 - go to racing
            else if (round == 2)
                GetTree().ChangeSceneToFile("res://Racing.tscn");
            // Just finished round 2 - go to sumo tiebreaker
            else if (round == 3)
                GetTree().ChangeSceneToFile("res://Sumo.tscn");
            // Fallback
            else
                GetTree().ChangeSceneToFile("res://GameOver.tscn");
        }
    }

    public override void _Draw()
    {
        // Background with gradient feel
        DrawRect(new Rect2(0, 0, 1280, 720), new Color(0.12f, 0f, 0.25f, 1f));

        // Top gold bar
        DrawRect(new Rect2(0, 0, 1280, 8), new Color(1f, 0.84f, 0f, 1f));
        // Bottom gold bar
        DrawRect(new Rect2(0, 712, 1280, 8), new Color(1f, 0.84f, 0f, 1f));

        int winner = GameManager.Instance?.RoundWinner ?? 1;
        int p1 = GameManager.Instance?.Player1Rounds ?? 0;
        int p2 = GameManager.Instance?.Player2Rounds ?? 0;
        int round = GameManager.Instance?.CurrentRound ?? 2;
        int prevRound = round - 1;

        // Round label
        DrawString(ThemeDB.FallbackFont, new Vector2(540, 70),
            $"ROUND {prevRound} OVER",
            HorizontalAlignment.Left, -1, 28, new Color(1f, 0.84f, 0f, 0.8f));

        // Winner announcement - centered big
        string winnerText = $"DANE {winner} WINS!";
        DrawString(ThemeDB.FallbackFont, new Vector2(380, 200),
            winnerText, HorizontalAlignment.Left, -1, 72, new Color(1f, 0.84f, 0f, 1f));

        // Decorative line
        DrawLine(new Vector2(200, 230), new Vector2(1080, 230), new Color(1f, 0.84f, 0f, 0.3f), 2f);

        // Score boxes
        // Player 1 box
        DrawRect(new Rect2(240, 270, 300, 120), new Color(0.29f, 0f, 0.51f, 0.8f));
        DrawRect(new Rect2(240, 270, 300, 120), new Color(1f, 0.84f, 0f, 0.3f));
        DrawString(ThemeDB.FallbackFont, new Vector2(310, 315), "DANE 1",
            HorizontalAlignment.Left, -1, 24, new Color(1f,1f,1f,0.8f));
        DrawString(ThemeDB.FallbackFont, new Vector2(345, 370), $"{p1}",
            HorizontalAlignment.Left, -1, 52, new Color(1f, 0.84f, 0f, 1f));

        // VS
        DrawString(ThemeDB.FallbackFont, new Vector2(610, 350), "VS",
            HorizontalAlignment.Left, -1, 36, new Color(1f,1f,1f,0.5f));

        // Player 2 box
        DrawRect(new Rect2(740, 270, 300, 120), new Color(0.29f, 0f, 0.51f, 0.8f));
        DrawRect(new Rect2(740, 270, 300, 120), new Color(1f, 0.84f, 0f, 0.3f));
        DrawString(ThemeDB.FallbackFont, new Vector2(810, 315), "DANE 2",
            HorizontalAlignment.Left, -1, 24, new Color(1f,1f,1f,0.8f));
        DrawString(ThemeDB.FallbackFont, new Vector2(845, 370), $"{p2}",
            HorizontalAlignment.Left, -1, 52, new Color(1f, 0.84f, 0f, 1f));

        // Next round text
        string nextText;
        if (p1 >= 2 || p2 >= 2)
            nextText = "Press F or J to see Final Results!";
        else if (round == 2)
            nextText = "▶  Round 2: Campus Race";
        else
            nextText = "▶  Round 3: Professor Showdown";

        DrawRect(new Rect2(340, 460, 600, 60), new Color(1f, 0.84f, 0f, 0.15f));
        DrawString(ThemeDB.FallbackFont, new Vector2(370, 502),
            nextText, HorizontalAlignment.Left, -1, 26, new Color(1f, 1f, 1f, 0.95f));

        // Pulsing press prompt
        if ((int)(timer * 2) % 2 == 0)
            DrawString(ThemeDB.FallbackFont, new Vector2(460, 610),
                "Press F or J to continue",
                HorizontalAlignment.Left, -1, 24, new Color(1f, 0.84f, 0f, 0.9f));
    }
}

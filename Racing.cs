using Godot;

public partial class Racing : Node2D
{
    private float player1X = 320f;
    private float player2X = 960f;
    private float player1Y = 600f;
    private float player2Y = 600f;
    private float player1Speed = 0f;
    private float player2Speed = 0f;
    private float maxSpeed = 400f;
    private float acceleration = 200f;
    private float friction = 150f;

    private float finishLineY = 50f;
    private bool gameOver = false;
    private bool player1Finished = false;
    private bool player2Finished = false;

    // Obstacles
    private Vector2[] obstacles = new Vector2[]
    {
        new Vector2(320, 400), new Vector2(960, 350),
        new Vector2(320, 250), new Vector2(960, 200),
        new Vector2(320, 150), new Vector2(960, 100)
    };

    public override void _Process(double delta)
    {
        if (gameOver) return;

        // Player 1 - F to accelerate
        if (Input.IsActionJustPressed("p1_action"))
            player1Speed = Mathf.Min(player1Speed + acceleration, maxSpeed);

        // Player 2 - J to accelerate
        if (Input.IsActionJustPressed("p2_action"))
            player2Speed = Mathf.Min(player2Speed + acceleration, maxSpeed);

        // Apply friction
        player1Speed = Mathf.Max(player1Speed - friction * (float)delta, 0f);
        player2Speed = Mathf.Max(player2Speed - friction * (float)delta, 0f);

        // Move players up
        player1Y -= player1Speed * (float)delta;
        player2Y -= player2Speed * (float)delta;

        // Scroll obstacles
        for (int i = 0; i < obstacles.Length; i++)
        {
            obstacles[i].Y += 80f * (float)delta;
            if (obstacles[i].Y > 750f)
                obstacles[i] = new Vector2(obstacles[i].X, -50f);
        }

        // Check obstacle collision
        foreach (var obs in obstacles)
        {
            if (Mathf.Abs(player1X - obs.X) < 40f && Mathf.Abs(player1Y - obs.Y) < 40f)
                player1Speed = 0f;
            if (Mathf.Abs(player2X - obs.X) < 40f && Mathf.Abs(player2Y - obs.Y) < 40f)
                player2Speed = 0f;
        }

        // Check finish
        if (player1Y <= finishLineY && !player1Finished)
        {
            player1Finished = true;
            EndRace(1);
        }
        if (player2Y <= finishLineY && !player2Finished)
        {
            player2Finished = true;
            EndRace(2);
        }

        QueueRedraw();
    }

    private void EndRace(int winner)
    {
        gameOver = true;
        GameManager.Instance.RoundWinner = winner;
        GetTree().ChangeSceneToFile("res://RoundResult.tscn");
    }

    public override void _Draw()
    {
        // Sky background
        DrawRect(new Rect2(0, 0, 1280, 720), new Color(0.5f, 0.8f, 1f, 1f));

        // Road
        DrawRect(new Rect2(200, 0, 250, 720), new Color(0.4f, 0.4f, 0.4f, 1f));
        DrawRect(new Rect2(830, 0, 250, 720), new Color(0.4f, 0.4f, 0.4f, 1f));

        // Finish line
        DrawRect(new Rect2(200, finishLineY, 250, 15), new Color(1f, 1f, 1f, 1f));
        DrawRect(new Rect2(830, finishLineY, 250, 15), new Color(1f, 1f, 1f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(270, 45), "FINISH", HorizontalAlignment.Left, -1, 20, new Color(1f, 0f, 0f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(900, 45), "FINISH", HorizontalAlignment.Left, -1, 20, new Color(1f, 0f, 0f, 1f));

        // Obstacles (public safety vehicles - gold rectangles)
        foreach (var obs in obstacles)
        {
            DrawRect(new Rect2(obs.X - 30, obs.Y - 25, 60, 50), new Color(1f, 0.84f, 0f, 1f));
            DrawString(ThemeDB.FallbackFont, new Vector2(obs.X - 20, obs.Y + 5), "UPD", HorizontalAlignment.Left, -1, 14, new Color(0.29f, 0f, 0.51f, 1f));
        }

        // Players
        DrawCircle(new Vector2(player1X, player1Y), 22f, new Color(0.29f, 0f, 0.51f, 1f));
        DrawCircle(new Vector2(player2X, player2Y), 22f, new Color(0.29f, 0f, 0.51f, 1f));

        // Labels
        DrawString(ThemeDB.FallbackFont, new Vector2(100, 50), "Dane 1", HorizontalAlignment.Left, -1, 28, new Color(0.29f, 0f, 0.51f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(1100, 50), "Dane 2", HorizontalAlignment.Left, -1, 28, new Color(0.29f, 0f, 0.51f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(400, 680), "Tap F to accelerate!", HorizontalAlignment.Left, -1, 24, new Color(0.29f, 0f, 0.51f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(750, 680), "Tap J to accelerate!", HorizontalAlignment.Left, -1, 24, new Color(0.29f, 0f, 0.51f, 1f));

        // Round indicator
        DrawString(ThemeDB.FallbackFont, new Vector2(490, 30), "ROUND 2: CAMPUS RACE", HorizontalAlignment.Left, -1, 26, new Color(1f, 0.84f, 0f, 1f));
    }
}

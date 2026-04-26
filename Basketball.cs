using Godot;

public partial class Basketball : Node2D
{
    private float hoop1X = 320f;
    private float hoop2X = 960f;
    private float hoopY = 130f;
    private float hoopSpeed = 100f;
    private float hoop1Dir = 1f;
    private float hoop2Dir = -1f;
    private float hoopMinX1 = 80f;
    private float hoopMaxX1 = 560f;
    private float hoopMinX2 = 720f;
    private float hoopMaxX2 = 1200f;
    private float rimRadius = 38f;

    private float player1X = 320f;
    private float player1Y = 490f;
    private float player2X = 960f;
    private float player2Y = 490f;

    private float ball1X, ball1Y, ball1VelX, ball1VelY;
    private float ball2X, ball2Y, ball2VelX, ball2VelY;
    private bool ball1Active = false;
    private bool ball2Active = false;
    private bool ball1WasAboveHoop = false;
    private bool ball2WasAboveHoop = false;

    private float gravity = 700f;
    private float ballRadius = 22f;

    private int score1 = 0;
    private int score2 = 0;
    private float timeLeft = 45f;
    private bool gameOver = false;
    private int overtimeCount = 0;
    private bool inOvertime = false;

    private float flash1Timer = 0f;
    private float flash2Timer = 0f;

    private float p1HoldTime = 0f;
    private float p2HoldTime = 0f;
    private bool p1Holding = false;
    private bool p2Holding = false;

    private Texture2D player1Texture;
    private Texture2D courtTexture;
    private Texture2D player2Texture;
    private Texture2D ballTexture;

    public override void _Ready()
    {
        ball1X = player1X; ball1Y = player1Y - 40f;
        ball2X = player2X; ball2Y = player2Y - 40f;
        player1Texture = GD.Load<Texture2D>("res://player1.png");
        courtTexture = GD.Load<Texture2D>("res://basketball_court.png");
        player2Texture = GD.Load<Texture2D>("res://player2.png");
        ballTexture = GD.Load<Texture2D>("res://basketball.png");
    }

    private void CalcVelocity(float bx, float by, float targetX, float holdTime, out float vx, out float vy)
    {
        float peakY = hoopY - 150f;
        float dx = targetX - bx;
        float tPeak = 0.5f + holdTime * 0.2f;
        vx = dx / (tPeak * 2f);
        vy = (peakY - by) / tPeak - 0.5f * gravity * tPeak;
    }

    public override void _Process(double delta)
    {
        if (gameOver) return;
        float dt = (float)delta;

        timeLeft -= dt;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            gameOver = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RoundWinner = score1 >= score2 ? 1 : 2;
            }
            GetTree().ChangeSceneToFile("res://RoundResult.tscn");
            return;
        }

        hoop1X += hoopSpeed * hoop1Dir * dt;
        if (hoop1X > hoopMaxX1) hoop1Dir = -1f;
        if (hoop1X < hoopMinX1) hoop1Dir = 1f;
        hoop2X += hoopSpeed * hoop2Dir * dt;
        if (hoop2X > hoopMaxX2) hoop2Dir = -1f;
        if (hoop2X < hoopMinX2) hoop2Dir = 1f;

        if (flash1Timer > 0) flash1Timer -= dt;
        if (flash2Timer > 0) flash2Timer -= dt;

        if (Input.IsActionPressed("p1_action") && !ball1Active)
        {
            p1Holding = true;
            p1HoldTime = Mathf.Min(p1HoldTime + dt, 1.0f);
        }
        if (Input.IsActionJustReleased("p1_action") && p1Holding && !ball1Active)
        {
            ball1X = player1X;
            ball1Y = player1Y - 50f;
            CalcVelocity(ball1X, ball1Y, hoop1X, p1HoldTime, out ball1VelX, out ball1VelY);
            ball1Active = true;
            ball1WasAboveHoop = false;
            p1Holding = false;
            p1HoldTime = 0f;
        }

        if (Input.IsActionPressed("p2_action") && !ball2Active)
        {
            p2Holding = true;
            p2HoldTime = Mathf.Min(p2HoldTime + dt, 1.0f);
        }
        if (Input.IsActionJustReleased("p2_action") && p2Holding && !ball2Active)
        {
            ball2X = player2X;
            ball2Y = player2Y - 50f;
            CalcVelocity(ball2X, ball2Y, hoop2X, p2HoldTime, out ball2VelX, out ball2VelY);
            ball2Active = true;
            ball2WasAboveHoop = false;
            p2Holding = false;
            p2HoldTime = 0f;
        }

        // Ball 1 physics
        if (ball1Active)
        {
            ball1VelY += gravity * dt;
            ball1X += ball1VelX * dt;
            ball1Y += ball1VelY * dt;

            if (ball1X < 8f + ballRadius) { ball1X = 8f + ballRadius; ball1VelX = Mathf.Abs(ball1VelX) * 0.7f; }
            if (ball1X > 632f - ballRadius) { ball1X = 632f - ballRadius; ball1VelX = -Mathf.Abs(ball1VelX) * 0.7f; }
            if (ball1Y < 8f + ballRadius) { ball1Y = 8f + ballRadius; ball1VelY = Mathf.Abs(ball1VelY) * 0.7f; }

            // Mark when ball goes above hoop
            if (ball1Y < hoopY) ball1WasAboveHoop = true;

            // Score: must have been above hoop, now falling through it
            if (ball1WasAboveHoop && ball1VelY > 0f &&
                ball1Y >= hoopY && ball1Y <= hoopY + 60f &&
                Mathf.Abs(ball1X - hoop1X) < rimRadius - 5f)
            {
                score1++;
                flash1Timer = 0.8f;
                ball1Active = false;
                ball1WasAboveHoop = false;
                ball1X = player1X;
                ball1Y = player1Y - 40f;
            }
            else if (ball1Y > 700f)
            {
                ball1Active = false;
                ball1WasAboveHoop = false;
                ball1X = player1X;
                ball1Y = player1Y - 40f;
            }
        }

        // Ball 2 physics
        if (ball2Active)
        {
            ball2VelY += gravity * dt;
            ball2X += ball2VelX * dt;
            ball2Y += ball2VelY * dt;

            if (ball2X < 648f + ballRadius) { ball2X = 648f + ballRadius; ball2VelX = Mathf.Abs(ball2VelX) * 0.7f; }
            if (ball2X > 1272f - ballRadius) { ball2X = 1272f - ballRadius; ball2VelX = -Mathf.Abs(ball2VelX) * 0.7f; }
            if (ball2Y < 8f + ballRadius) { ball2Y = 8f + ballRadius; ball2VelY = Mathf.Abs(ball2VelY) * 0.7f; }

            if (ball2Y < hoopY) ball2WasAboveHoop = true;

            if (ball2WasAboveHoop && ball2VelY > 0f &&
                ball2Y >= hoopY && ball2Y <= hoopY + 60f &&
                Mathf.Abs(ball2X - hoop2X) < rimRadius - 5f)
            {
                score2++;
                flash2Timer = 0.8f;
                ball2Active = false;
                ball2WasAboveHoop = false;
                ball2X = player2X;
                ball2Y = player2Y - 40f;
            }
            else if (ball2Y > 700f)
            {
                ball2Active = false;
                ball2WasAboveHoop = false;
                ball2X = player2X;
                ball2Y = player2Y - 40f;
            }
        }

        QueueRedraw();
    }

    private void DrawTrajectory(float startX, float startY, float vx, float vy)
    {
        float simX = startX, simY = startY, simVY = vy;
        float step = 0.04f;
        for (int i = 0; i < 22; i++)
        {
            simVY += gravity * step;
            simX += vx * step;
            simY += simVY * step;
            if (simY > 520f) break;
            float alpha = 1f - (i / 22f);
            DrawCircle(new Vector2(simX, simY), Mathf.Max(5f - i * 0.2f, 2f), new Color(1f, 0.84f, 0f, alpha * 0.75f));
        }
    }

    private void DrawBall(float bx, float by)
    {
        float size = ballRadius * 2.8f;
        // Shadow
        DrawCircle(new Vector2(bx + 3, by + 3), size/2f, new Color(0f,0f,0f,0.4f));
        if (ballTexture != null)
            DrawTextureRect(ballTexture, new Rect2(bx - size/2f, by - size/2f, size, size), false);
        else
            DrawCircle(new Vector2(bx, by), ballRadius, new Color(0.9f, 0.4f, 0.1f, 1f));
    }

    public override void _Draw()
    {
        if (courtTexture != null)
            DrawTextureRect(courtTexture, new Rect2(0, 0, 1280, 720), false);
        else
        {
            DrawRect(new Rect2(0, 0, 640, 720), new Color(0.18f, 0.0f, 0.35f, 1f));
            DrawRect(new Rect2(640, 0, 640, 720), new Color(0.15f, 0.0f, 0.28f, 1f));
        }

        DrawRect(new Rect2(0, 0, 8, 500), new Color(1f, 0.84f, 0f, 0.7f));
        DrawRect(new Rect2(0, 0, 640, 8), new Color(1f, 0.84f, 0f, 0.7f));
        DrawRect(new Rect2(632, 0, 8, 500), new Color(1f, 0.84f, 0f, 0.7f));
        DrawRect(new Rect2(640, 0, 8, 500), new Color(1f, 0.84f, 0f, 0.7f));
        DrawRect(new Rect2(640, 0, 640, 8), new Color(1f, 0.84f, 0f, 0.7f));
        DrawRect(new Rect2(1272, 0, 8, 500), new Color(1f, 0.84f, 0f, 0.7f));

        // Court background handles the floor

        // Divider handled by court image

        DrawHoop(hoop1X, hoopY, flash1Timer > 0);
        DrawHoop(hoop2X, hoopY, flash2Timer > 0);

        if (p1Holding && !ball1Active)
        {
            CalcVelocity(player1X, player1Y - 50f, hoop1X, p1HoldTime, out float vx1, out float vy1);
            DrawTrajectory(player1X, player1Y - 50f, vx1, vy1);
        }
        if (p2Holding && !ball2Active)
        {
            CalcVelocity(player2X, player2Y - 50f, hoop2X, p2HoldTime, out float vx2, out float vy2);
            DrawTrajectory(player2X, player2Y - 50f, vx2, vy2);
        }

        if (player1Texture != null)
        {
            float h = 160f, w = h * (player1Texture.GetWidth() / (float)player1Texture.GetHeight());
            DrawTextureRect(player1Texture, new Rect2(player1X - w/2f, player1Y - h, w, h), false);
        }
        if (player2Texture != null)
        {
            float h = 160f, w = h * (player2Texture.GetWidth() / (float)player2Texture.GetHeight());
            DrawTextureRect(player2Texture, new Rect2(player2X - w/2f, player2Y - h, w, h), false);
        }

        if (ball1Active) DrawBall(ball1X, ball1Y);
        else DrawBall(player1X, player1Y - 168f);
        if (ball2Active) DrawBall(ball2X, ball2Y);
        else DrawBall(player2X, player2Y - 168f);

        if (p1Holding)
        {
            DrawRect(new Rect2(60, 640, 200, 18), new Color(0.2f,0.2f,0.2f,0.8f));
            DrawRect(new Rect2(60, 640, p1HoldTime * 200f, 18), new Color(0.2f, 0.9f, 0.2f, 1f));
            DrawString(ThemeDB.FallbackFont, new Vector2(60, 635), "POWER", HorizontalAlignment.Left, -1, 14, new Color(1,1,1,1));
        }
        if (p2Holding)
        {
            DrawRect(new Rect2(1020, 640, 200, 18), new Color(0.2f,0.2f,0.2f,0.8f));
            DrawRect(new Rect2(1020, 640, p2HoldTime * 200f, 18), new Color(0.2f, 0.9f, 0.2f, 1f));
            DrawString(ThemeDB.FallbackFont, new Vector2(1020, 635), "POWER", HorizontalAlignment.Left, -1, 14, new Color(1,1,1,1));
        }

        if (flash1Timer > 0)
            DrawString(ThemeDB.FallbackFont, new Vector2(240, 160), "+1", HorizontalAlignment.Left, -1, 80, new Color(1f,1f,0f, flash1Timer/0.8f));
        if (flash2Timer > 0)
            DrawString(ThemeDB.FallbackFont, new Vector2(870, 160), "+1", HorizontalAlignment.Left, -1, 80, new Color(1f,1f,0f, flash2Timer/0.8f));

        // === DANE 1 HUD BOX (top left) ===
        DrawRect(new Rect2(10, 10, 180, 65), new Color(0f, 0f, 0f, 0.75f));
        DrawRect(new Rect2(10, 10, 180, 5), new Color(0.29f, 0f, 0.51f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(20, 38), "DANE 1", HorizontalAlignment.Left, -1, 18, new Color(1f,1f,1f,0.9f));
        DrawString(ThemeDB.FallbackFont, new Vector2(20, 68), $"{score1}", HorizontalAlignment.Left, -1, 38, new Color(1f, 0.84f, 0f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(20, 82), "Hold F + release", HorizontalAlignment.Left, -1, 12, new Color(1f,1f,1f,0.5f));

        // === ROUND 1 CENTER BOX ===
        int secs = Mathf.CeilToInt(timeLeft);
        Color timerColor = secs <= 10 ? new Color(1f, 0.2f, 0.2f, 1f) : new Color(1f, 1f, 1f, 1f);
        DrawRect(new Rect2(490, 10, 300, 65), new Color(0f, 0f, 0f, 0.75f));
        DrawRect(new Rect2(490, 10, 300, 5), new Color(1f, 0.84f, 0f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(560, 38), "ROUND 1", HorizontalAlignment.Left, -1, 18, new Color(1f, 0.84f, 0f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(610, 68), $"{secs}s", HorizontalAlignment.Left, -1, 32, timerColor);

        // === DANE 2 HUD BOX (top right) ===
        DrawRect(new Rect2(1090, 10, 180, 65), new Color(0f, 0f, 0f, 0.75f));
        DrawRect(new Rect2(1090, 10, 180, 5), new Color(1f, 0.84f, 0f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(1100, 38), "DANE 2", HorizontalAlignment.Left, -1, 18, new Color(1f,1f,1f,0.9f));
        DrawString(ThemeDB.FallbackFont, new Vector2(1100, 68), $"{score2}", HorizontalAlignment.Left, -1, 38, new Color(1f, 0.84f, 0f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(1100, 82), "Hold J + release", HorizontalAlignment.Left, -1, 12, new Color(1f,1f,1f,0.5f));
    }

    private void DrawHoop(float hx, float hy, bool lit)
    {
        Color rimColor = lit ? new Color(1f,1f,0.2f,1f) : new Color(1f,0.35f,0.05f,1f);
        DrawRect(new Rect2(hx - 58, hy - 58, 116, 14), new Color(1f,1f,1f,0.4f));
        DrawRect(new Rect2(hx - 55, hy - 55, 110, 10), new Color(0.95f,0.95f,0.95f,1f));
        DrawLine(new Vector2(hx, hy - 47), new Vector2(hx, hy - 4), new Color(0.6f,0.6f,0.6f,1f), 3f);
        // Glow effect behind rim
        DrawLine(new Vector2(hx - rimRadius, hy), new Vector2(hx + rimRadius, hy), new Color(1f,1f,1f,0.3f), 10f);
        DrawLine(new Vector2(hx - rimRadius, hy), new Vector2(hx + rimRadius, hy), rimColor, 6f);
        DrawCircle(new Vector2(hx - rimRadius, hy), 8f, new Color(1f,1f,1f,0.5f));
        DrawCircle(new Vector2(hx - rimRadius, hy), 6f, rimColor);
        DrawCircle(new Vector2(hx + rimRadius, hy), 8f, new Color(1f,1f,1f,0.5f));
        DrawCircle(new Vector2(hx + rimRadius, hy), 6f, rimColor);
        int segments = 6;
        float netBottom = hy + 32f;
        for (int i = 0; i <= segments; i++)
        {
            float nx = hx - rimRadius + (2f * rimRadius * i / segments);
            DrawLine(new Vector2(nx, hy), new Vector2(hx - 12f + (24f * i / segments), netBottom), new Color(1,1,1,0.6f), 1.5f);
        }
        DrawLine(new Vector2(hx - 12f, netBottom), new Vector2(hx + 12f, netBottom), new Color(1,1,1,0.6f), 1.5f);
    }
}

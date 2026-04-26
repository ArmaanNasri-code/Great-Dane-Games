using Godot;

public partial class Sumo : Node2D
{
    private const int TotalRows = 5;
    private const float DoubleTapWindow = 0.35f;

    private int p1Row = 0;
    private int p2Row = 0;
    private bool p1Lost = false;
    private bool p2Lost = false;

    private bool p1CorrectDoor = false;
    private bool p2CorrectDoor = false;

    private bool? p1WrongDoorWasLeft = null;
    private bool? p2WrongDoorWasLeft = null;
    private int p1WrongRow = -1;
    private int p2WrongRow = -1;

    private bool p1WaitingDouble = false;
    private bool p2WaitingDouble = false;
    private float p1TapTimer = 0f;
    private float p2TapTimer = 0f;

    private float p1FeedbackTimer = 0f;
    private float p2FeedbackTimer = 0f;
    private bool p1LastCorrect = false;
    private bool p2LastCorrect = false;
    private const float FeedbackDuration = 0.9f;

    private int currentTurn = 1;
    private bool turnComplete = false;

    private bool gameOver = false;
    private float resultTimer = 0f;
    private int winner = 0;

    private RandomNumberGenerator rng = new RandomNumberGenerator();
    private Texture2D p1Texture;
    private Texture2D p2Texture;

    public override void _Ready()
    {
        rng.Randomize();
        p1CorrectDoor = rng.RandiRange(0, 1) == 0;
        p2CorrectDoor = rng.RandiRange(0, 1) == 0;
        p1Texture = GD.Load<Texture2D>("res://sumo_player1.png");
        p2Texture = GD.Load<Texture2D>("res://sumo_player2.png");
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        if (gameOver)
        {
            resultTimer += dt;
            if (resultTimer > 1f && (Input.IsActionJustPressed("p1_action") || Input.IsActionJustPressed("p2_action")))
            {
                GameManager.Instance.RoundWinner = winner;
                GetTree().ChangeSceneToFile("res://RoundResult.tscn");
            }
            QueueRedraw();
            return;
        }

        if (p1FeedbackTimer > 0)
        {
            p1FeedbackTimer -= dt;
            if (p1FeedbackTimer <= 0 && currentTurn == 1) { currentTurn = 2; turnComplete = false; }
        }
        if (p2FeedbackTimer > 0)
        {
            p2FeedbackTimer -= dt;
            if (p2FeedbackTimer <= 0 && currentTurn == 2) { currentTurn = 1; turnComplete = false; }
        }

        if (currentTurn == 1 && !turnComplete && !p1Lost && p1FeedbackTimer <= 0)
        {
            if (Input.IsActionJustPressed("p1_action"))
            {
                if (p1WaitingDouble) { p1WaitingDouble = false; p1TapTimer = 0f; ResolveChoice(1, false); }
                else { p1WaitingDouble = true; p1TapTimer = DoubleTapWindow; }
            }
            if (p1WaitingDouble) { p1TapTimer -= dt; if (p1TapTimer <= 0f) { p1WaitingDouble = false; ResolveChoice(1, true); } }
        }

        if (currentTurn == 2 && !turnComplete && !p2Lost && p2FeedbackTimer <= 0)
        {
            if (Input.IsActionJustPressed("p2_action"))
            {
                if (p2WaitingDouble) { p2WaitingDouble = false; p2TapTimer = 0f; ResolveChoice(2, false); }
                else { p2WaitingDouble = true; p2TapTimer = DoubleTapWindow; }
            }
            if (p2WaitingDouble) { p2TapTimer -= dt; if (p2TapTimer <= 0f) { p2WaitingDouble = false; ResolveChoice(2, true); } }
        }

        QueueRedraw();
    }

    private void ResolveChoice(int player, bool choseLeft)
    {
        bool correct = player == 1 ? (choseLeft == p1CorrectDoor) : (choseLeft == p2CorrectDoor);
        turnComplete = true;

        if (player == 1)
        {
            p1LastCorrect = correct;
            p1FeedbackTimer = FeedbackDuration;
            if (!correct) { p1WrongDoorWasLeft = choseLeft; p1WrongRow = p1Row; p1Row++; if (p1Row >= TotalRows) { p1Lost = true; CheckGameOver(); return; } }
            else { p1WrongDoorWasLeft = null; p1WrongRow = -1; }
            p1CorrectDoor = rng.RandiRange(0, 1) == 0;
        }
        else
        {
            p2LastCorrect = correct;
            p2FeedbackTimer = FeedbackDuration;
            if (!correct) { p2WrongDoorWasLeft = choseLeft; p2WrongRow = p2Row; p2Row++; if (p2Row >= TotalRows) { p2Lost = true; CheckGameOver(); return; } }
            else { p2WrongDoorWasLeft = null; p2WrongRow = -1; }
            p2CorrectDoor = rng.RandiRange(0, 1) == 0;
        }
    }

    private void CheckGameOver()
    {
        if (p1Lost && p2Lost) { winner = 0; gameOver = true; }
        else if (p1Lost) { winner = 2; gameOver = true; }
        else if (p2Lost) { winner = 1; gameOver = true; }
    }

    private const float RowStartY = 120f;
    private const float RowGap = 95f;
    private const float DoorW = 80f;
    private const float DoorH = 75f;
    private const float P1CenterX = 320f;
    private const float P2CenterX = 960f;
    private const float LeftDoorOffset = -90f;
    private const float RightDoorOffset = 90f;

    public override void _Draw()
    {
        if (gameOver) { DrawResult(); return; }

        DrawRect(new Rect2(0, 0, 1280, 720), new Color(0.08f, 0.05f, 0.16f, 1f));
        DrawRect(new Rect2(0, 0, 1280, 8), new Color(1f, 0.84f, 0f, 1f));
        DrawRect(new Rect2(0, 712, 1280, 8), new Color(1f, 0.84f, 0f, 1f));
        DrawLine(new Vector2(640, 0), new Vector2(640, 720), new Color(1f, 0.84f, 0f, 0.25f), 3f);

        // HUD boxes - matching Racing style
        DrawRect(new Rect2(10, 10, 200, 55), new Color(0f, 0f, 0f, 0.75f));
        DrawRect(new Rect2(10, 10, 200, 5), new Color(0.29f, 0f, 0.51f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(20, 36), "DANE 1", HorizontalAlignment.Left, -1, 18, new Color(1f,1f,1f,0.9f));
        DrawString(ThemeDB.FallbackFont, new Vector2(20, 56), "F=Left  FF=Right", HorizontalAlignment.Left, -1, 13, new Color(1f,1f,1f,0.6f));

        DrawRect(new Rect2(440, 10, 400, 55), new Color(0f, 0f, 0f, 0.75f));
        DrawRect(new Rect2(440, 10, 400, 5), new Color(1f, 0.84f, 0f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(470, 36), "ROUND 3: SURVIVE THE PROFESSOR", HorizontalAlignment.Left, -1, 16, new Color(1f,0.84f,0f,1f));

        // Turn indicator
        string turnText = currentTurn == 1 ? "▶  DANE 1'S TURN" : "▶  DANE 2'S TURN";
        Color turnBg = currentTurn == 1 ? new Color(0.29f, 0f, 0.51f, 1f) : new Color(0.5f, 0.38f, 0f, 1f);
        DrawRect(new Rect2(470, 48, 340, 22), turnBg);
        DrawString(ThemeDB.FallbackFont, new Vector2(490, 65), turnText, HorizontalAlignment.Left, -1, 14, new Color(1f,1f,1f,1f));

        DrawRect(new Rect2(1070, 10, 200, 55), new Color(0f, 0f, 0f, 0.75f));
        DrawRect(new Rect2(1070, 10, 200, 5), new Color(1f, 0.84f, 0f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(1080, 36), "DANE 2", HorizontalAlignment.Left, -1, 18, new Color(1f,1f,1f,0.9f));
        DrawString(ThemeDB.FallbackFont, new Vector2(1080, 56), "J=Left  JJ=Right", HorizontalAlignment.Left, -1, 13, new Color(1f,1f,1f,0.6f));

        // Professor centered at bottom
        DrawProfessor(640f, 560f);

        // Rows
        for (int row = 0; row < TotalRows; row++)
        {
            float y = RowStartY + row * RowGap;
            DrawRowSide(P1CenterX, y, row, 1);
            DrawRowSide(P2CenterX, y, row, 2);
        }

        DrawPlayer(1, p1Row, p1FeedbackTimer > 0, p1LastCorrect, p1WaitingDouble);
        DrawPlayer(2, p2Row, p2FeedbackTimer > 0, p2LastCorrect, p2WaitingDouble);
    }

    private void DrawProfessor(float cx, float py)
    {
        // Desk
        DrawRect(new Rect2(cx - 80, py - 25, 160, 40), new Color(0.3f, 0.18f, 0.05f, 1f));
        DrawRect(new Rect2(cx - 80, py - 25, 160, 6), new Color(0.48f, 0.3f, 0.08f, 1f));
        DrawRect(new Rect2(cx - 75, py + 15, 12, 25), new Color(0.25f, 0.14f, 0.04f, 1f));
        DrawRect(new Rect2(cx + 63, py + 15, 12, 25), new Color(0.25f, 0.14f, 0.04f, 1f));
        // Papers
        DrawRect(new Rect2(cx - 60, py - 22, 35, 22), new Color(0.95f, 0.95f, 0.85f, 0.9f));
        DrawRect(new Rect2(cx + 15, py - 22, 28, 20), new Color(0.9f, 0.9f, 0.8f, 0.85f));
        // Body
        DrawRect(new Rect2(cx - 22, py - 82, 44, 60), new Color(0.15f, 0.1f, 0.3f, 1f));
        DrawRect(new Rect2(cx - 5, py - 78, 10, 38), new Color(0.8f, 0.1f, 0.1f, 1f));
        // Arms folded on desk
        DrawRect(new Rect2(cx - 55, py - 36, 40, 13), new Color(0.15f, 0.1f, 0.3f, 1f));
        DrawRect(new Rect2(cx + 15, py - 36, 40, 13), new Color(0.15f, 0.1f, 0.3f, 1f));
        DrawCircle(new Vector2(cx - 16, py - 30), 8f, new Color(0.85f, 0.7f, 0.55f, 1f));
        DrawCircle(new Vector2(cx + 16, py - 30), 8f, new Color(0.85f, 0.7f, 0.55f, 1f));
        // Head
        DrawCircle(new Vector2(cx, py - 96), 22f, new Color(0.85f, 0.7f, 0.55f, 1f));
        // Hair
        DrawRect(new Rect2(cx - 22, py - 118, 44, 12), new Color(0.25f, 0.15f, 0.05f, 1f));
        // Angry eyebrows
        DrawLine(new Vector2(cx - 16, py - 108), new Vector2(cx - 4, py - 103), new Color(0.15f, 0.08f, 0f, 1f), 3f);
        DrawLine(new Vector2(cx + 4, py - 103), new Vector2(cx + 16, py - 108), new Color(0.15f, 0.08f, 0f, 1f), 3f);
        // Glasses
        DrawRect(new Rect2(cx - 18, py - 102, 13, 8), new Color(0.1f, 0.1f, 0.1f, 0.8f));
        DrawRect(new Rect2(cx + 5, py - 102, 13, 8), new Color(0.1f, 0.1f, 0.1f, 0.8f));
        DrawLine(new Vector2(cx - 5, py - 98), new Vector2(cx + 5, py - 98), new Color(0.1f,0.1f,0.1f,1f), 2f);
        // Angry mouth (frown)
        DrawLine(new Vector2(cx - 10, py - 80), new Vector2(cx, py - 84), new Color(0.3f, 0.1f, 0f, 1f), 2f);
        DrawLine(new Vector2(cx, py - 84), new Vector2(cx + 10, py - 80), new Color(0.3f, 0.1f, 0f, 1f), 2f);
        // Speech bubble "DETENTION!"
        DrawRect(new Rect2(cx + 26, py - 130, 100, 26), new Color(1f, 1f, 1f, 0.95f));
        DrawRect(new Rect2(cx + 26, py - 130, 100, 26), new Color(0.8f, 0.1f, 0.1f, 0.4f));
        DrawLine(new Vector2(cx + 26, py - 115), new Vector2(cx + 12, py - 105), new Color(1f,1f,1f,0.95f), 4f);
        DrawString(ThemeDB.FallbackFont, new Vector2(cx + 30, py - 110), "DETENTION!", HorizontalAlignment.Left, -1, 13, new Color(0.8f, 0f, 0f, 1f));
        // Label
        DrawString(ThemeDB.FallbackFont, new Vector2(cx - 38, py - 142), "THE PROFESSOR", HorizontalAlignment.Left, -1, 12, new Color(1f, 0.2f, 0.2f, 1f));
    }

    private void DrawRowSide(float cx, float y, int row, int player)
    {
        bool isActivePlayer = (currentTurn == player);
        int activeRow = player == 1 ? p1Row : p2Row;
        bool isCurrentRow = (row == activeRow && isActivePlayer && !turnComplete);

        Color bg = isCurrentRow ? new Color(0.25f, 0.15f, 0.4f, 1f)
            : row % 2 == 0 ? new Color(0.18f, 0.1f, 0.3f, 1f) : new Color(0.14f, 0.08f, 0.24f, 1f);

        DrawRect(new Rect2(cx - 200, y - 4, 400, DoorH + 28), bg);
        DrawRect(new Rect2(cx - 200, y - 4, 400, 3),
            isCurrentRow ? new Color(1f, 0.84f, 0f, 0.7f) : new Color(1f, 0.84f, 0f, 0.12f));
        DrawString(ThemeDB.FallbackFont, new Vector2(cx - 195, y + 18),
            $"Row {row + 1}", HorizontalAlignment.Left, -1, 11, new Color(1f,1f,1f,0.25f));

        float ldx = cx + LeftDoorOffset - DoorW / 2f;
        float rdx = cx + RightDoorOffset - DoorW / 2f;

        bool? wrongLeft = player == 1 ? p1WrongDoorWasLeft : p2WrongDoorWasLeft;
        int wrongRow   = player == 1 ? p1WrongRow : p2WrongRow;
        bool showLeftX  = wrongLeft.HasValue && wrongLeft.Value  && wrongRow == row;
        bool showRightX = wrongLeft.HasValue && !wrongLeft.Value && wrongRow == row;

        DrawDoor(ldx, y, "LEFT", "(F/J)", showLeftX);
        DrawDoor(rdx, y, "RIGHT", "(FF/JJ)", showRightX);
    }

    private void DrawDoor(float x, float y, string label, string hint, bool showX)
    {
        DrawRect(new Rect2(x - 4, y - 4, DoorW + 8, DoorH + 8),
            showX ? new Color(0.8f, 0.1f, 0.1f, 1f) : new Color(0.5f, 0.35f, 0.1f, 1f));
        DrawRect(new Rect2(x, y, DoorW, DoorH),
            showX ? new Color(0.4f, 0.05f, 0.05f, 1f) : new Color(0.32f, 0.18f, 0.04f, 1f));
        DrawRect(new Rect2(x + 7, y + 6, DoorW - 14, DoorH/2f - 8),
            showX ? new Color(0.5f, 0.08f, 0.08f, 1f) : new Color(0.4f, 0.24f, 0.07f, 1f));
        DrawRect(new Rect2(x + 7, y + DoorH/2f + 2, DoorW - 14, DoorH/2f - 8),
            showX ? new Color(0.5f, 0.08f, 0.08f, 1f) : new Color(0.4f, 0.24f, 0.07f, 1f));
        DrawCircle(new Vector2(x + DoorW - 10, y + DoorH/2f), 4f, new Color(1f, 0.84f, 0f, 1f));

        if (showX)
        {
            float pad = 10f;
            DrawLine(new Vector2(x+pad, y+pad), new Vector2(x+DoorW-pad, y+DoorH-pad), new Color(1f,0.1f,0.1f,1f), 5f);
            DrawLine(new Vector2(x+DoorW-pad, y+pad), new Vector2(x+pad, y+DoorH-pad), new Color(1f,0.1f,0.1f,1f), 5f);
        }

        DrawString(ThemeDB.FallbackFont, new Vector2(x + 5, y + DoorH + 13),
            label, HorizontalAlignment.Left, -1, 11, new Color(1f,1f,1f,0.8f));
        DrawString(ThemeDB.FallbackFont, new Vector2(x + 5, y + DoorH + 24),
            hint, HorizontalAlignment.Left, -1, 10, new Color(1f,0.84f,0f,0.7f));
    }

    private void DrawPlayer(int player, int row, bool showFeedback, bool lastCorrect, bool waitingDouble)
    {
        float cx = player == 1 ? P1CenterX : P2CenterX;
        float y = RowStartY + row * RowGap;
        Texture2D tex = player == 1 ? p1Texture : p2Texture;

        float spriteH = 70f;
        float spriteW = tex != null ? spriteH * (tex.GetWidth() / (float)tex.GetHeight()) : 50f;
        float spriteX = cx - spriteW / 2f;
        float spriteY = y - spriteH - 2f;

        bool isActive = currentTurn == player;
        Color tint = isActive ? new Color(1f,1f,1f,1f) : new Color(0.5f,0.5f,0.5f,0.45f);

        if (tex != null)
            DrawTextureRect(tex, new Rect2(spriteX, spriteY, spriteW, spriteH), false, tint);
        else
            DrawCircle(new Vector2(cx, y - 20), 20f, player == 1 ? new Color(0.29f,0f,0.51f,1f) : new Color(1f,0.84f,0f,1f));

        if (isActive && !turnComplete)
            DrawLine(new Vector2(cx, y + 4), new Vector2(cx, y + 22), new Color(1f, 0.84f, 0f, 0.8f), 4f);

        if (showFeedback)
        {
            string msg = lastCorrect ? "✓ STAY!" : "✗ DOWN!";
            Color col = lastCorrect ? new Color(0.2f,1f,0.2f,1f) : new Color(1f,0.2f,0.2f,1f);
            DrawRect(new Rect2(cx - 38, spriteY - 28, 76, 24), new Color(0f,0f,0f,0.85f));
            DrawString(ThemeDB.FallbackFont, new Vector2(cx - 30, spriteY - 10), msg, HorizontalAlignment.Left, -1, 17, col);
        }

        if (waitingDouble)
        {
            DrawRect(new Rect2(cx - 44, spriteY - 52, 88, 20), new Color(1f,0.84f,0f,0.22f));
            DrawString(ThemeDB.FallbackFont, new Vector2(cx - 38, spriteY - 37), "tap again=RIGHT", HorizontalAlignment.Left, -1, 11, new Color(1f,0.84f,0f,1f));
        }
    }

    private void DrawResult()
    {
        DrawRect(new Rect2(0, 0, 1280, 720), new Color(0.08f, 0f, 0.15f, 1f));
        DrawRect(new Rect2(0, 0, 1280, 8), new Color(1f, 0.84f, 0f, 1f));
        DrawRect(new Rect2(0, 712, 1280, 8), new Color(1f, 0.84f, 0f, 1f));

        if (winner == 0)
        {
            DrawString(ThemeDB.FallbackFont, new Vector2(330, 220), "BOTH DUNGEONED!", HorizontalAlignment.Left, -1, 64, new Color(1f,0.2f,0.2f,1f));
            DrawString(ThemeDB.FallbackFont, new Vector2(400, 310), "The Professor wins this one.", HorizontalAlignment.Left, -1, 28, new Color(1f,1f,1f,0.8f));
        }
        else
        {
            int loser = winner == 1 ? 2 : 1;
            DrawString(ThemeDB.FallbackFont, new Vector2(320, 190), $"DANE {winner} SURVIVES!", HorizontalAlignment.Left, -1, 64, new Color(1f,0.84f,0f,1f));
            DrawString(ThemeDB.FallbackFont, new Vector2(340, 275), "Made it past the Professor!", HorizontalAlignment.Left, -1, 28, new Color(1f,1f,1f,0.85f));
            DrawRect(new Rect2(320, 310, 640, 55), new Color(0.5f,0f,0f,0.4f));
            DrawString(ThemeDB.FallbackFont, new Vector2(345, 348), $"Dane {loser} sent to the DUNGEON.", HorizontalAlignment.Left, -1, 22, new Color(1f,0.3f,0.3f,1f));
        }

        DrawLine(new Vector2(200, 390), new Vector2(1080, 390), new Color(1f,0.84f,0f,0.3f), 2f);
        DrawRect(new Rect2(390, 415, 500, 55), new Color(1f,0.84f,0f,0.12f));
        DrawString(ThemeDB.FallbackFont, new Vector2(415, 453), "Proceeding to Final Results...", HorizontalAlignment.Left, -1, 24, new Color(1f,1f,1f,0.9f));

        if (resultTimer > 1f && (int)(resultTimer * 2) % 2 == 0)
            DrawString(ThemeDB.FallbackFont, new Vector2(455, 555), "Press F or J to continue", HorizontalAlignment.Left, -1, 24, new Color(1f,0.84f,0f,0.9f));
    }
}

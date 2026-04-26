using Godot;
using System.Collections.Generic;

public partial class Racing : Node2D
{
    private float[] lanes1 = { 393f, 446f, 500f };
    private float[] lanes2 = { 779f, 831f, 884f };

    private float p1LanePos = 446f;
    private float p2LanePos = 831f;
    private float laneSpeed = 180f;
    private int p1Dir = 1;
    private int p2Dir = 1;

    private float p1Time = 0f;
    private float p2Time = 0f;
    private float p1PenaltyFlash = 0f;
    private float p2PenaltyFlash = 0f;
    private string p1PenaltyText = "";
    private string p2PenaltyText = "";

    private bool p1Finished = false;
    private bool p2Finished = false;
    private bool gameOver = false;
    private float trackLength = 6000f;
    private float p1Progress = 0f;
    private float p2Progress = 0f;
    private float scrollSpeed = 180f;

    private struct Obstacle
    {
        public float X, Y;
        public int Type;
        public bool Hit1, Hit2;
    }
    private List<Obstacle> obstacles = new List<Obstacle>();
    private float obstacleSpawnTimer = 0f;
    private float obstacleSpawnInterval = 1.2f;
    private float[] penaltySecs = { 1f, 2f, 3f, 5f };
    private string[] penaltyLabels = { "+1s", "+2s", "+3s", "+5s" };
    private Color[] obstacleColors = {
        new Color(1f, 0.6f, 0f, 1f),
        new Color(0.3f, 0.3f, 0.3f, 1f),
        new Color(0.6f, 0.6f, 0.6f, 1f),
        new Color(0f, 0.3f, 0.8f, 1f)
    };

    private float p1CarY = 520f;
    private float p2CarY = 520f;
    private float resultTimer = 0f;
    private bool showResult = false;
    private Texture2D publicSafetyTexture;
    private Texture2D campusBgTexture;
    private Texture2D professorTexture;
    // Professor obstacle - moves side to side across both lanes
    private float profObsX = 490f;
    private float profObsY = 300f;
    private float profObsSpeed = 120f;
    private int profObsDir = 1;
    private float profObsHitFlash1 = 0f;
    private float profObsHitFlash2 = 0f;

    public override void _Ready()
    {
        var rng = new RandomNumberGenerator();
        rng.Randomize();
        SpawnObstacleRow(rng);
        publicSafetyTexture = GD.Load<Texture2D>("res://publicsafety.png");
        campusBgTexture = GD.Load<Texture2D>("res://campus_bg.png");
        professorTexture = GD.Load<Texture2D>("res://professor.png");
    }

    public override void _Process(double delta)
    {
        if (gameOver)
        {
            resultTimer += (float)delta;
            if (resultTimer > 1f && (Input.IsActionJustPressed("p1_action") || Input.IsActionJustPressed("p2_action")))
            {
                GameManager.Instance.RoundWinner = p1Time <= p2Time ? 1 : 2;
                GetTree().ChangeSceneToFile("res://RoundResult.tscn");
            }
            QueueRedraw();
            return;
        }

        float dt = (float)delta;
        var rng = new RandomNumberGenerator();

        if (!p1Finished) p1Time += dt;
        if (!p2Finished) p2Time += dt;
        if (p1PenaltyFlash > 0) p1PenaltyFlash -= dt;
        if (p2PenaltyFlash > 0) p2PenaltyFlash -= dt;

        bool p1Locked = Input.IsActionPressed("p1_action");
        bool p2Locked = Input.IsActionPressed("p2_action");

        if (!p1Locked)
        {
            p1LanePos += laneSpeed * p1Dir * dt;
            if (p1LanePos >= lanes1[2]) { p1LanePos = lanes1[2]; p1Dir = -1; }
            if (p1LanePos <= lanes1[0]) { p1LanePos = lanes1[0]; p1Dir = 1; }
        }
        if (!p2Locked)
        {
            p2LanePos += laneSpeed * p2Dir * dt;
            if (p2LanePos >= lanes2[2]) { p2LanePos = lanes2[2]; p2Dir = -1; }
            if (p2LanePos <= lanes2[0]) { p2LanePos = lanes2[0]; p2Dir = 1; }
        }

        if (!p1Finished) p1Progress += scrollSpeed * dt;
        if (!p2Finished) p2Progress += scrollSpeed * dt;
        if (p1Progress >= trackLength && !p1Finished) p1Finished = true;
        if (p2Progress >= trackLength && !p2Finished) p2Finished = true;
        if (p1Finished && p2Finished) { gameOver = true; showResult = true; }

        obstacleSpawnTimer += dt;
        if (obstacleSpawnTimer >= obstacleSpawnInterval)
        {
            obstacleSpawnTimer = 0f;
            SpawnObstacleRow(rng);
        }

        for (int i = obstacles.Count - 1; i >= 0; i--)
        {
            var obs = obstacles[i];
            obs.Y += scrollSpeed * dt;

            if (!obs.Hit1 && !p1Finished &&
                Mathf.Abs(obs.X - p1LanePos) < 20f &&
                Mathf.Abs(obs.Y - p1CarY) < 25f)
            {
                obs.Hit1 = true;
                p1Time += penaltySecs[obs.Type];
                p1PenaltyFlash = 1.0f;
                p1PenaltyText = penaltyLabels[obs.Type];
            }

            float obs2X = obs.X + 385f;
            if (!obs.Hit2 && !p2Finished &&
                Mathf.Abs(obs2X - p2LanePos) < 20f &&
                Mathf.Abs(obs.Y - p2CarY) < 25f)
            {
                obs.Hit2 = true;
                p2Time += penaltySecs[obs.Type];
                p2PenaltyFlash = 1.0f;
                p2PenaltyText = penaltyLabels[obs.Type];
            }

            obstacles[i] = obs;
            if (obs.Y > 720f) obstacles.RemoveAt(i);
        }

        // Professor obstacle - moves side to side across both tracks
        profObsX += profObsSpeed * profObsDir * dt;
        if (profObsX > 590f) profObsDir = -1;
        if (profObsX < 390f) profObsDir = 1;

        if (profObsHitFlash1 > 0) profObsHitFlash1 -= dt;
        if (profObsHitFlash2 > 0) profObsHitFlash2 -= dt;

        // Check P1 collision with professor
        if (!p1Finished && Mathf.Abs(profObsX - p1LanePos) < 28f && Mathf.Abs(profObsY - p1CarY) < 30f && profObsHitFlash1 <= 0)
        {
            p1Time += 3f;
            p1PenaltyFlash = 1.2f;
            p1PenaltyText = "+3s (PROF!)";
            profObsHitFlash1 = 1.5f;
        }
        // Check P2 collision with professor (offset to right track)
        float profObsX2 = profObsX + 385f;
        if (!p2Finished && Mathf.Abs(profObsX2 - p2LanePos) < 28f && Mathf.Abs(profObsY - p2CarY) < 30f && profObsHitFlash2 <= 0)
        {
            p2Time += 3f;
            p2PenaltyFlash = 1.2f;
            p2PenaltyText = "+3s (PROF!)";
            profObsHitFlash2 = 1.5f;
        }

        QueueRedraw();
    }

    private void SpawnObstacleRow(RandomNumberGenerator rng)
    {
        int numObs = rng.RandiRange(1, 2);
        var usedLanes = new List<int>();
        for (int i = 0; i < numObs; i++)
        {
            int lane;
            do { lane = rng.RandiRange(0, 2); } while (usedLanes.Contains(lane));
            usedLanes.Add(lane);
            float r = rng.Randf();
            int type = r < 0.4f ? 0 : r < 0.7f ? 1 : r < 0.9f ? 2 : 3;
            obstacles.Add(new Obstacle { X = lanes1[lane], Y = -60f, Type = type, Hit1 = false, Hit2 = false });
        }
    }

    public override void _Draw()
    {
        if (showResult) { DrawResult(); return; }

        // Campus background
        if (campusBgTexture != null)
        {
            DrawTextureRect(campusBgTexture, new Rect2(0, 0, 1280, 720), false);
        }
        else
        {
            DrawRect(new Rect2(0, 0, 640, 720), new Color(0.3f, 0.6f, 0.3f, 1f));
            DrawRect(new Rect2(640, 0, 640, 720), new Color(0.28f, 0.55f, 0.28f, 1f));
        }



        // Road surfaces
        // Road overlay removed - using background image


        // Lane dividers


        // Center divider


        // Progress bars
        float p1Pct = Mathf.Min(p1Progress / trackLength, 1f);
        float p2Pct = Mathf.Min(p2Progress / trackLength, 1f);
        DrawRect(new Rect2(490, 700, 140, 12), new Color(0.2f,0.2f,0.2f,0.8f));
        DrawRect(new Rect2(490, 700, 140 * p1Pct, 12), new Color(0.29f, 0f, 0.51f, 1f));
        DrawRect(new Rect2(730, 700, 140, 12), new Color(0.2f,0.2f,0.2f,0.8f));
        DrawRect(new Rect2(730, 700, 140 * p2Pct, 12), new Color(1f, 0.84f, 0f, 1f));

        // Obstacles
        foreach (var obs in obstacles)
        {
            DrawObstacle(obs.X, obs.Y, obs.Type);
            DrawObstacle(obs.X + 385f, obs.Y, obs.Type);
        }

        // Professor obstacle
        DrawProfessorObstacle(profObsX, profObsY, profObsHitFlash1 > 0);
        DrawProfessorObstacle(profObsX + 385f, profObsY, profObsHitFlash2 > 0);

        // Cars
        bool p1Locked = Input.IsActionPressed("p1_action");
        bool p2Locked = Input.IsActionPressed("p2_action");
        DrawCar(p1LanePos, p1CarY, new Color(0.29f, 0f, 0.51f, 1f), p1Finished);
        DrawCar(p2LanePos, p2CarY, new Color(1f, 0.84f, 0f, 1f), p2Finished);

        if (p1Locked && !p1Finished)
            DrawRect(new Rect2(p1LanePos - 10, p1CarY - 22, 20, 3), new Color(0f, 1f, 0f, 0.8f));
        if (p2Locked && !p2Finished)
            DrawRect(new Rect2(p2LanePos - 10, p2CarY - 22, 20, 3), new Color(0f, 1f, 0f, 0.8f));

        // Penalty flash
        if (p1PenaltyFlash > 0)
            DrawString(ThemeDB.FallbackFont, new Vector2(500, 300), p1PenaltyText,
                HorizontalAlignment.Left, -1, 64, new Color(1f, 0.1f, 0.1f, p1PenaltyFlash));
        if (p2PenaltyFlash > 0)
            DrawString(ThemeDB.FallbackFont, new Vector2(740, 300), p2PenaltyText,
                HorizontalAlignment.Left, -1, 64, new Color(1f, 0.1f, 0.1f, p2PenaltyFlash));

        // Finish banners
        if (p1Finished)
            DrawString(ThemeDB.FallbackFont, new Vector2(480, 350), "FINISHED!",
                HorizontalAlignment.Left, -1, 48, new Color(1f,1f,0f,1f));
        if (p2Finished)
            DrawString(ThemeDB.FallbackFont, new Vector2(720, 350), "FINISHED!",
                HorizontalAlignment.Left, -1, 48, new Color(1f,1f,0f,1f));

        // HUD
        // Dane 1 HUD box - left side
        DrawRect(new Rect2(10, 10, 160, 65), new Color(0f, 0f, 0f, 0.7f));
        DrawRect(new Rect2(10, 10, 160, 65), new Color(0.29f, 0f, 0.51f, 0.5f));
        DrawRect(new Rect2(10, 10, 160, 4), new Color(0.29f, 0f, 0.51f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(18, 32), "DANE 1", HorizontalAlignment.Left, -1, 18, new Color(1f,1f,1f,0.9f));
        DrawString(ThemeDB.FallbackFont, new Vector2(18, 65), string.Format("{0:F1}s", p1Time), HorizontalAlignment.Left, -1, 34, new Color(0.8f, 0.6f, 1f, 1f));

        // Round 2 center box
        DrawRect(new Rect2(570, 8, 140, 40), new Color(0f, 0f, 0f, 0.7f));
        DrawRect(new Rect2(570, 8, 140, 40), new Color(1f, 0.84f, 0f, 0.3f));
        DrawRect(new Rect2(570, 8, 140, 3), new Color(1f, 0.84f, 0f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(592, 36), "ROUND 2", HorizontalAlignment.Left, -1, 22, new Color(1f, 0.84f, 0f, 1f));

        // Dane 2 HUD box - right side
        DrawRect(new Rect2(1110, 10, 160, 65), new Color(0f, 0f, 0f, 0.7f));
        DrawRect(new Rect2(1110, 10, 160, 65), new Color(0.29f, 0f, 0.51f, 0.5f));
        DrawRect(new Rect2(1110, 10, 160, 4), new Color(1f, 0.84f, 0f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(1118, 32), "DANE 2", HorizontalAlignment.Left, -1, 18, new Color(1f,1f,1f,0.9f));
        DrawString(ThemeDB.FallbackFont, new Vector2(1118, 65), string.Format("{0:F1}s", p2Time), HorizontalAlignment.Left, -1, 34, new Color(1f, 0.84f, 0f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(490, 680), "Hold F = lock lane", HorizontalAlignment.Left, -1, 16, new Color(1,1,1,0.9f));
        DrawString(ThemeDB.FallbackFont, new Vector2(730, 680), "Hold J = lock lane", HorizontalAlignment.Left, -1, 16, new Color(1,1,1,0.9f));
    }

    private void DrawObstacle(float x, float y, int type)
    {
        Color c = obstacleColors[type];
        if (type == 0) // cone
        {
            DrawColoredPolygon(new Vector2[] {
                new Vector2(x, y - 16), new Vector2(x - 10, y + 10), new Vector2(x + 10, y + 10)
            }, c);
            DrawRect(new Rect2(x - 12, y + 10, 24, 5), new Color(1f,1f,1f,0.8f));
        }
        else if (type == 1) // pothole
        {
            DrawCircle(new Vector2(x, y), 13f, c);
            DrawCircle(new Vector2(x, y), 8f, new Color(0.15f,0.15f,0.15f,1f));
        }
        else if (type == 2) // speedbump
        {
            DrawRect(new Rect2(x - 18, y - 5, 36, 10), c);
            DrawString(ThemeDB.FallbackFont, new Vector2(x - 14, y + 4), "BUMP", HorizontalAlignment.Left, -1, 10, new Color(1,1,1,1));
        }
        else // public safety
        {
            if (publicSafetyTexture != null)
                DrawTextureRect(publicSafetyTexture, new Rect2(x - 25, y - 14, 50, 30), false);
            else
            {
                DrawRect(new Rect2(x - 12, y - 8, 24, 16), c);
                DrawString(ThemeDB.FallbackFont, new Vector2(x - 8, y + 4), "UPD", HorizontalAlignment.Left, -1, 8, new Color(1,1,1,1));
            }
        }
    }

    private void DrawCar(float x, float y, Color color, bool finished)
    {
        if (finished) return;
        DrawRect(new Rect2(x - 14, y - 22, 28, 38), color);
        DrawRect(new Rect2(x - 10, y - 20, 20, 10), new Color(0.6f, 0.9f, 1f, 0.8f));
        DrawRect(new Rect2(x - 18, y - 16, 7, 10), new Color(0.1f,0.1f,0.1f,1f));
        DrawRect(new Rect2(x + 11, y - 16, 7, 10), new Color(0.1f,0.1f,0.1f,1f));
        DrawRect(new Rect2(x - 18, y + 4, 7, 10), new Color(0.1f,0.1f,0.1f,1f));
        DrawRect(new Rect2(x + 11, y + 4, 7, 10), new Color(0.1f,0.1f,0.1f,1f));
    }

    private void DrawProfessorObstacle(float x, float y, bool hit)
    {
        float w = 55f, h = 55f;
        // Flash red when hit
        if (hit)
            DrawRect(new Rect2(x - w/2f - 4, y - h/2f - 4, w + 8, h + 8), new Color(1f, 0.1f, 0.1f, 0.7f));
        if (professorTexture != null)
            DrawTextureRect(professorTexture, new Rect2(x - w/2f, y - h/2f, w, h), false);
        else
        {
            DrawCircle(new Vector2(x, y), 22f, new Color(0.85f, 0.7f, 0.55f, 1f));
            DrawString(ThemeDB.FallbackFont, new Vector2(x - 14, y + 4), "PROF", HorizontalAlignment.Left, -1, 11, new Color(1f,0.2f,0.2f,1f));
        }
        DrawString(ThemeDB.FallbackFont, new Vector2(x - 20, y - h/2f - 14), "⚠ PROF ⚠", HorizontalAlignment.Left, -1, 11, new Color(1f, 0.2f, 0.2f, 0.9f));
    }

    private void DrawResult()
    {
        DrawRect(new Rect2(0, 0, 1280, 720), new Color(0.12f, 0f, 0.25f, 1f));
        DrawRect(new Rect2(0, 0, 1280, 8), new Color(1f, 0.84f, 0f, 1f));
        DrawRect(new Rect2(0, 712, 1280, 8), new Color(1f, 0.84f, 0f, 1f));

        int winner = p1Time <= p2Time ? 1 : 2;

        DrawString(ThemeDB.FallbackFont, new Vector2(520, 70), "RACE RESULTS", HorizontalAlignment.Left, -1, 28, new Color(1f, 0.84f, 0f, 0.8f));
        DrawString(ThemeDB.FallbackFont, new Vector2(320, 180), string.Format("DANE {0} WINS THE RACE!", winner), HorizontalAlignment.Left, -1, 64, new Color(1f, 0.84f, 0f, 1f));
        DrawLine(new Vector2(200, 225), new Vector2(1080, 225), new Color(1f, 0.84f, 0f, 0.3f), 2f);

        DrawRect(new Rect2(240, 265, 300, 130), new Color(0.29f, 0f, 0.51f, 0.8f));
        DrawRect(new Rect2(240, 265, 300, 130), new Color(1f, 0.84f, 0f, 0.3f));
        DrawString(ThemeDB.FallbackFont, new Vector2(310, 310), "DANE 1", HorizontalAlignment.Left, -1, 24, new Color(1f,1f,1f,0.8f));
        DrawString(ThemeDB.FallbackFont, new Vector2(285, 368), string.Format("{0:F2}s", p1Time), HorizontalAlignment.Left, -1, 44, winner == 1 ? new Color(1f,0.84f,0f,1f) : new Color(1f,1f,1f,0.7f));

        DrawString(ThemeDB.FallbackFont, new Vector2(610, 345), "VS", HorizontalAlignment.Left, -1, 36, new Color(1f,1f,1f,0.5f));

        DrawRect(new Rect2(740, 265, 300, 130), new Color(0.29f, 0f, 0.51f, 0.8f));
        DrawRect(new Rect2(740, 265, 300, 130), new Color(1f, 0.84f, 0f, 0.3f));
        DrawString(ThemeDB.FallbackFont, new Vector2(810, 310), "DANE 2", HorizontalAlignment.Left, -1, 24, new Color(1f,1f,1f,0.8f));
        DrawString(ThemeDB.FallbackFont, new Vector2(785, 368), string.Format("{0:F2}s", p2Time), HorizontalAlignment.Left, -1, 44, winner == 2 ? new Color(1f,0.84f,0f,1f) : new Color(1f,1f,1f,0.7f));

        DrawRect(new Rect2(340, 465, 600, 60), new Color(1f, 0.84f, 0f, 0.15f));
        DrawString(ThemeDB.FallbackFont, new Vector2(365, 507), "Round 3: Professor Showdown!", HorizontalAlignment.Left, -1, 26, new Color(1f,1f,1f,0.95f));

        if (resultTimer > 1f && (int)(resultTimer * 2) % 2 == 0)
            DrawString(ThemeDB.FallbackFont, new Vector2(450, 615), "Press F or J to continue", HorizontalAlignment.Left, -1, 24, new Color(1f,0.84f,0f,0.9f));
    }
}

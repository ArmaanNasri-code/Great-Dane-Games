using Godot;

public partial class RoundResult : Node2D
{
    public override void _Ready()
    {
        if (GameManager.Instance != null)
        {
            int winner = GameManager.Instance.RoundWinner;
            if (winner == 1) GameManager.Instance.Player1Rounds++;
            else GameManager.Instance.Player2Rounds++;
            GameManager.Instance.CurrentRound++;
        }
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("p1_action") || Input.IsActionJustPressed("p2_action"))
        {
            if (GameManager.Instance != null)
            {
                int p1 = GameManager.Instance.Player1Rounds;
                int p2 = GameManager.Instance.Player2Rounds;
                int round = GameManager.Instance.CurrentRound;

                if (p1 >= 2 || p2 >= 2)
                    GetTree().ChangeSceneToFile("res://GameOver.tscn");
                else if (round == 2)
                    GetTree().ChangeSceneToFile("res://Racing.tscn");
                else
                    GetTree().ChangeSceneToFile("res://Sumo.tscn");
            }
        }
        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(0, 0, 1280, 720), new Color(0.29f, 0f, 0.51f, 1f));
        
        int winner = GameManager.Instance?.RoundWinner ?? 1;
        int p1 = GameManager.Instance?.Player1Rounds ?? 0;
        int p2 = GameManager.Instance?.Player2Rounds ?? 0;
        int round = GameManager.Instance?.CurrentRound ?? 2;

        DrawString(ThemeDB.FallbackFont, new Vector2(340, 200), $"DANE {winner} WINS ROUND!", HorizontalAlignment.Left, -1, 52, new Color(1f, 0.84f, 0f, 1f));
        DrawString(ThemeDB.FallbackFont, new Vector2(440, 320), $"Score: Dane 1 [{p1}] - [{p2}] Dane 2", HorizontalAlignment.Left, -1, 32, new Color(1f, 1f, 1f, 1f));

        string nextText = (p1 >= 2 || p2 >= 2) ? "Press F or J to see Final Results" : 
                          round == 2 ? "Press F or J for Round 2: Campus Race!" :
                          "Press F or J for Round 3: Professor Showdown!";
        DrawString(ThemeDB.FallbackFont, new Vector2(200, 480), nextText, HorizontalAlignment.Left, -1, 28, new Color(1f, 1f, 1f, 1f));
    }
}

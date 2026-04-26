using Godot;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }
    public int RoundWinner { get; set; } = 0;
    public int Player1Rounds { get; set; } = 0;
    public int Player2Rounds { get; set; } = 0;
    public int CurrentRound { get; set; } = 1;

    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            QueueFree();
        }
    }
}

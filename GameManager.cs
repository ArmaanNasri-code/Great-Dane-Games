using Godot;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }
    public int RoundWinner { get; set; } = 0;
    public int Player1Rounds { get; set; } = 0;
    public int Player2Rounds { get; set; } = 0;
    public int CurrentRound { get; set; } = 1;

    private AudioStreamPlayer _musicPlayer;

    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
            _musicPlayer = new AudioStreamPlayer();
            AddChild(_musicPlayer);
            PlayMusic();
        }
        else
        {
            QueueFree();
        }
    }

    public void PlayMusic()
    {
        if (_musicPlayer.Playing) return;
        var stream = GD.Load<AudioStream>("res://music.mp3");
        if (stream is AudioStreamMP3 mp3) mp3.Loop = true;
        _musicPlayer.Stream = stream;
        _musicPlayer.VolumeDb = -12f;
        _musicPlayer.Play();
    }

    public void StopMusic() => _musicPlayer.Stop();
}

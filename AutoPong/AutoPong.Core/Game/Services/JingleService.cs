namespace AutoPong.Core.Game.Services;

public sealed class JingleService
{
    private const int Speed = 7;

    private readonly SoundService _soundService;

    /// <summary>Used as a timeline to play notes.</summary>
    private int _jingleCounter;

    public JingleService(SoundService soundService) => _soundService = soundService;

    public void Initialize() => _jingleCounter = 0;

    public void Play()
    {
        _jingleCounter++;

        switch (_jingleCounter)
        {
            case Speed * 1:
                _soundService.PlayJingle1Sound();

                break;

            case Speed * 2:
                _soundService.PlayJingle2Sound();

                break;

            case Speed * 3:
                _soundService.PlayJingle3Sound();

                break;

            case Speed * 4:
                _soundService.PlayJingle4Sound();

                break;

            // Only play this jingle once.
            case > Speed * 4:
                _jingleCounter = int.MaxValue - 1;

                break;
        }
    }
}

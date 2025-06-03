using AutoPong.Core.Game.Enums;
using AutoPong.Core.Game.Providers;

namespace AutoPong.Core.Game.Services;

public sealed class SoundService
{
    private readonly AudioProvider _audioProvider;

    public SoundService(AudioProvider audioProvider) => _audioProvider = audioProvider;

    public void PlayBallHittingTheLeftOrRightWallSound() => _audioProvider.PlayWave(frequency: 440.0f, duration: 50, WaveTypes.Square, volume: 0.3f);

    public void PlayBallHittingAPaddleSound() => _audioProvider.PlayWave(frequency: 220.0f, duration: 50, WaveTypes.Sin, volume: 0.3f);

    public void PlayJingle1Sound() => _audioProvider.PlayWave(frequency: 440.0f, duration: 100, WaveTypes.Sin, volume: 0.2f);

    public void PlayJingle2Sound() => _audioProvider.PlayWave(frequency: 523.25f, duration: 100, WaveTypes.Sin, volume: 0.2f);

    public void PlayJingle3Sound() => _audioProvider.PlayWave(frequency: 659.25f, duration: 100, WaveTypes.Sin, volume: 0.2f);

    public void PlayJingle4Sound() => _audioProvider.PlayWave(frequency: 783.99f, duration: 100, WaveTypes.Sin, volume: 0.2f);
}

using System;

using AutoPong.Core.Game.Enums;

using Microsoft.Xna.Framework.Audio;

namespace AutoPong.Core.Game.Providers;

public sealed class AudioProvider
{
    private const int SampleRate = 48000;

    private readonly DynamicSoundEffectInstance _dynamicSoundEffectInstance;

    private byte[] _buffer;

    private int _bufferSize;

    private int _totalTime;

    public AudioProvider()
    {
        _dynamicSoundEffectInstance          = new DynamicSoundEffectInstance(SampleRate, AudioChannels.Mono);
        _bufferSize                          = _dynamicSoundEffectInstance.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(value: 500));
        _buffer                              = new byte[_bufferSize];
        _dynamicSoundEffectInstance.Volume   = 0.4f;
        _dynamicSoundEffectInstance.IsLooped = false;
    }

    public void PlayWave(double frequency, short duration, WaveTypes waveTypes, float volume)
    {
        _dynamicSoundEffectInstance.Stop();

        _bufferSize = _dynamicSoundEffectInstance.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(duration));
        _buffer     = new byte[_bufferSize];

        int size = _bufferSize - 1;

        for (int index = 0; index < size; index += 2)
        {
            double time = _totalTime / (double)SampleRate;

            short currentSample = waveTypes switch
            {
                WaveTypes.Sin    => (short)(Math.Sin(2 * Math.PI * frequency * time)                * short.MaxValue         * volume),
                WaveTypes.Tan    => (short)(Math.Tan(2 * Math.PI * frequency * time)                * short.MaxValue         * volume),
                WaveTypes.Square => (short)(Math.Sign(Math.Sin(2 * Math.PI * frequency * time))     * (double)short.MaxValue * volume),
                WaveTypes.Noise  => (short)(AutoPongGame.Rand.Next(-short.MaxValue, short.MaxValue) * volume),
                var _            => 0
            };

            _buffer[index]     =  (byte)(currentSample & 0xFF);
            _buffer[index + 1] =  (byte)(currentSample >> 8);
            _totalTime         += 2;
        }

        _dynamicSoundEffectInstance.SubmitBuffer(_buffer);
        _dynamicSoundEffectInstance.Play();
    }
}

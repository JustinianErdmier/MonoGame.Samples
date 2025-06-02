#region Using Statements

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

#endregion

namespace ShipGame.Core.Game;

public class SoundManager : IDisposable
{
    private ContentManager content; // content manager

    private readonly string[] soundAssets =
    {
        "fire_primary",
        "fire_secondary",
        "menu_cancel",
        "menu_change",
        "menu_select",
        "missile_explode",
        "powerup_get",
        "powerup_spawn",
        "shield_activate",
        "shield_collide",
        "ship_boost",
        "ship_collide",
        "ship_explode",
        "ship_spawn"
    };

    private readonly Dictionary<string, SoundEffect> sounds = new(); // list of sound effects

    /// <summary>Create a new sound manager</summary>
    public SoundManager()
    { }

    /// <summary>Load resources</summary>
    public void LoadContent(ContentManager content)
    {
        this.content = content;

        foreach (string asset in soundAssets)
        {
            sounds.Add(asset, content.Load<SoundEffect>($"sounds/{asset}"));
        }
    }

    /// <summary>Free resources</summary>
    public void UnloadContent()
    {
        foreach (string asset in soundAssets)
        {
            content.UnloadAsset($"sounds/{asset}");
        }

        foreach (SoundEffect sound in sounds.Values)
        {
            content.UnloadAsset(sound.Name);
            sound.Dispose();
        }

        sounds.Clear();
    }

    /// <summary>Play a sound in 2D</summary>
    public void PlaySound(string soundName)
    {
        if (sounds.TryGetValue(soundName, out SoundEffect soundEffect))
        {
            soundEffect.Play();
        }
    }

    /// <summary>Play a sound in 3D at given position (just fake 3D using distance attenuation but no stereo)</summary>
    public void PlaySound3D(string soundName, float distance)
    {
        if (sounds.TryGetValue(soundName, out SoundEffect soundEffect))
        {
            float volume = Math.Max(val1: 0.0f, 1.0f - distance / 1000.0f);
            soundEffect.Play(volume, pitch: 0.0f, pan: 0.0f);
        }
    }

    #region IDisposable Members

    public bool IsDisposed { get; } = false;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing && !IsDisposed)
        {
            if (content != null)
            {
                content.Dispose();
                content = null;
            }
        }
    }

    #endregion
}

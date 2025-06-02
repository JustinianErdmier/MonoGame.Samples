#region Using Statements

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace ShipGame.Core.Game.Graphics;

public class Powerup
{
    private Matrix bobbing; // powerup bobbing transform

    private float elapsedTime; // elapsed time since spawned

    private readonly Model model; // powerup model

    private readonly PowerupType powerupType; // powerup type

    private Matrix transform; // powerup position and orientation

    private float waitTime; // time to wait until respawn 

    // (zero when powerup is pickable)

    /// <summary>Create a new powerup</summary>
    public Powerup(PowerupType type,
                   Matrix      transform,
                   Model       model)
    {
        powerupType    = type;
        this.transform = transform;
        this.model     = model;
    }

    /// <summary>Update powerup for given elapsed time</summary>
    public bool Update(GameManager game, float elapsedTime)
    {
        if (game == null)
        {
            throw new ArgumentNullException(paramName: "game");
        }

        // add elapsed time for this frame
        this.elapsedTime += elapsedTime;

        // if waiting to respawn
        if (waitTime > 0)
        {
            // decrease wait time
            waitTime = Math.Max(val1: 0.0f, waitTime - elapsedTime);

            // if wait time is finished
            if (waitTime == 0)
            {
                // add powerup spawn animated sprite
                game.AddAnimSprite(AnimSpriteType.Spawn,
                                   transform.Translation,
                                   radius: 50,
                                   viewOffset: 40,
                                   frameRate: 30,
                                   DrawMode.Additive,
                                   player: -1);

                // play powerup spawn sound
                game.PlaySound3D(soundName: "powerup_spawn", transform.Translation);
            }

            // return true to keep powerup alive
            return true;
        }

        // calculate bobbing angles
        float turn_angle = this.elapsedTime * GameOptions.PowerupTurnSpeed % MathHelper.TwoPi;
        float move_angle = this.elapsedTime * GameOptions.PowerupMoveSpeed % MathHelper.TwoPi;

        // create bobbing matrix
        bobbing = Matrix.CreateRotationY(turn_angle) * Matrix.CreateTranslation((float)Math.Cos(move_angle) * GameOptions.PowerupMoveDistance * Vector3.Up);

        // check for any player at the powerup location
        int playerHit = game.GetPlayerAtPosition(transform.Translation);

        if (playerHit != -1)
        {
            // disable powerup until respawn time
            waitTime = GameOptions.PowerupRespawnTime;

            // get player at powerup location
            PlayerShip p = game.GetPlayer(playerHit);

            switch (powerupType)
            {
                case PowerupType.Energy:
                    p.AddEnergy(value: 0.5f); // add 50% energy

                    break;

                case PowerupType.Missile:
                    p.AddMissile(value: 3); // add 3 missiles

                    break;
            }

            // add powerup spawn animates sprite
            game.AddAnimSprite(AnimSpriteType.Spawn,
                               transform.Translation,
                               radius: 40,
                               viewOffset: 40,
                               frameRate: 30,
                               DrawMode.Additive,
                               player: -1);

            // play powerup get sound
            game.PlaySound(soundName: "powerup_get");
        }

        // return true to keep powerup alive
        return true;
    }

    /// <summary>Draw powerup</summary>
    public void Draw(GameManager     game,
                     GraphicsDevice  gd,
                     RenderTechnique technique,
                     Vector3         cameraPosition,
                     Matrix          viewProjection,
                     LightList       lights)
    {
        if (game == null)
        {
            throw new ArgumentNullException(paramName: "game");
        }

        // if now waiting to respawn
        if (waitTime == 0)
        {
            // draw powerup model
            game.DrawModel(gd,
                           model,
                           technique,
                           cameraPosition,
                           bobbing * transform,
                           viewProjection,
                           lights);
        }
    }
}

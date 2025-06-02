#region Using Statements

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace ShipGame.Core.Game.Graphics;

public class Projectile
{
    private AnimSpriteType animatedSprite; // animated sprite to play when projectile hits

    private DrawMode animatedSpriteDrawMode; // animated sprite drawing mode

    private float animatedSpriteFrameRate; // framerate for explosion animated sprite

    private float animatedSpriteSize; // size of explosion animated sprite

    private readonly float contactDamage; // contact damage if projectile hits a player

    private readonly Vector3 destinationPosition; // destination position

    private float elapsedTime; // elapsed time since created

    private float explosionDamage; // splash damage for explosion

    private float explosionDamageRadius; // splash damage radius

    private string explosionSound; // explosion sound

    private readonly Model model; // projectile model

    private readonly int player; // player owning the projectile

    private readonly ProjectileType projectileType; // projectile type

    private readonly Vector3 sourcePosition; // source position

    private ParticleSystem system; // particle system used for projectile trail

    private Matrix systemTransform; // local transform to position particle system

    private readonly RenderTechnique technique; // render technique

    private readonly float totalTime; // total time to reach destination

    private Matrix transform; // current projectile transform matrix

    /// <summary>Create a new projectile</summary>
    public Projectile(ProjectileType  type,
                      Model           model,
                      int             player,
                      float           velocity,
                      float           damage,
                      Matrix          source,
                      Vector3         destination,
                      RenderTechnique technique)
    {
        projectileType = type;
        this.model     = model;
        this.player    = player;

        sourcePosition      = source.Translation;
        destinationPosition = destination;

        this.technique = technique;

        contactDamage = damage;

        elapsedTime = 0;
        totalTime   = (source.Translation - destination).Length() / velocity;
        transform   = source;
    }

    /// <summary>Set projectile explosion parameters</summary>
    public void SetExplosion(AnimSpriteType sprite,
                             float          size,
                             float          frameRate,
                             DrawMode       mode,
                             float          damage,
                             float          damageRadius,
                             string         sound)
    {
        animatedSprite          = sprite;
        animatedSpriteSize      = size;
        animatedSpriteFrameRate = frameRate;
        animatedSpriteDrawMode  = mode;
        explosionDamage         = damage;
        explosionDamageRadius   = damageRadius;
        explosionSound          = sound;
    }

    /// <summary>Set projectile trail parameters</summary>
    public void SetTrail(ParticleSystem trail, Matrix transform)
    {
        system          = trail;
        systemTransform = transform;
    }

    /// <summary>Update projectile</summary>
    public bool Update(float elapsedTime, GameManager game)
    {
        if (game == null)
        {
            throw new ArgumentNullException(paramName: "game");
        }

        // add elapsed time for this frame
        this.elapsedTime += elapsedTime;

        // normalize time
        float normalizedTime = Math.Min(val1: 1.0f, this.elapsedTime / totalTime);

        // compute current projectile position
        Vector3 position = Vector3.Lerp(sourcePosition,
                                        destinationPosition,
                                        normalizedTime);

        // set postion into projectile transform matrix
        transform.Translation = position;

        // if projectile includes a particle system update its position
        if (system != null)
        {
            system.SetTransform(systemTransform * transform);
        }

        // check if projectile hit any player
        int playerHit = game.GetPlayerAtPosition(position);

        // if a player is hit or reached destination explode projectile
        if ((playerHit != player && playerHit != -1)
            || normalizedTime == 1.0f)
        {
            // compute explosion position moving hit point into hit normal direction
            Vector3 explosionPosition =
                position + 0.5f * animatedSpriteSize * transform.Backward;

            // set transform to explosion position
            transform.Translation = explosionPosition;

            // if an animated sprite explosion is available, create it
            if (animatedSpriteSize > 0)
            {
                game.AddAnimSprite(animatedSprite,
                                   explosionPosition,
                                   animatedSpriteSize,
                                   viewOffset: 10.0f,
                                   animatedSpriteFrameRate,
                                   animatedSpriteDrawMode,
                                   player: -1);
            }

            // if splash damage is available, apply splash damage to nearby players
            if (explosionDamageRadius > 0)
            {
                game.AddDamageSplash(player,
                                     explosionDamage,
                                     explosionPosition,
                                     explosionDamageRadius);
            }

            // if exploded on a player add contact damage
            if (playerHit != -1
                && game.GetPlayer(playerHit).IsAlive)
            {
                game.AddDamage(player,
                               playerHit,
                               contactDamage,
                               Vector3.Normalize(destinationPosition - sourcePosition));
            }

            // if explosion sound is available, play it
            if (explosionSound != null)
            {
                game.PlaySound3D(explosionSound, explosionPosition);
            }

            // add explosion particle system
            if (projectileType == ProjectileType.Missile)
            {
                game.AddParticleSystem(ParticleSystemType.MissileExplode,
                                       transform);
            }
            else if (projectileType == ProjectileType.Blaster)
            {
                game.AddParticleSystem(ParticleSystemType.BlasterExplode,
                                       transform);
            }

            // kill trail particle system
            if (system != null)
            {
                system.SetTotalTime(totalTime: -1e10f);
            }

            // return false to kill the projectile
            return false;
        }

        // return true to keep projectile alive
        return true;
    }

    /// <summary>Draw projectile</summary>
    public void Draw(GameManager     game,
                     GraphicsDevice  gd,
                     RenderTechnique defaultTechnique,
                     Vector3         cameraPosition,
                     Matrix          viewProjection,
                     LightList       lights)
    {
        if (game == null)
        {
            throw new ArgumentNullException(paramName: "game");
        }

        if (technique == RenderTechnique.ViewMapping)
        {
            game.DrawModel(gd,
                           model,
                           technique,
                           cameraPosition,
                           transform,
                           viewProjection,
                           lights: null);
        }
        else
        {
            game.DrawModel(gd,
                           model,
                           defaultTechnique,
                           cameraPosition,
                           transform,
                           viewProjection,
                           lights);
        }
    }
}

#region Using Statements

using System;

using BoxCollider;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ShipGame.Core.Game.Graphics;

#endregion

namespace ShipGame.Core.Game;

public class PlayerShip : CollisionTreeElemDynamic, IDisposable
{
    private readonly Matrix boostTransform; // current transform for engine position

    private readonly ChaseCamera chaseCamera; // chase camera object 

    private readonly GameManager gameManager; // the game manager

    // (blaster, missile, engine locations)

    private readonly PlayerMovement movement; // player movement 

    private readonly ParticleSystem particleBoost; // engine particle system

    private readonly int playerIndex; // the player index for this ship

    private readonly Random random; // random generator

    private readonly EntityList shipEntities; // player ship model entities 

    private readonly Model shipModel; // player ship model

    // view target offset for 1st person camera
    private readonly Matrix viewOffset = Matrix.CreateTranslation(GameOptions.CameraViewOffset);

    // shield animated sprite (not null when shild is active)
    private AnimSprite animatedSpriteShield;

    private float blaster; // blaster charge (1.0 when ready to fire)

    private Matrix bobbing = Matrix.Identity; // bobbing matrix

    private Matrix bobbingInverse = Matrix.Identity; // inverse of bobbing matrix

    private float bobbingTime; // current time for ship bobbing

    private float boost = 1.0f; // curren boost charge (1.0 when ready to use)

    private bool boostUse; // boost is active flag

    // (handles control forces and collision response)
    private bool collisionSound; // collision sound ready to play (use to disable 

    // (0.0 for no damage screen)
    private Vector4 damageColor; // current damage screen color

    private float damageTime; // time left showing damage screen 

    private float deadTime = 0.4f; // time left before ship respawn after death

    private float energy = 1.0f; // energy charge (0.0 when ship is destroyed)

    private float missile; // missile charge (1.0 when ready to fire)

    private float shield = 1.0f; // current shield charge (1.0 when ready to use)

    private bool shieldUse; // shield is active flag

    // multiple collision sounds when sliding through walls)

    private Matrix transform; // the player transform matrix (position/rotation)

    private Matrix transformInverse; // inverse of player transform matrix

    /// <summary>Create a new player ship</summary>
    public PlayerShip(GameManager game,     // game manager
                      int         player,   // player id
                      Model       model,    // model for player ship
                      EntityList  entities, // entity list for ship model
                      float       radius) // collision box radius
    {
        if (game == null)
        {
            throw new ArgumentNullException(paramName: "game");
        }

        if (entities == null)
        {
            throw new ArgumentNullException(paramName: "entities");
        }

        // save parameters
        gameManager  = game;
        shipModel    = model;
        shipEntities = entities;
        playerIndex  = player;

        // create movement controller
        movement             = new PlayerMovement();
        movement.maxVelocity = GameOptions.MovementVelocity;

        // enable collision sound
        collisionSound = true;

        // create engine particle system with infinite life time
        particleBoost = gameManager.AddParticleSystem(ParticleSystemType.ShipTrail,
                                                      transform);

        particleBoost.SetTotalTime(totalTime: 1e10f);
        boostTransform = shipEntities.GetTransform(name: "engine");

        // create the ship collision box
        box = new CollisionBox(-radius, radius);

        // random bobbing offset
        random      = new Random(player);
        bobbingTime = (float)random.NextDouble();

        // setup 3rd person camera parameters
        Camera3rdPerson                   = false;
        chaseCamera                       = new ChaseCamera();
        chaseCamera.DesiredPositionOffset = GameOptions.CameraOffset;
        chaseCamera.LookAtOffset          = GameOptions.CameraTargetOffset;
        chaseCamera.Stiffness             = GameOptions.CameraStiffness;
        chaseCamera.Damping               = GameOptions.CameraDamping;
        chaseCamera.Mass                  = GameOptions.CameraMass;
    }

    /// <summary>Returns true if player is currently alive</summary>
    public bool IsAlive => deadTime == 0.0f;

    /// <summary>Returns player total points</summary>
    public int Score { get; set; }

    /// <summary>Get the hud bars values (energy, shield and boost) as a Vector3</summary>
    public Vector3 Bars => new(energy, shield, boost);

    /// <summary>Renturns the number of missiles available</summary>
    public int MissileCount { get; private set; }

    /// <summary>Return color to be used by damage screen</summary>
    public Color DamageColor
    {
        get
        {
            damageColor.W = damageTime / GameOptions.DamageFadeout;

            return new Color(damageColor);
        }
    }

    /// <summary>Get current camera positon in world space</summary>
    public Vector3 CameraPosition
    {
        get
        {
            // if in 3rd person mode
            if (Camera3rdPerson)
            {
                // return chase camera position
                return chaseCamera.Position;
            }

            // return player position
            return Position;
        }
    }

    /// <summary>Get current player position in world space</summary>
    public Vector3 Position
        =>

            // return player position including bobbing
            (bobbing * transform).Translation;

    /// <summary>Get the camera view matrix</summary>
    public Matrix ViewMatrix
    {
        get
        {
            // if in 3rd person mode
            if (Camera3rdPerson)
            {
                // return chase camera view matrix
                return chaseCamera.View;
            }

            // return player view matrix including bobing and view offset
            return transformInverse * bobbingInverse * viewOffset;
        }
    }

    /// <summary>Get current player transform matrix (world matrix)</summary>
    public Matrix Transform => bobbing * transform;

    /// <summary>Get camera up vector</summary>
    public Vector3 ViewUp
    {
        get
        {
            // if 3rd person mode
            if (Camera3rdPerson)
            {
                // return chase camera up vector
                return chaseCamera.View.Up;
            }

            // return player up vector
            return transform.Up;
        }
    }

    /// <summary>True if camera in 3rd person mode</summary>
    public bool Camera3rdPerson { get; private set; }

    /// <summary>Adds energy to ship (positive for adding and negative for subtracting)</summary>
    public void AddEnergy(float value)
    {
        // if shield active and damaging energy
        if (value < 0 && shieldUse)
        {
            // play shield collide sound and return (no damage)
            gameManager.PlaySound(soundName: "shield_collide");

            return;
        }

        // apply value to energy 
        energy = Math.Max(val1: 0.0f, Math.Min(val1: 1.0f, energy + value));

        // if reducing energy, add damage screen intensity and timeout
        if (value < 0)
        {
            float intensity = damageTime / GameOptions.DamageFadeout;
            damageColor = damageColor * intensity + Vector4.UnitX;
            damageTime  = GameOptions.DamageFadeout;
        }

        // if no more energy, kill player
        if (energy == 0.0f)
        {
            deadTime = GameOptions.DeathTimeout;
        }
    }

    /// <summary>Adds missiles to ship (positive for adding and negative for subtracting)</summary>
    public void AddMissile(int value)
    {
        MissileCount = Math.Max(val1: 0, Math.Min(val1: 9, MissileCount + value));
    }

    /// <summary>Adds a impulse force to player (used to push player on explosion and player to player collision)</summary>
    public void AddImpulseForce(Vector3 force)
    {
        movement.velocity.X += Vector3.Dot(movement.rotation.Right, force);
        movement.velocity.Y += Vector3.Dot(movement.rotation.Up, force);
        movement.velocity.Z += Vector3.Dot(movement.rotation.Forward, force);
    }

    /// <summary>Fire a projectile from ship with given type and velocity</summary>
    public void FireProjectile(ProjectileType projectile, float velocity)
    {
        switch (projectile)
        {
            case ProjectileType.Blaster:
            {
                // fire left blaster
                Matrix m = shipEntities.GetTransform(name: "blaster_left") * bobbing * transform;

                Projectile p = gameManager.AddProjectile(projectile,
                                                         playerIndex,
                                                         m,
                                                         velocity,
                                                         damage: 0.1f,
                                                         RenderTechnique.ViewMapping);

                p.SetExplosion(AnimSpriteType.Blaster,
                               size: 30,
                               frameRate: 30,
                               DrawMode.AdditiveAndGlow,
                               damage: 0,
                               damageRadius: 0,
                               sound: null);

                // fire right blaster
                m = shipEntities.GetTransform(name: "blaster_right") * bobbing * transform;

                p = gameManager.AddProjectile(projectile,
                                              playerIndex,
                                              m,
                                              velocity,
                                              damage: 0.1f,
                                              RenderTechnique.ViewMapping);

                p.SetExplosion(AnimSpriteType.Blaster,
                               size: 30,
                               frameRate: 30,
                               DrawMode.AdditiveAndGlow,
                               damage: 0,
                               damageRadius: 0,
                               sound: null);

                // play blaster fire sound
                gameManager.PlaySound(soundName: "fire_primary");
            }

                break;

            case ProjectileType.Missile:
            {
                // fire missile
                Matrix m = shipEntities.GetTransform(name: "missile") * bobbing * transform;

                Projectile p = gameManager.AddProjectile(projectile,
                                                         playerIndex,
                                                         m,
                                                         velocity,
                                                         damage: 0.1f,
                                                         RenderTechnique.NormalMapping);

                p.SetExplosion(AnimSpriteType.Missile,
                               size: 90,
                               frameRate: 30,
                               DrawMode.AdditiveAndGlow,
                               damage: 0.5f,
                               damageRadius: 500,
                               sound: "missile_explode");

                // set missile trail
                ParticleSystem Trail = gameManager.AddParticleSystem(ParticleSystemType.MissileTrail, Matrix.Identity);

                p.SetTrail(Trail,
                           Matrix.CreateTranslation(GameOptions.MissileTrailOffset));

                // play missile fire sound
                gameManager.PlaySound(soundName: "fire_secondary");
            }

                break;
        }
    }

    /// <summary>Process input for player ship (movement and weapons)</summary>
    public void ProcessInput(float elapsedTime, InputManager input, int player)
    {
        if (input == null)
        {
            throw new ArgumentNullException(paramName: "input");
        }

        // if dead, don't process input for player
        if (IsAlive == false)
        {
            return;
        }

        // process movement related inputs
        movement.ProcessInput(elapsedTime, input.CurrentState, player);

        // if player invert Y is enabled, invert X rotation force
        if (gameManager.GetInvertY(player))
        {
            movement.rotationForce.X = -movement.rotationForce.X;
        }

        // if blaster ready and input activated
        if (blaster == 1)
        {
            if (input.IsKeyPressed(player, Keys.Space)
                || input.CurrentState.padState[player].Triggers.Right > 0)
            {
                // fire blaster
                FireProjectile(ProjectileType.Blaster, GameOptions.BlasterVelocity);

                // reset charge time
                blaster = 0;
            }
        }

        // if missile is ready and input activated
        if (missile         == 1
            && MissileCount > 0)
        {
            if (input.IsKeyPressed(player, Keys.Enter)
                || input.IsTriggerPressedLeft(player))
            {
                // fire missile
                FireProjectile(ProjectileType.Missile, GameOptions.MissileVelocity);

                // subtract missile count
                AddMissile(value: -1);

                // reset charge time
                missile = 0;
            }
        }

        // if shield is ready and input 
        if (shield == 1)
        {
            if (input.IsKeyPressed(player, Keys.R)
                || input.IsButtonPressedA(player))
            {
                // activate shield
                shieldUse = true;

                // create animated sprite
                animatedSpriteShield =
                    gameManager.AddAnimSprite(AnimSpriteType.Shield,
                                              transform.Translation + transform.Forward * 10,
                                              radius: 160,
                                              viewOffset: 80,
                                              frameRate: 15,
                                              DrawMode.Additive,
                                              player);

                // play shield sound
                gameManager.PlaySound(soundName: "shield_activate");
            }
        }

        // if boost ready and input activated
        if (boost == 1)
        {
            if (input.IsKeyPressed(player, Keys.LeftShift)
                || input.IsKeyPressed(player, Keys.RightShift)
                || input.IsButtonPressedY(player)
                || input.IsButtonPressedLeftStick(player))
            {
                // activate boost
                boostUse = true;

                // play boost sound
                gameManager.PlaySound(soundName: "ship_boost");
            }
        }

        // if camara switch input activated
        if (input.IsKeyPressed(player, Keys.Back)
            || input.IsButtonPressedB(player))
        {
            // switch 3rd person mode
            Camera3rdPerson = !Camera3rdPerson;

            // reset camera
            chaseCamera.Reset();
        }
    }

    /// <summary>Reset player ship to given position and rotation and reset chase camera</summary>
    public void Reset(Matrix newTransform)
    {
        // reset movement to new transform
        movement.Reset(newTransform);

        // store new transform and its inverse
        transform        = newTransform;
        transformInverse = Matrix.Invert(newTransform);

        // reset chase camera
        chaseCamera.ChasePosition  = transform.Translation;
        chaseCamera.ChaseDirection = transform.Forward;
        chaseCamera.Up             = transform.Up;
        chaseCamera.Reset();
    }

    /// <summary>Updates the player ship for given elapsed time</summary>
    public void Update(float          elapsedTime, // elapsed time on this frame
                       CollisionMesh? collision,   // level collision mesh
                       EntityList     entities) // level spawn points
    {
        if (collision == null)
        {
            throw new ArgumentNullException(paramName: "collision");
        }

        if (entities == null)
        {
            throw new ArgumentNullException(paramName: "entities");
        }

        // updates damage screen time (zero for no damage indication)
        damageTime = Math.Max(val1: 0.0f, damageTime - elapsedTime);

        // if player dead
        if (IsAlive == false)
        {
            // disable engine particle system
            particleBoost.Enabled = false;

            // updates dead time (if zero, player is alive)
            deadTime = Math.Max(val1: 0.0f, deadTime - elapsedTime);

            // if player dead time expires, respawn
            if (IsAlive)
            {
                // reset player to a random spawn point
                Reset(entities.GetTransformRandom(random));

                // add spawn animated sprite in front of player
                Vector3 Pos = movement.position + 10 * movement.rotation.Forward;

                gameManager.AddAnimSprite(AnimSpriteType.Spawn,
                                          Pos,
                                          radius: 140,
                                          viewOffset: 80,
                                          frameRate: 30,
                                          DrawMode.Additive,
                                          playerIndex);

                // play spawn sound
                gameManager.PlaySound(soundName: "ship_spawn");

                // reset energy, shield and boost
                energy       = 1.0f;
                shield       = 1.0f;
                boost        = 1.0f;
                MissileCount = 3;
            }

            return;
        }

        // hold position before movement
        Vector3 lastPostion = movement.position;

        // update movement
        movement.Update(elapsedTime);

        // test for collision with level
        Vector3 collisionPosition;

        if (collision.BoxMove(box,
                              lastPostion,
                              movement.position,
                              frictionFactor: 1.0f,
                              bumpFactor: 0.0f,
                              recurseLevel: 3,
                              out collisionPosition))
        {
            // update to valid position after collision
            movement.position = collisionPosition;

            // compute new velocity after collision
            Vector3 newVelocity =
                (collisionPosition - lastPostion) * (1.0f / elapsedTime);

            // if collision sound enabled
            if (collisionSound)
            {
                // test collision angle to play collision sound 
                Vector3 WorldVel = movement.WorldVelocity;
                float   dot      = Vector3.Dot(Vector3.Normalize(WorldVel), Vector3.Normalize(newVelocity));

                if (dot < 0.7071f)
                {
                    // play collision sound
                    gameManager.PlaySound(soundName: "ship_collide");

                    // set rumble intensity
                    dot = 1 - 0.5f * (dot + 1);
                    gameManager.SetVibration(playerIndex, dot * 0.5f);

                    // disable collision sounds until ship stops colliding
                    collisionSound = false;
                }
            }

            // set new velocity after collision
            movement.WorldVelocity = newVelocity;
        }
        else

            // clear of collisions, re-enable collision sounds
        {
            collisionSound = true;
        }

        // update player transform
        transform             = movement.rotation;
        transform.Translation = movement.position;

        // compute inverse transform
        transformInverse = Matrix.Invert(transform);

        // get normalized player velocity
        float velocityFactor = movement.VelocityFactor;

        // update bobbing
        bobbingTime += elapsedTime;
        float bobbingFactor = 1.0f - velocityFactor;
        float time          = GameOptions.ShipBobbingSpeed * bobbingTime % (2 * MathHelper.TwoPi);
        float distance      = bobbingFactor                              * GameOptions.ShipBobbingRange;
        bobbing.M41        = distance * (float)Math.Sin(time * 0.5f);
        bobbing.M42        = distance * (float)Math.Sin(time);
        bobbingInverse.M41 = -bobbing.M41;
        bobbingInverse.M42 = -bobbing.M42;

        // compute transform with bobbing
        Matrix bobbingTransform = bobbing * transform;

        // update particle system position
        particleBoost.Enabled = true;
        particleBoost.SetTransform(boostTransform * bobbingTransform);

        // if shield active
        if (shieldUse)
        {
            // update shield position
            animatedSpriteShield.Position = bobbingTransform.Translation + 10f * bobbingTransform.Forward;

            // update shiled charge
            shield -= elapsedTime / GameOptions.ShieldUse;

            // if shield charge depleted
            if (shield < 0)
            {
                // disable shield
                shieldUse = false;
                shield    = 0;

                // kill shield animated sprite
                animatedSpriteShield.SetTotalTime(totalTime: 0);
                animatedSpriteShield = null;
            }
        }
        else

            // change shield
        {
            shield = Math.Min(val1: 1.0f,
                              shield + elapsedTime / GameOptions.ShieldRecharge);
        }

        // if boost active
        if (boostUse)
        {
            // increase ship maximum velocity
            movement.maxVelocity = GameOptions.MovementVelocityBoost;

            // apply impulse force forward
            AddImpulseForce(transform.Forward * GameOptions.BoostForce);

            // set particle system velocity scale
            particleBoost.VelocityScale = Math.Min(val1: 1.0f,
                                                   particleBoost.VelocityScale + 4.0f * elapsedTime);

            // update shield charge
            boost -= elapsedTime / GameOptions.BoostUse;

            // if  boost depleated
            if (boost < 0)
            {
                // disable boost
                boostUse = false;
                boost    = 0;
            }
        }
        else
        {
            // slowly returns ship maximum velocity to normal levels
            if (movement.maxVelocity > GameOptions.MovementVelocity)
            {
                movement.maxVelocity -= GameOptions.BoostSlowdown * elapsedTime;
            }

            // slowly returns particle system velocity scale to normal levels
            particleBoost.VelocityScale = Math.Max(val1: 0.1f,
                                                   particleBoost.VelocityScale - 2.0f * elapsedTime);

            // charge boost
            boost = Math.Min(val1: 1.0f,
                             boost + elapsedTime / GameOptions.BoostRecharge);
        }

        // charge blaster
        blaster = Math.Min(val1: 1.0f,
                           blaster + elapsedTime / GameOptions.BlasterChargeTime);

        // charge missile
        missile = Math.Min(val1: 1.0f,
                           missile + elapsedTime / GameOptions.MissileChargeTime);

        // update chase camera 
        chaseCamera.ChasePosition  = transform.Translation;
        chaseCamera.ChaseDirection = transform.Forward;
        chaseCamera.Up             = transform.Up;
        chaseCamera.Update(elapsedTime, collision);
    }

    /// <summary>Renders the player ship model and blaster and missile if available and charged</summary>
    public void Draw(GraphicsDevice  gd,
                     RenderTechnique technique,
                     Vector3         cameraPosition,
                     Matrix          viewProjection,
                     LightList       lights)
    {
        // if not dead
        if (deadTime == 0.0f)
        {
            // render ship model
            gameManager.DrawModel(gd,
                                  shipModel,
                                  technique,
                                  cameraPosition,
                                  bobbing * transform,
                                  viewProjection,
                                  lights);
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
        particleBoost.SetTotalTime(totalTime: 0);

        if (disposing && !IsDisposed)
        {
            if (box != null)
            {
                box.Dispose();
                box = null;
            }
        }
    }

    #endregion
}

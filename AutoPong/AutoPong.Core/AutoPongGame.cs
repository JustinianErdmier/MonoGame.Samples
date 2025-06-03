using System;

using AutoPong.Core.Game.Core;
using AutoPong.Core.Game.Entities;
using AutoPong.Core.Game.Enums;
using AutoPong.Core.Game.Providers;
using AutoPong.Core.Game.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AutoPong.Core;

public sealed class AutoPongGame : Microsoft.Xna.Framework.Game
{
    public static readonly Random Rand = new();

    private readonly Ball _ball;

    private readonly GraphicsDeviceManager _graphics;

    private readonly JingleService _jingleService;

    private readonly Paddle _paddleLeft;

    private readonly Paddle _paddleRight;

    private DrawingService? _drawingService;

    private Texture2D? _texture;

    public AutoPongGame()
    {
        _graphics = new GraphicsDeviceManager(this);

        _graphics.PreferredBackBufferWidth  = GameOptions.WindowResolution.X;
        _graphics.PreferredBackBufferHeight = GameOptions.WindowResolution.Y;

        SoundService soundService = new(new AudioProvider());

        _jingleService = new JingleService(soundService);

        _paddleLeft  = new Paddle(this, PaddleLocations.Left);
        _paddleRight = new Paddle(this, PaddleLocations.Right);

        _ball = new Ball(_paddleLeft, _paddleRight, soundService);

        IsMouseVisible = GameOptions.IsMouseVisible;
    }

    protected override void Initialize()
    {
        InitializeTexture();
        InitializeServices();
        InitializeEntities();
    }

    protected override void Update(GameTime gameTime)
    {
        if (!OperatingSystem.IsIOS()
            && (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape)))
        {
            Exit();
        }

        _ball.Update();

        _paddleLeft.Update(_ball);

        _paddleRight.Update(_ball);

        _jingleService.Play();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        if (_drawingService is null)
        {
            throw new Exception(message: "Drawing service must be initialized before drawing can occur.");
        }

        _drawingService.Clear()
                       .Begin()
                       .Draw.CenterLine()
                       .Draw.Paddles(_paddleLeft, _paddleRight)
                       .Draw.Ball(_ball)
                       .Draw.ScorePoints(_paddleLeft, _paddleRight)
                       .End();

        base.Draw(gameTime);
    }

    /// <summary>Creates the <see cref="_texture" /> with which to draw if it does not exist.</summary>
    private void InitializeTexture()
    {
        if (_texture is not null)
        {
            return;
        }

        _texture = new Texture2D(_graphics.GraphicsDevice, width: 1, height: 1);

        _texture.SetData([GameOptions.Colors.Texture]);
    }

    private void InitializeEntities()
    {
        _ball.Initialize();
        _paddleLeft.Initialize();
        _paddleRight.Initialize();
    }

    private void InitializeServices()
    {
        if (_texture is null)
        {
            throw new Exception(message: "Texture must be initialized before services can be initialized.");
        }

        _drawingService ??= new DrawingService(GraphicsDevice, new SpriteBatch(GraphicsDevice), _texture);

        _jingleService.Initialize();
    }

    public void Reinitialize()
    {
        InitializeTexture();
        InitializeServices();
        InitializeEntities();
    }
}

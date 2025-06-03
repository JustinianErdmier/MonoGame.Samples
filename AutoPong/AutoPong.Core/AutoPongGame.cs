using System;
using System.Collections.Generic;

using AutoPong.Core.Game;
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

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly GraphicsDeviceManager _graphicsDeviceManager;

    private readonly InputState _inputState = new();

    private readonly JingleService _jingleService;

    private readonly List<Paddle> _paddles;

    private DrawingService? _drawingService;

    private bool _isPaused = GameOptions.IsPausedByDefault;

    public AutoPongGame()
    {
        _graphicsDeviceManager = new GraphicsDeviceManager(this);

        _graphicsDeviceManager.PreferredBackBufferWidth  = GameOptions.WindowResolution.X;
        _graphicsDeviceManager.PreferredBackBufferHeight = GameOptions.WindowResolution.Y;

        SoundService soundService = new(new AudioProvider());

        _jingleService = new JingleService(soundService);

        _paddles = new List<Paddle>
        {
            new(PaddleLocations.Left),
            new(PaddleLocations.Right)
        };

        _ball = new Ball(_paddles, soundService);

        IsMouseVisible = GameOptions.IsMouseVisible;

        _paddles.ForEach(x => x.WonGame += HandlePaddleWinning);
    }

    protected override void Initialize()
    {
        InitializeServices();
        InitializeEntities();
    }

    protected override void Update(GameTime gameTime)
    {
        _inputState.Update(gameTime, GraphicsDevice.Viewport);

        if (!OperatingSystem.IsIOS()
            && _inputState.IsNewKeyPress(Keys.Escape))
        {
            Exit();
        }


        if (!OperatingSystem.IsIOS()
            && _inputState.IsNewKeyPress(Keys.Space))
        {
            _isPaused = !_isPaused;
        }

        if (!_isPaused)
        {
            _ball.Update();

            _paddles.ForEach(x => x.Update(_ball));
        }

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
                       .Draw.Paddles(_paddles)
                       .Draw.Ball(_ball)
                       .Draw.ScorePoints(_paddles)
                       .End();

        base.Draw(gameTime);
    }

    private void HandlePaddleWinning(object? sender, EventArgs eventArgs) => Reinitialize();

    private void InitializeEntities()
    {
        _ball.Initialize();
        _paddles.ForEach(x => x.Initialize());
    }

    private void InitializeServices()
    {
        _drawingService ??= new DrawingService(GraphicsDevice, new SpriteBatch(GraphicsDevice));

        _jingleService.Initialize();
    }

    public void Reinitialize()
    {
        InitializeServices();
        InitializeEntities();
    }
}

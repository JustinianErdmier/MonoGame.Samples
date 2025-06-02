using System;

using Microsoft.Xna.Framework;

using ShipGame.Core.Game;
using ShipGame.Core.Game.Screens;

namespace ShipGame.Core;

/// <summary>This is the main type for your game</summary>
public class ShipGameGame : Microsoft.Xna.Framework.Game
{
    private const bool RenderVsync = true;

    private static ShipGameGame? s_instance;

    private readonly GameManager _gameManager;

    private readonly GraphicsDeviceManager _graphicsDeviceManager;

    private readonly SoundManager _soundManager;

    private FontManager? _fontManager;

    private ScreenManager? _screenManager;

    public ShipGameGame()
    {
        _graphicsDeviceManager = new GraphicsDeviceManager(this);
        Content.RootDirectory  = "Content";
        Window.Title           = "ShipGame";

        _soundManager = new SoundManager();
        _gameManager  = new GameManager(_soundManager);

        _graphicsDeviceManager.PreferredBackBufferWidth  = GameOptions.ScreenWidth;
        _graphicsDeviceManager.PreferredBackBufferHeight = GameOptions.ScreenHeight;

        IsFixedTimeStep                                       = RenderVsync;
        _graphicsDeviceManager.SynchronizeWithVerticalRetrace = RenderVsync;
    }


    // ReSharper disable once RedundantOverriddenMember
    /// <summary>
    ///     Allows the game to perform any initialization it needs to before starting to run. This is where it can query for any required services and load any non-graphic-related
    ///     content. Calling base.Initialize will enumerate through any components and initialize them as well.
    /// </summary>
    protected override void Initialize() => base.Initialize();


    /// <summary>Load your graphics content.</summary>
    protected override void LoadContent()
    {
        _fontManager   = new FontManager(_graphicsDeviceManager.GraphicsDevice);
        _screenManager = new ScreenManager(this, _fontManager, _gameManager);

        _fontManager.LoadContent(Content);
        _gameManager.LoadContent(_graphicsDeviceManager.GraphicsDevice, Content);
        _screenManager.LoadContent(_graphicsDeviceManager.GraphicsDevice, Content);
        _soundManager.LoadContent(Content);
    }


    /// <summary>Unload your graphics content.</summary>
    protected override void UnloadContent()
    {
        _fontManager?.UnloadContent();
        _gameManager.UnloadContent();
        _screenManager?.UnloadContent();
        _soundManager.UnloadContent();

        _fontManager   = null;
        _screenManager = null;
    }


    /// <summary>Allows the game to run logic such as updating the world, checking for collisions, gathering input and playing audio.</summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Update(GameTime gameTime)
    {
        float elapsedTimeFloat = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _screenManager?.ProcessInput(elapsedTimeFloat);
        _screenManager?.Update(elapsedTimeFloat);

        base.Update(gameTime);
    }


    /// <summary>This is called when the game should draw itself.</summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Draw(GameTime gameTime)
    {
        _screenManager?.Draw(_graphicsDeviceManager.GraphicsDevice);

        base.Draw(gameTime);
    }

    /// <summary>This is called to switch to full-screen mode.</summary>
    public void ToggleFullScreen() => _graphicsDeviceManager.ToggleFullScreen();

    public static ShipGameGame GetInstance() => s_instance ?? throw new Exception(message: "The Game Instance Must Be Set Before Using The Game.");

    public static void SetInstance(ShipGameGame game) => s_instance = game;
}

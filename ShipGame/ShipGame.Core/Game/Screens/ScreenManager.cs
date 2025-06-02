#region Using Statements

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ShipGame.Core.Game.Graphics;

#endregion

namespace ShipGame.Core.Game.Screens;

public class ScreenManager : IDisposable
{
    private float backgroundTime; // time for background animation used on menus

    private BlurManager blurManager; // blur manager

    private RenderTarget2D colorRT; // render target for main color buffer

    private ContentManager contentManager; // content manager

    private Screen current; // currently active screen

    private float fade; // current fade time when in a transition

    private Vector4 fadeColor = Vector4.One; // color fading in and out

    // (null for no transition)

    private float fadeTime = 1.0f; // total fade time when in a transition

    private readonly FontManager fontManager; // font manager

    private int frameRate; // current game frame rate (in frames per sec)

    private int frameRateCount; // current frame count since last frame rate update

    private float frameRateTime; // elapsed time since last frame rate update

    private readonly GameManager gameManager; // game manager

    private RenderTarget2D glowRT1; // render target for glow horizontal blur

    private RenderTarget2D glowRT2; // render target for glow vertical blur

    private readonly InputManager inputManager; // input manager

    private Screen next; // next screen on a transition 

    private readonly List<Screen> screens; // list of available screens

    private readonly ShipGameGame shipGame; // xna game

    private Texture2D textureBackground; // the background texture used on menus

    // constructor
    public ScreenManager(ShipGameGame shipGame, FontManager font, GameManager game)
    {
        this.shipGame = shipGame;
        gameManager   = game;
        fontManager   = font;

        screens      = new List<Screen>();
        inputManager = new InputManager();

        // add all screens
        screens.Add(new ScreenIntro(this, game));
        screens.Add(new ScreenHelp(this, game));
        screens.Add(new ScreenPlayer(this, game));
        screens.Add(new ScreenLevel(this, game));
        screens.Add(new ScreenGame(this, game));
        screens.Add(new ScreenEnd(this, game));

        // fade in to intro screen
        SetNextScreen(ScreenType.ScreenIntro,
                      GameOptions.FadeColor,
                      GameOptions.FadeTime);

        fade = fadeTime * 0.5f;
    }

    // get intro screen
    public ScreenIntro ScreenIntro => (ScreenIntro)screens[(int)ScreenType.ScreenIntro];

    // get help screen
    public ScreenIntro ScreenHelp => (ScreenIntro)screens[(int)ScreenType.ScreenHelp];

    // get player screen
    public ScreenPlayer ScreenPlayer => (ScreenPlayer)screens[(int)ScreenType.ScreenPlayer];

    // get level screen
    public ScreenLevel ScreenLevel => (ScreenLevel)screens[(int)ScreenType.ScreenLevel];

    // get game screen
    public ScreenGame ScreenGame => (ScreenGame)screens[(int)ScreenType.ScreenGame];

    // get end screen
    public ScreenEnd ScreenEnd => (ScreenEnd)screens[(int)ScreenType.ScreenEnd];

    // process input
    public void ProcessInput(float elapsedTime)
    {
        inputManager.BeginInputProcessing(gameManager.GameMode == GameMode.SinglePlayer);

        // process input for currently active screen
        if (current != null
            && next == null)
        {
            current.ProcessInput(elapsedTime, inputManager);
        }

        // toggle full screen with F5 key
        if (inputManager.IsKeyPressed(player: 0, Keys.F5)
            || inputManager.IsKeyPressed(player: 1, Keys.F5))
        {
            shipGame.ToggleFullScreen();
        }

        inputManager.EndInputProcessing();
    }

    // update for given elapsed time
    public void Update(float elapsedTime)
    {
        // if in a transition
        if (fade > 0)
        {
            // update transition time
            fade -= elapsedTime;

            // if time to switch to new screen (fade out finished)
            if (next    != null
                && fade < 0.5f * fadeTime)
            {
                // tell new screen it is getting in focus
                next.SetFocus(contentManager, focus: true);

                // tell the old screen it lost its focus
                if (current != null)
                {
                    current.SetFocus(contentManager, focus: false);
                }

                // set new screen as current
                current = next;
                next    = null;
            }
        }

        // if current screen available, update it
        if (current != null)
        {
            current.Update(elapsedTime);
        }

        // calulate frame rate
        frameRateTime += elapsedTime;

        if (frameRateTime > 0.5f)
        {
            frameRate      = (int)(frameRateCount / frameRateTime);
            frameRateCount = 0;
            frameRateTime  = 0;
        }

        // accumulate elapsed time for background animation
        backgroundTime += elapsedTime;
    }

    // blur the color render target using the alpha channel and blur intensity
    private void BlurGlowRenterTarget(GraphicsDevice gd)
    {
        if (gd == null)
        {
            throw new ArgumentNullException(paramName: "gd");
        }

        //DepthStencilState ds = new DepthStencilState() { DepthBufferEnable = true, DepthBufferWriteEnable = true };
        //gd.DepthStencilState = ds;

        gd.DepthStencilState = DepthStencilState.None;

        //gd.BlendState = BlendState.Opaque;


        // if in game screen and split screen mode
        if (current                 == ScreenGame
            && gameManager.GameMode == GameMode.MultiPlayer)
        {
            // blur horizontal with split horizontal blur shader
            gd.SetRenderTarget(glowRT1);

            blurManager.RenderScreenQuad(gd,
                                         BlurTechnique.BlurHorizontalSplit,
                                         colorRT,
                                         Vector4.One);
        }
        else
        {
            // blur horizontal with regular horizontal blur shader
            gd.SetRenderTarget(glowRT1);

            blurManager.RenderScreenQuad(gd,
                                         BlurTechnique.BlurHorizontal,
                                         colorRT,
                                         Vector4.One);
        }

        // blur vertical with regular vertical blur shader
        gd.SetRenderTarget(glowRT2);

        blurManager.RenderScreenQuad(gd,
                                     BlurTechnique.BlurVertical,
                                     glowRT1,
                                     Vector4.One);

        //ds = new DepthStencilState() { DepthBufferEnable = false, DepthBufferWriteEnable = false };
        //gd.DepthStencilState = ds;
        gd.DepthStencilState = DepthStencilState.Default;

        gd.SetRenderTarget(renderTarget: null);
    }

    // draw render target as fullscreen texture with given intensity and blend mode
    private void DrawRenderTargetTexture(GraphicsDevice gd,
                                         RenderTarget2D renderTarget,
                                         float          intensity,
                                         bool           additiveBlend)
    {
        if (gd == null)
        {
            throw new ArgumentNullException(paramName: "gd");
        }

        // set up render state and blend mode
        //BlendState bs = gd.BlendState;
        //gd.DepthStencilState = DepthStencilState.Default;
        //if (additiveBlend)
        //{
        //    gd.BlendState = BlendState.Additive;
        //}

        gd.DepthStencilState = DepthStencilState.None;

        if (additiveBlend)
        {
            gd.BlendState = BlendState.Additive;
        }


        // draw render tareget as fullscreen texture
        blurManager.RenderScreenQuad(gd,
                                     BlurTechnique.ColorTexture,
                                     renderTarget,
                                     new Vector4(intensity));

        // restore render state and blend mode
        //gd.BlendState = bs;
        //gd.DepthStencilState = DepthStencilState.Default;
        //gd.BlendState = BlendState.Opaque;
        gd.DepthStencilState = DepthStencilState.Default;
    }

    // draw a texture with destination rectangle, color and blend mode
    public void DrawTexture(Texture2D  texture,
                            Rectangle  rect,
                            Color      color,
                            BlendState blend)
    {
        fontManager.DrawTexture(texture, rect, color, blend);
    }

    // draw a texture with source and destination rectangles, color and blend mode
    public void DrawTexture(Texture2D  texture,
                            Rectangle  destinationRect,
                            Rectangle  sourceRect,
                            Color      color,
                            BlendState blend)
    {
        fontManager.DrawTexture(texture, destinationRect, sourceRect, color, blend);
    }

    // draw a texture with desination rectange, rotation, color and blend settings
    public void DrawTexture(Texture2D  texture,
                            Rectangle  rect,
                            float      rotation,
                            Color      color,
                            BlendState blend)
    {
        fontManager.DrawTexture(texture, rect, rotation, color, blend);
    }

    // draw the background animated image
    public void DrawBackground(GraphicsDevice gd)
    {
        if (gd == null)
        {
            throw new ArgumentNullException(paramName: "gd");
        }

        const float animationTime   = 3.0f;
        const float animationLength = 0.4f;
        const int   numberLayers    = 2;
        const float layerDistance   = 1.0f / numberLayers;

        // normalized time
        float normalizedTime = backgroundTime / animationTime % 1.0f;

        // set render states
        DepthStencilState ds = gd.DepthStencilState;
        BlendState        bs = gd.BlendState;
        gd.DepthStencilState = DepthStencilState.DepthRead;
        gd.BlendState        = BlendState.AlphaBlend;

        float   scale;
        Vector4 color;

        // render all background layers
        for (int i = 0; i < numberLayers; i++)
        {
            if (normalizedTime > 0.5f)
            {
                scale = 2 - normalizedTime * 2;
            }
            else
            {
                scale = normalizedTime * 2;
            }

            color = new Vector4(scale, scale, scale, w: 0);

            scale = 1 + normalizedTime * animationLength;

            blurManager.RenderScreenQuad(gd,
                                         BlurTechnique.ColorTexture,
                                         textureBackground,
                                         color,
                                         scale);

            normalizedTime = (normalizedTime + layerDistance) % 1.0f;
        }

        // restore render states
        gd.DepthStencilState = ds;
        gd.BlendState        = bs;
    }

    // draws the currently active screen
    public void Draw(GraphicsDevice gd)
    {
        if (gd == null)
        {
            throw new ArgumentNullException(paramName: "gd");
        }

        frameRateCount++;

        // if a valid current screen is set
        if (current != null)
        {
            // set the color render target
            gd.SetRenderTarget(colorRT);

            // draw the screen 3D scene
            current.Draw3D(gd);

            // resolve the color render target
            gd.SetRenderTarget(renderTarget: null);

            // blur the glow render target
            BlurGlowRenterTarget(gd);

            // draw the 3D scene texture
            DrawRenderTargetTexture(gd, colorRT, intensity: 1.0f, additiveBlend: false);

            // draw the glow texture with additive blending
            DrawRenderTargetTexture(gd, glowRT2, intensity: 2.0f, additiveBlend: true);

            // begin text mode
            fontManager.BeginText();

            // draw the 2D scene 
            current.Draw2D(gd, fontManager);

            // draw fps
            //fontManager.DrawText(
            //    FontType.SmallFont,
            //    "FPS: " + frameRate,
            //    new Vector2(gd.Viewport.Width - 80, 0), Color.White);

            // end text mode
            fontManager.EndText();
        }

        // if in a transition
        if (fade > 0)
        {
            // compute transtition fade intensity
            float size = fadeTime * 0.5f;
            fadeColor.W = 1.25f * (1.0f - Math.Abs(fade - size) / size);

            // set alpha blend and no depth test or write
            gd.DepthStencilState = DepthStencilState.None;
            gd.BlendState        = BlendState.AlphaBlend;

            // draw transition fade color
            blurManager.RenderScreenQuad(gd, BlurTechnique.Color, texture: null, fadeColor);

            // restore render states
            gd.DepthStencilState = DepthStencilState.Default;
            gd.BlendState        = BlendState.Opaque;
        }
    }

    // load all content
    public void LoadContent(GraphicsDevice gd,
                            ContentManager content)
    {
        if (gd == null)
        {
            throw new ArgumentNullException(paramName: "gd");
        }

        contentManager    = content;
        textureBackground = content.Load<Texture2D>(assetName: "screens/intro_bg");

        // create blur manager
        blurManager = new BlurManager(gd,
                                      content.Load<Effect>(assetName: "shaders/Blur"),
                                      GameOptions.GlowResolution,
                                      GameOptions.GlowResolution);

        int width  = gd.Viewport.Width;
        int height = gd.Viewport.Height;


        // create render targets
        colorRT = new RenderTarget2D(gd,
                                     width,
                                     height,
                                     mipMap: true,
                                     SurfaceFormat.Color,
                                     DepthFormat.Depth24);

        glowRT1 = new RenderTarget2D(gd,
                                     GameOptions.GlowResolution,
                                     GameOptions.GlowResolution,
                                     mipMap: true,
                                     SurfaceFormat.Color,
                                     DepthFormat.Depth24);

        glowRT2 = new RenderTarget2D(gd,
                                     GameOptions.GlowResolution,
                                     GameOptions.GlowResolution,
                                     mipMap: true,
                                     SurfaceFormat.Color,
                                     DepthFormat.Depth24);
    }

    // unload all content
    public void UnloadContent()
    {
        textureBackground = null;

        if (blurManager != null)
        {
            blurManager.Dispose();
            blurManager = null;
        }

        if (colorRT != null)
        {
            colorRT.Dispose();
            colorRT = null;
        }

        if (glowRT1 != null)
        {
            glowRT1.Dispose();
            glowRT1 = null;
        }

        if (glowRT2 != null)
        {
            glowRT2.Dispose();
            glowRT2 = null;
        }
    }

    // starts a transition to a new screen
    // using a 1 sec fade time to custom color
    public bool SetNextScreen(ScreenType screenType,
                              Vector4    fadeColor,
                              float      fadeTime)
    {
        // if no transition already happening
        if (next == null)
        {
            // set next screen and transition options
            next           = screens[(int)screenType];
            this.fadeTime  = fadeTime;
            this.fadeColor = fadeColor;
            fade           = this.fadeTime;

            return true;
        }

        return false;
    }

    // starts a transition to a new screen
    // using a 1 sec fade time to custom color
    public bool SetNextScreen(ScreenType screenType, Vector4 fadeColor) => SetNextScreen(screenType, fadeColor, fadeTime: 1.0f);

    // starts a transition to a new screen
    // using a 1 sec fade time to black
    public bool SetNextScreen(ScreenType screenType) => SetNextScreen(screenType, Vector4.Zero, fadeTime: 1.0f);

    // get screen with given type
    public Screen GetScreen(ScreenType screenType) => screens[(int)screenType];

    // exit game
    public void Exit()
    {
        shipGame.Exit();
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
            UnloadContent();
        }
    }

    #endregion
}

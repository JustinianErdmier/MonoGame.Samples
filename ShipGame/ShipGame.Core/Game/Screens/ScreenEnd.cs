#region Using Statements

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ShipGame.Core.Game.Graphics;

#endregion

namespace ShipGame.Core.Game.Screens;

public class ScreenEnd : Screen
{
    private float elapsedTime; // elapsed time for rotation animation

    private readonly GameManager gameManager; // game manager

    private LightList lights; // lights for scene

    private Model padHaloModel; // model for the ship pad halo

    private Model padModel; // model for the ship pad

    private readonly ScreenManager screenManager; // screen manager

    private Model shipModel; // winner player ship model

    private Texture2D textureContinue; // texture with continue message

    private Texture2D texturePlayerWin; // texture with winning player number

    // constructor
    public ScreenEnd(ScreenManager manager, GameManager game)
    {
        screenManager = manager;
        gameManager   = game;
    }

    // called before screen shows
    public override void SetFocus(ContentManager content, bool focus)
    {
        // if getting focus
        if (focus)
        {
            // load all resources
            int winner = gameManager.PlayerWinner;

            shipModel = content.Load<Model>("ships/" + gameManager.GetPlayerShip(winner));

            padModel     = content.Load<Model>(assetName: "ships/pad");
            padHaloModel = content.Load<Model>(assetName: "ships/pad_halo");

            lights = LightList.Load(filename: "content/screens/end_lights.xml");

            textureContinue = content.Load<Texture2D>(assetName: "screens/continue");

            if (winner == 0)
            {
                texturePlayerWin = content.Load<Texture2D>(assetName: "screens/player1_wins");
            }
            else
            {
                texturePlayerWin = content.Load<Texture2D>(assetName: "screens/player2_wins");
            }
        }
        else // loosing focus
        {
            // free all resources
            shipModel    = null;
            padModel     = null;
            padHaloModel = null;

            lights = null;

            textureContinue  = null;
            texturePlayerWin = null;
        }
    }

    // process input
    public override void ProcessInput(float elapsedTime, InputManager input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(paramName: "input");
        }

        int i, j = (int)gameManager.GameMode;

        for (i = 0; i < j; i++)
        {
            // Any key/button to go back
            if (input.IsButtonPressedA(i)
                || input.IsButtonPressedB(i)
                || input.IsButtonPressedX(i)
                || input.IsButtonPressedY(i)
                || input.IsButtonPressedLeftShoulder(i)
                || input.IsButtonPressedRightShoulder(i)
                || input.IsButtonPressedLeftStick(i)
                || input.IsButtonPressedRightStick(i)
                || input.IsButtonPressedBack(i)
                || input.IsButtonPressedStart(i)
                || input.IsKeyPressed(i, Keys.Enter)
                || input.IsKeyPressed(i, Keys.Escape)
                || input.IsKeyPressed(i, Keys.Space))
            {
                screenManager.SetNextScreen(ScreenType.ScreenIntro);
                gameManager.PlaySound(soundName: "menu_cancel");
            }
        }
    }

    // update screen
    public override void Update(float elapsedTime)
    {
        // accumulate elapsed time
        this.elapsedTime += elapsedTime;
    }

    // draw 3D scene
    public override void Draw3D(GraphicsDevice gd)
    {
        if (gd == null)
        {
            throw new ArgumentNullException(paramName: "gd");
        }

        // clear background
        gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, depth: 1, stencil: 0);

        // draw background animation
        screenManager.DrawBackground(gd);

        // screen aspect
        float aspect = gd.Viewport.Width / (float)gd.Viewport.Height;

        // camera position
        Vector3 cameraPosition = new(x: 0, y: 240, z: -800);

        // view and projection matrices
        Matrix view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);

        Matrix projection =
            Matrix.CreatePerspectiveFieldOfView(fieldOfView: 0.25f, aspect, nearPlaneDistance: 1, farPlaneDistance: 1000);

        Matrix viewProjection = view * projection;

        // rotation matrix
        Matrix rotation = Matrix.CreateRotationY(0.5f * elapsedTime);

        // translation matrix
        Matrix translation = Matrix.CreateTranslation(xPosition: 0, yPosition: -40, zPosition: 0);

        // draw ship model
        gameManager.DrawModel(gd,
                              shipModel,
                              RenderTechnique.NormalMapping,
                              cameraPosition,
                              rotation,
                              viewProjection,
                              lights);

        // draw pad model
        gameManager.DrawModel(gd,
                              padModel,
                              RenderTechnique.NormalMapping,
                              cameraPosition,
                              translation,
                              viewProjection,
                              lights);

        // set additive blend with no glow (zero on alpha)
        gd.DepthStencilState = DepthStencilState.DepthRead;
        gd.BlendState        = BlendState.AlphaBlend;

        // disable glow (zero in alpha)
        //gd.RenderState.SeparateAlphaBlendEnabled = true;
        //gd.RenderState.AlphaBlendOperation = BlendFunction.Add;
        //gd.RenderState.AlphaSourceBlend = Blend.Zero;
        //gd.RenderState.AlphaDestinationBlend = Blend.Zero;

        // draw pad halo model
        gameManager.DrawModel(gd,
                              padHaloModel,
                              RenderTechnique.PlainMapping,
                              cameraPosition,
                              translation,
                              viewProjection,
                              lights: null);

        // restore blend modes
        gd.BlendState        = BlendState.Opaque;
        gd.DepthStencilState = DepthStencilState.Default;
    }

    // draw 2D gui
    public override void Draw2D(GraphicsDevice gd, FontManager font)
    {
        if (gd == null)
        {
            throw new ArgumentNullException(paramName: "gd");
        }

        Rectangle rect = new(x: 0, y: 0, width: 0, height: 0);

        int screenSizeX = gd.Viewport.Width;
        int screenSizeY = gd.Viewport.Height;

        // draw continue message
        rect.Width  = textureContinue.Width;
        rect.Height = textureContinue.Height;
        rect.Y      = screenSizeY     - rect.Height - 60;
        rect.X      = screenSizeX / 2 - rect.Width / 2;

        screenManager.DrawTexture(textureContinue,
                                  rect,
                                  Color.White,
                                  BlendState.AlphaBlend);

        // deaw winning player number
        rect.Width  = texturePlayerWin.Width;
        rect.Height = texturePlayerWin.Height;
        rect.Y      = 20;
        rect.X      = screenSizeX / 2 - rect.Width / 2;

        screenManager.DrawTexture(texturePlayerWin,
                                  rect,
                                  Color.White,
                                  BlendState.AlphaBlend);
    }
}

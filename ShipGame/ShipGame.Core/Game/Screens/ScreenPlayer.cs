#region Using Statements

using System;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ShipGame.Core.Game.Graphics;

#endregion

namespace ShipGame.Core.Game.Screens;

public class ScreenPlayer : Screen
{
    private const int NumberShips = 2; // number of available ships to choose from

    private static TextureCube reflectCube;

    // confirmed status for each player
    private readonly bool[] confirmed = new bool[2] { false, false };

    // total elapsed time for ship model rotation
    private float elapsedTime;

    private readonly GameManager gameManager; // game manager

    // invert Y flags (bit flag for each player)
    private uint invertY;

    private LightList lights; // lights for scene

    private Model padHaloModel; // ship pad halo model

    private Model padModel; // ship pad model

    private Model padSelectModel; // ship pad select model

    // rotation matrix for each player ship model
    private readonly Matrix[] rotation = new Matrix[2] { Matrix.Identity, Matrix.Identity };

    private readonly ScreenManager screenManager; // screen manager

    // ship selection for each player
    private readonly int[] selection = new int[2] { 0, 1 };

    // model for each ship
    private readonly Model[] shipModels = new Model[NumberShips];

    // name for each ship
    private readonly string[] ships = new string[NumberShips] { "ship2", "ship1" };

    private Texture2D textureChangeShip; // change ship texture

    private Texture2D textureInvertYCheck; // checked invert y texture

    private Texture2D textureInvertYUncheck; // unchecked invert y texture

    private Texture2D textureRotateShip; // rotate ship texture

    private Texture2D textureSelectBack; // select and back texture

    private Texture2D textureSelectCancel; // select and cancel texture

    // constructor
    public ScreenPlayer(ScreenManager manager, GameManager game)
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
            confirmed[0] = false;
            confirmed[1] = gameManager.GameMode == GameMode.SinglePlayer;

            rotation[0] = Matrix.Identity;
            rotation[1] = Matrix.Identity;

            lights = LightList.Load(Path.Combine(content.RootDirectory, path2: "screens", path3: "player_lights.xml"));

            for (int i = 0; i < NumberShips; i++)
            {
                shipModels[i] = content.Load<Model>("ships/" + ships[i]);
                FixupShip(shipModels[i], "ships/" + ships[i]);
            }

            padModel       = content.Load<Model>(assetName: "ships/pad");
            padHaloModel   = content.Load<Model>(assetName: "ships/pad_halo");
            padSelectModel = content.Load<Model>(assetName: "ships/pad_select");

            textureChangeShip     = content.Load<Texture2D>(assetName: "screens/change_ship");
            textureRotateShip     = content.Load<Texture2D>(assetName: "screens/rotate_ship");
            textureSelectBack     = content.Load<Texture2D>(assetName: "screens/select_back");
            textureSelectCancel   = content.Load<Texture2D>(assetName: "screens/select_cancel");
            textureInvertYCheck   = content.Load<Texture2D>(assetName: "screens/inverty_check");
            textureInvertYUncheck = content.Load<Texture2D>(assetName: "screens/inverty_uncheck");
        }
        else // loosing focus
        {
            // free all resources
            lights = null;

            for (int i = 0; i < NumberShips; i++)
            {
                shipModels[i] = null;
            }

            padModel       = null;
            padHaloModel   = null;
            padSelectModel = null;

            textureChangeShip     = null;
            textureRotateShip     = null;
            textureSelectBack     = null;
            textureSelectCancel   = null;
            textureInvertYCheck   = null;
            textureInvertYUncheck = null;
        }
    }

    public override void ProcessInput(float elapsedTime, InputManager input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(paramName: "input");
        }

        const float rotationVelocity = 3.0f;

        int i, j = (int)gameManager.GameMode;

        for (i = 0; i < j; i++)
        {
            if (confirmed[i] == false)
            {
                // change invert Y selection 
                if (input.IsKeyPressed(i, Keys.Y)
                    || input.IsButtonPressedY(i))
                {
                    invertY ^= (uint)1 << i;
                    gameManager.PlaySound(soundName: "menu_change");
                }

                // confirm selection
                if (input.IsKeyPressed(i, Keys.Enter)
                    || input.IsButtonPressedA(i))
                {
                    confirmed[i] = true;
                    gameManager.PlaySound(soundName: "menu_select");
                }

                // cancel and return to intro menu
                if (input.IsKeyPressed(i, Keys.Escape)
                    || input.IsButtonPressedB(i))
                {
                    gameManager.SetShips(shipPlayer1: null, shipPlayer2: null, invertYAxis: 0);
                    screenManager.SetNextScreen(ScreenType.ScreenIntro);
                    gameManager.PlaySound(soundName: "menu_cancel");
                }

                // rotate ship
                float RotX = rotationVelocity * input.LeftStick(i).X * elapsedTime;

                if (input.IsKeyDown(i, Keys.Left))
                {
                    RotX -= rotationVelocity * elapsedTime;
                }

                if (input.IsKeyDown(i, Keys.Right))
                {
                    RotX += rotationVelocity * elapsedTime;
                }

                if (Math.Abs(RotX) < 0.001f)
                {
                    RotX = -0.5f * elapsedTime;
                }

                rotation[i] = rotation[i] * Matrix.CreateRotationY(RotX);

                // change ship (next)
                if (input.IsKeyPressed(i, Keys.Up)
                    || input.IsButtonPressedDPadUp(i)
                    || input.IsButtonPressedLeftStickUp(i))
                {
                    selection[i] = (selection[i] + 1) % NumberShips;
                    gameManager.PlaySound(soundName: "menu_change");
                }

                // change ship (previous)
                if (input.IsKeyPressed(i, Keys.Down)
                    || input.IsButtonPressedDPadDown(i)
                    || input.IsButtonPressedLeftStickDown(i))
                {
                    if (selection[i] == 0)
                    {
                        selection[i] = NumberShips - 1;
                    }
                    else
                    {
                        selection[i] = selection[i] - 1;
                    }

                    gameManager.PlaySound(soundName: "menu_change");
                }
            }
            else
            {
                // cancel selection
                if (input.IsKeyPressed(i, Keys.Escape)
                    || input.IsButtonPressedB(i))
                {
                    confirmed[i] = false;
                    gameManager.PlaySound(soundName: "menu_cancel");
                }
            }
        }

        // if both ships confirmed, go to game screen
        if (confirmed[0]
            && confirmed[1])
        {
            if (gameManager.GameMode == GameMode.SinglePlayer)
            {
                gameManager.SetShips(ships[selection[0]], shipPlayer2: null, invertY);
            }
            else
            {
                gameManager.SetShips(ships[selection[0]],
                                     ships[selection[1]],
                                     invertY);
            }

            screenManager.SetNextScreen(ScreenType.ScreenLevel);
        }
    }

    public override void Update(float elapsedTime)
    {
        // accumulate elapsed time
        this.elapsedTime += elapsedTime;
    }

    public override void Draw3D(GraphicsDevice gd)
    {
        if (gd == null)
        {
            throw new ArgumentNullException(paramName: "gd");
        }

        // clear backgournd
        gd.Clear(Color.Black);

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

        // translation matrix
        Matrix transform = Matrix.CreateTranslation(xPosition: 0, yPosition: -40, zPosition: 0);

        // if single player mode
        if (gameManager.GameMode == GameMode.SinglePlayer)
        {
            // draw ship model
            gameManager.DrawModel(gd,
                                  shipModels[selection[0]],
                                  RenderTechnique.NormalMapping,
                                  cameraPosition,
                                  rotation[0],
                                  viewProjection,
                                  lights);

            // draw pad model
            gameManager.DrawModel(gd,
                                  padModel,
                                  RenderTechnique.NormalMapping,
                                  cameraPosition,
                                  transform,
                                  viewProjection,
                                  lights);

            // set additive blend
            gd.DepthStencilState = DepthStencilState.DepthRead;
            gd.BlendState        = BlendState.Additive;


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
                                  transform,
                                  viewProjection,
                                  lights: null);

            // enable glow (alpha not zero)
            //gd.RenderState.SeparateAlphaBlendEnabled = false;
            gd.BlendState = BlendState.AlphaBlend;

            // if not confirmed, draw animated selection circle
            if (confirmed[0] == false)
            {
                transform = Matrix.CreateRotationY(elapsedTime);
                float scale = 1.0f + 0.03f * (float)Math.Cos(elapsedTime * 7);
                transform     = transform * Matrix.CreateScale(scale);
                transform.M42 = -10;

                gameManager.DrawModel(gd,
                                      padSelectModel,
                                      RenderTechnique.PlainMapping,
                                      cameraPosition,
                                      transform,
                                      viewProjection,
                                      lights: null);
            }

            // restore blend modes
            gd.DepthStencilState = DepthStencilState.Default;
            gd.BlendState        = BlendState.Opaque;
        }
        else // if multi player mode
        {
            Matrix transform1 = rotation[0] * Matrix.CreateTranslation(xPosition: 90, yPosition: 0, zPosition: 0);
            Matrix transform2 = rotation[1] * Matrix.CreateTranslation(xPosition: -90, yPosition: 0, zPosition: 0);

            // draw ship model for player 1
            gameManager.DrawModel(gd,
                                  shipModels[selection[0]],
                                  RenderTechnique.NormalMapping,
                                  cameraPosition,
                                  transform1,
                                  viewProjection,
                                  lights);

            // draw ship model for player 2
            gameManager.DrawModel(gd,
                                  shipModels[selection[1]],
                                  RenderTechnique.NormalMapping,
                                  cameraPosition,
                                  transform2,
                                  viewProjection,
                                  lights);

            // draw pad model for player 1
            transform.M41 = 90;

            gameManager.DrawModel(gd,
                                  padModel,
                                  RenderTechnique.NormalMapping,
                                  cameraPosition,
                                  transform,
                                  viewProjection,
                                  lights);

            // draw pad model for player 2
            transform.M41 = -90;

            gameManager.DrawModel(gd,
                                  padModel,
                                  RenderTechnique.NormalMapping,
                                  cameraPosition,
                                  transform,
                                  viewProjection,
                                  lights);

            // set additive blend
            gd.DepthStencilState = DepthStencilState.DepthRead;
            gd.BlendState        = BlendState.Additive;

            // disable glow (zero in alpha)
            //gd.RenderState.SeparateAlphaBlendEnabled = true;
            //gd.RenderState.AlphaBlendOperation = BlendFunction.Add;
            //gd.RenderState.AlphaSourceBlend = Blend.Zero;
            //gd.RenderState.AlphaDestinationBlend = Blend.Zero;

            // draw pad halo model for player 1
            transform.M41 = 90;

            gameManager.DrawModel(gd,
                                  padHaloModel,
                                  RenderTechnique.NormalMapping,
                                  cameraPosition,
                                  transform,
                                  viewProjection,
                                  lights: null);

            // draw pad halo model for player 2
            transform.M41 = -90;

            gameManager.DrawModel(gd,
                                  padHaloModel,
                                  RenderTechnique.NormalMapping,
                                  cameraPosition,
                                  transform,
                                  viewProjection,
                                  lights: null);

            // enable glow (alpha not zero)
            //gd.RenderState.SeparateAlphaBlendEnabled = false;
            gd.BlendState = BlendState.AlphaBlend;


            // if not confirmed, draw animated selection circle for player 1
            if (confirmed[0] == false)
            {
                transform = Matrix.CreateRotationY(elapsedTime);
                float scale = 0.9f + 0.03f * (float)Math.Cos(elapsedTime * 7);
                transform     = transform * Matrix.CreateScale(scale);
                transform.M41 = 90;
                transform.M42 = -10;

                gameManager.DrawModel(gd,
                                      padSelectModel,
                                      RenderTechnique.PlainMapping,
                                      cameraPosition,
                                      transform,
                                      viewProjection,
                                      lights: null);
            }

            // if not confirmed, draw animated selection circle for player 2
            if (confirmed[1] == false)
            {
                transform = Matrix.CreateRotationY(elapsedTime);
                float scale = 0.9f + 0.03f * (float)Math.Cos(elapsedTime * 7);
                transform     = transform * Matrix.CreateScale(scale);
                transform.M41 = -90;
                transform.M42 = -10;

                gameManager.DrawModel(gd,
                                      padSelectModel,
                                      RenderTechnique.PlainMapping,
                                      cameraPosition,
                                      transform,
                                      viewProjection,
                                      lights: null);
            }

            // restore blend modes
            gd.DepthStencilState = DepthStencilState.Default;
            gd.BlendState        = BlendState.Opaque;
        }
    }

    public override void Draw2D(GraphicsDevice gd, FontManager font)
    {
        if (gd == null)
        {
            throw new ArgumentNullException(paramName: "gd");
        }

        Rectangle rect = new(x: 0, y: 0, width: 0, height: 0);

        int screenSizeX = gd.Viewport.Width;
        int screenSizeY = gd.Viewport.Height;

        // if single player mode
        if (gameManager.GameMode == GameMode.SinglePlayer)
        {
            rect.Width  = textureSelectBack.Width;
            rect.Height = textureSelectBack.Height;
            rect.X      = screenSizeX / 2 - rect.Width / 2;
            rect.Y      = 50;

            if (confirmed[0])
            {
                rect.Width  = textureSelectCancel.Width;
                rect.Height = textureSelectCancel.Height;

                screenManager.DrawTexture(textureSelectCancel,
                                          rect,
                                          Color.White,
                                          BlendState.AlphaBlend);
            }
            else
            {
                screenManager.DrawTexture(textureSelectBack,
                                          rect,
                                          Color.White,
                                          BlendState.AlphaBlend);
            }

            rect.Width  = textureInvertYCheck.Width;
            rect.Height = textureInvertYCheck.Height;
            rect.Y      = screenSizeY     - rect.Height - 30;
            rect.X      = screenSizeX / 2 - rect.Width / 2;

            if ((invertY & 1) == 0)
            {
                screenManager.DrawTexture(textureInvertYUncheck,
                                          rect,
                                          Color.White,
                                          BlendState.AlphaBlend);
            }
            else
            {
                screenManager.DrawTexture(textureInvertYCheck,
                                          rect,
                                          Color.White,
                                          BlendState.AlphaBlend);
            }

            rect.Width  = textureChangeShip.Width;
            rect.Height = textureChangeShip.Height;
            rect.X      = screenSizeX / 5 - rect.Width / 2;
            rect.Y      = 60;

            screenManager.DrawTexture(textureChangeShip,
                                      rect,
                                      Color.White,
                                      BlendState.AlphaBlend);

            rect.Width  = textureRotateShip.Width;
            rect.Height = textureRotateShip.Height;
            rect.X      = screenSizeX * 4 / 5 - rect.Width / 2;
            rect.Y      = 60;

            screenManager.DrawTexture(textureRotateShip,
                                      rect,
                                      Color.White,
                                      BlendState.AlphaBlend);
        }
        else // if multi player mode
        {
            rect.Width  = textureChangeShip.Width;
            rect.Height = textureChangeShip.Height;
            rect.X      = (screenSizeX - rect.Width) / 2;
            rect.Y      = 40;

            screenManager.DrawTexture(textureChangeShip,
                                      rect,
                                      Color.White,
                                      BlendState.AlphaBlend);

            rect.Width  = textureRotateShip.Width;
            rect.Height = textureRotateShip.Height;
            rect.X      = (screenSizeX - rect.Width) / 2;
            rect.Y      = 40 + textureChangeShip.Height;

            screenManager.DrawTexture(textureRotateShip,
                                      rect,
                                      Color.White,
                                      BlendState.AlphaBlend);

            rect.Width  = textureInvertYCheck.Width;
            rect.Height = textureInvertYCheck.Height;
            rect.Y      = screenSizeY     - rect.Height - 30;
            rect.X      = screenSizeX / 4 - rect.Width / 2;

            if ((invertY & 1) == 0)
            {
                screenManager.DrawTexture(textureInvertYUncheck,
                                          rect,
                                          Color.White,
                                          BlendState.AlphaBlend);
            }
            else
            {
                screenManager.DrawTexture(textureInvertYCheck,
                                          rect,
                                          Color.White,
                                          BlendState.AlphaBlend);
            }

            rect.X = screenSizeX * 3 / 4 - rect.Width / 2;

            if ((invertY & 2) == 0)
            {
                screenManager.DrawTexture(textureInvertYUncheck,
                                          rect,
                                          Color.White,
                                          BlendState.AlphaBlend);
            }
            else
            {
                screenManager.DrawTexture(textureInvertYCheck,
                                          rect,
                                          Color.White,
                                          BlendState.AlphaBlend);
            }

            rect.Width  = textureSelectBack.Width;
            rect.Height = textureSelectBack.Height;
            rect.X      = screenSizeX / 8 - rect.Width / 2;
            rect.Y      = 40;

            if (confirmed[0])
            {
                rect.Width  = textureSelectCancel.Width;
                rect.Height = textureSelectCancel.Height;

                screenManager.DrawTexture(textureSelectCancel,
                                          rect,
                                          Color.White,
                                          BlendState.AlphaBlend);
            }
            else
            {
                screenManager.DrawTexture(textureSelectBack,
                                          rect,
                                          Color.White,
                                          BlendState.AlphaBlend);
            }

            rect.Width  = textureSelectBack.Width;
            rect.Height = textureSelectBack.Height;
            rect.X      = screenSizeX * 7 / 8 - rect.Width / 2;
            rect.Y      = 40;

            if (confirmed[1])
            {
                rect.Width  = textureSelectCancel.Width;
                rect.Height = textureSelectCancel.Height;

                screenManager.DrawTexture(textureSelectCancel,
                                          rect,
                                          Color.White,
                                          BlendState.AlphaBlend);
            }
            else
            {
                screenManager.DrawTexture(textureSelectBack,
                                          rect,
                                          Color.White,
                                          BlendState.AlphaBlend);
            }
        }
    }

    /// <summary>Performs effect initialization, which is required in XNA 4.0</summary>
    /// <param name="model"></param>
    private void FixupShip(Model model, string path)
    {
        ShipGameGame game = ShipGameGame.GetInstance();

        foreach (ModelMesh mesh in model.Meshes)
        {
            // for each mesh part
            foreach (Effect effect in mesh.Effects)
            {
                effect.Parameters[name: "Reflect"].SetValue(GetReflectCube());
            }
        }
    }

    /// <summary>Creates a reflection textureCube</summary>
    private static TextureCube GetReflectCube()
    {
        if (reflectCube != null)
        {
            return reflectCube;
        }

        Color[] cc = new[]
        {
            new Color(r: 1, g: 0, b: 0),
            new Color(r: 0.9f, g: 0, b: 0.1f),
            new Color(r: 0.8f, g: 0, b: 0.2f),
            new Color(r: 0.7f, g: 0, b: 0.3f),
            new Color(r: 0.6f, g: 0, b: 0.4f),
            new Color(r: 0.5f, g: 0, b: 0.5f),
            new Color(r: 0.4f, g: 0, b: 0.6f),
            new Color(r: 0.3f, g: 0, b: 0.7f),
            new Color(r: 0.2f, g: 0, b: 0.8f),
            new Color(r: 0.1f, g: 0, b: 0.9f),
            new Color(r: 0.1f, g: 0, b: 0.9f),
            new Color(r: 0.0f, g: 0, b: 1.0f)
        };

        reflectCube = new TextureCube(ShipGameGame.GetInstance().GraphicsDevice,
                                      size: 8,
                                      mipMap: true,
                                      SurfaceFormat.Color);

        Random rand = new();

        for (int s = 0; s < 6; s++)
        {
            Color[] sideData = new Color[reflectCube.Size * reflectCube.Size];

            for (int i = 0; i < sideData.Length; i++)
            {
                sideData[i] = cc[rand.Next(cc.Length)];
            }

            reflectCube.SetData((CubeMapFace)s, sideData);
        }

        return reflectCube;
    }
}

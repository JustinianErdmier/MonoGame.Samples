#region Using Statements

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace ShipGame.Core.Game.Screens;

public class ScreenLevel : Screen
{
    private const int NumberLevels = 2; // number of available levels to choose from

    private Texture2D changeLevel; // change level texture

    private readonly GameManager gameManager; // game manager

    // name for each level
    private readonly string[] levels = new string[NumberLevels] { "level1", "level2" };

    // screen shot for each level
    private readonly Texture2D[] levelShots = new Texture2D[NumberLevels];

    private readonly ScreenManager screenManager; // screen manager

    private Texture2D selectBack; // select and back texture

    private int selection;

    // constructor
    public ScreenLevel(ScreenManager manager, GameManager game)
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
            for (int i = 0; i < NumberLevels; i++)
            {
                levelShots[i] = content.Load<Texture2D>("screens/" + levels[i] + "_screen");
            }

            selectBack  = content.Load<Texture2D>(assetName: "screens/select_back");
            changeLevel = content.Load<Texture2D>(assetName: "screens/change_level");
        }
        else // loosing focus
        {
            // free all resources
            for (int i = 0; i < NumberLevels; i++)
            {
                levelShots[i] = null;
            }

            selectBack  = null;
            changeLevel = null;
        }
    }

    public override void ProcessInput(float elapsedTime, InputManager input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(paramName: "input");
        }

        int i, j = (int)gameManager.GameMode;

        for (i = 0; i < j; i++)
        {
            // select
            if (input.IsKeyPressed(i, Keys.Enter)
                || input.IsButtonPressedA(i))
            {
                gameManager.SetLevel(levels[selection]);
                screenManager.SetNextScreen(ScreenType.ScreenGame);
                gameManager.PlaySound(soundName: "menu_select");
            }

            // cancel
            if (input.IsKeyPressed(i, Keys.Escape)
                || input.IsButtonPressedB(i))
            {
                gameManager.SetLevel(levelFileName: null);
                screenManager.SetNextScreen(ScreenType.ScreenPlayer);
                gameManager.PlaySound(soundName: "menu_cancel");
            }

            // change selection (previous)
            if (input.IsKeyPressed(i, Keys.Left)
                || input.IsButtonPressedDPadLeft(i)
                || input.IsButtonPressedLeftStickLeft(i))
            {
                if (selection == 0)
                {
                    selection = levels.GetLength(dimension: 0) - 1;
                }
                else
                {
                    selection = selection - 1;
                }

                gameManager.PlaySound(soundName: "menu_change");
            }

            // change selection (next)
            if (input.IsKeyPressed(i, Keys.Right)
                || input.IsButtonPressedDPadRight(i)
                || input.IsButtonPressedLeftStickRight(i))
            {
                selection = (selection + 1) % levels.GetLength(dimension: 0);
                gameManager.PlaySound(soundName: "menu_change");
            }
        }
    }

    public override void Update(float elapsedTime)
    { }

    public override void Draw3D(GraphicsDevice gd)
    {
        if (gd == null)
        {
            throw new ArgumentNullException(paramName: "gd");
        }

        // clear background
        gd.Clear(Color.Black);

        // draw background animation
        screenManager.DrawBackground(gd);
    }

    public override void Draw2D(GraphicsDevice gd, FontManager font)
    {
        if (gd == null)
        {
            throw new ArgumentNullException(paramName: "gd");
        }

        int screenSizeX = gd.Viewport.Width;
        int screenSizeY = gd.Viewport.Height;

        Rectangle rect = new(x: 0, y: 0, width: 0, height: 0);

        // draw level screen shot
        rect.Width  = levelShots[selection].Width;
        rect.Height = levelShots[selection].Height;
        rect.X      = (screenSizeX - rect.Width) / 2;
        rect.Y      = (screenSizeY - rect.Height) / 2 + 30;

        screenManager.DrawTexture(levelShots[selection],
                                  rect,
                                  Color.White,
                                  BlendState.AlphaBlend);

        // draw back and select buttons
        rect.Width  = selectBack.Width;
        rect.Height = selectBack.Height;
        rect.X      = (screenSizeX - rect.Width) / 2;
        rect.Y      = 30;

        screenManager.DrawTexture(selectBack,
                                  rect,
                                  Color.White,
                                  BlendState.AlphaBlend);

        // draw change level text
        rect.Width  = changeLevel.Width;
        rect.Height = changeLevel.Height;
        rect.X      = (screenSizeX - rect.Width) / 2;
        rect.Y      = screenSizeY - rect.Height - 30;

        screenManager.DrawTexture(changeLevel,
                                  rect,
                                  Color.White,
                                  BlendState.AlphaBlend);
    }
}

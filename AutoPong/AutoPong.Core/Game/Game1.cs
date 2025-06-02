using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AutoPong.Core.Game;

public class Game1 : Microsoft.Xna.Framework.Game
{
    private const int PlayerIndex = (int)Microsoft.Xna.Framework.PlayerIndex.One;

    private const int PositionSpacing = 40;

    private const int TextStartPosition = 20;

    private readonly InputState _inputState = new();

    private readonly Color _textDrawColour = Color.White;

    private SpriteFont _hudFont;

    private SpriteBatch _spriteBatch;

    public Game1()
    {
        GraphicsDeviceManager graphics = new(this);

        Content.RootDirectory              = "Content";
        graphics.IsFullScreen              = true;
        graphics.PreferredBackBufferWidth  = 2560;
        graphics.PreferredBackBufferHeight = 1440;

        IsMouseVisible = true;
    }

    // ReSharper disable once RedundantOverriddenMember
    protected override void Initialize()
    {
        // TODO: Add your initialization logic here.

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: Use `this.Content` to load your game content here.
        _hudFont = Content.Load<SpriteFont>(assetName: "Fonts/Hud");
    }

    protected override void Update(GameTime gameTime)
    {
        _inputState.Update(gameTime, GraphicsDevice.Viewport);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        string state = _inputState.CurrentGamePadStates[PlayerIndex].IsConnected ? "Connected" : "Disconnected";

        string connectedValue = $"GamePad: {state}";
        string aButtonValue   = $"A Button: {_inputState.CurrentGamePadStates[PlayerIndex].IsButtonDown(Buttons.A)}";
        string bButtonValue   = $"B Button: {_inputState.CurrentGamePadStates[PlayerIndex].IsButtonDown(Buttons.B)}";
        string xButtonValue   = $"X Button: {_inputState.CurrentGamePadStates[PlayerIndex].IsButtonDown(Buttons.X)}";
        string yButtonValue   = $"Y Button: {_inputState.CurrentGamePadStates[PlayerIndex].IsButtonDown(Buttons.Y)}";

        string startButtonValue = $"Start Button: {_inputState.CurrentGamePadStates[PlayerIndex].IsButtonDown(Buttons.Start)}";
        string backButtonValue  = $"Back Button: {_inputState.CurrentGamePadStates[PlayerIndex].IsButtonDown(Buttons.Back)}";
        string bigButtonValue   = $"Big Button: {_inputState.CurrentGamePadStates[PlayerIndex].IsButtonDown(Buttons.BigButton)}";

        string dPadLButtonValue = $"DPadL Button: {_inputState.CurrentGamePadStates[PlayerIndex].IsButtonDown(Buttons.DPadLeft)}";
        string dPadRButtonValue = $"DPadR Button: {_inputState.CurrentGamePadStates[PlayerIndex].IsButtonDown(Buttons.DPadRight)}";
        string dPadUButtonValue = $"DPadU Button: {_inputState.CurrentGamePadStates[PlayerIndex].IsButtonDown(Buttons.DPadUp)}";
        string dPadDButtonValue = $"DPadD Button: {_inputState.CurrentGamePadStates[PlayerIndex].IsButtonDown(Buttons.DPadDown)}";

        string rBumperButtonValue     = $"RBumper Button: {_inputState.CurrentGamePadStates[PlayerIndex].IsButtonDown(Buttons.RightShoulder)}";
        string rThumbstickButtonValue = $"RThumbstick Button: {_inputState.CurrentGamePadStates[PlayerIndex].IsButtonDown(Buttons.RightThumbstickDown)}";
        string rThumbstickStateValue  = $"RThumbstick State: {_inputState.CurrentGamePadStates[PlayerIndex].ThumbSticks.Right}";
        string rTriggerValue          = $"RTrigger Button: {_inputState.CurrentGamePadStates[PlayerIndex].IsButtonDown(Buttons.RightTrigger)}";
        string rTriggerStateValue     = $"RTrigger State: {_inputState.CurrentGamePadStates[PlayerIndex].Triggers.Right}";

        string lBumperButtonValue     = $"LBumper Button: {_inputState.CurrentGamePadStates[PlayerIndex].IsButtonDown(Buttons.LeftShoulder)}";
        string lThumbstickButtonValue = $"LThumbstick Button: {_inputState.CurrentGamePadStates[PlayerIndex].IsButtonDown(Buttons.LeftThumbstickDown)}";
        string lThumbstickStateValue  = $"LThumbstick State: {_inputState.CurrentGamePadStates[PlayerIndex].ThumbSticks.Left}";
        string lTriggerValue          = $"LTrigger Button: {_inputState.CurrentGamePadStates[PlayerIndex].IsButtonDown(Buttons.LeftTrigger)}";
        string lTriggerStateValue     = $"LTrigger State: {_inputState.CurrentGamePadStates[PlayerIndex].Triggers.Left}";

        _spriteBatch.Begin();

        // TODO: Add your drawing code here.
        _spriteBatch.DrawString(_hudFont, connectedValue, new Vector2(x: 20, TextStartPosition), _textDrawColour);

        DrawInputValue(aButtonValue, index: 1);
        DrawInputValue(bButtonValue, index: 2);
        DrawInputValue(xButtonValue, index: 3);
        DrawInputValue(yButtonValue, index: 4);

        DrawInputValue(startButtonValue, index: 5);
        DrawInputValue(backButtonValue, index: 6);
        DrawInputValue(bigButtonValue, index: 7);

        DrawInputValue(dPadLButtonValue, index: 8);
        DrawInputValue(dPadRButtonValue, index: 9);
        DrawInputValue(dPadUButtonValue, index: 10);
        DrawInputValue(dPadDButtonValue, index: 11);

        DrawInputValue(lBumperButtonValue, index: 12);
        DrawInputValue(lThumbstickButtonValue, index: 13);
        DrawInputValue(lThumbstickStateValue, index: 14);
        DrawInputValue(lTriggerValue, index: 15);
        DrawInputValue(lTriggerStateValue, index: 16);

        DrawInputValue(rBumperButtonValue, index: 17);
        DrawInputValue(rThumbstickButtonValue, index: 18);
        DrawInputValue(rThumbstickStateValue, index: 19);
        DrawInputValue(rTriggerValue, index: 20);
        DrawInputValue(rTriggerStateValue, index: 21);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawInputValue(string aButtonValue, int index)
        => _spriteBatch.DrawString(_hudFont, aButtonValue, new Vector2(x: 20, GetTextPositionForIndex(index)), _textDrawColour);

    private static int GetTextPositionForIndex(int index) => TextStartPosition + PositionSpacing * index;
}

#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#endregion

namespace ShipGame.Core.Game;

public class InputState
{
    public KeyboardState[] keyState;

    public GamePadState[] padState;

    public InputState()
    {
        padState = new GamePadState[2];
        keyState = new KeyboardState[2];
        GetInput(singlePlayer: false);
    }

    public void GetInput(bool singlePlayer)
    {
        padState[0] = GamePad.GetState(PlayerIndex.One);
        padState[1] = GamePad.GetState(PlayerIndex.Two);

        if (singlePlayer)
        {
            keyState[0] = Keyboard.GetState();
        }
        else
        {
            keyState[1] = Keyboard.GetState();
        }
    }

    public void CopyInput(InputState state)
    {
        padState[0] = state.padState[0];
        padState[1] = state.padState[1];
        keyState[0] = state.keyState[0];
        keyState[1] = state.keyState[1];
    }
}

public class InputManager
{
    /// <summary>Create a new input manager</summary>
    public InputManager()
    {
        CurrentState = new InputState();
        LastState    = new InputState();
    }

    /// <summary>Get the current input state</summary>
    public InputState CurrentState { get; }

    /// <summary>Get last frame input state</summary>
    public InputState LastState { get; }

    /// <summary>Begin input (aqruire input from all controlls)</summary>
    public void BeginInputProcessing(bool singlePlayer)
    {
        CurrentState.GetInput(singlePlayer);
    }

    /// <summary>End input (save current input to last frame input)</summary>
    public void EndInputProcessing()
    {
        LastState.CopyInput(CurrentState);
    }

    /// <summary>Check if a key is down in current frame for a given player</summary>
    public bool IsKeyDown(int player, Keys key) => CurrentState.keyState[player].IsKeyDown(key);

    /// <summary>Check if a key was pressed in this frame for a given player (down in this frame and up in last frame)</summary>
    public bool IsKeyPressed(int player, Keys key) => CurrentState.keyState[player].IsKeyDown(key) && LastState.keyState[player].IsKeyUp(key);

    /// <summary>Return left stick position in a Vector2</summary>
    public Vector2 LeftStick(int player) => CurrentState.padState[player].ThumbSticks.Left;

    /// <summary>Return right stick position in a Vector2</summary>
    public Vector2 RightStick(int player) => CurrentState.padState[player].ThumbSticks.Right;

    /// <summary>Check if left trigger was pressed in this frame for a given player (positive this frame and zero in last frame)</summary>
    public bool IsTriggerPressedLeft(int player) => CurrentState.padState[player].Triggers.Left > 0 && LastState.padState[player].Triggers.Left == 0;

    /// <summary>Check if right trigger was pressed in this frame for a given player (positive this frame and zero in last frame)</summary>
    public bool IsTriggerPressedRigth(int player) => CurrentState.padState[player].Triggers.Right > 0 && LastState.padState[player].Triggers.Right == 0;

    /// <summary>Check if back button was pressed in this frame for a given player (down in this frame and up in last frame)</summary>
    public bool IsButtonPressedBack(int player)
        => CurrentState.padState[player].Buttons.Back == ButtonState.Pressed && LastState.padState[player].Buttons.Back == ButtonState.Released;

    /// <summary>Check if start button was pressed in this frame for a given player (down in this frame and up in last frame)</summary>
    public bool IsButtonPressedStart(int player)
        => CurrentState.padState[player].Buttons.Start == ButtonState.Pressed && LastState.padState[player].Buttons.Start == ButtonState.Released;

    /// <summary>Check if dpad left button was pressed in this frame for a given player (down in this frame and up in last frame)</summary>
    public bool IsButtonPressedDPadLeft(int player)
        => CurrentState.padState[player].DPad.Left == ButtonState.Pressed && LastState.padState[player].DPad.Left == ButtonState.Released;

    /// <summary>Check if dpad right button was pressed in this frame for a given player (down in this frame and up in last frame)</summary>
    public bool IsButtonPressedDPadRight(int player)
        => CurrentState.padState[player].DPad.Right == ButtonState.Pressed && LastState.padState[player].DPad.Right == ButtonState.Released;

    /// <summary>Check if dpad up button was pressed in this frame for a given player (down in this frame and up in last frame)</summary>
    public bool IsButtonPressedDPadUp(int player) => CurrentState.padState[player].DPad.Up == ButtonState.Pressed && LastState.padState[player].DPad.Up == ButtonState.Released;

    /// <summary>Check if dpad down button was pressed in this frame for a given player (down in this frame and up in last frame)</summary>
    public bool IsButtonPressedDPadDown(int player)
        => CurrentState.padState[player].DPad.Down == ButtonState.Pressed && LastState.padState[player].DPad.Down == ButtonState.Released;

    /// <summary>Check if A button was pressed in this frame for a given player (down in this frame and up in last frame)</summary>
    public bool IsButtonPressedA(int player) => CurrentState.padState[player].Buttons.A == ButtonState.Pressed && LastState.padState[player].Buttons.A == ButtonState.Released;

    /// <summary>Check if B button was pressed in this frame for a given player (down in this frame and up in last frame)</summary>
    public bool IsButtonPressedB(int player) => CurrentState.padState[player].Buttons.B == ButtonState.Pressed && LastState.padState[player].Buttons.B == ButtonState.Released;

    /// <summary>Check if X button was pressed in this frame for a given player (down in this frame and up in last frame)</summary>
    public bool IsButtonPressedX(int player) => CurrentState.padState[player].Buttons.X == ButtonState.Pressed && LastState.padState[player].Buttons.X == ButtonState.Released;

    /// <summary>Check if A button was pressed in this frame for a given player (down in this frame and up in last frame)</summary>
    public bool IsButtonPressedY(int player) => CurrentState.padState[player].Buttons.Y == ButtonState.Pressed && LastState.padState[player].Buttons.Y == ButtonState.Released;

    /// <summary>Check if left shoulder button was pressed in this frame for a given player (down in this frame and up in last frame)</summary>
    public bool IsButtonPressedLeftShoulder(int player)
        => CurrentState.padState[player].Buttons.LeftShoulder == ButtonState.Pressed && LastState.padState[player].Buttons.LeftShoulder == ButtonState.Released;

    /// <summary>Check if right shoulder button was pressed in this frame for a given player (down in this frame and up in last frame)</summary>
    public bool IsButtonPressedRightShoulder(int player)
        => CurrentState.padState[player].Buttons.RightShoulder == ButtonState.Pressed && LastState.padState[player].Buttons.RightShoulder == ButtonState.Released;

    /// <summary>Check if left stick was pressed in this frame for a given player (down in this frame and up in last frame)</summary>
    public bool IsButtonPressedLeftStick(int player)
        => CurrentState.padState[player].Buttons.LeftStick == ButtonState.Pressed && LastState.padState[player].Buttons.LeftStick == ButtonState.Released;

    /// <summary>Check if right stick was pressed in this frame for a given player (down in this frame and up in last frame)</summary>
    public bool IsButtonPressedRightStick(int player)
        => CurrentState.padState[player].Buttons.RightStick == ButtonState.Pressed && LastState.padState[player].Buttons.RightStick == ButtonState.Released;

    /// <summary>Check left stick as a button for up press</summary>
    public bool IsButtonPressedLeftStickUp(int player) => CurrentState.padState[player].ThumbSticks.Left.Y > 0.5f && LastState.padState[player].ThumbSticks.Left.Y <= 0.5f;

    /// <summary>Check left stick as a button for down press</summary>
    public bool IsButtonPressedLeftStickDown(int player) => CurrentState.padState[player].ThumbSticks.Left.Y < -0.5f && LastState.padState[player].ThumbSticks.Left.Y >= -0.5f;

    /// <summary>Check left stick as a button for left press</summary>
    public bool IsButtonPressedLeftStickLeft(int player) => CurrentState.padState[player].ThumbSticks.Left.X < -0.5f && LastState.padState[player].ThumbSticks.Left.X >= -0.5f;

    /// <summary>Check left stick as a button for right press</summary>
    public bool IsButtonPressedLeftStickRight(int player) => CurrentState.padState[player].ThumbSticks.Left.X > 0.5f && LastState.padState[player].ThumbSticks.Left.X <= 0.5f;
}

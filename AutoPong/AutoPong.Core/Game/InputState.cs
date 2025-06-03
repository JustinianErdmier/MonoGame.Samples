using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace AutoPong.Core.Game;

/// <summary>
///     Helper for reading input from keyboard, gamepad, and touch input. This class tracks both the current and previous state of the input devices, and implements query methods
///     for high level input actions such as "move up through the menu" or "pause the game".
/// </summary>
public class InputState
{
    /// <summary>Maximum number of supported input devices (e.g., players).</summary>
    private const int MaxInputs = 4;

    /// <summary>Cursor move speed in pixels per second</summary>
    private const float CursorMoveSpeed = 250.0f;

    // ReSharper disable once CollectionNeverQueried.Local
    /// <summary>Stores touch gestures.</summary>
    private readonly List<GestureSample> _gestures = [];

    /// <summary>Current input states - tracks the latest state of all input devices.</summary>
    public readonly GamePadState[] CurrentGamePadStates;

    // ReSharper disable once MemberCanBePrivate.Global
    public readonly KeyboardState[] CurrentKeyboardStates;

    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>Last input states - Stores the previous frame's input states for detecting changes.</summary>
    public readonly GamePadState[] LastGamePadStates;

    // ReSharper disable once MemberCanBePrivate.Global
    public readonly KeyboardState[] LastKeyboardStates;

    private Vector2 _currentCursorLocation;

    private MouseState _currentMouseState;

    /// <summary>Used to transform input coordinates between screen and game space.</summary>
    private Matrix _inputTransformation;

    private MouseState _lastMouseState;

    /// <summary>Number of active touch inputs.</summary>
    private int _touchCount;

    // ReSharper disable once MemberCanBePrivate.Global
    public TouchCollection CurrentTouchState;

    public TouchCollection LastTouchState;

    /// <summary>Constructs a new input state.</summary>
    public InputState()
    {
        // Initialize arrays for multiple controller/keyboard states
        CurrentKeyboardStates = new KeyboardState[MaxInputs];
        CurrentGamePadStates  = new GamePadState[MaxInputs];

        LastKeyboardStates = new KeyboardState[MaxInputs];
        LastGamePadStates  = new GamePadState[MaxInputs];
    }

    /// <summary>Current location of our cursor.</summary>
    private Vector2 CurrentCursorLocation => _currentCursorLocation;

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    /// <summary>Last location of our cursor.</summary>
    public Vector2 LastCursorLocation { get; private set; }

    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>Has the user scrolled the mouse wheel down?</summary>
    public bool IsMouseWheelScrolledDown { get; private set; }

    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>Has the user scrolled the mouse wheel up?</summary>
    public bool IsMouseWheelScrolledUp { get; private set; }

    public bool IsNewKeyPress(Keys key, PlayerIndex? controllingPlayer = null)
    {
        // Accept input from any player.
        if (!controllingPlayer.HasValue)
        {
            return IsNewKeyPress(key, PlayerIndex.One)
                   || IsNewKeyPress(key, PlayerIndex.Two)
                   || IsNewKeyPress(key, PlayerIndex.Three)
                   || IsNewKeyPress(key, PlayerIndex.Four);
        }

        // Read input from the specified player.
        PlayerIndex playerIndex = controllingPlayer.Value;

        int index = (int)playerIndex;

        return CurrentKeyboardStates[index].IsKeyDown(key) && LastKeyboardStates[index].IsKeyUp(key);
    }

    /// <summary>Reads the latest state of all the inputs.</summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    /// <param name="viewport">The viewport to constrain cursor movement within.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void Update(GameTime gameTime, Viewport viewport)
    {
        // Update the keyboard and gamepad states for all players.
        for (int index = 0; index < MaxInputs; index++)
        {
            LastKeyboardStates[index] = CurrentKeyboardStates[index];
            LastGamePadStates[index]  = CurrentGamePadStates[index];

            CurrentKeyboardStates[index] = Keyboard.GetState();
            CurrentGamePadStates[index]  = GamePad.GetState((PlayerIndex)index);
        }

        // Update the mouse state.
        _lastMouseState    = _currentMouseState;
        _currentMouseState = Mouse.GetState();

        // Update the touch state.
        _touchCount       = 0;
        LastTouchState    = CurrentTouchState;
        CurrentTouchState = TouchPanel.GetState();

        // Process all available gestures.
        _gestures.Clear();

        while (TouchPanel.IsGestureAvailable)
        {
            _gestures.Add(TouchPanel.ReadGesture());
        }

        // Process the touch inputs.
        foreach (TouchLocation location in CurrentTouchState)
        {
            switch (location.State)
            {
                case TouchLocationState.Pressed:
                    _touchCount++;

                    LastCursorLocation = _currentCursorLocation;

                    // Transform the touch position to the game coordinates.
                    _currentCursorLocation = TransformCursorLocation(location.Position);

                    break;

                case TouchLocationState.Moved:
                case TouchLocationState.Released:
                case TouchLocationState.Invalid:
                default:
#pragma warning disable CA2208
                    throw new ArgumentOutOfRangeException();
#pragma warning restore CA2208
            }
        }

        // Handle the mouse clicks as touch equivalents.
        if (IsLeftMouseButtonClicked())
        {
            LastCursorLocation = _currentCursorLocation;

            // Transform the mouse position to the game coordinates.
            _currentCursorLocation = TransformCursorLocation(new Vector2(_currentMouseState.X, _currentMouseState.Y));
            _touchCount            = 1;
        }

        // Treat the middle mouse click as a double touch.
        if (IsMiddleMouseButtonClicked())
        {
            _touchCount = 2;
        }

        // Treat the right mouse click as a triple touch.
        if (IsRightMouseButtonClicked())
        {
            _touchCount = 3;
        }

        // Reset the mouse wheel flags.
        IsMouseWheelScrolledUp   = false;
        IsMouseWheelScrolledDown = false;

        // Detect the mouse wheel scrolling.
        if (_currentMouseState.ScrollWheelValue != _lastMouseState.ScrollWheelValue)
        {
            int scrollWheelDelta = _currentMouseState.ScrollWheelValue - _lastMouseState.ScrollWheelValue;

            // Handle the scroll-wheel event based on the delta.
            switch (scrollWheelDelta)
            {
                // Mouse wheel scrolled up
                case > 0:
                    IsMouseWheelScrolledUp = true;

                    break;

                // Mouse wheel scrolled down
                case < 0:
                    IsMouseWheelScrolledDown = true;

                    break;
            }
        }

        // Update the cursor location using the gamepad and keyboard.
        float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Move the cursor with the gamepad thumbstick.
        if (CurrentGamePadStates[0].IsConnected)
        {
            LastCursorLocation = _currentCursorLocation;

            _currentCursorLocation.X += CurrentGamePadStates[0].ThumbSticks.Left.X * elapsedTime * CursorMoveSpeed;
            _currentCursorLocation.Y -= CurrentGamePadStates[0].ThumbSticks.Left.Y * elapsedTime * CursorMoveSpeed;
        }

        // Move the cursor with the keyboard arrow keys.
        if (CurrentKeyboardStates[0].IsKeyDown(Keys.Up))
        {
            _currentCursorLocation.Y -= elapsedTime * CursorMoveSpeed;
        }

        if (CurrentKeyboardStates[0].IsKeyDown(Keys.Down))
        {
            _currentCursorLocation.Y += elapsedTime * CursorMoveSpeed;
        }

        if (CurrentKeyboardStates[0].IsKeyDown(Keys.Left))
        {
            _currentCursorLocation.X -= elapsedTime * CursorMoveSpeed;
        }

        if (CurrentKeyboardStates[0].IsKeyDown(Keys.Right))
        {
            _currentCursorLocation.X += elapsedTime * CursorMoveSpeed;
        }

        // Keep the cursor within the viewport bounds.
        _currentCursorLocation.X = MathHelper.Clamp(_currentCursorLocation.X, min: 0f, viewport.Width);
        _currentCursorLocation.Y = MathHelper.Clamp(_currentCursorLocation.Y, min: 0f, viewport.Height);
    }

    /// <summary>Checks if the left mouse button was clicked (pressed and then released).</summary>
    /// <returns><c>TRUE</c> if the left mouse button was clicked, <c>FALSE</c> otherwise.</returns>
    private bool IsLeftMouseButtonClicked() => _currentMouseState.LeftButton == ButtonState.Released && _lastMouseState.LeftButton == ButtonState.Pressed;

    /// <summary>Checks if the middle mouse button was clicked (pressed and then released).</summary>
    /// <returns><c>TRUE</c> if middle mouse button was clicked, <c>FALSE</c> otherwise.</returns>
    private bool IsMiddleMouseButtonClicked() => _currentMouseState.MiddleButton == ButtonState.Released && _lastMouseState.MiddleButton == ButtonState.Pressed;

    /// <summary>Checks if the right mouse button was clicked (pressed and then released).</summary>
    /// <returns><c>TRUE</c> if right mouse button was clicked, <c>FALSE</c> otherwise.</returns>
    private bool IsRightMouseButtonClicked() => _currentMouseState.RightButton == ButtonState.Released && _lastMouseState.RightButton == ButtonState.Pressed;

    /// <summary>Helper for checking if a key was newly pressed during this update.</summary>
    /// <param name="key">The key to check.</param>
    /// <param name="controllingPlayer">The player to read input for, or null for any player.</param>
    /// <param name="playerIndex">Outputs which player pressed the key.</param>
    /// <returns>True if the key was newly pressed, false otherwise.</returns>
    private bool IsNewKeyPress(Keys key, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
    {
        // Accept input from any player.
        if (!controllingPlayer.HasValue)
        {
            return IsNewKeyPress(key, PlayerIndex.One, out playerIndex)
                   || IsNewKeyPress(key, PlayerIndex.Two, out playerIndex)
                   || IsNewKeyPress(key, PlayerIndex.Three, out playerIndex)
                   || IsNewKeyPress(key, PlayerIndex.Four, out playerIndex);
        }

        // Read input from the specified player.
        playerIndex = controllingPlayer.Value;

        int i = (int)playerIndex;

        return CurrentKeyboardStates[i].IsKeyDown(key) && LastKeyboardStates[i].IsKeyUp(key);
    }


    /// <summary>Helper for checking if a button was newly pressed during this update.</summary>
    /// <param name="button">The button to check.</param>
    /// <param name="controllingPlayer">The player to read input for, or null for any player.</param>
    /// <param name="playerIndex">Outputs which player pressed the button.</param>
    /// <returns>True if the button was newly pressed, false otherwise.</returns>
    private bool IsNewButtonPress(Buttons         button,
                                  PlayerIndex?    controllingPlayer,
                                  out PlayerIndex playerIndex)
    {
        // Accept input from any player.
        if (!controllingPlayer.HasValue)
        {
            return IsNewButtonPress(button, PlayerIndex.One, out playerIndex)
                   || IsNewButtonPress(button, PlayerIndex.Two, out playerIndex)
                   || IsNewButtonPress(button, PlayerIndex.Three, out playerIndex)
                   || IsNewButtonPress(button, PlayerIndex.Four, out playerIndex);
        }

        // Read input from the specified player.
        playerIndex = controllingPlayer.Value;

        int i = (int)playerIndex;

        return CurrentGamePadStates[i].IsButtonDown(button) && LastGamePadStates[i].IsButtonUp(button);
    }

    // ReSharper disable once UnusedMember.Global
    /// <summary>Checks for a "menu select" input action.</summary>
    /// <param name="controllingPlayer">The player to read input for, or null for any player.</param>
    /// <param name="playerIndex">Outputs which player triggered the action.</param>
    /// <returns><c>TRUE</c> if a menu select action occurred, <c>FALSE</c> otherwise.</returns>
    public bool IsMenuSelect(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        => IsNewKeyPress(Keys.Space, controllingPlayer, out playerIndex)
           || IsNewKeyPress(Keys.Enter, controllingPlayer, out playerIndex)
           || IsNewButtonPress(Buttons.A, controllingPlayer, out playerIndex)
           || IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);

    // ReSharper disable once UnusedMember.Global
    /// <summary>Checks for a "menu cancel" input action.</summary>
    /// <param name="controllingPlayer">The player to read input for, or null for any player.</param>
    /// <param name="playerIndex">Outputs which player triggered the action.</param>
    /// <returns><c>TRUE</c> if a menu cancel action occurred, <c>FALSE</c> otherwise.</returns>
    public bool IsMenuCancel(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        => IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex)
           || IsNewButtonPress(Buttons.B, controllingPlayer, out playerIndex)
           || IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex);

    // ReSharper disable once UnusedMember.Global
    /// <summary>Checks for a "menu up" input action.</summary>
    /// <param name="controllingPlayer">The player to read input for, or null for any player.</param>
    /// <returns><c>TRUE</c> if menu up action occurred, <c>FALSE</c> otherwise.</returns>
    public bool IsMenuUp(PlayerIndex? controllingPlayer)
        => IsNewKeyPress(Keys.Up, controllingPlayer, out PlayerIndex _)
           || IsNewButtonPress(Buttons.DPadUp, controllingPlayer, out PlayerIndex _)
           || IsNewButtonPress(Buttons.LeftThumbstickUp, controllingPlayer, out PlayerIndex _)
           || IsMouseWheelScrolledUp;

    // ReSharper disable once UnusedMember.Global
    /// <summary>Checks for a "menu down" input action.</summary>
    /// <param name="controllingPlayer">The player to read input for, or null for any player.</param>
    /// <returns><c>TRUE</c> if menu down action occurred, <c>FALSE</c> otherwise.</returns>
    public bool IsMenuDown(PlayerIndex? controllingPlayer)
        => IsNewKeyPress(Keys.Down, controllingPlayer, out PlayerIndex _)
           || IsNewButtonPress(Buttons.DPadDown, controllingPlayer, out PlayerIndex _)
           || IsNewButtonPress(Buttons.LeftThumbstickDown, controllingPlayer, out PlayerIndex _)
           || IsMouseWheelScrolledDown;

    // ReSharper disable once UnusedMember.Global
    /// <summary>Checks for a "pause the game" input action.</summary>
    /// <param name="controllingPlayer">The player to read input for, or null for any player.</param>
    /// <param name="rectangle">Optional rectangle to check for clicks within.</param>
    /// <returns><c>TRUE</c> if pause action occurred, <c>FALSE</c> otherwise.</returns>
    public bool IsPauseGame(PlayerIndex? controllingPlayer, Rectangle? rectangle = null)
    {
        // Check if the cursor is in the provided rectangle and was clicked
        bool pointInRect = rectangle.HasValue
                           && rectangle.Value.Contains(CurrentCursorLocation)
                           && (IsLeftMouseButtonClicked()
                               || _touchCount > 0);


        return IsNewKeyPress(Keys.Escape, controllingPlayer, out PlayerIndex _)
               || IsNewButtonPress(Buttons.Back, controllingPlayer, out PlayerIndex _)
               || IsNewButtonPress(Buttons.Start, controllingPlayer, out PlayerIndex _)
               || pointInRect;
    }

    // ReSharper disable once UnusedMember.Global
    /// <summary>Checks if the player has selected next on either keyboard or gamepad.</summary>
    /// <param name="controllingPlayer">The player to read input for, or null for any player.</param>
    /// <returns><c>TRUE</c> if select next action occurred, <c>FALSE</c> otherwise.</returns>
    public bool IsSelectNext(PlayerIndex? controllingPlayer)
        => IsNewKeyPress(Keys.Right, controllingPlayer, out PlayerIndex _)
           || IsNewButtonPress(Buttons.DPadRight, controllingPlayer, out PlayerIndex _);

    // ReSharper disable once UnusedMember.Global
    /// <summary>Checks if player has selected previous on either keyboard or gamepad.</summary>
    /// <param name="controllingPlayer">The player to read input for, or null for any player.</param>
    /// <returns><c>TRUE</c> if select previous action occurred, <c>FALSE</c> otherwise.</returns>
    public bool IsSelectPrevious(PlayerIndex? controllingPlayer)
        => IsNewKeyPress(Keys.Left, controllingPlayer, out PlayerIndex _)
           || IsNewButtonPress(Buttons.DPadLeft, controllingPlayer, out PlayerIndex _);

    // ReSharper disable once UnusedMember.Global
    /// <summary>Updates the matrix used to transform input coordinates.</summary>
    /// <param name="inputTransformation">The transformation matrix to apply.</param>
    internal void UpdateInputTransformation(Matrix inputTransformation) => _inputTransformation = inputTransformation;

    /// <summary>Transforms touch/mouse positions from screen space to game space.</summary>
    /// <param name="mousePosition">The screen-space position to transform.</param>
    /// <returns>The transformed position in game space.</returns>
    private Vector2 TransformCursorLocation(Vector2 mousePosition)
        =>

            // Transform back to the cursor location.
            Vector2.Transform(mousePosition, _inputTransformation);

    // ReSharper disable once UnusedMember.Global
    /// <summary>Checks if a UI element was clicked, either by mouse or touch.</summary>
    /// <param name="rectangle">The rectangle bounds of the UI element to check.</param>
    /// <returns><c>TRUE</c> if the UI element was clicked, <c>FALSE</c> otherwise.</returns>
    internal bool IsUiClicked(Rectangle rectangle)
        => rectangle.Contains(CurrentCursorLocation)
           && (IsLeftMouseButtonClicked()
               || _touchCount > 0);
}

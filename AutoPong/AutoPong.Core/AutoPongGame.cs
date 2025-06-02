using System;

using AutoPong.Core.Game;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AutoPong.Core;

public class AutoPongGame : Microsoft.Xna.Framework.Game
{
    private const float BallSpeed = 15.0f;

    private const int PointsPerGame = 4;

    // Window resolution.
    private readonly Point _gameBounds = new(x: 1280, y: 720);

    private readonly GraphicsDeviceManager _graphics;

    private readonly Random _rand = new();

    private Rectangle _ball;

    private Vector2 _ballPosition;

    private Vector2 _ballVelocity;

    private byte _hitCounter;

    private int _jingleCounter;

    private Rectangle _paddleLeft;

    private Rectangle _paddleRight;

    private int _pointsLeft;

    private int _pointsRight;

    private AudioSource _soundFx;

    private SpriteBatch _spriteBatch;

    private Texture2D _texture;

    public AutoPongGame()
    {
        _graphics = new GraphicsDeviceManager(this);

        _graphics.PreferredBackBufferWidth  = _gameBounds.X;
        _graphics.PreferredBackBufferHeight = _gameBounds.Y;

        IsMouseVisible = true;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        Reset();
    }

    protected override void Update(GameTime gameTime)
    {
        if (!OperatingSystem.IsIOS()
            && (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape)))
        {
            Exit();
        }

        UpdateBall();

        SimulateLeftPaddleInput();

        SimulateRightPaddleInput();

        CheckWin();

        PlayResetJingle();

        base.Update(gameTime);
    }

    private void UpdateBall()
    {
        // Limit how fast the ball can move each frame.
        const float maxVelocity = 1.5f;

        _ballVelocity.X = _ballVelocity.X switch
        {
            > maxVelocity  => maxVelocity,
            < -maxVelocity => -maxVelocity,
            var _          => _ballVelocity.X
        };

        _ballVelocity.Y = _ballVelocity.Y switch
        {
            > maxVelocity  => maxVelocity,
            < -maxVelocity => -maxVelocity,
            var _          => _ballVelocity.Y
        };

        // Apply the velocity to the position.
        _ballPosition.X += _ballVelocity.X * BallSpeed;
        _ballPosition.Y += _ballVelocity.Y * BallSpeed;

        // Check for collision with the paddles.
        _hitCounter++;

        if (_hitCounter > 10
            && _paddleLeft.Intersects(_ball))
        {
            _ballVelocity.X *= -1;
            _ballVelocity.Y *= 1.1f;
            _hitCounter     =  0;
            _ballPosition.X =  _paddleLeft.X + _paddleLeft.Width + 10;

            _soundFx.PlayWave(frequency: 220.0f, duration: 50, WaveType.Sin, volume: 0.3f);
        }

        if (_hitCounter > 10
            && _paddleRight.Intersects(_ball))
        {
            _ballVelocity.X *= -1;
            _ballVelocity.Y *= 1.1f;
            _hitCounter     =  0;
            _ballPosition.X =  _paddleRight.X - 10;

            _soundFx.PlayWave(frequency: 220.0f, duration: 50, WaveType.Sin, volume: 0.3f);
        }

        // Bounce off the screen.
        // TODO: Does this need to be an `else if`? Will both conditions ever be true at the same time?
        if (_ballPosition.X < 0)
        {
            // Point to the right.
            _ballPosition.X =  1;
            _ballVelocity.X *= -1;
            _pointsRight++;
            _soundFx.PlayWave(frequency: 440.0f, duration: 50, WaveType.Square, volume: 0.3f);
        }
        else if (_ballPosition.X > _gameBounds.X)
        {
            // Point to the left.
            _ballPosition.X =  _gameBounds.X - 1;
            _ballVelocity.X *= -1;
            _pointsLeft++;

            _soundFx.PlayWave(frequency: 440.0f, duration: 50, WaveType.Square, volume: 0.3f);
        }

        // Bounce off the top and bottom.
        // TODO: Does this need to be an `else if`? Will both conditions ever be true at the same time?
        if (_ballPosition.Y < 0 + 10)
        {
            // Limit to the minimum Y position.
            _ballPosition.Y =  10 + 1;
            _ballVelocity.Y *= -(1 + _rand.Next(minValue: -100, maxValue: 101) * 0.005f);
        }
        else if (_ballPosition.Y > _gameBounds.Y - 10)
        {
            // Limit to the maximum Y position.
            _ballPosition.Y =  _gameBounds.Y - 11;
            _ballVelocity.Y *= -(1 + _rand.Next(minValue: -100, maxValue: 101) * 0.005f);
        }
    }

    private void SimulateLeftPaddleInput()
    {
        // Simple AI - not very good; moves a random amount each frame.
        int amount       = _rand.Next(minValue: 0, maxValue: 6);
        int paddleCenter = _paddleLeft.Y + _paddleLeft.Height / 2;

        // TODO: Does this need to be an `else if`? Will both conditions ever be true at the same time?
        if (paddleCenter < _ballPosition.Y - 20)
        {
            _paddleLeft.Y += amount;
        }
        else if (paddleCenter > _ballPosition.Y + 20)
        {
            _paddleLeft.Y -= amount;
        }

        LimitPaddle(ref _paddleLeft);
    }

    private void SimulateRightPaddleInput()
    {
        // Simple AI - better than the left; moves % each frame
        int paddleCenter = _paddleRight.Y + _paddleRight.Height / 2;

        // TODO: Does this need to be an `else if`? Will both conditions ever be true at the same time?
        if (paddleCenter < _ballPosition.Y - 20)
        {
            _paddleRight.Y -= (int)((paddleCenter - _ballPosition.Y) * 0.08f);
        }
        else if (paddleCenter > _ballPosition.Y + 20)
        {
            _paddleRight.Y += (int)((_ballPosition.Y - paddleCenter) * 0.08f);
        }

        LimitPaddle(ref _paddleRight);
    }

    private void CheckWin()
    {
        // Check for win condition and reset.
        if (_pointsLeft >= PointsPerGame)
        {
            Reset();

            return;
        }

        if (_pointsRight >= PointsPerGame)
        {
            Reset();
        }
    }

    private void PlayResetJingle()
    {
        // Use the jingle counter as a timeline to play notes.
        _jingleCounter++;

        const int speed = 7;

        switch (_jingleCounter)
        {
            case speed * 1:
                _soundFx.PlayWave(frequency: 440.0f, duration: 100, WaveType.Sin, volume: 0.2f);

                break;

            case speed * 2:
                _soundFx.PlayWave(frequency: 523.25f, duration: 100, WaveType.Sin, volume: 0.2f);

                break;

            case speed * 3:
                _soundFx.PlayWave(frequency: 659.25f, duration: 100, WaveType.Sin, volume: 0.2f);

                break;

            case speed * 4:
                _soundFx.PlayWave(frequency: 783.99f, duration: 100, WaveType.Sin, volume: 0.2f);

                break;

            // Only play this jingle once.
            case > speed * 4:
                _jingleCounter = int.MaxValue - 1;

                break;
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

        // Draw dots down the center.
        DrawCenterLine();

        // Draw the paddles.
        DrawPaddles();

        // draw the ball.
        DrawBall();

        // TODO: What does this do?
        DrawRectangle(_spriteBatch, _ball, Color.White);

        // Draw the current game points.
        DrawGamePoints();

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawCenterLine()
    {
        int total = _gameBounds.Y / 20;

        for (int index = 0; index < total; index++)
        {
            DrawRectangle(_spriteBatch, new Rectangle(_gameBounds.X / 2 - 4, 5 + index * 20, width: 8, height: 8), Color.White * 0.2f);
        }
    }

    private void DrawPaddles()
    {
        DrawRectangle(_spriteBatch, _paddleLeft, Color.White);
        DrawRectangle(_spriteBatch, _paddleRight, Color.White);
    }

    private void DrawBall()
    {
        _ball.X = (int)_ballPosition.X;
        _ball.Y = (int)_ballPosition.Y;
    }

    private void DrawGamePoints()
    {
        for (int index = 0; index < _pointsLeft; index++)
        {
            DrawRectangle(_spriteBatch, new Rectangle(_gameBounds.X / 2 - 25 - index * 12, y: 10, width: 10, height: 10), Color.White * 1.0f);
        }

        for (int index = 0; index < _pointsRight; index++)
        {
            DrawRectangle(_spriteBatch, new Rectangle(_gameBounds.X / 2 + 15 + index * 12, y: 10, width: 10, height: 10), Color.White * 1.0f);
        }
    }

    private void DrawRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color)
    {
        Vector2 position = new(rectangle.X, rectangle.Y);

        spriteBatch.Draw(_texture,
                         position,
                         rectangle,
                         color * 1.0f,
                         rotation: 0,
                         Vector2.Zero,
                         scale: 1.0f,
                         SpriteEffects.None,
                         layerDepth: 0.00001f);
    }

    private void LimitPaddle(ref Rectangle paddle)
    {
        // Limit how far the paddles can travel on the Y axis so they don't exceed the top or bottom.
        if (paddle.Y < 10)
        {
            paddle.Y = 10;

            return;
        }

        if (paddle.Y + paddle.Height > _gameBounds.Y - 10)
        {
            paddle.Y = _gameBounds.Y - 10 - paddle.Height;
        }
    }

    private void Reset()
    {
        // Create the texture with which to draw if it does not exist.
        if (_texture is null)
        {
            _texture = new Texture2D(_graphics.GraphicsDevice, width: 1, height: 1);

            _texture.SetData([Color.White]);
        }

        const int paddleHeight = 100;

        _paddleLeft  = new Rectangle(0             + 10, y: 150, width: 20, paddleHeight);
        _paddleRight = new Rectangle(_gameBounds.X - 30, y: 150, width: 20, paddleHeight);

        // ReSharper disable once PossibleLossOfFraction
        _ballPosition = new Vector2(_gameBounds.X / 2, y: 200);
        _ball         = new Rectangle((int)_ballPosition.X, (int)_ballPosition.Y, width: 10, height: 10);
        _ballVelocity = new Vector2(x: 1, y: 0.1f);

        _pointsLeft    = 0;
        _pointsRight   = 0;
        _jingleCounter = 0;

        // Set up the sound sources
        _soundFx ??= new AudioSource();
    }
}

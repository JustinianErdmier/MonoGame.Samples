using System;
using System.Diagnostics;

using AutoPong.Core.Game.Core;
using AutoPong.Core.Game.Enums;

using Microsoft.Xna.Framework;

namespace AutoPong.Core.Game.Entities;

public sealed class Paddle
{
    private const int PaddleHeight = 100;

    private static readonly Rectangle InitialLeftPaddleBody = new(x: 10, y: 150, width: 20, PaddleHeight);

    private static readonly Rectangle InitialRightPaddleBody = new(GameOptions.WindowResolution.X - 30, y: 150, width: 20, PaddleHeight);

    private Rectangle _body;

    public Paddle(PaddleLocations location)
    {
        Location = location;

        Initialize();
    }

    public PaddleLocations Location { get; }

    public int Score { get; private set; }

    public bool HasScore => Score > 0;

    // ReSharper disable once UnusedMember.Global
    public int Height => _body.Height;

    public int Width => _body.Width;

    public int X => _body.X;

    // ReSharper disable once UnusedMember.Global
    public int Y => _body.Y;

    public Rectangle Body => _body;

    public event EventHandler? WonGame;

    public void IncrementScore() => Score++;

    public void Initialize()
    {
        Score = 0;

        _body = Location switch
        {
            PaddleLocations.Left  => InitialLeftPaddleBody,
            PaddleLocations.Right => InitialRightPaddleBody,
            var _                 => throw new UnreachableException()
        };
    }

    public bool IsIntersectingWithBall(Ball ball) => _body.Intersects(ball.Body);

    public void Update(Ball ball)
    {
        SimulatePaddleInput(ball);

        if (IsWinner())
        {
            WonGame?.Invoke(this, EventArgs.Empty);
        }
    }

    private bool IsWinner() => Score >= GameOptions.MaxPointsPerGame;

    /// <summary>Limit how far the paddles can travel on the Y axis so they don't exceed the top or bottom.</summary>
    private void LimitPaddle()
    {
        if (_body.Y < 10)
        {
            _body.Y = 10;

            return;
        }

        if (_body.Y + _body.Height > GameOptions.WindowResolution.Y - 10)
        {
            _body.Y = GameOptions.WindowResolution.Y - 10 - _body.Height;
        }
    }

    private void SimulatePaddleInput(Ball ball)
    {
        switch (Location)
        {
            case PaddleLocations.Left:
                SimulateLeftPaddleInput(ball);

                break;

            case PaddleLocations.Right:
                SimulateRightPaddleInput(ball);

                break;

            default:
#pragma warning disable CA2208
                throw new ArgumentOutOfRangeException();
#pragma warning restore CA2208
        }

        LimitPaddle();
    }

    private void SimulateLeftPaddleInput(Ball ball)
    {
        // Simple AI - not very good; moves a random amount each frame.
        int amount       = AutoPongGame.Rand.Next(minValue: 0, maxValue: 6);
        int paddleCenter = _body.Y + _body.Height / 2;

        // TODO: Does this need to be an `else if`? Will both conditions ever be true at the same time?
        if (paddleCenter < ball.PositionY - 20)
        {
            _body.Y += amount;
        }
        else if (paddleCenter > ball.PositionY + 20)
        {
            _body.Y -= amount;
        }
    }

    private void SimulateRightPaddleInput(Ball ball)
    {
        // Simple AI - better than the left; moves % each frame
        int paddleCenter = _body.Y + _body.Height / 2;

        // TODO: Does this need to be an `else if`? Will both conditions ever be true at the same time?
        if (paddleCenter < ball.PositionY - 20)
        {
            _body.Y -= (int)((paddleCenter - ball.PositionY) * 0.08f);
        }
        else if (paddleCenter > ball.PositionY + 20)
        {
            _body.Y += (int)((ball.PositionY - paddleCenter) * 0.08f);
        }
    }
}

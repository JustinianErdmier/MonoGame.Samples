using System.Collections.Generic;

using AutoPong.Core.Game.Core;
using AutoPong.Core.Game.Enums;
using AutoPong.Core.Game.Services;

using Microsoft.Xna.Framework;

namespace AutoPong.Core.Game.Entities;

public sealed class Ball
{
    private const float MaxVelocity = 1.5f;

    private const float Speed = 15.0f;

    private static readonly Vector2 InitialVelocity = new(x: 1, y: 0.1f);

    // ReSharper disable once PossibleLossOfFraction
    private static readonly Vector2 InitialPosition = new(GameOptions.WindowResolution.X / 2, y: 200);

    private static readonly Rectangle InitialBall = new((int)InitialPosition.X, (int)InitialPosition.Y, width: 10, height: 10);

    private readonly List<Paddle> _paddles;

    private readonly SoundService _soundService;

    private Rectangle _ball = InitialBall;

    private Vector2 _ballPosition = InitialPosition;

    private Vector2 _ballVelocity = InitialVelocity;

    private byte _hitCounter;

    public Ball(List<Paddle> paddles, SoundService soundService)
    {
        _paddles      = paddles;
        _soundService = soundService;
    }

    public Rectangle BallRectangle => _ball;

    // ReSharper disable once UnusedMember.Global
    public float PositionX => _ballPosition.X;

    public float PositionY => _ballPosition.Y;

    // ReSharper disable once UnusedMember.Global
    public float VelocityX => _ballVelocity.X;

    // ReSharper disable once UnusedMember.Global
    public float VelocityY => _ballVelocity.Y;

    public void Draw()
    {
        _ball.X = (int)_ballPosition.X;
        _ball.Y = (int)_ballPosition.Y;
    }

    public void Initialize()
    {
        _ball         = InitialBall;
        _ballPosition = InitialPosition;
        _ballVelocity = InitialVelocity;
    }

    public void Update()
    {
        _ballVelocity.X = _ballVelocity.X switch
        {
            > MaxVelocity  => MaxVelocity,
            < -MaxVelocity => -MaxVelocity,
            var _          => _ballVelocity.X
        };

        _ballVelocity.Y = _ballVelocity.Y switch
        {
            > MaxVelocity  => MaxVelocity,
            < -MaxVelocity => -MaxVelocity,
            var _          => _ballVelocity.Y
        };

        // Apply the velocity to the position.
        _ballPosition.X += _ballVelocity.X * Speed;
        _ballPosition.Y += _ballVelocity.Y * Speed;

        // Check for collision with the paddles.
        _hitCounter++;

        foreach (Paddle paddle in _paddles)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (paddle.Location)
            {
                case PaddleLocations.Left:
                    UpdateWithLeftPaddle(paddle);

                    break;

                case PaddleLocations.Right:
                    UpdateWithRightPaddle(paddle);

                    break;
            }
        }

        // Limit to the minimum Y position.
        // TODO: Why are we doing `0 + 10` and not just `10`?
        if (_ballPosition.Y < 0 + 10)
        {
            // TODO: Why are we doing `10 + 1` and not just `11`?
            _ballPosition.Y =  10 + 1;
            _ballVelocity.Y *= -(1 + AutoPongGame.Rand.Next(minValue: -100, maxValue: 101) * 0.005f);
        }

        // Limit to the maximum Y position.
        else if (_ballPosition.Y > GameOptions.WindowResolution.Y - 10)
        {
            _ballPosition.Y =  GameOptions.WindowResolution.Y - 11;
            _ballVelocity.Y *= -(1 + AutoPongGame.Rand.Next(minValue: -100, maxValue: 101) * 0.005f);
        }
    }

    private void UpdateWithLeftPaddle(Paddle paddle)
    {
        if (_hitCounter > 10
            && paddle.IsIntersectingWithBall(this))
        {
            _ballVelocity.X *= -1;
            _ballVelocity.Y *= 1.1f;
            _hitCounter     =  0;
            _ballPosition.X =  paddle.X + paddle.Width + 10;

            _soundService.PlayBallHittingAPaddleSound();
        }

        if (!(_ballPosition.X > GameOptions.WindowResolution.X))
        {
            return;
        }

        _ballPosition.X =  GameOptions.WindowResolution.X - 1;
        _ballVelocity.X *= -1;

        paddle.IncrementScore();

        _soundService.PlayBallHittingTheLeftOrRightWallSound();
    }

    private void UpdateWithRightPaddle(Paddle paddle)
    {
        if (_hitCounter > 10
            && paddle.IsIntersectingWithBall(this))
        {
            _ballVelocity.X *= -1;
            _ballVelocity.Y *= 1.1f;
            _hitCounter     =  0;
            _ballPosition.X =  paddle.X - 10;

            _soundService.PlayBallHittingAPaddleSound();
        }

        if (!(_ballPosition.X < 0))
        {
            return;
        }

        _ballPosition.X =  1;
        _ballVelocity.X *= -1;

        paddle.IncrementScore();

        _soundService.PlayBallHittingTheLeftOrRightWallSound();
    }
}

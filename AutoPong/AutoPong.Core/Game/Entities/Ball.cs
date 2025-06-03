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

    // ReSharper disable once PossibleLossOfFraction
    private static readonly Vector2 InitialPosition = new(GameOptions.WindowResolution.X / 2, y: 200);

    private static readonly Rectangle InitialBody = new((int)InitialPosition.X, (int)InitialPosition.Y, width: 10, height: 10);

    private static readonly Vector2 InitialVelocity = new(x: 1, y: 0.1f);

    private readonly List<Paddle> _paddles;

    private readonly SoundService _soundService;

    private Rectangle _body = InitialBody;

    // TODO: What is this for?
    private byte _hitCounter;

    private Vector2 _position = InitialPosition;

    private Vector2 _velocity = InitialVelocity;

    public Ball(List<Paddle> paddles, SoundService soundService)
    {
        _paddles      = paddles;
        _soundService = soundService;
    }

    public Rectangle Body => _body;

    // ReSharper disable once UnusedMember.Global
    public float PositionX => _position.X;

    public float PositionY => _position.Y;

    // ReSharper disable once UnusedMember.Global
    public float VelocityX => _velocity.X;

    // ReSharper disable once UnusedMember.Global
    public float VelocityY => _velocity.Y;

    public void Draw()
    {
        _body.X = (int)_position.X;
        _body.Y = (int)_position.Y;
    }

    public void Initialize()
    {
        _body     = InitialBody;
        _position = InitialPosition;
        _velocity = InitialVelocity;
    }

    public void Update()
    {
        _velocity.X = _velocity.X switch
        {
            > MaxVelocity  => MaxVelocity,
            < -MaxVelocity => -MaxVelocity,
            var _          => _velocity.X
        };

        _velocity.Y = _velocity.Y switch
        {
            > MaxVelocity  => MaxVelocity,
            < -MaxVelocity => -MaxVelocity,
            var _          => _velocity.Y
        };

        // Apply the velocity to the position.
        _position.X += _velocity.X * Speed;
        _position.Y += _velocity.Y * Speed;

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

        LimitBall();
    }

    private void LimitBall()
    {
        // Limit to the minimum Y position.
        // TODO: Where does the number 10 come from?
        if (_position.Y < 10)
        {
            _position.Y =  11;
            _velocity.Y *= -(1 + AutoPongGame.Rand.Next(minValue: -100, maxValue: 101) * 0.005f);

            return;
        }

        // Limit to the maximum Y position.
        if (_position.Y > GameOptions.WindowResolution.Y - 10)
        {
            _position.Y =  GameOptions.WindowResolution.Y - 11;
            _velocity.Y *= -(1 + AutoPongGame.Rand.Next(minValue: -100, maxValue: 101) * 0.005f);
        }
    }

    private void UpdateWithLeftPaddle(Paddle paddle)
    {
        if (_hitCounter > 10
            && paddle.IsIntersectingWithBall(this))
        {
            _velocity.X *= -1;
            _velocity.Y *= 1.1f;
            _hitCounter =  0;
            _position.X =  paddle.X + paddle.Width + 10;

            _soundService.PlayBallHittingAPaddleSound();
        }

        if (!(_position.X > GameOptions.WindowResolution.X))
        {
            return;
        }

        _position.X =  GameOptions.WindowResolution.X - 1;
        _velocity.X *= -1;

        paddle.IncrementScore();

        _soundService.PlayBallHittingTheLeftOrRightWallSound();
    }

    private void UpdateWithRightPaddle(Paddle paddle)
    {
        if (_hitCounter > 10
            && paddle.IsIntersectingWithBall(this))
        {
            _velocity.X *= -1;
            _velocity.Y *= 1.1f;
            _hitCounter =  0;
            _position.X =  paddle.X - 10;

            _soundService.PlayBallHittingAPaddleSound();
        }

        if (!(_position.X < 0))
        {
            return;
        }

        _position.X =  1;
        _velocity.X *= -1;

        paddle.IncrementScore();

        _soundService.PlayBallHittingTheLeftOrRightWallSound();
    }
}

using AutoPong.Core.Game.Core;
using AutoPong.Core.Game.Entities;
using AutoPong.Core.Game.Enums;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AutoPong.Core.Game.Services;

public sealed class DrawingService
{
    private readonly GraphicsDevice _graphicsDevice;

    private readonly SpriteBatch? _spriteBatch;

    public DrawingService(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Texture2D texture)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch    = spriteBatch;

        Draw = new Drawer(this, spriteBatch, texture);
    }

    public Drawer Draw { get; }

    public DrawingService Begin()
    {
        _spriteBatch?.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

        return this;
    }

    public DrawingService Clear()
    {
        _graphicsDevice.Clear(GameOptions.Colors.Background);

        return this;
    }

    public void End() => _spriteBatch?.End();

    public sealed class Drawer
    {
        private readonly DrawingService _drawingService;

        private readonly SpriteBatch? _spriteBatch;

        private readonly Texture2D _texture;

        public Drawer(DrawingService drawingService, SpriteBatch? spriteBatch, Texture2D texture)
        {
            _drawingService = drawingService;
            _spriteBatch    = spriteBatch;
            _texture        = texture;
        }

        public DrawingService Ball(Ball ball)
        {
            ball.Draw();

            DrawRectangle(ball.BallRectangle, GameOptions.Colors.Ball);

            return _drawingService;
        }

        public DrawingService CenterLine()
        {
            int total = GameOptions.WindowResolution.Y / 20;

            for (int index = 0; index < total; index++)
            {
                DrawRectangle(new Rectangle(GameOptions.WindowResolution.X / 2 - 4, 5 + index * 20, width: 8, height: 8), GameOptions.Colors.CenterLine * 0.2f);
            }

            return _drawingService;
        }

        public DrawingService Paddles(params Paddle[] paddles)
        {
            foreach (Paddle paddle in paddles)
            {
                DrawRectangle(paddle.PaddleRectangle, GameOptions.Colors.Paddles);
            }

            return _drawingService;
        }

        public DrawingService ScorePoints(params Paddle[] paddles)
        {
            foreach (Paddle paddle in paddles)
            {
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (paddle.Location)
                {
                    case PaddleLocations.Left
                        when paddle.HasScore:
                    {
                        for (int index = 0; index < paddle.Score; index++)
                        {
                            DrawRectangle(new Rectangle(GameOptions.WindowResolution.X / 2 - 25 - index * 12, y: 10, width: 10, height: 10), GameOptions.Colors.ScorePoints * 1.0f);
                        }

                        break;
                    }

                    case PaddleLocations.Right
                        when paddle.HasScore:
                    {
                        for (int index = 0; index < paddle.Score; index++)
                        {
                            DrawRectangle(new Rectangle(GameOptions.WindowResolution.X / 2 + 15 + index * 12, y: 10, width: 10, height: 10), GameOptions.Colors.ScorePoints * 1.0f);
                        }

                        break;
                    }
                }
            }

            return _drawingService;
        }

        private void DrawRectangle(Rectangle rectangle, Color color)
        {
            Vector2 position = new(rectangle.X, rectangle.Y);

            _spriteBatch?.Draw(_texture,
                               position,
                               rectangle,
                               color * 1.0f,
                               rotation: 0,
                               Vector2.Zero,
                               scale: 1.0f,
                               SpriteEffects.None,
                               layerDepth: 0.00001f);
        }
    }
}

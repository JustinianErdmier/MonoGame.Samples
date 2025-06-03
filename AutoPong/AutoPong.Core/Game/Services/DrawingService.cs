using System.Collections.Generic;

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

    public DrawingService(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch    = spriteBatch;

        Draw = new Drawer(this, graphicsDevice, spriteBatch);
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
        private readonly Texture2D _circleTexture;

        private readonly DrawingService _drawingService;

        private readonly GraphicsDevice _graphicsDevice;

        private readonly Texture2D _rectangleTexture;

        private readonly SpriteBatch? _spriteBatch;

        public Drawer(DrawingService drawingService, GraphicsDevice graphicsDevice, SpriteBatch? spriteBatch)
        {
            _drawingService = drawingService;
            _graphicsDevice = graphicsDevice;
            _spriteBatch    = spriteBatch;

            _circleTexture    = CreateCircleTexture(diameter: 32);
            _rectangleTexture = CreateRectangleTexture(width: 1, height: 1);
        }

        public DrawingService Ball(Ball ball)
        {
            ball.Draw();

            DrawCircle(ball.Body, GameOptions.Colors.Ball);

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

        public DrawingService Paddles(List<Paddle> paddles)
        {
            foreach (Paddle paddle in paddles)
            {
                DrawRectangle(paddle.Body, GameOptions.Colors.Paddles);
            }

            return _drawingService;
        }

        public DrawingService ScorePoints(List<Paddle> paddles)
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

        private Texture2D CreateCircleTexture(int diameter)
        {
            Texture2D texture   = new(_graphicsDevice, diameter, diameter);
            Color[]   colorData = new Color[diameter * diameter];

            float radius        = diameter / 2f;
            float radiusSquared = radius   * radius;

            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    int     index    = y * diameter + x;
                    Vector2 position = new(x - radius + 0.5f, y - radius + 0.5f);

                    if (position.LengthSquared() <= radiusSquared)
                    {
                        colorData[index] = GameOptions.Colors.Texture;
                    }
                    else
                    {
                        colorData[index] = Color.Transparent;
                    }
                }
            }

            texture.SetData(colorData);

            return texture;
        }

        private Texture2D CreateRectangleTexture(int width, int height)
        {
            Texture2D texture = new(_graphicsDevice, width, height);

            texture.SetData([GameOptions.Colors.Texture]);

            return texture;
        }

        private void DrawCircle(Rectangle rectangle, Color color)
        {
            Vector2 center = new(rectangle.X + rectangle.Width / 2f, rectangle.Y + rectangle.Height / 2f);
            float   scale  = rectangle.Width / (float)_circleTexture.Width;
            ;

            _spriteBatch?.Draw(_circleTexture,
                               center,
                               sourceRectangle: null,
                               color * 1.0f,
                               rotation: 0,
                               new Vector2(_circleTexture.Width / 2f, _circleTexture.Height / 2f),
                               scale,
                               SpriteEffects.None,
                               layerDepth: 0.00001f);
        }

        private void DrawRectangle(Rectangle rectangle, Color color)
        {
            Vector2 position = new(rectangle.X, rectangle.Y);

            _spriteBatch?.Draw(_rectangleTexture,
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

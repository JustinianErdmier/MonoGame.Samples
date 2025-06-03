using Microsoft.Xna.Framework;

namespace AutoPong.Core.Game.Core;

public static class GameOptions
{
    public const bool IsMouseVisible = true;

    public const bool IsPausedByDefault = true;

    public const int MaxPointsPerGame = 4;

    public const bool SlowBallWhenNearPaddles = false;

    public static readonly Point WindowResolution = new(x: 1280, y: 720);

    public static class Colors
    {
        public static readonly Color Background = new(r: 192, g: 224, b: 222);

        public static readonly Color Ball = new(r: 54, g: 64, b: 68);

        public static readonly Color CenterLine = new(r: 22, g: 37, b: 33);

        public static readonly Color Paddles = new(r: 79, g: 124, b: 172);

        public static readonly Color ScorePoints = new(r: 54, g: 64, b: 68);

        public static readonly Color Texture = Color.White;
    }
}

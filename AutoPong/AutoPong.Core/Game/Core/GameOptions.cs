using Microsoft.Xna.Framework;

namespace AutoPong.Core.Game.Core;

public static class GameOptions
{
    public const bool IsMouseVisible = true;

    public const int MaxPointsPerGame = 4;

    public const bool SlowBallWhenNearPaddles = false;

    public static readonly Point WindowResolution = new(x: 1280, y: 720);

    public static class Colors
    {
        public static readonly Color Background = Color.CornflowerBlue;

        public static readonly Color Ball = Color.White;

        public static readonly Color CenterLine = Color.White;

        public static readonly Color Paddles = Color.White;

        public static readonly Color ScorePoints = Color.White;

        public static readonly Color Texture = Color.White;
    }
}

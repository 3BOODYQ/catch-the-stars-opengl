using System.Text;
using CatchStars.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace CatchStars;

internal sealed class SmokeCheck
{
    private int _frame;
    private GameStatus Status => _frame switch
        {
            < 4 => GameStatus.Ready,
            < 8 => GameStatus.Paused,
            < 12 => GameStatus.Won,
            < 16 => GameStatus.Lost,
            _ => GameStatus.Playing
        };
    public GameState State => GameRules.NewGame() with
    {
        Status = Status,
        Score = Status == GameStatus.Won ? GameRules.WinningScore : 0,
        Lives = Status == GameStatus.Lost ? 0 : GameRules.StartingLives,
        StarX = 480, StarY = 360
    };

    public void CheckFrame(CatchWindow window)
    {
        var error = GL.GetError();
        if (error != ErrorCode.NoError)
            throw new InvalidOperationException($"OpenGL smoke check failed: {error}");
        if (_frame == 16) window.ClientSize = new Vector2i(800, 800);
        if (_frame == 20)
        {
            var viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);
            // Whole-pixel viewport dimensions can round the ideal aspect ratio by one pixel.
            if (window.ClientSize != new Vector2i(800, 800)
                || Math.Abs(viewport[2] * GameRules.Height - viewport[3] * GameRules.Width)
                    > Math.Max(GameRules.Width, GameRules.Height))
                throw new InvalidOperationException("Resized viewport distorted the world.");
            window.ClientSize = new Vector2i(GameRules.Width, GameRules.Height);
        }
        if (_frame == 25)
        {
            Capture(window.FramebufferSize.X, window.FramebufferSize.Y);
            Console.WriteLine("PASS: all five screens, OpenGL drawing, aspect-preserving resize, and pixel checks.");
            window.Close();
        }
        _frame++;
    }

    private static void Capture(int width, int height)
    {
        var pixels = new byte[width * height * 4];
        GL.ReadPixels(0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
        static bool IsGold(byte r, byte g, byte b) => r > 220 && g > 150 && b < 100;
        static bool IsCyan(byte r, byte g, byte b) => r < 100 && g > 180 && b > 160;
        if (CountRegion(pixels, width, height, 464, 344, 32, 32, IsGold) < 100
            || CountRegion(pixels, width, height, 424, 56, 112, 18, IsCyan) < 100
            || CountRegion(pixels, width, height, 648, 572, 164, 30, IsGold) < 100)
            throw new InvalidOperationException("Rendered frame is missing the star, paddle, or HUD.");
        Directory.CreateDirectory("build");
        using var file = File.Create("build/catch-stars.ppm");
        file.Write(Encoding.ASCII.GetBytes($"P6\n{width} {height}\n255\n"));
        for (var y = height - 1; y >= 0; y--)
            for (var x = 0; x < width; x++)
                file.Write(pixels.AsSpan((y * width + x) * 4, 3));
        Console.WriteLine("Screenshot: build/catch-stars.ppm");
    }

    private static int CountRegion(byte[] pixels, int width, int height,
        float x, float y, float regionWidth, float regionHeight,
        Func<byte, byte, byte, bool> matches)
    {
        var scale = Math.Min(width / (float)GameRules.Width, height / (float)GameRules.Height);
        var left = (width - GameRules.Width * scale) / 2;
        var bottom = (height - GameRules.Height * scale) / 2;
        var count = 0;
        for (var row = (int)(bottom + y * scale); row < bottom + (y + regionHeight) * scale; row++)
            for (var column = (int)(left + x * scale); column < left + (x + regionWidth) * scale; column++)
            {
                var i = (row * width + column) * 4;
                if (matches(pixels[i], pixels[i + 1], pixels[i + 2])) count++;
            }
        return count;
    }
}

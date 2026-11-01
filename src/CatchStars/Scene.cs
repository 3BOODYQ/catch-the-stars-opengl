using CatchStars.Core;
using OpenTK.Mathematics;

namespace CatchStars;

internal static class Scene
{
    private static readonly Color4 White = new(0.88f, 0.93f, 0.98f, 1);
    private static readonly Color4 Muted = new(0.39f, 0.51f, 0.64f, 1);
    private static readonly Color4 Cyan = new(0.2f, 0.88f, 0.83f, 1);
    private static readonly Color4 Gold = new(1f, 0.76f, 0.26f, 1);

    public static void Draw(Canvas canvas, GameState state)
    {
        Background(canvas);
        Header(canvas, state);
        var x = state.PaddleX - GameRules.PaddleWidth / 2;
        canvas.Rectangle(x - 5, GameRules.PaddleY - 5, GameRules.PaddleWidth + 10, 28,
            new Color4(0.2f, 0.88f, 0.83f, 0.12f));
        canvas.Rectangle(x, GameRules.PaddleY, GameRules.PaddleWidth, GameRules.PaddleHeight, Cyan);
        canvas.Rectangle(x + 5, GameRules.PaddleY + 13, GameRules.PaddleWidth - 10, 3, White);
        canvas.Star(state.StarX, state.StarY, GameRules.StarRadius + 7,
            new Color4(1f, 0.76f, 0.26f, 0.10f));
        canvas.Star(state.StarX, state.StarY, GameRules.StarRadius, Gold);
        canvas.Text("A D / ARROWS  MOVE     P  PAUSE     R  RESTART     ESC  QUIT",
            GameRules.Width / 2, 17, 1.5f, Muted, centered: true);
        if (state.Status != GameStatus.Playing) Overlay(canvas, state);
    }

    private static void Background(Canvas canvas)
    {
        canvas.Rectangle(0, 0, GameRules.Width, GameRules.Height,
            new Color4(0.025f, 0.043f, 0.075f, 1));
        for (var i = 0; i < 44; i++)
        {
            var x = 32 + i * 137 % 896;
            var y = 135 + i * 79 % 370;
            canvas.Rectangle(x, y, 2, 2, new Color4(0.16f, 0.24f, 0.34f, 1));
        }
        for (var y = 128; y < 530; y += 80)
            canvas.Rectangle(32, y, 896, 1, new Color4(0.06f, 0.09f, 0.14f, 1));
        canvas.Rectangle(32, 548, 896, 1, Muted);
        canvas.Rectangle(32, 40, 896, 1, new Color4(0.10f, 0.19f, 0.24f, 1));
    }

    private static void Header(Canvas canvas, GameState state)
    {
        canvas.Text("CATCH THE STARS", 36, 599, 3, White);
        canvas.Text("A SMALL GAME. A FIRST OPENGL ADVENTURE.", 37, 572, 1.5f, Muted);
        canvas.Text("SCORE", 652, 613, 1.5f, Muted);
        canvas.Text($"{state.Score:00} / {GameRules.WinningScore}", 652, 575, 3, Gold);
        canvas.Text("LIVES", 840, 613, 1.5f, Muted);
        for (var i = 0; i < GameRules.StartingLives; i++)
            canvas.Star(850 + i * 28, 587, 9,
                i < state.Lives ? Cyan : new Color4(0.12f, 0.20f, 0.26f, 1));
    }

    private static void Overlay(Canvas canvas, GameState state)
    {
        canvas.Rectangle(0, 41, GameRules.Width, 506, new Color4(0, 0, 0, 0.42f));
        canvas.Rectangle(168, 230, 624, 195, new Color4(0.045f, 0.075f, 0.12f, 1));
        canvas.Rectangle(168, 422, 624, 3, state.Status == GameStatus.Lost ? Gold : Cyan);
        var title = state.Status switch
        {
            GameStatus.Ready => "READY TO CATCH?",
            GameStatus.Paused => "PAUSED",
            GameStatus.Won => "CLEAR SKIES!",
            _ => "GAME OVER"
        };
        var subtitle = state.Status switch
        {
            GameStatus.Ready => "CATCH 10 STARS BEFORE 3 MISSES",
            GameStatus.Paused => "TAKE YOUR TIME. YOUR STARS CAN WAIT.",
            GameStatus.Won => "ALL 10 STARS ARE HOME.",
            _ => $"YOU CAUGHT {state.Score:00} / 10 STARS. TRY AGAIN!"
        };
        var action = state.Status switch
        {
            GameStatus.Ready => "SPACE TO START",
            GameStatus.Paused => "P TO CONTINUE",
            _ => "R TO PLAY AGAIN"
        };
        canvas.Text(title, 480, 360, 4, White, centered: true);
        canvas.Text(subtitle, 480, 314, 2, Muted, centered: true);
        canvas.Text(action, 480, 260, 2.5f, Cyan, centered: true);
    }
}

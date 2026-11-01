namespace CatchStars.Core;

public static class GameRules
{
    public const int Width = 960, Height = 640;
    public const float PaddleWidth = 112, PaddleHeight = 18, PaddleY = 56;
    public const float StarRadius = 12, PaddleSpeed = 480, MaxDeltaSeconds = 0.05f;
    public const int WinningScore = 10, StartingLives = 3;
    public const uint DefaultSeed = 2026;

    public static GameState NewGame(uint seed = DefaultSeed) => Spawn(new GameState(
        Width / 2f, 0, 0, 0, StartingLives, seed, GameStatus.Ready));

    public static GameState Update(GameState state, GameInput input, float dt)
    {
        if (!float.IsFinite(dt) || dt < 0 || dt > MaxDeltaSeconds)
            throw new ArgumentOutOfRangeException(nameof(dt));
        if (!float.IsFinite(input.Movement))
            throw new ArgumentOutOfRangeException(nameof(input));
        if (input.Restart)
            return NewGame();
        if (state.Status == GameStatus.Ready && input.Start)
            return state with { Status = GameStatus.Playing };
        if (input.Pause && state.Status is GameStatus.Playing or GameStatus.Paused)
            return state with { Status = state.Status == GameStatus.Playing
                ? GameStatus.Paused : GameStatus.Playing };
        if (state.Status != GameStatus.Playing)
            return state;

        return Move(state, input.Movement, dt);
    }

    private static GameState Move(GameState state, float movement, float dt)
    {
        var horizontalSpeed = Math.Clamp(movement, -1, 1) * PaddleSpeed;
        var fallSpeed = 200 + state.Score * 16;
        var next = state with
        {
            PaddleX = Math.Clamp(state.PaddleX + horizontalSpeed * dt,
                PaddleWidth / 2, Width - PaddleWidth / 2),
            StarY = state.StarY - fallSpeed * dt
        };
        var crossesPaddle = state.StarY - StarRadius >= PaddleY + PaddleHeight
            && next.StarY - StarRadius <= PaddleY + PaddleHeight;

        if (crossesPaddle && OverlapsAtContact(state, horizontalSpeed, fallSpeed))
        {
            var caught = next with { Score = next.Score + 1 };
            return caught.Score >= WinningScore
                ? caught with { Status = GameStatus.Won } : Spawn(caught);
        }
        if (next.StarY + StarRadius < 0)
        {
            var missed = next with { Lives = next.Lives - 1 };
            return missed.Lives == 0
                ? missed with { Status = GameStatus.Lost } : Spawn(missed);
        }
        return next;
    }

    private static bool OverlapsAtContact(GameState state, float horizontalSpeed, float fallSpeed)
    {
        // Test the moving paddle where the star reaches it, not at the end of the frame.
        var contactTime = (state.StarY - StarRadius - PaddleY - PaddleHeight) / fallSpeed;
        var paddleX = Math.Clamp(state.PaddleX + horizontalSpeed * contactTime,
            PaddleWidth / 2, Width - PaddleWidth / 2);
        return state.StarX + StarRadius >= paddleX - PaddleWidth / 2
            && state.StarX - StarRadius <= paddleX + PaddleWidth / 2;
    }

    private static GameState Spawn(GameState state)
    {
        // A fixed sequence makes the game reproducible and its tests independent of luck.
        var seed = unchecked(state.Seed * 1664525u + 1013904223u);
        return state with
        {
            Seed = seed,
            StarX = StarRadius + seed % (Width - 2 * StarRadius),
            StarY = Height - 144
        };
    }
}

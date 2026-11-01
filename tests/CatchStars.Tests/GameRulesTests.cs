using CatchStars.Core;

namespace CatchStars.Tests;

public class GameRulesTests
{
    private const float Step = 1f / 60f;
    private const float PaddleTop = GameRules.PaddleY + GameRules.PaddleHeight;

    [Theory]
    [InlineData(0u)]
    [InlineData(2026u)]
    [InlineData(uint.MaxValue)]
    public void NewGameCreatesAReproducibleReadyState(uint seed)
    {
        var game = GameRules.NewGame(seed);
        var nextSeed = unchecked(seed * 1664525u + 1013904223u);

        Assert.Equal(game, GameRules.NewGame(seed));
        Assert.Equal(GameStatus.Ready, game.Status);
        Assert.Equal(GameRules.Width / 2f, game.PaddleX);
        Assert.Equal(0, game.Score);
        Assert.Equal(GameRules.StartingLives, game.Lives);
        Assert.Equal(nextSeed, game.Seed);
        Assert.Equal(GameRules.StarRadius + nextSeed %
            (GameRules.Width - 2 * GameRules.StarRadius), game.StarX);
        Assert.InRange(game.StarX, GameRules.StarRadius,
            GameRules.Width - GameRules.StarRadius);
        Assert.Equal(GameRules.Height - 144f, game.StarY);
    }

    [Fact]
    public void DefaultSeedIs2026()
    {
        Assert.Equal(2026u, GameRules.DefaultSeed);
        Assert.Equal(GameRules.NewGame(2026u), GameRules.NewGame());
    }

    [Fact]
    public void StartEntersPlayingWithoutAdvancingTheWorld()
    {
        var game = GameRules.NewGame();
        var next = GameRules.Update(game, new GameInput(Movement: 1, Start: true), Step);

        Assert.Equal(game with { Status = GameStatus.Playing }, next);
        Assert.Equal(GameStatus.Ready, game.Status);
    }

    [Theory]
    [InlineData(GameStatus.Playing, GameStatus.Paused)]
    [InlineData(GameStatus.Paused, GameStatus.Playing)]
    public void PauseTogglesWithoutAdvancingTheWorld(GameStatus before, GameStatus after)
    {
        var game = GameRules.NewGame() with { Status = before };
        var next = GameRules.Update(game, new GameInput(Movement: 1, Pause: true), Step);

        Assert.Equal(game with { Status = after }, next);
    }

    [Theory]
    [InlineData(GameStatus.Ready)]
    [InlineData(GameStatus.Paused)]
    [InlineData(GameStatus.Won)]
    [InlineData(GameStatus.Lost)]
    public void InactiveModesFreezeOrdinaryInput(GameStatus status)
    {
        var game = GameRules.NewGame() with { Status = status };

        Assert.Equal(game, GameRules.Update(game, new GameInput(Movement: 1), Step));
    }

    [Theory]
    [InlineData(GameStatus.Won)]
    [InlineData(GameStatus.Lost)]
    public void StartAndPauseCannotLeaveATerminalMode(GameStatus status)
    {
        var game = GameRules.NewGame() with { Status = status };

        Assert.Equal(game, GameRules.Update(game,
            new GameInput(Start: true, Pause: true), Step));
    }

    [Theory]
    [InlineData(GameStatus.Ready)]
    [InlineData(GameStatus.Playing)]
    [InlineData(GameStatus.Paused)]
    [InlineData(GameStatus.Won)]
    [InlineData(GameStatus.Lost)]
    public void RestartResetsEveryModeAndTakesPrecedence(GameStatus status)
    {
        var game = GameRules.NewGame(1) with
        {
            Status = status, Score = 4, Lives = 1, PaddleX = 100, StarY = 150
        };

        var next = GameRules.Update(game,
            new GameInput(Movement: 1, Start: true, Pause: true, Restart: true), Step);

        Assert.Equal(GameRules.NewGame(), next);
    }

    [Theory]
    [InlineData(-0.01f)]
    [InlineData(0.05001f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void InvalidDeltaIsRejectedEvenWhenReady(float delta)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GameRules.Update(GameRules.NewGame(), new GameInput(), delta));
    }

    [Theory]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void NonfiniteMovementIsRejected(float movement)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GameRules.Update(GameRules.NewGame(), new GameInput(Movement: movement), Step));
    }

    [Fact]
    public void ZeroDeltaDoesNotMoveAnOrdinaryPlayingState()
    {
        var game = Playing();

        Assert.Equal(game, GameRules.Update(game, new GameInput(Movement: 1), 0));
    }

    [Fact]
    public void MaximumDeltaIsAccepted()
    {
        var game = Playing();
        var next = GameRules.Update(game, new GameInput(), GameRules.MaxDeltaSeconds);

        Assert.Equal(0.05f, GameRules.MaxDeltaSeconds);
        Assert.Equal(game.StarY - 200 * GameRules.MaxDeltaSeconds, next.StarY, 4);
    }

    [Theory]
    [InlineData(-2f, -1f)]
    [InlineData(-1f, -1f)]
    [InlineData(-0.5f, -0.5f)]
    [InlineData(0f, 0f)]
    [InlineData(0.5f, 0.5f)]
    [InlineData(1f, 1f)]
    [InlineData(2f, 1f)]
    public void MovementIsClampedBeforeIntegration(float input, float expectedMovement)
    {
        var game = Playing();
        var next = GameRules.Update(game, new GameInput(Movement: input), Step);

        Assert.Equal(game.PaddleX + expectedMovement * GameRules.PaddleSpeed * Step,
            next.PaddleX, 4);
    }

    [Theory]
    [InlineData(-1f)]
    [InlineData(1f)]
    public void PaddleStopsAtBothWorldEdges(float direction)
    {
        var edge = direction < 0 ? GameRules.PaddleWidth / 2f
            : GameRules.Width - GameRules.PaddleWidth / 2f;
        var game = Playing() with { PaddleX = edge - direction };

        var next = GameRules.Update(game, new GameInput(Movement: direction), Step);

        Assert.Equal(edge, next.PaddleX);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(9)]
    public void ScoreSetsTheFallingSpeed(int score)
    {
        var game = Playing() with { Score = score };
        var next = GameRules.Update(game, new GameInput(), Step);

        Assert.Equal(game.StarY - (200 + score * 16) * Step, next.StarY, 4);
        Assert.Equal(game.StarX, next.StarX);
        Assert.Equal(game.Score, next.Score);
        Assert.Equal(game.Lives, next.Lives);
        Assert.Equal(game.Seed, next.Seed);
    }

    [Theory]
    [InlineData(-1f)]
    [InlineData(0f)]
    [InlineData(1f)]
    public void SweptCrossingCatchesWithInclusiveHorizontalOverlap(float edge)
    {
        var game = Crossing() with
        {
            StarX = 480 + edge * (GameRules.PaddleWidth / 2f + GameRules.StarRadius)
        };

        var next = GameRules.Update(game, new GameInput(), GameRules.MaxDeltaSeconds);

        Assert.Equal(1, next.Score);
        Assert.Equal(GameRules.StartingLives, next.Lives);
        Assert.Equal(GameStatus.Playing, next.Status);
        Assert.NotEqual(game.Seed, next.Seed);
        Assert.Equal(GameRules.Height - 144f, next.StarY);
        Assert.InRange(next.StarX, GameRules.StarRadius,
            GameRules.Width - GameRules.StarRadius);
    }

    [Theory]
    [InlineData(-1f)]
    [InlineData(1f)]
    public void AHorizontalGapDoesNotCatch(float edge)
    {
        var game = Crossing() with
        {
            StarX = 480 + edge * (GameRules.PaddleWidth / 2f + GameRules.StarRadius + 0.125f)
        };

        var next = GameRules.Update(game, new GameInput(), GameRules.MaxDeltaSeconds);

        Assert.Equal(0, next.Score);
        Assert.Equal(game.Seed, next.Seed);
        Assert.True(next.StarY < game.StarY);
    }

    [Fact]
    public void AStarAlreadyBelowThePaddleCannotBeCaughtRetroactively()
    {
        var game = Crossing() with { StarY = PaddleTop + GameRules.StarRadius - 0.125f };

        var next = GameRules.Update(game, new GameInput(), Step);

        Assert.Equal(0, next.Score);
        Assert.Equal(game.Seed, next.Seed);
    }

    [Fact]
    public void AStarThatHasNotReachedThePaddleDoesNotCatch()
    {
        var game = Crossing() with { StarY = PaddleTop + GameRules.StarRadius + 20 };

        Assert.Equal(0, GameRules.Update(game, new GameInput(), Step).Score);
    }

    [Fact]
    public void MovingOutOfOverlapAfterVerticalContactStillCatches()
    {
        var game = Playing() with { PaddleX = 480, StarX = 420, StarY = 87 };
        var input = new GameInput(Movement: 1);

        var next = GameRules.Update(game, input, GameRules.MaxDeltaSeconds);

        Assert.Equal(1, next.Score);
    }

    [Fact]
    public void MovingIntoOverlapAfterVerticalContactDoesNotCatch()
    {
        var game = Playing() with { PaddleX = 480, StarX = 565, StarY = 87 };
        var input = new GameInput(Movement: 1);

        var next = GameRules.Update(game, input, GameRules.MaxDeltaSeconds);

        Assert.Equal(0, next.Score);
        Assert.Equal(game.Seed, next.Seed);
    }

    [Fact]
    public void CollisionUsesThePaddleClampedAtVerticalContact()
    {
        var game = Playing() with { PaddleX = 60, StarX = 125, StarY = 91 };
        var input = new GameInput(Movement: -1);

        var next = GameRules.Update(game, input, GameRules.MaxDeltaSeconds);

        Assert.Equal(0, next.Score);
        Assert.Equal(game.Seed, next.Seed);
        Assert.Equal(GameRules.PaddleWidth / 2f, next.PaddleX);
    }

    [Fact]
    public void ExactVerticalContactIsIncluded()
    {
        var game = Crossing() with { StarY = PaddleTop + GameRules.StarRadius };

        Assert.Equal(1, GameRules.Update(game, new GameInput(), 0).Score);
    }

    [Fact]
    public void TenthCatchWinsWithoutRespawning()
    {
        var game = Crossing() with { Score = GameRules.WinningScore - 1 };

        var next = GameRules.Update(game, new GameInput(), GameRules.MaxDeltaSeconds);

        Assert.Equal(GameRules.WinningScore, next.Score);
        Assert.Equal(GameStatus.Won, next.Status);
        Assert.Equal(game.Seed, next.Seed);
        Assert.Equal(game.Lives, next.Lives);
    }

    [Fact]
    public void TouchingTheBottomBoundaryDoesNotLoseALife()
    {
        var game = Playing() with { StarY = -GameRules.StarRadius + 10 };

        var next = GameRules.Update(game, new GameInput(), GameRules.MaxDeltaSeconds);

        Assert.Equal(-GameRules.StarRadius, next.StarY);
        Assert.Equal(game.Lives, next.Lives);
        Assert.Equal(game.Seed, next.Seed);
    }

    [Fact]
    public void CrossingBelowTheBottomLosesOneLifeAndRespawns()
    {
        var game = Playing() with { StarY = -GameRules.StarRadius + 9 };

        var next = GameRules.Update(game, new GameInput(), GameRules.MaxDeltaSeconds);

        Assert.Equal(game.Lives - 1, next.Lives);
        Assert.Equal(game.Score, next.Score);
        Assert.Equal(GameStatus.Playing, next.Status);
        Assert.Equal(GameRules.Height - 144f, next.StarY);
        Assert.NotEqual(game.Seed, next.Seed);
    }

    [Fact]
    public void LastMissLosesWithoutRespawning()
    {
        var game = Playing() with { Lives = 1, StarY = -GameRules.StarRadius };

        var next = GameRules.Update(game, new GameInput(), Step);

        Assert.Equal(0, next.Lives);
        Assert.Equal(GameStatus.Lost, next.Status);
        Assert.Equal(game.Score, next.Score);
        Assert.Equal(game.Seed, next.Seed);
    }

    [Fact]
    public void UpdatingDoesNotChangeTheOriginalState()
    {
        var original = Crossing();
        var snapshot = original;

        var changed = GameRules.Update(original, new GameInput(), GameRules.MaxDeltaSeconds);

        Assert.Equal(snapshot, original);
        Assert.NotEqual(original, changed);
    }

    [Fact]
    public void CompleteSeededGameCanWinUsingOnlyMovementInput()
    {
        var game = GameRules.Update(GameRules.NewGame(1), new GameInput(Start: true), Step);
        var replay = game;

        for (var frame = 0; frame < 6000 && game.Status == GameStatus.Playing; frame++)
        {
            var movement = Math.Clamp((game.StarX - game.PaddleX) /
                (GameRules.PaddleSpeed * Step), -1f, 1f);
            var input = new GameInput(Movement: movement);
            game = GameRules.Update(game, input, Step);
            replay = GameRules.Update(replay, input, Step);
            Assert.Equal(game, replay);
            AssertValid(game);
        }

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(GameRules.WinningScore, game.Score);
        Assert.Equal(GameRules.StartingLives, game.Lives);
        Assert.Equal(game, GameRules.Update(game, new GameInput(Movement: -1), Step));
    }

    [Fact]
    public void CompleteSeededGameCanLoseAndRestartUsingOnlyInputs()
    {
        var game = GameRules.Update(GameRules.NewGame(), new GameInput(Start: true), Step);

        for (var frame = 0; frame < 6000 && game.Status == GameStatus.Playing; frame++)
        {
            var target = game.StarX < GameRules.Width / 2f
                ? GameRules.Width - GameRules.PaddleWidth / 2f
                : GameRules.PaddleWidth / 2f;
            var movement = Math.Clamp((target - game.PaddleX) /
                (GameRules.PaddleSpeed * Step), -1f, 1f);
            game = GameRules.Update(game, new GameInput(Movement: movement), Step);
            AssertValid(game);
        }

        Assert.Equal(GameStatus.Lost, game.Status);
        Assert.Equal(0, game.Lives);
        Assert.Equal(0, game.Score);
        Assert.Equal(game, GameRules.Update(game, new GameInput(Start: true), Step));
        Assert.Equal(GameRules.NewGame(),
            GameRules.Update(game, new GameInput(Restart: true), Step));
    }

    private static GameState Playing() => GameRules.NewGame() with { Status = GameStatus.Playing };

    private static GameState Crossing() => Playing() with
    {
        PaddleX = 480, StarX = 480, StarY = PaddleTop + GameRules.StarRadius + 5
    };

    private static void AssertValid(GameState game)
    {
        Assert.True(float.IsFinite(game.PaddleX));
        Assert.True(float.IsFinite(game.StarX));
        Assert.True(float.IsFinite(game.StarY));
        Assert.InRange(game.PaddleX, GameRules.PaddleWidth / 2f,
            GameRules.Width - GameRules.PaddleWidth / 2f);
        Assert.InRange(game.Score, 0, GameRules.WinningScore);
        Assert.InRange(game.Lives, 0, GameRules.StartingLives);
    }
}

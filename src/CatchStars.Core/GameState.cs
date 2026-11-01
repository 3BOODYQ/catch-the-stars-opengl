namespace CatchStars.Core;

public enum GameStatus { Ready, Playing, Paused, Won, Lost }

public readonly record struct GameInput(
    float Movement = 0, bool Start = false, bool Pause = false, bool Restart = false);

public readonly record struct GameState(
    float PaddleX, float StarX, float StarY, int Score, int Lives,
    uint Seed, GameStatus Status);

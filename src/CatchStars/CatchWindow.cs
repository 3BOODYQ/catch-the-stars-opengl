using CatchStars.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CatchStars;

internal sealed class CatchWindow : GameWindow
{
    private GameState _state = GameRules.NewGame();
    private Renderer? _renderer;
    private readonly SmokeCheck? _smoke;

    public CatchWindow(bool smokeTest) : base(
        new GameWindowSettings { UpdateFrequency = 120 },
        new NativeWindowSettings
        {
            ClientSize = new Vector2i(GameRules.Width, GameRules.Height),
            Title = "Catch the Stars | Space: start | A/D: move | P: pause | R: restart",
            APIVersion = new Version(3, 3),
            Profile = ContextProfile.Core,
            Flags = ContextFlags.ForwardCompatible,
            StartVisible = !smokeTest
        })
    {
        _smoke = smokeTest ? new SmokeCheck() : null;
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        VSync = VSyncMode.On;
        Console.WriteLine($"OpenGL: {GL.GetString(StringName.Version)}");
        Console.WriteLine($"Renderer: {GL.GetString(StringName.Renderer)}");
        _renderer = new Renderer();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
            return;
        }
        if (!IsFocused && _state.Status == GameStatus.Playing)
            _state = _state with { Status = GameStatus.Paused };
        if (!IsFocused)
            return;

        var left = KeyboardState.IsKeyDown(Keys.A) || KeyboardState.IsKeyDown(Keys.Left);
        var right = KeyboardState.IsKeyDown(Keys.D) || KeyboardState.IsKeyDown(Keys.Right);
        var input = new GameInput((right ? 1 : 0) - (left ? 1 : 0),
            KeyboardState.IsKeyPressed(Keys.Space), KeyboardState.IsKeyPressed(Keys.P),
            KeyboardState.IsKeyPressed(Keys.R));
        // Discard long stalls rather than teleporting objects after dragging the window.
        var dt = (float)Math.Min(args.Time, GameRules.MaxDeltaSeconds);
        _state = GameRules.Update(_state, input, dt);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        if (FramebufferSize.X <= 0 || FramebufferSize.Y <= 0)
            return;
        GL.ClearColor(0.008f, 0.014f, 0.025f, 1);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        var scale = Math.Min(FramebufferSize.X / (float)GameRules.Width,
            FramebufferSize.Y / (float)GameRules.Height);
        var width = (int)(GameRules.Width * scale);
        var height = (int)(GameRules.Height * scale);
        GL.Viewport((FramebufferSize.X - width) / 2, (FramebufferSize.Y - height) / 2,
            width, height);
        var canvas = new Canvas();
        Scene.Draw(canvas, _smoke?.State ?? _state);
        _renderer!.Draw(canvas);
        _smoke?.CheckFrame(this);
        SwapBuffers();
    }

    protected override void OnUnload()
    {
        _renderer?.Dispose();
        base.OnUnload();
    }
}

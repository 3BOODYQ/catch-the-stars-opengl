# Build Catch the Stars yourself: C#, OpenGL, and Ubuntu

This guide takes you from an empty folder to the game in this repository. Work
through one checkpoint at a time; run it before adding the next idea. Keep this
repository open as the finished reference while creating a separate practice
folder. The short window example below is complete. Later code fragments are
explicitly marked as excerpts; the linked source files contain the complete code.

You will move a paddle, catch ten falling stars, and lose after three misses.
There is one falling star at a time. Everything visible is made from colored
triangles, including the text. There are no downloaded images or audio assets.

## 1. Know which part does what

C# runs on the CPU. It reads keys, updates the rules, and constructs vertices.
OpenGL sends those vertices through small GPU programs called shaders to produce
pixels. OpenTK supplies C# OpenGL bindings plus window creation and input; it does
not decide the game's rules. OpenTK supports Linux; see its
[official repository](https://github.com/opentk/opentk).

The application has three projects:

| Project | Responsibility | Depends on |
| --- | --- | --- |
| `CatchStars.Core` | State, movement, spawning, collision, scoring | .NET only |
| `CatchStars` | Window, input, geometry, shaders, drawing | Core and OpenTK |
| `CatchStars.Tests` | Deterministic rules and complete simulated games | Core and xUnit |

The important boundary is `newState = GameRules.Update(oldState, input, dt)`.
The core knows nothing about windows or GPUs, so you can test it without a desktop.
The renderer receives a state to display; drawing does not advance the game.

## 2. Prepare Ubuntu

Install the **.NET 10 SDK**, which includes the runtime, using the instructions
for your Ubuntu release and CPU architecture in Microsoft's
[Ubuntu installation guide](https://learn.microsoft.com/dotnet/core/install/linux-ubuntu).
Use that page's commands rather than mixing package feeds or copying commands
for a different Ubuntu version. You also need an editor, a graphical desktop,
and a driver capable of creating an OpenGL 3.3 core context.

Check the SDK before creating files:

```bash
dotnet --list-sdks
dotnet --info
```

**Pass:** a `10.0` SDK appears. **Fail:** only runtimes appear, or `dotnet` is
missing. Install the SDK before proceeding. Building and running tests do not
prove that the graphics driver works; the first window checks that separately.

## 3. Create the same project skeleton

Run these commands in a new parent directory, not inside the existing reference
project. They use the normal user's .NET/NuGet configuration.

```bash
mkdir CatchStarsPractice
cd CatchStarsPractice
dotnet new globaljson --sdk-version 10.0.100 --roll-forward latestFeature
dotnet new sln --name CatchStars --format slnx
dotnet new classlib --name CatchStars.Core --output src/CatchStars.Core --framework net10.0
dotnet new console --name CatchStars --output src/CatchStars --framework net10.0
dotnet new xunit --name CatchStars.Tests --output tests/CatchStars.Tests --framework net10.0
dotnet sln CatchStars.slnx add src/CatchStars.Core/CatchStars.Core.csproj src/CatchStars/CatchStars.csproj tests/CatchStars.Tests/CatchStars.Tests.csproj
dotnet add src/CatchStars/CatchStars.csproj reference src/CatchStars.Core/CatchStars.Core.csproj
dotnet add tests/CatchStars.Tests/CatchStars.Tests.csproj reference src/CatchStars.Core/CatchStars.Core.csproj
dotnet add src/CatchStars/CatchStars.csproj package OpenTK.Graphics --version 4.9.4
dotnet add src/CatchStars/CatchStars.csproj package OpenTK.Windowing.Desktop --version 4.9.4
dotnet add tests/CatchStars.Tests/CatchStars.Tests.csproj package Microsoft.NET.Test.Sdk --version 17.14.1
dotnet add tests/CatchStars.Tests/CatchStars.Tests.csproj package xunit --version 2.9.3
dotnet add tests/CatchStars.Tests/CatchStars.Tests.csproj package xunit.runner.visualstudio --version 3.1.4
dotnet add tests/CatchStars.Tests/CatchStars.Tests.csproj package coverlet.collector --version 6.0.4
dotnet restore CatchStars.slnx --use-lock-file
dotnet build CatchStars.slnx
```

The two OpenTK packages pull in their own required dependencies. Do not add the
entire toolkit or another windowing library. The pinned versions above match
this project's `.csproj` files; `global.json` allows a newer .NET 10 feature band.
The first restore requires network access to NuGet. Lock files record the resolved
dependency versions; later use `dotnet restore CatchStars.slnx --locked-mode`.

In the Core and application `.csproj` property groups, add
`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` to match the reference.
Keep `ImplicitUsings` and `Nullable` enabled. The test project also needs
`<Using Include="Xunit" />` in an item group, as in its reference project file.
Remove the generated `src/CatchStars.Core/Class1.cs` and, when you add real tests,
`tests/CatchStars.Tests/UnitTest1.cs`. These two files are template placeholders.

**Pass:** the solution builds. **Experiment:** identify the application project
reference in its `.csproj`; explain why Core needs no reference back to the app.

## 4. Open a window before drawing a game

Temporarily replace the practice application's entire `Program.cs` with this
complete program. Do not combine it with the finished `Program.cs` yet.

```csharp
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using var window = new FirstWindow();
window.Run();

sealed class FirstWindow : GameWindow
{
    public FirstWindow() : base(GameWindowSettings.Default,
        new NativeWindowSettings
        {
            ClientSize = new Vector2i(960, 640),
            Title = "My first OpenGL window",
            APIVersion = new Version(3, 3),
            Profile = ContextProfile.Core,
            Flags = ContextFlags.ForwardCompatible
        }) { }

    protected override void OnLoad()
    {
        base.OnLoad();
        VSync = VSyncMode.On;
        Console.WriteLine(GL.GetString(StringName.Version));
        Console.WriteLine(GL.GetString(StringName.Renderer));
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        if (KeyboardState.IsKeyDown(Keys.Escape)) Close();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        if (FramebufferSize.X <= 0 || FramebufferSize.Y <= 0) return;
        GL.Viewport(0, 0, FramebufferSize.X, FramebufferSize.Y);
        GL.ClearColor(0.025f, 0.043f, 0.075f, 1);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        SwapBuffers();
    }
}
```

Run `dotnet run --project src/CatchStars`. `Run` owns the event loop; `OnLoad`
runs after the context exists; update handles keys; render clears the back buffer;
`SwapBuffers` presents it. The `using` statement releases the window afterward.
VSync synchronizes presentation with the display where supported.
The [OpenTK window lesson](https://opentk.net/learn/chapter1/1-creating-a-window.html)
is a useful companion to this checkpoint.

**Pass:** a dark window opens, reports a driver/version, resizes, and closes on
Escape. **Experiment:** change only the clear color. A failure here is a
window/context problem, not a collision or shader problem.

## 5. Learn the triangle pipeline

Study the complete [Renderer.cs](../src/CatchStars/Renderer.cs) and the `Vertex`
definition in [Canvas.cs](../src/CatchStars/Canvas.cs). For your next practice
checkpoint, use those two complete files and add the completed Core files from
section 7, because `Renderer` uses their world-size constants. Keep `FirstWindow`.
Add a `Renderer?` field, create it in `OnLoad`, and dispose it in `OnUnload`.
Add `using CatchStars;` to the temporary `Program.cs` so it can see the helper
types in that namespace. The final project already uses that namespace throughout.

In `OnRenderFrame`, after clearing and before `SwapBuffers`, make a fresh Canvas
and call its `Star(480, 320, 100, new Color4(1, 0.76f, 0.26f, 1))`, then pass it
to `Renderer.Draw`. This draws a fan of ten triangles using the finished helper.
To isolate one triangle, temporarily make `Canvas.Triangle` public and replace
the star call with `Triangle(new(380, 220), new(580, 220), new(480, 420), color)`;
use a local `Color4 color` holding that same gold color. Restore the helper's
private visibility after this exercise. These are edits to the practice copy.

Trace the actual rendering sequence:

1. CPU code creates vertices with position and RGBA color.
2. A VBO (vertex buffer object) holds a GPU copy of the data.
3. A VAO (vertex array object) describes how to read each vertex, and
   `GL.DrawArrays(Triangles, 0, vertexCount)` requests GPU work.
4. The vertex shader converts positions to clip coordinates and passes colors on.
5. OpenGL rasterizes triangles; the fragment shader supplies their pixel colors.
6. Swapping the front and back buffers presents the result.

Each vertex is six 32-bit floats: `x, y, red, green, blue, alpha`.
The stride is **24 bytes**. Attribute 0 reads two floats at byte offset 0;
attribute 1 reads four floats at byte offset 8. `StructLayout.Sequential` and
`Marshal.SizeOf<Vertex>()` keep the upload and attribute layout consistent.
A wrong stride can make sensible coordinates appear as random shapes.

`CreateProgram` compiles both GLSL shaders, links the program, checks success,
and includes driver logs in errors. `#version 330 core` matches the requested
OpenGL baseline. The fragment shader simply returns its interpolated input color.
See the [OpenTK triangle lesson](https://opentk.net/learn/chapter1/2-hello-triangle.html)
for the underlying pipeline; use this project's source for its exact API calls.

**Pass:** a gold triangle appears. **Experiments:** change its three positions;
give its vertices different colors; deliberately misspell `gl_Position`, read
the compilation error, then restore it. Explain why C# compilation cannot catch
that GLSL error before the graphics context runs.

## 6. Make a paddle and understand coordinates

Use `Canvas.Rectangle` instead of the triangle. A rectangle consists of two
triangles sharing a diagonal; six vertices are uploaded because this tiny game
uses no index buffer. Draw it at `(424, 56)` with width `112` and height `18`.
Its center is `480`, matching `GameState.PaddleX` in the final game.

The logical world is always **960 × 640 units**, with `(0, 0)` at bottom left.
These units behave like design pixels, but need not equal physical screen pixels.
The vertex shader's conversion is, component by component:

```text
ndcX = x / 960 * 2 - 1
ndcY = y / 640 * 2 - 1
```

Thus bottom left maps to `(-1, -1)`, center to `(0, 0)`, and top right to `(1, 1)`.
The shader sets clip-space `z = 0` and `w = 1`, so the later perspective division
leaves these normalized device coordinates unchanged. This game needs no camera.

A window's client size can differ from its framebuffer pixel size on a high-DPI
display. OpenGL's viewport uses **FramebufferSize**, not assumed client pixels.
The finished `CatchWindow.OnRenderFrame` finds the smaller of framebuffer-width/960
and framebuffer-height/640, scales both world dimensions equally, and centers the
viewport. The unused area becomes letterbox bars; shapes retain their proportions.
The framebuffer is cleared before this viewport drawing. Minimized zero-size
framebuffers are skipped to avoid invalid dimensions.

**Pass:** a wide, short paddle appears. **Experiment:** borrow the finished
letterbox viewport code and resize the window to a square. A star should keep
its shape and the world should stay centered. Stretching means unequal scaling.

## 7. Build the rules without graphics

Create the complete [GameState.cs](../src/CatchStars.Core/GameState.cs), then
reconstruct [GameRules.cs](../src/CatchStars.Core/GameRules.cs) one method at a time.
Start tests in [GameRulesTests.cs](../tests/CatchStars.Tests/GameRulesTests.cs)
before each rule: initial state, starting, movement, catches, misses, and restart.
Use the finished file to check your implementation after predicting its behavior.

`GameState` is a readonly record struct containing paddle position, star position,
score, lives, random seed, and status. `GameInput` describes intent, without any
OpenTK key types. `GameStatus` has Ready, Playing, Paused, Won, and Lost states.
A `with` expression creates a new value; it does not alter the previous state.
This excerpt illustrates the API after those files exist:

```csharp
var ready = GameRules.NewGame();
var playing = GameRules.Update(ready, new GameInput(Start: true), 1f / 60f);
var moved = GameRules.Update(playing, new GameInput(Movement: 1), 1f / 60f);
```

`ready` is still Ready; `moved.PaddleX` is eight units farther right than
`playing.PaddleX`, since `480 units/second × 1/60 second = 8 units`.
Clamp movement to `[-1, 1]`, and clamp the paddle center between half its width
and world width minus half its width. This keeps the whole paddle on screen.

`Update` validates finite movement and `dt` in `[0, 0.05]` seconds. Restart has
priority, returns `NewGame()` with the default seed, and returns to Ready.
Start and pause change mode without moving on that update. Inactive modes return
the same state for ordinary movement. Won and Lost require restart to leave.

`Move` lowers the star by `(200 + score × 16) × dt`. A catch increases the score;
the tenth catch sets Won. A complete exit below the bottom costs one life; the
third miss sets Lost. Other catches/misses spawn the next star. Terminal events
do not respawn, making their state easier to inspect and test.

`Spawn` advances a stored unsigned seed using
`unchecked(seed * 1664525u + 1013904223u)`. Overflow deliberately wraps. The result
selects a horizontal position within the star's radius margins; vertical spawn
position is `640 - 144 = 496`. This is reproducible game randomness, not suitable
for security. Equal seeds and equal input/time sequences reproduce equal states.

Collision needs both horizontal overlap and a downward crossing of the paddle's
top at `56 + 18 = 74`. Compare the star's **old bottom** and **new bottom** with
that top, rather than testing only its final position. This swept vertical check
prevents a step from skipping through the catching surface. When it crosses,
calculate contact time as `(oldStarY - radius - paddleTop) / fallingSpeed`.
Evaluate the paddle at that time using `oldPaddleX + movement * speed * contactTime`,
clamped to the world edges, and test horizontal overlap there. Testing only the
paddle's end position could award a catch after it arrived too late, or miss a
catch because it moved away afterward. Touching edges counts. The visible star
is a ten-triangle shape; collision uses its radius bounds, not exact polygons.

**Pass:** movement, exact contact, near misses, and terminal rules pass their tests.
**Experiment:** predict the behavior of a star already below the paddle, then read
`AStarAlreadyBelowThePaddleCannotBeCaughtRetroactively`. Change a test seed and
confirm repeatability before changing any rule.

## 8. Connect real input and elapsed time

Replace the temporary program with the complete [Program.cs](../src/CatchStars/Program.cs)
and add [CatchWindow.cs](../src/CatchStars/CatchWindow.cs), along with the remaining
complete app files in section 9. There must be one entry point; remove FirstWindow.
`CatchWindow` translates held A/Left and D/Right into -1, 0, or +1. Holding both
directions cancels out. Held movement uses `IsKeyDown`; Space/P/R use
`IsKeyPressed` so one physical press triggers one transition rather than toggling
every update. Escape closes the window.

The game requests 120 updates per second, but uses actual `FrameEventArgs.Time`
for motion. It caps an update at **50 milliseconds** before calling Core.
This is a variable time step with a cap: excess time during a stall is discarded.
It is **not** a fixed-step accumulator and does not catch up the lost time.
That deliberate limit prevents a window drag or stall from teleporting objects;
long stalls make simulated time fall behind real elapsed time.

Losing focus changes Playing to Paused and stops ordinary updates while unfocused.
Returning focus leaves the game paused until P is pressed. This avoids losing
lives while switching applications. Input handling stays in the window; Core
continues to accept plain values suitable for tests.

**Pass:** movement speed is based on seconds, walls stop the paddle, one P press
pauses once, and switching windows pauses play. **Experiment:** compute the
largest ordinary paddle displacement permitted by the cap: `480 × 0.05 = 24`.

## 9. Assemble the complete presentation

Use these finished files in your practice folder, with the same names/namespaces:

| File | What to study |
| --- | --- |
| [Canvas.cs](../src/CatchStars/Canvas.cs) | CPU mesh building: rectangles, stars, bitmap text |
| [Renderer.cs](../src/CatchStars/Renderer.cs) | GPU buffer, attributes, shaders, upload, draw, deletion |
| [Scene.cs](../src/CatchStars/Scene.cs) | Colors, layout, HUD, paddle, star, five mode screens |
| [CatchWindow.cs](../src/CatchStars/CatchWindow.cs) | Lifecycle, controls, time, viewport, presentation |
| [Program.cs](../src/CatchStars/Program.cs) | Arguments, application lifetime, startup errors |
| [SmokeCheck.cs](../src/CatchStars/SmokeCheck.cs) | Automated graphics checks and image capture |

`Canvas.Star` alternates ten outer/inner points, connecting neighboring points
to the center. `Text` reads a 5 × 7 glyph's row bits; each set bit becomes a
rectangle. Character advance is six units times text size, leaving one column
of spacing. The row-to-y conversion reverses bitmap row order because the game
world's y axis points upward. Unsupported glyphs draw blank, so this is a small
uppercase HUD font rather than a general Unicode text system.

`Scene.Draw` appends background first, then HUD, game objects, instructions, and
an overlay when needed. Order matters because this 2D renderer draws later
triangles over earlier ones. Alpha blending uses source alpha and one minus
source alpha for dimmed panels and glows. There are no textures or depth buffer
rules to learn for this lesson.

Each render makes a fresh Canvas. Its private list accumulates vertices and
`Vertices` produces an array. This local mesh construction is separate from the
immutable game-state boundary. `Renderer.Draw` uploads that array with StreamDraw
and issues one triangle draw call. Rebuilding and uploading this small scene is
simple; do not add mesh caching before measuring a reason to need it.

OpenGL resources belong to the graphics context, not the garbage collector.
`OnUnload` disposes Renderer while that context is available. Renderer deletes
the VBO, VAO, and linked program; temporary shaders are deleted after linking,
including failure paths. Keep GL calls in the window's context callbacks.
`Program` reports startup/render errors and exits nonzero instead of silently
showing a broken game. Shader logs identify GLSL compilation/link failures.

**Pass:** Ready, Playing, Paused, Won, and Lost have readable displays. **Experiment:**
change a Scene color without touching GameRules; then change paddle speed without
touching Renderer. Explain the different reasons those changes remain separate.

## 10. Verify the finished reconstruction

From the practice solution root, run:

```bash
dotnet restore CatchStars.slnx --locked-mode
dotnet build CatchStars.slnx --no-restore
dotnet test CatchStars.slnx --no-build --collect:"XPlat Code Coverage"
dotnet run --project src/CatchStars --no-build
dotnet run --project src/CatchStars --no-build -- --smoke-test
```

The `--` forwards the final argument to the game. Normal play starts on the Ready
screen: Space starts, A/D or arrows move, P pauses/resumes, R resets to Ready,
and Escape quits. A catch increases score and falling speed; misses remove lives.

The test file contains small unit cases plus complete deterministic simulation
runs that win, lose, restart, and check replay equality. These latter tests
integrate the rules over many updates, without a window. They do not exercise
physical keys or the graphics driver. Coverage output appears under the test
project's `TestResults` directory in a Cobertura XML report.

Measured for the reference code: **58 passing tests and 100% Core line/branch
coverage**, with a build reporting zero warnings and zero errors. That percentage
covers `CatchStars.Core`; it does not cover Renderer, window input, or all visual
behavior. Your reconstruction should reproduce these results before you add more.

The smoke command creates a hidden real OpenGL window, draws all five modes,
checks GL errors, resizes to a square and checks viewport proportions, then
checks gold star pixels, cyan paddle pixels, and gold score pixels in separate
regions. Whole-pixel viewport rounding is allowed within one pixel. Success prints
`PASS: all five screens, OpenGL drawing, aspect-preserving resize, and pixel checks.`
and writes `build/catch-stars.ppm`. It requires a graphical session even though
its window is hidden. It is a graphics integration check, not proof that keyboard
controls or every text label are correct. Manually play, pause, restart, resize,
switch focus, win, and lose to cover those user flows.

The reference graphics smoke test passed on Ubuntu 26.04 with Mesa 26.0.8 and
Intel UHD Graphics (TGL GT2), reporting an OpenGL 4.6 core context. Requesting a
3.3 baseline permits a newer compatible context. Other machines still need
their own working driver/display. Rendering is not included in the Core coverage.

## 11. Troubleshoot one boundary at a time

| Symptom | First place to investigate |
| --- | --- |
| SDK selection/build fails | `dotnet --list-sdks`, `global.json`, net10.0 targets |
| NuGet restore fails | Network/feed access and the reported package version; keep pins |
| Cannot open display | Run inside your logged-in Ubuntu graphical session |
| Wayland desktop but GLFW display errors | Check that XWayland is available for this OpenTK/GLFW setup |
| `DISPLAY` is empty or points elsewhere | Check `printenv DISPLAY`; do not invent a display number |
| `libGL`/driver loading or context creation fails | Repair the desktop's OpenGL/Mesa/vendor-driver installation |
| Context below OpenGL 3.3 core | Use a suitable driver/GPU session; changing C# rules cannot fix it |
| Background only, no geometry | Shader logs, VAO/VBO bindings, 24-byte stride, draw vertex count |
| Stretched shapes after resize | Framebuffer dimensions and the shared letterbox scale |
| Shader compile/link error | Printed driver log and matching shader inputs/outputs |
| Moving too quickly or unevenly | Seconds versus milliseconds, speed × dt, and the 50 ms cap |
| Test passes but rendering fails | Core tests deliberately do not create a graphics context |

Do not run the game with `sudo` to work around display permissions. A remote shell,
container, or headless machine may lack a usable display/context even when .NET
builds successfully. The console's OpenGL version and renderer identify the actual
implementation used; software rendering may behave differently from your GPU.

## 12. Continue learning through small changes

First reconstruct the same app and explain one update and one draw from input
to pixels. Then make one independent change at a time: a different winning score,
a wider paddle with matching wall tests, a new HUD color, or another supported
glyph. Predict the result, add a test for rule changes, run, and compare.

After you can explain this version, learn indexed drawing, texture coordinates,
textures, and transforms in separate runnable lessons. A camera, lighting, sound,
asset pipeline, and a general game engine are later topics. This lesson's goal
is understanding a complete small loop that you can rebuild and debug yourself.

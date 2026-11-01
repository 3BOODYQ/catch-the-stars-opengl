# Catch the Stars — C# and OpenGL

A small 2D game for learning modern OpenGL on Ubuntu. Move the cyan paddle,
catch 10 falling stars, and avoid missing 3. All shapes and bitmap text are drawn
with triangles and GLSL shaders; no external art assets are needed.

The stack is **C#, .NET 10, OpenTK 4.9.4, and OpenGL 3.3 core**. OpenTK provides
window creation, keyboard input, OpenGL bindings, and mathematics. The project
uses its graphics and desktop-window packages instead of the full audio toolkit.

## Build and play

From this folder, with the .NET 10 SDK and a working graphical desktop:

```bash
dotnet restore CatchStars.slnx --locked-mode
dotnet build CatchStars.slnx --no-restore
dotnet run --project src/CatchStars --no-build
```

On Ubuntu 26.04, the .NET SDK is available from Ubuntu's package feed; see
[Microsoft's Ubuntu installation guide](https://learn.microsoft.com/dotnet/core/install/linux-ubuntu).
The first restore downloads NuGet dependencies. A GPU driver supporting OpenGL
3.3 core and a working graphical desktop are required; no Unity or C++ tools
are needed.

| Key | Action |
| --- | --- |
| Space | Start |
| A / D or left / right arrows | Move |
| P | Pause / resume |
| R | Restart to the ready screen |
| Escape | Quit |

Losing focus pauses active play. Returning focus leaves it paused until P is
pressed. Restart uses the same star sequence so practice and tests are reproducible.

## Learn and verify

Start with [Build it yourself](docs/build-it-yourself.md): setup, a complete first
window, shaders, triangles, game state, collision, UI, tests, and exercises leading
to this exact app. The broader [learning roadmap](docs/learning-roadmap.md) lists
what to study after the game.

```bash
dotnet test tests/CatchStars.Tests --collect:"XPlat Code Coverage"
dotnet run --project src/CatchStars --no-build -- --smoke-test
```

Core tests do not need a display. The smoke test needs one: it renders all five
screens in a hidden native window, checks resize and drawing, saves a frame to
`build/catch-stars.ppm`, and exits. Coverage from the test project measures the
pure game rules, not GPU rendering or real keyboard input. Use the manual checks
in the guide for the interactive path.

The system is deliberately small: `CatchStars.Core` owns immutable state and
rules; `CatchStars` owns input, scene composition, and GPU resources; the test
project exercises the rules independently of graphics.

## Graphify

Graphify maps this project's documentation and source code into a queryable
knowledge graph. Planned advanced lessons remain distinct from implemented game
code.

Graphify 0.9.97 is already installed on this machine. On another machine with uv:

```bash
uv tool install graphifyy==0.9.97
graphify --help
```

Run commands from the project root. In Codex chat:

```text
/graphify .
/graphify . --update
/graphify query "How does a caught star change the score?"
```

Use `/graphify . --update` after changing documentation or source files. The
installed skill handles document extraction through the current agent session;
no API key is required for this workflow.

In a terminal, inspect the existing graph:

```bash
graphify query "GameRules Update" --budget 1200
graphify explain "CatchWindow"
graphify export html
graphify diagnose multigraph
```

Open [graphify-out/graph.html](graphify-out/graph.html) in a browser. The other
outputs are [GRAPH_REPORT.md](graphify-out/GRAPH_REPORT.md) and
[graph.json](graphify-out/graph.json).

After adding or editing code, update its structural relationships locally:

```bash
graphify update .
graphify export html
```

`graphify update .` updates code using AST extraction. Documentation changes
still need `/graphify . --update` in Codex; a terminal update can flag them for
semantic extraction rather than incorporating their new content. For continuous
code updates, run `graphify watch .` in a terminal and stop it with Ctrl+C.

`.graphifyignore` excludes build products, downloaded dependencies, local agent
settings, and secrets from scans. `.gitignore` excludes generated Graphify output
and local build products. Keep these rules when adding dependencies.

This folder is not yet an initialized Git repository. After Git is initialized,
`graphify hook install` enables updates after commits and other Git events;
`graphify hook status` checks installation. Git hooks do not replace document
updates through the Codex skill.

The graph is a navigation aid. Check cited source files before changing code;
planned components in the roadmap are not evidence that code already exists.

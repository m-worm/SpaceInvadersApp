# Space Invaders

A classic Space Invaders arcade game built with [Avalonia UI 12](https://avaloniaui.net/) and .NET 10. Runs as a native desktop application (Windows, macOS, Linux) and in the browser via WebAssembly.

## Solution Structure

```
SpaceInvaders.slnx               Solution file (XML format)

SpaceInvaders.Core/                 Shared class library (net10.0)
    GameCanvas.cs                   Custom Avalonia Control — game loop and rendering
    Engine/
        Entities.cs                 Data models: PlayerShip, Alien, Bullet, Barrier, GameInfo
        GameEngine.cs               All game logic: movement, firing, collision, state machine
        CollisionDetector.cs        AABB collision detection between all entity types
        SpriteData.cs               Pixel-art sprite definitions as bool[,] arrays

SpaceInvaders.Desktop/              Desktop host application (net10.0)
    Program.cs                      Entry point — classic desktop lifetime
    App.axaml / App.axaml.cs        Application setup with Fluent dark theme
    MainWindow.axaml / .axaml.cs    800x600 fixed window with keyboard capture

SpaceInvaders.Web/                  Browser/WASM host application (net10.0-browser)
    Program.cs                      Entry point — browser app async lifetime
    App.axaml / App.axaml.cs        Application setup with single-view lifetime
    MainView.axaml / .axaml.cs      UserControl root with keyboard capture
    wwwroot/
        index.html                  Minimal HTML shell
        main.js                     WASM bootstrap script
```

## Design and Implementation

### Architecture

The project follows a shared-library pattern. All game logic and rendering live in **SpaceInvaders.Core**, while **SpaceInvaders.Desktop** and **SpaceInvaders.Web** are thin host shells that provide platform-specific bootstrapping and keyboard input forwarding.

### Game Loop

`GameCanvas` (a custom Avalonia `Control`) drives the game loop using a `DispatcherTimer` ticking at ~60 FPS (16 ms interval). A `Stopwatch` measures real elapsed time for frame-rate-independent movement. Each tick calls `GameEngine.Update(dt)` then `InvalidateVisual()` to trigger a repaint.

### Rendering

All rendering happens in `GameCanvas.Render(DrawingContext)`. Sprites are defined as `bool[,]` arrays in `SpriteData` — each `true` cell becomes a filled rectangle at 3x pixel scale. Pre-allocated `static readonly` brushes eliminate per-frame GC pressure. Text is rendered via `FormattedText` for the HUD and overlay screens.

### Sprite System

Classic Space Invaders aliens are represented with pixel-accurate sprite data:
- **Squid** (8x8, top row) — two animation frames
- **Crab** (11x8, middle rows) — two animation frames
- **Octopus** (12x8, bottom rows) — two animation frames
- **Player ship** (13x8) — single frame

### Input

The host window/view captures `OnKeyDown`/`OnKeyUp` events and maintains a `HashSet<Key>` of currently pressed keys. This set is forwarded to `GameEngine` each frame, enabling simultaneous key support (e.g., moving and shooting at the same time).

### Game Logic

- **Player**: moves left/right at 300 px/s, fires with a 0.4-second cooldown
- **Aliens**: 5x11 grid marches horizontally, drops one row and reverses at screen edges, with two-frame animation toggled on each step
- **Speed scaling**: alien move interval scales by `aliveCount / totalCount` — fewer aliens alive means faster movement
- **Enemy fire**: a random bottom-row alien fires on a ~1.5-second timer (faster at higher levels)
- **Barriers**: four destructible shields with an arch cutout, cells erode on bullet or alien contact with splash damage
- **Scoring**: top row = 30 pts, middle rows = 20 pts, bottom rows = 10 pts
- **Lives**: 3 initial, 1.5-second respawn invulnerability on hit, game over at 0
- **Levels**: clearing all aliens advances the level with progressively faster base speed

### Collision Detection

`CollisionDetector` uses brute-force AABB (axis-aligned bounding box) checks. With at most ~60 entities on screen, this is well within budget. Four collision passes run each frame:
1. Player bullets vs. aliens (scoring and kill)
2. Enemy bullets vs. player (life loss)
3. All bullets vs. barrier cells (erosion with damage radius)
4. Alien bodies vs. barrier cells (full erasure on overlap)

### Game States

A simple state machine drives transitions:

```
StartScreen  ──[Enter]──>  Playing  ──[lives=0 or aliens reach bottom]──>  GameOver
     ^                                                                        |
     └────────────────────────────[Enter]─────────────────────────────────────┘
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (preview or release)
- Avalonia packages are restored automatically via NuGet

## Build

Build the entire solution from the repository root:

```bash
dotnet build SpaceInvaders.slnx
```

Or build individual projects:

```bash
dotnet build SpaceInvaders.Desktop/SpaceInvaders.Desktop.csproj
dotnet build SpaceInvaders.Web/SpaceInvaders.Web.csproj
```

## How to Run

### Desktop

```bash
dotnet run --project SpaceInvaders.Desktop/SpaceInvaders.Desktop.csproj
```

An 800x600 window opens with the start screen. Press **Enter** to begin.

### Web (WASM)

```bash
dotnet run --project SpaceInvaders.Web/SpaceInvaders.Web.csproj
```

Then open [http://localhost:5199](http://localhost:5199) in your browser. Click the game area first to give it keyboard focus.

### Controls

| Key | Action |
|-----|--------|
| Arrow Left / A | Move left |
| Arrow Right / D | Move right |
| Space | Fire |
| Enter | Start game / Restart after game over |

**Author:** Matthew Wormington  
**Version:** 1.0.0 — 4 May 2026  
**License:** MIT — see [LICENSE](LICENSE)

---

## AI Disclosure

This project was developed using **vibe coding** with [GitHub Copilot CLI](https://github.com/features/copilot) (GitHub/Anthropic, claude-sonnet-4-6). AI assistance was used throughout: project architecture, all source code, compatibility research and debugging, and this documentation.

All code was verified to build without errors and confirmed to render correctly at runtime on both Desktop and Browser (WASM) targets by the author. The author takes full responsibility for the published content.

> This disclosure follows the transparency recommendations of the Nature Methods editorial *"Using AI responsibly in scientific publishing"* (Nature Methods 23, 271, 2026; https://doi.org/10.1038/s41592-026-03020-1), which requires disclosure of AI tools used, the model/version, the scope of use, and confirmation that all AI-assisted content has been validated by the author.

---

## Future Enhancements

1. **Mystery ship (UFO)** — add the classic bonus saucer that flies across the top of the screen at random intervals, awarding 50-300 points
2. **Sound effects** — integrate audio for firing, alien explosions, player death, and the iconic alien march tempo that speeds up as aliens are eliminated
3. **High score persistence** — save and display a local leaderboard across sessions using file-based or browser local storage
4. **Mobile/touch input** — add on-screen touch controls for the web version to support phones and tablets
5. **Animated explosions** — add sprite-based explosion animations when aliens or the player are destroyed, with brief particle effects for visual feedback

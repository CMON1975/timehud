# TimeHud

Always-on-top HUD clock for Windows 11. Borderless, transparent, runtime-configurable.

## Features

- `yyyy.MM.dd (DDD) HH:mm:ss` format (uppercase 3-letter weekday), ticking each second
- Stand-up cycle beside the clock (`CLOCK | 30:00 ▶`): press play to count down the work block, one beep per second for the last three seconds, then a tone and the digits blink red. The button becomes footprints (Lucide `footprints`): press it to start the walk countdown (5:00 by default). At 00:00 the same beeps and tone play, the digits blink bright green, and the button becomes reset (Lucide `rotate-ccw`), which starts the next work block. Pressing the button mid-countdown restarts that phase. Optional, on by default.
- Drag with left-click anywhere on the clock
- Mouse wheel: backdrop opacity (10%–100%, 5% steps)
- `Ctrl` + wheel: font size (16–200pt, 4pt steps)
- `Shift` + wheel: text alpha (10%–100%, 5% steps)
- Right-click menu:
  - **Font** — Cascadia Mono · Cascadia Code · Consolas · DSEG7 Classic Mini · Digital-7 Mono
  - **Color** — Phosphor Green · Amber · Cyan · White · Red · Custom… (color picker dialog)
  - **Size** — preset sizes 20–96 pt
  - **Text alpha** — preset text transparency 30%–100%
  - **Timer** — Show timer toggle · Work length 5 / 10 / 15 / 30 / 45 / 60 min · Walk length 1 / 2 / 3 / 5 / 10 min (changing the current phase's length mid-run restarts it; hiding cancels the cycle)
  - **Start with Windows** — adds/removes a `HKCU\Software\Microsoft\Windows\CurrentVersion\Run\TimeHud` entry
  - **Reset position** — recenter on primary monitor
  - **Exit**
- Self-keeping topmost: re-asserts `HWND_TOPMOST` every 2 seconds, defeating UAC / fullscreen demotion
- All settings persist to `%APPDATA%\TimeHud\settings.json` (position, opacity, text alpha, size, font, color, work length, walk length, timer visibility, autostart); on every launch the previous file is snapshotted to `settings.json.bak` as a one-deep restore point

## Build & run

Requires the .NET 9 SDK on Windows. All commands are run from the repository root.

```powershell
git clone https://github.com/CMON1975/timehud.git
cd timehud

dotnet build TimeHud\TimeHud.sln
dotnet run --project TimeHud
```

For a redistributable build:

```powershell
dotnet publish TimeHud\TimeHud.csproj -c Release -r win-x64 --self-contained false -o publish
```

The result is `publish\TimeHud.exe` — framework-dependent, so the target machine needs the .NET 9 desktop runtime. Point a Start-menu shortcut at it, or use the **Start with Windows** menu item to register it for autostart.

## Tests

```powershell
dotnet test TimeHud\TimeHud.sln
```

108 xUnit tests cover the pure-logic units (`ClockFormatter`, `CountdownModel`, `StandUpCycle`, `CountdownFormatter`, `OpacityModel`, `SizeModel`, `SettingsStore`, `AutostartManager`, `FontRegistry`, `ColorPalette`). The WPF surface (drag, P/Invoke topmost, ColorDialog, timer button, flash animation, beep) is verified manually — see "Things that are easy to get wrong" in [CLAUDE.md](CLAUDE.md).

## Repository layout

- `TimeHud/` — WPF app (.NET 9, `net9.0-windows`, `UseWPF=true`, `UseWindowsForms=true`)
- `TimeHud.Tests/` — xUnit test project, also `net9.0-windows` so it can `ProjectReference` the WPF project
- `clock.ico` — application icon
- `CLAUDE.md` — architecture notes and gotchas for future Claude Code sessions

## Architecture (one-liner)

The interesting logic — formatting, clamping, persistence, registry, font/color lookup — lives in pure C# units that are TDD'd. The WPF window is a thin shell wiring those units to events. See [CLAUDE.md](CLAUDE.md) for the full split and the gotchas worth knowing before editing.

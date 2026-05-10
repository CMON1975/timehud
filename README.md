# TimeHud

Always-on-top HUD clock for Windows 11. Borderless, transparent, runtime-configurable.

## Features

- `yyyy.MM.dd.HH:mm:ss` format, ticking each second
- Drag with left-click anywhere on the clock
- Mouse wheel: opacity (10%–100%, 5% steps)
- `Ctrl` + wheel: font size (16–200pt, 4pt steps)
- Right-click menu:
  - **Font** — Cascadia Mono · Cascadia Code · Consolas · DSEG7 Classic Mini · Digital-7 Mono
  - **Color** — Phosphor Green · Amber · Cyan · White · Red · Custom… (color picker dialog)
  - **Start with Windows** — adds/removes a `HKCU\Software\Microsoft\Windows\CurrentVersion\Run\TimeHud` entry
  - **Reset position** — recenter on primary monitor
  - **Exit**
- Self-keeping topmost: re-asserts `HWND_TOPMOST` every 2 seconds, defeating UAC / fullscreen demotion
- All settings persist to `%APPDATA%\TimeHud\settings.json` (position, opacity, size, font, color, autostart)

## Build & run

Requires the .NET 9 SDK on Windows.

```powershell
dotnet build  C:\Tools\TimeHud\TimeHud.sln
dotnet run    --project C:\Tools\TimeHud
```

For a redistributable build:

```powershell
dotnet publish C:\Tools\TimeHud\TimeHud.csproj -c Release -r win-x64 --self-contained false
```

## Tests

```powershell
dotnet test C:\Tools\TimeHud\TimeHud.sln
```

47 xUnit tests cover the pure-logic units (`ClockFormatter`, `OpacityModel`, `SizeModel`, `SettingsStore`, `AutostartManager`, `FontRegistry`, `ColorPalette`). The WPF surface (drag, P/Invoke topmost, ColorDialog) is verified manually — see the verification checklist in [CLAUDE.md](CLAUDE.md).

## Repository layout

- `TimeHud/` — WPF app (.NET 9, `net9.0-windows`, `UseWPF=true`, `UseWindowsForms=true`)
- `TimeHud.Tests/` — xUnit test project, also `net9.0-windows` so it can `ProjectReference` the WPF project
- `clock.ico` — application icon
- `CLAUDE.md` — architecture notes and gotchas for future Claude Code sessions

## Architecture (one-liner)

The interesting logic — formatting, clamping, persistence, registry, font/color lookup — lives in pure C# units that are TDD'd. The WPF window is a thin shell wiring those units to events. See [CLAUDE.md](CLAUDE.md) for the full split and the gotchas worth knowing before editing.

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository purpose

`TimeHud/` — WPF (.NET 9) always-on-top clock for Windows. Shows `yyyy.MM.dd (DDD) HH:mm:ss`. Borderless, transparent backdrop, drag-to-move, mouse-wheel backdrop opacity, Ctrl+wheel font size, Shift+wheel text alpha, runtime font swap (5 fonts), runtime color (5 presets + WinForms ColorDialog for custom), Size / Text alpha preset submenus, autostart toggle, position+settings persistence in `%APPDATA%\TimeHud\settings.json` (snapshotted to `settings.json.bak` at each launch).

An earlier PowerShell + WinForms `MiniClock` lived in this repo and was removed in a subsequent commit; it's recoverable from `git log --diff-filter=D --name-only` if anyone ever asks.

## Build & run

```powershell
dotnet test C:\Users\chris\personal_projects\time_hud\TimeHud\TimeHud.sln          # unit tests for the testable units
dotnet build C:\Users\chris\personal_projects\time_hud\TimeHud\TimeHud.sln         # both projects
dotnet run --project C:\Users\chris\personal_projects\time_hud\TimeHud             # launch
```
Targets `net9.0-windows` with `UseWPF=true` and `UseWindowsForms=true` (the latter for `System.Windows.Forms.ColorDialog`). Solution contains two projects: `TimeHud` (WPF app) and `TimeHud.Tests` (xUnit). The test project also targets `net9.0-windows` so it can `ProjectReference` the WPF project; tests themselves only exercise non-WPF logic.

## Architecture

TimeHud splits into **TDD'd pure-C# units** and **WPF wiring** (single window, no MVVM framework, no DI container — straight code-behind). The split is deliberate: WPF surface (drag, P/Invoke, font construction) is verified manually; everything else has tests.

**Tested (`TimeHud.Tests/`):**
- `ClockFormatter` — fixed `yyyy.MM.dd (DDD) HH:mm:ss` invariant-culture format; the weekday is the invariant `ddd` abbreviation upper-cased (always English, never the machine's locale).
- `OpacityModel` — clamp [0.10, 1.00] at 0.05 steps.
- `SizeModel` — clamp [16, 200] at 4pt steps.
- `SettingsStore` / `Settings` — JSON load/save, defaults on missing/corrupt (missing *properties* also fall back per-property, so a partial file silently resurrects defaults — this once reset a user's size/opacity). `Archive` copies the file to `settings.json.bak` at launch as the one-deep restore point.
- `AutostartManager` — registry HKCU\…\Run toggle, idempotent. Uses `IRegistryStore` seam; tests use an in-memory fake.
- `FontRegistry` — 5-font key↔display-name+source map (`cascadiamono` default, `cascadia`, `consolas`, `dseg7`, `d7mono`); unknown→default. Modern monos are listed first; bundled DSEG7 + Digital-7 Mono come after a separator.
- `ColorPalette` — 5-preset key↔display-name+hex map (`green` default phosphor `#00FF5A`, `amber`, `cyan`, `white`, `red`); unknown→default. Tests verify each preset's hex parses and matches the documented sRGB.

**Untested WPF wiring:**
- `MainWindow.xaml`/`.cs` — borderless transparent window, rounded `Backdrop` border + `ClockText`. Plain wheel → `OpacityModel` (backdrop); Shift+wheel → a second `OpacityModel` driving `ClockText.Opacity` (text alpha); Ctrl+wheel → `SizeModel`. Right-click context menu wires Font/Color (presets + Custom...)/Size/Text alpha/Autostart/Reset/Exit. Color "Custom..." opens `System.Windows.Forms.ColorDialog` (modal). Size and Text alpha submenu checkmarks are synced by iterating the submenu's `MenuItem`s and comparing `Tag` (invariant-parsed) to the model value.
- `RegistryStore` — production `IRegistryStore` over `Microsoft.Win32.Registry`.
- `FontFamilyFactory` — turns a `FontEntry` into a WPF `FontFamily` (handles both pack URIs and system family names).
- `TopmostKeeper` — P/Invoke `SetWindowPos(HWND_TOPMOST, …)`. A 2-second `DispatcherTimer` re-asserts topmost in `MainWindow` because Windows demotes `Topmost=true` after UAC prompts, fullscreen apps, DWM restarts, etc. Without this the clock silently drops behind other windows over time.

## Things that are easy to get wrong

- **WPF bundled-font URI:** in `FontRegistry`, the pack URI's `#` is followed by the font's *embedded family name* (`DSEG7 Classic Mini`), not the file name. If you replace the .ttf, verify the embedded family with `(New-Object System.Windows.Media.GlyphTypeface($uri)).FamilyNames.Values` — a mismatch silently falls back to a default font with no error.
- **Backdrop opacity vs. window opacity:** opacity is applied to the `Backdrop` `Border.Opacity`, not `Window.Opacity`. Putting it on the Window would also fade the text. The `Backdrop` and `ClockText` are siblings in a `Grid` so the backdrop's opacity doesn't cascade.
- **`Window.AllowsTransparency="True"` requires `WindowStyle="None"`** — they're paired; changing one without the other will throw at startup.
- **Test project must target `net9.0-windows`** to reference the WPF project. A plain `net9.0` test project won't satisfy the project-reference constraint.
- **`UseWPF=true` + `UseWindowsForms=true` together cause name collisions** between `System.Windows.Media` (WPF) and `System.Drawing` / `System.Windows.Forms` (WinForms). The collisions seen so far: `Application`, `FontFamily`, `Color`, `ColorConverter`. We resolve them with `using` aliases at the top of each affected file (`SwmColor`, `SdColor`, `SwmColorConverter`, `SwfColorDialog`, etc.) rather than dropping `ImplicitUsings` or removing one of the two flags. Adding new code that touches these types? Add the alias.

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository purpose

`TimeHud/` — WPF (.NET 9) always-on-top clock for Windows. Shows `yyyy.MM.dd (DDD) HH:mm:ss`, plus an optional stand-up cycle beside it (`CLOCK | 30:00 ▶`: work countdown beeps at 3/2/1 and blinks red at 00:00; the button becomes Lucide `footprints` and starts a walk countdown (default 5:00), which beeps the same way and blinks bright green at 00:00; reset then starts the next work countdown). Borderless, transparent backdrop, drag-to-move, mouse-wheel backdrop opacity, Ctrl+wheel font size, Shift+wheel text alpha, runtime font swap (5 fonts), runtime color (5 presets + WinForms ColorDialog for custom), Size / Text alpha / Timer preset submenus, autostart toggle, position+settings persistence in `%APPDATA%\TimeHud\settings.json` (snapshotted to `settings.json.bak` at each launch).

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
- `CountdownModel` — deadline-based stand-up timer (Idle/Running/Finished). `Tick(now)` derives remaining seconds as `ceil(deadline - now)` clamped to [0, duration], so the tick interval never drifts it; returns `TickEffects` flags: `Beep` when remaining changes into 3/2/1 (change-detection dedupes sub-second ticks; a stall from 5→1 beeps once), `Finished` exactly once at 0. `Start` restarts from full in any state; `SetDuration` stays Idle if Idle, else restarts; `Cancel` returns to Idle. Duration clamps [1, 180] min.
- `StandUpCycle` — Work/Walk phase machine over one `CountdownModel`. `Press(now)` is the button: Work/Idle and Work/Running (re)start the work countdown; Work/Finished flips to Walk and starts it; any press during Walk (Running or Finished) returns to Work and starts a fresh work countdown, so cutting a walk short lands on the seated timer (phase changes go through `SetDuration`, which restarts from Running/Finished). `SetWorkMinutes`/`SetWalkMinutes` only touch the countdown when that phase is current (so changing the work length mid-walk doesn't disturb the walk); `Cancel` returns to Work/Idle at the work length. Both lengths clamp like `CountdownModel`.
- `CountdownFormatter` — `mm:ss`, invariant culture, minutes may exceed 59, negatives clamp to `00:00`.
- `SettingsStore` / `Settings` — JSON load/save, defaults on missing/corrupt (missing *properties* also fall back per-property, so a partial file silently resurrects defaults — this once reset a user's size/opacity). `Archive` copies the file to `settings.json.bak` at launch as the one-deep restore point.
- `AutostartManager` — registry HKCU\…\Run toggle, idempotent. Uses `IRegistryStore` seam; tests use an in-memory fake.
- `FontRegistry` — 5-font key↔display-name+source map (`cascadiamono` default, `cascadia`, `consolas`, `dseg7`, `d7mono`); unknown→default. Modern monos are listed first; bundled DSEG7 + Digital-7 Mono come after a separator.
- `WavTone` — in-memory 16-bit mono PCM WAV sine tone with 5 ms fade edges; tests check header fields, byte lengths, silent endpoints and peak amplitude.
- `WavTone` — in-memory 16-bit mono PCM WAV sine tone with 5 ms fade edges; tests check header fields, byte lengths, silent endpoints and peak amplitude.
- `ColorPalette` — 5-preset key↔display-name+hex map (`green` default phosphor `#00FF5A`, `amber`, `cyan`, `white`, `red`); unknown→default. Tests verify each preset's hex parses and matches the documented sRGB.

**Untested WPF wiring:**
- `MainWindow.xaml`/`.cs` — borderless transparent window, rounded `Backdrop` border + a horizontal `Row` StackPanel holding `ClockText` and `TimerGroup` (divider `Rectangle`, `TimerText`, `TimerButton`). Font family/size/color are set on `Row` via `TextElement.SetFontFamily/SetFontSize/SetForeground` (attached, inherited by both TextBlocks); `ApplyColor` also pushes the brush to `Divider.Fill` and `TimerIcon.Stroke` because shapes don't inherit Foreground. Text alpha is applied per element by `ApplyTextAlpha` (`ClockText`, `Divider` at 0.5×, `TimerButton`, `TimerText`), not on `Row`, so the finished countdown can be forced to full opacity on its own. Plain wheel → `OpacityModel` (backdrop); Shift+wheel → a second `OpacityModel` (text alpha); Ctrl+wheel → `SizeModel` via `ApplySize` (also scales the icon Viewbox to 0.8× the font size). Right-click context menu wires Font/Color (presets + Custom...)/Size/Text alpha/Timer/Autostart/Reset/Exit. Color "Custom..." opens `System.Windows.Forms.ColorDialog` (modal). Size, Text alpha and Timer submenu checkmarks are synced by iterating the submenu's `MenuItem`s and comparing `Tag` (invariant-parsed) to the model value.
- Stand-up timer wiring: `MainWindow` holds a `StandUpCycle` (`_cycle`), not a bare `CountdownModel`. Icon per (phase, state) in `RefreshTimerUi`: Work/Idle play, Work/Running reset, Work/Finished footprints, Walk/Running footprints, Walk/Finished reset. `StartFlash(hex)` takes the colour: red after work, `WalkDoneHex` (`#39FF14`) after walk. Timer menu has `WorkMenu` (5–60) and `WalkMenu` (1–10) submenus with separate check sync; `WalkMinutes` persists in settings. A 200 ms `_countdownTimer` runs only while `_cycle.State == Running` (the 1 s clock timer has arbitrary phase, so reusing it would lag display/beeps by up to a second); it stops on `Finished`. The button is a chrome-free `ControlTemplate` (transparent Border + ContentPresenter, `Focusable=False`) around a `Viewbox`/`Canvas`/`Path` whose `Data` swaps between Lucide `play` and `rotate-ccw` geometry (`PlayIcon`/`ResetIcon` statics). `ButtonBase` marks `MouseLeftButtonDown` handled, so clicks don't reach the window's `DragMove`. Flash = red `TimerText.Foreground` + `DoubleAnimation` on `TimerText.Opacity` (400 ms, AutoReverse, Forever; text alpha dropped for the countdown while flashing); `StopFlash` removes it with `BeginAnimation(OpacityProperty, null)` and `ClearValue(ForegroundProperty)` so the inherited colour returns. "Show timer" collapses `TimerGroup` and `Cancel`s the model so a hidden timer never beeps. `TonePlayer` (`ISoundPlayer`) plays pre-rendered WAVs from `WavTone` (880 Hz / 150 ms `Beep` at 3/2/1, 1175 Hz / 1 s `BeepLong` on `Finished`) through `System.Media.SoundPlayer`; kernel32 `Beep` was silent in practice.
- `RegistryStore` — production `IRegistryStore` over `Microsoft.Win32.Registry`.
- `FontFamilyFactory` — turns a `FontEntry` into a WPF `FontFamily` (handles both pack URIs and system family names).
- `TopmostKeeper` — P/Invoke `SetWindowPos(HWND_TOPMOST, …)`. A 2-second `DispatcherTimer` re-asserts topmost in `MainWindow` because Windows demotes `Topmost=true` after UAC prompts, fullscreen apps, DWM restarts, etc. Without this the clock silently drops behind other windows over time.

## Things that are easy to get wrong

- **WPF bundled-font URI:** in `FontRegistry`, the pack URI's `#` is followed by the font's *embedded family name* (`DSEG7 Classic Mini`), not the file name. If you replace the .ttf, verify the embedded family with `(New-Object System.Windows.Media.GlyphTypeface($uri)).FamilyNames.Values` — a mismatch silently falls back to a default font with no error.
- **Backdrop opacity vs. window opacity:** opacity is applied to the `Backdrop` `Border.Opacity`, not `Window.Opacity`. Putting it on the Window would also fade the text. The `Backdrop` and `Row` are siblings in a `Grid` so the backdrop's opacity doesn't cascade. Text alpha is set per element (never on `Row`, because a child cannot be more opaque than its parent); while flashing, `TimerText` ignores text alpha and the blink animates its opacity from a base of 1.0.
- **`StackPanel` has no `Foreground`/`FontSize`:** it isn't a `Control`. Use the `TextElement.*` attached properties (XAML `TextElement.Foreground="..."`, code `TextElement.SetForeground(Row, brush)`); a plain `Foreground=` on the panel is a XAML compile error (MC3072).
- **Shapes don't inherit Foreground:** the divider `Rectangle` and icon `Path` need their `Fill`/`Stroke` set explicitly in `ApplyColor`.
- **More WPF/WinForms collisions:** `System.Windows.Shapes.Path` vs `System.IO.Path` (don't `using System.Windows.Shapes` in `MainWindow.xaml.cs`; reach the icon by its `x:Name` field) and `Brushes` (WPF vs Drawing). Build brushes from hex via the existing `SwmColorConverter` route.
- **7-segment fonts have no `|` glyph:** DSEG7 / Digital-7 would fall back to another font for a pipe, so the clock/timer divider is a 1 px `Rectangle`, not text.
- **`Window.AllowsTransparency="True"` requires `WindowStyle="None"`** — they're paired; changing one without the other will throw at startup.
- **Test project must target `net9.0-windows`** to reference the WPF project. A plain `net9.0` test project won't satisfy the project-reference constraint.
- **`UseWPF=true` + `UseWindowsForms=true` together cause name collisions** between `System.Windows.Media` (WPF) and `System.Drawing` / `System.Windows.Forms` (WinForms). The collisions seen so far: `Application`, `FontFamily`, `Color`, `ColorConverter`. We resolve them with `using` aliases at the top of each affected file (`SwmColor`, `SdColor`, `SwmColorConverter`, `SwfColorDialog`, etc.) rather than dropping `ImplicitUsings` or removing one of the two flags. Adding new code that touches these types? Add the alias.

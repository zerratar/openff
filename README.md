# FF3 — MonoGame (Windows)

A Windows port of the Final Fantasy III mobile build, reconstructed from the original
Windows Phone assembly and rebuilt on MonoGame.

```
FF3.sln
  FF3.Game/          the game
  FF3.ContentTool/   XNB extractor and content pipeline generator
  Content/           extracted, editable source assets + Content.mgcb
  Docs/              porting notes and reference
```

## Build and run

```bash
dotnet build
dotnet run --project FF3.Game
```

The game locates its data by looking for a `Content` directory containing `data000.bin`,
walking up from the executable and also checking `Unpacked/Content`. Override with
`FF3_CONTENT=<path>`. Nothing is copied into `bin/`, so the ~540 MB of original data
stays where it is.

## Controls

| Input | Action |
| --- | --- |
| Arrow keys / WASD | D-pad |
| Z / Space / Enter | A (confirm) |
| X / Backspace / Esc | B (cancel) |
| C / V | X / Y |
| Q / E | L / R |
| Left Shift / Right Shift | Start / Select |
| Mouse | Touch (click, drag) |
| Tab (hold) | Fast-forward |
| F12 | Screenshot |

Keyboard input is fed into the game's own NDS-style pad register, so it drives every
menu and field control the DS original supported. See `Docs/Porting-Notes.md`.

## Diagnostics

Every run writes `FF3.Game/bin/Debug/net8.0/logs/ff3.log` (previous run kept as
`ff3.prev.log`).

| Variable | Effect |
| --- | --- |
| `FF3_LOG=all` | Enable all channels: `gl, texture, content, file, sound, input, event, firstchance` |
| `FF3_LOG_FILE=<path>` | Write the log elsewhere |
| `FF3_DUMP=<dir>` | Dump decoded source blobs (PNGs the game loads) |
| `FF3_DUMP_FONTS=<dir>` | Dump SpriteFont atlases as they load |
| `FF3_SCREENSHOT_DIR=<dir>` | Where F12 screenshots go |
| `FF3_SCREENSHOT_EVERY=<s>` | Capture automatically every N seconds |
| `FF3_SPEED=<n>` | Extra update passes while fast-forwarding |

`firstchance` is worth knowing about: large parts of the decompiled game swallow
exceptions, so a porting bug usually shows up as "nothing rendered" rather than a crash.

## Content

`Content/` holds the assets extracted back out of the shipped XNB files, plus a
`Content.mgcb` so the MonoGame pipeline can rebuild them. See
`Docs/Content-Pipeline.md`.

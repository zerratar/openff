# OpenFF

One client that plays Final Fantasy III and Final Fantasy IV (the 3D remakes) from the copies
you own, rendering natively on the desktop, and an engine underneath that mods can reach all
the way down - so that a mod can use both games' content, give each character the progression
system of either game, or make something that is not the game's game at all. Each game stays
playable on its own; a mod can target one game or the client.

It began as a Windows port of the FF3 mobile build, reconstructed from the Windows Phone
assembly and rebuilt on MonoGame. That path - **FF3 from the Steam install, title to credits,
1:1 with the original** - is the one that works today and must not break. FF4 (3D) runs
through the same code behind a `GameProfile` seam and is the work in progress: Baron and the
overworld, the opening scenes on FF4's own scene engine, battles on their battle stages with
FF4's HUD, the menu in FF4's dress, shops, inns, saves, encounters - with what is still missing
listed in `Docs/Client-Plan.md`, and every unread piece of FF4's binary in `Docs/FF4-Internals.md`.

None of the games' data is in this repository (see *Game data*).

| | |
| --- | --- |
| ![Final Fantasy III's title, from the Steam install](Docs/Images/ff3-title.png) | ![Ur, in Final Fantasy III](Docs/Images/ff3-ur.png) |
| *FF3 from its Steam install: the title, TrueType text at the window's resolution* | *Ur; the game plays as shipped* |
| ![The Red Wings deck, Final Fantasy IV's opening scene](Docs/Images/ff4-deck.png) | ![A battle on FF4's battle stage with its HUD](Docs/Images/ff4-battle.png) |
| *FF4's opening on its own scene engine: camera motions, casts, the message bar* | *A battle on FF4's battle stage - its stage, party positions, window art and glove* |
| ![FF4's menu drawn from its own layouts and data](Docs/Images/ff4-menu.png) | ![Crystal, the editor, with an FF3 map open in 3D](Docs/Images/crystal-map.png) |
| *FF4's menu from its own layouts, over the unified party data* | *Crystal: a map in 3D, its characters and exits in the hierarchy, the inspector* |

## Download

The [Releases](https://github.com/zerratar/openff/releases) page has `OpenFF-<version>-win-x64.zip`:
unzip anywhere, `OpenFF.exe` plays, `crystal.exe` edits and makes mods, `mods\` is where mods
go. Nothing to install; it needs the Steam copies of the games on the machine. What each
release carries is in `Docs/Releases.md`.

## What you need (to build)

- Windows, the .NET 8 SDK. MonoGame (DesktopGL), FontStashSharp and NVorbis come from NuGet.
- Your own copy of **Final Fantasy III** and/or **Final Fantasy IV (3D Remake)** on Steam. The
  client reads the installs in place; nothing is extracted or copied.
- Python 3 for the scripts in `Tools/` (optional).

## Build and run

```
dotnet build
OpenFF\bin\Debug\net8.0\OpenFF.exe
```

With no arguments `OpenFF.exe` finds the Steam installs, boots FF3 and remembers the choice in
`%LocalAppData%\OpenFF\launch.json`. The switches that matter:

| Switch | Effect |
| --- | --- |
| `--game=ff4` | Boot FF4 (Baron town) instead; remembered for the next start |
| `--content=<dir>` | A specific Steam install, when it is not where Steam usually puts it |
| `--map=<id> --pos=x,y,z --rot=<deg>` | Any map of either game, no title (`d01_05`, `t01_00`, `f00`, `e01_00`) |
| `--party=4:10 --gil=500 --load=<slot>` | FF4 test starts: members by type and level, gil, a saved slot |
| `--size=1600x960` | Window size |
| `--drive=<file>` | Play a scripted key drive from inside the game (see `Docs/Drives/`) |
| `--nomods` | Load no mod code |

The editor:

```
Crystal.Editor\bin\Debug\net8.0\crystal.exe
```

opens Crystal in the browser. With a command (`extract`, `script`, `tables`, `msd`, `pak`,
`xbn`, `mdl`, `tex`, `cells`, `hich`, `mcl`, `lz` ...) it converts the games' files on the
command line instead; `crystal` alone prints the list. Crystal keeps its projects under
`%LocalAppData%\OpenFF\Crystal\projects` (an older `FF3ContentTool` folder is moved there on
first start). A running `crystal.exe` - one started from Visual Studio included - holds its
own build output, so `dotnet build` fails on `Crystal.Editor` with "file is locked by
crystal.exe" until it is closed; the client's build is not affected.

**For people without the SDK:** `publish.cmd` builds `dist\OpenFF\` - `OpenFF.exe`, `crystal.exe`,
the engine, an empty `mods\` folder and these documents, with the .NET runtime included. Zip
that folder and hand it over; it needs nothing but the Steam games on the machine it runs on.

## The layout

```
OpenFF/          the client
  GlobalScope/     the decompiled game (FF3's own code, ported)
  Compat/          the port's seams: GameProfile, the FF4 systems (Ff4*), the engine host, the API implementation
OpenFF.Engine/     the mod API and object model: what a mod references (no MonoGame, no game code)
Crystal.Editor/   Crystal, the editor, and the command-line converters
Shared/            every file format once - Content, Data (the unified tables), Script, Text - compiled into both programs
Samples/           HelloMod (a service, a behaviour, a save chunk) and Survivors (a survivors-style run on the field)
Tools/             Python: the FF4 binary dumps, table generators, format tests
Docs/              reference, plans and the journal (below)
Reference/libff4/  what we wrote about the FF4 binary; the dumps themselves are regenerated, not committed
Content/           the pipeline file and the override notes; the data goes beside them, ignored
```

## Mods

A `mods/` folder beside `OpenFF.exe`, one mod per subfolder:

- `mod.json` - `id`, `name`, `version`, `author`, `description`, `target` (`openff`, or `steam`
  for a file-replacement mod Crystal installs into a Steam copy), `assemblies`, `dependencies`.
- `files/` mirroring the game's names: any file here replaces the game's. An OpenFF mod is not
  tied to one game - under OpenFF the booted game is an asset source.
- C# assemblies: a `GameService` runs for the mod's lifetime, `Behaviour`s attach to objects,
  `ISaveable`s ride in the save; the client loads them in a collectible context and hot-reloads
  a rebuild within a second. `mods/loadorder.json` orders and enables them; the title screen's
  MODS entry does the same in play.

The API (`OpenFF.Engine`, reached as `Game.*`): `Dialogue`, `Hero`, `Npcs`, `Party`, `Items`,
`Magic`, `Monsters`, `Shops`, `Battle`, `Field`, `Camera`, `Effects`, `Audio`, `Screen`,
`Draw`, `Input`, `Flags`, `Saves`, `Events`, coroutines (`Game.Run`, `Wait.*`), and scene files
(`scenes/<map>.json`) that put behaviours and spawn points on a map. `Samples/HelloMod/install.cmd`
builds the sample into the mods folder; Crystal writes the `mod.json` and the C# project for you.

**`Docs/Modding.md` is the tutorial** - a Steam mod with Crystal, an OpenFF mod's folder, and
code from the first service to scenes and saving - and **`Docs/API.md` the reference**, generated
from the engine by `crystal api-docs`. `Docs/OpenFF-Engine.md` is the design, `Docs/Client-Plan.md`
the log of each slice as it landed.

## Crystal

`crystal.exe` opens Crystal, the editor, in the browser. A project targets **FF3 on Steam**,
**FF4 on Steam** or **OpenFF** (or several at once):

- For a Steam target it edits the game's own files - text, scripts (as source, `.ffs`), tables,
  menu layouts, maps (characters, exits, in 2D and 3D), models with their motions, textures,
  cells - and installs the edits into the Steam copy with a backup, so that anyone with the game
  can play the mod without this client; *Remove* puts the original back.
- For the OpenFF target it exports the project as a mod, makes and builds the mod's C# project,
  attaches behaviours to map objects and points in the 3D view, shows the API reference, and
  starts the client (*Run in OpenFF*).

`Docs/Editor.md` describes all of it; `Docs/Testing.md` has the cases that prove it against the
real games.

## Documentation

| File | What it covers |
| --- | --- |
| `Docs/Modding.md` | How to make a mod: a Steam mod with Crystal; an OpenFF mod's folder, code, scenes and saving |
| `Docs/API.md` | The modding API, every public type and member, generated from the engine (`crystal api-docs`) |
| `Docs/Architecture.md` | The frame, input, rendering, content, the two games, menus, events, tables |
| `Docs/OpenFF-Engine.md` | The engine we are building towards: goal, layers, object model, scripting, saving, the API at a glance |
| `Docs/Client-Plan.md` | The client's stages and the journal of every piece as it landed (long; the running record) |
| `Docs/Editor.md` | Crystal: projects and targets, both games, installing into Steam, every editor |
| `Docs/FF4-Internals.md` | What has been read out of FF4's binary: battle, field, camera, scenes, data files, menus |
| `Docs/Testing.md` | The test cases, by hand and by drive, for the editor and the client |
| `Docs/Drives/` | Scripted key drives for headless regression runs (`--drive=`) |
| `Docs/Content-Pipeline.md`, `Compression.md`, `Graphics.md`, `Text.md`, `Tables.md`, `Audio.md`, `Menus.md`, `Events.md`, `Script-Language.md` | The formats and subsystems, one each |
| `Docs/Porting-Notes.md`, `Modernisation-Plan.md` | How the port was made and how the phone-shaped code was modernised |

## Game data

The client needs the Steam releases of the games and plays from them in place: it finds the
installs on its own, or takes one with `--content=<dir>`. No game file is distributed here and
none is written into the install (Crystal's *Install* into a Steam copy keeps a backup and
*Remove* restores it). `Content/` in this repository holds only the pipeline file and the
override notes (`Content/Override/README.md`); `.gitignore` keeps any game data placed there out
of commits. `Reference/libff4/` carries only what we wrote; `symbols.txt`, `strings.txt` and
`menu-layouts.txt` are regenerated from your own copy of the FF4 binary and files with
`Tools/ff4_dump_all.py` and `Tools/xbn_dump.py` (see `Reference/libff4/INDEX.md`).

This repository's history is the project's, rewritten once to leave the data out; the commits
are otherwise as they were made.

## Controls

| Input | Action |
| --- | --- |
| Arrow keys / WASD | D-pad |
| Z / Space / Enter | A (confirm) |
| X / Backspace / Esc | B (cancel) |
| C / V | X / Y (C opens the menu on FF4) |
| Q / E | L / R |
| Left Shift / Right Shift | Start / Select |
| Mouse | Touch (click, drag) |
| Tab (hold) | Fast-forward |
| F1 | Debug overlay (F2 menu frames, F3 ids, F4 sprites, F5 world text, F6 stats) |
| F12 | Screenshot |

Keyboard input is fed into the game's own NDS-style pad register, so it drives every menu and
field control the DS original supported. Mods read the keyboard through `Game.Input`.

## Diagnostics

Every run writes `logs/ff3.log` beside the executable (the previous run kept as `ff3.prev.log`).

| Switch or variable | Effect |
| --- | --- |
| `--log=general,file` (`all`, `gl`, `texture`, `content`, `sound`, `input`, `event`, `firstchance`) | The log channels; `FF3_LOG=all` does the same |
| `--log-file=<path>`, `FF3_LOG_FILE=<path>` | Write the log elsewhere |
| `--screenshot-dir=<dir> --screenshot-every=<s>` | Capture a picture every N seconds - with `--drive=`, a headless run that can be read from its pictures |
| `--trace=<file>` | A parity trace: what the game did (flags, messages, sounds, maps, everyone on the map at each `say` of the drive); `Tools\parity.ps1` runs a drive with and without mods and diffs two |
| `--debug=all` or `--debug=boxes,labels` | Start with the F1 overlay on |
| `--probe` | A per-frame heartbeat in the log (vertices, bounds, camera, world state) |
| `--ff4table` | List every FF4 script command that is only skipped |
| `FF3_DUMP=<dir>`, `FF3_DUMP_FONTS=<dir>` | Dump decoded source blobs and font atlases as they load |
| `FF3_SPEED=<n>` | Extra update passes while fast-forwarding |

`firstchance` is worth knowing about: large parts of the decompiled game swallow exceptions,
so a porting bug usually shows up as "nothing rendered" rather than a crash. Every scene and
map on FF4 ends with one log line of the script commands it skipped.

## Status and legal

FF3 is complete from its Steam install. FF4 plays its opening and its first maps, battles,
menus and shops on the unified layer and is being brought to the same standard, in the order
`Docs/Client-Plan.md` sets out. The code in `OpenFF/GlobalScope` is the game's own logic
recovered from the mobile build and ported; the games' assets are not distributed here and are
required, in the form of the Steam releases, to run anything. This project is not affiliated
with or endorsed by Square Enix; Final Fantasy is their trademark.

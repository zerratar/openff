# OpenFF

One client that plays Final Fantasy III and Final Fantasy IV (the 3D remakes) from the copies
you own, rendering natively on the desktop, and an engine underneath that mods can reach all
the way down - so that a mod can use both games' content, give each character the progression
system of either game, or make something that is not the game's game at all. Each game stays
playable on its own; a mod can target one game or the client.

It began as a Windows port of the FF3 mobile build - the game's logic recreated in C# from the
Windows Phone release by reverse engineering, and rebuilt on MonoGame. That path - **FF3 from
the Steam install, title to credits, 1:1 with the original** - is the one that works today and
must not break. FF4 (3D) runs through the same code behind a `GameProfile` seam and is the work
in progress; *Status* below says what works, what is ours, and what is still to do.

None of the games' data is in this repository (see *Game data*). MIT licence, for study and
for play with the copies you own (see *Licence*).

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
go. What each release carries is in `Docs/Releases.md`.

## What you need

| To | You need |
| --- | --- |
| **Play**, and **make mods** in Crystal - for the Steam games (files the game itself plays) or for OpenFF (maps, objects, models, sounds, text, definitions, scenes with the built-in components) - and **test them in the client** | Windows 10/11 x64, the release zip (the .NET runtime is inside; nothing to install), and your own copy of **Final Fantasy III** and/or **Final Fantasy IV (3D Remake)** on Steam. The client reads the installs in place; nothing is extracted or copied. |
| **Write C# for a mod** (behaviours, services - Crystal's *Add C# code* and *Build*) | The above, plus the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (the *SDK*, not just the runtime; x64). Crystal builds the mod's project with it. A mod without C# never needs it. |
| **Build OpenFF itself** | The .NET 8 SDK; `dotnet build` fetches MonoGame (DesktopGL), FontStashSharp and NVorbis from NuGet. Python 3 for the scripts in `Tools/` (optional). |

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
  GlobalScope/     the game's logic, recreated from the FF3 mobile build (its own names kept, so the reference stays readable)
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

`firstchance` is worth knowing about: large parts of the game's own logic swallow exceptions,
so a porting bug usually shows up as "nothing rendered" rather than a crash. Every scene and
map on FF4 ends with one log line of the script commands it skipped.

## Status

Three columns matter: what runs **1:1** - the game's own logic, recreated, doing what the
original does; what is **ours** - written for this client where the original's code was not
available or not usable (FF4's engine is a native binary; the phone build's touch shell); and
what is **not there yet**. `Docs/Client-Plan.md` is the journal behind every row,
`Docs/Testing.md` the cases that check them, `Docs/FF4-Internals.md` what has been read out of
FF4's binary so far.

**Final Fantasy III** - complete: title to credits from the Steam install.

| Piece | State | Note |
| --- | --- | --- |
| Field, events, battles, jobs, magic, menus, shops, inns, saves, vehicles, the ending | 1:1 | The mobile build's own logic |
| Rendering | ours | The DS-style GL state is drawn natively on MonoGame (depth, blending, texture formats as the DS had them); the phone's software path is kept as a fallback |
| Text | ours | TrueType at the window's resolution from the Steam build's faces, as the Steam release draws it; `--text=atlas` gives the phone's glyph atlases |
| Music and effects | ours | The Steam build's Ogg files, decoded natively; loop points from the game's own `.dat` |
| Input, window, saves' location | ours | Keyboard/mouse into the DS pad register; a desktop window; `%AppData%\FF3` |
| Touch buttons (Map, Menu) | 1:1 | The phone's, drawn as the phone drew them; a desktop layout is on the list |

**Final Fantasy IV (3D Remake)** - in progress; boots to Baron and plays on.

| Piece | State | Note |
| --- | --- | --- |
| Maps: Baron castle and town, the overworld, dungeons | 1:1 | The shared engine reads FF4's files in place; the world map is FF4's chip layout (z mirrored relative to FF3's - measured, `--fieldmirror`) |
| Field scripts | mostly 1:1 | 247 of FF4's 500 commands run FF3's handler (same command, or renamed, or extra operands); the rest are FF4-only - see below |
| FF4-only field commands (exits `setInsideMapJump`, the name window, confirm boxes, locale waits, reward messages …) | ours | Reimplemented from the binary's disassembly; cosmetic ones (doors' dust, BGM ducking) skip quietly, and every skipped command is logged per map |
| Cutscenes (the `ce_*` scene engine: the Red Wings opening, the flashbacks) | ours, most of it | Casts, motions, camera motions from `EVT_CAMERA.dat`, expressions, bind objects (a spear in a hand), shadows, the message bar, stage swaps run; open: the flight's sky geometry in some shots, scene casts' own shadow discs, lights and toon shading |
| Battles on FF4's stages with FF4's HUD | ours over 1:1 | FF3's battle system drives FF4's stages, monsters, party positions, window art and glove; damage and hit formulas, magic and physical blows from the binary's tables |
| Party data: characters, growth, magic, equipment, items | ours | FF4's tables read from the binary and its files onto the unified data layer (`Shared/Data`) |
| Menu, shops, inns, saves, encounters | ours | The menu from FF4's own `.xbn` layouts over the unified data; shops and inns as its scripts call them; saves on the unified layer; encounters from the map parameters read from the binary |
| Movement tuning | ours | The two tuning tables FF4 compiled into its executable are synthesised in code |
| Not yet | - | Some scene lighting and materials, the rest of the FF4-only commands as they turn up (`--ff4table` lists them), the Steam shell's extras (achievements, its own launcher screens), every item in `Docs/FF4-Internals.md` ▸ *Not yet read* |

**Crystal and mods** - what the editor and the API can do is in `Docs/Editor.md` and
`Docs/Modding.md`; `Docs/Releases.md` sums up each release.

Pull requests are welcome - an FF4 command implemented, a scene fixed, a format read, an
editor that is missing a thing, a mod component. `Docs/Client-Plan.md` says how each piece was
approached and `Docs/Testing.md` how to prove one; the log's `skipped` lines and
`Docs/FF4-Internals.md` ▸ *Not yet read* are the open list. Keep the FF3 path green
(`Docs/Drives/ff3-boot.drive` runs the opening unattended) and add a test case for what you
change.

## Licence and legal

MIT - see `LICENSE`. Do what you like with the code; it comes with no warranty and its authors
carry no liability. It is published for study, preservation and for playing the copies you own
in new ways.

The code in `OpenFF/GlobalScope` is the game's logic, recreated by reverse engineering the
FF3 mobile release and ported; the FF4 parts are written from reading its binary. None of the
games' assets, data or binaries are distributed here or in the releases, and the Steam
releases are required to run anything. Final Fantasy is a trademark of Square Enix Co., Ltd.;
this project is not affiliated with or endorsed by Square Enix.

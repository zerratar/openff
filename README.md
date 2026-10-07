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
| ![The Red Wings deck, Final Fantasy IV's opening scene](Docs/Images/ff4-deck.png) | ![Cecil's title card in FF4's opening](Docs/Images/ff4-scene.png) |
| *FF4's opening on its own scene engine: camera motions, casts, the message bar* | *The opening's shots framed and timed as the Steam game's, measured frame by frame* |
| ![A battle on FF4's battle stage with its HUD](Docs/Images/ff4-battle.png) | ![Picking a foe in FF4's battle](Docs/Images/ff4-target.png) |
| *FF4's battle: its stage, placings, motions and active time, the HUD as Steam lays it out* | *The HUD is a layout and a stylesheet a mod can reshape (Docs/Menus.md)* |
| ![FF4's menu drawn from its own layouts and data](Docs/Images/ff4-menu.png) | ![Crystal, the editor, with an FF3 map open in 3D](Docs/Images/crystal-map.png) |
| *FF4's menu from its own layouts, over the unified party data* | *Crystal: a map in 3D, its characters and exits in the hierarchy, the inspector* |

## Download

The [Releases](https://github.com/zerratar/openff/releases) page has `OpenFF-<version>-win-x64.zip`:
unzip anywhere, `OpenFF.exe` plays, `crystal.exe` edits and makes mods, `mods\` is where mods
go. What each release carries is in `Docs/Releases.md`.

**The modding guide** - short tutorials with pictures, and a reference - is the project site:
[zerratar.github.io/openff](https://zerratar.github.io/openff/). The same pages are `Guide\` in
the zip and *Help ▸ Guide* in Crystal.

## What you need

| To | You need |
| --- | --- |
| **Play**, and **make mods** in Crystal - for the Steam games (files the game itself plays) or for OpenFF (maps, objects, models, sounds, text, definitions, scenes with the built-in components) - and **test them in the client** | Windows 10/11 x64, the release zip (the .NET runtime is inside; nothing to install), and your own copy of **Final Fantasy III** and/or **Final Fantasy IV (3D Remake)** on Steam or GOG. The client reads the installs in place; nothing is extracted or copied. |
| **Write C# for a mod** (behaviours, services - Crystal's *Add C# code* and *Build*) | The above, plus the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (the *SDK*, not just the runtime; x64). Crystal builds the mod's project with it. A mod without C# never needs it. |
| **Build OpenFF itself** | The .NET 8 SDK; `dotnet build` fetches MonoGame (DesktopGL), FontStashSharp and NVorbis from NuGet. Python 3 for the scripts in `Tools/` (optional). |

## Build and run

```
dotnet build
OpenFF\bin\Debug\net8.0\OpenFF.exe
```

With no arguments `OpenFF.exe` finds the Steam or GOG installs, boots FF3 and remembers the choice in
`%LocalAppData%\OpenFF\launch.json`. The switches that matter:

| Switch | Effect |
| --- | --- |
| `--game=ff4` | Boot FF4 (Baron town) instead; remembered for the next start |
| `--content=<dir>` | A specific install, when it is not where Steam or GOG usually puts it |
| `--map=<id> --pos=x,y,z --rot=<deg>` | Any map of either game, no title (`d01_05`, `t01_00`, `f00`, `e01_00`) |
| `--party=4:10 --gil=500 --load=<slot>` | FF4 test starts: members by type and level, gil, a saved slot |
| `--size=1600x960`, `--fullscreen`, `--borderless`, `--windowed`, `--msaa=off|2|4|8`, `--novsync` | Display for this run; the lasting settings are in `%LocalAppData%\OpenFF\settings.json` (size, windowed / borderless / fullscreen, anti-aliasing, vsync, frame rate), written with its defaults on the first start |
| `--fps=30|60|max` | What the display shows: `30` the game's frames as they are; `60` (the default) smoothed - a frame drawn between each two of the game's, with motion and the camera interpolated; `max` smoothed at the display's rate. On a display sixty does not divide, `60` takes the nearest even rate above sixty (72 a second at 144 Hz); the pause menu says what it does on yours. The game's own logic runs 30 steps a second in every mode, so nothing it computes changes. Also in the pause menu's settings (Esc) |
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
Samples/           HelloMod (a service, a behaviour, a save chunk), Survivors (a survivors-style run on the field), Showcase (a map, weapons and models of a mod's own - no code) Mastery (FF3 with FF5's job system and an Abilities screen in the game's own menu), Fellowship (a network mod: travellers on the LAN walk one world, speak, send aid, give items and gil) and Rogue (a roguelike run of FF3's battles from the title: acts, rewards, elites, bosses)
OpenFF/Data/       the client's own screens as data: menus (the Gambits) and HUDs (FF4's battle), layouts and stylesheets a mod can replace
Tools/             Python: the FF4 binary dumps, table generators, format tests; ff4hook, a DLL beside the Steam FF4.exe that drives it and records its frames, cameras and characters to compare against; FramePacerSim, the frame pacer against simulated displays
Docs/              reference, plans and the journal (below)
Reference/         what we wrote about the FF4 binaries (libff4, FF4.exe); the dumps and decompilations are regenerated, not committed
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
(`scenes/<map>.json`) that put behaviours and spawn points on a map. Menus and HUDs are layouts
and stylesheets too (`menus/`, `menus/hud/`), so a mod can reshape a screen without code. `Samples/HelloMod/install.cmd`
builds the sample into the mods folder; Crystal writes the `mod.json` and the C# project for you.

**`Docs/Guide/` is the short guide with pictures** (Crystal's *Help ▸ Guide*; `Guide\` in the
release), **`Docs/Modding.md` the manual** - a Steam mod with Crystal, an OpenFF mod's folder, and
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
- Its start page installs the sample mods and opens or creates projects; the *Mods* tab manages
  the mods installed in OpenFF and in a Steam copy; it checks the game's own files against the
  Steam release's.

`Docs/Editor.md` describes all of it; `Docs/Testing.md` has the cases that prove it against the
real games.

## Documentation

| File | What it covers |
| --- | --- |
| `Docs/Guide/` | **The modding guide**: short HTML tutorials with pictures - getting started, the map editor, a map of your own, items and weapons, cutscenes, assets, code, Steam mods - and a reference. In the release as `Guide\`, and Crystal's *Help ▸ Guide* |
| `Docs/Showcase/` | Pictures of the client and the editor at work, for sharing |
| `Docs/Modding.md` | How to make a mod, in full: a Steam mod with Crystal; an OpenFF mod's folder, code, scenes and saving |
| `Docs/API.md` | The modding API, every public type and member, generated from the engine (`crystal api-docs`) |
| `Docs/Architecture.md` | The frame, input, rendering, content, the two games, menus, events, tables |
| `Docs/OpenFF-Engine.md` | The engine we are building towards: goal, layers, object model, scripting, saving, the API at a glance |
| `Docs/Client-Plan.md` | The client's stages and the journal of every piece as it landed (long; the running record) |
| `Docs/Editor.md` | Crystal: projects and targets, both games, installing into Steam, every editor |
| `Docs/FF4-Status.md` | How far each FF4 subsystem is from playing like the original, in detail |
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
| X / Backspace | B (cancel) |
| Esc, or Start (Options) held on a pad | The client's own menu (a tap of Start is the game's - it skips its waits; the menu reads the pad's own buttons, whatever they are bound to for the game): Resume, Settings (window, size, anti-aliasing, vsync, how the stick runs, the pad's buttons - press one to bind), Exit game. Anywhere, the title included |
| C / V | X / Y (C opens the menu on FF4) |
| Q / E | L / R |
| Left Shift / Right Shift | Start / Select |
| Mouse | Touch (click, drag) |
| Tab (hold) | Fast-forward |
| Alt+Enter | Windowed and full screen in turn (remembered in `settings.json`) |
| Name entry | Type, or with a pad: Up/Down between the name and OK, A opens the on-screen keys (d-pad picks, A types, B deletes, Start is Done) |
| Menu tabs (FF3) | X / Y (C / V, Square / Triangle) turn a menu's tabs - Items / Key Items, Config 1 / 2, Magic's Use / Learn / Remove / Exchange; L / R do too where they are not the character switch (Items, Config) |
| Game pad (XInput, DualShock, DualSense, Switch Pro …) | D-pad or left stick: directions - the stick's push is the pace, a walk part way rising to the full run all the way (`"run": "stick"` in `settings.json`; `"hold"` for the run button instead). Buttons as `settings.json` ▸ `pad` maps them, by default Cross/Circle/Square/Triangle = the DS's A/B/X/Y, L1/R1 = L/R, Options = Start, Share = Select, R2 = run, L2 = fast-forward; any DS button can be given any pad button by name. The first connected pad; plugged in at any time. A pad SDL does not know can be described in a `gamecontrollerdb.txt` beside the executable |
| F1 | Debug overlay (F2 menu frames, F3 ids, F4 sprites, F5 world text, F6 stats) |
| F12 | Screenshot |

Keyboard and game pad input are fed into the game's own NDS-style pad register, so they drive
every menu and field control the DS original supported. Mods read both through `Game.Input`.

## Diagnostics

Every run writes `logs/ff3.log` beside the executable (the previous run kept as `ff3.prev.log`).

| Switch or variable | Effect |
| --- | --- |
| `--log=general,file` (`all`, `gl`, `texture`, `content`, `sound`, `input`, `event`, `firstchance`) | The log channels; `FF3_LOG=all` does the same |
| `--log-file=<path>`, `FF3_LOG_FILE=<path>` | Write the log elsewhere |
| `--screenshot-dir=<dir> --screenshot-every=<s>` | Capture a picture every N seconds - with `--drive=`, a headless run that can be read from its pictures |
| `--trace=<file>` | A parity trace: what the game did (flags, messages, sounds, maps, everyone on the map at each `say` of the drive); `Tools\parity.ps1` runs a drive with and without mods and diffs two |
| `--debug=all` or `--debug=boxes,labels` | Start with the F1 overlay on |
| `--log-stalls` | Write every frame that took over 50 ms to the log ("stall: 240 ms", and whether a screenshot was being written) - to find where a hitch comes from |
| `--probe` | A per-frame heartbeat in the log (vertices, bounds, camera, world state) |
| `--ff4table` | List every FF4 script command that is only skipped |
| `FF3_DUMP=<dir>`, `FF3_DUMP_FONTS=<dir>` | Dump decoded source blobs and font atlases as they load |
| `FF3_SPEED=<n>` | While fast-forwarding, each of the game's steps runs n times over - the same speed at any refresh rate |

`firstchance` is worth knowing about: large parts of the game's own logic swallow exceptions,
so a porting bug usually shows up as "nothing rendered" rather than a crash. Every scene and
map on FF4 ends with one log line of the script commands it skipped.

## Status

How far each piece is from **playing like the original** - the whole game, looking and
feeling as it did - as we judge it. 100% means it does; anything under means something is
still missing, stands in, or is made up. `Docs/Client-Plan.md` is the journal behind every
row and `Docs/Testing.md` the cases that check them.

**Final Fantasy III - complete.** The whole game plays from the Steam install, title to
credits, and looks and feels as the original does: the game's own logic, recreated. What is
ours around it - the native renderer, TrueType text, Ogg sound, the desktop window - shows the
same game, crisper. The parts still in progress are the ones the original never had.

| Piece | Done | Note |
| --- | --- | --- |
| The game: field, events, battles, jobs, magic, menus, shops, inns, saves, vehicles, the ending | 100% | The original's logic; plays and looks as it did |
| Presentation on the desktop: rendering, text, music and effects, the window | 100% | Native and crisp; the Steam release's look |
| Keyboard and mouse | 100% | The DS pad and the touch screen, both |
| Game pad | 80% | Plays everything; the touch-only corners are being given pad paths as they are found (the name entry and Config were two) |
| The client's own UI: the menu, settings, on-screen keys | 60% | New, not the game's; taking shape - its screens are layouts and stylesheets a mod can restyle |

**Final Fantasy IV (3D Remake) - in progress, about 50%.** The world renders - Baron, the
overworld, the dungeons, the opening's scenes - and Cecil walks it. The opening and its first
battle are measured against the Steam game frame by frame (Tools/ff4hook) and written from its
own code (the Android build's libff4, the Steam FF4.exe decompiled): the title, the scenes'
cameras and effects, the message window, the encounter whirl, and the battle's turns, gauges,
blows, numbers, deaths, the monsters' AI and counters, the battle events and the win with its
result pages. The field menu is being rebuilt screen by screen on Steam's measurements - the root,
Inventory, Magic, Equipment, Abilities, Status and Quicksave are done, with OpenFF's Gambits in
the same style; Party, Settings and Steam's Save screen are next. Half the field commands are
still unread, and statuses and most monster abilities are still to come. `Docs/FF4-Status.md` has each subsystem in
detail; `Docs/FF4-Internals.md` what has been read out of the binary.

| Piece | Done | Note |
| --- | --- | --- |
| Maps, models, textures, the world map | 85% | Render as the game's from its files; some scene lighting and materials missing |
| Field: walking, exits, talking, the script commands | 55% | 247 of 500 commands run; the FF4-only ones are being written from the binary as they turn up; scripted walks, turns and the event camera follow as libff4's; the message window and its caption bar as Steam's |
| Cutscenes (the opening, the flashbacks) | 70% | Casts, motions, camera motions on Steam's frames, expressions, bind objects, effects, the message bar, the call into battle; lights, sky and shadow details open |
| Title, logos, map changes | 85% | Steam's logos and title, New Game's white fade, FF4's map fades, CONTINUE resuming a quicksave; the opening movie and Steam's load screen not yet |
| Party data: characters, growth, magic, equipment, items | 60% | Read from the binary and the files onto the unified data layer; HP grown level by level as FF4 rolls it |
| Encounters, shops, inns, saves | 40% | Work as calls; the presentation is not FF4's |
| Battle | 70% | FF4's own: its stages and placings, active time and the turn queue, attacks, effects, sounds, the damage and hit formulas with elements, races and rows, numbers, deaths, the monster AI (conditions, action sets, target types, counters), the battle events (battle_ai.bbd), statuses, magic and summons, the bosses, the openings (preemptive strike, back attack, surprised), escape, the win and its result pages; every party command (Jump, Kick, Steal, Twincast, Bardsong...) checked frame by frame against the Steam game |
| Menus | 50% | Steam's field menu, measured off its draws, as layouts a mod can restyle: the root with the lead's thought, Inventory, Magic, Equipment, Abilities, Status, Quicksave, and OpenFF's Gambits in the same style; Party, Settings, Save screen and save points, Sort to come |
| The Steam shell (achievements, launcher screens) | 0% | Not started |

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

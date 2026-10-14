# OpenFF

OpenFF plays **Final Fantasy III** and **Final Fantasy IV** (the 3D remakes) from the copies you
own on Steam or GOG, as a native desktop game, and lets mods change almost anything in them.

- **FF3 is complete**: title to credits, played exactly as the original.
- **FF4 is in progress** (about 80%): it plays from the title to the ending, matched frame by
  frame against the Steam game. See [FF4 status](Docs/FF4-Status.md).
- **Crystal**, the editor, makes mods: maps, text, models, scenes and C# code, either for OpenFF
  or installed straight into your Steam copy.

No game files are included. You need your own copy of the games.

| | |
| --- | --- |
| ![Final Fantasy III's title](Docs/Images/ff3-title.png) | ![Ur, in Final Fantasy III](Docs/Images/ff3-ur.png) |
| *FF3's title, with crisp text at any window size* | *Ur: FF3 plays as shipped* |
| ![The Red Wings deck in FF4's opening](Docs/Images/ff4-deck.png) | ![Cecil's title card in FF4's opening](Docs/Images/ff4-scene.png) |
| *FF4's opening scene* | *Scenes framed and timed as the Steam game's* |
| ![An FF4 battle](Docs/Images/ff4-battle.png) | ![Picking a foe in an FF4 battle](Docs/Images/ff4-target.png) |
| *FF4's battles, by FF4's own rules* | *The battle screen is a layout a mod can restyle* |
| ![FF4's menu](Docs/Images/ff4-menu.png) | ![Crystal with an FF3 map open in 3D](Docs/Images/crystal-map.png) |
| *FF4's menu* | *Crystal, the editor* |

## Get started

1. Download `OpenFF-<version>-win-x64.zip` from [Releases](https://github.com/zerratar/openff/releases)
   and unzip it anywhere. Nothing to install.
2. Have **Final Fantasy III** and/or **Final Fantasy IV (3D Remake)** installed from Steam or GOG.
3. Run `OpenFF.exe`. It finds the games by itself and starts FF3. For FF4, start it once with
   `OpenFF.exe --game=ff4`; it remembers the choice.

You need Windows 10 or 11 (64-bit). [What's in each release](Docs/Releases.md).

## Playing

Arrow keys move, **Enter** confirms, **Backspace** cancels, **Esc** opens OpenFF's own menu
(settings, exit). Game pads work too. All keys: [Controls](Docs/Controls.md).

What OpenFF adds on top of the originals:

- Native rendering at any window size, smoothed to 60 fps or more.
- Fast-forward (hold Tab).
- Game pad support with rebindable buttons.
- Mods, including OpenFF's own extras such as Gambits.

Command-line switches (start on any map, window size, logs): [Command line](Docs/Command-Line.md).

## Making mods

Put a mod in a folder under `mods\` beside `OpenFF.exe`. A mod can:

- **replace game files**: models, textures, text and sounds;
- **restyle screens**: menus and HUDs are layouts and stylesheets;
- **add maps, scenes and items**;
- **run C# code** through the OpenFF API (the hero, the party, battles, the camera, saves and more).

Mods can be turned on and off from the title screen.

**Crystal** (`crystal.exe`) is the editor. It opens in your browser and can:

- edit the games' own files and install the changes into your Steam copy (with a backup, and
  *Remove* to undo);
- export an OpenFF mod, set up and build its C# project, and run it in OpenFF.

Where to read:

- [The modding guide](https://zerratar.github.io/openff/): short tutorials with pictures (also
  *Help ▸ Guide* in Crystal)
- [Modding manual](Docs/Modding.md): everything about making a mod
- [API reference](Docs/API.md): every type and member a mod can use
- [Crystal](Docs/Editor.md): every editor and what installing into Steam does
- [Samples](Samples/): working mods to learn from

The samples include a sample mod with code, a survivors-style game on the field, FF3 with FF5's
job system, a roguelike run of FF3's battles, a LAN mod where players walk one world together,
and a menu restyle.

## Building from source

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (x64).

```
dotnet build
OpenFF\bin\Debug\net8.0\OpenFF.exe
Crystal.Editor\bin\Debug\net8.0\crystal.exe
```

`publish.cmd` builds a self-contained `dist\OpenFF\` folder (the release zip's contents).
Writing C# for a mod also needs the .NET 8 SDK; mods without code don't.

How the code is laid out: [Architecture](Docs/Architecture.md). How changes are tested:
[Testing](Docs/Testing.md).

## Status

**Final Fantasy III: complete.** The whole game plays as the original: its own logic,
recreated from the mobile release, with native rendering, text and sound around it.

| FF3 | Done |
| --- | --- |
| The game: field, events, battles, jobs, menus, saves, vehicles, the ending | 100% |
| Rendering, text, music and sound on the desktop | 100% |
| Keyboard and mouse | 100% |
| Game pad | 80% |
| OpenFF's own menus and settings | 60% |

**Final Fantasy IV: in progress.** Written from FF4's own code (the Android build and the Steam
executable) and checked frame by frame against the Steam game.

| FF4 | Done | Still open |
| --- | --- | --- |
| Battle | 100% | |
| Field: walking, exits, talking, events, vehicles | 85% | Some details per map |
| Maps, models and the world map | 85% | Some lighting and materials |
| Title and map changes | 85% | The opening movie |
| The Fat Chocobo's extras, name entry | 85% | |
| Cutscenes | 80% | Sky and shadow details |
| Menus | 80% | Some screens |
| Party data: growth, magic, equipment, items | 60% | |
| Shops, inns, saves, encounters | 75% | Encounter details |
| Steam extras (achievements) | 0% | |

Details for every part: [FF4 status](Docs/FF4-Status.md). What has been read out of FF4's
code: [FF4 internals](Docs/FF4-Internals.md).

## Contributing

Pull requests are welcome: an FF4 piece finished, a scene fixed, a file format read, an editor
feature, a mod component. Please keep FF3 working (`Docs/Drives/ff3-boot.drive` plays its
opening unattended) and add a test case for what you change ([Testing](Docs/Testing.md)).
The open work is in [FF4 status](Docs/FF4-Status.md) and [FF4 internals](Docs/FF4-Internals.md).

## All documentation

| Document | About |
| --- | --- |
| [Modding guide](Docs/Guide/) | Tutorials with pictures |
| [Modding](Docs/Modding.md), [API](Docs/API.md) | Making mods, and the API reference |
| [Editor](Docs/Editor.md) | Crystal |
| [Controls](Docs/Controls.md), [Command line](Docs/Command-Line.md) | Keys, pads, switches, logs |
| [FF4 status](Docs/FF4-Status.md), [FF4 internals](Docs/FF4-Internals.md) | How far FF4 is, and what is known of its code |
| [Architecture](Docs/Architecture.md), [Engine design](Docs/OpenFF-Engine.md) | How OpenFF is built |
| [Client plan](Docs/Client-Plan.md) | The project's running journal |
| [Testing](Docs/Testing.md), [Drives](Docs/Drives/) | Test cases and scripted test runs |
| [Menus](Docs/Menus.md), [Text](Docs/Text.md), [Tables](Docs/Tables.md), [Audio](Docs/Audio.md), [Graphics](Docs/Graphics.md), [Events](Docs/Events.md), [Script language](Docs/Script-Language.md), [Content pipeline](Docs/Content-Pipeline.md), [Compression](Docs/Compression.md) | File formats and subsystems |
| [Porting notes](Docs/Porting-Notes.md), [Modernisation](Docs/Modernisation-Plan.md) | How the port was made |
| [Releases](Docs/Releases.md) | What each release contains |

## Game data and legal

OpenFF reads the games from your own install and never copies or ships their files. Crystal's
*Install* into a Steam copy keeps a backup, and *Remove* restores it. This repository contains no
game data: `Content/` holds only the pipeline file and [override notes](Content/Override/README.md),
and `Reference/libff4/` only what we wrote (see [its index](Reference/libff4/INDEX.md) for how the
dumps are made from your own copy).

MIT licence, see [LICENSE](LICENSE). The code is for study, preservation and playing the copies
you own in new ways, with no warranty. The FF3 logic in `OpenFF/GlobalScope` was recreated by
reverse engineering the mobile release; the FF4 parts are written from reading its binaries.
Final Fantasy is a trademark of Square Enix Co., Ltd. This project is not affiliated with or
endorsed by Square Enix.

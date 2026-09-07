# Making mods

Two kinds of mod exist, for two audiences:

| | A **Steam mod** | An **OpenFF mod** |
| --- | --- | --- |
| Plays on | the Steam release of FF3 or FF4, unmodified client | the OpenFF client |
| Made with | Crystal | Crystal, or a text editor and the .NET SDK |
| Can change | the game's own files: text, scripts, tables, menu layouts, maps, models, textures | the same, plus anything code can do through the engine API, and content from both games at once |
| Installed by | Crystal, into the Steam copy, with a backup; *Remove* restores the original | dropping a folder into `mods/` beside `OpenFF.exe` |
| Shared as | a `.zip` of the project (Crystal installs it on the other machine) | the mod folder |

The first is for people who own the game and want it changed; the second is for people who
want to make something the game could not do. Both start from the same place.

`Docs/API.md` is the reference for everything named here; `Docs/Editor.md` describes Crystal
in full; `Docs/Testing.md` has the test cases that prove each step against the real games.

## Part 1 - a Steam mod with Crystal

You need the Steam release installed and the repository built (`dotnet build`).

1. Start `crystal.exe` (in `Crystal.Editor\bin\Debug\net8.0\`). It finds the Steam installs
   and opens in your browser with a tab for each game it found.
2. **File ▸ New project**, name it, and pick the target: **FF3 on Steam** or **FF4 on Steam**
   (or both - a project can carry edits for either game; each file shows a badge for its game).
3. Edit. Everything you touch is a copy in the project; the game's files are not changed yet.
   - **Text**: `files/babil_menu.msd` (FF4) or `files/menu.msd` (FF3) - pick a line by its id,
     type, *Save*. Dialogue is in the map's own `.msd` (`t01_00.msd` is Baron town).
   - **Scripts**: a map's `.script` opens as source (`.ffs`): the same commands the game's
     designers used, with completion for the game you target. Change a `startMessage` id, a
     flag, an exit's destination; *Save* compiles it back.
   - **Tables**: `item_parameter.pak` and friends as rows with named columns - an item's
     attack, a monster's HP, a shop's wares.
   - **Maps**: characters (add one with a model, a position and a line; delete one), exits
     and their destinations, in 2D and 3D.
   - **Menus**, **Textures**, **Models**, **Images**: the layouts, sheets and models as they
     are, with export to PNG / glTF and import back.
4. **Project ▸ Install**. Crystal writes the edited files into the Steam install (repacking
   a container when a file lives in one), keeping a backup of every original and an
   `installed.json` listing what it did. Start the game from Steam: the mod is live.
5. **Project ▸ Remove** puts every original back. **File ▸ Changes…** shows what the project
   changes and lets you revert one file. Steam's *Verify integrity of game files* finds
   nothing to fix after a Remove.
6. **File ▸ Export as .zip…** packs the project with a README. Whoever receives it opens it
   in their own Crystal and presses *Install* - no OpenFF client involved.

What a Steam mod cannot do: add commands the game does not have, use the other game's
content, draw its own UI, or run code. That is what the OpenFF target is for.

## Part 2 - an OpenFF mod: files

The client reads a `mods/` folder beside `OpenFF.exe`, one mod per subfolder:

```
mods/
  my-mod/
    mod.json
    files/                the game's own file names; a file here replaces the game's, whichever game runs
      babil_menu.msd
      t01_00.script
    ff3/files/            optional: files that apply only when FF3 is played
    ff4/files/            optional: ... only when FF4 is played (the two games name files alike)
    scenes/               optional: behaviours and points placed on maps (Part 3)
    MyMod.dll             optional: code (Part 3)
  loadorder.json          written by the client; order and enabled flags
```

`mod.json`:

```json
{
  "id": "my-mod",
  "name": "My mod",
  "version": "1.0",
  "author": "you",
  "description": "What it does, in a sentence.",
  "target": "openff",
  "assemblies": [ "MyMod.dll" ],
  "dependencies": [ { "id": "hello", "minVersion": "1.0" } ]
}
```

- `target` is `openff`. (A `steam` target is a file-replacement mod Crystal installs; the
  client does not load those.)
- An OpenFF mod is not tied to one game: under OpenFF the booted game is an asset source, so
  the same mod applies whether FF3 or FF4 was started, and a mod may carry files for both.
  `ff3/files/` and `ff4/files/` are read only for their game (a `d01_01.script` is Ur in one
  and a Baron corridor in the other); `files/` is read for either. Code and scenes apply to
  both.
- Files: the first mod in the load order wins a file two mods both carry; the log's `mods:`
  lines say what applied and which conflicts fell which way.
- `loadorder.json` is written for you the first time a folder appears; the title screen's
  **MODS** entry lets you enable, disable and reorder in play (it applies at the next start).
- Crystal makes this folder for you: a project with a game ticked under **OpenFF mod** (New
  project, or Project settings) has **Project ▸ Export to OpenFF**, which writes `mod.json`,
  `ff3/files/` and `ff4/files/` (one per game ticked - tick both and the mod may take from
  either game), the built code and the scenes into the client's mods folder; **Run in OpenFF**
  exports and starts the client. In the editor the mod's own things - its code and its scenes -
  are the **OpenFF mod** folder at the bottom of the project tree, beside the games' libraries.

## Part 3 - an OpenFF mod: code

### The project

With Crystal: **Project ▸ Add C# code…** writes `code/<Name>.csproj` referencing the client's
`OpenFF.Engine.dll`, a starting `Mod.cs`, and a `.gitignore`; **Build C# code** compiles it;
**Open C# code in editor** hands the project to Visual Studio, Rider or VS Code; **Export to
OpenFF** carries the assembly into the mod folder and names it in `mod.json`. The same things
sit on the **OpenFF mod ▸ Code** row of the project tree, where the files list and open in the
editor itself (line numbers, colouring, Ctrl+S; **New file…** starts a Behaviour, a
GameService or an empty class; a build's errors click through to the line), so a small mod
never needs an IDE, and a large one has *Open in IDE* one click away.

By hand: a class library targeting `net8.0` that references the engine without copying it -
the client shares its own `OpenFF.Engine.dll` with every mod, and a mod folder must not carry
another. `Samples/HelloMod/HelloMod.csproj` is the template:

```xml
<ProjectReference Include="..\..\OpenFF.Engine\OpenFF.Engine.csproj" Private="false" />
<!-- or, outside the repository: -->
<Reference Include="OpenFF.Engine"><HintPath>C:\...\OpenFF\bin\Debug\net8.0\OpenFF.Engine.dll</HintPath><Private>false</Private></Reference>
```

Build straight into the mods folder: `dotnet build -o "<client>\mods\my-mod"` (with
`mod.json` and `scenes/**` as `None` items copied to the output). `Samples/HelloMod/install.cmd`
does exactly this. Rebuild while the client runs and it **hot-reloads** the mod within a
second: the old service's public fields are handed to the new instance, its objects are
remade, and the log says `mod my-mod: reloading`. A build that fails to load leaves the mod
out with a message, never a crash.

### The three things a mod is made of

**A `GameService`** - one instance for the whole run, never destroyed by a map change. It
hears `OnGameStart`, `OnUpdate` (when `WantsUpdate` is true), `OnSceneLoaded` /
`OnSceneUnloading` when a map comes and goes, and `OnQuit`; it subscribes to events; across a
hot reload its public fields survive.

```csharp
using OpenFF;
using OpenFF.Events;

public class MyService : GameService
{
    public int MapsEntered;                       // survives a hot reload

    public override void OnGameStart()
    {
        Game.Events.Subscribe<MapEntered>(e =>
        {
            MapsEntered++;
            Game.Log("entered " + e.Scene.Name + " (" + e.Scene.Type + ", " + e.Scene.Source + ")");
        });
        Game.Events.Subscribe<BattleStarting>(_ => Game.Log("a battle starts"));
    }
}
```

**A `Behaviour`** on a `GameObject` - `Awake`, `Start`, `Update`, `LateUpdate`, `OnDestroy`,
as in Unity. Objects a mod creates on the running map (`Game.World.Legacy`) are destroyed when
the mod unloads or the map changes; a behaviour on a map character reaches it through the
`MapObject` component beside it (`GetComponent<MapObject>().Npc`).

```csharp
public class Spinner : Behaviour
{
    public float DegreesPerFrame = 2f;            // public fields show up in Crystal as editable
    protected override void Update() => Transform.Yaw += DegreesPerFrame;
}

// somewhere in a service:
GameObject marker = Game.World.Legacy.Add(new GameObject("marker") { Owner = Mod });
marker.AddComponent<Spinner>();
```

**`ISaveable`** - the service's state travels with the save slot the game writes, in a chunk
of its own (`ChunkId`, `ChunkVersion`, `Save()` returns an object serialised as JSON, `Load`
gets the version and the data). A chunk from a mod that is no longer installed is kept, not
deleted.

```csharp
public class MyService : GameService, ISaveable
{
    public int Talks;
    public string ChunkId => "my-mod/MyService";
    public int ChunkVersion => 1;
    public object Save() => new { Talks };
    public void Load(int version, JsonElement data) => Talks = data.GetProperty("Talks").GetInt32();
}
```

### Acting on the game

Everything goes through `Game.*`; every service is an interface the client implements over
the running game, and a mod registering its own implementation later in the load order
replaces one for everybody. The whole list is `Docs/API.md`; the shape of it:

| | |
| --- | --- |
| `Game.Dialogue` | `Say(text)` in the field's window, `Ask(question, yes => ...)` with the game's yes/no box, `IsOpen` |
| `Game.Hero` | `Position`, `Yaw`, `Teleport`, `Face`, `LookAt`, `MoveTo`, `Freeze`/`Unfreeze`, `PlayMotion`, `BindBattleMotions` |
| `Game.Npcs` | `Spawn(model, position, yaw)` a character, `SpawnModel` any model (a monster, an object), `Existing(index)` the map's own; an `Npc` moves, turns, talks (`Interacted`), fades, scales |
| `Game.Party` | `Members` with their sheets, `Gil`, `Items`/`AddItem`/`RemoveItem`, `Equip`, `Hurt`/`Heal`, `GiveExperience`, `SetJob`, `LearnSpell`, `Inflict`/`Cure` |
| `Game.Items`, `Game.Magic`, `Game.Monsters`, `Game.Shops` | the tables as data; `Magic.Cast`/`CastOn` play the game's own effects, `Damage`/`Healing` are the game's formulas, `Magic.Add` a spell of your own; `Shops.Open(row)` the game's shop screen |
| `Game.Battle` | `Start(monsterParty)` the game's own battle; `BattleEnded` says Won, Lost or Escaped |
| `Game.Field` | `Map`, `Warp`, `GroundHeight`/`OnGround`/`Walkable`, `Encounters` on and off |
| `Game.Camera` | `MoveTo`, `LookAt`, `Follow`, `Shake`, `Zoom`, `Reset`, `WorldToScreen` |
| `Game.Effects`, `Game.Audio`, `Game.Screen` | the game's effects by id; BGM and SE by name; fades, flashes, floating numbers |
| `Game.Draw` | immediate-mode text, rectangles, lines and sprites (PNGs the mod ships, `LoadTexture`) over the frame, in 800x480 units |
| `Game.Input` | the pad (`Held`/`Pressed`/`Released`), the pointer, the keyboard by key name; `Capture = true` takes all input away from the game while the mod uses it |
| `Game.Flags` | the scripts' flag space |
| `Game.Events` | `Subscribe<T>`: `MapEntered`, `MapLeaving`, `FlagChanged`, `MessageShown`, `CutsceneStarted`/`Ended`, `BattleStarting`/`Ended`, `ItemGained`, `WarpRequested`, `SaveWritten`/`SaveRead`, `ModReloaded` |
| `Game.Run(IEnumerator)` | a coroutine, one step per frame; yield `Wait.Frames`, `Wait.Seconds`, `Wait.Until`, `Wait.Dialogue`, `Wait.Walk(npc)`, `Wait.HeroWalk`, or another routine |

A scene as one coroutine, from the sample:

```csharp
private IEnumerator Talk(Npc npc)
{
    Game.Hero.Freeze();
    Game.Hero.LookAt(npc.Position);
    Game.Dialogue.Say("Hello from a mod!");
    yield return Wait.Dialogue();

    bool? answer = null;
    Game.Dialogue.Ask("Would you like 10 gil?", yes => answer = yes);
    yield return Wait.Until(() => answer != null);
    if (answer == true) { Game.Party.Gil += 10; Game.Camera.Shake(20, 0.5f); }

    npc.MoveTo(npc.Position + new Vector3(0, 0, 8), 60);
    yield return Wait.Walk(npc);
    Game.Hero.Unfreeze();
}
```

Positions are world units (the engine hides the game's 1/4096ths), yaw is in degrees, screen
coordinates are the 800x480 the game was designed for. The field's own buttons are taken (X
opens the menu, a shoulder button zooms, Select toggles encounters), so mods read keyboard
letters: `Game.Input.KeyPressed("T")`.

### Scenes without code: `scenes/<map>.json`

Crystal's map view puts behaviours from your built assembly onto a map's characters, its
exits, the terrain, or **points** you place and drag in 3D (`Points (OpenFF)` in the
hierarchy). The result is saved as `scenes/<map>.json` in the project and exported with the
mod; the engine reads it when the map is entered and makes a `GameObject` per target with the
behaviours and their fields set. A point is a `GameObject` named `<map>/point:<name>` with
its tags, so a mod finds spawn points with `Game.World.Legacy.WithTag("spawn")`. The scenes
a project has are listed under **OpenFF mod ▸ Scenes** in the project tree; opening one
opens the map itself, in 3D with its behaviours and points, and the inspector offers the
JSON as text for when that is what you want.

```json
{
  "map": "d01_05",
  "attachments": [
    { "target": "map", "behaviour": "Welcome",
      "fields": { "Text": "The Altar Cave.", "Delay": 120 } }
  ],
  "points": [ { "name": "north", "x": 12.0, "y": 0.0, "z": -20.0, "yaw": 180, "tags": [ "spawn" ] } ]
}
```

### Seeing what happens

- The log (`logs/ff3.log` beside the client, `--log=general,file`): `engine: mod my-mod 1.0: 1
  service(s), 2 behaviour type(s)`, your own `Game.Log` lines, and `engine: WARNING ...` for
  an exception a script threw - the game goes on; the mod's call did not.
- **F1** in the client: the debug overlay lists the engine, the mods, each service's
  `DebugLines()`, the objects and their behaviours.
- `--nomods` starts the client without mod code (the mods' files still apply).
- `Docs/Drives/` and `--drive=<file>` play a scripted run without touching the keyboard, for
  a test that repeats.

### The samples

- `Samples/HelloMod` - a service, a behaviour, a save chunk, a villager who talks through a
  coroutine, and a scene file; `install.cmd` builds it into the mods folder.
- `Samples/Survivors` - a survivors-style run on the field: waves of goblins from the map's
  spawn points, auto-aimed spells with the game's own effects and formulas, run levels with
  cards, chests with equipment, a trader who opens the game's shop. Everything it uses is in
  the API table above; it is the best single answer to "what can a mod do".

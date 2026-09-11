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

Everything before this part needs nothing installed: the release zip, the Steam games, and
Crystal. Code is the one thing that does - **the .NET 8 SDK** (the *SDK*, x64, from
[dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0); the runtime the
zip carries is not enough to compile), because Crystal's *Build C# code* runs `dotnet build`
on the mod's project. A mod of maps, models, sounds, text, definitions and the built-in
components never compiles anything and never needs it.

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
| `Game.Dialogue` | `Say(text)` in the field's window, `Ask(question, yes => ...)` with the game's yes/no box, `IsOpen`. A text of the form `@1000142` is one of the game's own lines by its .msd id, in the player's language; `@1000142 item=5001 gold=250 color=9` fills its item and gold codes and sets the text colour |
| `Game.Hero` | `Position`, `Yaw`, `Teleport`, `Face`, `LookAt`, `MoveTo`, `Freeze`/`Unfreeze`, `PlayMotion`, `BindBattleMotions` |
| `Game.Npcs` | `Spawn(model, position, yaw)` a character, `SpawnPlain` the scripts' bootPlainCharacter kind, `SpawnModel` any model (a monster, an object), `Existing(slot)` / `ByRow(row)` the map's own; an `Npc` moves, turns, talks (`Interacted`), fades, scales |
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
exits, the terrain, or the mod's **own objects**, which you place and drag in 3D
(`Objects (OpenFF)` in the hierarchy). The result is saved as `scenes/<map>.json` in the
project and exported with the mod; the engine reads it when the map is entered and makes a
`GameObject` per target with the behaviours and their fields set, and takes them all down
again when the map is left.

The mod's own objects are the part of this that owes the game nothing. **Add a game
object** on an OpenFF project offers *OpenFF object* first: no `.hich` row, no cast, no
script - a `GameObject` named `<map>/<name>`, tagged `scene` and whatever tags you give it,
with a `MapObject` of kind `scene`. Give it a **model** (any of the game's - `o001` is the
chest) and the client shows it as a plain character standing there, with `MapObject.Npc`
to move, turn, hide or talk through; leave the model off and it is a spot with logic on it -
a spawn point, a mark, a trigger. Everything about it is a field in the inspector and a
property in the file, changeable at any time: its name (attachments on it follow), its
model, its place, yaw and scale, its tags, its parent. Objects nest: **Add a child object**
puts one under another, the hierarchy shows the tree, and a child's x/y/z, yaw and scale
are relative to its parent (turned by the parent's yaw, scaled by its scale), so moving the
parent moves the lot - in the editor and in play. `GameObject.Parent` and `.Children` carry
the tree in the engine, and `Transform` knows it: `Position`, `Rotation`, `Scale` are the
values relative to the parent (the world's for an object at the top), `WorldPosition`,
`WorldYaw` and `WorldScale` the resolved ones, settable either way; `SetParent(parent)`
keeps the object where it stands in the world unless told `keepWorld: false`. A model
spawned for an object follows its transform, so a behaviour that slides the parent slides
the children and their models. Attachments target an object by its path: `"chest"`,
`"chest/trigger"`.

From code the objects are `SceneObjects.Find("chest")` (the current map's, by path),
`SceneObjects.All()`, and **`SceneObjects.Spawn("chest", at, yaw)`** - a new object from
the file's definition of that path, with its model, tags and behaviours (the fields as the
file has them), named `<map>/chest#N`: one authored object as the template for many, a
Destroy to take one down. A behaviour's field of type **`ObjectRef`** names another object
on the map by path; Crystal offers the map's objects to pick from, a rename follows, and
`field.Resolve()` is the `GameObject` (`.Link` its `MapObject`, `.Link.Npc` its model).
A behaviour deriving **`SavedBehaviour`** (Save/Load, as `ISaveable`) has its state written
with the game's save and back on load, keyed by the object it is on and its type.

A mod finds them with `Game.World.Legacy.Find("d01_05/chest")` or by tag:
`Game.World.Legacy.WithTag("spawn")`.

**Built-in components.** The engine has behaviours of its own, in the Add Behaviour list
under *Built in* whether or not the mod has code, so the usual things need no C# at all:

| | fields | does |
| --- | --- | --- |
| `Chest` | Item (picked from the game's list), Count, Gil; Once, Message, EmptyMessage, Flag, Animate | gives the contents and says so when the player talks to it (a model: face it and press A, the game's own talk), or, without one, presses A standing within Radius - approach, then interact; OnWalkIn makes it act the moment the hero walks in instead. Once remembers it across saves; Flag is the game's own chest flag; Animate plays the lid, the sound and the sparkle as the game does (the shut lid held at its motion's end when the object appears, not swung shut). Message `@` (the default) is the game's own words for the contents ("The chest contained Potion.", "You find Potion." for an o000 item spot), `@<id>` any line of the .msd, other text as written with {what} the contents; EmptyMessage empty says nothing, as the game's opened chests. An o000/o001 model is the game's map object whichever way, and the game's own treasure logic steps aside for the component (`Npc.OwnChest`) |
| `Talk` | Speaker, Lines (one per line), FaceHero, When, Then | says the lines one window at a time when talked to (face it and press A; without a model, A within Radius, or on walking in with OnWalkIn); a wanderer stops and turns for the talk and walks on after, as the game's talkBegin/talkEnd do; When is the flags it applies under, Then the flags it sets - several Talks on one object, the first that applies speaks |
| `Trigger` | Radius, Once | raises `Entered`/`Left` and publishes `Events.TriggerEntered` / `TriggerLeft` (with the `GameObject`, so its tags say which) when the hero comes within Radius; logs `trigger <map>/<path>: hero entered` |
| `GameCast` | Cast, OnBoot; Setup; Treasure, Item, Gil, Flag; Recolour | the object *is* that cast of the map's script: talking to it runs `cast<N>_main` and the script's commands on cast N land on it (`Npc.RunCast`). *Setup* is the boot's own commands on the cast, one per line as the disassembly writes them (`setTreasureItem(15, 5001, 1, 22, 0, 0)`, `bindMotion(21, "w_light_old")`, `setCharacterDetectionRadius(…)`, `setSignEffect(…)` - any of them), replayed through the game's script engine once the stand-in runs the cast (`Npc.RunScript`); `[flags] command(...)` runs one only while the flags hold. *OnBoot*: a scene's actor - not spawned with the map but when the script boots the cast (`Events.CastBooted`), where it boots it, and driven by the scene from there. Treasure and Recolour are the same two things as fields, for an object made by hand (`Npc.SetTreasure`, `Npc.Recolour`). What Crystal's exact conversion puts on a stand-in - the same code on the same kind of character, set up by the same commands, 1:1 |
| `Motion` | Set, Index, Loop | binds a motion set and plays a motion from the start - the boot's `bindMotion` + `startMotionCharacter`, the villagers' idle sway |
| `Wander` | Ai (Still, Wander, Follow), Gait | the object's character walks about its spot exactly as the map scripts' `moveCharacter_StartRandom` does (`Npc.StartWander`: no autopilot, no operator, the random-move AI, the gait - Man, Woman, Boy, Girl, Uncle, Aunt, OldMan, OldWoman, the walk's pattern and pace); Still is `moveCharacter_EndRandom` |
| `[FlagField]` on a string | | the inspector offers the map's flags to pick from (who tests and sets each) and checks the shape - what `WhenFlags.When`, `Talk.When/Then`, `Chest.Flag`, `GameCast.Flag` carry |
| `WhenFlags` | When, Live | the object is there only while the flags hold - not even spawned until they do; hidden and its behaviours off otherwise. `When` is "0:14 !0:11" (all of them), or alternatives with `|` ("!0:14 \| 0:14 !0:11" - either), as a boot reached by more than one path has. *Live* follows the flags while the map is up; off, they count once at the map's start, as the game's boot tests do (what the conversions set) |
| `Removed` | HideOnly, StandIn | on one of the *game's* characters (`object:N`): takes it off the map when the map is entered - what Crystal's conversions leave on the original, so the mod's stand-in (spawned first) is the only one there |

`Talk` also takes `When` and `Then` - flags as "0:14 !0:11" (group:index, `!` for off). Of
several Talks on one object the first whose `When` holds is the one that speaks, and `Then`
is set after its last line - the shape of a villager's flag-branched cast, which is what
Crystal's *Convert the map's characters* turns them into. An object with a model is spawned
as a plain figure unless marked **character** in the inspector (`"character": true`), which
gives it the game's walker: it turns to the player, can `Wander`, is talked to the game's way.
The scripts boot characters two ways, and **plain** (`"plain": true`, under *character*) picks
the second: `bootPlainCharacter`'s light walker with the model's own scale and kind (a child's
model at 0.8, the chocobo, the frog) - `Npcs.SpawnPlain` against `Npcs.Spawn`. The converter
keeps whichever the original had.

So a chest is: an OpenFF object with the chest's model (`o001`), a `Chest` on it, the item
picked in the inspector - every one of those changeable later. `Chest` and `Talk` derive
from `Interactable` (talked to with a model; A within Radius without one, or walked into with OnWalkIn) and are not sealed: a
class of your own deriving `Chest` and overriding `OnOpened`, or `Talk` and `OnSaid`, or
`Interactable` and `Activate`, is a component with everything the built-in one has plus
yours, and shows in the list under the mod's name. What `Chest` remembers lives in
`SceneMemory` (`Has`/`Mark`/`Forget` by key), one chunk in the engine's saves, which a mod's
code may use for its own once-only things.

**Editor fields.** Every public field (or settable property) of a Behaviour is an editor
field: the inspector shows it with an input for its type - number, text, checkbox, enum
list, x/y/z, a colour, a list of strings one per line - filled with the code's default, and
the scene file carries what was typed. The attributes in `OpenFF` say more, the way Unity's
do: `[Header("Contents")]` puts a heading over the fields that follow, `[Tooltip("…")]` is
the hover text (the `///` summary otherwise), `[Range(1, 99)]` makes a slider, `[ItemField]`
on an int offers the game's item list, `[HideInInspector]` keeps a public field out.

```csharp
public class GiveKey : Chest
{
    [Header("Quest")]
    [Tooltip("The flag set when the key is taken")]
    public int Flag = 12;

    protected override void OnOpened(string what) => Game.Flags.Set(0, (uint)Flag, true);
}
```

The scenes a project has are listed under **OpenFF mod ▸ Scenes** in the project tree;
opening one opens the map itself, in 3D with its objects and behaviours, and the inspector
offers the JSON as text for when that is what you want.

```json
{
  "map": "d01_05",
  "objects": [
    { "name": "Chest", "x": 110, "y": 52, "z": -30, "yaw": 90, "model": "o001", "tags": [ "chest" ],
      "children": [ { "name": "Trigger", "x": 0, "y": 0, "z": 6, "yaw": 0 } ] },
    { "name": "north", "x": 12, "y": 0, "z": -20, "yaw": 180, "tags": [ "spawn" ] }
  ],
  "attachments": [
    { "target": "map", "behaviour": "Welcome", "fields": { "Text": "The Altar Cave.", "Delay": 120 } },
    { "target": "chest", "behaviour": "GiveItem", "fields": { "Item": 30 } },
    { "target": "chest/trigger", "behaviour": "Trigger", "fields": { "Radius": 10, "Once": true } }
  ]
}
```

Files from before the objects had `"points"` (a flat list, no model) and targeted them as
`point:<name>`; both still read, as objects without a model, and Crystal writes the new
shape the next time the scene is saved.

### Cutscenes on a timeline: `Cutscene`

A `Cutscene` component plays a **Timeline** - tracks of clips on a time axis, laid out the
way Unity's Timeline window lays a cutscene out, in the timeline dock under the 3D view
(the component's *Open timeline…* button). Put it on any object; it starts on talking to
the object (or walking in, with *On Walk In*), the moment the map is entered, or only from
code (`GetComponent<Cutscene>().Play()`), gated by *If* flags, once per save with *Once*,
setting *Then* flags when it ends. The hero stands still while it plays (the pad stays on,
so the player advances the lines), B skips to the end with every remaining clip run to its
final state, and the camera goes back on the hero at the end.

The tracks and their clips:

| track | clips |
|---|---|
| **object** (one of the scene's objects; blank is the component's own) | *move* - walk (a character, with its walking animation) or glide (a model) to a point; *turn* - to a yaw or toward `@hero`, an object or a point; *motion* - a motion by index, a set bound first; *show / hide*; *fade* - a character's alpha; *scale*; *glTF clip* - an animation of a `Mesh` |
| **hero** | move, turn, motion on the player's character |
| **camera** | *camera* - glide to a position looking at a target (from where it stands, or from a given start); *follow hero*; *shake*; *zoom* |
| **dialogue** | *say* - a line, holding the playhead until it is dismissed; *ask* - a yes/no with flags set either way |
| **screen** | *fade out*, *fade in*, *flash* |
| **audio** | *sound* (archive, number), *music*, *stop music* |
| **game** | *flags*, *wait*, *warp* (ends the cutscene), *battle* (ends it), *give item*, *signal* - a `CutsceneSignal` by name for code (`Game.Events.Subscribe<CutsceneSignal>`) |

In the dock: *+ Track…* adds a row; double-click a lane to add the track's first kind of
clip there, right-click for all of them; drag a clip to move it, its right edge to change
its length; the ruler scrubs the playhead and Space plays. Scrubbing previews in the 3D view
- objects move, turn, scale, fade and hide as the clips say, a camera clip moves the view -
and a line, a fade or a sound shows as a caption. The picked clip's arguments sit in the
column on the right; a point takes *← selection* (the selected object's spot), *← view*
(where the 3D view stands, or what it looks at) or *pick…* (a click on the ground). Every
edit autosaves and is one undo step. **Play in OpenFF** exports the mod and starts the client
on the map with the cutscene playing (`--cutscene=<object path>`).

In the file the field is plain data, so a cutscene can be written or generated by hand:

```json
{ "target": "Crystal", "behaviour": "Cutscene", "fields": {
  "Begins": "OnTalk", "Once": true, "Then": "0:14",
  "Timeline": { "tracks": [
    { "name": "Camera", "kind": "camera", "clips": [
      { "type": "camera", "start": 0, "length": 2, "args": { "position": { "x": 12, "y": 22, "z": 42 }, "target": { "x": -10, "y": 4, "z": 5 }, "ease": "smooth" } },
      { "type": "follow", "start": 6, "length": 0, "args": {} } ] },
    { "name": "Fox", "kind": "object", "target": "Fox", "clips": [
      { "type": "move", "start": 0.5, "length": 2, "args": { "to": { "x": 20, "y": 0, "z": 5 }, "walk": true } },
      { "type": "turn", "start": 2.5, "length": 0.5, "args": { "at": "@hero" } } ] },
    { "name": "Dialogue", "kind": "dialogue", "clips": [
      { "type": "say", "start": 3, "length": 1, "args": { "text": "The crystal hums.", "speaker": "" } } ] }
  ] } } }
```

Times are seconds; `ease` is `smooth` (the default), `linear`, `in` or `out`. The player
is `OpenFF.Engine/Timeline.cs` (`Cutscene`), the dock `Crystal.Editor/wwwroot/timeline.js`;
the clip types and their arguments are listed in both and kept in step by hand.

### A cast's code as the mod's own: `CastScript`

The third conversion, *Convert to OpenFF scripts*, gives each converted talker a `CastScript`
beside its `GameCast`: the cast's main function as text, one line per entry, exactly as
Crystal's disassembly writes it -

```
flagOnJump(0, 14, loc_419E);
call(2, 0xB6744D73);                 // the library's talk begin
startMessageWindow(0);
startMessage2(0, 0x98E569, 0, 0);    // "Aren't those your friends making a ruckus…"
deleteMessageWindow(0);
flagOn(0, 13);
call(2, 0xC39DEA76);                 // talk end
end();
loc_419E:
…
```

- and the code is the mod's: it lives in the scene file, the inspector shows it as a text
field, a modder changes a line, a flag, a message id, adds a branch. In play the engine
compiles the lines with the script language's own compiler (`Shared/Script/Ffs`, the same
one `crystal script-build` uses) into a script of the mod's, registers it with the game's
logic manager beside the map's under a map number of its own, and starts the cast's main
when the object is talked to - through the same `startLogic` the game's talk uses. The
game's interpreter runs every command, so each means exactly what it means in the game;
the `GameCast` beside it binds the cast's row to the object (`Npc.BindCast`) so the
commands that name the cast reach it, and replays the boot's setup as before. The
converter leaves a cast on `GameCast` alone when its main reaches outside itself (a jump
to another function's label, a `call` into the map's script), with the reason in the plan.
Judge: `Tools\parity.ps1` - Ur's four drives (chest, boy, elder with the item menu, the
opening scene) agree with the game's own run after the conversion.

An object of the mod's own can have code too - a villager you placed, nothing converted.
Add a `CastScript` and leave `Cast` at 0: the engine gives the object a cast number of its
own (from 5000 on each map; the map's script never has those) and claims a free row of the
map's cast table for it (`.hich` has 48; a map uses the first few, the rest are cleared at
every map change), so the game's commands find it as they find any cast. In the lines
`@me` stands for that number:

```
bindMotion(@me, "w_light_old");
call(2, 0xB6744D73);
startMotionCharacter(@me, 1002, 0, 5, 0);
startMessageWindow(0);
startMessage2(0, 40000001, 0, 0);
deleteMessageWindow(0);
startMotionCharacter(@me, 1001, 1, 5, 0);
call(2, 0xC39DEA76);
end();
```

The message id is the mod's own line - the next section.

Writing the code: the CastScript card shows the first lines and *Edit code…* (or *Write
code…*) opens them in a tab of their own - the script editor Crystal uses for the game's
`.ffs` files. Command names complete as you type (Ctrl+Space asks), the strip under the
editor says what each argument of the command under the caret is (`flagOn group:word,
index:word`), *Commands…* lists all 298 with what they take and puts one at the caret, and
the lines are compiled as the client will compile them a moment after each change
(`Ffs.CastCode`, the same frame on both sides): a wrong name or a missing bracket shows
under the editor with its line, and a click goes there. What you type is the scene's
within half a second - the map's autosave takes it from there.

### Lines of the mod's own: `defs/text/<name>.json`

Text is a definition as an item is. Under *OpenFF mod ▸ Strings*, *New text file…* makes a
`defs/text/<name>.json` of message id → line, the first free id filled in, and opens it as
a table: a row per line with its id, a *Copy* button for the id, the words (Enter breaks
the line; Ctrl+Enter adds a row after), and ×. *Add line* takes the next free id. Every
change saves itself; an id that is not a number, is used twice, or sits in the game's own
range is marked red with the reason. The JSON the file holds sits in a pane underneath
(collapsed by default, like the menu editor's XML): it follows the table, and *Apply to the
table* reads it back after a hand edit. *Open as JSON* has the file in the code editor
instead. The file:

```json
{
  "40000000": "Hello there, traveller!\nWelcome to Ur.",
  "40000001": "%shuyaku1% looks tired.\nCome back tomorrow."
}
```

Ids from 40000000 are the mod's to use (the game's own stop far below); `\n` breaks the
line in the window and the game's control codes (`%shuyaku1%` the first hero's name,
`%unfixed_item%` the item a chest gave) work as in its own text. Say a line with `"@id"` in
a `Chest`'s or `Talk`'s text field (the inspector shows the line under the field), with
`startMessage2(0, id, 0, 0)` in a `CastScript`, or `Game.Say("@40000000")` from C#. In play
the client appends every mod's lines to `eureka_permanent.msd` - the file every map falls
back to - as it is read (`Shared/Data/ModText.cs`), so the game's own message window says
them; a Steam project has the composed file written into its `files/` at Install and
Export, like the item tables. Two mods with one id: the first in load order keeps it.

A line can be written per language - the first step of localisation, no more yet:

```json
{ "40000003": { "en": "Welcome!", "de": "Willkommen!", "ja": "ようこそ！" } }
```

The client picks the game's language setting (`ja`, `en`, `fr`, `de`, `it`, `es`, `zh-CN`,
`zh-TW`, `ko`), then English, then the first written; the table shows the English and keeps
the rest as they are (the JSON pane edits them).

### Items of the mod's own: `defs/items/<id>.json`

An item is a definition, not code. In Crystal, under *OpenFF mod ▸ Items*, *New item…* asks
for a name and the game's item to start from (a Hi-Potion for a stronger potion, a sword
for a new sword - FF3's own ids: weapons 1000-2309, armour 3001-3331, magic 4001-6660,
consumables 5001-5122, key items 5201-5241); the definition opens in the inspector - the name, the caption, the shop
prices, and every field of the base's record by the game's own name (`usedPower`,
`aggressivity`, `phylacticPower`, `equipJob`…) with the base's value greyed in. Set a field
to change it, clear it to fall back to the base's. Every change saves itself.

The fields are shown for what they are, from the game's own enums (itm/*.cs): a weapon's
*Kind* (knife, sword, bow… - which motion set swings it), its *Model* (the w### battle
model - graphId is that number; *View* opens it in the model viewer), *Jobs* as checkboxes,
*Damage type* (grapple, slash, blow, charge), *Inflicts* (the status a hit may give, at
*Status chance %*), *Casts when used*; armour's *Kind*, *Guards against*; a spell's *School*,
*Level*, *Element*, *Status*, *Targets*; a consumable's *Casts*, *Power*, *Element*. Plain
numbers (Attack, Defence, Accuracy %, the five stat bonuses, Weight) are numbers with a tip.
A field whose meaning has not been read yet (tckType, calculate) says so and takes a raw
number. In the file everything is a number, as the game's record holds it. The file:

```json
{
  "id": "hi-potion-plus",
  "number": 20001,
  "base": 5002,
  "name": "Hi-Potion+",
  "caption": "Restores 999 HP to one ally.",
  "buy": 1500,
  "fields": { "usedPower": 999 }
}
```

`number` is the item id the game knows it by - a Chest's `Item`, `Game.Party.AddItem`, a
save - given once by Crystal (from 20001 up) and kept for good; the [ItemField] picker
lists the mod's items after the game's, marked "(mod)". *Export to OpenFF* copies `defs/`
into the mod. In play the client appends every enabled mod's definitions to
`item_parameter.pak` and their names and captions to `eureka_item.msd` as the game reads
those files (a content-chain transform, `Shared/Data/ModItems.cs`), so menus, shops, chests
and `Game.Items` see them as the game's own; with no definitions anywhere the files pass
through untouched, and `--nomods` reads them as shipped. Two mods that claim one number:
the first in load order keeps it. A Steam project gets the same items the native way: at
Install and in the zip the two files are composed from the shipped ones and written into
the target's `files/`, so the Steam game reads them as any replaced file. FF3 for now;
FF4's tables get their own composer.

#### A weapon's own look: `"model": "assets/blade.glb"`

On the OpenFF target a weapon definition may name a glTF of the project's, and the client
draws it in the hero's hand in battle in place of the game's `w###` model. The inspector's
*Look* card (weapons only) picks it from the project's `assets/` - *Pick…* shows them as
pictures with *Import a model…* for a Blender export, *View* opens the file - with a *Scale*
and a *Clip* (an animation of the file to loop while the weapon is held: a glowing rune, a
turning gem). The file:

```json
{
  "id": "rune-blade",
  "number": 20002,
  "base": 1001,
  "name": "Rune Blade",
  "caption": "A blade with a light of its own.",
  "model": "assets/rune-blade.glb",
  "modelScale": 1,
  "modelClip": "Glow"
}
```

Model the weapon in the hand's frame, the `w###` frame: the grip at the origin, the blade
along +Z, the guard across Y (a sword is about 7 units long - `w005` runs z -0.9 to 6.2). A
shield's face is the XY plane with its boss toward -Z, tilted about 35° toward +Y and centred
near (0, 0.3, -0.4) - the forearm's angle; the game's `w260` is a round one of radius 1.6.
`modelScale` scales the file into it, and a file made another way is turned and moved rather
than re-exported: `"modelRotation": [x, y, z]` (degrees, applied about X, then Y, then Z) and
`"modelOffset": [x, y, z]` (the hand's units) - the *Rotation* and *Offset* rows of the card.
A shield exported facing +Z wants a rotation of `[35, 180, 0]`. The same fit goes into *Write
as a w### model* below, baked into the vertices, so one definition places the model alike on
both targets. Rather than guess the numbers, the card's **On a character…** opens the model
viewer with the file in a character's hand: the viewer's *on a character* toggle puts one of
the game's party models (j101…) under the viewed weapon, the transport plays that
character's battle motions (`b_b01`: the idle, the swings), and the weapon follows the hand
joint each frame exactly as the game poses it (`R_te` / `L_te`; a shield the forearm,
`R_ude` / `L_ude`; then the game's own grip turn and offset). The *fit* fields there - scale,
rotation, offset - move the model live and save to the definition as you change them
(arrow keys nudge a field, Shift for ten steps), so a Blender export is fitted to the attack
animations by eye. The toggle works on any weapon model, the game's `w###` too, for
comparison; off, the viewer shows the model on its own as before. Not yet: while the
character is on, the transport is the character's - the glTF's own clips (the Rune Blade's
glow) do not play in the viewer; the client plays them in battle regardless. The record's
*Model* (graphId) still names the model
the game loads and poses - the base's is fine - and *Kind* still picks the swing motions; the
game's weapon character is hidden, shown, faded and shrunk exactly as before (its render
object gets a stand-in, `CRenderObject.StandIn`, and only the draw is the glTF's:
`OpenFF/Compat/WeaponMeshes.cs`). The log says `weapons: item 20002 drawn as rune-blade.glb`
when a battle sets it up and `weapons: rune-blade.glb drawn in the hand at x, y, z` the
first time it shows (the weapon shows in the swing, not while standing, as the game does).

A Steam target plays only the game's own formats, so `model` is left out there: the look is
the *Model* field. Two ways to a new one: *Duplicate as…* an existing `w###` and *Replace
with a PNG* on its texture (a new colour on the same shape), or the glTF converted into the
game's own model - the *Look* card's **Write as a w### model** writes the picked glTF as
`w###.nmdp.lz` (BMD0/MDL0) and `w###.ntxp.lz` (its textures) into the target's `files/`,
under the first number from 300 the game does not use, and sets *Model* to it; the card
offers this on OpenFF projects too, as the game-format alternative to the glTF look. The
same at the command line: `crystal mdl-import blade.glb w300 [out-dir] [--scale=n]`. What
the converter makes (`Crystal.Editor/Mdl0Write.cs`, laid out as `w005.nmdp` is): one node,
one material and one shape per glTF material, a display list of triangles with normals and
texture coordinates (VTX_16 under a position scale that fits the model; the meshes as the
file's node tree places them, so a Blender hierarchy flattens), one pal256 texture per
material - its PNG times the base colour, or an 8x8 of the colour - resampled to a power
of two and kept under the package's 512 KB of texels. Not carried: skins and animations (a
skinned model comes in at its bind pose), vertex colours, JPEG pictures (the base colour
stands in, with a note). The model viewer opens the result like any `w###`, and the client
plays it the game's way (a w### of the mod's is a file override, so `--nomods` shows the
game's).

#### A character remade: a mesh of yours on the game's skeleton

*Export .glb* on a character (`j101`…) or monster gives Blender the model with its real
skeleton - the game's node tree, bones where the joints are, the game's weights including
the blended ones at knees, elbows and hips - and, with *+ all motions*, every motion of its
battle pack as an action (`j101.b_b01.glb`). Model over it, weight to the same bones (the
vertex groups carry the game's names - `hara`, `mune`, `L_ude`, `R_te`…), check the mesh
against every swing in Blender, export a `.glb` with the skin. Then, in the model viewer
with a project open, **Remake from glTF…** picks the file (the picker imports one too) and
writes the model back in the game's format over the original - `files/j101.nmdp.lz` and its
`.ntxp.lz` as the project's overrides - so it plays in the OpenFF client and the Steam game
alike with every motion the game has. The viewer reloads on it; **Undo remake** takes the
override out. The same from the command line: `crystal mdl-reskin hero.glb j101 [out-dir]
[--target=steam]`.

What the writer does (`Crystal.Editor/Mdl0Reskin.cs`): keeps the original's node
dictionary, node data, envelope matrices and the node-building part of its SBC byte for
byte - `NODEDESC` with parents and stack slots, the billboards, `NODEMIX` blends - so the
`.ncap` motions, which name nodes by index, drive it as before and the battle's weapon hangs
from the same `R_te`; replaces the shapes, materials and textures with the file's. The
mesh's vertices at the file's rest pose are moved into the game's model space by the root
joint (an armature moved in Blender still lands; a note says how far the rest pose is from
the game's bind pose if it differs), and each vertex is sent through the matrix stack slot
whose weights are nearest its own - a node's slot (one bone, weight 1) or one of the
envelope slots the original had (its blends of several nodes) - stored in that slot's space
and restored by the display list. A node the original drew without a slot (`j101`'s head)
gets its own shape right after its `NODEDESC`, through the node's matrix, as the original
does. Smooth weights the original never had are snapped to the nearest; the result reports
how many. A mesh without weights is bound to the nearest bone. Limits the Steam game
imposes: one bone (or one of its blends) per vertex; 10,922 triangles per model (its vertex
buffer, `GlobalScope.DrawModel`); 20,480 vertices per `BEGIN_VTXS` run, which the writer
splits itself; 255 shapes; 512 KB of texels. The round trip is exact: the game's own `j101`
exported and written back lands on the original to 1/4096 in the bind pose and through
every frame of the idle and a swing, and a Blender re-export of it the same.

**On the OpenFF target the client draws the glTF itself.** Remake on an OpenFF project
writes, besides the game-format model, `defs/models/j101.json`:

```json
{ "model": "j101", "gltf": "assets/luneth-hd.glb" }
```

and the client (`OpenFF/Compat/CharacterMeshes.cs`, `Shared/Data/ModModels.cs`) takes the
model's draw over wherever the game draws it - field, battle, menus. That one file is the
whole of the binding: there is no character definition to it. To see what a running game
draws, the log says `models: j101 looks like assets/luneth-chibi-rigged.glb` at start and
`models: j101 first drawn as luneth-chibi-rigged.glb - 25 of 25 joints reached, retargeted`
at the first draw; in Crystal, the game model (`j101` under Models, in the project panel or
open) says *in OpenFF: drawn as assets/… (defs/models/j101.json)*. The client reads the
definition from the mod folder it plays from - *Play in OpenFF* exports the project into the
client's `mods/<project>` first, so a save in Crystal reaches a game started after it, not
one already running (start the game again). The game's `j101`
still loads and animates - its node tree, its `.ncap` motions, the hand the weapon hangs
from, its shadow, alpha and LOD are untouched - and at each draw the stand-in walks the
model's SBC exactly as the game would, with the shapes masked off and the frame's node
matrices written down, then skins the glTF through them on the CPU: every vertex through
up to four joints with the weights as Blender painted them, the file's inverse bind
matrices times the game's node matrices. So: real smooth weights, any triangle count, any
texture size and count, materials with a base colour tint. The vertex colour is the one the
game gives its own vertices at the same draw - the scene's light times the original
model's diffuse and ambient plus its emission, per material by index (the export keeps
`m0, m1, m2`) - so the character is exactly as bright as the game's beside it and dims with
it in a dark place (checked: mean brightness 131.4 native, 131.8 glTF on the same frame).
The joints match the model's nodes by name; the inverse bind matrices are the file's, so
the armature must stay where the export put it. A joint the model lacks is noted and left
out of the blend; a mesh with no skin rides on the model's root node.

**One character, not every one: a Look.** A definition dresses every instance of a model -
every `n021` in every town. To dress *one* character on *one* map (a villager in Ur as the
chibi, one guard in a uniform of his own), pick a glTF of the project's as that character's
model in the map editor: a `.hich` row can only name one of the game's models, so the row's
model stays as it is (the character still loads, walks, talks and animates as the game has
it) and the editor puts a **Look** behaviour on the character instead (`object:<index>`,
`Path` the file, `Fitted` from the file's rig record), shown in its Behaviours and in the 3D
view; *Draw the game's model again* takes it off. The client draws the file in the
character's place through the same stand-in as a definition, skinned by that character's own
model and motions (`CharacterMeshes.AttachOwn`). From code the same is `Npc.SetLook(path,
fitted)` and `Game.Hero.SetLook(...)` (null puts the game's draw back) - a disguise, a
uniform handed out at run time. A Look wants a file rigged to the game's bone names (a
Remake's export, the auto-rig's `-rigged.glb`); one rigged another way still goes through
the client's retarget by name table. OpenFF only.

**A mesh with no rig at all** is bound to the character's skeleton for you (`AutoRig`;
`crystal mdl-autorig file.glb j101`): Remake with such a file writes `assets/<name>-rigged.glb`
first and goes on with that. The mesh is scaled to the character's height and stood on its
feet (or as `scale`/`rotation`/`offset` in the request say); then every vertex takes the
game's own weights from the nearest point of the original mesh's surface (the three nearest
triangles, the corners' weights blended by where the point lies, the nearest triangle
counting far more), smoothed once over the mesh. Because a modelled character stands with
its arms down while the game's bind pose is a T, the original is first put into a frame
that looks like the file - the battle idle's first frame, chosen when the file's proportions
are nearer it than the T - the weights are taken against that, and the file's vertices are
then carried back into the bind pose through the weights they got. Without this the sleeves
take the chest's weights and swing as a shirt while the arm stays. Two more things stand
between a nearest-surface transfer and a mesh of other proportions, and both are handled.
The matched pose is alike as a whole, not limb by limb - the file's forearm hangs straight
where the game's angles forward - and carried through the game's changes that difference
would stay as a permanent turn of the forearm off its bone; so once the first pass says which
vertices each arm bone owns, each arm limb is turned about its joint onto the game's posed
bone before the carry (arms up to 90°, legs up to 35°; feet and head are never turned - a
foot points off its shin, a head's middle is where its hair is). And nearest surface cannot
tell a chin from a chest: a big-headed file's lower face sits at the height of the original's
chest and neck, its baggy sleeves beside its belly, and they would take those bones. So the
file is cut by its own shape into regions - the head above the *neck* cut (found: the
narrowest horizontal slice between the head's widest and the shoulders'); the arms outside
the *torso width* between the *arm floor* and the neck (found slice by slice: the torso is the
cluster of a slice's vertices about the middle, an arm a cluster a gap away from it); below
the *hips* cut (found: the model's hip joint) the legs where a slice has two clusters, one a
side, and a *skirt* where it hangs as one piece across the middle; the rest the body - and
each region takes weights only from the like part of the original, strictly: the head from
`atama`'s triangles, a sleeve from its side's `sakotu`/`kata`/`ude`/`te`, a leg from its
`momo`/`hiza`/`sune`/`asi` and `kosi`, a skirt from `kosi` and the body, the body from the
body and hips (and, without a skirt, the legs - a tunic's hem flew out with the legs because
the original blends its thighs into its waist). What those triangles blend in from outside
the region (the neck under the jaw) goes to the region's heaviest bone, and the smoothing
after still softens the cuts. **The cuts are yours to move.** Open the unrigged file under
Models and pick *auto-rig cuts* in the view's toolbox (the properties come up in the
inspector): each cut is a slider with an *auto* box (auto: found from
the shape, the found value shown), drawn on the model as a frame at its height (neck violet,
hips orange, arm floor green) and two blue lines at the torso's width; *skirt* and *regions*
are switches; *Find* re-reads the shape with your settings and counts each region's vertices
without writing anything; *Auto-rig* binds the file with them. The cuts you set are saved
beside the file as `assets/<name>.rig.json` and used again by *Remake from glTF…* and *Redo
auto-rig*; the CLI takes them as `--neck=63 --hips=38 --arm-floor=27 --torso=28`,
`--no-skirt`, `--no-regions` (percentages). Not every model is cut alike: a long coat wants
the hips cut at the waist, a character in trousers wants *skirt* off. **Or place markers**, the
way an auto-rigger asks for them: pick *markers* in the toolbox and click the model at the chin, a
wrist, an elbow, a knee and the groin (*symmetry* places the other side's too); each shows as a
ring in its colour, the chips below say which are placed and which comes next, a chip
right-clicked takes its marker off. Placed markers set the cuts a slider leaves on auto - the
neck just under the chin, the hips at the groin, the arm floor under the wrists - and, more
usefully, tell the auto-rig where the arms really bend and point: the elbow is the forearm's
pivot and the wrist its direction, the upper arm points at the elbow, the knee is the shin's
pivot, in place of what the shape alone suggests (the boundary rings between one bone's
vertices and the next). They are saved with the cuts beside the file and used by every rig of
it after; *Cuts & markers…* on the rigged file opens the original where they are.

**A fitted skeleton** (*fitted skeleton* in the same bar; OpenFF only) is the other road, and
the better one for a character whose proportions are not the game's. Instead of binding the
file to the game's joints where they are, the auto-rig writes the game's skeleton - its names,
tree and bone orientations - with every joint moved to where *the file* has it: the shoulder
where the sleeve leaves the torso, the elbow and wrist at the markers (else at the rings between
one bone's vertices and the next), the hips over the knees at the groin's height, the ankle over
the knee a little above the foot's lowest, the spine up the torso's middle with the head's base
at the chin. Weights go by distance to those bones, which now lie inside the mesh, within the
regions; the file is carried into the T-pose about the fitted joints. The definition gets
`"fitted": true`, and the client drives the file by retargeting - the game's rotations about
the file's own joints, positions down the file's own tree (the other-rig path, with every bone
matched by its own name and no fit of its own) - as the viewer does, which tells a fitted
skeleton from the game's by where its joints sit. The game's motions move nodes as well as
turn them - `trans` carries the hop, the knockback and the fall to the ground, a shoulder
slides in a swing - and the retarget carries each node's move within its parent along with
its turn (it once carried only the root's: a felled character lay in the air with a sleeve
stretched to where its node had been, while Crystal, which always carried them, showed it
right). Limbs of the file's own lengths bend where
the file bends: no pivot a hand's breadth from the elbow, no forearm that turns about a point
beyond it. What the game's proportions still decide are contacts: a hand that reaches the hip
on the game's Luneth reaches wherever a shorter arm reaches. The DS format has one skeleton per
model, so a Steam target keeps the bound rig. The result is a skinned
glTF with the game's joint names at the game's bind pose: exact on OpenFF, the game format
for Steam and the viewer. The closer the mesh's volume to the original's, the better the
weights land; a mesh of quite other proportions (a realistic body on the chibi skeleton)
is a case for the retarget below instead.

**A model's own animations.** A glTF that carries animation clips may play them in place of
the game's motions - when it has them, for the motions you say; everything else stays the
game's. In the definition:

```json
{ "model": "j101", "gltf": "assets/hero.glb",
  "clips": { "idle": "Idle", "walk": "Walk", "run": { "clip": "Run", "sync": false, "speed": 1.2 },
             "attack": "Slash", "damage": "Hit", "victory": "Cheer", "b01_003_01": "Surprise", "1004": "Walk" } }
```

A key is a role - `idle` (battle 101, field 1001), `walk` (1004), `run` (1005), `attack` (every
weapon swing, 1101-2401; a monster's 201), `damage`, `death`, `magic` (4001-4003), `victory`
(4101-4104), `guard`, `item`, `escape`, `poise`, `levelup`, `front`, `back`, `cover`, `steal`,
`jump`, a monster's `special` - or a motion's id, or its name in the pack (`b01_003_01`, as the
viewer's transport lists them). The value is the clip's name, or `{ "clip", "sync", "speed" }`:
in step (the default) the clip's whole length runs over the game motion's frames, so a swing
lands when the game's does and a loop keeps the game's pace; not in step, the clip runs at its
own pace, looping. The game's motion keeps playing underneath - the walk still moves the
character, the battle still times its turn - only the picture is the clip's, posed down the
file's own skeleton (the game's export, a fitted one, or another rig's) and stood where the
game has the character. In Crystal, open the file under Models: the transport offers *the
file's own clips* as a pack to play them, and *own clips* under the viewer is the map - a row a
role, *Add motion* for any motion by id or name, *Save clips* writing it into the definition.
From C#: `Game.Hero.PlayClip("Wave")` (or on an `Npc`) plays one of the model's clips over
whatever the game is doing, until it ends - `loop: true` until `StopClip()` - and
`Game.Hero.Clips` lists what the model carries; `ClipPlaying` says whether one is on. A
character with no glTF look, or a look with no clips, answers false and plays the game's.

**Another rig's model** (Mixamo, Tripo, Rigify, a hand-made one) is retargeted by the
client at draw time: its bones matched to the game's by a table of the usual names
(`Hips, Spine1, Head, LeftArm, R_Forearm, mixamorig:RightHand`...) plus the definition's
`"bones"`, twist and helper bones following their nearest matched ancestor; the file scaled
to the model's height and stood on its feet, its up and facing found from the rig (a file
facing -z is turned round); each limb bone turned into the game bone's direction at bind;
and the game's motion applied as each node's world rotation from its bind pose about the
file's own joint, positions running down the file's own hierarchy so limbs of other lengths
stay in one piece. `--retarget-force` sends the export's own rig through this path, which
then renders the same as the direct one - the check of the maths. Proportions still tell:
the game's motions are made for a chibi with legs four units long, and on a realistic body
the same joint rotations read as odd. That is what the auto-rig above is for.

**The viewer plays the game's motions on a skinned glTF**: open the rigged or remade file
under Models (the project's assets are listed first) and the transport offers the game
model's packs - the model a `defs/models` definition binds it to, else j101 when the joints
are the character bones - skinning the file in the browser with the model's node matrices
per frame (`/api/model/rig-pose`). Orbit, zoom, scrub: a wrong weight shows at once.

**The view's toolbox.** The model view's tools are icon buttons floating at the view's left,
top to bottom: *look* (no tool on the mesh: the left button orbits, the middle button - or
Shift with the right - drags the view, the wheel zooms, on every model), the view's switches
(*bones*, *wireframe*, and with weights on *heat map* and *all bones*), the brushes (*assign*,
*add*, *erase*, *smooth*), and on an unrigged file of the project's *auto-rig cuts* and
*markers*, on a file with clips of its own *own clips*. The active tool's properties sit in
the **inspector** as a card above the model's facts - the brush's bone, radius, strength,
mirror and its buttons; the cuts' sliders and marker chips; the clip rows - so the view itself
never resizes as a tool comes and goes, and the readout of the vertex under the cursor floats
over the view's bottom right for the same reason. Every tool has a key, printed on its button:
**Q** look (Esc too), **B** bones, **W** wireframe, **G** the backdrop (dark, grey, light, sand -
a stretched sleeve shows against a light one), **H** heat map, **C** all bones' colours,
**1 2 3 4** assign / add / erase / smooth, **X** cuts, **M** markers, **L** clips; with a brush
on, **[ ]** step the radius and **{ }** the strength, **Ctrl+Z** undoes a stroke, **Ctrl+S**
saves the weights, and **Up / Down** step the bone once one is picked in the hierarchy. The
keys work wherever the focus is except in the project panel (its own list) and in a field.

The mouse, with a brush on: the left button paints; **Ctrl** held makes the stroke an erase
and **Shift** a smooth, whichever brush is picked (as Blender has them), so a wrong bit is
taken back or blended without changing tools; the **right button dragged** sets the brush -
sideways the radius, up and down the strength - with the ring following (the inner ring is the
strength: a full brush fills the ring, a light one is a dot at its middle) and the numbers by
the cursor; **Alt + drag** orbits and **Alt + click**, not moved, picks the bone under the
cursor; the middle button pans, the wheel zooms. With no brush on, the left and right buttons
both orbit.

**What a file is for: the Kind card.** A `.glb` asset picked in the project panel has a *Kind*
card in the inspector, above *Normals*: *character*, *prop / scene object*, *weapon*, *shield*,
*map / terrain*, *battle map* - or *as the file says*, which is what an unflagged file counts
as: a skinned file is a character, a plain one a prop. The flag is for a file whose name and
shape do not say (a sword exported as plain geometry, a rigged fox that is no character) and
is written into the file (`asset.extras.kind`; `/api/project/models/kind`). What reads it: a
weapon or shield gets the *on a character* preview under the viewer (a flagged shield sits on
the forearm from the start), a prop or a map does not; and every character of the project's
is offered as the body under a weapon, ahead of the game's `j###` - so a weapon is seen in
*your* character's hand. A glTF body is skinned by the viewer through the game model its skin
is bound to (a rigged export, an auto-rig's `-rigged.glb`; one bound to no model has no motions
to play and is refused with a word) and the weapon hangs from *its* hand joint - a fitted
skeleton's own wrist, where the retarget puts it this frame - through the same grip and item
fit as on the game's model.

**Normals, an importer's way.** A `.glb` asset picked in the project panel has a *Normals*
card in the inspector, as Unity's model importer has: *from the file* (as exported) or
*calculate*, with a smoothing angle - faces meeting at less than it share a normal, an edge
sharper than it stays hard; 60° is the usual, 180° smooths everything - and *Apply*. The
normals are recalculated from the triangles across the seams where the file split a vertex
(a UV island's edge, a primitive's edge), so those smooth over too, and written into the
`.glb` (`/api/project/models/normals`, `Gltf.RecalculateNormals`); the file's own are kept
inside it (`_SOURCE_NORMAL`, `asset.extras.normals` records the angle) so *Revert* is exact,
and the card and the facts say which the file has. The viewer, the map editor and exports
shade by them. The client draws a character unlit, in the game's own material colours, as
the DS does, so a character's normals show in Crystal and in Blender, not in play. A file
faceted on export - every vertex its face's normal, the chibi's look - is the case for it.

**And paints the weights.** Pick a brush on a skinned file of the project's: the mesh becomes
a heat map of one bone's weight (blue none, green half, red all) and the left button paints
while Alt + drag orbits and the middle pans. Pick the bone from the card's list, from
the hierarchy's *Bones* tree (the file's joints nested as the file has them, each row folding
its children away; the inspector says what the bone moves; with a bone picked there, Up and
Down step to the one above or below as the tree shows them, from the 3D view too - not from
the project panel, whose arrows walk the assets), or Alt+click the mesh to take
the bone under the cursor. Under the cursor, the vertex's bones are read out with their
weights (`hara 50% · mune 42% · kosi 8%`), each chip in its bone's colour; *all bones* colours
the whole mesh by each vertex's heaviest bone, the hierarchy's bone rows carrying the same
colours, so the whole assignment is visible at once. *assign* (the default) makes the ring
the picked bone's outright - the tool for "this part belongs to that bone"; and the geometry
follows the paint: the auto-rig keeps a record beside the original (`assets/<name>.rig.json`:
its fit, and the pose it carried the file out of), and with it the viewer carries the file's
own vertices again through the weights as they are now, so a pouch painted from the arm to the
hips moves back onto the hip in the bind pose rather than staying where the arm's carry left
it, a bone cleared everywhere puts its part back in the file's own pose, and *Save weights*
writes the re-carried positions with the weights. Weights are kept as Blender keeps them: each
bone's its own, 0 to 1, with no sum held to one while you paint - so *erase* takes only that
bone's weight off and nothing moves elsewhere, *add* puts only that bone's on, and *smooth*
blends with the neighbours. A vertex may end up carrying less than one in all (it moves by
what it has) or nothing (it stays as the file has it - the plain sign that no bone owns it
yet; the readout says *unweighted* or *no bone*). The sum is made one only when saving, a
vertex with nothing given its neighbours' heaviest bone. Nothing is ever handed to a parent
or a stray bone: erasing `L_kata` from a sleeve cannot put it on `trans`. *Clear bone* takes
the picked bone off the whole mesh the same way, *Smooth all* is one smoothing pass over everything (each an undo step), *Discard* throws
the unsaved strokes away, and *Redo auto-rig* runs the auto-rig again from the original,
unrigged file (`<name>.glb` beside `<name>-rigged.glb`) and starts over; radius and strength are the brush, more at its centre; *mirror* (off
unless ticked: few models are symmetric to the vertex, and a mirrored stroke lands where the
other side's mesh is not) paints the x-mirrored spot with the `L_`/`R_` counterpart. The brush is a ring drawn on the surface under
the cursor, in that surface's plane, yellow for add, red for erase, blue for smooth - and it
paints the surface it sits on: out from the touched triangle along the mesh's own edges to the
ring's radius, not a ball about the point, so a sleeve is painted without the chest beneath
it and the eye without the hair behind it. Every vertex keeps four bones summing to one
- the others give way as one is painted - and twin vertices at a seam paint as one. Play or
scrub while painting: the brush works on the mesh as posed, so a stretched sleeve is
painted where it stretches. The view's switches in the toolbox work with no brush on too:
*bones* draws the skeleton over the shaded model (the picked bone yellow), *wireframe* the
mesh's edges over whatever is shown (game models as well), and *heat map* (shown with a brush
on) can be turned off to paint on the textured model. *Undo* (Ctrl+Z) takes a stroke back; *Save weights* writes the
weights into the `.glb` in place (`/api/project/models/weights`) and remakes the game
model from it, so the game-format preview, the Steam version and the client's definition
follow the paint.

The game-format remake is still written on an OpenFF project: it is what the model viewer
previews when the file has no skin of its own and what a Steam target would get from the same file. On an OpenFF target the
model-size limit is waived - the OpenFF client's `DrawModel` buffers grow to fit (they
were the Steam port's fixed 20,480 / 32,768 / 256; a 41,280-triangle `j101` draws) - with a
note that a Steam target would refuse the file; `Undo remake` removes the definition and
the two overrides together. `crystal mdl-reskin ... --target=ours` is the same at the
command line (the definition is the editor's to write).

### Monsters of the mod's own: `defs/monsters/<id>.json`

A monster is a definition too. Under *OpenFF mod ▸ Monsters*, *New monster…* asks for a name
and the game's monster to start from; the inspector then has the base (its family is the
battle model), the *Look* - whose texture it wears, any monster of the same family, so a
recolour is one pick away - and every field of the record by the game's own name
(`mon.MonsterParameter`: `level`, `maxHp`, the five stats, `aggressivity`, `hitProbability`,
`phylacticPower`, `avoidanceNumber`, `weakType`, the two special actions, `dropProbability`,
`dropTable`, `gil`, `exp`…) with the base's value greyed in. Every change saves itself.

```json
{
  "id": "goblin-chief",
  "number": 1001,
  "base": 1,
  "name": "Goblin Chief",
  "look": 44,
  "fields": { "maxHp": 60, "level": 4, "exp": 30, "gil": 80, "strength": 9 }
}
```

`number` is the monster id the game knows it by, given by Crystal from 1001 up. In play the
client appends the record to `monster.chaindata` (chain 0, and the base's offset record -
where the cursor and the damage numbers go - under the new id in chain 4), the name to every
language's `eureka_battle.msd` from 2001 up, and answers the battle's request for the
monster's texture (`f<family>_<id>.ntxp.lz`, which a mod monster has none of) with the one
`look` names - all as the files are read (`Shared/Data/ModMonsters.cs`). A Steam project has
the composed files written into `files/` at Install and Export. Not yet: a model of the
monster's own (a family's model with a texture of the mod's is the next step), a bestiary
page, a drop table of its own.

### Formations: `defs/formations/<id>.json`

A formation is a monster party - up to four slots of a monster with a count, the game's
monsters or the mod's. *OpenFF mod ▸ Formations ▸ New formation…* (or *New formation with
it…* on a monster) makes one; the inspector sets the slots.

```json
{
  "id": "chief-and-goblins",
  "number": 1001,
  "name": "Chief and goblins",
  "slots": [ { "monster": 1001, "min": 1, "max": 1 }, { "monster": 1, "min": 1, "max": 2 } ]
}
```

The client appends it to `monster_party_table.bbd`; `number` is the party id. The battle
draws each slot's count between min and max, as it does for the game's own parties (six
fighters at most, three of the medium size).

To put it in the game: the **Encounter** component. On any object with a figure (an n021
villager, an o000 spot, nothing at all), it starts the game's battle with its *Formation*
when the hero walks into it (or presses A at it), on the *Battle Map* background given (0
for the default). Won once, the figure is gone for good across saves (`Once`); run from, it
stays; `CanEscape` off holds the party in. The picker offers the game's 242 parties and the
mod's formations. Inherit and override `OnWon` / `OnLost` for what follows (a reward, a
flag, a line) - they run on the object as the map comes back from the battle. From code,
`Game.Battle.Start(number)` fights a formation anywhere; in a CastScript the game's own
battle commands do.

Random fights too: the map's terrain card (click the terrain in the hierarchy) has
**Random encounters** - the five groups of four parties the map's `.pak` holds, each slot a
picker over the game's parties and the mod's formations, saved into the map's `.pak` as you
change them (a file of the project's: Export carries it, a Steam mod installs it). Which
ground fights which group is in the terrain's collision materials (attribute flags 20-24;
group 1 is the usual one), so a map that has random fights keeps having them, with your
formation among them - or only yours. The client accepts a mod formation's number where
the game checks a party id against its own table's size.

A skin of the monster's own: *A skin of its own…* on the monster copies the texture it
wears to `files/f<family>_<number>.ntxp.lz` - the name the battle asks for - and opens it
in Textures, where *Replace with a PNG…* paints it (every format, the monsters' 4x4 blocks
included). The file is the project's; Export carries it, and the game reads it before any
look-alike.

### Models of the mod's own: glTF (OpenFF targets)

A Steam target is held to the game's own formats - a model there is a package in the DS
format, so the game can read it. An OpenFF target is not: the OpenFF client reads glTF
itself. Put a Blender export (`.glb`, or `.gltf` with its `.bin` and pictures) in the
project's `assets/` folder - the model picker's *Import a model…* does it - and give an
OpenFF object `assets/hut.glb` as its Model. The map editor's 3D view draws it in place like
any of the game's models; in play the client draws it with the field's camera, in the
game's depth, lit from above as the DS models are shaded. Materials' base colour and
texture come through; the node tree is applied. One glTF unit is one world unit (two characters side
by side are about 8 apart), and the model is stood on its lowest point unless the `Mesh`
component's *On Ground* is off. Export copies `assets/` into the mod. From code:
`Game.Meshes.Spawn(path, position, yaw, scale)` gives a handle to move, hide and remove.

Animation: the file's clips (Blender's actions) play by name - the `Mesh` component's *Clip*
loops one from the start at *Speed*; from code `handle.Play("Walk")`, `handle.Stop()`,
`handle.Clips`. Node animation (a turning sign, a bobbing lantern) and skinned meshes
(joints and weights, the inverse bind matrices) both pose on the CPU each frame; STEP and
LINEAR keys as they are, CUBICSPLINE by its values. Without a clip the model stands at its
bind pose.

Collision: the `Mesh` component's *Solid* (from code `handle.Solid = true`) makes the model's
own triangles ground and walls to the characters, through the same collision the map's
polygons go through - a face pointing up is a floor to stand on (the hero steps onto a
2-unit ledge and drops off its edge), a face pointing sideways is a wall to bump into, a
ceiling is nothing. The bind pose's triangles, at the mesh's place; an animated solid keeps
its bind-pose shape. Off (the default), the model is walked through, like scenery. This is
the ground a new map for the OpenFF target stands on: one glTF for the look, the same glTF
as the floor.

### Maps of the mod's own (OpenFF targets)

*New map…* (Scenes or Maps, in an OpenFF project) makes a map with nothing of the game's
behind it: a name of yours, a kind (town or dungeon - the stage name's first letter, which
the client reads the map's type off), a ground - one of the project's glTF files, or a
flat slab the editor writes into `assets/` at the side you give - and a tune. The map gets
a stage name the game leaves free (`t90_00`, `t90_01` …, `d90_00` … for dungeons) and a
scene file `scenes/<name>.json` marked `"own": true` with a `"title"`; it opens in the map
editor like any map, listed among the game's as *The Old Quarry (t90_00)*. The client
loads such a stage as it loads any - finding no model, animation, collision or parameter
file behind the name - and draws the scene alone: the *Ground* object's Solid `Mesh` is the
floor, so the hero stands on it and drops off its edge; the field's camera follows as on
any map; `--map=t90_00 --pos=…` and *Play here* land where they say.

What a map needs, as components on its objects:

- **MapSettings** - on the map itself (the *map* row of the hierarchy; the New map dialog
  puts one there): the *Background* colour behind the scene - the sky, where the game's
  maps have black - and the follow camera as a parameter file would set it: *Camera
  Offset* (where it stands relative to the hero; the game's maps use about 0, 110, 110 -
  high and behind), *Look Offset* (0, 10, 0) and *Zoom Range*. The 3D view shows the sky
  as it is picked; the client puts the game's black back on leaving the map.
- **Exit** - a way out: the hero within *Radius* of the object is taken to *Map* (a picker
  of the game's maps and the mod's), arriving at *Arrive* facing *Facing* degrees, by the
  game's own map jump. Put one on a map of the game's to lead into yours, and one on yours
  to lead back; a two-way door is two Exits.
- **Music** - the game's tune by number (*Bgm*, the Audio library's BGMnn) at *Volume*,
  fading in over *Fade In* frames, started as the map is entered.
- **Sound** - an effect (the game's or the mod's own, by archive and number) as the hero
  comes within *Radius*, or as the map is entered with Radius 0; *Once* or every time.
- **Roam** - on an object with a `Mesh`: the model walks about its spot (*Radius*, *Speed*,
  *Pause* between walks), faces the way it goes, keeps to the ground (the map's or a Solid
  Mesh's), stops short of the hero, and plays the file's *Walk Clip* while moving and
  *Idle Clip* while standing when it has them. What `Wander` is for a game character,
  this is for a glTF creature of yours. A `Talk` on the same object works as on a
  villager: A within *Radius* (there is no game character to face, so the radius applies),
  the roamer halts and turns to the hero for the lines and walks on after.
- **Encounter**, **Chest**, **Talk**, **Trigger**, **Mesh** and the rest as on any map;
  characters of the game's models as scene objects with a model (an `Npc` behind them).

Not yet: a name on the menu's map screen, the map screen itself.

A creature of yours on such a map is a glTF from Blender with its actions: the Khronos
`Fox` sample (a skinned model with *Survey*, *Walk* and *Run* clips) roams with
`Mesh { Path: assets/fox.glb }` and `Roam { WalkClip: "Walk", IdleClip: "Survey" }` at
scale 0.08, textured and posed, as any export of yours will.

### Sounds of the mod's own (OpenFF targets)

The Audio library's *Import a sound…* (OpenFF projects) takes an Ogg Vorbis or WAV:

- **A tune**: the loop, an intro to play once before it if there is one, and where the
  loop begins in milliseconds, under a BGM number the game leaves free (59 and up; the
  first free one is filled in). It lands in the project's files as `sound/BGMnn_1.ogg`
  (or `.wav`), `sound/BGMnn_0…` for the intro and `sound/BGMnn.dat` for the loop point -
  the shape the game keeps its own in - and Export carries it. `Music { Bgm: nn }` on a
  map's object (the picker lists it with the game's), `playBGM nn` in a script or
  `Game.Audio.PlayBgm(nn)` in C# plays it, through the game's own music player, fades
  and all.
- **An effect**: one file under an archive number of its own (300 and up: `SE300_00`),
  as `sound/SE300_00_0.ogg`. A `Sound` component on a map's object plays it - as the hero
  comes within *Radius*, or as the map is entered with Radius 0, once or every time - and
  so do `Game.Audio.PlaySe(300, 0)` in C# and `playSE 300, 0` in a script.

A file under one of the game's names replaces that sound (or that part of a tune) instead.

### A font of the mod's own (OpenFF targets)

The Text library's *Import a font…* (OpenFF projects) takes a TrueType or OpenType face
into the project's files as `fonts/<file>`; the client loads a mod's faces before the
game's, so every line of text - windows, menus, names, the title's - is drawn from it,
the game's faces filling in the glyphs it lacks (Japanese, symbols). The face opens in the
editor as sample lines at the sizes the game draws; *Remove* takes it out again. Mind the
licence of a face you ship.

### New content from existing: Duplicate as…

Any file in the libraries - a model, a texture package, a picture, a sound, a menu, a
script, a table - has *Duplicate as…* in its inspector: a copy under a name of the mod's
own, as a file of the project's. A model brings its `.ntxp` along, so the copy can be
retextured without touching the original; the copy shows in the Models library and in the
model picker ("the project's own"), an OpenFF object wears it by name (`n900` from
`n021`, recoloured green, stands in Ur beside the untouched original), and a Steam mod
carries the file the same way (a `.hich` row would still need a character id). Pictures
come in directly: *Import a PNG…* in the Images library puts a new picture under a name
the menus can refer to; *Replace with a PNG…* on one of the game's swaps it.

A texture package from nothing: *New texture package…* in the Textures library builds a
`.ntxp.lz` of the mod's own from a PNG - the package's name, the texture's name inside it
(a model's material asks for its texture by that name: n021's for "n021", a monster
family's for its own), the format (pal256 for most things, a5i3 for shadows, 4x4 for the
battle monsters, rgb555 for true colour), whether entry 0 is see-through, and the size
(powers of two, 8 to 1024; the picture is scaled to it). The file is written and read back
through Crystal's own reader before it is kept, and the game loads it like any of its own.

### The heroes: `defs/characters/<id>.json`

The first slice of character definitions: what a hero slot is as a game begins. Under
*OpenFF mod ▸ Characters*, *New character…* takes one of the four slots (0 Luneth, 1 Arc,
2 Refia, 3 Ingus) and a name; the inspector sets the starting job (any of the 23, by the
game's `JOB_TYPE` order) and level.

```json
{ "id": "alto", "slot": 0, "name": "Alto", "job": "knight", "level": 7, "fixedJob": true, "look": 2 }
```

`fixedJob` keeps the hero in its job - the job menu beeps at a change, as it does for a job
not yet won - which is a class of its own in place of the job system, the first step toward
FF4-style characters. `look` is which hero's model set the character wears (0 Luneth, 1 Arc,
2 Refia, 3 Ingus: `j<look+1><job+1>` on the field, in battle, in the menus and shops); every
job has a figure in every set, so nothing is missing anywhere.

The client applies it where the game sets the party up - a `--map` start and the title's
New Game (`ModCharactersLayer.ApplyToNewParty`): the name through `setName` (the name entry
still lets the player change it), the job through `changeJob`, the level the game's own way
(`setExp` then `levelUp`, one level at a time along the job's growth tables), so the field
model follows (a Knight in slot 0 is `j109`). A save loaded afterwards carries its own
heroes. One definition per slot; a `--nomods` run is the game's. Not yet: a model of the
character's own (an NPC model has no job figures and no battle motions to stand in with),
and heroes beyond the four.

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

Each is a finished mod folder; *File ▸ Sample projects…* in Crystal copies one into a project of
your own (the code under `code/` with Crystal's csproj), to read, change and Run in OpenFF. The
release zip carries them in `Samples\`.

- `Samples/HelloMod` - a service, a behaviour, a save chunk, a villager who talks through a
  coroutine, and a scene file; `install.cmd` builds it into the mods folder.
- `Samples/Survivors` - a survivors-style run on the field: waves of goblins from the map's
  spawn points, auto-aimed spells with the game's own effects and formulas, run levels with
  cards, chests with equipment, a trader who opens the game's shop. Everything it uses is in
  the API table above; it is the best single answer to "what can a mod do".
- `Samples/Showcase` - a mod with no code at all: `mod.json`, two weapon definitions with
  glTF looks (a *Rune Blade* whose ring of light turns and drifts along the blade through a
  clip, an *Oak Shield*), a map of its own (the *Crystal Shrine*, `t91_00` - ground, kerb,
  pillars, an arc of wall, trees, a shrine and its crystal, every one a glTF with `Solid`
  collision, its own camera, sky colour and music) reached by a new exit from Ur
  (`scenes/t01_01.json`), chests holding the two weapons, a crystal that talks - the first
  time through a `Cutscene` on a timeline: two camera shots, a flash, two lines - and the
  Khronos fox wandering the shrine. Copy the folder into `mods/` to play it. Every model and
  texture but the fox is made by `crystal sample-assets` (`Crystal.Editor/Editor/SampleAssets.cs`,
  on `GltfBuilder`) - the place to see how a glTF is put together for the hand's frame, a
  ground, or a clip.

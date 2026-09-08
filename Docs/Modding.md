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
to change it, clear it to fall back to the base's. Every change saves itself. The file:

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
texture come through; the node tree is applied; skins and animations are not read yet (the
model stands at its bind pose) and a mesh has no collision (walked through - a Solid box
is on the list). One glTF unit is one world unit (two characters side by side are about 8
apart), and the model is stood on its lowest point unless the `Mesh` component's *On
Ground* is off. Export copies `assets/` into the mod. From code: `Game.Meshes.Spawn(path,
position, yaw, scale)` gives a handle to move, hide and remove.

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
{ "id": "karl", "slot": 0, "name": "Karl", "job": "knight", "level": 7, "fixedJob": true, "look": 2 }
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

- `Samples/HelloMod` - a service, a behaviour, a save chunk, a villager who talks through a
  coroutine, and a scene file; `install.cmd` builds it into the mods folder.
- `Samples/Survivors` - a survivors-style run on the field: waves of goblins from the map's
  spawn points, auto-aimed spells with the game's own effects and formulas, run levels with
  cards, chests with equipment, a trader who opens the game's shop. Everything it uses is in
  the API table above; it is the best single answer to "what can a mod do".

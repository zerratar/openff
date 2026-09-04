# OpenFF: the engine we are building towards

Karl's goal (2026-09-04), and the shape of the work to reach it. `Client-Plan.md` is the
day-to-day plan; this is the destination it serves.

## The goal

One game engine of our own that plays FF3, plays FF4, and plays what modders make from
both - "mixed" meaning logic that co-exists, not just FF4 assets on FF3 gameplay. A party
whose members progress by FF3's job system or FF4's fixed classes, chosen per character or
per mod. Maps from either game reachable from either game. One script interpreter that
runs FF3's 298-command dialect and FF4's 500-command dialect, with events, exits and
encounters fed either from the games' data (FF3's tables) or from scripts (FF4's), so the
FF3 path is not limited to what FF3 shipped. Beyond that, a new scene format in the manner
of Unity - game objects with transforms, C# behaviours with a lifecycle, triggers, cameras,
lighting, dialogue and UI systems - with the full power of .NET, so a multiplayer mod, a
roguelike, or anything else is a mod and not a fork.

Mods built in Crystal for the Steam games keep their limits (those engines are what they
are); mods built for OpenFF get all of this. OpenFF has a `mods/` folder beside the
executable, one mod per subfolder, a load order (`mods/loadorder.json`) and an in-game
screen to enable, disable and reorder.

## Why it is feasible

The games differ in *rules*, not in *kinds of things*. Both have maps with collision,
actors with transforms and motion sets, cameras, exits, dialogue, flags, and tables. Of
FF4's 500 commands, 217 already run FF3's handlers because they mean the same action.
The unification is therefore an explicit engine API of actions ("verbs") with three
front-ends - FF3's dialect, FF4's dialect, C# - not FF3's code stretched over FF4.

## The layers

| Layer | What it is | State |
| --- | --- | --- |
| Content | sources (archives, loose, mass files, memory), overrides, LZ, the chain | done, in `Shared/Content` |
| Formats | one reader per format, both games' variants | done, in `Shared/` and the editor; the client still reads most through the decompiled loaders |
| Unified assets | one object per kind - model, motion, map, cell bank, text, table, exit - built from either game's version, **each carrying its own provenance** (text encoding, motion numbering, exit source) | next |
| Engine core | scene, actors, transforms, collision, cameras, motion playback, rendering, audio; **game state as plain data, separate from rendering and input** so it can be saved, simulated and one day replicated | to design; the decompiled FF3 runtime is the reference and the regression oracle |
| Engine API | the verbs: boot/move/turn a character, message windows, camera, flags, exits, effects, sound... | to define; the FF3 handlers and the FF4 command table are its specification |
| Dialects | FF3's 298 and FF4's 500 commands lowered onto the API; C# behaviours calling it directly | dialects exist as tables in `Shared/Script`; lowering is the work |
| Rules | progression (FF3 jobs, FF4 classes) as components on party members; battle and menus data-driven over them | the largest single piece; FF3's battle and menus are hand-wired to jobs today |
| Scene format | the serialisation of engine objects and behaviours; Crystal edits it | after the API, so it does not freeze around the legacy engine |
| Mods | `mods/<name>/mod.json`, `loadorder.json`, enable/disable/reorder in game; C# mods as compiled assemblies loaded in isolation | early and cheap on top of the chain. Folder, `mod.json`, `loadorder.json`, conflicts in the log and Crystal's Export to OpenFF are done (2026-09-04); the in-game list and C# assemblies remain |

## Decisions and constraints

- **Provenance is per asset, never a global switch.** `GameProfile.IsFf4` is scaffolding;
  it must become a property of each loaded asset, so an FF3 map can neighbour an FF4 map.
- **Nothing of one game is needed to play the other.** Synthesise, never ship (see
  `MovementDefaults`).
- **State is data.** No engine module keeps game state inside rendering or input code.
- **The FF3 path is the oracle.** Every layer is introduced behind it, verified against it
  (`Docs/Testing.md`), before the old path is removed.
- **C# mods are code, and that is the player's and the modder's business** (Karl, 2026-09-04):
  as with Skyrim's and Fallout's native plugins, OpenFF loads compiled mod assemblies
  directly, with no signing or sandbox; whether a modder publishes source is up to them.
  Compiled assemblies (Crystal driving `dotnet build`) come first; runtime scripting and
  hot reload later.
- **Rename before publishing.** The decompilation's Android-shaped names go when the frame
  is reorganised for the engine core, not before there is something to rename into.

## Order of work

1. Finish FF4 on the current scaffold only where it teaches what the API must cover.
2. Per-asset provenance and the unified asset objects.
3. The engine API; both dialects lowered onto it; the frame reorganised and renamed.
4. C# behaviours against the API; the mods folder and load order.
5. The scene format, and Crystal editing it.
6. Progression components; battle and menus data-driven.

## What "OpenFF" means as a target (Karl, 2026-09-04)

An OpenFF mod is not a mod *of FF3* or *of FF4*. Under OpenFF the game that was booted
is an asset source and nothing more: the engine is ours, the content of either game (or
both, or neither) feeds it, and mixed content is the point rather than a limitation. So an
OpenFF mod applies whatever is in front, and the mods folder has no per-game filter.

FF4 under OpenFF is not a reproduction of the FF4 client. It is our take on FF4's gameplay
on our engine, which frees the menus and the rest of the presentation to be interpreted
rather than copied. FF3 is the case where we hold the complete code and assets, so it can
become a faithful rendition on the new object model: the same game, running on C#
behaviours and game objects instead of the decompiled frame.

Crystal, when a project targets OpenFF, gets the tools the Steam targets cannot have:
scene editing, our own formats, and the converters that make them from the Steam data.
Migration is a tool in the editor, not a precondition: legacy data stays loadable as it
is (an FF3 map is a scene with one legacy map object in it until someone converts it),
and a conversion is re-runnable from the game's files, so nothing is lost by converting
early or late.

## The object model and scripting

Unity is the reference because it has proven that one object model carries any kind of
game. What follows is that model, sized for us, with the parts a modding engine needs
that Unity leaves to the developer: persistent services, save extension, load-order
aware composition.

### Objects

- **Scene**: a tree of game objects, serialised as data (our format, edited in Crystal).
  A scene is loaded and unloaded as a unit; several can be loaded at once (a map and the
  UI scene over it, or two neighbouring maps for streaming).
- **GameObject**: name, stable id, tags, layer, active flag, transform (position,
  rotation, scale, parent) and a list of components. Nothing else; all behaviour lives in
  components.
- **Component**: a typed piece of data on a game object. Engine components (renderer,
  animator, collider, trigger volume, camera, light, audio source, text, sprite, legacy
  map, legacy cast, exit) and mod components (behaviours). Fields marked for
  serialisation appear in Crystal's inspector and in the scene file.
- **Prefab**: a game object subtree stored as an asset, instantiated by reference with
  per-instance overrides. Mods add prefabs; a mod can also override a prefab another mod
  or the base provides, in load order.
- **Assets**: everything that is not a scene object - models, motions, textures, sound,
  text, tables, prefabs, data assets. Addressed by stable id and by path; each carries its
  provenance (which game, which mod, which conversion produced it). Unified asset types
  (the layer above the format readers) are what components reference.
- **Data assets** (Unity's ScriptableObject): typed, serialised objects that are not in a
  scene - a job definition, an item table, a dialogue tree, a mod's settings. Behaviours
  reference them; Crystal edits them; mods ship them.

### Scripts, by lifetime

The question of "where does a script live" is answered by what its state must outlive.

1. **Behaviour** (`Behaviour : Component`) - on a game object, lives as long as the object.
   Unity's callbacks: `Awake`, `OnEnable`, `Start`, `Update`, `LateUpdate`, `FixedUpdate`,
   `OnDisable`, `OnDestroy`; plus the engine's: `OnTriggerEnter/Exit`, `OnInteract` (the
   player talks to or touches this), `OnSceneLoaded`, `OnDrawDebug` (into the F1 overlay).
   Coroutines (`IEnumerator` with yields for frames, seconds, a message to close, a walk
   to finish) are how cutscenes and any multi-frame logic read naturally.
2. **Scene script** - a behaviour on the scene's root object. Nothing special in the
   engine; the convention gives per-map logic (what FF3 scripts do today) one obvious
   home, and the legacy dialect interpreter is itself a behaviour there, running the
   map's original script.
3. **Service** (`GameService`) - one instance for the whole run, created at start (or on
   first request) and never destroyed by a scene change. Lifecycle: `OnGameStart`,
   `OnSceneLoading/Loaded/Unloading`, `OnSave/OnLoad`, `OnQuit`, and `Update` if it asks.
   Reached through `Game.Services.Get<T>()`. This is the "static shared script": a
   multiplayer host or client, a mod's global state, a quest log, a difficulty rule set,
   an achievements bridge. The engine's own systems (party, inventory, flags, time,
   input, audio, scene loading, save) are services of the same shape, so a mod can
   replace one by registering its own under the same interface, in load order.
4. **Rules as components on the party** - progression (FF3 jobs, FF4 classes) as
   components on party members, so a character can be on either system, and a mod can add
   a third. Battle and menus read the components; that is the data-driven rebuild.
5. **Event bus** - typed events (`Events.Publish(new MapEntered(...))`,
   `Events.Subscribe<MapEntered>(...)`) for mods that must react without owning the object:
   map entered, battle started, item gained, flag set, dialogue line shown, save about to
   be written. The legacy dialects raise the same events as they run, so a C# mod can
   react to an FF3 script without editing it.

### Saving, with mods

- A save is a set of **chunks**, each keyed by owner (engine or a mod id) and a version.
  Any service or behaviour implementing `ISaveable` writes its own chunk; persistent
  object state (a chest opened, an NPC moved) is saved by stable object id.
- The save records the **mod set** (ids and versions) it was written under. A mod's chunk
  is left opaque and kept when the mod is absent, so disabling a mod does not destroy its
  data; a mod may mark itself `required` in `mod.json`, and such a save then says which mod
  it needs instead of loading half a game.
- Versions are for **migration**: a chunk's `Load(version, reader)` upgrades older data,
  the same way the engine will migrate its own chunks between releases.
- Legacy saves (the phone's and Steam's `save.bin`) import into the engine's core chunks,
  so nothing a player has is lost when the object model arrives.

### State as data, for multiplayer later

Multiplayer is a mod, but only if the engine never hides state where a network layer
cannot reach it. Hence the rules already agreed: state in components' fields and in
services, never in rendering or input code; no statics holding game state; a fixed update
step for simulation separate from rendering; input arrives as commands, so a remote
player's commands are ordinary input. A `[Replicated]` marker on fields is the later
addition that lets a networking service ship changes; nothing in the model has to change
for it.

### Loading C# mods

- `mod.json` names the mod's assemblies; the engine loads each mod in its own
  `AssemblyLoadContext` (isolation for unloading and versioning, not a sandbox), in load
  order, after all mods' assets are known. A mod *may* have dependencies (most will not):
  an optional `dependencies` list of mod ids, with a minimum version each, in `mod.json`.
  The load order is checked against them - a dependency must be enabled and come first -
  and a mod whose dependency is missing is disabled with a message saying which, not a
  crash (Karl, 2026-09-04).
- The engine API is a separate, versioned assembly; a mod says `minEngine`. Reflection
  finds the mod's behaviours (for the inspector and the scene loader) and its services.
- Crystal builds a mod's C# (`dotnet build` driven from the editor, errors shown inline)
  and exports it with the assets. **Hot reload is a goal, not an afterthought** (Karl,
  2026-09-04): the client watches a mod's assemblies, unloads the mod's load context and
  loads the new build, re-creating its behaviours and services from their serialised
  state. A change too large for the running state to survive (a removed field, a changed
  service contract) is reported and the answer is a restart - expected, not a failure of
  the feature. Behaviours therefore keep their state in serialised fields, which is also
  what saving needs.

### Crystal for the OpenFF target

Scene hierarchy and inspector; adding components and behaviours from the loaded mods;
prefab editing; data asset editing from their serialised fields; an asset browser showing
provenance; converters (Steam map to scene, FF3 tables to data assets, casts to prefabs)
that run per asset, re-runnably; and Play, which starts the client with the project as a
mod - which `--project` already does.

### Order, refined

The object model can arrive before the legacy engine is gone: a scene may contain a
**legacy map object** (the decompiled field code as one component) beside new objects, so
FF3 keeps running as the oracle while behaviours, services, events and the save chunks
are introduced around it. So step 4 above grows to: the mods folder (done); the engine
core - objects, components, services, events, save chunks - hosting the legacy game as a
component (done 2026-09-04: `OpenFF.Engine`, hosted by `Compat/EngineHost.cs`; the legacy
game is the World's "legacy" scene); C# mod loading with hot reload (done, `Samples/HelloMod`);
the in-game mod list (done); then behaviours gain the engine API verbs as step 3 delivers
them (first slice done 2026-09-04: dialogue, hero, NPC spawn/move/talk, flags, party, audio,
fades, warp - `OpenFF.Engine/Api.cs`; second slice the same day: coroutines, the events the
game's scripts raise, a yes/no question, camera, effects, party members, richer characters);
then the scene format and Crystal's editing of it
(step 5), with the first converters. Crystal generating a mod's csproj, building it and
handing it to Visual Studio or any editor is done (2026-09-04, `Editor/ModCode.cs`).

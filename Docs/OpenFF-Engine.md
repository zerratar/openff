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

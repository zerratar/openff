# The client: OpenFF

Where the game goes now that the editor is in good shape. The aim Karl set: one client
that runs FF3, FF4 (3D) and mods that mix the two, from the content people already own,
rendering natively at desktop quality - TrueType text like the Steam release, not the
phone's 12- and 16-pixel atlases - and moddable all the way down, so that targeting
"our" client can do things the shipped engines cannot.

This continues `Modernisation-Plan.md` (stages 1-2 are done: the phone boot chain is
gone, rendering is native). It is ordered so the game keeps running at every step and
each step is useful on its own.

## Stage A - one content layer, shared with the editor (done)

The editor already reads every shape the content comes in: our `data*.bin` archives,
Steam FF3's loose `files/`, FF4's `SSAM` mass files, all behind `IContentSource`, with
a project's edits layered on top. The client reads only the archives, with one loose
override directory. Moving the sources into `Shared/Content` and reading through a
`ContentChain` (overrides, then project files, then the shipped content whatever its
shape) gives the client:

- `--content=<Steam FF3 install>`: boot from the game people bought. Nothing to extract.
- the editor's projects as the client's mods, with no copy step - the same folder.
- FF4's files readable by the same code, which is the first requirement of running FF4.

The chain has no automatic fallback: what is in `--content` is all the client reads.
Fonts and sound come from the install too (stages B and C), so a Steam install is enough
on its own - see "Steam-only" below.

## Steam-only - nothing of ours needed at runtime (done)

The requirement: people can only play with what they bought, because none of Square's
assets can be distributed with the client. So a boot from `--content=<Steam FF3>` must
need nothing from our `Content/`. Three things stood in the way, all in the 2D layer:

- **44 files Steam does not ship** - the about and repair title sprites, the achievement
  `link_icon_00..37` and the menu's `icon_16dot_2` - all phone-only features. The 2D
  loaders now treat a missing file as an empty bank (`NNS_G2dGetUnpackedBank` and the
  cell/anim/sequence lookups return nothing instead of dereferencing), so those screens
  simply draw nothing there.
- **Steam's cell banks are laid out for Steam's UI.** Its `.NCER` files place and size
  cells for a widescreen desktop over sheets authored at 1x, 1.2x, 1.33x and 2.22x the
  phone's. But both builds keep the same cells in the same order (Steam only appends to
  `m014_button`), and each Steam OAM's sheet rectangle covers the same picture as the
  phone's. So `Tools/gen_steam_cells.py` writes `FF3.Game/Data/ff3-steam-cells.json` -
  the phone's placement (x, y, w, h, flags) for the 29 banks that differ - and
  `Compat/SteamCells.cs` applies it when a bank is loaded from a Steam install, keeping
  the file's own sheet rectangle as the source. `NNSG2dCellOAMAttrData` grew a source
  size (`srcW/srcH`) for this, and `NNS_G2dDrawCell`/the BG path pass it to `drawImage`.
  The table is a few hundred positions of layout metadata we generated, not art.
  `--steam-cells-off` draws Steam's banks as they are.
- **The `.glp` glyph tables** were read at start-up from our XNBs; they are now optional
  (TrueType does not use them) and the atlas text path returns when a size has none.

Verified: from the Steam install alone the client shows the logos, the title (logo,
menu, cursor), naming, the opening, and plays the first battle in the Altar Cave with its
windows, HP bar and targeting cursor. A boot from our archives is unchanged.

## Stage B - TrueType text (done)

The Steam build ships `SE-EYEGLCOB.TTF` (the FF face), `TBUDRGoStd-Bold.otf`, `arial`,
`unifont`, `simsun`, `mona`; the phone build baked two sizes into 256-page atlases and
indexed them through `.glp` tables. The game's text path (`GlobalScope.Graphics`,
`drawString`/`measureString` over `SpriteFont` pages) gets a second implementation over
a TrueType rasteriser (FontStashSharp is the MonoGame-native choice), rendering at the
window's real resolution with the same metrics the atlases had, so layout does not
move. `--text=atlas` keeps the old path for A/B. Fonts are found in the Steam install
or beside our content; a mod can ship its own.

Done in `Compat/TrueTypeText.cs`: every string the game draws goes through
`Graphics.DrawString`/`StringWidth`, and those ask TrueTypeText first. The face is
rasterised at `size x 1.15 x window scale` (the atlas glyphs ran a little large) and
drawn back down, so text is sharp at 1600x960 and lays out where it did. Faces: `--font`,
then the install the content came from (Arial, Arial Unicode, TBUDRGothic, unifont - one
FontSystem, per-glyph fallback), then `Content/Fonts`, then Windows' Arial. The title and
its menu are unaffected - they are baked art, not text.

## Stage C - native sound (done for Ogg)

Steam FF3 ships `sound/BGMnn_0.ogg` + `_1.ogg` (intro and loop) with `.dat` loop points -
the same names our XNBs use. A decoder (NVorbis) into `SoundEffect`/`DynamicSoundEffectInstance`
lets the client play Steam's audio directly, and FF4's `.akb` (an Ogg with a header) with
the same code. `SoundManager` already keys everything by name; only the loader changes.

Done in `Compat/OggSound.cs`: `MediaPlayer.setDataSource` asks the chain for
`sound/<name>.ogg` (or FF4's `files/SOUND/BGM|SE|VOICE/<name>.akb`, skipping to the
`OggS` page) and decodes it with NVorbis into a `SoundEffect`, cached by name; the XNB is
loaded only when the chain has no Ogg. From the Steam install the title theme, the
opening's intro-and-loop pair and the effects all come from Steam's own files.

## Stage D - FF4 (in progress: Baron castle renders)

FF4 is the same engine with a 500-entry script table, its own tables, `setInsideMapJump`
exits and a few format extensions the editor already handles. Running it in the client
means: the script interpreter takes its table from `ScriptOpTable` (shared with the
editor - the 255 FF4-only handlers are the actual porting work, in order of how often the
scripts use them); the table readers take FF4's layouts; menus read `<layout>/<unit>`.
The measure of progress is maps that play through: Baron castle first (`d01_00`), the
opening (`t01_00`) second.

The install has everything the game needs - FF4 boots and plays on Steam - but under its
own names and in its own containers. What the FF3 logic asks for by FF3's name and does
not get is logged as `missing: <name>` (`Compat/MissingFiles.cs`), and each of those is a
mapping to write, not a file to ship.

**Done (2026-09-03):** `FF3.exe --content="<FF4 install>"` lands Cecil (`p00_00`) in
Baron castle's grounds (`d01_00`) from the Steam install alone: the map with its textures,
water and bridge, Cecil with his shadow, the castle's BGM from `BGM12.akb`, and the map's
own script running through the shared command table. The pieces:

- `Compat/GameProfile.cs` - what the FF3 logic has to know about the other game: no jobs
  or growth tables (`Ff3Party`), UTF-16 text (`MsdIsUtf16`), no FF3 map-parameter chains
  (`Ff3MapParameters`), the leader's model (`LeaderModel`, Cecil unless `--leader=`), the
  field motion sets (`FieldMotion`: `p00_00` walks with `f00.ncap`, NPC bodies with
  `f_man001` and friends, objects with their own), the start map (`--map`, Baron by
  default for FF4).
- `Compat/JumpPart.cs` - the game part in the debug-menu slot that the world code always
  expected: names the stage and the arrival (`--pos=x,y,z --rot=deg`), then hands over
  to the world part. `--map=t01_01` does the same for FF3 - any map, no title.
- `Compat/ScriptCommands.cs` - the FF4 command table: 200 of the 500 commands are FF3's
  command at FF3's number with FF3's operands and run FF3's handler; the rest read their
  operands per the shared table and are skipped, each logged once (`script: FF4 command
  333 setInsideMapJump not implemented`). That log, per map, is the porting list.
- `Compat/GxCommands.cs` - FF4's display lists carry matrix and material commands between
  vertex runs; the phone decoder terminated on them (into an infinite loop). Unknown
  commands are stepped past by their parameter count; `0x2C` (FF4's 16.16 texcoord) is
  decoded. `OS_Terminate` now throws with a stack instead of spinning.
- `Compat/Ff4Exits.cs` - FF4 declares a map's exits in its script (`setInsideMapJump` /
  `setOutsideMapJump`: trigger, destination, arrival, facing, the door's box). Those are
  real commands here (`Ff4Commands`): an exit exists from the moment the script executes
  its declaration - so one inside a branch exists only when that branch runs, one
  redeclared by trigger name replaces the old, and a modder's script behaves as written.
  The leader is checked against the boxes each frame, and walking into one requests the
  same absolute map jump FF3's scripts do - fade, unload, load and arrival are the
  engine's own. A whole-script decode is used only to pick where `--map` lands.
- `Compat/Ff4Text.cs` - FF4's `.msd` is UTF-16LE where the message code walks UTF-8 (FF3
  rewrote its SJIS files to UTF-8 at load); the same rewrite for UTF-16 gives whole lines
  instead of first letters. `Compat/Ff4Assets.cs` answers FF3's 2D asset names with FF4's
  files (`m000_window` is `MENU_Common`'s `window_frame_00`) and rearranges the bank into
  the cell order FF3's window code indexes, re-centring FF4's edge pieces and standing in
  for the wallpaper FF4 draws in code with one white texel tinted navy. `Compat/Ff4Commands.cs`
  holds FF4-only commands with an implementation of their own: `openCharacterNameWindow`
  and `closeCharacterNameWindow` (the speaker's name is a text id in the map's `.msd`).
  `--say=<message id>[,<name id>]` opens the field window with a line for checking.
- The overworld: `--map=f00` enters FF4's world map. A field stage is a chip (`f00_67`),
  which `JumpPart` works out from the start position and the stage profile the way the
  stage manager's `getSpot` does (half a chip and the centre chip's index, wrapped);
  the chips stream in over the first seconds. The field's chip texture sheet and its
  casts, script and text answer to FF3's per-chip names; map objects numbered past
  FF3's 78 types take the first type's radii. The FF4 start map is Baron town
  (`t01_00`), whose world exit lands beside the castle; motions are renumbered so idle,
  walk and run play as such (`GameProfile.FieldMotionId`).
  Measured (2026-09-04): FF4 places the field's chips with z negated relative to FF3's
  layout - 31 of the 33 scripted world-map arrivals land on ground with the mirror and
  none on walls or sea, against 19 in the sea without it, and Crystal's chip-grid view
  (E-15) shows the exits on coasts and plains with "mirror z" on and in the sea with it
  off. `Compat/FieldMirror.cs` applies it in the client (chip position, chip scale,
  the stage world matrix collision goes through, spot lookups, world edges, cull face
  for a mirroring matrix); `stg` also promotes FF4's f00 (stage type 0) to FIELD01 so
  chip streaming and collision run at all. `--fieldmirror=off|force`.
  Resolved (2026-09-04): the "flattened relief" was the camera, not the mirror. FF4's
  mountains and forests are quads baked with a 45-degree tilt towards FF4's overworld
  camera, which stands on the far side of the party (looking towards +z); from FF3's side
  they are seen edge-on. `setupCamera` puts the camera on that side for FF4 field stages,
  and the chips are mirrored in their data (`FieldMirror.MirrorModel`: vertices and node
  offsets) rather than by a scale of -1, so every matrix stays a rotation. Verified at
  Baron town's exit and the castle plain (Testing E-17). Crystal looks at a mirrored field
  from the same side. The ground's hard-edged patchwork in both renderers was a third
  thing: the coast and grass-blend tiles use the DS's flip wrap (mirrored repeat), which
  neither renderer honoured (E-16).
- `Compat/MovementDefaults.cs` - the two movement tuning tables the FF3 logic reads at
  start-up (`player_world_move_parameter.pak`, `npc_world_move_parameter.pak`), which
  FF4 compiled into its executable, are OpenFF's own tuning synthesised in code and served
  from memory as the last content source (`Shared/Content/MemoryContentSource.cs`). A
  player who owns only FF4 needs nothing else; a mod shipping either file replaces them.
- Guards, all game-agnostic: empty tables clamp instead of throwing (map parameters,
  secret ways, smith list, jump table), missing sounds do not dereference, a loose LZ file
  answers to its plain name and a container entry to its `.lz` name, matrix stacks start
  as identity.
- `--probe` logs a per-frame heartbeat (vertices, bounds, camera, world state) and
  `--noscript` / `--noanim` isolate a map from its scripts and animations - the tools that
  found the black screen (the camera treats an all-zero position as unset).

Commands, as of the latest count: 214 of FF4's 500 run FF3's handler - the same
command at the same number (200), the same command renamed (`startMessage` is FF3's
`startMessage2`, `flagON` is `flagOn`) or the same command with extra trailing operands
FF3's handler never read (camera moves, BGM, inn, select wait: run it, step past the rest).

Talking to an NPC needs contact: FF3 starts a talk when the tapped character is also the
one the leader is pressing against (`ColType & 2`), and Baron's Tellah stands behind an
invisible barrier object at the game's start, so the tap registers (`--probe` logs
`touch: ... HIT`) but no talk begins - not an FF4 gap; test it on an NPC in the open.

FF4's event maps (`e01_00`, the Red Wings over Baron) have no parameter file at all; the
map-parameter accessors answer with a blank record there. They render, but their
cutscenes run on FF4's own cutscene engine - the `ce_*` family (`ce_StartEvent`,
`ce_SetupCharacter`, `ce_PlayCameraMotion` over `.dsc` camera files, `ce_SetupExpression`,
`ce_ShadowSetting`...), some thirty FF4-only commands - which is a subsystem to write,
not a mapping.

**Next, in order:** implement the FF4-only commands Baron and the opening use
(`setInsideMapJump` as an exit, `_3DS*` sprites, the name window, `startMessage`,
`bindMotion`'s FF4 semantics); FF4's `.msd` text into the message window; the 26
same-name commands with extra operands (`playBGM`, `moveCamera_AbsoluteCoordination`...);
NPC body types; FF4 2D (menus and windows are `.xbn` layouts, not FF3's hard-coded
screens); FF4's map parameter chains (encounters, landforms).

## Stage E - what "our client" can do that the engines cannot

Once both games run from the same code: higher-resolution textures by name (a mod drops
a 4x PNG next to the model), TrueType at any size, widescreen UI layouts, more casts per
map, longer scripts, new opcodes exposed to the script language, and a mod manifest the
editor's Export already writes. Each is small once A-D exist; none is possible before.

## Starting the client (2026-09-04, from Karl's note)

`FF3.exe` with no arguments starts FF3 from its Steam install: the client finds the
installs the way the editor does and remembers them, with the last choice, in
`%LocalAppData%\OpenFF\launch.json`. `--game=ff4` switches game, `--source=content`
uses our extracted Content directory instead of Steam; both are remembered, so the next
plain start repeats them. `--content=<dir>` still wins for a one-off. The log's `launch:`
line says what was chosen and why. (`Compat/Launch.cs`; `SteamInstalls` moved to
`Shared/Content` so both programs find the games the same way.)

## The engine core (2026-09-04)

`OpenFF.Engine` is its own project and assembly - the API mods reference, free of
MonoGame and of the decompiled game - and the client hosts it (`Compat/EngineHost.cs`):
created after the content opens, ticked once per game tick after the legacy frame,
told what the legacy game did. What exists: `Game` (Services, Events, World, Saves,
Time, Log), `GameService` with its lifecycle and hand-over for hot reload, the typed
`EventBus` (GameStarted, PartChanged, MapEntered/MapLeaving with a SceneInfo, SaveWritten/
SaveRead, ModReloaded), the object model (`Scene`, `GameObject`, `Component`,
`Behaviour` with Awake/Start/Update/LateUpdate/OnDestroy, `Transform`), `ISaveable` and
`SaveChunks` (one chunk per owner per slot, the mod set recorded, unknown chunks kept,
`%LocalAppData%\OpenFF\saves\mods.json` for now), and `ModLoader`/`ModWatcher`: a
collectible `AssemblyLoadContext` per mod loading from bytes (the file stays free for
the next build), the engine assembly shared, services found by reflection, a rebuilt
assembly reloaded within a second with each service's state handed over. The legacy
game is the World's one scene, "legacy", whose SceneInfo the host keeps current from
the stage name and part. `mod.json` gained `id`, `assemblies` and optional
`dependencies` (checked against the load order). `Samples/HelloMod` is the template.
Testing.md C-17..C-20.

Next on this path: the in-game mod list; the engine API verbs (message windows,
movement, camera, flags) as services the legacy handlers implement, so behaviours can
act rather than observe; then the scene format.

## The mods folder (2026-09-04)

`mods/` beside `FF3.exe`, one mod per subfolder: `mod.json` (name, version, author,
description, target), `files/` mirroring the game's names, a README. The client
reads the folder at start (`Shared/Content/Mods.cs`, `GameArchive.ModsFolderOverrides`),
puts the enabled mods that target `openff` in front of the shipped content (an OpenFF mod
is not tied to a game: the booted game is an asset source under OpenFF, mixed content is
the point) in the order `mods/loadorder.json` gives - first wins on a file two carry, and the
log's `mods:` lines say what applied and which conflicts fell which way - and writes the
order back so a folder dropped in by hand is enabled at the end. The command line's
`--project` and `--mod` still come first. Crystal's Project ▸ Export to OpenFF writes a
project's `ours` files there as a mod; it finds the client through
`%LocalAppData%\OpenFF\launch.json`, where the client records its location every run.

Mods for the Steam builds are a different thing: those clients know no mods folder, so a
Steam mod is installed by replacing files (Crystal's install, with its backup) and load
order does not apply. Still to come here: the in-game mod list (enable, disable, reorder,
conflicts shown) and C# mods as assemblies in the mod folder.

## Debug overlay (2026-09-04, from Karl's note)

F1 draws a diagnostic layer over the finished frame (`Compat/DebugOverlay.cs`, a
DrawableGameComponent placed after the game and before the screenshot component, so
`--screenshot-every` pictures include it). It reads state and draws on top; nothing in
the game changes. Sub-toggles while it is up: F2 menu frames as boxes with a cross where
the hand cursor is put, F3 their ids, F4 the 2D sprites as drawn, F5 the world text
(part, stage, type, mirror, hero position and spot, focused frame), F6 the stats (fps,
frame time, draw calls and vertices from `NativeRenderer`, memory, viewport).
`--debug=all` or `--debug=boxes,labels` starts with it on, which is how a headless run is
inspected: `FF3.exe --debug=all --screenshot-every=3 --screenshot-dir=<dir>` and read the
pictures. Add to it freely: it is the place for anything that helps a diagnosis (camera,
chip streaming, script state) rather than more log lines.

## Steam's menu layouts (2026-09-04)

Steam's `MenuDefine.xbn` is not the phone's: the main menu's commands are one column of
96x28 frames in a side panel with alignment 6 (a value the phone enum does not have),
its title cells are 300 px strips with the words left-justified inside, and its OAMs
carry flag 8 (drawn at 0.6 of the sheet's width and 2/3 of its height). The phone code
put the hand on top of the word and hung the text from the frame's top. `SteamLayout`
(active when the content is a Steam FF3 install) centres text in frames taller than the
line, puts the hand a gap before the text with the cell's drawn right edge in mind
(`SteamCells.DrawnRect`), lifts it to the text's middle, and the title centres by the
columns its strip actually paints (`SteamCells.CellVisibleSpan`, reading the sheet's
alpha). Nothing there changes the layouts, which stay Steam's.

## Direction set by Karl (2026-09-04)

The destination in full, with the layering and order of work: `Docs/OpenFF-Engine.md`.

- **Nothing of one game's is needed to play the other.** A player who owns only FF4 plays
  FF4. Where the FF3 logic reads a table FF4 compiled into its executable, the client
  synthesises the table (`MovementDefaults`); it never ships FF3's file.
- **Mixed content is coming.** FF3 and FF4 assets are two *versions* of the same kinds of
  thing. The asset pipeline should take either as input and produce one unified internal
  object per kind - model, motion, map, cell bank, text, table - that the engine consumes,
  so a custom OpenFF mod can draw on both games (with whatever limits the two data sets
  impose). The format readers already live once in `Shared/`; the unified object types are
  the next layer, ours to design. Nothing in the client should assume a single game's
  layout below the `GameProfile` seam.
- **Before the public repository, the code stops looking like the decompilation.** The
  Android-shaped names (`MainActivity`, `android.*` namespaces, `syrcusW`, the JNI entry
  points) get renamed and the frame reorganised, behind regression runs of the FF3 path -
  the one that is nearly fully playable and must not break.
- Seen in an FF3 new-game cutscene: one treasure chest drawn with geometry missing while
  characters, monsters and the rest of the environment were fine. Screenshot to follow.

## Working rules

- Keep the game running at every commit; keep the old path behind a flag until the new
  one is proven, then delete the old path.
- Formats live once, in `Shared/`, compiled into both the editor and the client.
- Anything the editor learns about a format (the FF4 tables, the `.ncap` evaluator) is
  already the client's implementation - do not write it twice.
- `Docs/Testing.md` gets a case for every stage; the automated suites cover the formats.

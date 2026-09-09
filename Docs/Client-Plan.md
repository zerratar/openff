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
  phone's. So `Tools/gen_steam_cells.py` writes `OpenFF/Data/ff3-steam-cells.json` -
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

**Done (2026-09-03):** `OpenFF.exe --content="<FF4 install>"` lands Cecil (`p00_00`) in
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

### FF4, 2026-09-05: field commands, the cutscene engine's first cut, tools

The FF4-only commands are registered **by name** now (`Ff4Commands.ByName`, `Ff4Cutscene.ByName`):
the table is generated from the FF4 binary, and a number that moved between generations had
the name window swallowing `cleanUpEffectData2`'s string and derailing the opening scene
("opcode 14645 is outside the table"). `ScriptCommands.Dispatch` remembers the last ten
opcodes and prints them with that message, which is how the culprit showed. A same-named
FF4 command with the same operands at another number runs FF3's handler; 247 of 500 run.
`Compat/Ff4FieldCommands.cs`: confirm/confirmWait (the engine's Yes/No box, jumps by the
answer), waitByLocale (words: Japanese count, other count), jumpByLocale (falls through),
setRewardMessage/executeRewardMessageWindow (the message window), setPlayerLevel by FF4's
ids. Cosmetic commands (doors, footstep dust, BGM ducking, the jump history) skip quietly.
`Compat/Ff4Cutscene.cs`: the `ce_*` character side - slots set up with model and texture
through `CCharacterMng.setCharacterWithTexture`, motion files bound and started by id,
placed, turned, shown, faded, cleaned up; `ce_CameraPos/Target` set the free camera;
`ce_WaitTillEndOfMotion` waits. Not yet: camera motions (`ce_SetupCameraMotion` loads a
CMS2 set from EVT_CAMERA.dat, `ce_PlayCameraMotion(set, id, ?, loop)` - the format is the next
piece), `ce_SetMap` (a stage change inside a scene), expressions (face textures through
`bindChainTexel`), lights and toon shading, `ce_CallBattle`. The opening (`--map=e01_00`)
runs through with its characters; Baron town's script has no unanswered command.
Tools: `Tools/ssam_extract.py` (a mass file to files), `Tools/ff4_calls.py` (a command's
handler: operand layout and callees, from the unstripped libff4.so); `crystal lz` +
`crystal script --game=ff4` over the extracted CAST_SCRIPT.dat gives every FF4 script
as text, which is where the usage counts (`ce_*`: 20k uses on 42 maps) come from.

## Stage E - what "our client" can do that the engines cannot

Once both games run from the same code: higher-resolution textures by name (a mod drops
a 4x PNG next to the model), TrueType at any size, widescreen UI layouts, more casts per
map, longer scripts, new opcodes exposed to the script language, and a mod manifest the
editor's Export already writes. Each is small once A-D exist; none is possible before.

## Starting the client (2026-09-04, from Karl's note)

`OpenFF.exe` with no arguments starts FF3 from its Steam install: the client finds the
installs the way the editor does and remembers them, with the last choice, in
`%LocalAppData%\OpenFF\launch.json`. `--game=ff4` switches game, `--source=content`
uses our extracted Content directory instead of Steam; both are remembered, so the next
plain start repeats them. `--content=<dir>` still wins for a one-off. The log's `launch:`
line says what was chosen and why. (`Compat/Launch.cs`; `SteamInstalls` moved to
`Shared/Content` so both programs find the games the same way.)

## Points: spots placed in Crystal, objects in the engine (2026-09-05)

The scene file gained `points`: `{ name, x, y, z, yaw, tags }`, placed and dragged in the map's
3D view like characters (`map-scene.js` draws them as small boxes with a facing sliver, picks
them, and the gizmo moves and turns them; yaw is the engine's, 0 = +z), listed under "Points
(OpenFF)" in the hierarchy with a ◆ on anything that carries behaviours, edited in their own
inspector (name, spot, yaw, tags, behaviours, delete), saved with the attachments. The engine
makes a GameObject per point on entering the map - `<map>/point:<name>`, tags "point" plus
its own, `Transform` at the spot, a `MapObject` with Kind "point" - so a mod finds spawn
points by tag (`Game.World.Legacy.WithTag("spawn")`) and behaviours attach to `point:<name>`.
Goblin Survivors uses spawn points when the map has them. Testing E-24, C-33.

## The fifth API slice: motions, items, shops; Goblin Survivors (2026-09-04, night)

Karl's redesign of the arena into a survivors-style run needed three more pieces of the
game as API. **Motions:** the battle binds its motion sets onto the very models the field
uses (`characterMng.addMotion(id, "b_b01")` for party members, `"b_f<family>"` for monsters)
and plays them by id, so `Hero.BindBattleMotions` (common, magic, job and extra sets) / `Npc.BindMotions` + `PlayMotion(HeroMotion.MagicShot)`
/ `MonsterMotion.Attack` give the field the battle's casting and attack animations;
`MotionDone` says when one ends. **Items:** `Game.Items` reads all five item tables
(`ItemManager.*Count/*At` added) into `Item` - name, caption, category, price, jobs, slot,
attack/defence/evasion, elements, stat bonuses, weapon model; `Party.Items/RemoveItem/Equip/
Unequip/Equipped` work the bag and the equipment slots through the game's own `doEquip`.
**Shops:** `Game.Shops.Open(index, table)` is the script command BootShop with a PORT
override of the shop table (`shop.CShopManager.OverrideTable`), so the game's shop screen
opens on any map; `Info` lists what a shop sells. **Goblin Survivors** (`Samples/Survivors`)
replaces the arena: waves of goblins that walk up and swing, a hero who auto-aims and casts,
run levels with three cards, chests with wearable equipment, a trader between waves who opens
the shop or the next wave. Testing C-31, C-32. Found on the way: the field's own buttons are
X (menu), one shoulder button (menu or zoom by option) and Select (a debug encounter toggle),
so mods take keyboard letters (`Game.Input.KeyPressed("T")`); the samples moved to H/J and T.

## Crystal for OpenFF projects: behaviours on map objects, Run, the API reference (2026-09-04)

Karl asked for Crystal to think Unity when a project targets OpenFF. Three pieces:
**Behaviours on map objects.** `Editor/ModCatalog.cs` reads the project's built assemblies
in a collectible load context (the engine resolved from the client's copy) and lists every
`Behaviour` and `GameService` with its public fields, their defaults (from an instance) and
XML summaries (the generated csproj now documents). The map inspector (`map-editor.js`
`behavioursSection`) ends with "Behaviours (OpenFF)" for a character (`object:N`), an exit
(`exit:N`) and Terrain (`map`): add from the catalog, edit fields, remove, save. Saved as
`<project>/scenes/<map>.json` (`Editor/ProjectScenes.cs`; routes `/api/project/scene`,
`/api/project/scene/save`, `/api/project/code/catalog`); Export to OpenFF copies the folder
into the mod as `scenes/` (mod.json `scenes`). The engine (`OpenFF.Engine/Scenes.cs`) reads
each mod's `scenes/<map>.json` on entering the map: a GameObject per target in the legacy
scene, a `MapObject` component (kind, index, and for a character the `Npc` handle from the
new `Npcs.Existing(index)`), the behaviours with fields set from JSON (numbers, strings,
bools, enums, Vector3/Vector2/Color); owned by the mod, so a hot reload remakes them.
**Run in OpenFF** (`/api/project/run`): export, then start OpenFF.exe (`OpenFFClient.Executable`,
from launch.json or the development build) unless it is running. **API reference**
(`Editor/ApiReference.cs`, `/api/openff/reference`): OpenFF.Engine.xml as a searchable
dialog. Testing C-30, E-21..E-23. Later: a scene view that shows attached behaviours in the
hierarchy, gizmos for behaviour fields that are positions, and behaviours on spawned objects.

## The abilities slice: spells, monsters, the formulas (2026-09-04)

Karl's bullet hell, real-time fights and "a mod adds its own skill" all need the game's
abilities as data, not as battle-menu behaviour. `Game.Magic` (`OpenFF.Engine/Abilities.cs`,
host `Compat/EngineAbilities.cs`) reads the magic table (`itm.ItemManager`, item_parameter.pak
chain 3) into `Spell` objects - school, level (charges), kind, power, accuracy, elements,
targeting, conditions, the jobs that equip it, and the effect and sound the battle plays
(from the normal-magic table) - with names resolved through the message system on a map.
`Cast/CastOn/CastOnHero` play the effect (loading its pack, e%03d.efp, into the field's
five slots) and the sound; `Damage` and `Healing` are `btl.NewMagicFormula` ported over plain
`Stats` (so a mod's own creatures work), `CanCast/Spend` handle charges, `UseInField` is the
menu's own Cure-on-the-field path. `Add` puts a mod's own `Spell` beside the game's, with
`OnCast` for what it does. `Game.Monsters` reads monster.chaindata (names, family model,
level, HP, stats, weakness/resist, gil, exp) and the normal encounter table (`Group`).
`PartyMember` now carries the whole sheet (max HP, charges per level, stats with bonuses,
job skill, conditions, equipped spells) and `IParty` gained Hurt/Heal/SetHp/SetCharges/
GiveExperience/SetJob/SetStat/LearnSpell/ForgetSpell/Inflict/Cure. `IScreen` gained
`Flash` (the damage-floor red) and `PopNumber/PopMiss` (the battle's floating numbers,
whose sprite sheet is loaded on the field and released with the map). `IEffects` gained
Load/Loaded/Move/Scale/Pause/Follow/FollowHero. Legacy additions: `ItemManager.magicCount/
magicAt`, `MonsterManager.monsterCount/monsterAt/isLoaded`, `MonsterPartyManager.
loadNormalTable/findMonsterParty`. Testing C-28, C-29. Not yet: a mod's spell in the
battle's own menu (needs the command list hooks), monsters as fighting things on the field
(the mod does that with SpawnModel + Stats + Damage today).

## World verbs: ground and screen (2026-09-04)

`Game.Field.GroundHeight(at)` / `OnGround(at)` / `Walkable(at)` ask the map's collision
what the characters ask each frame: an arrow straight down from seven units above the
point through every active collision restrictor, in the stage's own space, against the
walkable-ground attribute; nearest hit wins (`chr.CCharacterEureka.calculateBottom` is the
model). `Game.Camera.WorldToScreen(world)` / `OnScreen(world)` run a world point through
the renderer's own camera and perspective matrices (`NNS_G3dGlb.cameraMtx/projMtx`, fx32,
GL column order) to the 800x480 units `Game.Draw` uses - names over heads, health bars,
markers. `Vector3` gained the usual helpers (Length, Normalized, Distance, Dot, Cross,
Lerp, MoveToward, FromYaw/Yaw) and `Vector2` exists for screen points. Testing C-27.

## The engine API, third slice: the raw material for new kinds of play (2026-09-04)

Karl's aim is mods that are not the game's game - real-time fights, a bullet hell over
the field, whatever the imagination allows - so this slice opens the frame itself.
`Game.Input` (`OpenFF.Engine/Input.cs`, fed by `Compat/EngineInput.cs`): the pad as
held/pressed/released flags and a direction, the pointer in screen units with press and
release, the keyboard by key name, and `Capture`, which takes all input away from the
legacy game while a mod runs its own. `Game.Draw` (`Drawing.cs`, drawn by `Compat/ModDraw.cs`
after the game): text in the game's font, rectangles, lines and sprites from PNGs the mod
ships, in 800x480 screen units, immediate-mode (draw each frame). `Npcs.SpawnModel` puts
any character-format model - a monster, an object - on the map as a plain figure with the
same handle as a character (position, walk, turn, scale, alpha, hidden, motion).
`Field.Encounters` turns random battles off and on; `Game.Battle.Start(monsterParty,
map)` runs the game's own battle and `BattleEnded` reports Won/Lost/Escaped (for
encounters too). Not yet: drawing into the 3D scene, a ground-height query, and
world-to-screen projection for HUD markers over characters - the next of this kind.
Testing C-25, C-26.

## The engine API, second slice (2026-09-04)

Coroutines (`OpenFF.Engine/Coroutines.cs`): `Game.Run(IEnumerator)` steps a routine once
per engine frame; it yields `Wait.Frames`, `Wait.Seconds`, `Wait.Until`, `Wait.Dialogue`,
`Wait.Walk(npc)`, `Wait.HeroWalk`, or another routine; a mod's routines end with it.
`Behaviour.StartCoroutine` as in Unity. The events the game's own scripts raise as they run
(`Compat/EngineHooks.cs`, one line each in the decompiled code): FlagChanged, MessageShown,
CutsceneStarted/Ended, BattleStarting, ItemGained, WarpRequested; and Answered for the
API's own question. The API grew: `Dialogue.Ask` (the field's Yes/No box over the message
window, answered by tap, up/down + A, or B); the hero walks (`MoveTo`, `Stop`, `Moving`),
looks, plays a motion, shows a balloon; a character plays a motion, has Alpha, Hidden,
Balloon and Scale; `Party.Members` (id, slot, name, level, hp, mp, job), `AddMember`,
`RemoveMember`, `SetLevel`, `HealAll`; `Game.Camera` (MoveTo, LookAt, Follow, Shake, Zoom,
Reset over the field camera) and `Game.Effects` (Spawn/Remove by the game's effect table).
The sample's talk is now one coroutine that uses most of it. Testing C-23, C-24.

## Crystal makes a mod's C# project (2026-09-04)

A project targeting our build gets code in one click (`Editor/ModCode.cs`): Project ▸ Add
C# code writes `code/<Name>.csproj` referencing the client's `OpenFF.Engine.dll` (found
through `OpenFFClient`, never copied into the mod), a starting `Mod.cs` and a `.gitignore`;
Build C# code runs `dotnet build` with the output in `build/` and shows the first errors;
Open C# code in editor hands the csproj to Visual Studio, Rider or VS Code, so nobody is
limited to Crystal's own text editing; Export to OpenFF carries the assemblies and their
symbols into the mod folder and names them in `mod.json`. The client loads them and
hot-reloads a rebuild. `Samples/HelloMod/install.cmd` installs the sample the same way.
Testing E-19, E-20.

## The engine API, first slice (2026-09-04)

`OpenFF.Engine/Api.cs` declares what a script may do, as interfaces a service implements;
`OpenFF/Compat/EngineApi.cs` implements them on the legacy game by calling what the
FF3 script command handlers call, and registers them before any mod loads. Reached as
`Game.Dialogue` (Say a text in the field's message window, with the tap-to-continue mark;
IsOpen; Closed), `Game.Hero` (Position, Yaw, Model, Teleport, Face, Freeze/Unfreeze),
`Game.Npcs` (Spawn a character model on the map: Position, Teleport, MoveTo over frames,
Face, LookAt, SetAi, Solid, Remove, and an Interacted event when the player presses A
within InteractRadius), `Game.Flags` (the scripts' flag space), `Game.Party` (Gil, AddItem,
ItemCount), `Game.Audio` (PlaySe, PlayBgm, StopBgm), `Game.Screen` (FadeOut/FadeIn) and
`Game.Field` (Map, Warp). A mod registering its own implementation later in the load order
replaces one for everybody. Positions are world units (the legacy 1/4096ths hidden), yaw
in degrees.

Two things learnt on the legacy side: a character with no script cast behind it does not
walk through MoveCharaImp (its acceleration is reset the frame after), so `MoveTo` steps
the position itself and lets the WALK action play the motion; and a spawned character's
character-collision (SetCharacter_CharaCollision, flag 4) shoves the hero frame after
frame when it stands close, so spawned characters are non-solid unless asked. A text in
the message window that is in no message file goes through `MessageWindow.mwSetMessageText`
and `CMessageWindow.createText`. `Samples/HelloMod` exercises all of it (Testing C-22).

Next: the rest of the vocabulary as it is needed - camera, effects, motions, party members,
battle - and the events the legacy dialects raise as they run.

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

`mods/` beside `OpenFF.exe`, one mod per subfolder: `mod.json` (name, version, author,
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
inspected: `OpenFF.exe --debug=all --screenshot-every=3 --screenshot-dir=<dir>` and read the
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

## FF4 scenes: camera, stage and faces (2026-09-05)

The `ce_*` scene engine now stages a scene the way FF4 does, read out of the binary with
`Tools/ff4_calls.py` (operands, callees) and `Tools/ff4_disasm.py` (the bodies):

- **Camera motions** (`Ff4CameraMotion`): EVT_CAMERA.dat holds one LZ'd "CMS2" set per scene,
  a table of {id, offset} to "CM4" motions of `frames` frames and eight channels - rotation
  quaternion x, y, z, w (fx12), position x, y, z (fx32), field of view as a 16-bit half-angle
  index. A channel is `u16 keys | u16 type | keys`; delta keys (types 0/1/2: u8/u16/u32 frames
  with s8/s16/s32 delta) add their delta once per frame from zero, type 3 is a float per frame,
  type 4 one constant. `CameraHandle::calculatePosition` builds R x T, puts the camera at T,
  looks along (0,0,-1) x R with (0,1,0) x R up - **row vectors**, so up is the matrix's second
  row and forward the negated third; the columns give a plausible but wrong scene (Karl saw
  "angles off in some transitions") - and calls setFOV(sin, cos) of the half angle. The frame
  shown first is frame 1. `CWorldCamera.ExternalDrive` is a hook the camera calls in place of
  its mode controllers (MODE_FREE recomputes position from distance and angle every frame, so
  Pos_set alone never took). `ce_PlayCameraMotion(slot, id, ?, loop)`,
  `ce_WaitTillEndOfCameraMotion` suspends until a non-looping motion ends; `ce_EndEvent` and
  a map change let the field camera go. `eventCameraSetFovyMove(degrees, frames)` = the full
  vertical FOV in degrees, moved over frames, taking precedence over the channel until the
  next Play. `--ff4cam=off` skips the motions (debugging the stage).
- **Map motions**: `ce_SetMapMotion(slot, name)` loads `name.ncap.lz` onto the stage model
  (`CStageMng.addMotion`, a CMotSet like a character's), `ce_MapStartMotion(id, loop, ?, blend)`
  starts one, `ce_StartMapAnimation(index, type)` picks animation `index` of the stage's .namp
  by type (3 = visibility). e01_00's pack has 13 joint motions with the camera motions' ids;
  its .namp has 13 visibility animations that hide the flashback hall or the deck per shot.
- **Visibility animations were dropped by the port** (`NNS_G3dRenderObjAddAnmObj` kept only
  'J' and 'M'); `NNSG3dResVisAnm` parses BVA0 (4-byte header, u16 frames, u16 nodes, u32 size,
  bits frame-major: bit `frame * nodes + node`) and the SBC NODE command consults it. FF3
  never used one; FF4 scenes do, everywhere.
- **`CAnimation.startAnimation` made a new animation object and left the render object holding
  the old one** - C++ got the same address back from the heap; C# does not. It swaps them now.
- **`EngineHost` cleared the cutscene when the first stage name appeared** (the script was
  already loading its camera set); it clears on leaving a map only.
- **Expressions**: `ce_SetupExpression(slot, pack)` = FF3's `setChainTexture(ctrl, pack + ".face")`
  (FACE.dat: p00_001 ... p11_001, m095_001, n034_001), `ce_ChangeExpression(slot, 0 | 1, index)`
  = `bindChainTexel/Pltt` on "eye"/"eye_pl" or "mouth"/"mouth_pl", waiting for the texture
  loader like FF3's ChangeFaceEye.
- **Lights and shading**: `ce_CreateToonTable(index, r, g, b)` fills a 32-entry RGB555 table,
  index 100 applies it (`G3X_SetToonTable`, shading 0); `ce_SetShadingMode(slot, mode)` sets
  polygon mode MODULATE with diffuse 0x6739 / ambient 0x7fff / emission 0x7fff (mode 0) or
  TOON with diffuse 0x7fff and the rest 0 (mode 1); `ce_SetLightForCharacter(slot, light, x, y, z,
  r, g, b)` is the global light's vector (fx12) and colour.
- `ce_setFrameWait(frames)` is a wait (it was skipped, so scenes rushed); `ce_SetMap(name)`
  names the script's own map in every script but e23_02 (logged when it differs).
- **The story-scene chain**: FF4 plays scenes on a separate part (ContEventPart) and comes
  back to the world at a return map. Here everything runs on the world part, so
  `conteEventJumpAndReturnMapJamp(event, part, returnMap, x, y, z)` records the return map and
  position (EventConteParameter::setReturnMapName/setPlayerPosition) and map-jumps to
  `e<event>_<part>`; `ce_setConteNextPart(map, x, y, z)` replaces the return map;
  `ce_CallBattle(id, ?, ?, map, x, y, z)` would fight and return to `map` - with no FF4 battles
  it jumps there at once; `ce_EndEvent` jumps to the return map when it is not the current
  stage. `ce_SetMap` to another map (e01_01 -> e01_18) swaps the stage under the running
  scene (`sceneMng.gotoStage` + `stageMng.setStage`) and `Ff4Cutscene.StageSwapPending` tells
  EngineHost not to treat it as leaving the map. A new game starts on t00_00 at the origin
  (NewGameInitPart's message to the world part), where the castle script starts scene 1.
  FF4 dispatch now catches a throwing FF3 handler and logs it once (the castle's addItem with
  FF4 item ids took the client down).
- **Sound**: `ce_PlayBGM(bgm)` / `ce_SlotBGMPlay(slot, bgm)` / `ce_SlotBGMStop` / `ce_StopBGM(fade)` /
  `ce_SetVolumeBGM` / `ce_SlotBGMSetVolume` drive MatrixSound's four BGM slots (BGMnn.akb by
  number); `ce_PlaySE(bank, no, volume, pan)` and the `_slot` variants are FF3's SE player;
  `ce_StartVoice(file.ahx)` plays SOUND/VOICE/en_<file>.akb (ja_ when English is missing)
  through OggSound as a SoundEffect and `ce_EndVoice` waits for it; `--novoice` mutes voices.
- **The field event camera** (`Ff4EventCamera`): `moveCamera_AbsoluteCoordination(x, y, z, frames,
  alsoTarget, ?)`, `moveCamera_RelativeCoordination`, `setCamera_AbsoluteGaze(x, y, z, frames, ?)`,
  `setCamera_RelativeGaze` move a position and a target linearly over frames (FF4's
  EventCamera::setPositionLinerMove / setTargetLinerMove); `setCameraOffset(pos xyz, trg xyz, ?, ?, ?)`
  follows the leader at pos-offset, looking at that point + trg-offset (CUFollowCamera::set);
  `changeCamera_Mode`, `setCamera_BeforeEvent`, `cancelCameraControl`, `moveCamera_LookPlayer2` and a
  map change hand the camera back. FF3's handlers for the same names set Pos/Trg in MODE_FREE,
  whose controller rebuilds the position from a distance FF4's maps never set - the throne
  room scene played at the party's legs. By-name FF4 handlers now take precedence over the
  "same number, same operands" reuse of FF3's handler in the table build.
- **The follow camera on FF4 maps**: FF4's map parameters carry no camera chain; its
  `world::WorldCamera::initialize_usr` puts the camera at the leader + (0, 90, 85) looking at
  that point + (0, -73, -80). `CBaseSystem.setupCamera` uses those offsets on FF4 (FF3's built-in
  (0, 110, 110) / (0, 10, 0) framed Baron town from above its gate);
  `setWorldCameraPosAndTargetOffset(offset xyz, target-from-offset xyz, ?, ?)` changes them.
- **2D plates** (`ce_3DSSetup` and the field spelling `3DSSetup`, `..SetAlpha(from, to, frames)`,
  `..SetPosition`, `..SetVisiblity`, `..Release`): a `sys2d.Sprite3d` on the MAIN3D plane from the
  named .ncer/.nanr/.ncbr (np00 = "Baron"), alpha faded per frame by `Ff4Cutscene.Tick`.
- `clearCountJump(count, label)` jumps on a first playthrough (count 0); `setChacterOffset(cast,
  x, y, z)` is a pose-matrix translation on the cast's model; `setMessageAlignment` is quiet.
- **Quiet on purpose** (Ff4Commands.Cosmetic, with the reason beside each): the battle-theme
  and BGM bookkeeping, `decantLevelChekcJump` (jumps when an augment level equals 2 - it is 0
  in a new game) and `checkCharacterStatusJump`, the party roster and equipment commands
  (`addPartyPC`, `subPartyPC`, `addAbility`, `setPartyPCEquipItem` - FF4's own player and item
  data, not FF3's; the field draws only the leader), bind objects, effect scale, the second
  screen's sub-plane. The FF4 data model - `player.chaindata`, the 4-chain `item_parameter.pak`
  (consumables 48 bytes, weapons 88, armour 84, key items 32; the editor's PakRecordsFf4 has the
  fields) - is the foundation the menu, the roster and battles all need, and the next stage.
- **What the scene still lacks**: `setEffect_Scale` (packs load by name now), the castle's
  `addItem` (FF4's item tables), the 2D
  sprites (`ce_3DSSetup`: the "Baron" plate from /2D/MIDDLE_EVENT), `ce_CallBattle`, `ce_setFog` (FF4 has fog: the overworld enables it in `WSPrepare` with range
  0x80000..0x200000 and colour 0x73f5; the port has no fog at all), per-character light
  enables. The clear colour is black, as `ContEventPart::initialize` sets it - the sky in
  e01_00 is geometry ("sora", "kumo"), not a backdrop.

## FF4's player tables, read from the binary (2026-09-05)

**Started 2026-09-05 (Karl: "do the unified approach"):** `Shared/Data` (namespace `OpenFF.Data`),
compiled into the editor and the client like the other Shared code: `ChainPack` (the tables'
container), `Tables.cs` (`GameTables`, `CharacterDefinition` with `LevelRow` growth,
`ItemDefinition` with `EquipStats`, `SpellDefinition`, the five `Attribute`s), `Party.cs`
(`Party`, `Character`, `ItemStack` - the runtime roster the scripts and Game.Party act on),
`Ff4Tables` and `Ff3Tables` (the readers; `TableFiles.Read` picks by the item pack's chain
count) - FF3's items, spells and experience curve come through the same shape, its per-job
growth not yet. Monsters too (`MonsterDefinition`: FF4's 152-byte records with attributes at
0x12, drops at 0x6C, experience at 0x88 and gil at 0x8C; FF3's 100-byte records with the drop
block at 0x54), named from babil_battle.msd / eureka_battle.msd; `Ff4Monsters` is Game.Monsters
on FF4. The editor's msd reader moved to `Shared/Text` so names resolve
on both sides. `crystal tables <install>` dumps a game's tables through it (E-25).

The client's side (`OpenFF/Compat/Ff4Party.cs`): `Ff4Party.Tables` reads FF4's tables from the
content chain once, `Ff4Party.Party` is the roster (a new game: Cecil, type 0, at level 10 -
what `initForNewgame` leaves), and the scripts' `addItem`/`subItem`/`addPartyPC`/`subPartyPC`/
`setPartyPCEquipItem` (left hand, right hand, head, body, arm)/`addAbility` act on it;
`Ff4PartyService` is Game.Party on FF4 (FF3 keeps LegacyParty over pl.PlayerParty). A joiner
takes the leader's level until FF4's own rule is read. Test C-38. `Ff4Items` is Game.Items on
FF4 from the same tables, and `Ff4Menu` is the first thing built on nothing but OpenFF.Data: a
status menu the engine draws (Game.Draw/Game.Input, as a mod would) on the pad's menu button -
party page and bag page (C-39). FF3's menu part is blocked on FF4 (CStateWorldMove), where it
crashed on FF3-only face cells. FF4's own menu layouts (MenuLayout_*.xbn) remain to be ported
onto this data. `Ff4Battle` is the second: an ATB fight on the current map over the unified party
and monsters (FF4's m<family>_00 models with b_m<family> motions 101/201; the leader's
b_p_player_<type> motions 2007-2010), with Fight/Item/Run, targets, damage pops, victory paying
experience, gil and rolled drops into the party (C-40). K starts a test fight against group 1. The hooks are
connected: `bootEventBattle(group, ...)`, `Game.Battle.Start(group)` and `ce_CallBattle` (which
jumps on after the fight, or at once where the scene has no field hero) start it, and random
encounters come from the map's parameter pack (`Ff4Encounters`: the encount record's rate at 0,
the monsterParty record's first group and three cumulative percentages) - the Mist cave rolls
groups 10..13 at rate 11; towns are 0; the overworld's pack holds one such four-record pack per chip (256 x 192
bytes) and the chip under the party (`stageMng.getChipName`, f00_48) picks it - its rate word is
a land-form table instead, so 6 stands in. `Game.Field.Map` answers the chip's stage (f00) on the
overworld, whose stage has no name of its own. `monster_party_table.bbd` is FF4's encounter-group table: 520 records of 140 bytes, id
first, up to six slots of monster id, flag and x/y/z placement (`MonsterParty` in OpenFF.Data,
read for FF3's 18-byte records too). The damage
formula is a placeholder (attack x 2 - defence, +-10%) until FF4's is read from libff4.so;
FF4's monster record has an attack-like word at 0x20 and a hit chance at 0x22.

The stage after the scenes is FF4's data model - the party, its members' growth, items -
because the menu, the roster commands (`addPartyPC`...) and battles all stand on it. Opening
notes from `pl::PlayerParty::load`, `levelParameter`, `normalMagic`, `normalAttack` in
libff4.so (Tools/ff4_disasm.py) and the file itself (`player.chaindata.lz`, 37 chains):

| chain | bytes | records | what |
| --- | --- | --- | --- |
| 0 | 396 | 99 x u32 | experience to reach each level (0, 16, 47, 105, 204...) |
| 1 | 1259 | 63 x 20 | normal attacks, id in the first word (`normalAttack(id)` walks 0x14 apart) |
| 2..16 | 1187 each | 99 x 12 | one per PLAYER_TYPE (15 of them): the level table, `levelParameter(type, level)` = table[type] + 12 * (level - 1); 12 bytes = u16, u16, u16 then six bytes (hp, mp, ?, then the stats) |
| 17..31 | 52..252 | size / 4 | fifteen small u32 tables, stored in order (`+0x22a0`, counts at `+0x2318`) |
| 32 | 3936 | 123 x 32 | magic, id in the first word (`normalMagic(id)` walks 0x20 apart) - FF3's 32-byte magic record |
| 33 | 400 | 50 x 8 | (id, value) pairs: 0->0, 1->11, 2->12, 3->13... |
| 34 | 1296 | size / 4 | the engine counts it in u32s; the editor's 108-byte guess is unconfirmed |
| 35 | 1104 | | zeros at the start |
| 36 | 519 | size / 8 | |

FF4's `Player::initialize(type)` builds a member from `GameParameter::playerSaveParameter(type)`
- the save block per character (equipment at +0x24, abilities at +0x164, learning at +0x184)
- not from these tables directly; `initForNewgame` (3752 bytes of code) fills those blocks
for a new game. That save block, per PLAYER_TYPE, is the shape our FF4 party has to take.
The 15 types match the scripts' `addPartyPC(0..14)` and the models `b_p_player_00..12`.

## FF4 saves on the unified layer (2026-09-05)

FF4's own save part did not come across with the port, and FF3's legacy save file cannot
hold FF4's party. The engine's save chunks (OpenFF.Engine/Saving.cs, built for mods) hold
the whole FF4 game instead: `Ff4PartyService` is an `ISaveable` ("ff4/party": roster with
levels, experience, hit and magic points, equipment, abilities, line-up slots; the bag; gil)
and `Ff4FieldState` another ("ff4/field": map, position, the leader's rotation, every set
script flag as "group:index", and a summary line). A slot is offsets 1..3 in
`saves/ff4.json` - FF4 gets a store of its own (`EngineHost.Attach`) so FF3's mods.json is
untouched. `Ff4Saves.Save/Load` drive it; the menu (`Ff4Menu`) grew Save and Load pages; at
boot `--load=<slot>` reads the slot before the parts start and `JumpPart` takes the stage,
position and rotation from `Ff4Saves.Pending` instead of --map/--pos; in play the Load page
warps through `Game.Field.Warp` (the overworld gets its chip through `JumpPart.WithChip`).
`SaveChunks` gained `Slots`, `WrittenAt` and `Peek` so a menu can list slots without handing
chunks to their owners. Mods' saveables ride in the same slot as they do on FF3. Test C-42.

Not in a save yet: NPC positions and states, the scene chain (`SceneStage`), the conditions
map, a member's `SetStat` overrides (the growth table wins on restore), and the legacy save
file's own contents (the game's write at map entry still lands as slot 41568 - harmless).
FF4 restricts saving to the overworld and save points (a flag on the map); ours saves
anywhere until that flag is read.

Seen while testing: a few steps north of the d01_00 arrival the FF4 follow camera sits
inside the cave's rock, on a fresh start as much as after a load - the dungeon ceilings are
drawn from above where the real game's camera (or its culling) keeps them out of view. A
camera stage for FF4 dungeons is owed.

## FF4 magic, from the binary and its tables (2026-09-05)

The MP costs and powers were not in player.chaindata's chain 32 after all (that 32-byte
record is `pl::PlayerParty::normalMagic`'s, with an effect id at 10). Following
`btl::NewMagicFormula` and `pl::Player::isUseMagic` in libff4.so (Tools/ff4_disasm.py,
ff4_fields.py, and the new Tools/ff4_callers.py for who calls what) led to
`common::BabilMagicParameterManager`, which loads **magic_parameter.bbd**: 36 bytes per
spell - id s16, power s16, school byte (0 white, 1 black, 2 summon, 3 song, 6 ninjutsu),
MP cost byte at 5, hit rate u16 at 6 (100, or 45 for Hold, 30 for Death), effect group
u16 at 10 (0xA0 the cure line) and rank at 12, element bits at 22 (0x20 fire, 0x10 ice,
0x08 lightning, 0x80 earth, 0x100 holy), status inflicted at 24, granted at 26 and 28,
and a target byte at 32 (0x01 all, 0x02 one, 0x08 may spread, 0x10 in battle, 0x20 in
the menu, 0x40 chosen). Cure 3 MP power 24, Fire 5 MP power 20, Meteor 99 MP power 250:
the numbers the game shows. `SpellDefinition` carries them; `GameTables.Spell(id)` finds one.

The formulas, as the disassembly reads: attack damage = power x caster level x caster stat
(will for white, wisdom for the rest) / (target will + target level + target magic defence),
times 1.0..1.3 (rand32(301)/1000); when spread over n targets times (90 - 10n)%. Healing =
(target vitality / 8 + caster will / 2) x power, times (100 - rand(10))%, spread the same
way. `Ff4Battle.AttackMagicDamage/HealingValue` do exactly that. Status spells roll the hit
rate; only death is carried out so far.

**efficacy.beld** (`EfficacyDataConvection::loadBELD`) gives what items do: a potions
section (id, hp, mp: Potion 100, Hi-Potion 500, X-Potion 1000, Ether 50 mp, Elixir
9999/9999, Phoenix Down 0/0 = revive) and a section of abilities items cast (40 casts
1501, the Goblin summon item). `Efficacy`/`GameTables.Efficacy(id)`; the battle's Item
command reads it instead of the old by-id amounts, and asks whom to use it on.

**Learn lists**: player.chaindata chains 17..31 are one list per PLAYER_TYPE, u32 =
ability << 16 | level: the class's commands (1 Fight, 3 Item, 0x2e Change, then its own -
0x1f Jump, 0x0a Cover, 0x40 Pray, 0x34 Recall...) and under each magic command (6 white,
5 black, 13 summon, 4 sing, 0x53 ninjutsu) the spells with the level each is learnt at.
Every list ends with Sing and the eight songs - the Bardsong augment - so those are kept
only for type 11 until augments exist. The lists settle the PLAYER_TYPE order, which the
earlier guess had wrong: 0 Cecil (Dark Knight), 1 Cecil (Paladin), 2 Kain, 3 Rosa, 4 Rydia
(child), 5 Rydia (adult), 6 Tellah, 7 Porom, 8 Palom, 9 Yang, 10 Cid, 11 Edward, 12 Edge,
13 FuSoYa, 14 Golbez. `CharacterDefinition.Learning`, `SpellsAt/CommandsAt`;
`Character.Learn()` runs at every SetLevel, so joining and levelling up teach spells.

In the client: the battle's command window is Fight, Magic, Item, Run; Magic lists the
member's spells with costs, picks an ally for healing, reviving and white buffs and a foe
otherwise (0x01 in the target byte hits every one); `--party=<type[:level],...>` adds
members for a test start; the menu's party page lists magic with costs. Test C-43.

Not done: spreading a single-target spell over all (the 0x08 toggle), status effects
beyond death, summons' own animations (they cast as black magic), monsters casting, the
white/black command split (one Magic command holds all schools), monster magic defence
(the record field is not named yet; 0 stands in), and the physical formula.

## Physical blows, camera tables, and a scripted drive (2026-09-05)

**Physical formula** (`btl::NewAttackFormula`, read with Tools/ff4_disasm.py): calcHitRate =
weapon hit + attacker agility - (target evade + target agility) + 20, clamped to 0..100
(x0.8 when blinded); calcDamageValueForBabil's core = attack x attacker level x attacker
strength / (target defence + target level + target vitality), times 1.0..1.3, then the
element, row, critical (x1.5) and status factors, and x1.2 from a party member onto a
monster or x0.7 the other way. `Ff4Battle.Hits/Damage` carry the core, the hit roll and
the side factor; elements, rows, criticals and statuses are still owed. The monster record's
blocks: `BattleMonster::setMonster` copies 16 bytes from 0x4C (defence at 0x4C, evade at
0x50 - a Goblin's 20 and 5), a word at 0x68 (read as magic defence) and the five stat bytes;
the attack word at 0x20 and hit at 0x22 stand until the monster's physics-attack block is
named. `MonsterDefinition.Attack/Hit/Defence/Evade/MagicDefence` (FF4 reader; tentative).

**Camera tables**: `world::WSPrepare::wsProcessSetupCamera` picks the follow camera's
offsets by the map's kind letter: fields ('f') stand at leader + (0, 100, 110) looking at
leader + (0, 30, 30); dungeons ('d') at (0, 80, 105) looking at (0, 27, 25); everything
else keeps `WorldCamera::initialize_usr`'s (0, 90, 85) and (0, 17, 5). It also sets the
clip to 11..2048, the FOV words 0x424/0xf74 and, on fields, the camera's limits from the
stage's edges. `CBaseSystem.setupCamera` applies the three offset pairs; the rock over
the waterway a few steps from its arrival is still drawn (culling overrides make no
difference), so that is geometry the game hides some other way - open.
`setCamera_PositionOffset/TargetOffset` (128 maps) are the EVENT camera's relative slides
(current position + offset over frames, the target following when the flag says); they are
handled through `Ff4EventCamera.MoveBy/LookBy`.

**Scripted drive** (`Compat/Drive.cs`, `--drive=<file>`): the machine was locked while
these were built, and key2.ps1's SetForegroundWindow taps went to the lock screen. The
drive plays a step file from inside the game - wait / press <key> [ms] / until <regex> [s]
/ say / quit - injecting keys where the keyboard is read (`DesktopInput.Injected`, read
without focus; the engine's Game.Input sees them too) and waiting on log lines (Log.Written,
with the lines since the last satisfied until counted). `Docs/Drives/ff4-battle-magic.drive`
plays C-43/C-45's fight in about a minute; test C-44. PowerShell drives (key2.ps1) remain
for the FF3 title, which needs mouse taps.

## FF3 onto the unified layer: jobs and spells (2026-09-05)

FF3 grows by job, so the growth sits on a new `JobDefinition` (Shared/Data/Tables.cs): 23 jobs
in pl.JOB_TYPE order, each with its six growth types (player.chaindata chain 1, 23 x 6 bytes:
the curve for strength, vitality, agility, intellect, mind and the charge table), 99 LevelRows
whose attributes come from the eight curves (chain 2, 99 bytes each), whose hit-point gain is
level + vitality up to level + vitality + vitality / 2 (pl.Player.setHp; 32 at level 1), and
whose `Charges[8]` come from the seven charge tables (chains 4..10, 99 x 8). Names are
eureka_menu.msd 50105 + job (wmenu.CWMenuJob), with English fallbacks marked tentative.
`GameTables.Jobs/Job(id)`, `crystal tables --game=ff3` lists them (test E-26). The 52-byte
magic records now fill `SpellDefinition`: `Level` = magicClass + 1 (the charge level), power,
hit rate, `UseKind` (0 attack, 1 recovery, 2 special, 3 status), element bits, status and the
`CanUse` job mask; schools by id range (4001 white, 4101 black, 4201 summon, 6001 songs).

Still to come for FF3 on this layer: a read-only view of pl.PlayerParty as an OpenFF.Data.Party
(so Game.Party and the editor speak one shape), and eventually the FF3 battle and menus on
the same components as FF4's - the largest piece of the engine plan.

## FF4 shops, and the scene battle's hold (2026-09-05)

`bootShop(row, ?)` (Ff4Commands) opens `Ff4Shop`, the engine-drawn shop on the unified party
and items, and holds the calling script (suspendRedo) until it closes. The row is the record
in MENU/babil_shop.bbd, 124 bytes each: a 32-byte label, the keeper's title id and six line
ids (babil_menu.msd 51220.. and 51260..), up to sixteen item ids - read straight from
`world::MSSShop::mssInitialize` (row x 124, one 0x7C read) and `babilCommand_BootShop`
(the byte lands in the menu node's word at 0x58). Row 0 is the "Debug Shop!"; the Steam
build's Baron interiors boot rows 1 and 2 (t01_40: weapons, armour) and 3 and 40 (t01_60:
items). (An older script set in the scratch area named other maps and rows - the Steam
CAST_SCRIPT.dat is the authority.) Prices are the item
records' own: buy at 0x1C, sell at 0x20 (half). `Game.Shops` on FF4 is this service
(`Open(row)`, `Info(row)`); O opens row 1 anywhere for tests (C-47). Not done: the inns
(no bootShop; their scripts ask and heal), the wares' "equip who" preview, FF4's own layout.

The scene battle: `ce_CallBattle` now holds its script (suspendRedo) while the battle runs,
as the battle part took over in FF4; the story goes on from the return map through the
jump `AfterBattle` makes, never from the line after (the first drive showed the next scene
starting under the fight). The battle's result message is closed by A/B on Game.Input,
since the legacy window's own button is gated off while the battle holds the input.

## FF4 inns (2026-09-05)

`bootInn(price, ?, ?)` (babilCommand_BootInn: message, gil and confirm windows, then a hold)
opens the engine's Yes/No box through `Ff4FieldCommands.Ask` and holds the script until it is
answered; Yes with enough gil takes the price. `selectEndWait(label, ...)` then jumps when the
party did not stay - FF3's handler jumps on `cast_getInnConfirm()`, and the FF4 scripts put the
"come again" line at the label and the night's routine after the command. `setRecovery2(order,
?, ?, amount)` restores one member (order 1..) or all (0), 9999 meaning everything;
`setConditionRecovery(...)` clears the statuses the party service keeps. Test C-48; 24 maps
have inns (t01_30 is Baron's, 50 gil a night).

## FF4 menu: equipment and item use (2026-09-05)

`Ff4Menu` grew what the loop needed once shops sold things: A on a member opens their five
slots; A on a slot lists the bag's fitting items - an item fits when its position word's low
bits name the slot (1 right hand, 2 left hand, 4 head, 8 body, 16 arms; a weapon with 3 goes
in either hand; the high word is not read yet) and its canEquip mask has the character type's
bit - plus "(take off)"; equipping goes through `Party.Equip`, which returns the old item to
the bag. A on a consumable with a usable efficacy (hit or magic points, or Phoenix Down's
revival) asks whom to use it on and applies it with the same numbers as the battle's Item
command. Tents and the like wait for camping. Test C-49.

## FF4 encounters, read again from the binary (2026-09-05)

The earlier reading of the map parameter pack was wrong in one place: the groups. `world::
MapParameterManager::load` keeps the four chains in order - 0 landFormParameter (50 bytes),
1 monsterPartyParameter (8-byte entries of four u16 group ids), 2 encountParameter (16 bytes),
3 unnamed - and `WSEncountSetting::wsProcess` reads the rate as the u16 at 2 x the
party's land form in chain 0 (the twelve u16s at 0x18 are the battle stage per land form,
`world::battleMapID`, over 30 = none), rolls a group among the four of the land
form's set in chain 1 (a static helper; up to five re-rolls when it repeats the last fight),
and hands chain 2's first s16 to `world::attackType` with the party's average level (back
attacks, pre-emptive strikes - not applied yet). So the Watery Pass (d01_00) is Sword Rat +
Goblin, Tiny Mages and Fangshells at rate 9, and the Baron plain chip a Floating Eye, a
Helldiver or three Goblins at rate 1 - not the "base group 10 + 0..3" the old reading gave
(that u32 and its three floats are chain 2's attack-type parameter). The land form under
the party (`PCObject` + 0x340/0x348, from the ground polygon's attribute) is not read yet;
set 0 stands for every land form. `Ff4Encounters` and test C-41 updated.

## FF4 battles on their battle stage (2026-09-05)

Karl's biggest difference against the Steam game: FF4 moves the party onto a battle stage,
ours fought where it stood. `battle_map.dat` holds b00..b30 (a `.nmdp` model with a `.namp`
animation each; the b_soto/b_naka/b_uti backdrop entries are empty on Steam), and the map's
land-form parameter names the stage per land form (the u16s at 0x18 of chain 0: b01 for the
Baron plain, b08 for the Watery Pass). `Ff4BattleStage` gets there by a map jump - FF4's own
battle is a part change, and a stage swap under the running field (`sceneMng.gotoStage` /
`stageMng.setStage`, as a scene's `ce_SetMap` does) left the field's characters with dead
textures on the way back (`glTexParameteri` in `BindTextureWrap`). Begin remembers the field
map, the hero's position and facing and warps to `bNN`; the battle starts once the party
stands there (a few frames after the map name changes; gives up after 600 frames and fights
in place); End warps back to the spot left. On the stage the fight is a side view: monsters
on the left in the encounter table's own placements (x across, z depth), the party on the
right, the event camera (`Ff4EventCamera`) driving the view. Scene battles
(`ce_CallBattle`) stay where the scene is. Tests C-40 (K jumps to the map's stage) and C-50.

The view, from the binary (Karl asked whether the angle was right - it is FF4's own now):
`btl::CBattleDisplay::initialize` sets the battle camera's field of view to 641/4046 (18
degrees) and its clip to 10..2000; `readyOpeningCamera` starts it at (0, 32.7, 166) looking
at (0, 0, -34) and `goOpeningCamera` eases it a fifth of the way per frame for five frames
to `CAMERA_BATTLE_POSITION[type]` / `CAMERA_BATTLE_TARGET[type]` - (0, 45, 240) looking at
(0, -5, -20) for type 0, which 515 of the 520 encounter groups use (the type is byte 3 of
the group's record, `MonsterParty.CameraType`; types 1 and 2 are closer shots). A long,
level-ish shot down the stage towards -z: the backdrop (b01's mountains, lake and clouds, a
plane along the far edge at z about -67) fills the top, the monsters stand left in the
tables' own placements, the party right. `Ff4EventCamera` drives it (SetFov, SetClip, a
six-frame slide from the opening pose). BTL_CAMERA.dat's CMS2 sets (s00_00..s91_00) turned
out to be the ability and summon cameras (`ds::sys3d::CameraHandle`), not this one.

The party stands where FF4 puts it: `battle_parameter.chain` chain 0 (`BattleParameter::
partyRoot`, `BattlePartyPosition::position`) - records of 164 bytes, a u16 id and two rows
of five 16-byte slots (x, y, z, facing in degrees), record 0 the normal fight: the front
row at x 17..19 and the back row at x 29..33, z -25, -5, 12, 35, 50 down the screen, facing
-90 (towards the monsters); record 1 the back attack, 2 a pincer. Read into
`GameTables.PartyRoots` (`PartyRoot`, `PartyRootSlot`); `Ff4BattleStage.PartySpot` uses the
front row (a member's row is not modelled yet). Every member stands on the stage as FF4's
own battle model pNN_00 (`Game.Npcs.SpawnModel`) with the b_p_player_NN motion set - the
field's pNN_01 has different joints and binding the set onto it crashed the joint
animation - so the field's hero waits unseen (transparency 100) at the leader's spot and
the battle animates the spawned bodies (`Play(fighter, motion)`).

The HUD, after FF4's screens (Karl's Steam screenshots): the command window bottom left
(Attack / Magic / Items / Run, 41-px rows, a wedge for the glove), the party's rows bottom
right (name, HP / max, MP, the ATB gauge), "Z Confirm  X Back  M Run away" over them (M runs,
as FF4's), the target pick listing the foes with "Accuracy: NN%" and a card of the picked
one (name, HP, Weaknesses, Absorbs), and the result as FF4's window at the top - Gil Found
and New Total left, EXP right, level-ups and drops in a second window - closed by A. The
panels are drawn (translucent blue-violet, a light top edge, a pale frame); FF4's own window
art and layouts come next, see below. Gauges start staggered down the line so the leader acts
first (the drives rely on it).

## FF4's menu data, found (2026-09-05, Karl: "use the menu format/data we already have")

Karl wants the FF4 screens 1:1 from FF4's own data, as FF3's are. The Steam install has it:
`MENU_LAYOUT.dat` holds 34 `MenuLayout_*.xbn.lz` layouts (Root, Item, ItemWnd, Magic,
Equipment, Status, Config, Save, Suspend, Title, Formation, Ability, ShopSpr, Name, the
CS*/Chk* extras) in the same XBN binary-XML the FF3 client reads (`XbnFile`/`XbnNode`;
`Tools/xbn_dump.py` prints one as a tree): `layout > unit {name, display} > frame {x, y,
width, height} > frame {choices, group, id, x, y, width, height, link, top, behavior
"FBText" {parameter = message id, ...}}` in the DS's 256 x 192 units (RootMenu: a 72 x 136
frame at 20, 0 with 64 x 16 text rows every 16 px, messages 50002..). The binary's classes
are `layout::Frame`, `FrameBehavior`, `FBText`, `FBTextSCC`, `FBSprite`, `Layout::makeup`.
`MENU_Common.dat` holds the 2D art (NCGR/NCER/NANR, DS formats the FF3 port already draws):
`frame_00..05` (the window frames), `cursor` (the glove, animated), `button_00..05`,
`button_up_down`, `icon_16dot`, `icon_8`, `face`, `balloon`, `fukidashi`; `battle2d_Common.dat`
has `gauge_atb`, `battle_icon`, `button_battle_00..05` (the C/M key badges); `battle2d.dat`
`battle_number` (damage digits) and `battle_pause`. The battle windows have no XBN: their
frames are laid out in code (`btl::BattleCommandWindow`, `BasicBattleWindow`,
`BattleSelectWindow`, `HelpWindow`, `BattleHpGauge`, `BattleMenuNumber`) - to read from the
binary. Steam's look (soft gradients, a soft glove) is these DS assets scaled up, with text
from `arial.ttf` (SDL2_ttf) - so 1:1 means drawing FF4's frames and cursor at the layouts'
positions with our font.

Built the same evening: `OpenFF/Compat/Ff4Ui.cs`. The phone and Steam builds' .NCGR/.NCBR
are PNG sheets (8-bit palette or RGBA) and their .NCER cell banks the port's seven-word
parts, so the client reads a sheet through the game's own file system (`ds.g_File`, archive
members by name) into a texture (`Game.Draw.LoadTexture(key, bytes)`, new; the host makes a
Texture2D from the bytes) and the bank through `Shared/Content/CellBanks.cs` (Crystal's
Cells.cs reader, shared). `Ff4Ui.Window` paints the fill (winsample.NCGR's navy gradient,
else BasicWindow's 0x4A2214) and the frame's eight cells of window_frame_00 style 1 (the
blue bevel; style 0 is the white line; frame_01..05 are the other window colours) around a
rectangle; `Glove` draws cursor.NCER's cell 0 (pressed: cell 1) with its fingertip at the
spot; `Gauge` the ATB trough and a fill cut to the fraction. The phone UI's space is
1136 x 640 (the Steam window is exactly that; the glove is 85 px of 1122), so pieces are
drawn at 800/1136 of their sheet size. The battle HUD now draws only these: the command
window at (58, 300, 188 x 180) with 45-px rows and centred names, the party window at
(276, 338, 460 x 142) with 28-px rows (name, HP / max, MP, gauge), the hints above, the
target pick's foe list and card, the magic and item grid as one wide window of three
columns by four rows (`BtlMagicMenu::BMTEXT_POS` has the three columns; Left/Right step,
Up/Down move a row, `GridMove`), the last action in a small window at the top, the result
window. Those positions are measured from Karl's Steam screenshots; FF4 lays its battle
windows out in code (`btl::TouchWindow`, `BattleCommandWindow::create`, windows are
`menu::BasicWindow`s from `ui::CWidgetMng::addWidget(id, x, y, w, h, type, ...)`), so the
exact numbers are still to be read from the binary. The np00..np11 sheets turned out to be
the intro name plates (Cecil / Lord Captain / Baron Red Wings), useful for `ce_3DSSetup`.
Next: the menu from the XBN layouts with the same pieces, the damage digits from
battle_number, the battle windows' numbers from the binary.


## The FF4 menu in FF4's dress (2026-09-05, late)

`Compat/Ff4Menu.cs` rewritten on FF4's own pieces and data: the windows are Steam's
window.png / point.png through `Ff4Ui` (the phone frames when absent), the texts babil_menu.msd
(`Ff4Layouts.Text`: 50002 Inventory .. 50011 Abilities, 50204.. Right/Left/Head/Body/Arms,
50401 Lv, 50410 HP, 50411 MP, 50420.. the attributes, 50446 Gil, 50451 EXP), the command
list in MenuLayout_Root's order (its FBText frames; Load added after them since the root has
none), the Status screen's attribute rows where MenuLayout_Status puts them (frames 4030..
and 4080.., a DS unit two pixels down the main window), the portraits face.NCER (a cell per
player type). `Shared/Content/Xbn.cs` reads the layouts (`Xbn.ReadLayout`: frames with
absolute DS positions as `layout::Frame::setup` computes them); `Compat/Ff4Layouts.cs`
loads MenuLayout_<name> and the texts through the game's file system. The arrangement is
the Steam game's, measured from Karl's screenshots (the phone code that places the DS-unit
layouts on a 16:9 screen is not read yet): Root = the party's five rows on the left
(portrait, name, Lv, HP, MP) and the commands on the right, six visible with a bar, the
place and gil below; every other screen = a title bar, a main window and a footer of key
hints. Screens: Status (attributes, EXP, next level, what is worn; Z opens Abilities),
Inventory (two columns, a caption window, A on a usable item asks whom), Equipment (the five
slots; A lists what fits with Attack or Defense, Remove first; the old piece returns to the
bag), Magic and Abilities (three columns, MP costs), Party (A twice swaps two members; the
leader changes at the next map), Save and Load (three slots). Settings and Quicksave say
they come later. Left/Right on a member's screen switch member. The place line is the name
the map's own plate last showed (`MapNameWindow.LastMessageNo`, a PORT hook), else the map
id. Everything reads OpenFF.Data through Ff4Party - the unified path. Drive
`Docs/Drives/ff4-menu.drive`, test C-51; C-39 and C-49 updated to the new navigation.

## Battle digits and two fixes from Karl's screenshot (2026-09-05, late)

Damage and healing now pop in FF4's own battle digits (battle2d.dat's battle_number: cells
0..9 the 24 x 24 digits, 10 "Hit!!", 11 "CRITICAL!", 12 "MISS!", 30 "NO EFFECT!", 33 "DEATH", 34
"WEAKNESS"; `Ff4Ui.Number` / `Ff4Ui.Word`), white for damage, green for a heal, rising with a
small bounce and fading over 70 frames (`Ff4Battle.Pop`, `PopWord` for a miss) - the legacy
`Screen.PopNumber` stays for FF3. Karl's screenshot showed two Cecils on the stage and every
animation freezing after the first kill: the field's hero was "hidden" by a transparency the
renderer does not honour (now `setHidden`), and the one-shot motions (attack, hurt, cast) were
never followed by the idle loop - `Idle()` restarts it for every fighter whose motion has
finished (`Fighter.Acted`).

## The scenes' message bar (2026-09-05, late)

FF4's scenes speak over a dark, translucent bar across the bottom of the screen (Karl's deck
screenshots); ours showed bare text. In the binary `babilCommand_CE_ShowMessageWindow` reads a
byte and calls `evt::EventConteManager::enableMessageWindow(byte != 0)` - the scripts call
`ce_ShowMessageWindow(0)` after a `deleteMessage` to take the bar down for a pause, and the
next `startMessage` brings it back. The port: `menu.BasicWindow.SetBarStyle` (frames at alpha
0, the fill black at alpha 150), `menu.MessageWindow.mwSetBarWindow` (the window at (0, 236)
size 480 x 84 in the 2D plane, the text centred on 240), `wld.CMessageWindow.createBarWindow`;
`ff3Command_StartMessage2` creates the bar when a FF4 cutscene is active and no window is made;
`Ff4Cutscene` handles `ce_ShowMessageWindow` (0 releases the window) and releases it when the
scene ends. Field talks keep their framed window with the name tag. The intro run
(`ff4-story.drive`, 250 s) now shows: the deck scenes over the bar, the Mysidia flashback in
green, the Floating Eye fight with FF4's HUD on the deck, the flight, the castle scenes with
Cecil / Baigan / King of Baron name tags, the throne room. Still off: the flight's sky (a
blocky grey with black holes where FF4 shows clouds), the intro name plates (`np00..` sheets,
`ce_3DSSetup`), the scene's fog and lights.

## The intro scene, second look (2026-09-05, night; Karl's three reports)

Karl saw the sky fixed but the soldiers gone from the wide shots, a shadow on the deck's centre
with nobody over it, and the name plates flickering instead of fading in, holding and fading out.
Each traced to its cause, all three in `Ff4Cutscene` and its neighbours:

- **The sky**: `evt::ContEventPart::initialize` sets the scene camera's clip to 2..4096 (the field's
  10..500 cut the deck's sky dome away). `ce_StartEvent` sets it, `ce_EndEvent` restores the field's.
- **The soldiers**: `ce_SetEnbleViewClip(slot, 0)` turns view-volume culling off for a cast so the wide
  shots keep them; it sat in the `Quiet` set, silently dropped. It is `Characters.setViewVolumeClip` now.
- **The plates**: sprite alpha is the DS's 0..31 and `DS2DManager` scales it to 0..255 itself
  (`G3_PolygonAttr`, `NNS_G3dSetRenderColor`); a 0..255 value handed to `SetAlpha` wrapped the byte
  about eight times over a fade - the flicker - and left the held plate at 49/255, the faint text in
  Karl's screenshot. The plates pass 0..31 again, at half size (the phone's sheets are 2x its screen).
- **The shadow**: a scene cast has no shadow in FF4 until `ce_ShadowSetting` + `ce_ShadowVisiblity(slot, 1)`
  (e01_00 gives them to Cecil and the four soldiers only), and the shadow stands under the cast's
  `kosi` joint because the casts are moved by their motions' root - the object position stays at the
  origin. The port gave every model a disc at its object position, so every cast's disc lay stacked at
  the origin: the shadow on the deck's centre, and in the flashback hall. Now `setupCharacter`'s disc
  is off for scene casts (`CCharacterMng.setShadowVisible`, survives the asynchronous setup and
  `setHidden(false)`), the two commands are real (`ShadowSetting`/`ShadowVisibility`), and the disc
  follows the named joint's x and z through the port's existing joint capture (`reserveToGetJntMtx`,
  world space) with a ground hook (`CShadowObject.GroundQuery` -> `LegacyField.GroundHitFx`) for maps
  with collision - none of the event maps has one, as in FF4, which draws them at `height + 0x29`.
  A first cut selected FF3's shadow type 0 for FF4's type 0: that is the player's `shadow02`, a rounded
  body FF3 draws squashed to a quarter height, and at a cast's scale it stood on the deck as a
  translucent block (Karl's second screenshot). The type is left alone now. Open: the discs under
  Cecil and the soldiers are drawn at their hips' x/z but do not show yet - not visible enough to
  chase tonight; the field hero is hidden through a scene (its own shadow goes with it) and shown
  again at the end or on leaving the map.
- **The camera "transitions"**: two readings of `CameraHandle` in the binary. `ce_PlayCameraMotion`'s
  third operand is a blend length - `start` saves the displayed pose and `calculatePosition` slides
  position, rotation (`Quaternion::leap`) and FOV to the new shot over that many frames - but no
  shipped script passes one (627 calls, all `0, 0`), so it is read and logged, not built. What was off
  is the FOV: `calculatePosition` applies a motion's FOV channel only behind a flag nothing in the game
  sets, so FF4's scene FOV is the part's 30-degree default (`setFOV(0x424, 0xf74)`) and the script's
  `eventCameraSetFovyMove`, and the channel constants disagree with the script on most shots (103's
  channel says 43 degrees, the script 30; shots 105-110 and 115 set nothing and keep 30 and 23 where
  the port switched to 26..43). `Ff4CameraMotion` no longer applies the channel; `SceneStarted()` sets
  the default at `ce_StartEvent`; the script's FOV persists across motions.
- The window's title says which game runs (`GameProfile.Title`).
- Found on the way: the sample `hello` mod's HUD and villager spawn run on FF4 scenes (`--nomods`
  for clean screenshots), and `crystal lz <file> <out>` makes `<out>` a directory when it exists.

### The same night: what the intro still dropped, and a missed departure

**The `Quiet` set is bookkeeping only now** - scene skipping and sound resource management, with
the reason beside each group. It had grown to list handlers that exist (`ce_SetupExpression`,
`ce_SetupCameraMotion`, the `3DS*` family) and visible behaviours that were missing, and two of
the night's bugs hid in it. Two safeguards: the table build warns when a quiet name has a handler,
and `ScriptCommands.ReportDropped` writes one line per scene and per map with everything skipped,
by name and count (`script: FF4 scene e01_01 skipped 8 command(s): ...`), so a missing behaviour is
read off the log of the scene that lacked it. The first report over the whole intro named three:
`ce_setScale` (the hall's `o032` at (2, 2, -2), doubled and mirrored), `ce_PauseAnimation` (its
material animation frozen; `ce_StartAnimation` came along) and `ce_SetBindObject("p02_01b", 4,
"L_wepon", ...)` - a spear in a soldier's left hand. Bind objects (`Ff4Cutscene` `_binds`) are
`pl.BindObject`'s mechanism: the host's joint matrix captured while it draws, the bound model posed
from offset x joint each tick, hidden until the first capture; `ce_SetBindObject2` binds one cast to
another's joint, `ce_BindObjectVisiblity` shows and hides what a cast holds; the rotation offset is
logged, not applied (every shipped call passes zeros). After that the intro's reports list nothing
but quiet bookkeeping.

**A missed departure.** `EngineHost` watches the stage name once per tick; `e01_00`'s battle hands
over to `e01_01`, whose script starts and swaps its stage to `e01_18` (`ce_SetMap`) within the same
tick, so the watcher saw one change with `StageSwapPending` set and never called `MapLeft` for
`e01_00`. The old scene's slots then pointed at the new scene's casts: `Setup(5)` deleted the
soldier just made for slot 4 (both logged as character 6), the spear went to the wrong body, the
camera sets and the scene FOV carried over, and `e01_00`'s dropped commands were reported under
`e01_01`. `ce_StartEvent` now notices a scene state from another stage and releases it first
(`script: FF4 cutscene: e01_00 was left unnoticed ...`); slots 0..5 come out as characters 2..7 and
camera 118's low shot has its soldier. Open: camera 119 (Cecil's "..." and "prepare for landing")
stands at deck level near the starboard rail looking across the deck at the origin, where soldiers
4 and 5 stand, but the frame shows only sky and clouds - `e01_18`'s cloud geometry and its per-shot
visibility animations are the suspect ("the flight's sky" above), not the casts. Also open: the
casts' own discs (Cecil, the soldiers) follow their hips but do not show on the deck.

## Crystal: the mod's own things in the tree, and one mod for both games (2026-09-07)

Karl's report: a project's C# code and scenes were reachable only from the File menu, so the
tree - the thing a user reads the project off - said nothing about them, and the OpenFF path
felt like a separate tool for C# rather than the same editor with more in it. Three tabs
labelled *FF3 our build / FF3 on Steam / FF4 on Steam* had drifted from what they were meant
to be, a filter on whose content shows, into a statement about what kind of mod this is. And a
scene file opened as JSON when it is a map.

- **The OpenFF mod folder** ends the project tree, pinned to the bottom (`tree-foot`, sticky),
  with two rows: **Code** (`code/`, listed by `ModCode.Files` - the csproj and sources, not
  `bin/`/`obj/`) and **Scenes** (`scenes/<map>.json`, `ProjectScenes.Maps` with attachment and
  point counts). Both are libraries like the others: `state.browse` of `code` and `scene`,
  inspected and opened the same way. Code files open in `code-editor.js` - a `<pre>` behind a
  transparent `<textarea>` with the same metrics, as the script editor does, with a C#/JSON/XML
  tokenizer, line numbers, Tab/Shift+Tab, Ctrl+S; the original line ending is kept (the textarea
  normalises to LF, which used to flag a freshly opened file as unsaved). Routes:
  `/api/project/files`, `/api/project/file[/save|/new|/open]`, all through `ModCode.Resolve`,
  which refuses anything outside `code/` and `scenes/`. `dotnet build`'s output is parsed into
  problems (file, line, column, code, message); the strip lists them and a click lands on the
  line. A scene opens the **map editor** on its map (`openScene`: switch to the project's OpenFF
  workspace, clear the map list so it reloads, `openDoc('map', ...)`); *Open as JSON* in the
  inspector is the text. Maps with a scene carry a mark in the Maps library.
- **A target is a game and a kind.** `Targets` gained `oursff4` (FF4 content in an OpenFF mod)
  beside `ours`, and `GameOf`/`KindOf`/`For`; labels read *FF3 in OpenFF*, *FF4 on Steam*. The
  New project and Project settings dialogs are a grid, games down, kinds across (OpenFF mod:
  "plays in the OpenFF client; may use both games; can carry C# code"; Steam mod: "files copied
  into the Steam game with Project ▸ Install"); with both games installed the default is both
  under OpenFF. An OpenFF target is never *installable* (`Session.Installable`), so Install /
  Remove leave the menu for it and `/api/mod/status` says why. With no project open, sessions
  are one per install (the OpenFF and Steam targets of a game read the same folder;
  `Targets.All` puts Steam first so that is the one shown).
- **The tabs are games again.** FF3, FF4 - a filter on whose libraries the panel shows. The
  kind appears as a subtitle only when a game is open twice (an OpenFF and a Steam target).
  A greyed tab (installed, not targeted) opens Project settings.
- **One mod, both games.** Export to OpenFF writes `ff3/files/` and `ff4/files/`, one per OpenFF
  target the project has, and clears an older export's folders first (the pre-07 layout put
  FF3's edits in `files/`). `ModsFolder.Mod.FilesDirectories(game)` yields the game's folder and
  then `files/`; `GameArchive.ModsFolderOverrides` applies them in that order, `Conflicts` takes
  the game so an FF3 name and the same FF4 name never meet. `--project=<dir>` reads the booted
  game's target folder (`GameArchive.ProjectFiles`, from the project's own target list). The
  README names which folder is which game. `ContentChain.GameOf(root)` tells the two apart by
  the mass files. ff3-boot passed with a probe mod carrying both folders, and each game took
  only its own.
- **The tree is resizable**: a fourth splitter (`data-split="tree"`, 110..400px, kept
  in localStorage). The splitters carry `role="separator"` now; `setPointerCapture` is guarded
  and the move/up listeners sit on `document`, so a drag that leaves the bar still ends.
- Open: the tree's Scenes count is the project's, not the current game's. (The Code row does
  not list the `.gitignore` - `Hidden` drops dot-files - as a note here once said.)

### Later the same morning: Add Behaviour as Unity has it, and the cog

Karl's second look: opening a scene by double click threw `Cannot set properties of null
(setting 'problem')` - `inspectAsset` awaited the scene's details while the double click's
open cleared `inspected`, and the catch wrote to null; the details land on a local now and
draw only if it is still the one shown. The Behaviours section's `<select>` of "Name - summary"
rows is an **Add Behaviour** button with a dropdown (`behaviourPicker`: search box, icon +
name rows, summary as tooltip, arrows/Enter/Escape), and a name that matches nothing is a
**New Behaviour "Name"** row that writes the file from the Behaviour starter, attaches it and
opens it. For that the server reads the *source* as well as the build: `ModCode.Sources`
finds `class X : Behaviour|GameService` by regular expression over `code/**/*.cs` (line
comments blanked), so a class is offered the moment it is written and marked *not built* until
`Build` gives it fields; `/api/project/code/catalog` carries `sources`, `/api/status`'s project
a `service` file. The cards lost their summary paragraph (icon, name, open-source, ×). The
header has a cog beside the project's name for the GameService: open, stub, or add the code.
And the tree's highlight follows the list when a document switches it (`runView`).

Also this morning: Crystal's Visual Studio profile still said `editor --content=..\Content
--text=...` from the old repository, so F5 failed on `bin\Debug\Content` before the Steam
lookup ran; the profiles are a plain start, FF4 first, no browser on 5077, and `installs`.

### The session, and a start page that says what is open

Karl, on opening a project: the start page still showed the list of projects, so it read as
"which one did I open?" - and the work was behind it every time. Two changes. The page with a
project open says **Open project** over the name and puts the other projects under a folded
*Switch to another project…* (the open one left out); with no project it is the list as before.
And the project remembers its session: `session.json` beside `project.json` holds the tabs
(previews left out), the focused one, the panel's library and game, and which split group
each tab sat in; `syncHash`, `pinDoc` and `selectKind` note it, debounced 600 ms, through
`/api/project/session` (GET/POST; the server stores what the page sends). `restoreSession`
opens them on start and after a project switch, then the address bar's hash has its say only
when it names something other than what was just focused - a reload carries the focused
document's hash and is not a request. Saves are held while restoring and while
`reloadEverything` closes the old project's tabs, which would otherwise be written as the new
project's empty session. The export does not carry the file.

### The mod's own objects: no script, a model or none, a tree

Karl's point: an OpenFF mod should be able to add game objects that owe the game nothing -
no `.hich` row, no cast, no `.ffs` - with a model or without one (a spot that only holds
logic, for "the hero walked here"), renamed freely, nested under one another, every property
a field in the JSON so a chest's item or its model can change any time. The scene file's
`points` were most of the way there and are generalised into **objects**:

- **Format** (`OpenFF.Engine/Scenes.cs`, `SceneObject`): `{ name, x, y, z, yaw, scale, model,
  tags, children }`, a tree under `"objects"`. Each becomes a `GameObject` named `<map>/<path>`
  (`d01_05/Chest/Trigger`), tagged `scene` plus its own, with `MapObject { Kind = "scene",
  Name, Path, Model, Npc }`; `GameObject.SetParent` holds the tree. `Transform` has no
  hierarchy, so `SceneLoader.Place` works out the world transform - a child's offset turned
  by the parent's yaw and scaled by its scale, yaw added, scale multiplied - and the objects
  carry world values. Attachments target the path (`"chest/trigger"`); the older `points` and
  `point:<name>` still read (`SceneFile.AllObjects`), and Crystal writes the new shape on save.
- **A model** is shown through `Game.Npcs.SpawnModel(model, at, yaw, scale)` - a plain
  character without a cast - kept on `MapObject.Npc` and removed when the object goes.
  `Component` gained an internal `OnDetached` hook for that (a `Behaviour` has `OnDestroy`; a
  plain component had nothing).
- **Objects went with the map now.** Nothing destroyed a scene file's objects when a map was
  left: the next map's file added to them and the old behaviours kept updating. `EngineHost`
  calls `SceneLoader.Clear()` on leaving (after `MapLeaving`), and `ApplyAll` clears first.
- **`Trigger`**, an engine behaviour (`Radius`, `Once`, `HeroInside`, `Entered`/`Left`,
  `Events.TriggerEntered`/`TriggerLeft`, a log line), so a trigger is placeable from the editor
  with no code, and a `GameService` can hear it by the object's tags. `SceneLoader` falls back
  to the engine's behaviours when the mod's code lacks the name; `ModCatalog` lists them too
  (from `OpenFF.Engine.dll` with its XML summaries), whether or not the project has code.
- **Crystal**: `sceneState.objects` is the tree (a non-enumerable `parent` on each);
  `flattenSceneObjects` hands the 3D view a flat list in world terms with `package` for a
  model, and `map-scene.js` draws those as the model (washed blue when chosen, picked by
  bounds) and the rest as the blue box; a gizmo move comes back in world terms and
  `sceneSetWorld` writes it into the object as local ones, so children follow. Hierarchy:
  *Objects (OpenFF)* as an indented tree (`depth` on rows). Inspector (`buildSceneObject`):
  model picker or *No model*, name (attachments retarget on rename, on it and under it),
  x/y/z/yaw/scale/tags, a parent dropdown (reparenting keeps the world place), children with
  *Add a child object*, delete with children, behaviours. *Add a game object* offers *OpenFF
  object* first on an OpenFF project (name, model or none, spot). Verified: a chest (`o001`)
  with a child trigger 6 ahead, moved and turned (the child landed 6 along +x at yaw 90),
  renamed, saved as `objects`, exported; the client placed the model ("placed model o001 as
  character 3"), made 4 objects, and the trigger logged "hero entered". ff3-boot passes.
- Open: reparenting by drag in the hierarchy (the dropdown does it for now); a child with a
  model of its own works but has no separate pick priority over its parent's model.

### The inspector as Unity's, components built in, and no Save buttons

Karl's next look: Transform belongs at the top, the "No model" button is a × on the picker,
scripts should expose editor fields with headings and the like, a chest needs a way to say
what it gives, saving should be automatic, common things should be components (built in but
overridable), and the map's facts at the top of the panel are in the way.

- **Layout** (`buildSceneObject`): the name in a box with the object's icon, then cards -
  Transform (Position X Y Z, Rotation Y, Scale; "relative to <parent>" for a child), Model
  (the picker with a × beside it), Tags and Parent - then Behaviours, Children, Delete. The
  document's own facts (`factsFor`, materials) fold under *About <name>* at the bottom of the
  inspector whenever something is selected (`drawInspector`); with nothing selected they are
  the panel as before.
- **Autosave** (`sceneChanged`): every edit to the scene state - a field, a move, an attach,
  a rename - writes `scenes/<map>.json` 700 ms later, quietly (the status line, no redraw:
  a redraw under a half-typed field would take the field away). The Save buttons are gone.
- **Editor fields** (`OpenFF.Engine/Inspector.cs`): `[Header]`, `[Tooltip]`, `[Range]`,
  `[ItemField]`, `[HideInInspector]`. `ModCatalog.Decorate` reads them by attribute type
  name (the types live in the catalog's own load context), fields come base class first in
  declaration order, `string[]`/`List<string>` classify as `strings`; the card draws headings,
  tooltips (the XML summary when there is none), a slider with its number, the item list
  (`/api/items`, fetched once) and a textarea of lines.
- **Built-in components** (`Scenes.cs`): `Interactable` (abstract: `Radius`; the object's Npc's
  `Interacted` when it has a model, the hero walking within Radius when not; `Activate`),
  `Chest : Interactable` (Item, Count, Gil; Once, Message, EmptyMessage; `OnOpened` hook;
  gives through `Game.Party.AddItem` / `.Gil`, says "Found {what}!"), `Talk : Interactable`
  (Speaker, Lines, FaceHero; one window per line, the next when the last has closed and a
  frame has passed; `OnSaid`). Neither is sealed: a mod's class deriving them is a component
  with theirs plus its own. `SceneMemory : ISaveable` ("openff/scene", a list of keys) is
  what `Chest.Once` remembers by, registered at `Game.Start`. The picker heads its list with
  *Built in*, then the mod's assembly. Verified from the editor with no code in play: a Chest
  on an object, Item Potion (5001) x2 and 100 gil, exported; the client said "Found Potion x2
  and 100 gil!" and logged it.
- Found on the way: a `crystal.exe` with no readable command line (Visual Studio's, most
  likely) held `bin\Debug\net8.0`, so the test build went to `bin\Debug\net8.0-test`; the
  wwwroot copy that build serves is the same files.

### The hierarchy's right click and drag

Karl asked for the tree to be worked in the tree. `drawHierarchy` takes three more things
on an outline node: `menu()` (items for `showContextMenu`, a `.dropdown` at the pointer,
closed by the next click or Escape; the right click selects the row first), `drag` (a key
the row carries) and `drop(key)` (on rows and on group headers; `wireDrop` lights the target
while a drag is over it). The scene objects use all three: `sceneObjectMenu` - New child
object, Duplicate (`duplicateSceneObject`: the subtree through `objectsForFile` and back,
a fresh sibling name, the attachments on it and under it copied to the new paths), Rename
(the caret into the inspector's name box), Focus in view, Move to top level, Delete; the
group's menu has New object; dropping a row on another calls `reparentSceneObject`, on the
group with `null`, so the object keeps its world place either way. The inspector lost its
"Add a child object" button; the children stay listed as links. Open: the game's own rows
(characters, exits) have no menu yet - "Add Behaviour…" there would be the natural one.
Also from Karl: *New object* on a row's menu makes a sibling right below it, at its spot.

### One inspector shape for the game's things and the mod's

Karl: the workflow must not change between a Steam mod's objects and an OpenFF mod's. Three
helpers now shape every map inspector - `inspectorHead` (icon and name, an input that is
read-only for the game's things), `transformCard` (Position X Y Z, Rotation Y, Scale when
there is one; `read`/`write` callbacks, focus/blur for an undo record), `componentCard` - and
`cardify` folds a panel written as h3 headings and fields into cards, so the game's panels
(characters, exits, terrain) kept their content and gained the shape: a character is head,
Transform (its `.hich` position and facing, typed numbers moving it in the view as before),
Model, Cast, What it says, Behaviour, Referred to by, Behaviours; an exit is head, Transform
(the arrival spot, noted "arrival"), Leads to, Doorway, Conditions, Referred to by; the
terrain head, Model, Behaviours. `buildSceneObject` uses the same helpers. `inspectRef`
applies `cardify` to all four.

### The game's chests edited and converted; the start page, dialogs and fields

Karl's afternoon list. **Treasure**: a game chest is one line in the map's boot cast
(`setTreasureItem(cast, item, group, index, 0, 0)`, `setTreasureMoney` with gil), so the
character's inspector gets a Treasure card when the script has one for its cast
(`treasureOf` over `/api/script`'s source): item from the game's list or gil; *Save treasure*
rewrites that one call (`withTreasure`) and goes through `/api/script/save` - the same
compile the script editor uses - then marks the override. **Convert to OpenFF object**: the
`.hich` row and the cast stay (other things name them), so the conversion is two things in
the mod - `addSceneObject` at the character's spot with its model, yaw negated (a `.hich`
facing is negated by the game; `facingSign`), a `Chest` with the parsed contents when it was
one - and a new engine behaviour `Removed` on `object:N`, which calls `Npc.Remove()` (or
`Hidden` with HideOnly) at Start; the client logged "removed: the map's object:5 on d01_05"
beside the placed stand-in. The character's hierarchy row reads *replaced*, greyed. **Start
page**: a × (sessionStorage, File ▸ Start page back), project rows carry `projectKindIcon`
(the `mod` folder, a new `steam` game pad, both for both) here and in Open project. **Dialogs**
close with `.shut` × like the model picker. **Fields**: one base rule for `input`/`textarea`
(dark, bordered, accent on focus) so a dialog's boxes stop being the browser's white ones.

### The list of improvements, done (2026-09-07, evening)

Asked what else the editor and engine wanted, then told to do all of it:

- **Transform has a hierarchy** (`Objects.cs`). `Position`/`Rotation`/`Scale` stay public
  fields - so `Transform.Rotation.Y = ...` in the samples still compiles - and mean the local
  values (the world's at the top); `WorldPosition`, `WorldYaw`, `WorldScale` resolve through
  `GameObject.Parent` (yaw only, as the field and the files do) and are settable, working the
  local value out. `SetParent(parent, keepWorld = true)` keeps the world place. `SceneLoader.
  Place` sets local values and parents with `keepWorld: false`; a spawned model follows its
  transform through an internal `ModelFollow` behaviour (Teleport/Face/Scale when the world
  values change). C-41: the child trigger fired on its world spot in the client.
- **`SceneObjects`**: `Find(path)`, `All()`, `Defined`, `Spawn(path, at, yaw, parent)` from the
  definitions the loader remembers per map (`Remember`), named `<map>/<path>#N`, with the
  file's attachments through `SceneLoader.Attach` (the attachment loop factored out);
  `Destroy`. **`ObjectRef`** as a field type (a path; `Resolve`, `Link`), read from JSON as a
  string, classified `object` by the catalog and picked from the map's objects in the
  inspector, renamed along with the object. **`SavedBehaviour`**: `ISaveable` registered at
  Awake, dropped at OnDestroy, chunk `<mod>/<object>/<type>`. **`Interactable.OnWalkIn`**: off,
  a modelless object waits for A within Radius.
- **Editor**: undo for the scene (`recordSceneStep` in `sceneChanged`: snapshots before and
  after on the map document's stack, coalesced within a second, `restoreSceneSnapshot` redraws
  and autosaves); *play here* (`/api/project/run` takes `map` and `pos`, `OpenFFClient.Launch`
  takes arguments: `--game --map --pos`); a model dropped on the 3D view of an OpenFF project is
  an object there and then (Shift for the game's way); right-click menus on the game's rows
  (characters: Add Behaviour…, Focus, Open script at cast, Convert; exits: Focus, Open map;
  terrain: the model, a new object); the conversion says when the script still uses the cast
  beyond boot and treasure; `#inspector-panel` has a minimum width and the transform cells wrap.
- **Housekeeping**: Crystal's home is `%LocalAppData%\OpenFF\Crystal` (`CrystalHome`: projects,
  mods; the old `FF3ContentTool` moved on first start - done on this machine: "moved ... ->
  ...OpenFF\Crystal"); the client's `--project=<name>` looks in both. ~90 members got XML
  summaries (GameObject, Scene, Component, the vectors, Npc, IHero, IParty, InputState,
  LoadedMod, the scene classes). README says where the projects are and why a running
  `crystal.exe` blocks the editor's build.

### Converting a map's characters, stage one (2026-09-07, evening)

Karl asked whether a whole map could be the mod's. Stage one of the plan laid out for it:
everything a cast does that the built-in components can stand in for. `MapConvert.Plan`
reads the map's characters (`MapModel`) and its script's disassembly (as `References.
MessagesOf` does) and decides per character: logic rows are skipped; `BootedBy` walks every
cast's reachable code for boot commands (`CastArgs.Positions`), and a character not booted by
the map's boot cast (the lowest-numbered) stays the game's - a scene's actor appears when the
scene says; a `setTreasureItem/Money` naming the cast is a Chest; a cast with no main is a
prop; otherwise `Walk` follows every path from the cast's main - `flagOnJump`/`flagOffJump`
split into taken and not taken with the flag on the path's condition, `flagOn/Off` go to its
Then, `startMessage2` ids resolve through the message lookup (" / " to a line break), library-0
calls are followed, library-2 calls other than talkBegin/talkEnd and any command outside the
talk set make the cast unknown with the names. `BootConditions` walks the boot cast the same
way and keeps, per booted cast, the flags every booting path agrees on (`flagOnEnd` counts as
a guard) - Ur's villagers are there while `!0:439`, Arc (j201) while `!0:14`. The engine grew
`Talk.When/Then` (Interactable picks the first component whose `Applies()` holds), `Wander`,
`WhenFlags`, `SceneObject.Character` (spawned with `Npcs.Spawn`, the walker, rather than
`SpawnModel`), and `Removed.StandIn`: the loader spawns a stand-in and then removes its
original, pair by pair - the first run removed everything first and eight stand-ins failed to
spawn (a model only the removed character used goes with it; and the slots were still held).
Recolours (`changeColorCharacter` to n024/n073) are not models the map loads, so the row's own
model stands. On Ur: 14 of 36 converted, the village stands in the client with no failures.
Stage two (flag-gated talk is in; shops, inns, item hand-ins remain) and stage three (a
`Sequence` component for the scenes' casts) are the open items.

### Exact conversion: the stand-in is the cast (2026-09-07, evening)

Karl: if it is not 1:1 nobody will convert. The components approximated the game's handlers
(idle motions missing, a wanderer talking on the move, a chest without the lid and sound,
recolours lost); the way to 1:1 is not to approximate at all. `bootCharacterImp` is what
makes a game character its cast: `LogicIndex_set(cast)` (talking runs `cast<N>_main`) and
`CHichParameterManager.setCharaIndex(row, slot)` (every `changeHichNumber(cast)` in a command
resolves to that slot). `Npc.RunCast(cast)` does both on a stand-in, so the same bytecode
runs on it - the old man's item hand-in, the shop calls, the branches, the placeholders
(`%shuyaku1%` came out as "Luneth" on the mod's object). `Npc.SetTreasure` does what
`ff3Command_SetTreasureItem` does to the map object (flag, item or gold, the arrow count of
20, shut lid or open), so the chest opens the game's own way; `Npc.Recolour` is
`changeColorCharacter`'s `bindReplaceTex(model_variant)`. `GameCast` carries the three; the
converter's exact mode puts it on every stand-in (a chest spawned as a character, so it is a
map object with its lid motions), with `Motion` (the boot's `bindMotion` + `startMotion
Character`), `Wander`, `WhenFlags`. Two more things the client taught: a stand-in whose
WhenFlags do not hold must not be spawned at all (its model is not loaded - the game did not
boot it either), so the loader gates the spawn and `WhenFlags` spawns it when the flags come
true, with `INeedsNpc` letting GameCast/Motion/Wander/Interactable take the character then;
and the components mode is kept for editing (`Chest` now with the game's lid motions, sound
36, sparkle 102 and its flag; `Talk` stopping a wanderer for the talk). Ur, exact: 16 of 36
(the 20 left are the scenes' actors), 14 spawned at once, no warnings, the talk verified in
play. Open: the scenes' actors (stage three), and the recolour needs the character spawned
before `bindReplaceTex` - it is, but a variant texture the map has not loaded is untested.

### The scenes' actors, and a row is not a slot (2026-09-07, later)

Stage three turned out not to need a `Sequence` component: with the stand-in being its
cast, a scene's actor only has to *arrive* when the scene boots it. `bootCharacterImp` -
every boot path ends there - now tells the host (`EngineApi.CastBooted`, a one-line hook
guarded against the engine being down), which publishes `Events.CastBooted` with the
character it made. `GameCast.OnBoot` marks an actor: the loader does not spawn it with the
map; on the event it spawns the stand-in where the script put the original, facing as it
does, removes the original and runs the cast - the scene's next commands land on the
stand-in. A scene that opens with the map (the Altar Cave's falling Luneth, casts 40/36/35
booted at 1.35 s, the files applied at 2.36 s) has booted before the files apply, so the
loader takes over on the spot when the original is already there. A new game then plays
the opening on the mod's Luneth, crystal and chest, frame for frame the same.

The check of the slots found a real bug in the conversion so far: a scene file's
`object:<n>` is the map's `.hich` *row* (what Crystal lists), but the client resolved it as
a player *slot* (`Npcs.Existing(n)`), and those coincide only by luck - in the Altar Cave
the chests' rows 5-7 are slots 28-30. `Removed` had been taking off whichever character sat
in slot n, and the originals stayed behind their stand-ins. `Npcs.ByRow` goes through the
hich table (`CharaIndex(row)`), and the log now shows each stand-in spawned and then the
right row removed, the freed slot reused by the next. Also fixed on the way: two map tabs
open shared one `mapState`/`sceneState`, so a conversion could act on the other tab's map -
`onShow` re-points them (writing a pending autosave first). Ur converts 30 of 36 now, the 6
left the rows nothing boots.

### The name screen, and drives that can touch (2026-09-07, night)

Karl asked whether Luneth's name entry works - the new-game drive above had sat on it. It
does: the screen is touch-only in the original (`NameEntry.execute` reads `g_TouchPanel`;
A does nothing there), so a drive pressing Z could not leave it. Drives can touch now:
`tap <x> <y> [ms]` sends a touch down/up at a point of the 800x480 view through
`DesktopInput.InjectTouch`, the same `Send` a click goes through after `MapToViewSpace`;
`type <text>` puts characters into the open `TextEntry` (bare `type` clears it), `submit` /
`cancel` are its Enter and Escape (the field reads the real keyboard, so `press Enter` would
not reach it). `Docs/Drives/ff3-name.drive` taps the name, types Karl, confirms, and the
first battle's row reads "Karl 32 / 32" (C-51). The 2D plane's touch rectangles map to the
view as x * 800/480, y * 480/320.

Two things seen on the way, not fixed: `--pos` on a town (`t01_01`) is overridden by the
map's own entry placement, so a drive cannot start beside a character - walking blind from
the entrance is the only way, and the chests are out of reach that way (the components-mode
Chest/Talk are therefore verified by their fields and the log, not by a walk-up); and
`ff3-boot.drive` on `d01_05` spends its 40 s in the opening scene and the name screen rather
than walking and opening the menu as its comment says - it still catches exceptions, which
is what it is for.

### The wanderers' gait, what a GameCast says, approach then interact (2026-09-07, late)

Karl's three questions. Do the converted wander as before? Not quite: `Wander` set the AI
kind only, where `moveCharacter_StartRandom` also clears autopilot and operator and sets the
`NPC_RANDOM_MOVE_TYPE` from its second operand - the gait (man, woman, boy, girl, uncle,
aunt, old man, old woman: the walk's pattern and pace). `Npc.StartWander(gait)` /
`EndWander()` are those two commands exactly, `Wander.Gait` carries the operand, the
converter reads it (Ur's are all 0), and `Talk` pauses and resumes through the same calls.
Standing in Ur for 16 s with the mod: the girl and the woman have moved, the old man has not
- as without it. Where do a GameCast's lines come from? From the cast's own code in the map
script, at talk time - that is the point; the card was hiding it, so it now shows what the
cast does (the script's lines, read by `MapModel`), opens the script at `castN_main:`, and
offers *Make it editable*, which swaps in the analysis's Talk/Chest. Approach then
interact: with a model that was always so (the game's own talk: face it, press A); without
one, `Interactable.OnWalkIn` defaulted to on, so an invisible Talk spoke when walked into.
It defaults to off now - A within Radius - and walk-in is the option.

### Every boot command, replayed (2026-09-07, night)

Karl: cover everything. The survey first: every FF3 map disassembled (`crystal script`),
every command in each boot cast whose first operand is one of the map's casts, counted -
1023 bootCharacter, 400 setTreasureItem, 168 moveCharacter_StartRandom, 160
changeColorCharacter, 120 setCharacterDetectionRadius, 120 setCharacter_CheckTurnType, 104
startMotionCharacter, 75 bindMotion, 64 setSignEffect (the sparkle over an unopened chest),
59 bootPlainCharacter (the model named in the command), 49 setCharacter_TalkMotion, the
collision radii, shadow alpha, wall collision, 12 bootCharacter_AbsoluteCoordination (the
spot named in the command, not the .hich's), moveCharacter_AbsoluteCoordination, scale,
alpha, face data, setCharacter_ItemEvent... 31 distinct setup commands. The converter knew
five. Translating each into a component field would be 26 more approximations, so it does
none: `GameCast.Setup` is the boot's own lines, and the client replays them through the
game's own script engine - `ScriptSnippet` encodes a line against the FF3 command table
(Shared/Script's operand kinds; little-endian words and dwords, NUL-ended strings) and
`ScriptEngine.runSnippet` dispatches it on a spare engine, after `RunCast` has pointed the
cast at the stand-in. `Npc.RunScript(line)` is the API. Crystal's `MapConvert.BootLines`
walks the boot cast down every path (calls included, conditional calls under their flag),
collects each command by the cast it acts on (`CastArgs`' first cast operand), and
`Condition` turns the paths a line was on into one flag expression - contradictory paths
dropped, alternatives differing in one flag's sense merged, implied ones removed, `|`
between what is left (`WhenFlags.Holds` reads `|` now). Ur's cast 30 reads "!0:11 !0:439
0:14 | !0:14 !0:439", which is what the script says. `WhenFlags.Live` off makes the flags
count once, as the boot's test did. Every map's plan answers; the setup commands across all
355 are exactly the survey's 31. In play: 37 lines replayed on Ur's 14 stand-ins, no
warnings. The test dll: the editor was built to a temp folder while Karl's Crystal ran.
Also, at Karl's ask: the replaced characters are hidden in the hierarchy - the Characters
group counts what is still the game's, a *replaced (N)* tick on the header lists them,
off unless asked and remembered.

### The loose ends, in play (2026-09-08, morning)

`--pos` on a town: `wld.MapJump`'s entry placement takes the arrival from the map's jump
table whenever there is one, so the jump part's position only counted where there was
none. When the previous part is the jump part and `--pos` was given, it counts now; a
map's own exits still arrive where the table says. That made every walk-up test possible.
Exact mode: the boy (cast 23) says his script's line, the west chest "You find Potion." -
the game's code on the stand-ins. Components: *Make it editable* on the boy, his line
changed in the inspector, and the Talk says the new words in play; the Chest gives its
Potion - and its lid motions had been going nowhere: `Npc.PlayMotion` asked the walkers'
motion sets, but a chest is a `map.CMapObject` with motions of its own (`startMotion`), so
the map-object slots route there now. The Chest's default message is the game's wording
("You find {what}."). The scene path proper - a boot *after* the files applied - with a new
drive verb `flag g:i on|off` and `--pos=150,0,-80` inside Ur's opening-scene trigger box:
casts 33 and 35-39 "booted into slot", each "took over cast N as the script booted it",
and the scene played on the mod's actors.

One more thing the survey had not shown, because it counts commands and not what they do:
`bootPlainCharacter` is not `bootCharacter` with a model name. It goes through
`setupPlainCharacter` - the light walker - with a scale table by model (n031/n041 children
0.8, n551 1.3, n431 the frog 0.3, n221 the fairy 0.4...) and a human type (chocobo, frog,
sheep, fairy). Twelve of Ur's villagers are booted that way, and their stand-ins had been
full walkers at 1.0 - the girl a head too tall. The command's body is split into
`bootPlainCharacterImp` (the same code, the .hich row's index left to the caller), the
CastBooted hook is on it too (Arc in cast 6 is booted plain), `Npcs.SpawnPlain` calls it,
`SceneObject.Plain` says which, the converter sets it, and the Model card has the tick.
Side by side with the original the children are the same size.

Editor: the dialog's *Undo this conversion*, *Restore the game's character* on a replaced
row (the Removed and the stand-in go), and `[FlagField]`: flag strings get a picker of the
map's flags (`/api/map/flags`: who tests, who sets, whose chest) and a shape check.

Karl's first Play from Ur after this: no colliders, no talk, everyone shrunk, walking over
the houses. All one thing - *Play here* with nothing selected sent the 3D view's camera
target as the spot, and its Y drifts up as the camera flies; with `--pos` honoured on towns
now, the hero stood 60 units in the air, closer to the camera than the village (so bigger),
above every wall and character. `scene3d.viewGround()` is the ground under the view's
centre pixel (the height the map's things stand at), and Play here and a new object at the
view's centre use it.

Two inspector asks: the Add dialog's "no model" checkbox (misaligned, and a second way of
saying what the inspector's × says) is gone - the model row is the picker with an × as in
the inspector; and Tags are Unity's way - chips with an × each, a picker of the tags the
project's scene files already use (`/api/project/tags`), New tag…, and Edit tags…, which
renames or removes a tag across every object on the map.

### A chest of the mod's own, opened as the game opens one (2026-09-08)

Karl's chest with a Chest component played no lid, made no sound, stayed shut, and said
"It's locked." Three things. The object was not ticked *character*, so the model went the
plain-figure way (`SpawnModel`) and had no map-object motions to play: an o/w model is one
of the game's map objects whichever way now (`SceneLoader.IsObjectModel`) - that is where a
lid comes from. `Npc.MotionDone` asked the walkers' bookkeeping for a map object; it asks
the box (`isEndOfMotion`) now, so the open lid (1002) follows the opening (1001). And the
game had opened it too: o001 is `TREASURE_BOX` to the game by its number, so
`CPlayerHumanCheck` ran the game's own opening beside ours - flag 0:0, no treasure set,
"It's locked." (1000140, the "nothing" line). `Npc.OwnChest()` makes the box a plain object
to the game (`MAP_OBJECT_TYPE_ERR`, also for o000 `INVISIBLE`), the Chest calls it when its
character is ready, and the talk reaches Interacted the characters' way.

Messages: `Dialogue.Say("@1000142 item=5001 color=9")` says one of the game's lines by its
.msd id through `createMessage`, with the item's and the gold's control codes set as
`CMapObject` sets them, and the window's colour (9, the chests' gold). The Chest's Message
is `@` by default - the game's own words for the contents in the player's language: a box
says 1000142 "The chest contained Potion.", an item spot (o000, or no model) 1000146 "You
find Potion.", gold 1000141, nothing 1000140 - and EmptyMessage is empty, as the game's
opened chests say nothing. `@<id>` works in every string field (Talk's Lines too); the
inspector shows what the id says under the field (`/api/messages`).

Two more from Karl's chest. "It rotates towards the player": it did not turn - it stood
mirrored. `YawToRot` carried the scripts' negation (`4096 * FX_DEG_TO_IDX(deg) * -1`), so a
scene object at yaw 90 faced -x in play while Crystal drew it (and `Direction(90)`,
`Face(90)`, an exit's arrival facing) toward +x; `Yaw` and `Face(Yaw)` disagreed on the
host too. Four chests at 0/90/180/270 beside the hero (`--rot=90`, who faces +x) showed
it. The word is the engine's convention now, no negation, `RotToYaw` its inverse; the
converter already negated a .hich posture once ("the mod's yaw is the engine's own"), so
its stand-ins come out facing as the originals, and the exact path's `Npc.Yaw` round-trips.
"The chests open and close as the scene loads": the model's own pose is the open lid, and
1003 (shut) is a motion *to* it - the game's chests play it behind the boot's fade-in, a
scene object appears on a map in view. `Npc.HoldMotion(index)`: the motion at its last
frame, no blend (`setCurrentFrame(getMaxFrame())`); the Chest holds 1003 when it appears,
`SetTreasure` does the same for a stand-in and moves the box to act 2 (shut, waiting).
`ChestLook` is `Animate` - the old name read as "look at".

### The mod's tags (2026-09-08)

Tags had one home: the objects that carry them, with the picker offering whatever the
project's scene files happened to use. Karl wants a vocabulary shared by every scene of the
mod, Unity's Tags & Layers. `project.json` has a `tags` list now; the picker shows *Mod
tags* first (offered everywhere, carried or not) and *On the maps* after; New tag… asks
whether the tag is mod-wide (default yes); Edit tags has the mod's list - add, rename, ×,
each rename or removal applied to every scene file of the project by the server
(`ProjectScenes.Retag`, `/api/project/tags/retag`) and to the open map's objects at once -
and the map's own tags below, with ↑ to promote one. A tag on one map only still lives in
that map's file; nothing in the engine changes - `WithTag` is a string either way.

### The parity harness (2026-09-08)

Karl's direction: the unified path - scripts and scenes of both games as the engine's own,
assets mixed - with FF3 1:1 on the Steam assets as the standing condition, FF4 to follow,
and Crystal always offering the native Steam games for modding as it does. Before any
translator of the script vocabulary, the thing every step gets measured against:
`--trace=<file>` writes what the game did - flag changes, messages by id with their
item/gold codes (`menu.MessageWindow.mwSetMessage`), texts, sounds (`MtxSENDS_Play`), maps,
battles, items - and at every `say` of a drive everyone on the map (cast or model, position
to the unit, motion; a wanderer as "wandering", since where the random walk took it is
not the fact) and the party's gil and bag. `Tools/parity.ps1` runs a drive with `--nomods`
and with the mods folder and diffs the traces with the frame column off (`-Ignore` for the
mod's own objects, `-Keep`, exit 1 on a difference). One line per hook, after the game's
own call; nothing changes for FF3.

Ur, converted exactly, against itself - three drives (`Docs/Drives/parity-ur-*.drive`):
the west chest, the boy, the opening scene. Both bugs it found on the first run were slot
reuse. A stand-in spawns into the slot the original just left, and the game's `terminate`
leaves the slot's `NPCAiManager` as it was, so a wanderer's RANDOM_MOVE carried over to
three characters that stood still (`FreshSlot`: the DEFAULT AI, as `initialize` gives a
slot at a map's start). And the scene: it deletes the villagers and boots its actors into
their slots; `Npcs.Existing(slot)` handed back the villager's dead handle, whose position
read 0,0,0 and whose Remove did nothing - the actors were taken over "at 0,0,0" and the
game's own stood beside them, and the cutscene did not play (Karl saw it: "first one played
a cutscene, second didn't"). A dead handle is dropped now, and the engine's own spawns pin
the character id they were made for, so a slot reused under them is not theirs. All three
drives agree - 58, 77 and 189 lines, the scene over 13 marks.

### Items as definitions (2026-09-08)

The first of the data objects Karl asked for - "I have no idea how to create an item".
`defs/items/<id>.json` in a mod: a base item (whose record it starts from), a name, a
caption, prices, and any field of the record by the game's own name; a `number` from 20001
that Crystal gives once and the game then knows the item by. Composition happens where the
game reads its tables: `ContentChain.AddTransform` is a step on a file's bytes after they
are found, and `ModItemsLayer` registers one for `item_parameter.pak` (every definition's
record - the base's bytes with the id, the text ids, the prices and the fields written in -
appended to its chain, the pak laid out again) and one for `eureka_item.msd` (a name and a
caption message per definition, ids from 31001 and 32001, past the file's own 30217 and
within a record's signed-short name id). Nothing downstream knows: `ItemManager` counts the
records, the menus and the chest's `%unfixed_item%` resolve the names, `Game.Items` reads
the manager. No definitions, no transform; `--nomods` skips them too, so the parity
harness's game-as-shipped run is that. Field layouts live in `Shared/Data/ModItems.cs`
from the `itm.*Parameter.parse` methods; the editor's generated `PakRecords` has the same
and could feed it later.

Crystal: *Items* under the OpenFF mod folder, the list by name with "number · chain from
base", *New item…* (name + base picker over the game's 409), the inspector as the editor -
Item, Shop, Record cards, the base's values as placeholders, autosave - Open as JSON,
Delete; `/api/items` lists the mod's after the game's, "(mod)", so a Chest's Item picks
one; Export copies `defs/`. A Hi-Potion+ (usedPower 999) from a chest in Ur: "The chest
contained Hi-Potion+.", `20001x2` in the bag. Karl: "it said hi-potion+ so it worked :)".

Left for the definitions line: characters (the job/class seam), and the Steam-mod path -
the same composer writing the two files into a Steam project's `files/` at Install, so a
native mod gets new items too.

### Items the native way; the heroes as definitions (2026-09-08, afternoon)

The Steam-mod path for items: `ProjectItems.WriteTables` composes the shipped
`item_parameter.pak` and `eureka_item.msd` with the project's definitions and writes them
into the target's `files/` at Install and before the zip, so the Steam game reads them as
any replaced file; the composer skips a number the pak already has, so a Steam project's
composed files read again by the client's transform add nothing twice, and `/api/items`
lists such an item once. A probe project with a definition based on 6001 came out in the
*magic* chain - 6001 is Minne, a song: FF3's ids are weapons 1000-2309, armour 3001-3331,
magic and songs 4001-6660, consumables 5001-5122, key items 5201-5241 (the earlier note had
FF4's ranges). Corrected in `ModItems.cs` and Modding.md.

Characters, the first slice: `defs/characters/<id>.json` - a hero slot (0..3), a name, a
starting job (by `JOB_TYPE` word, English name or number) and level.
`ModCharactersLayer.ApplyToNewParty` runs where the game builds its party - after
`addPlayer(0)` at boot (`GlobalScope.sys`) and after the title's New Game sets slot 0 back
to Freelancer (`GlobalScope.ttl`) - `setName`, `changeJob`, `setExp(level-1)` then
`levelUp(0)` so the growth tables do the climbing, `updateParameter`. The field model
follows the job (`j109` for a Knight in slot 0). A save carries its own; `--nomods` is the
game's. Trace: the party line names the heroes with level and job now (`heroes Karl L7 job
8`), so parity sees them too. Crystal: *Characters* under the mod folder, New character…,
the inspector as the form (slot, job, level), delete, Open as JSON.

What the party code showed for the seam: `GameProfile.Ff3Party` already switches the shared
`pl.Player` between FF3's job growth and FF4's fixed party (`updateParameter` returns for
FF4; `initialize` changes to SUPPINN only for FF3), the heroes' names are four literals per
language in `pl.getPlayerInitialName`, and the field model is `j<slot+1><job+1>` in
`bootCharacterImp`. A fixed-class character in FF3's engine is a job that cannot change plus
a model of its own; that gate and the model hook are where the next slice goes.

Both landed the same afternoon. `fixedJob`: the job menu's confirm (`CWMenuJob`, case 0)
asks `ModCharactersLayer.JobFixed(slot)` beside the game's own "is the job won" test and
beeps the same way - the only place a job changes outside the title's reset. `look`: every
site that names a hero's model - `bootCharacterImp`, `setupHero`, the event boot as top
player, the battle's three and its `_stone`, the shop, the job menu's two - builds
`j<set+1><job+1>`, and the set now comes from `ModelSet(playerId)` (the definition's look or
the slot). Every job has a figure in every set, so a look is safe everywhere a job is; a
model of the character's own is not - an NPC model has no job figures and no battle
motions - and stays out until there is something to stand in with. Slot 0 as a Knight in
Refia's figures: `j309`. Boot drive and the chest parity agree.

A note on method, for the next time: the eleven sprintf sites were edited with a PowerShell
table of replacements, and PowerShell's comma binds tighter than `+`, so every "pair" was a
flat list and `Replace` ran on single characters - two files were shredded and restored from
git before the build saw them. Code edits go through the editor's replace tool, one at a
time, and `git diff` is read before `dotnet build`.

### The casts' code as the engine's own (2026-09-08, afternoon)

The script translator, first form - and the form that may stay. The `.ffs` language is
already a language with a compiler (`Crystal.Editor/Ffs`: lexer, parser, two-pass compiler
that round-trips every shipped script byte for byte), so a cast's main function as *text*
is the action list Karl asked for: one line per command, labels for the branches, the
disassembly's comments carrying the lines' words. The compiler moved to
`Shared/Script/Ffs` (with `ScriptOpTable.Names`; `SourceWriter`, which reads the editor's
decoded types, stayed), so the client has it too.

`CastScript : Interactable` holds `Cast` and `Main` (string[]). `Game.Scripts`
(`IScripts`, `LegacyScripts` on the host) compiles every defined cast of a map into one
`.script` of the mod's - `map 60000;`, a cast entry each, `castN_main:` and its lines -
`ScriptData.cast(bytes)`, and registers it with the game's `LogicManager` beside the map's
(the manager holds two scripts now, `CEventManager` PORT: `new ScriptData[2]`, `init(2u,
…)`); `Start(cast)` is the manager's own `startLogic(60000, cast)`, and the game's logic
loop runs it - the same interpreter, every command the game's. The object's `GameCast`
binds the row without the logic (`Npc.BindCast`: `setCharaIndex`, `LogicIndex`
INVALID) so the game's talk starts nothing and the component's `Activate` does. A cast
whose main jumps outside itself or calls into the map's script (`call(1, …)`) is refused
with the reason; the library calls (`call(2, hash)`) resolve everywhere. Crystal: *Convert
to OpenFF scripts* (`MapConvert.Mains`, the source sliced from `castN_main:` to the next
function's label; `CastPlan.Main`/`MainProblem`), a CastScript beside each GameCast for the
talkers - 14 of Ur's; chests and the scene's actors stay GameCast alone.

Parity, Ur converted this way: the boy, the elder both ways (`flag 0:11 on` for the item
menu; SE 0/2 on its cancel), the chest, the opening scene - all agree. So the whole town's
talk is the mod's code now, read and changed in the inspector, and the game cannot tell.

Karl watched the elder run: the item-use menu with nothing in the bag draws a crosshair -
a line across the screen and one down its middle - over the empty list, and the game's own
run (`--nomods`) draws it the same. `menu.BasicWindow.SetBar` makes the `m015_bar` sprite
(cell 1) and never positions it, so it sits at the sprite's default place; whether the
original positioned it through something the port lost, or the phone's item-use window
never had a visible bar, is the open question. FF3 1:1: an open item, not touched today.

### Code and words for objects of the mod's own (2026-09-08, evening)

CastScript was for converted casts: the code names a number the map's `.hich` knows. A
villager the modder placed has no number and no row, so its `startMotionCharacter(N, …)`
had nobody to reach. Now `Cast` 0 means "mine": `IScripts.Allocate` gives a number from
5000 on each map (the map's script never has those; `Fresh` resets the count), and
`LegacyNpc.TakeCast` finding no row for the number claims the first row past the map's
`m_NumMan` (`m_Kind` ERR → CAST, `m_Id`, `m_CharaName`, `setCharaIndex`) - the table has 48,
a town uses 20-odd, and `CHichParameterManager.initialize` clears every row at the next
map, so nothing leaks. `getManCastIndex` scans all 48, so every command that names the cast
finds the object; `startAllMapLogic` looks for MAP_LOGIC rows only and skips it. `@me` in
the lines is the number (`LegacyScripts.Define` substitutes before compiling). Tried: an
n021 at Ur's crossroads with `bindMotion(@me, …)`, a bow (1002) while the window is up, back
to 1001 after - "n021 (character 13) is bound to cast 5000 (row 41)", the trace shows the
motions change.

Its words: `defs/text/<name>.json`, message id → line, ids from 40000000 (`Shared/Data/
ModText.cs`). The client appends every mod's lines to `eureka_permanent.msd` as it is read
(`ModItemsLayer.RegisterText`, the same content-chain transform as the items) - PERMANENT
is the file every `mwSetMessage` falls back to after the map's own, so `startMessage2(0,
40000001, 0, 0)`, a Talk's `@40000001`, `Game.Say("@…")` all reach it, `\n` breaks the line,
the control codes work. Crystal: *OpenFF mod ▸ Strings* lists the files with their id range,
*New text file…* makes one at the next free id and opens it, the inspector shows the lines,
`/api/messages` answers the `@id` hints from the project's lines where the game's have
nothing, and a Steam target gets the composed `files/eureka_permanent.msd` at Install and
Export beside the item tables (`ProjectItems.WriteTables`). The game's own "Text" kind kept
its name; the mod's is "Strings" so the two rows read apart.

Karl's look at it: the empty-list note was a grid tile - 84px wide, a column of words -
and a text file as raw JSON asks the modder to type ids by hand. Now `emptyNote()` in
app.js wraps the words in a span, and the note is one li across the list, centred in the
panel when it is alone (`#files:has(> li.note:only-child)`). And `strings-editor.js` is a
view of its own for the kind: a table of id / Copy / words / ×, *Add line* at the next free
id (the file's max + 1, or the project's `next` for an empty file), autosave 500 ms after
a change through `/api/project/file/save`, the ids checked live (`stringsIdProblem`: a
number, no twins, not below 40000000 - a line with one of the game's ids is skipped by
`Compose` in the game's favour), the hierarchy listing the lines and the inspector the
facts. The JSON pane underneath reuses the menu editor's `xml-split` (grip, collapse,
Apply); `stringsUi` remembers its height across files. One catch found on the way: with
two rows on one id the object form folds them into one at the next `JSON.parse`, so the
table writes the array form `[{ id, text }]` while a problem stands (`ModText.TryId` reads
an id as a number or a string) and goes back to the object form when it is fixed.

Localisation, the first step only: a line may be `{ "en": …, "de": … }`; `ModText.TextOf`
picks the language asked for, then English, then the first; the client asks per read with
the game's setting (`AppShell.getLanguage()` → ja, en, fr, de, it, es, zh-CN, zh-TW, ko),
cached per language, since the player can change it after the mods load. The table shows
the English and carries the rest through untouched. Not yet: the same for item names and
captions, a language column in the table, and whether the game's own message files come
per language on Steam (`changeCompanyDirectory` is the place to read).

Enemies are the definition class still missing beside items, heroes and text. What one
needs: the monster record (`monster.chaindata`, chain 0 - 100-byte `MonsterParameter`
records, chain 1 the 18-byte drop records; `MonsterManager.load` reads it whole, so a
composer appends to the chain as `ModItems` appends to `item_parameter.pak`), its name in
the monster msd, a battle model and its motions (an existing monster's, recoloured or not,
is the realistic first slice - as items start from a base), and a way into an encounter:
`monster_party_table.bbd` / `event_monster_party_table.bbd` name monster ids per formation
and the maps' encounter data picks formations per area, so a new monster needs a slot in
an existing formation, or a formation of its own placed on a map. That last part is the
real design; the record and the name are the item pattern again. Next on the list.

### The cast's code with the script editor's help (2026-09-08, late afternoon)

The CastScript's `Main` was a text area in a 250px inspector: no command list, no word on
what an argument is, a slip found in the log at the next Play. Now the frame the client
puts around a cast's lines - `map 60000; cast N { main = castN_main; } castN_main: …` -
lives once in `Shared/Script/Ffs/CastCode.cs` (`Source` with the line map back to the
cast's own lines, `Compile` with `CastCodeProblem`s placed on them, `Substitute` for
`@me`); `LegacyScripts.TryCompile` is three lines over it, and Crystal's
`/api/scene/cast-check` compiles the same way with the workspace's op table. In the
editor `cast-code.js` is a document kind (`castcode`, not in the session - it belongs to
the map's object and is opened again from it): the `.ffs` editor's template with its
completion, signature strip and highlighting (`enhanceScriptEditor`, unchanged), a
*Commands…* picker over `/api/ops` with the operand shapes, problems under the editor
with a click to the line, and a write into the attachment 400 ms after a keystroke that
hands over to the scene's autosave. The card keeps a five-line preview and the button.
Found on the way: `projectChanged` → `resetOps` after every scene save emptied the op set
under an open script editor, and `show()` then threw on `ops.list.filter` at each
keystroke - the game's own script editor had the same hole; the three users of the set now
fetch it again and come back. Tried in the browser: the list, the strip, the wrong-name
problem on its line, the picker's `flagOn();` landing with the caret between the brackets,
the scene file carrying the lines; the greeter drive runs the same through the shared
frame.

### Monsters and formations as definitions (2026-09-08, evening)

The item pattern once more, with the battle's own quirks read out of the code first.
`monster.chaindata` has six chains: 0 the 100-byte `MonsterParameter` records (255, ids
0-254 - the offsets now written down field by field in `ModMonsters.Fields`, the body at
0x10, attack 0x1C, defence 0x2C, magic defence 0x40, two specials at 0x44, drops 0x54), 1
drop tables (18), 2 normal attacks (28), 3 special attacks (16), 4 `MonsterOffsetParameter`
(160, keyed by monster id: cursor, damage numbers, shadow - `offset(id)` returns null for
an id it lacks, and the battle dereferences it), 5 effects (56). The header is 128 bytes
and every chain starts on a 128-byte line; `ModMonsters.Append` keeps that (`Append` is the
general rebuild - `ModItems.ComposePak` could sit on it). The battle model is the family's
(`f%03d.nmdp`) and a monster's own part is only a replacement texture `f%03d_%03d.ntxp.lz`,
optional (Goblin has none) - so a mod monster needs no model at all: `ContentChain.AddAlias`
(a name to try when a file is nowhere) answers `f001_1001.ntxp.lz` with the texture of the
monster `look` names, and a Red Cap-coloured goblin is one number in a JSON. Names go to
`eureka_battle.msd` (the game's 1001-1373; ours from 2001), every `.lproj` copy since the
transform matches by file name.

Two of the game's loops had the table size baked in: `MonsterPartyManager.monsterParty`
scanned `MONSTER_PARTY_MAX` = 259 rows - exactly the shipped table, so an appended party was
never found and the fallback (party 1, a Goblin) fought instead, which is what the first
run showed; and `setMonsterIdForMonsterManaia` ran `i <= MONSTER_MAX` (256) over the
bestiary's 256 entries, fine for 255 monsters, out of range with two of ours. Both now use
the table's own length / `<`; with the shipped files they do exactly what they did.

Formations: `monster_party_table.bbd` records appended (party id s16, four (monster s16,
min u8, max u8); an empty slot is -1). The `Encounter` component (an Interactable that acts
on walking in even with a figure - `Interactable.Update` now lets OnWalkIn through when the
object has an Npc; Talk and Chest keep OnWalkIn off) starts `Game.Battle.Start(Formation,
BattleMap)`. The battle takes the game out of the map and back, and the scene is cleared
and placed again around it, so the component that started the fight is gone when
`BattleEnded` comes: a static watch keeps the pending object's key, marks `SceneMemory` on
a win, and hands the result to the object's next `Start`, which hides it, makes it
walk-through (`Npc.Solid`) and calls `OnWon`. Crystal: Monsters and Formations under the
mod folder with the item-style inspectors (base picker, Look among the family, fields by
block), `[FormationField]` for the Encounter's picker (`/api/formations`: the game's 242
parties, the mod's), `/api/monsters`, `ProjectMonsters.WriteTables` for Steam targets.
Tried: three goblins on the grass, the middle one red-capped, "Goblin Chief" in the target
list; a lone goblin fought to the end and the figure gone on return (C-57).

Also on the way: the text transform reads the language off the `.lproj` folder the game
asks for (`en.lproj/eureka_permanent.msd`) before falling back to the game's setting.

Next for monsters: a texture of the mod's own (a `.ntxp` beside the definition, served
through the same alias), the encounter areas of a map (`map.CEnCountManager` checks party
ids against `MONSTER_PARTY_MAX`; the map's own encounter table is where random fights
come from), a bestiary page.

### Random encounters from the map's own table (2026-09-08, late)

Where random fights come from, read out: `CMapParameterManager` loads `<map>.pak` (chain 2,
`CMapMonsterPartyParameter`, one 40-byte record = five groups × four party ids); the step's
ground says the group - not the `.pak`'s land forms, as first assumed, but the terrain's
collision material attribute flags 20-24 (`chr.CCharacterEureka.getMonsterGroupId`); a rare
monster lottery (`RareMonsterGroupMng`) may pre-empt; then `setMonsterPartyId` draws one of
the group's four parties, skipping zeros, and refused any id ≥ `MONSTER_PARTY_MAX` - now it
asks `ModItemsLayer.HasFormation` first, so the game's ids are checked as before and a
mod's pass. Crystal's terrain card gained *Random encounters*: `MapEncounters` reads and
writes the chain through the Tables view's own `Pak` reader (the record's s16 fields come
as shorts and must go back as shorts for `Pak.Write`), the pickers are the Encounter's
(`/api/formations`), a change saves into the map's `.pak` as a project override - which is
the Steam path too. Tried on d01_02: group 1 set to formation 1001, the first fight on the
cave floor Goblin / Goblin Chief / Goblin on the cave's own b09 (E-55). One thing to keep in
mind for mod files by hand: the game's names carry `files/`, so a loose override lives at
`ff3/files/files/d01_02.pak` - the first try in `ff3/files/` was never read.

A `.ntxp` writer is what a texture of the monster's own still wants; until then a file of
the right name under `ff3/files/files/` is used before the alias, and Modding.md says so.

### The game's records as forms; the first texture written back (2026-09-08, evening)

Karl asked for UI editors on the data definitions generally, and for asset import - models,
fonts, images, in time whole maps - for both clients. Two pieces of that today.

**Records as forms.** The Game data pages were read-only grids and the only way to change
a shipped item or monster was the Tables grid, field by unnamed field. Now a row on Items,
Spells, Monsters or Encounter groups opens the record in the inspector with the same field
tables the mod definitions use (`GameRecords.cs` over `ModItems.Set` / `ModMonsters.Set`;
`record-editor.js`), grouped as the record is, the shipped value greyed in, autosave into
the project's copy of the table (the same override the Tables grid writes). Names and
captions stay in Text - the form says which message. Steam mods install the table; OpenFF
mods carry it and the client's definitions compose on top. On the way `Ff3Tables.
ReadMonsters` learned the attack/defence/stat offsets that `ModMonsters.Fields` had written
down, so the Monsters grid stopped showing zeros there.

**Textures written back.** `Tex0Write.cs`: a texture replaced in place - same size, same
format - so the TEX0's four kinds of offsets and every other texture in the package stand.
The browser decodes the PNG on a canvas at the texture's size and sends RGBA; the server
quantises. Palette formats: 15-bit colours, median cut to the entries the texture's palette
has room for (its offset to the next palette's, within `sizePltt` - which sits at +48 of
the TEX0 header, after the `vramKey`; the first try read +44 and got nothing), entry 0 kept
for see-through where the texture says so; a3i5/a5i3 keep alpha per pixel; rgb555 straight
in. The 4x4 format - what every battle monster wears - has an encoder: per block two
endpoints along its colour range (the two blended variants: mid + transparent, or 5/8-3/8)
against an explicit fit of three colours + transparent or four; the better kept while the
palette room holds, the four-entry blocks demoted by least gain, and as a last resort the
endpoint pairs merged nearest-first to fit. Round trips: Red Cap's 4x4 skin back within
4.9/255 a channel (the 15-bit floor is 4), n021's pal256 within 1.4, a5i3 exact; the game
draws them (a blue Goblin Chief in battle, blue villagers in Ur; E-57). `Textures.Replace`
recompresses and writes the override. Not done: changing a texture's size or format (a
relayout of the block), and the Images tab's 2D pictures (XNB SpriteFont/Texture2D - a
different writer, a simpler one).

**The rest of asset import, as it stands** - what each needs, from the readers we have:

- *Images (menus, the 2D layer)*: `Xnb.cs` reads XNBm Texture2D (Color, and DXT via
  `Dxt.cs`) and SpriteFont. A writer for uncompressed Color is a header and the pixels; the
  game's `SurfaceFormats` decide whether a DXT original may come back as Color (the reader
  side takes any). Days, not weeks; the same PNG-in path as textures.
- *Fonts*: FF3's are XNB SpriteFonts (glyph rectangles, cropping, kerning, a texture) and the
  DS-side `NNSG2dFont` for the message window. A SpriteFont writer is the XNB writer plus a
  glyph packer; the G2d font is a bitmap font with a character table - `GlobalScope.
  dgsmFontVector` loads it, and the Shift-JIS table in `ShiftJis.cs` is half of it. Medium.
- *Models*: `Mdl0.cs` reads BMD0 (geometry as DS display lists, materials, the bones), `Gltf.cs`
  exports it; `Ncap.cs` reads the motions. Import is the reverse of the export: glTF in →
  display lists, a material per texture, the bone tree, then `.ncap` from the animation.
  The DS geometry model (fixed-point vertices, 8 KB of matrix stack, texture coordinates in
  12.4 or 16.16) is small enough that a converter is real work but bounded; the joints must
  match the game's motion sets to reuse animations, or ship their own. Weeks.
- *Maps*: a `.nmdp` for the look (a model import), an `.mcl` for the ground (`Mcl.cs` reads
  the collision mesh with its material attributes - the very flags the encounter groups
  hang on), a `.pak` of parameters (already editable), a `.hich` of casts (readable), the
  scripts (compiler in hand), the camera set. A "new map" is all of those from a scene
  description; the pieces exist as readers, the writers do not yet. The order that pays:
  the collision writer first (a retextured or re-lit existing map with a changed floor),
  then a model importer, then the whole.
- *Sound*: `Wav.cs` reads; the archives (`Archives.cs`) hold the banks. A WAV-in for SEs is
  small; music is the sequence format and larger.

Every one of these lands in `Shared/` as a writer beside its reader and serves both the
OpenFF client and a Steam install through the same override files, as the tables do.

### New content from existing (2026-09-08, night)

Karl's point sharpened: not replacing the game's assets but adding to them. The quickest
true path there is a copy under a new name - the game loads models, textures and pictures
by name and nothing else (`setUpWorldCharacter(model)` reads `<model>.nmdp.lz`, the battle
`f<family>_<id>.ntxp.lz`, a menu its picture) - so `Workspace` now lists the project's own
files beside the shipped ones (an override with no shipped twin; `IsShipped` tells them
apart; `Write` adds to the list, `Revert` of one of ours removes it), `/api/file/duplicate`
copies any file under a new name, and *Duplicate as…* sits in every library file's
inspector - a model bringing its `.ntxp`. `CharacterIds` lists a project-own model as
placeable with id 0 ("the project's own"). Tried: n021 → n900, tinted green, an OpenFF
object wearing it in Ur beside the originals (`spawned n900 as character 13`; the game
asked for `n900.namp.lz` and `n900.nsbtx.lz` too and did without, as it does for n021).
The monster got *A skin of its own…* (`/api/project/monsters/skin`: the worn package copied
to `f<family>_<number>.ntxp.lz`, then Replace with a PNG) - the alias was the stopgap, the
file is the thing. Pictures: `/api/image/import` takes a PNG as a new `.NCGR` or over one
of the game's (*Import a PNG…* in Images, *Replace with a PNG…* on one).

What "new" still needs a writer for: a model that is not a copy (the glTF importer - next),
a map that is not a copy (the collision and parameter files beside it), a font, a sound.

### A texture package from nothing (2026-09-08, night)

The first writer that makes a file rather than patching one: `Tex0Write.Build` lays out a
whole `.ntxp` - NMDP wrapper, BTX0, one TEX0 with any number of textures - the way
n021.ntxp has it, read off the file byte by byte (the head layout is in the code's comment;
the info blocks sit at +8, +24 and +44, which is also where the reader's +20/+36/+40/+48/+56
offsets come from). The encoders were refactored to give back arrays (`Encode` → texels,
4x4 indices, palette) so `Replace` copies them in place and `Build` lays them out fresh, a
4x4 texture getting palette room for four entries a block so the fit is never forced.
The dictionaries are NNSG3dResDict with a patricia tree the game walks when it looks a
texture up by name: built as NNS's converter does (each name inserted below the last node
whose bit is higher, splitting at the highest differing bit), and for one entry exactly
the shipped files' node. Every package is read back through `Tex0.Read` before it is
written. Tried: a three-format package reading back within the encoders' error; a fresh
pal256 package on a duplicated model standing in Ur (E-59). Crystal: *New texture
package…* in Textures with name, texture name, format, transparency, size, PNG.

This is the texture half of the model importer; what a new model still wants is the MDL0
(geometry as display lists, materials, the node tree, the SBC) - the next writer.

### The OpenFF target's own formats: glTF drawn by the client (2026-09-08, night)

Karl's point redrew the plan: a Steam target is held to the game's formats, an OpenFF target
is not - so a model of the mod's own need not become a DS package at all when the client
can read the modern one. It can now. `Shared/Graphics/GltfFile.cs` reads glTF 2.0 (.glb and
.gltf; the node tree composed into each mesh's vertices; positions, normals, uvs, colours,
indices, strips and fans; materials with base colour, texture, double sided, blend; images)
into plain arrays - Shared, so both ends use one reader. The client's `GltfModel` makes
MonoGame triangle lists of it (the normal baked into the vertex colour as the DS models
carry their shade) and `ModMeshes` (the `IMeshes` service; `MeshHandle`; `Game.Meshes`)
draws them from `WorldPart.onDrawPart` right after `m_Scene.draw` - the field's camera
read off `NNS_G3dGlb.cameraMtx` as GL floats (the emulation transforms the game's own
vertices on the CPU with that matrix and leaves the GL model-view identity; the first try
read the effect's World and drew nothing), the frame's projection from the effect, each
mesh's pose as an XNA matrix on top, through `NativeRenderer.Draw` so depth, blend and the
depth-range fix are the game's. The engine: `Mesh` component (Path, OnGround), the loader
routing a Model ending .glb/.gltf to it instead of the character system. Crystal:
`GltfBundle` lays the same reader's arrays out as the map editor's ModelBundle so the 3D
view draws the file in place; `/api/project/assets` and `/import`; the model picker lists
the mod's files first with *Import a model…*; Export carries `assets/`. Tried: a red box
standing in Ur in the game (C-58) and beside the chest in the editor (E-60).

**The roadmap, redrawn.** For OpenFF targets the client reads the modern format, and the
Steam converters (glTF → MDL0, PNG → XNB, TTF → SpriteFont) are a separate, later line for
mods that must also install into the Steam games:

- Models: glTF in - done for static meshes, animated ones, as collision (`Solid`) and
  walking about (`Roam`, below). Next: `Talk` and the other Interactables on a glTF.
- Maps: a whole map of the mod's own - done (below): a free stage name, a scene file with
  a Solid Mesh for the ground, `Exit`, `Music`, `Sound` and `MapSettings` (sky, camera)
  components, *New map…* in Crystal. Next on it: its name on the menu, the map screen.
- Images, fonts, sounds: PNG is read already for pictures; music and effects of the mod's
  own as OGG/WAV - done; a TTF/OTF face of the mod's own for all the game's text - done.
- Steam targets keep to the game's formats: the duplication path, Replace with a PNG, the
  record forms, the table composers - and the DS converters when someone needs them.

### glTF animation: nodes and skins (2026-09-08, late night)

The reader keeps what it used to bake away: every node with its TRS or matrix and its
parent, the skins (joints and inverse bind matrices), the animations (channels of
translation, rotation, scale with their keys; STEP kept, CUBICSPLINE taken by its values),
and each mesh's local positions and normals with its JOINTS_0 and WEIGHTS_0.
`WorldMatrices(animation, time)` lays a clip's sampled values over the tree (rotations by
the shorter-way normalised lerp) and `Pose(world)` puts the meshes through it - an
unskinned mesh through its node, a skinned one through Σ w · (inverseBind · jointWorld) -
so the bind pose at load is the same call with no clip. The client's `GltfModel.Vertices`
rebuilds a mesh's triangle list from posed arrays; a `MeshHandle` plays a clip (`Play`,
`Stop`, `Clips`), advances it by wall time each draw and keeps its own posed vertices, so
two handles on one file may run different clips. The `Mesh` component gained `Clip` and
`Speed`. Tried: a box skinned to one joint rising and turning in Ur (C-59). CPU posing is
fine for the sizes a field model has; a skinned mesh of tens of thousands of vertices
would want the joint matrices in a shader, which the NativeRenderer path can grow.

### glTF as ground and walls: Mesh.Solid (2026-09-08, night)

The characters ask `dgs.CRestrictor` for their collision: an arrow down for the ground
(`getBottomPolygon`, GROUND polygons), a sphere along the step for walls (`calculateWallCollision`,
WALL_01..05, pushed out by `radius - length` along the averaged normal). The restrictor
walks the map's `mcl.CObject`s; it now also asks `ModCollision` (client, `Compat/`) - after
the map's objects: the arrow takes a mod triangle when nearer than the map's hit
(`ret.length` is the distance to beat), the sphere only where the map has no wall. The
arithmetic is `evaluateArrowImp`/`evaluateSphereImp`'s, on `ds.pri`'s own primitives, so
the behaviour is the map's. A solid is a bag of world-space fixed-point triangles with a
material: up-facing GROUND, side-facing WALL_01, ceilings dropped (a floor polygon with the
wall flag would lift the hero as the sphere test grazes it). `MeshHandle.Solid` rebuilds it
from the bind pose through the pose matrix whenever the handle moves; the `Mesh` component's
*Solid* field drives it. Tried: the hero stopped by a 6-wide box at 3 units (its radius),
standing at y = 2 on a 20 × 2 slab and dropping to 0 off its edge (C-60). The restrictor
also survives a map with no collision object now, which the whole-map-from-glTF line needs.

### Maps of the mod's own (2026-09-09, morning)

Tried first: `--map=t99_01`, a stage the game has no files for. The stage loader takes it
(every file it asks for is size 0 and skipped), the world part runs, the hero stands in a
black void with the field's camera on them - so a new map is a stage *name* with a scene
file behind it, not a new loader. What was missing: the hero landed at 0,0,10 whatever
`--pos` said (MapJump.setupMapJumpPosition's no-parameter-file branch; it now honours the
jump part's position), and a way out and a tune. The engine: `Exit` (Map, Arrive, Facing,
Radius - `Game.Field.Warp` when the hero walks in, the game's own map jump; re-armed on
re-entry) and `Music` (Bgm, Volume, FadeIn - `Game.Audio.PlayBgm` at Start), with
`[MapField]` and `[BgmField]` attributes the inspector turns into pickers (the game's maps
and the mod's; the Audio library's BGM list). Crystal: `ProjectScenes.Create` picks a free
stage name (`t90_00`… / `d90_00`… - the client reads the map's kind off the first letter;
the game's own names are checked), writes the scene marked `own` with a `title`, a Ground
object (a Solid Mesh of the glTF chosen, or a slab `GltfWriter.WriteBox` writes) and a
Music object; `Write` keeps `own`/`title` and never deletes an own map's file; `MapModel.Load`
answers a map with no .hich with no characters; the pages list own maps by title among the
game's, the tab and inspector say so, the .hich-only toolbar is hidden. Tried: The Old
Quarry (t90_00) made in the dialog, opened in the 3D view (the slab drawn, the Door a
marker), played - standing on the slab at --pos, BGM03 playing, the Door's Exit into Ur at
-2,0,155 facing 180 (C-61), and Ur's own Exit back onto the slab (both through the game's
map jump, fade and all). Left for later: a map's camera settings and a background colour
(the void is black), its name on the menu, the map screen.

### Music of the mod's own (2026-09-09, morning)

The game's music path is `playBGM n` → `BGMInfoMng` (a 64-entry table, seq = n) →
`NNS_SndMain` ("BGM%.2d") → `SoundManager.playSound`, which consults a hard-coded
`SoundAssignTable` for which parts (_0 intro, _1 loop) a name has and asks `MediaPlayer`
for each - and `MediaPlayer` already reads `OggSound` from the content chain before the
XNB. Two PORT points open it to a mod's own tunes: `playSound` takes a name the table
lacks with whatever parts the chain holds, and `getBGMInfo` answers a number past the
table (or with no sequence) when the chain has `BGMnn_0/_1`. `OggSound` reads a WAV too
(RIFF → `SoundEffect.FromStream`), looked for as `sound/<name>.wav`. The game ships
BGM00..58, so a mod's numbers start at 59 (`Audio.FirstFreeBgm`; to 199 - the format
takes three digits). Crystal: `Audio.AddOwn` lists the project's `files/sound/*` as own
assets (length from the Ogg's last granule or the WAV's data chunk), `Audio.Import`
writes a part and the `.dat`, `/api/audio/free` and `/api/audio/import`, *Import a tune…*
on the Audio library (OpenFF projects), the row marked the mod's own, the Bgm picker
lists it. Tried: BGM59 as an Ogg loop (the game's BGM03 loop, as a stand-in) with a
generated 1.5 s WAV tone as intro, `Music { Bgm: 59 }` on The Old Quarry - the log's
"BGM59 is the mods' own (parts 3)", both parts loaded, the tone then the loop (C-62).
Effects turned out to have no table gate at all - `playSE archive, n` → "SE%.3d_%.2d" →
`playSound` - so the same `playSound` change plays a mod's `sound/SEnnn_mm_0` under any
archive number; the game's run to 277, a mod's start at 300 (`Audio.FirstFreeSe`). The
dialog became *Import a sound…* with a tune/effect switch; the engine gained `Sound`
(Archive, Number, Volume, Radius - 0 for on entering - Once). Tried: a 0.4 s WAV chime as
SE300_00, `Sound { Archive: 300, Radius: 0 }` on The Old Quarry - the trace's `se 300/0`
as the map opens. Left: the same import for a Steam target as an XNB writer; an SE picker
for `Sound`'s two numbers.

### Roam: a glTF that walks (2026-09-09, morning)

`Wander` drives a game character's walker; a Mesh has none, so `Roam` moves the Transform
itself: a target within Radius of home, Speed units a second, a Pause between walks, the
yaw from the step, the height from `Game.Field.GroundHeight` (which now also asks
`ModCollision` outright - on a map with no collision object of the game's no restrictor is
active, so the loop over them found nothing), a stop short of the hero, and the Mesh's
Walk/Idle clips as it goes and stands. Tried: a red crate roaming a slab raised to y = 4
on The Old Quarry, the hero standing at 4 beside it (C-63). `Talk` on a glTF needed
nothing new to speak - an Interactable with no Npc already acts on A within Radius - and
gained the villager's manners: the roamer `Halt`s, the object turns to the hero, and `Go`
after the last line (C-64).

### A map's sky and camera, a mod's font, a Blender fox (2026-09-09, morning)

`MapSettings` (on the map target): Background, CameraOffset, LookOffset, ZoomRange. The
clear colour: `G3X_SetClearColor` is a no-op and `glClearColor` is called once with black,
so `glClear` reads `LegacyScreen.BackgroundOverride` (the `IScreen.Background` property)
when set; the component clears it on OnDestroy so the next map is black again. The camera:
`ICamera.Configure` lays the offsets and zoom in as `CBaseSystem.setupCamera` does from a
parameter file (z negated on the way, the zoom's reach as a negative max) and snaps the
camera to the hero; a map of the game's runs its own setupCamera on entry, so nothing
leaks (checked: Ur framed the same with and without). The 3D view clears to the sky as it
is picked. Fonts: `TrueTypeText.FindFaces` adds `fonts/*.ttf|otf` from every override
directory first - FontStashSharp draws from the first face and falls through to the rest -
so a mod's face is the game's text; Crystal's Text library lists `fonts/`, *Import a font…*
writes one (`/api/typeface/import`), and the face opens as sample lines at the game's
sizes via @font-face from `/api/typeface`. Tried: Comic Sans on the name entry screen
(C-66). Blender end to end: the Khronos Fox sample (a skinned glB with a texture and
three clips, an exporter's output rather than a hand-made file) roams with Walk and
Survey (C-67). Next: `Chest` and `Encounter` figures as glTFs, the map screen for own maps,
and a first release.

## Working rules

- Keep the game running at every commit; keep the old path behind a flag until the new
  one is proven, then delete the old path.
- Formats live once, in `Shared/`, compiled into both the editor and the client.
- Anything the editor learns about a format (the FF4 tables, the `.ncap` evaluator) is
  already the client's implementation - do not write it twice.
- `Docs/Testing.md` gets a case for every stage; the automated suites cover the formats.

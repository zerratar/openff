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
handler: operand layout and callees, from the unstripped libff4.so); `ff3content lz` +
`ff3content script --game=ff4` over the extracted CAST_SCRIPT.dat gives every FF4 script
as text, which is where the usage counts (`ce_*`: 20k uses on 42 maps) come from.

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
**Run in OpenFF** (`/api/project/run`): export, then start FF3.exe (`OpenFFClient.Executable`,
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
`FF3.Game/Compat/EngineApi.cs` implements them on the legacy game by calling what the
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
on both sides. `ff3content tables <install>` dumps a game's tables through it (E-25).

The client's side (`FF3.Game/Compat/Ff4Party.cs`): `Ff4Party.Tables` reads FF4's tables from the
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

## Working rules

- Keep the game running at every commit; keep the old path behind a flag until the new
  one is proven, then delete the old path.
- Formats live once, in `Shared/`, compiled into both the editor and the client.
- Anything the editor learns about a format (the FF4 tables, the `.ncap` evaluator) is
  already the client's implementation - do not write it twice.
- `Docs/Testing.md` gets a case for every stage; the automated suites cover the formats.

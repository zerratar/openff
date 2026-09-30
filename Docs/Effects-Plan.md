# Effects plan

Visual effects of the mods' own - spells, summons, hits, the field's sparkles - made in Crystal
the way a menu or a cutscene is: an editor with a live preview, and the game's own effects to
start from. This is the plan; stages 1 to 3 are built (below).

## What the game has

The game draws its effects with `eld` (`OpenFF/GlobalScope/eld`, about 10,000 lines), the DS
effect library the phone build kept.

- **Files.** An effect pack is `e###.efp`; `effect.efi` maps a *category* and a *member* to a
  template in some pack. The install has 247 packs in `files/` (193 of them the battle's `e###`),
  each holding several templates; the game asks for them as `/EFFECT/...` and the loader drops
  the folder. The battle loads `effect.efi`, `e201.efp`, `e435.efp`
  and a pack a category as actions need them (`btl.BattleEffect.setup`, `/EFFECT/e%03d.efp`);
  the field loads `w_common.efp`, `event%02d_%s.efp` and `w_landform_%d.efp`
  (`wld.CBaseSystem`).
- **Templates**, five kinds, each a factory GUID and a parameter block (`eld.Template`):
  - `ParticleDS` / `ParticleLargeDS` - billboard particles: a birth range, a size, speed and
    spread, gravity, an orbit (`CircleController`), a fade, after-image trails, and keyframed
    colour, scale and texture-cell (UV) animation (`spr.Eff_AnimationHeader`).
  - `ParticleGatherDS` - particles drawn in toward a point.
  - `ModelDS` - a Nitro model with its own motion and material animation.
  - `SequenceDS` - a script (wait, position, boot, halt, move) that moves along line or
    Hermite paths (`cv.Ferguson`) and boots other templates as it goes. The larger spells are
    sequences of the others.
- **Runtime.** `eld.ServerFF3` (`eld.g_elsvr`) steps every object once a game step (30 a
  second); particles spawn in groups (`usTimeInterval`, `nbGroups`), their pools sized as the
  object starts; they are drawn as camera-facing quads through the DS GL emulation
  (`ds.pt.PrimitiveDisplay.drawParticles`).
- **A spell's effect.** `pl.PlayerParty.normalMagic(id)` - chain 12 of `player.chaindata`,
  `PlayerNormalMagicParameter` - holds the hit effect's category and member, the frame it
  starts on, a loop flag, a sound and an offset. The cast effect is one a school (black 407,
  white 408, summon 243, monsters 406). Summons are scripted (`summon_script_command.pack`,
  `SummonCommand.drawSummonEffect`). Where it plays: the target pulled toward the camera
  (`setHitEffectPosition`), or fixed points for spells on a whole side.
- **The client already** spawns, moves and follows the game's effects from mods
  (`IEffects`, `Compat/EngineApi.cs` `LegacyEffects`) and plays a spell's effect when it is
  cast on the field (`Compat/EngineAbilities.cs`). Nothing edits or replaces effect data;
  Crystal has no effect code.

**Its limits are the DS's.** Fixed-point maths; colours and alpha in 0-31 steps; DS palette
and compressed textures; blending only as `MODULATE` with an alpha - no additive light; fixed
caps (32 live effects, 24 packs in a battle - the 25th overwrites slot 0 -, one pool of 128
list nodes shared by everything, which fails silently when full); every template keeping a
copy of its whole pack; and some dead code (particle `SetScale`, `Pause` and `Restart` do
nothing, a few parsed fields are never used).

## The decision

**A new effect engine beside `eld`, and an importer from `.efp` into it.**

- *Not extending `eld`*: every limit above is in its data, not just its code. Writing DS
  binaries back is fragile, and an editor over them would inherit 0-31 alpha and no additive
  blending. Fixing its bugs changes how the game's own effects look, which the port avoids.
- *Not a new engine alone*: a blank editor has nothing to start from. The game's own effects -
  247 packs of them - as templates are the best part of the offer - a modder opens Fire, makes it bigger and
  greener, and saves it as their own.
- **`eld` stays** and plays the game's effects exactly as it does now. The new engine plays
  the mods' effects. A spell points at either.

The importer is one-way and faithful enough to start from, not a second player of the game's
effects: an imported Fire looks like Fire, and then it is the mod's.

## The new engine

### The format - `defs/effects/<id>.json`

An effect is a set of **tracks** on a timeline (frames at the game's 30 a second, or seconds):

| Track | What it is |
| --- | --- |
| `emitter` | Particles, a stack of modules (below). |
| `mesh` | A glTF (the mod's, as items and maps have them), with its clip, moved by curves. |
| `trail` | A ribbon behind a moving point. |
| `flash` | The screen or the scene tinted for a moment. |
| `shake` | The camera shaken. |
| `sound` | A sound of the game's or the mod's, at a frame. |
| `spawn` | Another effect started at a frame (what `SequenceDS`'s BOOT did). |

**Emitter modules**, after Unity's particle system, in a fixed order:

- *Emission*: rate over time, bursts, a total limit, loop.
- *Shape*: point, sphere, box, cone, ring, a mesh's surface.
- *Life, speed, size, rotation*: each a constant, a range, or a curve over the particle's life.
- *Forces*: gravity, drag, a point to be pulled toward (the old *gather*), orbit about an axis
  (the old *circle*), noise.
- *Colour over life*: a gradient, colour and alpha.
- *Texture*: a PNG beside the definition, or one of the game's pictures by name (as menus
  do - nothing of the game's shipped); a flipbook of cells; UV scroll.
- *Render*: billboard, stretched along velocity, horizontal, mesh; blend alpha, **additive**,
  premultiplied or multiply; depth test and write; sort.

**Where it plays** - an effect's anchors: the caster, the target, each target of a spell on
several, between them, a side's centre, or a fixed point; a track is placed on an anchor, in
its space (world, anchor, or following it), with an offset. A path track moves along points
(a line, or a spline - what `SequenceDS`'s paths did) between anchors: a bolt from caster to
target.

Numbers are floats and colours 0-255 (0-1 in curves); every module is optional, and what a
file leaves out is its default, so an effect is as short as it is simple.

### The runtime - `OpenFF/Compat/Effects/`

- Written like the rest of the client's own drawing: particles simulated on the CPU (a few
  thousand live is plenty here and costs little), drawn through `NativeRenderer.Draw` - which
  already takes a blend state - in the scene's own passes, so they sort with the game's models
  and the glTF look-alikes. Recorded by `FrameCapture` like everything else, so 60 fps and
  above interpolate them between steps.
- One step a game step. Seeded randomness: the same effect with the same seed plays the same,
  so Crystal's preview matches the game.
- No fixed pools: effects and particles are lists, with a cap a definition can raise (a
  runaway emitter is stopped and logged, not the game).
- Hot reload: a definition saved while the client runs plays as saved the next time it
  starts, as menu layouts do now.

### Hooking it in

- **Spells.** A magic item's definition gets `"effect"`: the id of a mod effect, or the
  game's own as `"game:407/2"` (category / member) - so the first thing a modder can do, before
  any editor exists, is point Fire at one of Ifrit's effects. The client answers
  `normalMagic` with the definition's effect (the frame it starts on, loop, sound and offset
  too), and when it is a mod effect, the battle plays it through the new engine at the same
  place and frame the game would have.
- **Casts, hits, summons.** The same for a school's cast effect, a weapon's hit, and a
  summon's steps (a summon's script names its effects one by one; a definition can replace
  each).
- **The engine API.** `IEffects.Spawn(string id, ...)` beside the category/member one, for
  scripts and cutscenes; `Cutscene` gets an effect track.

## The importer - `Shared/Effects/Efp.cs`

Reads `.efp` and `effect.efi` into the new format - the code Crystal and the client share.

| `eld` | Becomes |
| --- | --- |
| `ParticleDS` / `ParticleLargeDS` | an `emitter`: birth range → *shape* box; `usTimeInterval` × `nbGroups` → bursts; speed, spread, gravity → *speed* and *forces*; `CircleController` → orbit; `FadeController` → alpha in the gradient; size base / random → *size*. |
| after-images | a `trail`, or the emitter's particles leaving copies (a sub-emitter). |
| `Eff_ColorSeq`, `Eff_ScaleSeq` | colour and size curves, with their keys. |
| `Eff_UVAnimation` | *texture*: a flipbook of the texture's cells. |
| DS texture | decoded to a PNG (the palette formats, 4x4 compressed) - as a `resource:` reference, the game's own read from the player's install, never copied into the mod. |
| `ParticleGatherDS` | an emitter with a pull toward its anchor. |
| `ModelDS` | a `mesh` track, the model converted through the model pipeline Crystal already has. |
| `SequenceDS` | the effect's timeline: WAIT is time, BOOT a `spawn` at a point along the path, MOVE and POSITION a path track. |

The importer's test: the game's effects imported and played side by side with `eld` - screen
shots a frame apart - for a list of spells (Fire, Blizzara, Thundaga, Cure, Ifrit's steps).
Close enough to recognise and to start from, not pixel for pixel.

## Crystal - the Effects tab

- **Browser.** Every effect of the game by pack and member (named where the spells that use
  them say: "e407 / 2 - black magic cast"), and the project's own. Each plays in a small
  preview on hover. *Duplicate into the mod* imports one.
- **Stage.** The preview: one of the game's battle backgrounds, a caster and a target (a
  hero, a monster, several for spells on a side), the battle camera, play / pause / step a
  frame / loop, a scrubber, 30 or 60 fps, and a particle count.
- **Inspector.** The selected track's modules, each folding open, with a switch to turn it
  off; a value can be a constant, a range, or a curve.
- **Curve and gradient editors.** Keys to drag, tangents, presets (fade in-out, pulse, ease).
- **Timeline.** The effect's tracks in rows, as Crystal's cutscene timeline has them
  (`wwwroot/timeline.js`) - drag a track's start, its length, a sound's frame.
- **Play in the game.** As with maps and menus, the running client plays the edited effect on
  its next start - the Stage for looking, the game for how it feels in battle.
- **In the item's inspector.** A magic item's card gets an *Effect* picker (the game's,
  or the project's), with a preview.

**The preview's engine.** Crystal draws in the browser, and its menu canvas keeps JS ports of
the client's code in step (`menu-layout.js`, `menu-styles.js`). An effect's simulation is the
same kind of code: a port, `effects.js`, drawing with WebGL, kept to the C# runtime by shared
test cases (an effect, a seed, a frame, and the particles' positions, colours and sizes it must
come to). The format is small enough that two copies stay honest that way.

## Stages

Each stage is usable on its own; each ends with the docs (`Modding.md`) and a sample.

1. **A spell's effect in its definition** (small) - *done*. `defs/spells/<id>.json` rather
   than a field on magic items, since the game's own spells have no item definition to carry it:
   `"effect"` another spell or `game:<pack>/<member>`, `"sound"`, `"frame"`, patching
   `normalMagic`; a mod item gets its base's record. The summons turned out to be script plus
   effects: their finales (367, 371, 375 ... 392) play on any spell, and their other outcomes are
   spell records of their own (4205 ... 4221). A school's cast is `"cast"` (black, white, summon,
   none or a pack), answered where the battle picks it (`TurnSystem.magicStartEffect`).
   `Samples/SummonMagic`.
2. **The reader and a viewer** (medium) - *done*. `Shared/Effects/Efp.cs` reads packs and the
   index (every template kind, the sprite animations, the textures decoded);
   `Shared/Effects/EffectImport.cs` makes a member a definition of format 1 (below); Crystal's
   Effects library plays it on the Stage (`wwwroot/effects.js`, the runtime's first port).
   Sequences came in with it - member 1 of a spell is a sequence booting the particles along
   paths, and nothing looks like Fire without it. Fire, Blizzard and Cure, and the schools'
   casts, play as the game plays them, models and their motions too (not yet their material
   animations), on the target the battle would play them on. The game's side - every
   layout, the step-by-step runtime, its quirks - is `Docs/Effects-Eld.md`.
3. **The runtime in the client** (large) - *done*. `Shared/Effects/EffectPlayer.cs` plays
   format 1 in the client and is effects.js's twin - `Tools/EffectCases` holds the cases both
   are checked against (`node Tools/effect_cases.mjs`, `crystal effect-cases`). A mod's effects
   (`defs/effects`) get categories from 1000 up, run as eld objects the battle places and deletes
   (`OpenFF/Compat/ModEffects.cs`; `eld.Manager.createObject`), drawn after the game's effects in
   `Scene.draw` through `NativeRenderer.Draw` (alpha or additive), each particle recorded by
   `FrameCapture` so 60 fps draws it between steps; mesh tracks are the game's own model objects.
   Spells name them (`defs/spells` `effect`, `cast`), `IEffects.Spawn(id, position)`, the
   cutscene's *Effect* clip; hot reload as the effect starts. Anchors: the target, the caster
   (its hit point in battle, the hero on the field), between them, the world; a path from one
   anchor to another with an arc (a bolt). Sound, flash and shake tracks, the battle's own in
   battle and the field's API on the field. A mesh track is the game's model or a glTF of the
   mod's, posed by its clip. `Samples/EmeraldFire`.
4. **The editor** (large). The inspector, curves, gradients, timeline, the item card's picker,
   *Duplicate into the mod*, *Play in the game*. *Done when* a modder can make a new spell's
   effect in Crystal from one of the game's without touching JSON.
5. **Later.** GPU particles if a real effect needs more than the CPU gives; lights from
   effects; decals on the ground; a node graph only if the module stack is found wanting.

## Format 1, as the importer writes it

The first cut of the format above, what the Stage plays today: an effect is `format`,
`from` (the game's category/member it came from), `length` (the frame its sequence ends on),
`loop`, and `tracks`. A track is `type` (`emitter`, or `mesh`: `model` - `game:<pack>:0x<id>`, the
game's own -, `scale`, `loop`; it plays its motion and ends with it), `name`,
`start` (its frame), `id` (the sequence's, for a loop not to be booted twice), `anchor`
(`target` - the default -, `caster`, `between`, `world`), `offset`, `path` (`point`; `segments`
of four points - P0, P1 and the two tangents - with `curve`, `times`, `length` and `end`: hold,
pingpong, repeat; or `from` an anchor `to` another over `length` steps, rising by `arc` at the
middle, the track's anchor aside) and `stop`. A `mesh`'s `model` is the game's
(`game:<pack>:0x<id>`) or a glTF of the mod's (a path from its folder: `assets/x.glb`), with
`scale`, `yaw`, `clip` (the glTF's animation, `speed`, `loop`) and `life` (steps; else while the
effect plays). The tracks that are neither: `sound` (`archive`, `number`, `volume`, `load`),
`flash` (`colour`, `frames`, `interval`, `count`), `shake` (`frames`, `power`, `speed`), at their
`start`. An emitter adds:

- `emission`: `duration`, `interval`, `count`, `bursts`, `loop` - groups of `count` particles,
  one every `interval` frames (0 and 1 alike), `bursts` of them in the `duration`, as eld has it;
- `life` (frames), `space` (`local` follows the emitter), `shape.box` (half extents),
  `size` (the full width, a range);
- `speed`: `direction`, `value` (a range), `spread` (degrees either way about X, Y, Z - an import
  writes 180, a whole turn, where the game has any: that is how the port turns them in play);
  `gravity`: `direction`, `value` (per frame, a range); `orbit`: `radius`, `grow`, `turn`
  (degrees a frame); `gather`: `speed`, `accel`, `swirl`; `trail`: `count`, `colour`;
- `colour` (keys `[age, r, g, b, a]`, 0-255), `scale` (keys `[age, x, y]`) - straight lines
  between keys, whole frames of age;
- `texture`: `image` (`game:<pack>:<name>`, the game's own read from the install), `width`,
  `height`, `cell` (the first cell's rectangle), `columns`, `frames` (keys `[age, cell]`);
- `render`: `blend` (`alpha`), `facing` (`camera`).

## Open questions

- **Summons.** Partly answered by stage 1: a summon is its script (`summon_script_command.pack`)
  loading packs and drawing members, its outcomes spell records of their own. Whether a
  definition replaces a summon step by step or its script whole is still open.
- **The DS textures** - answered: A3I5 (774) and 256-colour (24), 32-colour palettes, none
  compressed; they decode exactly.
- **Sequences.** How faithfully `SequenceDS`'s paths and boots map to tracks - the largest
  spells are the test.
- **Performance.** Where a battle full of mod effects lands on a slow machine; the cap per
  effect.
- **FF4.** Its effects are another format (`Ff4Cutscene` loads packs by name); the new engine
  serves both games, the importer FF3's first.

# Final Fantasy IV in OpenFF - where each piece stands

The README gives the overview; this is the detail, subsystem by subsystem, of how far FF4
is from playing like the original in this client. The percentages are our judgement of
distance from 1:1 - the whole game, looking and behaving as the Steam release does - not
lines of code. Every row's history is in `Docs/Client-Plan.md` (the journal, dated);
what has been read out of the game's binary, and what has not, is in
`Docs/FF4-Internals.md`. Pull requests on any row are welcome - the journal says how each
piece was approached, `Docs/Testing.md` how one is proven, and the client's log names
what a map or scene skipped (`script: FF4 scene … skipped n command(s): …`).

How FF4 runs here: the same client as FF3, behind a `GameProfile` seam. FF3's logic is the
recreated original; FF4's is a native binary, so everything FF4-specific is written from
reading that binary and its data files, and everything not yet written falls back to FF3's
behaviour or to a stand-in. That is why "renders" comes first and "behaves" later.

## The world - 85%

| | State |
| --- | --- |
| Maps: towns, castles, dungeons, event stages | Render from the Steam files in place - models, textures (FF4's texcoord command decoded), water, bridges, animations. |
| The overworld | FF4's chip layout (mirrored in z relative to FF3's - measured against 33 scripted arrivals), the chips streaming in, the tilted mountain and forest quads seen from FF4's camera side. |
| Characters and objects | Cecil and the party's field models, NPC bodies, objects; the field motion sets renumbered so idle, walk and run play as such. |
| Lighting and materials | Scene lights and toon shading not applied; a few scene shots show sky geometry wrongly (the Red Wings' flight). |
| Shadows | The field's; scene casts' own shadow discs follow their hips but are not yet visible. |

## The field - 55%

| | State |
| --- | --- |
| Walking, collision, the camera | The engine's own, with FF4's camera offsets per map kind (field, dungeon, town) read from the binary. Movement tuning tables FF4 compiled in are synthesised. |
| Exits | `setInsideMapJump` / `setOutsideMapJump` are real commands: an exit exists from the moment its script declares it, and walking into its box makes the jump. |
| Talking | The game's talk on contact; the speaker's name window (`openCharacterNameWindow`) is implemented. |
| Script commands | 247 of FF4's 500 run - FF3's handler where the command is the same, renamed or has extra operands; FF4-only commands implemented so far: exits, the name window, confirm boxes, locale waits, reward messages, player levels; cosmetic ones (doors' dust, BGM ducking, the jump history) skip quietly. Every map and scene logs what it skipped, by name and count - that log is the work list. `--ff4table` lists them all. |
| Map parameters | Encounter tables and landforms read from the binary; camera chains as above. |

## Cutscenes (the `ce_*` scene engine) - 70%

The opening on the Red Wings, the flashbacks, and every scripted scene run on FF4's own
scene engine, which is ours from the binary's disassembly.

| | State |
| --- | --- |
| Casts | Slots set up with model and texture, motions bound and started, placed, turned, shown, faded, cleaned up; stage swaps mid-scene; view-volume clipping per cast. |
| Camera | Camera motions from `EVT_CAMERA.dat` (CMS2 sets) with the script's FOV, on the Steam game's frames (Tools/ff4hook's camera trace, shot by shot); the blend length is read (no shipped script uses it). |
| Effects | The scenes' effects (the flashback sparkles, the wind streaks) from their packs, scaled and placed as the script says. |
| Into battle | `ce_CallBattle`: the encounter whirl (SPBlurRotate) to white, then the battle on its stage as a part change, and back to the scene after. |
| Faces and props | Expressions through face textures; bind objects (a spear in a hand) posed from the host's joint. |
| Text | The message bar; the name window. |
| Open | Lights and toon shading; the flight's sky geometry and per-shot visibility; the casts' shadow discs. |

## Party data - 60%

| | State |
| --- | --- |
| Characters, growth, magic, equipment, items | FF4's tables read from the binary and its files onto the unified data layer (`Shared/Data`), the same layer FF3's jobs and spells sit on. |
| Logos and title | Steam's, read from FF4.exe (Ff4Title): the Square Enix and Matrix logos, then the title - background, logo, CONTINUE / NEW GAME / LOAD GAME / QUIT GAME by the same rules and in the same places, the glove, the Prelude. New Game fades to white and starts in t00_00, whose event calls the opening scene with no fade, so the scene fades in from white as on Steam; maps are left and entered with FF4's 15-frame fades, not FF3's shutter (Ff4MapChange). Load Game opens a file list (the port's; Steam's load screen is not read yet). The opening movie (opening.mkv) is not played; CONTINUE never shows (no suspend save); the title's mouse widgets are not there. |
| Saves | On the unified layer (a save slot loads into the field from the title's Load Game or `--load`); FF4's own save format is not written. |
| Open | Growth curves and monster records in part; the details of each are in FF4-Internals. |

## Encounters, shops, inns - 40%

Encounters start from the map's tables and the field's step counter; shops and inns run as
the scripts call them, with FF4's prices and stock. The windows they show are FF3's dressed
with FF4's art, not FF4's own screens.

## Battle - 55%

Ff4Battle, written from FF4's own battle code (libff4's btl::) and measured against the Steam game's
frames (Tools/ff4hook). A scene's battle comes after the encounter whirl (SPBlurRotate) on its stage as
a part change; the group's monsters at their places and facings, the party on the group's root at its
FF4 positions and rows, the motions addBasicMotion binds, the weapon and shield held. The turns as
BattleBehaviorManager runs them: decided actions queue and go one at a time, a turn over when its
steps are done and its effects have stopped (the fallen fade then, 10 frames), the gauges filling
while nothing or a monster's action is under way and the commands open throughout (the active mode;
--ff4-battle-wait for the wait mode), an action's wait (ATW) from ability.bbd. The member's attack
(poise, swing, the weapon's hit effect and sound, the number) and the monster's (its chain 2 record:
effect, sound, number), effects at hitEffectPosition, the damage numbers a digit at a time from each
monster's spot, the damage and hit formulas with criticals, the elements' and races' multipliers
and the back rows'; the monster AI (MonsterActionThinker: the AI record's conditions - HP, the events'
variables, what struck it - its action sets in turn or at random, the target types, the counters
every monster weighs as a turn ends); the battle events (BattleScriptEngine, battle_ai.bbd); the win
(fade through black, the poses and camera, the result with the gil counting up, a page for each
level-up and one for the items, the fade out). The HUD is a layout (Data/hud/ff4_battle_hud.xml,
Docs/Menus.md) a mod can reshape or restyle. Since then: magic shown as executeCommonMagic shows it (the name, the
chant, each target's effect in turn, the numbers once the effects end) on both sides, the monsters' own abilities
(effectsInfo), Needles and Pincers, Alarm and Summon, the Mist Dragon's mist, the statuses (condition_parameter.bbd:
inflicted by blows and spells, their timers, Poison's ticks, Sleep, Paralyze, Confuse, Berserk, Blind, Silence, Toad,
Mini, Protect, Shell, Slow, Haste, Doom; the name in the party window, the kneel and the effect over the member), the
KO motion and FF4's defeat line; then Reflect, the bosses' own actions (transformations, the Octomammoth's legs, the
Four's rematch...), Toad/Pig/Mini's models, the status tints, the game's end to the title, the boss entrances' camera
(battle_parameter chain 4), the invoke close-ups, the camera shake, the random encounter's zoom (Encount), the
escape (Run away held, the roll, the dropped gil) and the summons' cast scenes (CAST_SCRIPT.dat on the field's script
engine: Rydia's prelude, the summon's stage and show). Not FF4's yet: a summon's own model-animation pack (the
Chocobo's parts), the summons whose stages animate, skipping a summon, enemy-player commands, the battle modes'
other cases.

## Menus - 15%

The menu is drawn from FF4's own `.xbn` layouts over the unified party data, so it wears
FF4's dress - but the screens' contents and flow are largely made up where the original's
behaviour has not been read: equipment and item use work, much else is placeholder. Far
from the original.

## The Steam shell - 0%

FF4's Windows shell (`FF4.exe`: the launcher's screens, achievements, its own settings)
is documented in FF4-Internals from the Babil Decompilation Project's work and not
started here.

## In one line

FF4 in OpenFF is a world you can walk, with its scenes playing and its battles fought by FF4's own
rules, on top of field commands and menus that are still largely FF3's. Bringing the field commands,
the rest of the battle (statuses, abilities) and the menus to FF4's own is the work ahead.

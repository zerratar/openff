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
| Text | The message bar; the name window; the field's message window opening from its middle over five frames and its caption bar, as Steam's, drawn from a layout (`ff4_field_hud`). |
| Field events | Scripted walks and turns (MoveCharacter_*Coordination2, Turn*2) and the event camera following the cast (CUFollowCamera: moveCamera_LookPlayer2, setCameraOffset easing in over its frames) as libff4's; a map's first script steps survive the host noticing the map change. |
| Open | Lights and toon shading; the flight's sky geometry and per-shot visibility; the casts' shadow discs. |

## Party data - 60%

| | State |
| --- | --- |
| Characters, growth, magic, equipment, items | FF4's tables read from the binary and its files onto the unified data layer (`Shared/Data`), the same layer FF3's jobs and spells sit on. |
| Logos and title | Steam's, read from FF4.exe (Ff4Title): the Square Enix and Matrix logos, then the title - background, logo, CONTINUE / NEW GAME / LOAD GAME / QUIT GAME by the same rules and in the same places, the glove, the Prelude. New Game fades to white and starts in t00_00, whose event calls the opening scene with no fade, so the scene fades in from white as on Steam; maps are left and entered with FF4's 15-frame fades, not FF3's shutter (Ff4MapChange). Load Game opens Steam's load screen - the menu's Save screen over the title, the hand on the slot written last, "Load data from Slot N?", Back or Title Menu back to the commands. CONTINUE shows when there is a quicksave and resumes it at once, as Steam's (the quicksave is kept, as libff4 keeps its suspend data). The opening movie (opening.mkv) is not played; the title's mouse widgets are not there. |
| Saves | On the unified layer: three slots, written from the menu's Save screen (Steam's) with the place and the play time; a slot loads into the field from the title's Load Game or `--load`. FF4's own save format is not written. |
| Open | Growth curves and monster records in part; the details of each are in FF4-Internals. |

## Encounters, shops, inns - 40%

Encounters start from the map's tables and the field's step counter; shops and inns run as
the scripts call them, with FF4's prices and stock. The windows they show are FF3's dressed
with FF4's art, not FF4's own screens.

## Battle - 70%

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
engine: Rydia's prelude, the summon's stage and show). The party's commands, each read from libff4 and then set
frame by frame beside the Steam game (Tools/ff4hook's `exec` puts any member in the party and starts any fight): Jump,
Darkness, Aim, Pray, Focus, Brace, Kick, Steal, Throw, Bluff, Cry, Hide and Return, Salve, Recall, Twincast, Analyze,
Upgrade, Cover and its interception, Bardsong (the lasting songs too), Ninjutsu (Smoke's escape), Bless; Defend instant;
the poise from a decision, the confirm windows, the command window's second line (Focus's rounds, Twincast's sync) and
greyed commands, the target window's line (accuracy, a steal's chance), the lists as Steam's with their description
window, the lists' icons, Re-equip, the target card's elements, Skip, the battle speed setting. The openings
(world::attackType's roll by the dash and the party's level against the area's, an event's own): a preemptive
strike's "!" over the monsters and the party's full gauges, a back attack's swapped rows, the party turned away under
the "!" and then turning (Steam's 18 + 16 frames), a surprise, a boss's back attack after its entrance; the
formation put back as the battle ends. The summons' custom casts (setCharacterWithTextureAndAnimation: the Chocobo on
the monster model with its own visibility pack - its blinks) and the stage commands (BTL_SetMapMotion, MapStartMotion,
StartMapAnimation on the summon's stage), the casts' lights, the animations' loop flag; the cast camera's 2..8192 clip
no longer overridden by the battle camera's (the Chocobo's eye close-up as Steam's); skipping a summon (a press once
BTL_SetSkip allows it: 15 frames to black, the sounds out over 30, the scene run through to BTL_StopSkip with its waits
and shows passed by - Steam's fade, black and fade in frame for frame). The monsters that fight as members
(BattleEnemyPlayer, battle_parameter chain 7: the Dark Knight, Kain, the Bard, the Girl, Yang): the player model and
motions, the table's weapons, a member's Fight (Steam's Dark Knight: 11 frames of poise, his first swing 95),
Darkness with its close-up and chant (b_pa_018 - a member's Darkness lacked it too), Kain's Jump (b_pa_016 / 017,
bound for members now as well), the kneel when weak but for the Dark Knight, the KO motion. Dualcast (an augment
command learned - ability ids under 256 in a member's abilities are commands): the dual list (White then Black Magic),
the second pick within the MP both cost, the MP shown less the first's, the two back to back (counters after);
the spells' chant motions 4004 / 4005 (b_pa_005, initializeMagic) bound for every caster. Not FF4's yet: the Config menu's battle speed (--ff4-battle-speed for now), augments learned in play (no augment system - the drive's `augment` grants one), hit reactions.

## Menus - 60%

The field menu is being rebuilt screen by screen to match the Steam game's: each screen is
captured from Steam (Tools/ff4hook's `draws`: every quad of a frame in 1080p pixels), its places,
sizes and colours written into a layout (`Data/hud/ff4_menu.xml` and `styles/ff4_menu.css`) that
a mod reshapes or restyles like the battle HUD (Docs/Menus.md), its texts and art read from the
game's files (MENU_Common.dat, babil_menu.msd and the other tables) and its rules from libff4.

| | State |
| --- | --- |
| Root | The nine commands in MENU_LAYOUT's order with the scroll bar, the place and the gil, the key hints, the member panel's five places, the lead's thought in its balloon (babil_speculation.bbd's line for where the story stands). The place as WSMenu::wsmGetSavePointIndex names it (babil_savepoint.bbd: "Baron Castle", not the plate's "- 1F"). Open: the lead's 3D model, the world map's area names (tables compiled into the game). |
| Inventory | Two columns with the items' symbols, greyed by the record's field-use flag as WSCMenu::checkItem (Red Fang); key items on C; using an item on the party's places; status cures cure; Sort (Tab) as MSSItem's - consumables, weapons or armour at the top in turn, each in its records' order. |
| Magic | The member between arrows, the spell's cost and line from babil_ability.msd, three columns, C for the next school, Z / M to change member. Casting from the menu follows the old path. |
| Equipment | The member's figures, the lit piece's line, the five slots and what in the bag fits the lit one; C removes, Tab optimizes. Open: the figures' before / after comparison. |
| Abilities | The auto-battle command and the five battle commands; Enter picks one up and swaps it with another. Open: the auto-battle command's own list (libff4's abilityIDList 5), choosing from a list. |
| Status | As Steam lays it out: the figures, EXP and the next level, what is worn. |
| Quicksave | "Quicksave game and quit?" then the title, whose CONTINUE resumes it. |
| Gambits | OpenFF's auto-battle rules (as FF3's) in the FF4 menu's style: twelve rules with ON / OFF, condition and action, and the picker for both. |
| Party | MSSFormation's: Swap Rows turns the formation over, Party Formation trades two places (either may be empty); back-row faces stand further in, as on every member panel. |
| Settings | Steam's nine lines: Music, Sound Effects, Voices (0..10), Battle Mode (Active / Wait), Battle Speed (1..6), Subtitles, Window Design (MENU_Common's six sets: frames, scroll bars, buttons, wallpaper - the layout's design-N class), Help, Quit (to the title). Kept in %LocalAppData%\OpenFF\ff4-settings.json; the volumes and the battle's mode and speed follow them. Not yet checked on screen against Steam's captures; Help has no screen, Voices and Subtitles are not used yet. |
| Save | Steam's: offered on the world map and at save points (land forms whose encounter rate is 0xFF, read under the hero as checkLandForm does). Slot 1..3 and Title Menu down the right; the lit slot's party by places, the place, the play time and the gil on the left, or "No save data found."; "Save data to Slot N?" / "Return to the title menu?". |
| Open | The first save point's explanation; New Game's difficulty question (Normal / Hard); the lead's 3D model on the root; Abilities' auto-battle list. |

## The Steam shell - 0%

FF4's Windows shell (`FF4.exe`: the launcher's screens, achievements, its own settings)
is documented in FF4-Internals from the Babil Decompilation Project's work and not
started here.

## In one line

FF4 in OpenFF is a world you can walk, with its scenes playing and its battles fought by FF4's own
rules, its field menu half rebuilt as Steam's, on top of field commands that are still largely FF3's.
Bringing the field commands, the rest of the battle (statuses, abilities) and the last menu screens to
FF4's own is the work ahead.

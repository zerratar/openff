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

## Cutscenes (the `ce_*` scene engine) - 60%

The opening on the Red Wings, the flashbacks, and every scripted scene run on FF4's own
scene engine, which is ours from the binary's disassembly.

| | State |
| --- | --- |
| Casts | Slots set up with model and texture, motions bound and started, placed, turned, shown, faded, cleaned up; stage swaps mid-scene; view-volume clipping per cast. |
| Camera | Camera motions from `EVT_CAMERA.dat` (CMS2 sets) with the script's FOV; the blend length is read (no shipped script uses it). |
| Faces and props | Expressions through face textures; bind objects (a spear in a hand) posed from the host's joint. |
| Text | The message bar; the name window. |
| Open | Lights and toon shading; `ce_CallBattle` from a scene; the flight's sky geometry and per-shot visibility; the casts' shadow discs. |

## Party data - 60%

| | State |
| --- | --- |
| Characters, growth, magic, equipment, items | FF4's tables read from the binary and its files onto the unified data layer (`Shared/Data`), the same layer FF3's jobs and spells sit on. |
| Saves | On the unified layer (a save slot loads into the field with `--load`); FF4's own save format is not written. |
| Open | Growth curves and monster records in part; the details of each are in FF4-Internals. |

## Encounters, shops, inns - 40%

Encounters start from the map's tables and the field's step counter; shops and inns run as
the scripts call them, with FF4's prices and stock. The windows they show are FF3's dressed
with FF4's art, not FF4's own screens.

## Battle - 25%

A stand-in, honestly: FF3's battle system drives FF4's battle stages, monsters, party
positions, window art and glove, with damage, hit and magic formulas taken from the
binary's tables. FF4's own battle - its ATB flow, its command set, its animations, its HUD
behaviour - is not there. It fights, and it is not FF4's fight.

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

FF4 in OpenFF is a world you can walk, with its scenes playing, on top of systems that are
mostly FF3's. Bringing the field commands, the battle and the menus to FF4's own is the
work ahead, in that order.

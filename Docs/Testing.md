# Testing the editor against the real games

What to try at the computer, in the order that proves the most with the least effort.
Each case says what to do in the UI, what the game should show, and how to get back to
vanilla. Automated suites cover the file formats (`Tools/test_*.py`); these cases cover
the one thing a test cannot - the shipped game reading what the editor wrote.

Before any of it: `ff3content.exe` with no arguments finds the Steam installs, opens
the browser, and offers the projects menu. Nothing here needs a command line.

## FF4 3D (Steam)

Create a project first: **File ▸ New project**, target **FF4 on Steam**.

| # | Case | Steps | Expect | Undo |
| --- | --- | --- | --- | --- |
| F4-1 | Loose file: menu text | Text ▸ `files/babil_menu.msd` ▸ id **50002** "Inventory" ▸ rename ▸ Save ▸ Project ▸ Install | Main menu shows the new word | Project ▸ Remove; vanilla word back |
| F4-2 | Mass-file entry: dialogue | Text ▸ `files/t01_00.msd` ▸ id **581000** "So, you're Cecil." ▸ edit ▸ Save ▸ Install ▸ New Game | The Baron opening says the new line | Remove; check `CAST_EVENT_MSD.dat` size unchanged from original |
| F4-3 | Changes panel revert | With F4-1 and F4-2 both installed: File ▸ Changes… ▸ revert only the dialogue | Menu still modded, dialogue vanilla, in the game | Remove |
| F4-4 | Script edit | Scripts ▸ `files/d01_00.script` (Baron castle) ▸ in `func_676074621` change the first `startMessage` id to another id from the same msd ▸ Save ▸ Install | The NPC says the other line | Remove |
| F4-5 | Table edit | Tables ▸ `files/item_parameter.pak.lz` ▸ weapons ▸ row 1 (id 6001, aggressivity 10) ▸ set aggressivity to 99 ▸ Save ▸ Install | Equip that weapon; Attack on the status screen reflects it | Remove |
| F4-6 | Menu layout | Menus ▸ `files/MenuLayout_Root.xbn` ▸ move a frame by 8px ▸ Save ▸ Install | The main menu's element has moved | Remove |
| F4-7 | Exit | Maps ▸ `d01_00` ▸ select a door ▸ change its destination to `d01_02` ▸ Save ▸ Install | Walking through lands on the other map | Remove |
| F4-8 | Add a character | Maps ▸ `d01_00` ▸ Add character ▸ pick a model, position, and a line ▸ Save ▸ Install | The new NPC stands there and talks | Remove |
| F4-9 | Uninstall integrity | After any of the above, Remove, then Steam ▸ Properties ▸ Verify integrity | Steam finds 0 files to reacquire | - |

## FF3 (Steam)

Target **FF3 on Steam**. The chest test (Ultima Weapon in the first chest) already
passed; these are the rest.

| # | Case | Steps | Expect | Undo |
| --- | --- | --- | --- | --- |
| F3-1 | Text | Text ▸ `files/menu.msd` ▸ any visible menu word ▸ edit ▸ Save ▸ Install | Menu shows it | Remove |
| F3-2 | Table | Tables ▸ `files/item_parameter.pak` ▸ weapons ▸ Attack of the starting knife ▸ Save ▸ Install | Status screen shows the new Attack | Remove |
| F3-3 | Exit | Maps ▸ `d01_01` ▸ a door ▸ change destination ▸ Save ▸ Install | Door leads elsewhere | Remove |
| F3-4 | Delete a character | Maps ▸ `d01_01` ▸ select an NPC ▸ Delete ▸ Save ▸ Install | NPC gone, nothing else broken (talk to the others) | Remove |
| F3-5 | Sound | Audio ▸ any BGM ▸ play in the browser | Plays, loop point shown | - |
| F3-6 | Integrity | Remove everything, Verify integrity | 0 files | - |

## Both games, editor behaviour

| # | Case | Expect |
| --- | --- | --- |
| E-1 | File ▸ Open project switching FF3 ↔ FF4 | Script editor completion changes to the other game's instruction set without a reload (FF4 has `setInsideMapJump`, FF3 does not) |
| E-2 | Project with an FF4 target opened while FF3 install missing (rename the folder briefly) | A clear message naming the missing install, not a crash |
| E-3 | Install while the game is running | A clear refusal or a note that the game must be restarted; no half-written container |
| E-4 | Revert a file the game has since updated (touch the container) | Skipped with a reason; the file is left alone |
| E-5 | Two projects for the same game, install A then B | B's files replace A's; Remove leaves vanilla, not A |
| E-6 | Project targeting both games (Settings ▸ tick both) | Two tabs above the libraries; FF3 and FF4 badges on document tabs; `#/ff4steam/...` in the address bar |
| E-7 | Open a map from each game, drag one tab beside the other | Both render, each with its own textures (no 404s in the console) |
| E-8 | Edit a text line in FF4, then File ▸ Changes… | The FF4 tab in the dialog lists it; the FF3 tab is empty; Project ▸ Install — FF4 on Steam installs only that |
| E-9 | File ▸ Export as .zip… | A zip beside the projects folder with project.json, the per-target files, and a README; Explorer opens on it |
| E-10 | Models ▸ `n441` (FF3) - the motion bar picks `★ n441` and plays `n441_101_01` | The figure animates smoothly for 90 frames and loops; scrubbing the timeline and the arrow keys step frames; switching to a `b_f…` pack of another skeleton distorts it (expected - different bones) |
| E-11 | Models ▸ a monster, e.g. `b_m005`-family model with `b_f005.ncap` | The battle idle plays; compare against the game's battle screen for direction and speed |

## What "works" looks like in the log

The console pane shows every install as `wrote <file>` or `rebuilt <container> (<n>
entries, one replaced)`. A backup of anything replaced sits in
`<override>.backup`, and `installed.json` beside it lists every file with hashes.

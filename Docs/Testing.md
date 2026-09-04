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
| E-12 | Models ▸ `n441` with `n441_101_01` playing ▸ Export .glb ▸ open in Blender (File ▸ Import ▸ glTF 2.0) | The chocobo appears textured with an armature of 31 bones and an action of 90 frames that plays the same walk; UVs and vertex colours intact |
| E-13 | Maps ▸ any map (FF3 `d03_02`, FF4 `d01_03`) after a change to the model viewer's shader | Terrain, characters, the gizmo and the exit boxes all draw, not just the exit tags. The scene view shares the viewer's vertex shader, which now skins through `palette[mindex]`; the scene pins `mindex` to 0 and writes its matrix to `palette[0]`. A uniform that no longer exists fails silently in WebGL (no console error), so an empty view with tags is this regression |
| E-14 | Start `ff3content` with no project, both games installed | Two tabs above the libraries, FF3 and FF4, both live: the editor behaves as a project targeting every game. Open an FF3-only project (`Text`): the FF4 tab stays but greyed, its tooltip saying the project does not target it; tick FF4 in File ▸ Project settings… and it comes alive |

## The client

| # | Case | Steps | Expect |
| --- | --- | --- | --- |
| C-1 | Boot from Steam alone | `FF3.exe --content="C:\Program Files (x86)\Steam\steamapps\common\Final Fantasy III"` (nothing else; the log's `content:` line must name only the install) | Logos, then the title with the logo at the phone's size, the three menu words and the hand cursor; `steam cells: phone placement applied to title_gousei_new.NCER` in the log |
| C-2 | Play from Steam alone | New game from C-1: naming, the opening, the first battle, walk out of Ur, talk to an NPC, open the menu | Everything the archive build does, with no exception in the log; windows, HP bars, icons and the cursor sized as on the phone build; the about screen (title ▸ about) is blank rather than a crash |
| C-2b | Steam-only really is Steam-only | Rename `Project\Content` briefly, run C-1 and C-2 | Identical; nothing in the log mentions `Content\` |
| C-2c | Steam's own layouts | Add `--steam-cells-off` to C-1 | The title logo draws about a third too large and the copyright strip moves - the reason the table exists |
| C-3 | A project as a mod | `FF3.exe --project=<your project>` after the chest test | The Ultima Weapon chest edit is live without installing anything; the log lists the override in use |
| C-4 | Mod folder | Copy one edited file into a folder mirroring `files/...`, `FF3.exe --mod=<folder>` | The edit is live |
| C-5 | TrueType text | `FF3.exe --content="<Steam FF3>" --size=1600x960`, New Game, read the opening dialogue; then the same with `--text=atlas` | Sharp Arial at window resolution vs the blurry 16px atlas; the text sits in the same place in both; menus line up |
| C-6 | A face of your own | `--font=C:\Windows\Fonts\georgia.ttf` | Dialogue in Georgia; widths still consistent (right-aligned numbers in menus stay aligned) |
| C-7 | Steam's sound | Boot from Steam with `--log=file`; listen through the title and the opening | Music and effects play; the log lists `sound: sound/BGM00_1.ogg -> 43.4s` and friends, no `Content.Load` of a sound XNB; the intro (`_0`) hands over to the loop (`_1`) without a gap |
| C-8 | Regenerating the layout table | `python Tools/gen_steam_cells.py <extracted files/> "<Steam FF3>"` | `29 cell banks`; only `mastercard.NCER` skipped; the json under `FF3.Game/Data` is unchanged (git shows no diff) |
| D-1 | FF4 boots into Baron | `FF3.exe --content="C:\Program Files (x86)\Steam\steamapps\common\Final Fantasy IV" --size=1600x960` | Within ~10 s: Baron castle's grounds (stone platform, moat, bridge) with Cecil in dark-knight armour and his shadow, at the world-map arrival; the castle theme plays; the log says `content: loose files + mass files: 11361 files ...; then synthesised movement tables (2 files)` and `script: FF4 table built - 200 of 500 commands` |
| D-2 | Any map, any game | `--map=d01_01` on FF4 (castle interior), `--map=t01_01` on FF3 (Ur), `--map=d01_01 --pos=0,0,10 --rot=90` | The named map with the party at the position; for FF3 the game plays normally from there (NPCs talk, exits work); for FF4 walking works, NPCs stand where the .hich puts them |
| D-3 | The porting list | Boot FF4 with `--log=general,file --log-file=ff4.log`, walk around, read the log | `missing:` lines name FF3 files FF4 has no equivalent for yet (2D banks, `WorldDefine.xbn`, `eureka_*.msd`); `script: FF4 command N ... not implemented` lines name the commands the map used; no `Exception` lines |
| D-4 | Probe | Add `--probe` | Every 3 s a `probe:` line with vertices > 0, `cameraMtx` not all zero, `bounds` a few hundred units wide; `--noscript` keeps the map, `--noanim` keeps it too |
| D-6b | Exits follow the script | Boot FF4 with `--log=general`; watch for `exits: d01_00 declares j001 -> f00 (1 now)` lines as the map's script runs, not one summary at load; then edit a map script in Crystal so a `setInsideMapJump` sits behind a flag test and install it | The exit appears in the log only when the flag branch runs; the door does nothing until then |
| D-6 | Walking between FF4 maps | Boot FF4 (D-1), walk north along the path to the castle gate (touch-drag or WASD), through the arch | The screen fades and Cecil arrives inside Baron castle (`d01_01`) at the door; the log says `exits: j002 -> d01_01 at (216,24,-213) facing 180` then `exits: d01_01 has 4 scripted exit(s)`; walking back out returns to the grounds; `--map=d01_01` alone starts at the same door |
| D-7 | Tapping an NPC | Boot FF4 with `--probe`, walk up to an NPC in the open and tap them | The log shows `touch: (x,y) vs <model> at (...) r=8 HIT`; when the leader is touching them a talk starts (FF3 mechanics: message text through TrueType, no window art yet for FF4) |
| D-8 | An event map | `--map=e01_00` on FF4 with `--log=general` | The Red Wings deck renders and stays up; the log lists the `ce_*` cutscene commands as `not implemented - skipped` and no `Exception` |
| D-9 | FF4 dialogue | `--map=t01_00 --pos=0,0,72 --say=581000,100500` on FF4 | After ~4 s a navy window with a white frame at the bottom reads "So, you're Cecil." in TrueType, with "Cecil" as the speaker's name; the same window and text appear when a townsperson is touched and tapped |
| D-10 | FF4 motions | Boot FF4, stand, then walk (drag or WASD) | Standing plays the idle (hand on hip), walking plays the walk cycle; no T-pose. FF4 numbers its motions 1000/1001/1002 where FF3 asks 1001/1004/1005; `GameProfile.FieldMotionId` renumbers them as a pack registers |
| D-11 | FF4 overworld | `--map=f00` on FF4; wait ~10 s; also `--map=f00 --pos=-280,0,-40` | Mountains, grass and sea with Cecil on the ground; by the castle position a settlement is in view; walking streams new chips in; the log's `jump:` line names a chip (`f00_67`), never bare `f00` |
| D-5 | FF3 unchanged | C-1, C-2 and the archive boot | Identical to before (the FF4 work is behind `GameProfile`, which reads the content's shape) |
| C-9 | World map and menus from Steam alone | From C-2 reach the world map; open the menu, status, items, config | The ship and map markers (`w_map_*`, `map_marker_*`), volume slider (`m008_volume`) and menu icons (`icon_8dot`, `icon_16dot`, `m015_bar`) are placed and sized as on the phone build |

## What "works" looks like in the log

The console pane shows every install as `wrote <file>` or `rebuilt <container> (<n>
entries, one replaced)`. A backup of anything replaced sits in
`<override>.backup`, and `installed.json` beside it lists every file with hashes.

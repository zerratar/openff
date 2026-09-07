# Crystal - the OpenFF editor

Crystal is the editor for Final Fantasy III and Final Fantasy IV (3D) content: the
shipped Steam games, and OpenFF, the client being built to run either. The executable
is still called `crystal`; the page, the start screen and the console say Crystal.

```bash
dotnet run --project Crystal.Editor -- editor --content=Content --text=..\text\en.lproj
```

Then open <http://localhost:5050/>.

It edits the game's content directly - nothing has to be extracted first, and nothing
is repacked afterwards. A file is read from the override directory if it is there and
from the shipped content if it is not, which is the same rule the game plays by, and
saving always writes to the override. So a change is live the next time the game
starts, and undoing one is deleting a file.

| Option | Default | |
| --- | --- | --- |
| `--project=<name>` | none | the mod to edit; made if it does not exist |
| `--target=ours\|oursff4\|steam\|ff4steam` | the project's own | which game to open first |
| `--content=<dir>` | `Content`, else a Steam install | a directory holding `data000.bin`, or a game install |
| `--override=<dir>` | see below | where edits are written |
| `--language=<code>` | `en` | which language the dialogue is read as |
| `--port=<n>` | 5050 | |
| `--no-browser` | | do not open the page |

With no `--content` it takes the `Content` directory if there is one, and otherwise
looks for a Steam copy of the game. `crystal installs` prints what it found. So
for somebody who just has the game, the whole of it is: run it - it finds the game,
serves the editor, and opens the page, because an editor that prints a URL and waits
is one whose UI most people never find.

## Projects

A project is one mod. It is the override directory with a name on it and a note of
which game it is for, which is what turns "my edits" into something you can hand to
somebody.

```
<projects>/<name>/project.json     what it is and what it targets
<projects>/<name>/files/...        the edited content, named as the game names it
<projects>/<name>/session.json     what was open in the editor last time (not exported)
<projects>/<name>.backup/          originals, once it has been installed
```

`<projects>` is `%LOCALAPPDATA%\FF3ContentTool\projects`, or `FF3_PROJECTS`.
`crystal projects` lists them. In the editor they are under **File**: new, open,
**Changes…** (everything edited, per game, with revert), **Project settings…** (name,
author, version, description, and which games), **Export as .zip…** and **Show project
folder**. The start page - what the document area shows with nothing open - has the same
things one click away. With no project it is about the projects: the list, New, Open. With
one open it is about that project - *Open project* over its name, its games, its actions -
and the other projects fold away under *Switch to another project…*, so the page never
reads as a question of which one is open. Every project listed carries the mark of what
it is: the OpenFF folder for an OpenFF mod, a game pad for a Steam mod, both for one that
is both. The page has an × and stays away for the session once closed (a line saying
*Nothing open* with a link back; **File ▸ Start page** too).

Opening a project puts back what was open in it: the tabs (not previews), which one was
focused, and which library and game the panel showed, from `session.json`, written a
moment after every change. A link in the address bar to something else opens that on top;
a file that has gone since is skipped. The file is the page's, not the mod's - the export
leaves it out.

A project that targets two games keeps their edits apart, because the two games name
their files alike (`files/d01_01.script` is a Baron corridor in one and Ur in the other):

```
<projects>/<name>/files/...                    the first target's edits (and a one-target project's)
<projects>/<name>/targets/<target>/files/...   every further target's edits
```

**Export as .zip** writes `<projects>/<name>-<version>.zip`: `project.json`, the files
per target in that same layout, and a `README.md` saying what the mod is, which game
each folder is for, and how to install it - with Crystal, or by hand for loose files.

### Targets

A project is one of two kinds of mod, for one game or both. The New project and Project
settings dialogs show that as a grid - the games down the side, the kinds across the top:

| | what it is | how a change is tested |
| --- | --- | --- |
| **OpenFF mod** (`ours`, `oursff4`) | a folder the OpenFF client loads from `mods/`; may carry C# code and scenes | **Project ▸ Export to OpenFF** or **Run in OpenFF**; nothing is installed into any game |
| **Steam mod** (`steam`, `ff4steam`) | the game's own files, replaced | **Project ▸ Install into the game**, which copies in and keeps the originals |

A target is a game and a kind; `Targets.GameOf` and `Targets.KindOf` read them apart, and
the labels say both: *FF3 in OpenFF*, *FF4 on Steam*. An OpenFF target opens the same
content as the Steam target of that game (the client plays the Steam installs, and a
`Content` folder of our own when there is one), so ticking both games under OpenFF is the
normal thing to do: the mod can then take a model from FF4 and a sound from FF3, and the
client applies each game's edits only when that game is played (`ff3/files/`, `ff4/files/`
in the export). Nothing stops a project from being both an OpenFF mod and a Steam mod of
the same game, but the two never share edits, because a Steam install and an OpenFF mod
are not the same place.

### Two games at once

A project with two targets opens both. The server keeps a *session* per target - a
workspace and the indexes built over it - and every request says which one it means
with `?ws=<target>` (`api()` and `wsUrl()` add it; nothing in the page builds an API URL
without them). The project panel gets a tab per **game** above the libraries - FF3, FF4 -
which is a filter over whose content the panel shows, no more; the kind appears under the
game's name only when the same game is open twice (an OpenFF and a Steam target at once).
Each document tab carries an FF3 or FF4 badge; the address bar becomes
`#/<target>/maps/d01_01`. Focusing a document makes its game current for the inspector,
the file list and the next thing opened. **Project ▸ Install / Remove** is per game, and
the *default game* under the same menu is only what the command line opens first. With
no project open, the panel shows every installed game once, live, as if a project
targeted all of them.

Targeting both is worth it for data - `.pak`, `.msd` and `.script` are largely byte
identical between the two releases - and wants care for art, which is authored against
a different virtual screen in each. See *Borrowing Steam's art* below.

A mod published to Nexus targets Steam, since that is the game other people have.

## FF4

The same engine shipped Final Fantasy IV (3D Remake) a year later, and the editor
opens it: `--target=ff4steam`, or point `--content` at the install. `crystal
installs` lists both games. The status line and `/api/status` say `game: ff4`.

What is different, and what the editor does about it:

| | FF3 | FF4 | |
| --- | --- | --- | --- |
| install layout | `files/` beside the exe | `EXTRACTED_DATA/files/` | resolved; point at the install either way |
| per-map script, hich, msd, pak | loose | bundled into `SSAM` mass files, `files/*.dat` | `SsamContentSource` exposes them under FF3's names, decompressed |
| `.msd` text | UTF-8 | UTF-16LE | decided per file from how a message ends |
| `.pak` records | 7 chains, `jumps` first | 4 fixed singletons: encount, landForm, monsterParty, environEffect | container reads; records stay raw. There is no jumps chain |
| exits | rows in the map's `.pak` | `setInsideMapJump` in the map's script | read from the script when the game is FF4 |
| script opcodes | 298 | 500; 222 at the same number, 48 of those with different operands | `ScriptOpsFf4.cs`, generated from `libff4.so`; every script decodes and rebuilds byte for byte |
| model display lists | DS GX commands | plus `0x2C`, a 16.16 texture coordinate | every command stepped by its arity; `0x2C` decoded |
| menu layouts | `<menulist>` of `<menu>`, 9 loose `.xbn` | `<layout>` of `<unit>`, 28 `MenuLayout_*.xbn` in `MENU_LAYOUT.dat` | same frames inside; the menu editor takes either root |
| sound | `BGMnn_0/1.xnb`, loop point in `sound/*.dat` | `files/SOUND/BGM\|SE\|VOICE/*.akb`: an `AKB ` header over Ogg Vorbis | listed from the header (2757 sounds; loop start is in it); the Ogg is served as it is |
| tables | `item_parameter.pak`, `monster.chaindata`, `player.chaindata` loose; 7-chain map `.pak` | the same three, LZ-compressed; 4-chain map `.pak` in `MAPPARAMETER.dat` | `PakRecordsFf4.cs`: FF4 families, so FF3's fields are never applied; see Tables below |

The mass file is `SSAM | count | offset0 | size0`, then 40 byte records of
`name[32] | offset | size`, data after the directory. Thirty of them: `CAST_SCRIPT.dat`
389 scripts, `CAST_HICH.dat` 388 placements, `CAST_EVENT_MSD.dat` 351 dialogue files,
`MAPPARAMETER.dat` 321 paks, `MENU_LAYOUT.dat` 34 menus, `EFFECT.dat` 534 effects.

Measured over the whole install through the editor: 388 of 388 maps, scenes and exit
tables; 389 of 389 scripts; 376 text files; 28 menus; 400 cell banks; 995 models -
no failures. Byte-exact round trips: 380 of 389 scripts, 361 of 362 text files, every
menu.

Known exception, read correctly and only failing to rewrite byte for byte:

- `babil_scenario.msd`: message 10006's offset overlaps the tail of 10005 in Square's
  own file, so recomputing offsets from the text cannot reproduce it.

### Scripts

FF4 runs FF3's script engine with its own command table - 500 entries to FF3's 298.
222 commands sit at the same number in both games, but 48 of those read different
operands (`bootEventBattle` takes three more bytes, `moveCamera` an extra word), and
255 are FF4's alone. So an FF4 script can only be decoded against FF4's table, and
FF3's table plus a hand-written overlay - the first attempt - left 89% of FF4's
bytecode as `op(N)` because functions began with instructions FF3 never had.

There is no FF4 source, but the Android build of the engine (`libff4.so` in the APK)
is unstripped: every handler is a named symbol, so are the operand accessors, and the
dispatch table is 500 relocations against the handler symbols. `Tools/gen_opcodes_ff4.py`
disassembles each handler and lists its calls to `getByte/getWord/getDword/getString`
in order - the same evidence `gen_opcodes.py` reads from FF3's C# - and writes
`ScriptOpsFf4.cs`. It needs `pip install capstone pyelftools` and the path to the `.so`;
the generated file is committed, so the build does not.

```bash
python Tools/gen_opcodes_ff4.py <apk>/lib/arm64-v8a/libff4.so
```

`ScriptOpTable` is the seam: `ScriptOpTable.Ff3`, `.Ff4`, `.For(game)`. A `Workspace`
hands out the right one as `Ops`; `ScriptFile.Read`, the source writer, the compiler
and the mnemonics all take it, and the CLI script commands accept `--game=ff4`. With it
all 389 FF4 scripts decompile with no `op(N)` and compile back to the exact bytes.
Mnemonics whose handler name starts with a digit keep the underscore that separated
it from the prefix - `_3DSSetup` - so they stay legal identifiers.

### Exits

FF4 has no exit table. Its `world::MapParameterManager` exposes encount, landForm,
monsterParty and environEffect and nothing else, and each map's `.pak` is those four as
one record apiece. A door is declared by the map's script instead:

```
setInsideMapJump("j002", "d01_01", ax, ay, az, facing, x1, y1, z1, x2, y2, z2);
```

trigger object, destination, arrival position (FX32), facing in eighths of a turn, and
the two corners of the door's trigger box on this map. Opcode 333 in FF4's table
(`ScriptOpsFf4.SetInsideMapJump`); its shape was first measured from the scripts - 658
instances across 369 maps, two strings then exactly ten S32s - and the handler's code
later confirmed it. `MapModel` reads them when the game is FF4 and puts the exit at
the box's centre.

### Tables

FF4's tables have FF3's file names and different records: 97 weapons of 88 bytes where
FF3's are 56, 251 monsters of 152 where FF3's are 100, a map `.pak` of four single
records (encount 52, landForm 42, monsterParty 16, environEffect 8) where FF3 has seven
chains. Applying FF3's layouts typed the wrong bytes and a save would have rewritten the
table wrongly, so `Pak.FamilyOf` takes the game and an FF4 workspace gets the FF4
families in `PakRecordsFf4.cs`. There is no FF4 source to name fields from, so each
name carries its evidence: **engine** (a getter in `libff4.so` reads that offset -
`itm::EquipParameter::aggressivity` is the word at 0x34, `ItemManager` finds an item by
the id at 2 in tables of stride 48/88/84/32, `MonsterManager` a monster by the id at 8
in a table of stride 152), **FF3** (same field, same offset, and FF4's values fit - a
Potion has buy 30 and price 15 where FF3 keeps them), or **stride** alone (an id that
counts up record after record). A field nobody has named is called by its offset,
`x1A`, so it is still a column and its name says how much is known. Two of Square's
chains are two bytes short of their last record (60 consumables, 252 monsters);
`Pak.Read` pads that record and `Pak.Write` trims it, and refuses any other layout that
does not divide its chain, for either game. All four FF4 table kinds rebuild byte for
byte. `Tools/ff4_fields.py <libff4.so> '<symbol regex>'` prints the loads a getter does,
which is how to name the next field.

### Motion

A model plays its motions under the viewer. Both games keep them as `.ncap.lz`: a pack
of joint animations (Nitro BCA0 inside) for one skeleton - `b_f<family>` for a monster
family, `w_<name>` for a field character, `w_event<nn>` for cutscenes, and the game
decides which pack goes with which model in code, so the editor offers every pack and
puts the ones that fit first: same node count as the model (★ when the name matches too).
Pick a pack and a motion; it plays at the game's 30 frames a second, loops, scrubs on
the timeline, steps with the arrow keys, pauses with space.

How it works: the model reader already knows which matrix moved each vertex - the node's
own, or the stack slot a `MTX_RESTORE` inside the display list switched to, which is how
an arm sits on its arm bone - and now says so per vertex (`matrixIndex`). For a motion,
`Ncap.cs` evaluates each node's scale, rotation and translation per frame exactly as the
game's SBC `NODEDESC` handler does (pivot rotations from `rot3`, packed 3x3s from `rot5`,
"base" channels keeping the model's own value), the SBC is walked again with those
matrices and no geometry (`Mdl0.Posed`), and the viewer gets, per frame and per matrix,
`animated × inverse(bind)` - so the same vertex buffer is skinned in the shader through a
small matrix palette. Nothing is re-uploaded per frame. `/api/model/motions` lists the
packs; `/api/model/pose` evaluates one motion for a model.

**Export .glb** (in the model's bar) writes the model as glTF 2.0 into the project's
`exports/` folder and opens Explorer on it: the mesh with its textures embedded and
vertex colours, a skin whose joints are the model's matrix instances (bind pose is the
mesh as it is, so every inverse bind matrix is identity), and - if a motion is playing -
that motion as an animation, one translation/rotation/scale key per frame at 30 fps.
Blender's glTF importer opens it with the armature and the action. This is the way out
for anyone who wants to look at, measure or retexture an asset in a real modelling tool;
the way back in - a mesh to NDS display lists, a motion to packed joint tables - is not
built yet.

### Models

The display-list walk used to end a shape at the first GX command it had no use for.
Right for FF3, whose models never put one mid-list; wrong for FF4, which does. It now
steps every command by its parameter count and only stops on a byte that is not a GX
command at all - and FF4 has one of those too: `0x2C`, two words, a texture coordinate
in 16.16 fixed point for textures too large for `TEXCOORD`'s 12.4. Eleven models used
it and lost most of their geometry: `t01_00`, `t04_00`, the `d17` dungeon, `b17`,
`e19_00`, `e22_00`.

Whether a model is complete is measured against its own header - emitted faces against
declared. FF4: 995 models, 1 436 131 faces declared, 1 436 131 emitted. FF3: 832 of 833
complete; `d10_13.nmdp.lz` is a 161 byte stub whose header claims 64 812 vertices, and
there is nothing in it to read. A model's `notes` in `/api/model` say what, if anything,
the reader stepped over.

### Installing into a mass file

An FF4 edit to a script, a placement file, a map's dialogue or its `.pak` goes back
inside its container, so **Install into the game** rebuilds the container. The rule it
is rebuilt by was measured against every entry of every container FF4 ships: entries in
directory order, each starting at `roundup(size + 1, 32)` after the last, the first at
offset 0, whatever trails the last entry kept as it was. With no replacements that
reproduces all nineteen containers the editor writes to byte for byte.

The container is the unit. Its pristine copy is taken once, into the same backup
directory as loose files; every install rebuilds from that copy plus everything the
project has recorded against the container, compressing an entry the way it was
compressed; uninstall puts the pristine copy back; reverting one edit rebuilds from the
copy plus the others that remain. A container whose bytes are neither the shipped ones
nor the ones we wrote is left alone and said so - the game updated it.

FF4 lays its containers out two ways and the repacker reads which off the container:
nineteen use a stride of 32 with the first entry at offset 0, six - `EFFECT`, `FACE`,
`BTL_CAMERA`, `EVT_CAMERA`, `MOTION_MENU`, `SIGHTRO`, `DEBUGJUMP` - a stride of 512 with
the first at 504. With no replacements, all twenty-five come back byte for byte.

Four more - `NAVIMAP`, `STAGEMNG_D`, `STAGEMNG_T`, `battle_map` - are not containers at
all any more. Each is exactly its header and records, with offsets running to tens of
megabytes into a payload that is not there: an index left over from the Android build.
Every one of their 1 934 entries ships as a loose file in Steam's `files/`, which is
where the editor already reads them. The reader skips the four, and nothing is lost.

`Tools/test_mod_ssam.py` runs the whole cycle against a copy of FF4's own
`CAST_SCRIPT.dat`: install, reinstall, uninstall, revert one of two, and a container
changed since.

## Two content layouts

`--content` accepts either shape, and everything above this line reads the same
through both:

- **Archives** - `data000.bin` and its numbered blobs. Our own build and the phone's.
- **Loose files** - a `files` directory next to the executable, plus `sound` and any
  `.lproj` folders. This is how the Steam release ships, already unpacked, because a
  desktop install has no reason to pack it.

Which one it opened is on the status line and in `/api/status` as `content`.

```bash
dotnet run --project Crystal.Editor -- editor ^
  "--content=C:\Program Files (x86)\Steam\steamapps\common\Final Fantasy III" ^
  --override=..\SteamMods --port=5051
```

Edits never go into a game install by accident. `--override` defaults to
`Content/Override` for our own build, which is where that game reads them from, and
to `%LOCALAPPDATA%\FF3ContentTool\mods\<install>` for anything else.

Not Documents, which was the first choice for being easier to find: Windows protects
it with Controlled Folder Access, on by default on plenty of machines, and creating a
folder there fails with a "could not find file" naming the folder it is creating.
Local application data is never protected and never needs administrator rights.

## Installing a mod into a Steam install

Our own build reads the override directory itself, so a change is live the next time
the game starts. The Steam executable has no such idea - it reads `files/` and that
is all - so for that one the edits have to be copied over the originals.

The header grows two buttons when the content is a loose install, and the same thing
is on the command line:

```bash
dotnet run --project Crystal.Editor -- install      # edits -> the game
dotnet run --project Crystal.Editor -- uninstall    # the originals back
```

The override directory stays the master copy either way. That is what makes a mod a
folder you can zip and put on Nexus, and it means Steam validating its own files
costs the install rather than the work.

It is built to be undone:

- the original of every file it overwrites is copied out first, **once**, to
  `<override>.backup`. A file already backed up is never backed up again - the first
  copy is the pristine one, and a second install must not replace it with modded
  bytes;
- what was written is recorded by hash, and uninstall only restores a file that still
  holds exactly those bytes. If the game has been updated or verified since, that file
  is left alone and said so, rather than a stale original going back over a newer one;
- a file the mod **adds**, that the install did not have, is recorded as new and
  removed on uninstall rather than restored. This one was wrong first time round:
  after one install the added file exists, so a second install decided it had replaced
  an original and uninstall then left it behind.

`Tools/test_mod_install.py` exercises all of that against a fake install, so no real
game is touched by the test.

### Two facings, opposite ways round

A character's facing and an exit's arrival facing are the same idea in two formats,
and they are applied with opposite signs. Worth knowing before trusting either.

A `.hich` row keeps a posture in **degrees**, and the game negates it:

```
vecFx2.set(..., 4096 * FX_DEG_TO_IDX(m_Posture[1]) * -1, ...);
```

sitting between a position and a scale that both pass straight through. An exit keeps
a **16 bit angle where a whole turn is 65536**, and `setupMapJumpPosition` uses it as
it stands:

```
vecFx2.y = MapJumpParameter(id).PlRot();
```

So the same number turns opposite ways depending on which of the two it came from.
The 3D view drew both the same way, which meant a character placed facing right in
the editor faced left in the game - found by putting a chest in d01_05, installing it
into a Steam copy and opening it. `facingSign` in `map-scene.js` is that difference,
and it applies to the model, its facing arrow, the picking bounds and the drag to
turn, which all have to agree or the gizmo fights the model.

### Undoing one file

**File ▸ Changes…** lists everything the project holds, with a checkbox each and a
select all, and reverts whichever are ticked.

Reverting is two things at once, and it matters that it is both: the edit is deleted
from the project, **and** if that edit had been installed, the game's own file goes
back at the same moment. Undoing only the first half is the trap - the file would then
read as shipped everywhere except in the game, which is the one place it matters. The
per-file Revert button in each editor goes through the same path for the same reason.

The rule about a game updated since install still holds: such a file is left alone and
the edit kept, rather than a stale original going back over a newer one.
`Tools/test_revert.py` covers all of it.

## Giving it to somebody who does not have the SDK

```bash
dotnet publish Crystal -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

One 66 MB `crystal.exe` and the `wwwroot` beside it. Run it with no arguments: it
finds the Steam install, puts the mod directory in local application data, opens on
<http://localhost:5050/>, and never writes into the game until somebody presses the
button.

Published as a single file, `Assembly.Location` is the empty string, so the web root
comes from `AppContext.BaseDirectory` - otherwise the editor looks for its own pages
in the root of the drive and serves nothing.

### What is the same, and what is not

The two releases ship the same formats. Of the 4647 files they have in common, 2918
are byte for byte identical - every `.msd`, every `.efp`, `.wbc`, `.shp`, `.rmg`,
`.area`, and 344 of the 345 `.pak` files. So the maps, the exits, the dialogue and
the scripts are the same data, and the reference index, the exit editor and the
collision reader work on either without knowing which they have.

What differs is presentation and revisions:

| | Ours | Steam |
| --- | --- | --- |
| textures | 800x480, mostly 8-bit indexed PNG | up to 1496x720, 58 of them truecolour |
| models | | 371 of 833 identical; the rest carry more geometry, not bigger textures |
| `.msd` location | one folder per language, `en.lproj/` | one language, straight in `files/` |
| menus | 7 of 9 `.xbn` differ - laid out for a phone | laid out for a desktop |
| text | 4110 messages | 4372 |
| fonts | `Font{size}.glp` plus atlas pages | TrueType, rendered at run time |

The `.NCGR` and `.NCBR` extensions are vestigial in both: the files are PNGs, named
after the DS formats they replaced. That is why the higher colour depths cost nothing
here - the browser decodes them and the editor only moves the bytes.

## Borrowing Steam's art for our build

Tempting, and mostly not possible as a file copy. Measured over the 232 textures the
two releases share:

| | |
| --- | --- |
| byte identical | 57 |
| same size, different bytes | 19 |
| bigger on Steam | 156 |

The middle group looks like the easy win and is not one. Comparing them pixel by
pixel: `menu_bg_01` has 253 colours in ours and 244 in Steam's, and a mean channel
difference under 1 - the same picture stored as RGBA rather than a palette. `ope_00`
has 776 colours in ours and 268 in Steam's, so ours is the better one. And
`icon_8dot`, `icon_yubi`, `w_map_mark` and `pad` differ by a mean of 88 to 204 per
channel, which is not a quality difference at all - they are different pictures, the
button prompts a desktop build needs instead of a phone's.

All the real quality is in the third group, and that group cannot be copied, because
in this engine resolution and layout are the same number. A cell part carries one
width and height, used both for the rectangle it draws into and for how much of the
sheet it reads; `G3_TexCoord` turns the source pixels into UVs with
`texScaleU = 1f / sizeW` off the decoded image, so the sheet may be any size, but the
part has to name its own extent in sheet pixels. Bigger art therefore means bigger
numbers in the layout, which means a bigger virtual screen. Steam's is about 2.22x
ours.

Tried, rather than reasoned about. Dropping Steam's `title_gousei_new` pair - the
1280x1109 sheet and the cell bank that goes with it - into `Content/Override` and
starting the game: it decodes without complaint, the bank parses, nothing errors, and
the logo draws 2.22x too large, cropped to "AL FANTAS" across an 800 wide window. The
data is completely compatible. The coordinate space is not.

Nor is it a per asset fix. Ours puts the logo at `(-288,-240) 576x272` and Steam at
`(-640,-534) 1280x608`, but the strip under it goes `(-208,212) 416x16` to
`(-368,472) 736x36` - 2.22x down, 1.77x across. The art was laid out again, not
scaled.

What would work is the render side: draw a bank at the numbers it was authored with
and scale that plane by `ours / theirs`. That is the shape of the fix already in
`DrawStringStart`, which scales text by `view.Width / 800f` so it tracks a resized
window. Doing it per cell bank would let both coordinate spaces coexist, which is what
mixing the two releases' art needs.

Models are the exception worth knowing: 371 of 833 `.nmdp.lz` are byte identical, and
where they differ it is geometry rather than textures - `t17_01` goes from 8875
vertices in 1 node to 11843 in 38, and `b42` from 304 to 540. A model package carries
its own UVs, so it has no screen coordinates to disagree about. Steam's `t19_01`
renders correctly in the editor's 3D view with our textures and with the exit gizmos
still in place. That path is untested in the game itself, which parses models through
the decompiled `NNS_G3d` code rather than through `OpenFF.Formats`.

The font preview is the one thing that does not carry over. It reads `Font{size}.glp`
and the atlas pages beside it, and a Steam install has neither - it ships `arial.ttf`,
`TBUDRGoStd-Bold.otf` and friends and rasterises them as it goes. Menu text there
falls back to the browser's own font.

The server binds to localhost, has no authentication, and is meant to be run by the
person editing their own copy of the game. It is not a service.

## The workbench

Four panels round a document area, the way a scene editor is laid out:

- **Hierarchy**, left - what is inside the thing you have open. A map lists its terrain,
  its characters, its logic casts and its exits; a script lists its declarations with
  line numbers; a model lists its parts. Clicking a row selects it, and for a map that
  also takes the view to it.
- **Documents**, middle - a tab per open asset. A pane is built once and then kept,
  hidden rather than thrown away when you switch, so a half-typed script is still there
  when you come back to it.

  Middle click closes a tab. A single click opens a **preview** tab: italic, one per group, replaced by the next
  thing you click. It becomes a real tab as soon as you change something in it, double
  click the tab, or double click it in the project list - so skimming twenty files
  leaves one tab behind rather than twenty.

  Tabs **drag**. Onto another tab bar moves them; onto the right edge of the document
  area splits the view in two, which is how you read a script beside its map. Both
  halves stay live, but only one is focused, and the hierarchy and inspector follow
  that one. The bar between the halves drags, so the split does not have to stay even -
  a script usually wants more room than the map beside it.
- **Inspector**, right - facts about the open asset even with nothing selected, and the
  details of whatever is selected underneath. There is exactly one of these, and every
  view uses it: a menu widget's properties, a model part's material, a texture's format,
  a cell's parts, a character's cast. A view holds the thing itself and nothing else.
- **Project and Console**, bottom - the libraries as a tree with their files beside
  them, and a running record of everything the status line has said. The files show
  either one per line or as a wrapped grid of icons; which one is remembered. The four
  bars between the panels drag - the three round the document area and the one between
  the tree and its file list - and their sizes are remembered too.

  The tree ends with the **OpenFF mod** folder, pinned to the bottom so it never scrolls
  away: the project's own things, as against the games' libraries above it. It has two
  rows - **Code**, the C# project's files (`code/`, with the `.csproj` and anything the
  build leaves out of `bin/` and `obj/`), and **Scenes**, the maps the project has put
  behaviours or points on (`scenes/<map>.json`). Both list like any library: click to
  inspect, double-click to open. A code file opens in a text pane with line numbers,
  colouring, Tab/Shift+Tab, Ctrl+S, and an unsaved mark; line endings are kept as found.
  A scene opens the **map editor** on that map - the 3D view, its characters and exits,
  the behaviours and points, everything a map has - because a scene is a map with extras,
  not a file of its own; the raw JSON is one click away in the inspector (*Open as JSON*).
  Maps that have a scene carry a small mark in the Maps library too. Above the list a
  strip of actions follows the row: on Code, **Add C# code** until there is some, then
  **New file…**, **Build** and **Open in IDE** (whatever opens `.csproj` on the machine),
  with a build's errors listed under it, each a click to the line; on Scenes, **Export to
  OpenFF** and **Run in OpenFF**; **Folder** on both. A Steam-only project sees *Make it
  an OpenFF mod…* instead, which is Project settings - the OpenFF path is the same editor
  with more in it, not a different one.

A model's inspector lists its **materials**, each with the picture it is painted with and
its tint and alpha. Seeing the texture beside the material is how a wrongly coloured
character gives itself away - the atlas is blue and the model is not.

Along the very bottom is a **status line** showing the last thing the editor said, so
the answer to "did that compile" is in front of you rather than one tab away. Clicking
it opens the console at that row with the row highlighted. The header keeps the
workspace summary, which is true all session and would only be wiped by the next thing
that happened if it shared a line with the messages.

**Ctrl+Z** undoes, **Ctrl+Y** or **Ctrl+Shift+Z** redoes. The stack is per document, so
undoing on a map cannot reach into a script, and views record their own steps - a step
being a label and the two functions that put things back and forward - which keeps the
stack out of the business of knowing what a map is. Moving a character counts, whether
it was moved with the gizmo, dragged on the plan, or typed into the inspector.

Fields and text areas are deliberately left alone: the browser's own undo is better
inside one than anything this could do, and taking Ctrl+Z off a half-typed line would be
worse than not having it.

Models get **thumbnails**, drawn rather than stored: one hidden canvas renders each in
turn and the result is kept for the session. Three things stop that being a nuisance -
they are drawn one at a time, only for cells actually on screen, and only in the grid
view. Pictures and textures need no drawing, being pictures already.

Choosing a model goes through a **picker**: the same 145 models as a wrapped grid of
thumbnails with a filter over it. A dropdown of names cannot tell you which of `o043`
and `o001` is a treasure chest, and this can.

Every asset kind has a mark of its own, and so does every kind of thing inside one - a
character, a logic cast, an exit, a mesh part. They are inline SVG, drawn in
`currentColor` so one copy of each serves a dim row, a bright one and a selected one
without a second file. `wwwroot/icons.js`.

## Maps in 2D and 3D

**3D** is what opens first, and it is the same map as the game builds it - the
terrain model with every character standing on it, wearing the model its `.hich` row
names and facing the way that row says. **2D** is the plan it always was: pins you can
drag, which is still the right thing for moving somebody two steps left.

Left-drag orbits, middle-drag or shift-drag pans, the wheel zooms, and a click picks.
Clicking tests against the model's own size rather than the spot it stands on - a `.hich`
position is where something's feet are, and aiming at the ground under somebody instead
of at them was a miss that got worse the flatter the camera angle. Where two overlap, the
nearer one wins.
Holding the **right button flies**, the way a scene view usually does: the mouse looks
around from where the camera already is rather than swinging it round a target, and WASD
walks it, with Q and E for down and up and shift to hurry. Those keys are only listened
for while the button is held, so W and E stay free to swap the gizmo the rest of the time. Picking in
the scene and clicking in the hierarchy are the same selection, and editing a position in
the inspector moves the character in whichever view is showing.

Whatever is selected gets a **gizmo**, on by default and switched off from the bar.
**W** moves, **E** turns and **F** frames whatever is selected, the same keys a scene
editor usually uses. F works from either end - the hierarchy and the scene are one
selection - and how far back it stands comes from the model's own size, so a villager
fills the view and a building does not fall out of it. On the terrain row it frames the
whole map instead. There is no scale gizmo because there is no scale to edit: a `.hich` row holds a
position and one angle, and its scale is 1 in 2063 of 2067 rows.

The rotate gizmo is a single ring in the ground plane, because facing is the one angle
these files hold. Dragging it works off where the cursor lands on that plane rather than
where it is on screen, so the ring turns with the mouse from any camera angle, and a
short arrow shows which way the character is facing without going to the inspector for it.

The move gizmo is three arrows, one per axis.

With the gizmo on, the map's **exits** show as small tags floating over the scene, each
naming the map it leads to with a stalk down to the spot. They are elements in the page
rather than geometry in the scene, which is deliberate: a label stays the same size
however far out the camera is, where a marker built out of triangles does not. Clicking
one selects it and the inspector says where it goes and which way you arrive facing.

The **slider** beside the gizmo toggle sets how big the arrows are, and is remembered.
Their size is measured from the eye to the thing itself rather than from the camera's
orbit distance - those are only the same for whatever the camera happens to be focused
on, which is why a gizmo out at the edge of a zoomed-out map used to come out as big as
the room and then shrink the moment you focused it. It now measures the same on screen
at any zoom.

Exits are the only thing on a map that is a place rather than an object. The `cameras`
chain beside them in the `.pak` looks positional and is not - its numbers are offsets
from whatever the camera is following, so there is nowhere to draw them. Their arrival
angle is kept in a different unit from everything else, too: a 16 bit angle where a whole
turn is 65536, where a `.hich` row uses plain degrees. Dragging one slides the character along that axis and
nowhere else, snapping to whole units because that is what a `.hich` row holds. The
arrows are geometry rather than lines: a line comes out one pixel wide however thick you
ask for it, which is not something you can reliably grab. Dragging is worked out in
screen space - the axis is projected, the mouse movement is projected onto it, and the
ratio says how far to go - so an axis seen nearly end-on simply stops responding instead
of sending the character into the distance. Moving something is a placement change like
any other, so **Save placement** is still what writes it.

The units needed no conversion, which was worth checking rather than assuming: a `.hich`
position is in the same units as the geometry - measured across 179 maps, every character
falls inside its terrain's own bounding box - and its posture is in degrees rather than
the fixed point the scripts use. `Editor/MapScene.cs`.

## The address bar

The tab and the open file are in the URL - `#/scripts/files/d04_02.script` - so a
refresh comes back to where you were rather than to the first tab, back and forward
work, and a link can be handed to somebody. The slug is the tab's own label lowercased,
read off the button rather than kept in a second list that could drift out of step.

Static files are served `no-store`. The editor gets rebuilt while it is open, and a
browser holding on to an old script shows bugs that are already fixed - or hides ones
that are not.

## Game data

The Tables library shows a file's records as the game stores them; **Game data** shows what
they mean. It lists one page per kind of thing the unified tables (`Shared/Data`, the
`OpenFF.Data` the client plays from) know about: Characters, Jobs (FF3), Spells, Items,
Monsters, Encounter groups, and for FF4 Shops and Efficacies. Each page is a read-only grid
with named columns and the game's own names - a job's growth curves and level 30 attributes,
a spell's MP cost and power, a monster's drops with item names, an encounter group's members
and placements, a shop row's wares and prices - built from the same files the Tables library
edits, through the workspace's own content chain, so a saved override shows up on the next
open. The note above the grid says which file and offsets the page reads; a `*` after a
name marks one the reader guessed because no text file named it. Filter rows with the box,
sort by clicking a column, click again to flip. `/api/list?kind=data` lists the pages,
`/api/data?name=<page>` serves one (`Editor/GameData.cs`).

## Maps

A plan view of a map: everything that stands on it, drawn at the position it stands
at. x across, z down, which is what the game's coordinates mean.

- **Green** pins talk, **blue** pins do not, **orange** squares are exits, labelled
  with the map they lead to.
- **Click** one and the inspector shows what it is: its model, its cast number, how
  much code that cast has, its position and facing, **every line it says**, and a link
  that opens the script at its cast. The lines are found by walking the bytecode from
  the cast's own entry points - through jumps, and through `call(0, id)` into this
  file's functions, which is where FF4 casts keep their dialogue behind a one-line
  main - so they are the lines that cast can actually reach, in either game.
- **Drag** to move it. *Save placement* writes the map's `.hich`.
- **logic-only casts** shows the entries that have no position - casts that run the
  map itself rather than standing in it.
- **Add character** puts a new one on the map. Pick a model, a spot and a line, and
  it does all four edits at once - see below.

This is the view that joins the others up. An NPC is not a row in any one file: it is
a `.hich` entry saying which model stands where and which cast drives it, plus that
cast in the map's `.script`, plus the messages that cast shows. The map view puts
those three together, which is the difference between browsing tables and editing a
game.

One thing it cannot know: a script can override a character's position when it boots
it with `bootCharacter_AbsoluteCoordination`, and where that happens, moving the pin
will not move the character. Those coordinates are also fixed point while `.hich`
positions are whole units - the two are not the same numbers.

### Adding a character

Four edits in three formats, which is why it is a button rather than a manual job:

1. a `.hich` row - the model, where it stands, and a free cast number
2. `bootPlainCharacter <cast>, 0, "<model>"` in the script, next to the ones already
   there, because a `.hich` row on its own places nothing - something has to boot from
   it, and every shipped map does that once per character
3. a cast with that number, holding the talk sequence
4. the line, in **every** language that has this map's text, under one id - the script
   names one number and the game picks the file for the language it runs in

The talk sequence is copied from a real NPC rather than invented:

```
cast66_main:
    call 2, 0xB6744D73        // turn to the player and begin
    startMessageWindow 0
    startMessage2 0, 0x1CA1206, 0, 0
    deleteMessageWindow 0
    call 2, 0xC39DEA76        // end the conversation
    end
```

Those two calls into `global.script` appear 1291 and 1185 times across the game.

Only models the map already loads are offered. A model the map has never loaded would
need the map's model data changed too, and that is graphics.

### The mod's own objects (OpenFF)

Everything above edits the game's data: a character is a `.hich` row, a boot call and a
cast, and a chest is a treasure command in a script. An OpenFF mod can also have objects
of its **own**, which owe the game nothing. **Add a game object** on an OpenFF project
offers *OpenFF object - the mod's own, no script* first: a name, a model or *no model*,
a spot. It goes into `scenes/<map>.json`, and the OpenFF client makes a `GameObject`
for it when the map is entered - the model standing there as a plain character, or
nothing drawn at all, for a spot that only holds logic (a trigger, a spawn point). The
terrain's inspector has *Add an OpenFF object where the camera looks* for the same.

They are listed as a tree under **Objects (OpenFF)** in the hierarchy, drawn in the 3D
view (the model itself, or a small blue box for one without) and moved with the same
gizmo as everything else. The hierarchy is where the tree is worked: **right-click** a row
for *New object* (a sibling right below it, at its spot), *New child object*, *Duplicate* (the subtree and its behaviours, under a fresh name),
*Rename*, *Focus in view*, *Move to top level* and *Delete*; right-click the group for
*New object*; and **drag** a row onto another to make it that one's child, or onto the
group to put it back at the top level - it keeps its place in the world either way. The inspector of one is laid out as Unity lays out a
GameObject, and it is all the file's: the **name** at the top (free to change -
behaviours on it and under it follow), then **Transform** (Position X Y Z, Rotation Y,
Scale), then **Model** (the picker, with a × to have none - a spot with logic on it),
**Tags** (what a mod finds it by), **Parent**, the **behaviours**, its **children** (links
down the tree), and *Delete*. A child's numbers are relative to its parent - turned
by the parent's yaw, scaled by its scale - so moving the parent moves the lot, and
reparenting keeps the child where it stands in the world. The document's own facts
(terrain, characters, exits) fold away under *About <map>* at the bottom while something
is selected: the selection is the panel.

The game's own things are inspected in the same shape, so the hand does not relearn the
panel between a Steam mod and an OpenFF one: a character is its icon and model name at
the top, **Transform** (its `.hich` position and facing), then Model, Cast, What it says,
Behaviour, Referred to by, and its OpenFF behaviours; an exit is its name, **Transform**
(the arrival spot - where the player appears coming in through it), Leads to, Doorway,
Conditions, Referred to by; the terrain is its model and behaviours. Each aspect is a card.

The game's own chests are edited in place too: a character whose cast the map's script
sets a treasure for (`setTreasureItem` / `setTreasureMoney`) gets a **Treasure** card -
an item from the game's list, or gil - and *Save treasure* rewrites that one line of the
script and compiles it. And on an OpenFF project every game character has an **OpenFF**
card with **Convert to OpenFF object**: it makes an object of the mod's own at the same
spot with the same model - a `Chest` with the same contents when it was a chest - and puts
a `Removed` behaviour on the original, which takes the game's one off the map when the
client enters it. The character's row reads *replaced* from then on; its cast stays in
the script for whatever else names it, and everything about the stand-in is the mod's.

Nothing on an OpenFF object has a Save button. A change - a field typed, an object
dragged, a behaviour added - writes `scenes/<map>.json` a moment later, the way the
session is kept; the status line says so. (The game's own rows keep their *Save
placement* / *Save exit*, since those write into the game's files.) A chest that gives an item is such an object with the chest's model
and the built-in **Chest** behaviour, its item picked from the game's list - change either
any time.

### Behaviours (OpenFF)

In an OpenFF project the inspector of a character, an exit, the terrain or one of the
mod's objects ends with **Behaviours (OpenFF)**: the mod's `Behaviour` classes attached to that object, each
a card with its public fields as inputs, and an **Add Behaviour** button - Unity's *Add
Component*. It drops a list with a search box: one row per class, its icon and name, the
summary as the tooltip; arrow keys and Enter pick, Escape closes. A class the source
declares but no build has seen yet is listed too, marked *not built*: it attaches now and
gets its fields after **Build**. The engine's own behaviours head the list under *Built
in*, code or no code: **Chest** (an item from the game's list, a count, gil; once, and
what it says), **Talk** (a speaker and lines, one per line), **Trigger** (`Radius`,
`Once`: says when the hero walks in - an event for the mod's code, and a line in the
log). A card shows a behaviour's public fields the way Unity does - `[Header]` groups,
`[Tooltip]` on hover, `[Range]` as a slider, an `[ItemField]` as the item list, a list of
strings as lines - and every change saves itself. Type a name nobody has written and the last row becomes
**New Behaviour "Name"** - it writes `code/Name.cs` from the Behaviour starter, attaches
it to the object, and opens the file. **Save behaviours** writes `scenes/<map>.json`.

The cog beside the project's name in the header is the mod's **GameService** - its entry
point, where `Start`, the map events and saving live. It opens the file when there is
one (`Mod.cs` as the starter writes it), writes the stub when the code has none, and
adds the C# project when there is no code at all. Whatever the page shows, the mod's
own code is that one click away.

## Scripts

The event bytecode, as source in the script language, with dialogue written in beside
the calls that show it. **Check** compiles without saving; **Compile & save** writes the
`.script` into the override.

Problems are listed underneath with their line and column, and clicking one jumps the
caret to it.

Because this is a language nobody has seen before, the editor tries to teach it while
you use it:

- **Highlighting.** Instructions the compiler knows are green; a word it does not know
  is red and underlined, so a typo shows before you press anything.
- **Completion.** Start typing an instruction and a list appears with its full
  signature. Arrow keys move, Enter or Tab accepts, Escape dismisses, Ctrl+Space asks
  for it. Filtering is on substring, so `magic` finds everything with magic in it.
- **Signature strip.** Under the editor, showing the instruction the caret is in with
  the argument you are on picked out - `bootCharacter_AbsoluteCoordination` reads
  `id:word, x:dword fixed, y:dword fixed, z:dword fixed`.

Keywords, conditions and instructions are coloured differently, and a name the
compiler does not know is underlined in red before you press anything.

The names come from the game's own handlers; `Docs/Script-Language.md` explains how,
and what "fixed" means.

## Menus

A canvas showing a menu screen laid out exactly as the game draws it - widget
positions come from the same `.xbn` data, with the nesting resolved the way
`MenuManager` resolves it, so a parent frame's position is added to its children's.

- **Drag** a widget to move it. Arrow keys nudge by one, shift+arrow by eight.
- **Select** one to edit its id, position, size and focus neighbours (`up`, `down`,
  `left`, `right`), or to edit that widget's raw XML.
- **Duplicate** clones the selected widget and everything nested in it. Give the copy
  a unique id.
- **Delete** removes it.
- **Zoom** from 100% to 400%. The default is 200%, because at 100% the labels of
  neighbouring widgets sit on top of each other.
- **XML** swaps the canvas for the whole file as text. *Apply to the canvas* parses it
  back; nothing is written until you press Save.

The screen picker lists every menu in the file - `MenuDefine.xbn` alone holds 29 of
them: the main menu, the item list, equipment, status and the rest.

### Preview

**Preview** replaces each widget's id with the text the game would draw there. A
widget with the `Text` behaviour names a message id, and that id is looked up in the
`.msd` files given by `--text`, so the main menu comes up reading Item, Magic,
Equipment, Status, Formation, Job, Config, Quicksave, Save.

Two things it cannot resolve, and says so rather than guessing:

- **`«Gold»`, `«CStatus»`, `«ItemList»`** - widgets filled from the game's own state.
  There is nothing to show ahead of time, so the behaviour is named instead.
- **`«msg 50419»`** - a message id with no text in the loaded language.

What preview still does not show is the *look*: window frames, fonts, colours and
icons come from graphics that are not decoded yet. The boxes are true to position,
size and wording - not to appearance.

## Text

Every message in a `.msd`, editable in place. Messages that are not valid UTF-8 are
marked `latin1` and are held byte for byte - see the encoding section of
`Docs/Text.md` before changing one.

## Images

All 542 pictures in the game, which are PNGs rather than the NDS formats their
extensions claim - see `Docs/Graphics.md`. Each shows at its real size on a
checkerboard, so transparency is obvious, with its dimensions, colour type and size.

**Replace…** takes a PNG from disk. It is checked before anything is written, and a
different size is allowed but called out, because the game lays some of these out
expecting a particular one. This is the only art that can be replaced today.

## Textures

The 3D textures, all 2828 of them, across 1589 packages. A package opens as a gallery -
most hold one to three textures, some a dozen - and clicking one shows it large with its
size, format, palette and a plain-English note on what the format is. **Save as PNG**
takes it out.

Unlike the Images tab, nothing here is served straight through: a texture lives inside a
TEX0 block, inside an NMDP package, inside an LZ-compressed archive entry, so each is
decoded on the way out. The listing is deliberately in two steps - naming the packages
is free, opening one costs a decompression - so it happens when you click, not up front.

A model with no textures of its own says so and offers to open the `.ntxp` that has
them. Textures are read-only; writing one back means re-quantising to a palette or to
4x4 blocks, which is not done yet. `Docs/Graphics.md`.

## Models

All 833 models, drawn in the browser - drag to orbit, wheel to zoom, **Recentre** to get
back if you lose it. Beside the view is what the model is made of: one row per part with
its shape, node and texture, and above it the decoded vertex and triangle counts set
against the counts the model records for itself, so you can see the decode agreeing with
the file rather than take it on trust.

Two models in the game have a part their node switches off, which the game never draws.
**hidden parts** shows those in red rather than pretending they are not there.

Drawing follows the game's own path: two passes with the translucent one second, nearest
filtering, repeat on both axes, and billboards that turn to face you. Hovering a part
says which of those apply to it.

There is no lighting, because the game does not light these either - what you see in FF3
is the texture and the material colour. A model that looks flat here looks flat in the
game. `Docs/Graphics.md`.

## Cells

The tables that say which piece of a sheet goes where - 187 cell banks, 3 screens and 51
animation banks. Each bank is composed against its sheet and drawn, so a window frame
shows up as a window frame rather than a list of coordinates, and beside it is every
part with its position, size, source and flags.

**as the game draws it** applies the 0.6 x 2/3 squash and the half-size flag the way the
game's own draw call does; turning it off shows the parts at their stored size.
**outlines** draws a box round each part, which is how you see that a menu is 345 of them.

A part that asks for more than its sheet has is marked in red. Fifteen do, all because
the art was replaced at a smaller size and the table left alone. `Docs/Graphics.md`.

## Audio

445 sounds - 30 music tracks and the rest effects - with their length, format, parts
and loop point, a player per part, the exact script line that plays them, and every
script that does. `Docs/Audio.md` explains how a number in a script becomes a file on
disk. Replacing a sound is not possible yet: they are XNBs rather than archive
entries, so the override directory does not reach them.

## Tables

Items, weapons, armour, spells, monsters, jobs and per-map data as a grid, one tab per
chain. Each chain says what it is for above the grid - a map's `jumps` is "where each
exit on this map leads: the position and facing the player arrives at, the map they
arrive on, and the flag that has to be set for the exit to work at all".

Field names are the game's own, tidied: `m_NextMapIndex` reads as `nextMapIndex`, and
a value read into a temporary buffer takes the name of the field it ends up in, so the
local called `array` in the map exit table is `nextMapName`.

Values are edited in place; an array field is edited as a comma separated list,
and turns red rather than saving if the list stops being the right length or stops
being numbers.

Two things are hidden or read only, deliberately:

- **Padding fields** (`_pad0`, `unnamed3`) are behind the *padding* checkbox. They are
  real and they round trip, but they are noise while editing.
- **`nameText` and `captionText`** are annotations from the message an id points at.
  They are read only, because saving writes the id and never the text - to rename an
  item, edit that message under **Text**.

The row filter matches anything in a record, so typing an item's name finds it.

## How it fits together

The browser does the editing; the server only decodes, compiles and saves, through
exactly the same code the command line uses. Menus are handled as XML in the browser,
because the DOM already knows how to parse and serialise it - the canvas is a view
over the same tree that gets posted back, so what you drag is what gets compiled.

That is also the limit worth knowing: there is no undo beyond **Revert**, which throws
away every change to that file, and nothing warns you about unsaved changes when you
switch files.

## Not in it yet

- **Reference picking.** A script names things by number: `startMessage2` takes a
  message id, a warp takes a map, an item command takes an item id. The editor knows
  what all of those numbers mean - it resolves them for the annotations already - but
  it will not yet let you go the other way and *choose* one. Picking a line of text and
  having the id filled in, or a map, or an item, is the obvious next step, and it wants
  a per-operand note saying which table an argument points into.

- **Maps drawn as maps** - models and textures both decode now, so the pieces are all
  there; what the map view still needs is to place them from the `.hich` data rather
  than drawing dots.
- **Menus drawn as menus** - the Menus tab still previews a layout as boxes. The window
  frames it would need are decoded now and sit in the Cells tab, so joining the two is
  the obvious next step. `Docs/Graphics.md`.
- **Writing textures back** - reading a TEX0 is done; writing one means re-quantising
  to a 256 colour palette or to 4x4 blocks.
- **Audio** - deliberately untouched.
- **New menus** - adding a widget means duplicating one.
- **New maps** - cloning one is plausible; authoring geometry is not, because models
  and collision are still opaque.

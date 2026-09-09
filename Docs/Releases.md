# Releases

What each release carries, and how one is made. The download is one zip,
`OpenFF-<version>-win-x64.zip`: unzip anywhere, run `OpenFF.exe` to play, `crystal.exe` to
edit and make mods. It needs the Steam copies of the games on the machine (Final Fantasy III
and/or Final Fantasy IV, the 3D remakes); none of their data is in the zip or in this
repository. Windows 10/11, x64; the .NET runtime is inside, nothing to install. How a release
is made is in `Docs/Releasing.md`; each version's section below is its release's description.

## 0.1.2 - weapons of the mod's own (in progress)

- **A glTF in the hand (OpenFF target).** A weapon definition's `"model": "assets/blade.glb"`
  is drawn in the hero's hand in battle in place of the game's `w###` model, at
  `"modelScale"`, with `"modelClip"` looping an animation of the file while it is held. The
  game's weapon character stays - loaded, posed from the hand joint, hidden, shown and faded
  as before - and only its draw is the glTF's (`CRenderObject.StandIn`,
  `OpenFF/Compat/WeaponMeshes.cs`), so the file sits exactly where a `w###` would: grip at
  the origin, blade along +Z. Crystal's item inspector has the *Look* card for weapons - the
  project's glTFs as pictures, *Import a model…*, *View*, the scale and the clip. Drives can
  set a fight up: `item <id> [count]`, `equip <member> <id>`, `battle <formation> [map]`.
- **A glTF as the game's own model (Steam target too).** The *Look* card's *Write as a w###
  model* converts the picked glTF into `w###.nmdp.lz` and `w###.ntxp.lz` - BMD0/MDL0 and its
  textures, the format the Steam games read - into the target's files under the first free
  number from 300, and sets the definition's *Model* to it; `crystal mdl-import file.glb w300`
  does the same at the command line. One node, a shape and a pal256 texture per material,
  triangles with normals and texture coordinates; skins and animations are not carried (the
  bind pose is). Read back by the reader, drawn by the model viewer, played by the client. On
  the way: the dictionary writer numbered its tree nodes in insertion order, which the game's
  lookup misreads for three or more names - fixed, which also mends *New texture package…*
  with three or more textures.

## 0.1.1 - game pads (2026-09-09)

- **Game pad support.** The first connected pad drives the game as the keyboard does: the
  d-pad or the left stick for the directions, A/B/X/Y (Cross/Circle/Square/Triangle on a
  PlayStation pad) for the DS's A/B/X/Y, the shoulders for L and R, Start (Options) and Back
  (Share/Create) for Start and Select, the right trigger held to run, the left trigger held to
  fast-forward. It works in every menu and on the field, in the title's MODS list, and mods
  read it through `Game.Input` with the keyboard. XInput pads, DualShock 4, DualSense and
  Switch Pro are known to SDL; an unknown one can be described in a `gamecontrollerdb.txt`
  beside the executable. Found in testing with a DualSense.
- **Running from the stick, and your own buttons.** `settings.json` ▸ `"run"`: `"stick"` (the
  default) runs when the left stick is pushed all the way and walks part way, as the phone's
  touch stick did; `"hold"` runs only while the run button is held. `settings.json` ▸ `"pad"`
  maps each DS button (`a`, `b`, `x`, `y`, `l`, `r`, `start`, `select`, plus `run` and `fast`)
  to a pad button by name - `cross`/`circle`/`square`/`triangle` (or `a`/`b`/`x`/`y`),
  `l1`/`r1`/`l2`/`r2` (or `lb`/`rb`/`lt`/`rt`), `l3`/`r3`, `options`, `share`, `touchpad`,
  `none`.
- **Display settings.** `%LocalAppData%\OpenFF\settings.json`, written with its defaults on
  the first start: `width`/`height` (1600×960 to begin with - the game's 800×480 space,
  doubled), `mode` (`windowed`, `borderless` - the desktop's size with no frame - or
  `fullscreen`), `msaa` (0, 2, 4 or 8; 4 to begin with) and `vsync`. A resized window is the
  size next time. The command line still takes a run: `--size=WxH`, `--fullscreen`,
  `--borderless`, `--windowed`, `--msaa=off|2|4|8`, `--novsync`.
- **Anti-aliasing.** The edges were stair-stepped because the sample count asked of the
  driver was never a real one; it is the setting's now, and 4× is on by default (the DS
  look - pixel textures, no filtering - is untouched; only polygon edges smooth).
- **Alt+Enter** switches windowed and full screen while playing and remembers the choice.
- **The client's own menu.** Esc, or Start (Options) on a pad, anywhere - the title too:
  Resume, Settings, Exit game. Settings holds what `settings.json` holds and applies it as
  it is changed: the window (windowed / borderless / full screen), its size, anti-aliasing
  (at the next start), vsync, how the stick runs, and the pad's buttons - pick a DS button,
  press the pad button that should be it; *Reset to the defaults* puts them back. Written to
  the file on the way out. Drawn like the mod list, with the game's font; the game goes on
  behind it. (Esc used to be a second B; X and Backspace still are.)
- **The name entry with a pad.** Up/Down move between the name and OK and A acts on them
  (touch only, before); with a pad connected the name field shows on-screen keys laid out as
  a console's - digits, qwerty, Shift / Space / Backspace / Done - the d-pad picks, A types,
  B deletes, X is a space, Y is Shift, Start is Done; the keyboard types as before. The
  client's own screens (this, the menu, the mod list) share one look now: the game's window
  blue, crisp TrueType, rounded plates, and hints as the pad's own button glyphs (Cross /
  Circle / Square / Triangle on a PlayStation pad, A / B / X / Y elsewhere).
- **The stick walks like the touch stick.** The left stick's direction is the hero's, at any
  angle, and its push the pace - the phone's touch stick's own path in the game, fed the
  stick's vector - where before it was read as eight keys and the hero snapped to one of
  eight ways. The eight-way reading stays for the menus. The game's Config > Movement (Walk /
  Run) is left out of the stick's decision - the push, or the run button, is the whole of it;
  the d-pad still follows the config as the DS did.
- **The pace follows the push.** With `"run": "stick"` the speed rises with the stick from a
  walk (up to 45% push) to the full run (from 95%), and the walk or run motion plays at the
  speed's ratio so the feet keep up - where before the hero stepped from walking to running
  at 80% (found in testing). The run button still means a full run at any push; `"hold"` is
  unchanged. Drives can hold the stick: `stick <x> <y> [ms]`.
- **The quick save notice** ("returning to the title") takes A or B as well as a tap.
- **Menu tabs with a pad.** The tab rows the touch build tapped - Items / Key Items, Config 1
  / Config 2, Magic's Use / Learn / Remove / Exchange - turn with X / Y (Square / Triangle,
  C / V) in every menu, and with L / R where those are not the character switch (Items,
  Config). The press goes through the menu's own tap handling, so the tab cursor moves to
  the tab and the list is re-focused, as a tap does. Up past Config's first option no longer
  crashes (an index off the explanation table). Found in testing.
- **Left and Right no longer close the main menu.** The menu-zoom button (L or R, by the
  config) closes it; the recreated condition had been `(edge != 0) & (button != 0)`, which
  any button satisfied - a pad's d-pad shut the menu.
- **Crystal's 3D view.** A click on nothing deselects; a pick in the view takes the
  inspector even when something from the project panel was in it (only the hierarchy did
  before); among the mod's objects the smallest under the cursor wins, so a crate on a
  ground slab is picked, not the slab; an item made in the project shows in the Chest's
  item picker at once.
- **Button glyphs as icons.** The client's screens draw the pad's buttons from geometry -
  Cross, Circle, Square, Triangle, the menu lines, Shift, Backspace, the arrows - crisp at
  any window size, no font glyph coverage involved; `settings.json` ▸ `"padStyle"`
  (`auto`, `ps`, `xbox`) or `--padstyle=` picks the set when the pad's name misleads.
- **Item definitions with pickers.** A record's fields are shown for what they are: a
  weapon's kind, its battle model (`w###`, with *View*), the jobs that may equip it, damage
  type, the status a hit inflicts and its chance, the spell it casts; armour's kind and what
  it guards against; a spell's school, level, element, status and targets - from the game's
  own enums, the base's value greyed in, ↺ back to it. Plain numbers carry a tip.
- **Saves were being lost - fixed.** The game read `save.bin` through .NET's IsolatedStorage,
  a store keyed by the executable's path, but created the file under `%AppData%\FF3` - so a
  build in any new folder never found a save file, its first write failed, and every save and
  quick save after it was thrown away (0.1.0's zip included; running from Visual Studio too).
  Both now use `%AppData%\FF3\save.bin`; a save left in an old isolated store is brought over
  once, automatically; FF4's card data is kept apart as `ff4-save.bin`. Found in testing.
- The release description carries only the version's notes (the procedure moved to
  `Docs/Releasing.md`). A settings screen in the game, or a launcher, is the plan for
  editing these without a text editor.

## 0.1.0 - the first release (2026-09-09)

**The client.** Final Fantasy III from its Steam install, title to credits, as shipped -
rendered natively on the desktop, TrueType text at the window's resolution, music and
effects from the Steam build's Ogg files. Final Fantasy IV through the same client behind a
`GameProfile` seam: Baron and the overworld, the opening scenes, battles on FF4's stages
with its HUD, the menu, shops, inns, saves, encounters - a work in progress; what is still
missing is in `Docs/Client-Plan.md`.

**Mods, with Crystal.** A `mods/` folder beside the client, one mod per subfolder, made in
Crystal (the editor, in your browser):

- *Replace anything of the game's*: maps and their characters, exits, encounters; scripts
  in a readable language; text in every language; menus; tables (items, monsters, jobs …);
  models, textures, pictures, sounds - each in its own editor, saved into the project, never
  into the game, exported as a mod.
- *Add what the game never had*: items, monsters, formations, characters and text of the
  mod's own (definitions the client adds to the game's tables); **maps of the mod's own** -
  a scene file with a glTF for the ground and the look, exits, music, a sky and a camera;
  **glTF models** from Blender, static, animated (skins and clips) and solid (ground and
  walls); **music and sound effects** as Ogg Vorbis or WAV; **a font** for all the game's
  text; C# behaviours and services on the engine's API (`OpenFF.Engine`), hot-reloaded while
  the game runs.
- *Built-in components* placed from the editor, Unity-style: Chest, Talk, Encounter, Trigger,
  Exit, Music, Sound, Mesh, Roam, Wander, Motion, GameCast, CastScript, WhenFlags,
  MapSettings - with an inspector, a hierarchy, undo, autosave and *Play here*.

`Modding.md` in the zip walks through it; `Docs/API.md` in the repository is the API
reference; `Docs/Testing.md` is what was tried.

**Known gaps.** FF4 is not finished (see the plan). A map of the mod's own has no name on
the menu and no map screen. Mods that must also install into the Steam games (the *steam*
target) keep to the game's own formats: glTF, Ogg and TTF are for the OpenFF target.
Sound import for a Steam target (an XNB writer) is not there. There is no Linux or macOS
build yet, though nothing in the client is Windows-only but the build.

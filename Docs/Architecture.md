# Architecture

How the port fits together, so the next change can be made with confidence rather
than by experiment.

## The frame

```
Program.Main
  ContentLocator  -> finds Content/, sets the working directory
  Game1           -> MonoGame Game

Game1.LoadContent   -> GameHost.Create()   -> GameArchive.Load()   (6962 files, 52 volumes)
                                            -> new MainActivity().onCreate()
Game1.Update        -> DesktopInput.Update()  -> MainActivity.onTouchEvent
                    -> DesktopInput.BeginFrame()  (sets GlobalScope.boost)
Game1.Draw          -> GameHost.Tick()        -> MainActivity.onDrawFrame()
                                                   touch(...)   feeds the touch state in
                                                   render()     runs the whole game frame
                                                   updateSound()
```

`onDrawFrame` runs the entire tick — input, logic and drawing. That is the original
NDS-derived design, not an artefact of the port: `Android.onUpdate()` was an empty
method, so nothing ever ran in the update half. `--speed` therefore has to run extra
`Tick()` calls, which is why it is documented as the crude option next to `boost`.

Before, this path went `Game1 -> Android (activity list) -> Activity -> View ->
GLSurfaceView -> Renderer -> MainActivity`, with the list never holding more than one
activity. `GameHost` replaces all of it.

## Input

Two surfaces, both fed from `Compat/DesktopInput.cs`:

**Touch** — mouse position is mapped from the window's client area into the fixed
800×480 space `MainActivity.onTouchEvent` normalises against, then delivered as a
`MotionEvent` (action 0 down, 1 up, 2 move).

**Pad** — `GlobalScope.cont` is an NDS button bitmask (A=1, B=2, Select=4, Start=8,
Right=16, Left=32, Up=64, Down=128, R=256, L=512, X=1024, Y=2048), read through
`PAD_Read()` into `ds.CPad`, which does edge detection and key repeat. Nearly 200 call
sites poll it. `MainActivity.getKeyEvent` ORs the keyboard into it once per frame.

Do not route keys through the old `Android.onKeyDown`: it only ever produced the Back
keycode, and an unhandled Back quit the game.

Running is not a separate button. `pl.isRun()` tests the B bit, and whether B means
run or walk depends on Config > movement type — so Shift aliases onto B, but only
while a direction is held, since B is also cancel.

## Rendering

`Compat/NativeRenderer.cs` owns the device. The GL emulation in
`GlobalScope.Members.cs` still holds the *state* the NDS renderer sets (matrices,
blend, depth, cull, bound texture); `NativeRenderer` intercepts the two calls that
touch the device — the draw and the clear — and does them MonoGame's way.

The critical detail is in `Docs/Porting-Notes.md`: the emulation builds a
GL-convention projection (clip z in `[-w, w]`) where MonoGame expects Direct3D's
`[0, w]`. `NativeRenderer` rebases it. Without that, every depth-tested draw fails and
3D renders black while 2D looks fine.

`--renderer=emulated` still selects the old path, for A/B only. It does not render 3D
correctly and is due for removal.

## Content

| Source | Contents | Read by |
| --- | --- | --- |
| `Content/data*.bin` | maps, sprites, models, scripts, tables | `Compat/GameArchive.cs` |
| `Content/*.xnb` | audio and font atlases | MonoGame ContentManager |
| `Content/*.glp` | glyph tables: char → (atlas page, shift class) | `GameFiles.ReadAllBytes` |

`GameArchive` reads through `Shared/Content/ContentChain.cs`, the same layer the
editor uses: override directories first (an editor project's edits via `--project`, mod
folders via `--mod`, the legacy `Content/Override`), then the shipped content in
whatever shape it came - our archives, a Steam FF3 install's loose `files/`, a Steam FF4
install's files plus `SSAM` mass files - then the rest of `--content`'s list, and last
`Data/defaults/` with the few tables OpenFF authors itself. Nothing is added on its own:
`--content=<Steam FF3 install>` boots the game people bought from that install alone
(what Steam does not ship, the 2D loaders treat as empty; what Steam laid out for its
own screens, `Data/ff3-steam-cells.json` re-places). The formats - archive, LZ, mass
files, the sources, the script command tables - live once in `Shared/`, compiled into
both the game and the editor. `Docs/Client-Plan.md` is where this goes next.

Names in the table are path qualified - `en.lproj/ca_text_01.NCGR`, `files/*.script`
- and the localised copies share base names, so that structure has to be preserved
anywhere content is written out.

**Overrides.** `GameArchive.Read` checks `Content/Override/<name>` before the
archives (`--content-override=<dir>` to point elsewhere). A changed file therefore
needs no repacking and no rebuild. That is the seam every content change goes
through from here: extract with `ff3content extract-archives`, edit, drop it in.
Lookups are held inside the override directory, because the names come from game
data rather than from us.

Saves go to `%APPDATA%\FF3` via `Compat/SaveFiles.cs`.

## Two games

The game logic is FF3's, decompiled. FF4 shares the engine and the formats, not the
facts, so the client keeps one seam for everything the logic has to know about the game
in front of it - `Compat/GameProfile.cs`, derived from the shape of the content - and a
handful of adapters behind it, all game-agnostic in their guards:

| Piece | What it does |
| --- | --- |
| `GameProfile` | no jobs or growth tables, UTF-16 text, no FF3 map-parameter chains, the leader's model, FF4's field motion-set names, the start map |
| `JumpPart` | the game part in the debug-menu slot the world code checks for; `--map`, `--pos`, `--rot` land any map in either game |
| `ScriptCommands` | dispatches the content's command table: FF3's handler where FF4 has the same command (by number, by alias, or with extra trailing operands stepped past), `Ff4Commands` where FF4 has one of its own, and an operand-exact skip, logged once, for the rest |
| `Ff4Exits` | FF4 declares exits in its scripts; decoded with the shared table, checked against the leader, jumped through the engine's own map jump |
| `Ff4Assets` / `Ff4Text` | FF4's names and cell layouts for the 2D assets FF3 asks for by FF3's names; FF4's UTF-16 text rewritten into the layout the message code walks |
| `GxCommands` | display-list commands FF4 puts between vertex runs, stepped past by parameter count; FF4's 16.16 texcoord |
| `MissingFiles`, `FrameProbe`, `DevSay` | the log of FF3-named files the install lacks; `--probe`'s per-frame heartbeat and tap log; `--say` to open a line |

The rule for a new FF4 gap: find what the FF3 logic asks for (`missing:` or `not
implemented` in the log), find where FF4 keeps the same thing, and add the mapping in
one of these - never a copy of Square's file.

## Menus

Menus are data. Eight `.xbn` files describe every screen - layout, focus order, and a
behaviour name per widget - and `MenuManager` builds the widget tree from them at load
time. Behaviours resolve by name through a self-registering factory list, so a new one
is a subclass plus a static field, with no table to edit.

`Docs/Menus.md` has the format, the element vocabulary and the full behaviour list.
Combined with the override path, a menu change is: decode to XML, edit, build, restart.

Text is data too: `.msd` files, referenced by message id from both menus and event
scripts, and readable as JSON. `Docs/Text.md`.

## Events

Everything that happens on a map is bytecode. `ScriptEngine` runs a 298 opcode
instruction set (FF4's has 500; `Shared/Script` holds both tables) cooperatively - `wait` and the message commands suspend a script and
resume it frames later - and each map's `.script` holds one program per actor. Quest
state lives in a global flag space that scripts set and branch on.

`Docs/Events.md`; the disassembler's instruction table is generated from the engine's
own handlers by `Tools/gen_opcodes.py`. Scripts are also editable as text -
`Docs/Script-Language.md` covers the language and its compiler.

## Tables

Items, spells, monsters, jobs and per map data are `.pak` files: a container of chains,
each chain an array of fixed size records. The record layouts are generated from the game's
own parse methods by `Tools/gen_records.py`, and checked against the strides the game
divides by. `Docs/Tables.md`.

## What is still Android-shaped, and why that is fine

`android/` retains a handful of types that are genuinely carrying behaviour or are
plain data:

- `MotionEvent`, `KeyEvent` — data types the input path uses
- `DialogInterface`, `AlertDialog` — the yes/no prompt, which reaches MonoGame's
  `MessageBox` through `GlobalScope.Dialog`
- `MediaPlayer` + `SoundManager` — the audio implementation

The name is historical. These are not costing anything, and audio in particular is
working; there is no reason to rewrite it for tidiness.

## What a map is

Four files, and none of them means much alone:

| File | Holds |
| --- | --- |
| `<map>.hich` | the roster: model, position, and the cast number that drives each character |
| `<map>.script` | those casts - one program per actor |
| `<map>.pak` | exits, encounters, camera, terrain |
| `*.msd` | the words the casts show |

An NPC is a `.hich` row plus a script cast plus its messages. The cast number is the
join: `.hich` says *what and where*, the script says *what it does*. That is close
enough to a game object and a behaviour that the comparison is worth making, and it is
what the editor's map view is built on.

## Tooling

`FF3.ContentTool` reads and writes every format the game uses that has been worked
out, and `ff3content editor` puts a browser front end on the ones worth seeing while
editing - scripts, menus and text. `Docs/Editor.md`.

## Diagnostics

Every run writes `bin/Debug/net8.0/logs/ff3.log`. `--log=all` or a channel list
(`gl, texture, content, file, sound, input, event, firstchance`). `--test=3d` and
`--test=model` are isolated render harnesses; `--capture-model` grabs live geometry
out of the game so it can be inspected on its own. `FF3.exe --help` lists everything.

`firstchance` matters: large parts of the decompiled game swallow exceptions, so a
fault usually surfaces as "nothing happened" rather than an error.

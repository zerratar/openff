# The content editor

```bash
dotnet run --project FF3.ContentTool -- editor --content=Content --text=..\text\en.lproj
```

Then open <http://localhost:5050/>.

It edits the game's content directly - nothing has to be extracted first, and nothing
is repacked afterwards. A file is read from the override directory if it is there and
from the archives if it is not, which is the same rule the game plays by, and saving
always writes to the override. So a change is live the next time the game starts, and
undoing one is deleting a file.

| Option | Default | |
| --- | --- | --- |
| `--content=<dir>` | `Content` | the directory holding `data000.bin` |
| `--override=<dir>` | `<content>/Override` | where edits are written |
| `--language=<code>` | `en` | which `.lproj` the dialogue is read from |
| `--port=<n>` | 5050 | |

The server binds to localhost, has no authentication, and is meant to be run by the
person editing their own copy of the game. It is not a service.

## Maps

A plan view of a map: everything that stands on it, drawn at the position it stands
at. x across, z down, which is what the game's coordinates mean.

- **Green** pins talk, **blue** pins do not, **orange** squares are exits, labelled
  with the map they lead to.
- **Click** one and the inspector shows what it is: its model, its cast number, how
  much code that cast has, its position and facing, **every line it says**, and a link
  that opens the script at its cast.
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

- **3D graphics** - models and textures are NitroSDK `BMD0`/`BTX0` inside an `NMDP`
  wrapper and are not decoded. That is what stands between the map view and a town
  rather than dots. The 2D pictures are readable; what is missing for a menu that
  looks like the game is `.NCER` and `.NSCR`, which say which part of a sheet is used
  and where it goes. `Docs/Graphics.md`.
- **Audio** - deliberately untouched.
- **New menus** - adding a widget means duplicating one.
- **New maps** - cloning one is plausible; authoring geometry is not, because models
  and collision are still opaque.

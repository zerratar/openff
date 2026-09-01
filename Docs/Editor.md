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
| `--text=<dir>` | none | decoded `.msd` JSON, for dialogue annotations |
| `--port=<n>` | 5050 | |

The server binds to localhost, has no authentication, and is meant to be run by the
person editing their own copy of the game. It is not a service.

## Scripts

The event bytecode, as source in the script language, with dialogue written in beside
the calls that show it. **Check** compiles without saving; **Compile & save** writes the
`.script` into the override.

Problems are listed underneath with their line and column, and clicking one jumps the
caret to it. `Docs/Script-Language.md` has the language.

## Menus

A canvas showing a menu screen laid out exactly as the game draws it - widget
positions come from the same `.xbn` data, with the nesting resolved the way
`MenuManager` resolves it, so a parent frame's position is added to its children's.

- **Drag** a widget to move it. Arrow keys nudge by one, shift+arrow by eight.
- **Select** one to edit its id, position, size and focus neighbours (`up`, `down`,
  `left`, `right`), or to edit the widget's raw XML.
- **Duplicate** clones the selected widget and everything nested in it. Give the copy
  a unique id.
- **Delete** removes it.

The screen picker lists every menu in the file - `MenuDefine.xbn` alone holds the main
menu, the item list, equipment, status and the rest.

What the canvas cannot show is what the game actually paints there: a widget's
appearance comes from its behaviour class and from graphics that are not decoded yet.
The boxes are true to position and size, not to looks.

## Text

Every message in a `.msd`, editable in place. Messages that are not valid UTF-8 are
marked `latin1` and are held byte for byte - see the encoding section of
`Docs/Text.md` before changing one.

## Tables

Items, weapons, armour, spells, monsters, jobs and per-map data as a grid, one tab per
chain. Values are edited in place; an array field is edited as a comma separated list,
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

- **Graphics** - models, textures and sprites are still opaque, which is what stands
  between this and a menu editor that shows the real thing, or any kind of map editor.
- **New menus and new messages** - the editor changes what is there. Adding a widget
  means duplicating one; adding a message means an id that nothing allocates yet.

# Menus

Every menu, shop, battle panel and the name entry screen is *data*, not code. Eight
`.xbn` files in the archives describe 51 screens between them: widget positions, focus
wiring, and which behaviour class drives each widget. Changing a menu is an edit to
one of those files, and none of them needs the game rebuilt.

| File | Nodes | What it covers |
| --- | ---: | --- |
| `files/MenuDefine.xbn` | 4213 | the main menu and everything under it |
| `files/ShopDefine.xbn` | 1208 | shops |
| `files/SpecialDefine.xbn` | 663 | job change, bestiary, tips |
| `files/BattleDefine.xbn` | 651 | the battle HUD and command windows |
| `files/WorldDefine.xbn` | 264 | the world map overlay |
| `files/MogNet.xbn` | 255 | Mognet letters |
| `files/ChocoboBank.xbn` | 248 | the chocobo bank |
| `files/NameEntry.xbn` | 3 | name entry |

## Editing one

```bash
dotnet run --project Crystal.Editor -- extract-archives Content Content/Override "files/MenuDefine.xbn"
dotnet run --project Crystal.Editor -- xbn Content/Override/files/MenuDefine.xbn
# edit Content/Override/files/MenuDefine.xml
dotnet run --project Crystal.Editor -- xbn-build Content/Override/files/MenuDefine.xml
OpenFF.exe
```

`xbn` decodes and then immediately rebuilds what it decoded, comparing against the
original bytes. All eight shipped files round trip byte for byte, so a warning there
means the decode lost something and the file should not be edited through the tool
until that is understood.

The `.xml` can sit next to the `.xbn` in the override directory; the game only ever
asks for names that are in the archives, so it is ignored.

## The XML

```xml
<frame>
  <focus />                 <!-- joins the focus list: reachable with the d-pad -->
  <id>com_item</id>         <!-- name, referenced by up/down/left/right -->
  <x>288</x> <y>0</y>       <!-- position, relative to the parent frame -->
  <width>96</width> <height>56</height>
  <work>0</work>            <!-- passed to the behaviour, meaning is per behaviour -->
  <myTag>0</myTag>
  <up>com_save</up>         <!-- focus neighbours, by id -->
  <down>com_magic</down>
  <left>dummy</left>
  <right>dummy</right>
  <behavior value="Text">   <!-- what actually draws and reacts -->
    <parameter>50003</parameter>   <!-- message id -->
    <parameter>8</parameter>
    <parameter>5</parameter>
  </behavior>
</frame>
```

Frames nest, and a child's `x`/`y` are added to its parent's, so moving a container
moves everything inside it. A `<table>` on a frame repeats it into a grid.

Two conventions come from the binary format rather than from the game:

- A value on an element that also has children moves into a `value` attribute, since
  it cannot be element text. That is why `behavior` reads `<behavior value="Text">`.
- `type="string"` marks a string that would otherwise read back as a number, and text
  with leading or trailing blanks is written as `value="..."` for the same reason. One
  widget's label is 32 ideographic spaces, and trimming it would corrupt the file.

## Layout rules

A frame can be placed by rules instead of numbers, the way Unity's UI Toolkit places a
`VisualElement`: the layout part of Crystal Style Sheets (below), in a `style` attribute. OpenFF understands it; the game never
sees it. `MenuXbn.FromXml` bakes every frame's rules into the plain `x`, `y`, `width` and
`height` the game reads, then strips the rules (`Shared/Text/MenuLayout.cs`). So the client
(which builds its patched `.xbn` through `FromXml`), `crystal xbn-build` and Crystal's Save
all hand the game ordinary frames, and a Steam or GOG build never meets the extension.
`crystal xbn-bake <file.xml>` writes the baked XML, so you can see what the game will get.

```xml
<frame style="left: 0; right: 0; top: 0; height: 60px">                   <!-- held to both sides -->
<frame style="right: 8px; top: 50%; translate: 0 -50%; width: 100px">      <!-- right edge, centred down -->
<frame style="flex-direction: column; gap: 2px; padding: 6px; height: auto">
  <frame style="height: 21px"> ... </frame>                               <!-- rows, one after another -->
```

A number is menu units (`px` optional); a percent is of the parent's size, or of the
frame's own for `translate`. Whatever the style does not set falls back on the frame's own
elements, so a style can take over part of the rect, and a frame without one stays exactly
as it was. A top-level frame's parent is the screen: 480 × 288, the menu area above the
bottom bar, unless the `<menu>`'s own style gives a `width` and `height`.

| Property | What it does |
|---|---|
| `left` `right` `top` `bottom` | Hold the frame this far from the parent's edge. Both sides set, with no size, stretches it between them. |
| `width` `height` | Size in units or %. `auto` on a column or row: what its frames take, plus its padding. |
| `min-/max-width` `min-/max-height` | Clamp the size. |
| `translate` | `x [y]`, applied after placing. `left: 50%; translate: -50% 0` centres the frame. |
| `margin(-side)` | Space kept clear around the frame (from its held edge, or in a column/row). |
| `flex-direction` | `column` or `row`: the frames inside follow one another, `gap` apart, inside the `padding`. |
| `gap` `padding(-side)` | Spacing inside a column or row. |
| `justify-content` | Where the run sits along the direction when there is room left: `flex-start`, `center`, `flex-end`, `space-between`. |
| `align-items` `align-self` | Across the direction: `stretch` (default: the full width of a column, unless the frame gives its own), `flex-start`, `center`, `flex-end`. |
| `flex-grow` | A frame's share of the space left over. |
| `position: absolute` | Takes a frame in a column or row out of the flow, placed by its own edges. |

For one of the game's own screens edited in Crystal, Save writes the baked `.xbn` and keeps
the rules beside the override folder, in `layout-sources/<name>.xml`, outside what the game
reads and what a Steam zip carries. That copy is what reopens while the `.xbn` is still that
edit, and Revert removes it. Screens of a mod's own and the client's (`menus/*.xml`) keep
their rules in the file; the client bakes them as it loads.

Crystal's menu canvas draws the rules through a port of the resolver (`menu-layout.js`), kept
to the same rules. The inspector writes them as you work:
- **Rect**'s X/Y/W/H edit whatever places the frame.
- The anchor presets write the edges (a centre is `50%` and a `translate`).
- **Layout** switches each rule on or off without moving the frame.
- **Children layout** makes the frame a column or a row.

A frame placed by rules has a dashed outline on the canvas.

## Crystal Style Sheets

The look goes in stylesheets: Crystal Style Sheets, a small subset of CSS in `.css` files, so
any editor highlights them. A screen's sheets are the `styles/*.css` files beside its layout
(`menus/styles/` in a mod, `Data/menus/styles/` in the client), read in name order, then any
`<style>` elements in the layout itself. A frame's own `style` attribute beats every sheet.
The layout's older words (`<colour>`, `<font>`, `<align>`, `<window/>`) sit under all of them,
the way HTML's attributes do.

```css
#gambits window   { opacity: 90%; }                 /* every panel of the screen */
#w_top            { -ff-panel: bar; -ff-tint: #402020; }
.row > text       { color: pale-blue; font-size: 14px; }
text.picked       { color: #ffd080; }
#help             { visibility: hidden; }
```

**Selectors:**
- a type: `menu`; `frame`; `text` (a frame with the Text behaviour); `window` (a frame with a panel);
- `#id` (a frame's `<id>`, or a screen's `<name>`), `.class` (the `class` attribute), and `*`;
- combined with a space (anywhere inside) or `>` (directly inside), and listed with commas;
- pseudo-classes:
  - states: `:focus` (the cursor is on the frame), `:focus-within` (on it or a frame inside it), `:disabled` / `:enabled` (`IMenuWidget.Enabled` from code);
  - structure: `:first-child`, `:last-child`, `:only-child`, `:nth-child()` and `:nth-last-child()` (`An+B`, `odd`, `even`);
  - `:not()` of one simple selector (`.row:not(:first-child)`).

Specificity and source order decide, as in CSS; a pseudo-class counts as a class. A rule whose selector uses anything else, such as `:hover`, `::before` or `[attr]`, is skipped, and so is any `@` block but `@keyframes`.

| Property | What it does |
|---|---|
| the layout properties | As in [Layout rules](#layout-rules). A sheet can place frames too. |
| `display: none` | Not drawn, and left out of its column or row. |
| `color` | A palette word (`white`, `yellow`, `pale-blue`, `pale-yellow`, `pale-red`, `disabled`…) or `#rgb` / `#rrggbb`. Inherited. |
| `font-size` | 6–31 (px optional), or `large` / `normal`. Inherited. |
| `text-align` | `left`, `right`, `center`, `menu` (the hand cursor stands clear), `button`. Inherited. |
| `opacity` | 0–1 or a percent, for the panel and the text. Multiplies down to the frames inside. |
| `visibility: hidden` | Text and panel not drawn. The frame still takes its place and the cursor. Inherited. |
| `-ff-panel` | `window` (the game's window art), `bar` (its dark translucent bar), `none` (no panel). |
| `-ff-tint` | `#rrggbb`: the window's art multiplied by it. For a bar, the bar's colour. |

Properties CSS already has keep their CSS names. The ones only OpenFF has carry a `-ff-`
prefix, the way browsers mark their own. Anything else is skipped; Crystal's inspector lists
it as "not Crystal Style Sheets".

The cascade is worked out as the layout loads (`Shared/Text/MenuStyles.cs`). The layout
properties go into each frame's `style` for the layout bake; the look is written into elements
the client reads as the screen opens: `<colour>`, `<font>`, `<align>`, `<opacity>`, `<hidden/>`,
`<window/>`, `<panel>`, `<tint>`, `<background>`, `<textstyle>`, `<transition>`, `<animation>`.
While the screen runs, the client cascades again when a class, a state or an animation changes
something (see [States and motion](#states-and-motion)). The game - and Steam or GOG, when a
layout is exported - gets the base look: no state, nothing moving.
- **At runtime:** `ModMenus` draws a text in its own colour and opacity through its text canvas (`NNSG2dTextCanvas.rgba`/`alpha`), and a window through `BasicWindow.SetLook` and `SetBarStyle`.
- **Stacking:** windows stack in the layout's order, so a later one, bar or see-through, sits over an earlier one.
- **For the game:** `MenuXbn.FromXml` cascades the `<style>` elements it finds, then bakes and strips everything like the layout rules.

In Crystal:
- **Styles panel:** the picker on the panel under the canvas switches it between the XML and the screen's sheets (**+ New stylesheet** makes `styles/<screen>.css`). A sheet applies as you type, and Save writes it.
- **Canvas:** draws the cascade through `menu-styles.js`, a port kept to the same rules.
- **Style section in the inspector:**
  - the frame's classes;
  - its own panel, opacity, tint, text colour and hidden;
  - every sheet rule that reaches it, in cascade order. Anything overridden is struck through, the way a browser's devtools show it.
- **Hierarchy eye:** hides a frame and what's inside it from the canvas, for the editor only; the file doesn't change. Alt+click shows only that frame (with its parents and its frames); Alt+click again brings the rest back.

### Lettering

A text's own shadow, outline, face, weight, slant, spacing and case, as CSS has them
(`Shared/Text/MenuText.cs`). All of them are inherited.

```css
#title { text-shadow: 2px 2px 3px #000000c0; font-family: serif; font-weight: bold; letter-spacing: 1px; }
.hint  { font-style: italic; text-transform: uppercase; }
.badge { -ff-text-stroke: 1px #203060; text-shadow: none; }
```

| Property | What it does |
|---|---|
| `text-shadow` | `x y [blur] colour`, a list, the first on top (menu units). `none` takes even the game's own drop shadow away; left unsaid, the game's shadow stays. |
| `font-family` | A list; the first found is used. `url("fonts/x.ttf")` is a face beside the layout (it ships with the mod). `serif` is Times New Roman, `monospace` Consolas, `sans-serif` / `game` the game's own face. A name (`"Georgia"`, `"Segoe UI"`) is one of Windows' faces, read from the player's Windows as the title's Times New Roman is - nothing of it is shipped. The game's faces stand behind any face for the glyphs it lacks (Japanese, Korean). |
| `font-weight` | `bold` (or 600 and over): the face's bold where Windows has one, else drawn heavier. |
| `font-style` | `italic` / `oblique`: the face's italic where Windows has one, else slanted. |
| `letter-spacing` | Menu units between the letters. Measured too, so right-aligned and centred texts stay where they belong. |
| `line-height` | For a text of several lines: menu units from one line to the next, or a number times the size. |
| `text-decoration` | `underline`, `line-through`, `none`. |
| `text-transform` | `uppercase`, `lowercase`, `capitalize`, `none`. |
| `-ff-text-stroke` | `width colour`: an outline round the letters (`-webkit-text-stroke` is read too). |

In the client the shadows and the outline are draws of their own under the text (`GlobalScope.MenuText.cs`), a shadow's blur the face's blurry effect; the face, weight, slant and spacing are the text's (`TrueTypeText`).

### States and motion

A frame's look can follow its state and move, as a web page's does (`Shared/Text/MenuAnimation.cs`):

```css
.row           { transition: color 0.2s ease-out, opacity 0.2s; }
.row:focus     { color: #ffd080; -ff-text-stroke: 1px #402000; }
.row:disabled  { color: disabled; opacity: 60%; }
@keyframes pulse { from { opacity: 1; } 50% { opacity: 0.4; } to { opacity: 1; } }
#new           { animation: pulse 1.2s ease-in-out infinite; }
```

- **States:** `:focus` follows the hand cursor, `:disabled` a frame whose `Enabled` code set to false. The client cascades again as they change.
- **`transition`:** `<property | all> <duration> [<timing>] [<delay>]`, a list; or `transition-property`, `-duration`, `-timing-function` and `-delay`. A shorthand's name moves its longhands (`border` moves `border-color`).
- **`animation`:** `<name> <duration> [<timing>] [<delay>] [<count> | infinite] [<direction>] [<fill-mode>] [<play-state>]`, a list; or its longhands (`animation-name`, `-duration`, `-timing-function`, `-delay`, `-iteration-count`, `-direction`, `-fill-mode`, `-play-state`). `@keyframes name { from {…} 50% {…} to {…} }` says what it goes through; a stop that leaves a property out takes the frame's own value.
- **Timing:** `linear`, `ease`, `ease-in`, `ease-out`, `ease-in-out`, `cubic-bezier(a, b, c, d)`, `steps(n[, start | end])`, `step-start`, `step-end`. Direction: `normal`, `reverse`, `alternate`, `alternate-reverse`. Fill mode: `none`, `forwards`, `backwards`, `both`.
- **What moves:** the look - opacity, `color`, `-ff-tint`, the background's colours, gradients, borders, corners and shadows, the text's shadows, outline and spacing. Two values move from one to the other when they have the same shape (`0 2px 8px #000` to `0 4px 16px #f00`: the same words, with numbers and colours in the same places); otherwise the value changes half way through, as CSS changes what it can't interpolate. A palette word (`yellow`) moves to and from `#hex` through its colour in the game's palette.
- **`translate` moves too:** what the frame draws (its window, its panel, its texts) and its children with it, by the difference from the translate its layout was placed with. The layout itself doesn't move, as in CSS: a press still reaches the frame where the layout put it. `#next { animation: bob 0.5s ease-in-out infinite alternate } @keyframes bob { from { translate: 0 -3px } to { translate: 0 3px } }` bobs the dialogue's arrow.
- **What doesn't:** the rest of the layout (`left`, `width`…) - the game is given it as plain numbers - and `display` or `-ff-panel`.

A screen with nothing moving costs nothing: the client works the look out again every frame only while a transition or an animation is under way.

**In Crystal:** the preview cascades the same states and draws the same boxes and lettering (`menu-styles.js`, `menu-background.js`, `font.js`); **Play** runs the motion, a translate's included.
- **Preview state** (the Style section): the selected frame as `:focus` or `:disabled` - the editor's alone, never written to the file.
- **▶ Play** (the canvas's toolbar): runs the transitions and animations as the client does (`menu-animation.js`); a state changed while it plays transitions.
- **The inspector:** a **Lettering** section (shadows, face, weight, slant, spacing, line height, decoration, case, outline), a **Box** section (borders, corners, box shadows), a gradient editor in **Background** (Picture or Gradient: kind, direction, shape, stops), and a **Motion** section (transitions; animations, their names offered from the sheets' `@keyframes`).

### Backgrounds

A frame can have a colour and a picture behind it, in place of the game's window or on top of its fill. It works like UI Toolkit's Background panel:

```css
#w_rules {
  -ff-panel: none;                                 /* no game window: the picture is the panel */
  background-image: url("images/panel.png");       /* a file in images/ beside the layout */
  -ff-slice: 12;                                   /* 9-slice: 12 px borders keep their size */
}
#w_title   { background-image: resource("files/m000_window.NCGR"); -ff-background-rect: 0 0 64 64; -ff-slice: 16; }
#portrait  { background-image: url("images/face.png"); -ff-background-scale-mode: scale-to-fit; }
#stars     { background-color: #10204080; background-image: url("images/stars.png"); background-repeat: repeat; }
```

| Property | What it does |
|---|---|
| `background-color` | `#rgb`, `#rrggbb`, `#rrggbbaa`, `rgb()`/`rgba()`, `transparent`. Drawn under the picture. |
| `background-image` | `url("…")`: a file beside the layout (`images/` in a mod's `menus/` or the client's `Data/menus/`); it ships with the mod. `resource("…")`: one of the game's pictures by name (`files/m000_window.NCGR`), read from the player's install, so nothing of the game's is copied into the mod. `none`. |
| `-ff-background-rect` | `x y w h`: the part of the picture to use (one sprite of a sheet), in its pixels. |
| `-ff-background-tint` | The picture multiplied by this colour. |
| `-ff-background-scale-mode` | `stretch-to-fill` (the default), `scale-and-crop`, `scale-to-fit`. |
| `background-size`, `background-position`, `background-repeat` | As in CSS, for tiles: `repeat`, `repeat-x`, `repeat-y`; `cover` / `contain`; `left top`, `center`, `50% 100%`. When given, these decide the layout rather than the scale mode. |
| `-ff-slice`, `-ff-slice-left` … `-bottom` | 9-slice borders in the picture's pixels (one to four values, as `margin`). The borders keep their size; the middle and edges stretch. Any slice over 0 overrides the scale mode, size and repeat. |
| `-ff-slice-scale` | Menu units per picture pixel for the borders (1). |
| `-ff-slice-type` | `sliced` (stretched) or `tiled` (the edges and the middle repeated). |
| `-ff-background-filter` | `linear` (smooth when scaled, the default) or `point` (sharp pixels). |

#### The box: gradients, borders, corners, shadows

Round a frame's background, as CSS draws a box. A frame with any of these is painted into
pictures of its own (`Shared/Text/MenuPaint.cs`), at the window's resolution, since the game
draws only textured quads.

```css
#w_top  { -ff-panel: none; background-image: linear-gradient(to bottom, #2a4a8a, #0e1a36); border: 2px solid #c8d8ff; border-radius: 10px; box-shadow: 0 4px 10px #000000c0; }
#badge  { -ff-panel: none; background-image: radial-gradient(circle at 30% 40%, #ffe080, #c04000 60%, #200000); border-radius: 50%; }
#w_row  { box-shadow: inset 0 0 10px #000; }                                  /* over the game's window too */
#stripe { background-image: repeating-linear-gradient(45deg, #203050 0 6px, #283a60 6px 12px); }
```

| Property | What it does |
|---|---|
| `background-image` | Also `linear-gradient()`, `radial-gradient()`, `conic-gradient()` and their `repeating-` kinds, in place of a picture: `[angle \| to side]` / `[circle \| ellipse] [size] [at x y]` / `[from angle] [at x y]`, then colour stops (`colour [position]` in %, px or deg; missing positions are spread out). |
| `border` | `width [style] colour`; also `border-width` and `border-color` (one to four values, as `margin`), `border-style` (`solid`; `none` takes it away), and `border-top` / `-right` / `-bottom` / `-left` with their `-width` and `-color`. |
| `border-radius` | One to four corners (top-left, top-right, bottom-right, bottom-left), px or % of the shorter side; `border-top-left-radius` and the rest. Corners that would overlap shrink together. |
| `box-shadow` | `[inset] x y [blur [spread]] colour`, a list, the first on top; `none`. An outer shadow is drawn behind the frame - behind the game's window, if it has one - and not under it; an inset one inside the border. |

Colours anywhere here are `#hex`, `rgb()`/`rgba()`, `hsl()`/`hsla()`, CSS's common names (`gold`, `navy`…), or the game's palette words.
- **Layers:** the outer shadows are a sprite of their own behind the window's fill; the colour and the gradient go under the frame's picture, the inset shadows and the border over it. A round corner doesn't clip a picture (`background-image: url(…)`) or the game's window art.
- **Cost:** a painted box takes a texture per layer while the screen is open; a big frame is painted at less resolution so that one stays near a million pixels.

**How it draws in the client:** `Shared/Text/MenuBackground.cs` lays the picture out as quads. `GlobalScope.MenuPanel.cs` draws them as a sprite of the game's own, in the same depth-sorted pass as the windows. That puts a background above the backdrop, between a window's fill and its frame, and under the cursor, the portrait and the texts. A picture takes one GL texture slot while the screen is open.

#### Sprites of a sheet

A sheet holds named sprites, each with its own 9-slice borders, like Unity's Sprite Editor in *Multiple* mode. The sheet can be one of yours or one of the game's. A folder of screens keeps them in `sprites.json` beside its layouts (`Shared/Text/MenuSprites.cs`). A frame names one with `-ff-sprite`, with the sheet as its picture:

```css
#w_rules { -ff-panel: none; background-image: url("images/ui.png"); -ff-sprite: panel_blue; }
```

```json
{ "sheets": { "url:images/ui.png": { "sprites": [
  { "name": "panel_blue", "x": 0, "y": 0, "w": 48, "h": 48, "left": 12, "top": 12, "right": 12, "bottom": 12 } ] } } }
```

The sprite gives the part of the picture and its borders. A frame's own `-ff-background-rect` or `-ff-slice` still wins over them. Sheets are keyed `url:<path>` or `resource:<name>`.

#### The portrait

A screen that asks for a hero (`"characterSelect": true`) shows the game's own portrait of that hero. That's the face picture drawn where the game puts it (8, 8, 56 × 56), fixed and at its own size.

A **portrait frame** in the layout takes its place, like a template: `<frame><portrait/>…</frame>`. The client puts the game's face away and draws the hero's face (`files/pc<hero>_<job>.NCGR`) fitted into that frame's rect, over the frame's own window or background, with its styling (opacity, the `portrait` selector in a sheet). Without one, the game draws its own as it always has.
- **Which hero:** `<portrait>1</portrait>` shows a party slot's hero instead of the screen's.
- **From code:** `Menu.Portrait = heroId` sets whose face it is. A screen that moves between heroes with L / R sets it; Gambits does. With no portrait frame, the game's own face swaps to that hero in the same place, and the faces go back where they were when the screen closes.
- **In Crystal:** a screen with the game's portrait has a *portrait* row in the Hierarchy (with an eye) and a ghost of it in the view. **Make it a frame** (on the row's right-click, or its card) creates a portrait frame at the game's spot, to move, size and style. **New ▾ > New portrait** and the Frame section's **Portrait** switch do the same for any frame. Preview draws a sample face in it.

#### The backdrop

Under every frame is the screen's **backdrop**: one of the game's menu backdrops (the definition's `"background"`), or `-1` for none. Over it, the screen can have a background of its own: the `<menu>`'s style, or a sheet's `menu` / `#name` rule, with the same properties as a frame. It's drawn behind every window.

One of the game's screens picks its backdrop in code (the main menu 9, Item 0, Magic 1…). A definition of the mod's that reaches it can put another in its place, with no layout needed: `{ "id": "main_menu_bg", "screen": "main_menu", "file": "MenuDefine.xbn", "background": 5 }` (`-1` for none). It holds while that screen is up, whenever the screen asks for its backdrop again, and the next screen gets its own back. Crystal's backdrop card for a game screen has the **Backdrop** picker; it writes that definition into the open project.

The game's backdrops are made of pieces of `files/menu_bg_01.NCGR` (a cell bank), so they aren't whole pictures in the picker. Their pieces can be used as sprites of that sheet.

**Row lines.** Every backdrop draws the lines between its rows with one piece: a 16-pixel strip from row 96 of `menu_bg_01`, at half size. The lines sit at the heights of the game's own layout. A definition's `"backdropLines"` says what happens to them:
- `"game"`: as the game draws them.
- `"none"`: taken away. A line that fills the gap between two panels, with nothing under it (the main menu's party panels), is their edge and stays.
- `"fit"`: laid again at the list's rows as they are now. This is the main menu's default: when the client adds entries (Gambits), the command column's lines follow the new rows, and the columns beside it stay as they were.

One of the game's screens takes the setting from a definition in a mod that reaches it, `{ "id": "main_menu_lines", "screen": "main_menu", "file": "MenuDefine.xbn", "backdropLines": "none" }`, with no layout needed. The game's own file isn't the mod's to change. Crystal's backdrop card has the **Row lines** switch, and for a game screen it writes that definition into the open project. The preview draws the backdrop at the zoom's resolution, so the lines show there as in the game (`GlobalScope.BackdropLines.cs`).

**In Crystal:** the inspector's **Background** section has the colour (with its alpha), the picture and its **Sprite**, tint, part, scale mode, size, position, repeat, slices, slice scale and type, and the filter.
- **Where values come from:** each label says it. A bold label with `•` is set on the frame (click it to take it off); a `∙` label comes from a stylesheet (hover for which rule). **×** takes a value off the frame, or blocks a sheet's value for this frame alone.
- **No picture:** the section says what's drawn instead. For example, "the game's window (built in, m000_window) from the layout's `<window/>`", with **No window** and **A picture instead…**.
- **Hierarchy:** the **backdrop** has its own row and eye. Its card picks the game's backdrop (or none) and edits the screen's own background.
- **Sprite editor** (**Sprites…**, or after choosing a sheet in the picker): the sheet with its sprites outlined.
  - Drag to make or move a sprite.
  - Edit the selected sprite's name, rect and borders (its borders are lines to drag on its own large view).
  - Tools: **From the game's cells** (the pieces the game cuts one of its sheets into, from its `.NCER`), **Auto** (islands of pixels that aren't see-through), **Grid**, and **Delete**.
  - **Save** writes `sprites.json`; **Use this sprite** puts it on the frame.
- **Picture picker:** chooses between *Beside the screen* (with **Import PNG…** to copy one into `images/`) and *The game's*.
- **Slice editor** (**Edit slices…**): shows the picture large with its borders as green lines to drag. **Pick part…** drags out the sprite of a sheet first.
- **Canvas:** draws backgrounds with a port of the same layout (`menu-background.js`), in the game's order.

## Data bindings

A frame can take its text, visibility and classes from the game's data while the screen runs,
the way Unity's UI Toolkit binds an element to a data source by a path. It's written as
attributes, so the game's own file never carries them:

```xml
<menu data-source="hero">                                   <!-- the screen's data: the picked hero -->
  <frame bind-text="{name}   Lv {level}"> ...               <!-- a template: {path} or {path:format} -->
  <frame bind-text="HP {hp}/{maxHp}" bind-class="low: hp &lt; maxHp / 4; ko: !alive"> ...
  <frame data-source="party[1]" bind-text="{name}"> ...     <!-- a frame's own data, for it and its frames -->
  <frame bind-visible="gil &gt;= 1000" bind-text="{gil:N0} G"> ...
```

**Paths:**
- **Syntax:** names joined by dots, with `[index]` for a list or a dictionary (`party[0]`, `rules[menu.hero]`). Public properties and fields, any case; `count` works on lists.
- **Where the first name is looked up:** first on the data source, then among the roots:
  - what the screen's code put in `Menu.Data`;
  - `hero` (the party member the screen asked for);
  - `party` (the members), `gil`, `items` (the bag);
  - `menu` (the screen: `hero`, `focused`, `id`);
  - `this` (the data source itself).

**A bound style** (`bind-style`): declarations with `{paths}` in them, put over the frame's own style as the data says - `bind-style="background-image: {dialogue.avatar}"`, `bind-style="-ff-tint: {tint}"`. What it binds cascades like the frame's own style, over every sheet.

**Out of the layout** (`bind-display`): while its expression is false the frame is `display: none` - out of its parent's row or column, the others closing up over its place, as a horizontal layout group leaves out an inactive child. `bind-visible` hides a frame and keeps its place, as CSS's `visibility` does. The field's HUD lays its rows and columns out again as the data changes (a speaker with no picture: `bind-display="dialogue.avatar"` on the avatar, and the text takes its room); a menu screen's layout is laid out once, as it opens.

**Expressions** (`bind-visible`, `bind-display`, and a class's condition in `bind-class`):
- a path or a literal (a number, `'text'`, `true`, `false`, `null`), with simple sums (`maxHp / 4`, `level + 1`);
- compared with `== != < <= > >=`, turned round by `!`, and joined by `&&` and `||`.

A plain value counts as true when it's true, a non-zero number, a non-empty text or list, or anything else that isn't null.

A class a binding switches on brings its stylesheet rules with it. That's the way to colour a hero red when low, or to grey out a row.

Bindings are worked out every frame (`OpenFF.Engine/MenuBindings.cs`), and only what changed is put on. A text is written only when its value changes, so code that writes a bound frame's text keeps it until the bound value moves.

Steam and GOG builds show the layout's own `<data>` text: bindings are OpenFF's.

In Crystal, the inspector's **Bindings** section edits a frame's data source, text template (**+ path** puts a path in at the cursor), visibility, bound classes and bound style. It shows what they come to with a sample of the game's data (Luneth, Lv 12, 12,345 G), and Preview draws bound frames that way too (`menu-bindings.js`, a port kept to the same rules).

## From code

A `MenuBehaviour` reaches the screen through `Menu`, and each frame through its `IMenuWidget`, much as a UI Toolkit script reaches its `VisualElement`s:

```csharp
public sealed class PartyScreen : MenuBehaviour
{
    public override void OnOpen()
    {
        Menu.Data["quests"] = MyQuests.Open();                 // a root for the layout's bindings: {quests[0].title}
        IMenuWidget title = Menu.Q("#title");                  // by id, as the sheets pick frames out
        title.Text = "Party";
        foreach (IMenuWidget row in Menu.Query("#list > .row"))
            row.ToggleClass("empty", row.Work >= Game.Party.Members.Count);
        Menu.Q("#gil").Bind("text", "{gil:N0} G");             // a binding from code, as bind-text
        Menu.Q("#hint").SetStyle("opacity", "60%");            // the frame's own style, over the sheets
    }

    public override void OnFocus() => Menu.Q("#help").Text = "On " + Menu.Focused;
}
```

**`IMenuScreen`:**
- **Finding frames:** `Q(selector)` gives the first match and `Query(selector)` every match, using the stylesheets' selectors (`#id`, `.class`, `text`, `window`, `>`, and so on). `Widget(id)` and `Widgets` also work.
- **Data for bindings:** `Data` holds the binding roots, and `Refresh()` works the bindings out at once.
- **Moving about:** `Focus`, `Focused`, `Hero`, `Open`, `Close`, and the sounds.

**`IMenuWidget`:**
- **Text:** `Text`, `Colour` (a palette colour - the text's own, which a sheet's `:focus` colour is shown over while it holds), `FontSize`, `Visible`.
- **State:** `Enabled`: false puts the frame in `:disabled` for the sheets. It is only a look; the cursor can still land on it.
- **Classes:** `Classes`, `HasClass`, `AddClass`, `RemoveClass`, `ToggleClass`. A change cascades the sheets again and puts on what it changes: colour, size, opacity, lettering, panel, hidden - a transition moving it if the sheet has one. A panel's background, opacity and tint are put on in place; the windows are made again only if a panel comes or goes.
- **Style:** `GetStyle`/`SetStyle` for the frame's own look properties, over every sheet; `Opacity` as a shortcut.
- **Bindings:** `DataSource`, `Bind(property, expression)` (`"text"`, `"visible"` or `"class.<name>"`; null unbinds), `Binding(property)`.
- **Structure:** `Parent`, `Children`, `Id`, `Work`, the rect, and `ScreenRect` for drawing over the frame.

Layout properties (`left`, `width`, `flex-direction`…) are fixed when the screen is built; change them in the layout, not from code. Frames need an id for their classes and style to reach the stylesheets. On one of the game's own screens with no mod layout, `SetStyle` puts colour, font size, opacity and visibility on directly.

`MenuBindings.Format`, `Test` and `Resolve` are public, for code of your own that wants the same paths and templates.

## The field's HUD

The field's dialogue window and the map-name banner are laid out by `field_hud`, a screen of the client's in `WorldDefine.xbn` (it's in Crystal's list of that file's screens), with the rest of what the client draws from code outside battle: the Yes / No box, the field's buttons, the title's commands and the menus' touch buttons. It's styled like any screen, and a layout of it can add frames of its own:

| Frame | What it is |
|---|---|
| `dialogue` | The message window. Its look: `-ff-panel: none` takes the game's window art away, so a background, painted box or picture can take its place; also `opacity` and `-ff-tint`. |
| `dialogue/text` | Where the text starts, and its colour, size, lettering and opacity. A whole text (a mod's `Game.Dialogue.Say`) is broken into lines at the frame's width; the game's own lines keep the breaks they were written with, so leave them the game's width or give them a smaller size. |
| `dialogue/name` | FF4's speaker name. |
| `dialogue/next` | The page-turn arrow: `visibility: hidden` hides it, `-ff-tint` and `opacity` colour it. A background of its own (a picture, a painted box, with an `animation` if you like) takes its place, up exactly when the game's would be. |
| `dialogue/<yours>` | A frame of the mod's, shown with the window: a window, a background, a text (`<data>`, or `bind-text`). |
| `map_name` | The banner a map's name comes up in; its text takes the frame's look. |
| `confirm` | The Yes / No box (an inn's question, a script's `Game.Dialogue.Ask`): the game's window, or `-ff-panel: none` and a look of the mod's. `confirm/question`, `confirm/yes` and `confirm/no` place its lines and give them their look; the one the hand is on is in `:focus`, and `confirm/yes/cursor` and `confirm/no/cursor` are where the hand stands on each. Asked with its question in the message window (`Ask`), the box closes up over the question's line. |
| `menu_button`, `map_button`, `talk_button` | The field's buttons, where the options put them (they swap the menu's and the map's). `-ff-tint` and `opacity` colour the game's picture; a background (or `-ff-panel: none`) takes its place, with frames of the mod's in it (a label). |
| `title`, `title/row` | The title's column of commands: the first row at the frame's top, the last no lower than its bottom, a row's height the step between them. `title/row` is the commands' lettering (colour, size, face, shadows, opacity) where they're drawn as text; the one the hand is on is in `:focus`, a Continue with nothing to continue `:disabled`. |
| `a_button`, `b_button`, `l_button`, `r_button` | A menu's touch buttons (OK, Back, the previous and next hero): where they are and a press reaches. `a_button/text` and `b_button/text` are their labels' look; a background takes the place of L's and R's pictures. |

Their bindings (`bind-text`, `bind-visible`, `bind-display`, `bind-class`, `bind-style`) reach `dialogue` (`number`, `text`, `speaker`, `avatar`, `map`), `banner` (`number`, `text`), `hero`, `party` and `gil`. With nothing styled, the game's windows are drawn as they always were.

```xml
<frame><id>dialogue</id> ...
  <frame><id>text</id><x>86</x><y>13</y><width>376</width><height>60</height></frame>
  <frame bind-display="dialogue.avatar" bind-style="background-image: {dialogue.avatar}"><id>avatar</id><x>10</x><y>9</y><width>66</width><height>66</height></frame>
  <frame bind-visible="dialogue.speaker" bind-text="{dialogue.speaker}"><id>speaker</id><x>14</x><y>-20</y><width>120</width><height>22</height></frame>
</frame>
```

```css
#dialogue { -ff-panel: none; background-image: linear-gradient(#203468f0, #0a1228f0); border: 2px solid #c8d8ff; border-radius: 10px; box-shadow: 0 4px 12px #000000c0;
            flex-direction: row; gap: 10px; padding: 9px 10px; align-items: flex-start; }   /* the avatar, then the text in the room left */
#dialogue > #text { flex-grow: 1; margin-top: 4px; }
#dialogue > #name, #dialogue > #next, #dialogue > #speaker { position: absolute; }
#avatar   { -ff-background-scale-mode: scale-to-fit; border: 1px solid #8090c0; border-radius: 6px; }
#speaker  { background-image: linear-gradient(#3a5cb0, #1a2c5c); border-radius: 6px; color: pale-yellow; text-align: center; }
```

**Who speaks.** FF3 never names its speakers, so a mod says who does:
- **`menus/speakers.json`:** speakers with a name and a picture, and the game's message numbers they say (a number, or a range):

  ```json
  { "speakers": { "elder": { "name": "Elder", "avatar": "images/elder.png" },
                  "luneth": { "name": "Luneth", "avatar": "resource:files/pc1_01.NCGR" } },
    "messages": { "1000142": "elder", "1000150-1000160": "elder" } }
  ```

  A picture is a file beside the table, or `resource:` one of the game's (a hero's face: `files/pc1_01.NCGR`). `{dialogue.avatar}` gives it as a background image says it (`url("…")`, `resource("…")`).
- **`Game.Dialogue.Say(text, speaker)`:** the speaker by a table's id or name (with its picture), or just the name.
- **Code:** `Game.Events` publishes `DialogueShown` (`Number`, `Text`, `Map`) before the window shows it; a handler can set `Speaker` and `Avatar` (`AvatarFile(path)` for a file), over the tables.

The client keeps `field_hud` as it last built it (`OpenFF/Compat/FieldHud.cs`), so its frames hold while another file is loaded too (a battle's, the menus'); at the title, before the field has loaded it, the client reads it ahead (`ModMenus.PrepareFieldHud`). Crystal's preview draws the game's pieces in it: the window, the page arrow, the buttons, the hand, sample lines.

## Screens of a mod's own

The client ships screens of its own, such as the Gambits, in `Data/menus`. A mod's screen with the same id takes the place of the client's. That's how a player keeps their own version: in Crystal, **Copy into my mod** on a client screen copies it into the open project. The copy includes its layout and the client's stylesheets, pictures and `sprites.json`; anything the project already has is kept. The mod's copy survives an update of the client, which replaces the install's own `Data/menus`. It can still name the client's behaviours (`GambitsScreen`): a mod's screen finds the host's behaviours after its own code and the engine's.

Editing the client screens themselves is for the client's own sources. On a source checkout Crystal saves them to `OpenFF/Data/menus` and mirrors them into the built client. On an install it warns that an update will replace them.

The same XML, a `<menu>` at a time, is how an OpenFF mod adds a screen: `menus/<id>.xml`
beside a `menus/<id>.json` naming it and the `MenuBehaviour`s on its frames. As the client
loads `MenuDefine.xbn`, `OpenFF.Client.ModMenus` decodes the file with `MenuXbn.ToXml`
(the codec is `Shared/Text/MenuXbn.cs`), appends every mod screen's `<menu>` (renamed to
the definition's `screen`), gives each `<focus/>` frame a `myTag` equal to its place in the
focus list - `MoveCursor` lands on `initFocus(target.myTag())`, so a layout with the tags
wrong moves nowhere - fills in `dummy` for a missing `up/down/left/right`, puts an entry per
screen that asks for one into `main_menu`'s `main_command` (cloned from `com_job`, `work` =
`CWMenuMod.KIND + index`, the rows re-spaced, the ring re-linked, the character panels'
tags moved down as many places as entries went in), and encodes it back with
`MenuXbn.FromXml`. `CWMenuManager.CSelectInitialize` and the Status check that read the
panels as "tag 9 and up" now count the command list instead.

`wmenu.CWMenuMod` is one `WMENU_KIND` (15, `WMENU_KIND_MAX`) for all of them: `initialize`
sets the backdrop, hides the faces (or shows the picked hero's), `buildMenu(screen)`;
`run` is `execute()` plus the decide/cancel states and L/R/X/Y edges handed to `ModMenus`,
which dispatches to the definition's behaviours over an `IMenuScreen` adapter (widgets by id,
text through `MBText.mbSetBufferMsg`, colour through `changeTextColor`, focus through
`setFocuseMedget`). `--trace-menu` logs every `initFocus` with its caller; `--dump-menus`
writes the patched file as XML to `%TEMP%`.

## Behaviours

`<behavior>` names a class, resolved at load time by `MenuBehaviorFactory.
createMenuBehavior`. Factories link themselves into a list from their constructors, so
adding a behaviour is a new subclass plus one static field in `menu.Members.cs` - there
is no table to update. `MenuBehavior` itself derives from `MenuBehaviorFactory`, which
is how the three singleton screens below register under their own names.

All 35 names the data uses resolve. `Text` alone accounts for 348 of the 444 widgets.

| Behaviour | Widgets | Defined in |
| --- | ---: | --- |
| `Text` | 348 | `menu.MBTextFactory.cs` |
| `CConfig` | 15 | `menu.MBConfigFactory.cs` |
| `Icon` | 9 | `menu.MBIconFactory.cs` |
| `ItemList` | 7 | `menu.MBItemWindowFactory.cs` |
| `MonsterBossMark` | 7 | `menu.MBMonsterBossMarkFactory.cs` |
| `MonsterNewMark` | 7 | `menu.MBMonsterNewMarkFactory.cs` |
| `Item` | 5 | `menu.MBItemNameFactory.cs` |
| `MenuSaveLoad` | 4 | `menu.MBSaveLoadFactory.cs` |
| `ShopText` | 4 | `menu.MBShopTextFactory.cs` |
| `UseItem` | 4 | `menu.MBItemUseFactory.cs` |
| `Command` | 3 | `menu.MBCommandFactory.cs` |
| `Gold` | 3 | `menu.MBPlayerGoldFactory.cs` |
| `CConfigExplanation` | 2 | `menu.MBConfigExplanationFactory.cs` |
| `CConfigSound` | 2 | `menu.MBConfigSoundFactory.cs` |
| `PramMagic` | 2 | `menu.MBMagicPramFactory.cs` |
| `Question` | 2 | `menu.MBQuestionFactory.cs` |
| `SelectItem` | 2 | `menu.MBSelectItemFactory.cs` |
| `CStatus` | 1 | `menu.MBStatusFactory.cs` |
| `CWMenuEquip` | 1 | `wmenu.CWMenuEquip.cs` |
| `EquipCommand` | 1 | `menu.MBBattleEquipFactory.cs` |
| `JName` | 1 | `menu.MBJobNameFactory.cs` |
| `JobParamList` | 1 | `menu.MBJobParamListFactory.cs` |
| `LinkList` | 1 | `menu.MBLinkListFactory.cs` |
| `MBMogNetSelectPerson` | 1 | `mognet.MNSSelectPerson.cs` |
| `MBStatus` | 1 | `wmenu.CWMenuStatus.cs` |
| `MenuSuspend` | 1 | `menu.MBSuspendFactory.cs` |
| `MogNetLetterBrowse` | 1 | `mognet.MBMogNetLetterBrowseFactory.cs` |
| `MonsterList` | 1 | `menu.MBMonsterListFactory.cs` |
| `ShopBuyList` | 1 | `menu.MBShopBuyListFactory.cs` |
| `ShopName` | 1 | `menu.MBShopNameFactory.cs` |
| `ShopNumber` | 1 | `menu.MBShopNumberSelectFactory.cs` |
| `ShopSellList` | 1 | `menu.MBShopSellListFactory.cs` |
| `SongList` | 1 | `menu.MBSongWindowFactory.cs` |
| `TipsList` | 1 | `menu.MBTipsListFactory.cs` |
| `WeaponName` | 1 | `menu.MBWeaponNameFactory.cs` |

Two more are registered but never referenced by the shipped data - `ChocoboBank` and
`SelectJobParam` - which are spare slots rather than dead code.

## Message ids

`Text` takes a message id (`50003` above), which lives in the `.msd` files, not here.
Those are not decoded yet, so new text still has to reuse an existing id.

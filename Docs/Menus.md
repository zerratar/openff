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
- combined with a space (anywhere inside) or `>` (directly inside), and listed with commas.

Specificity and source order decide, as in CSS. A rule whose selector uses anything else, such as `:hover` or `[attr]`, is skipped, and so is any `@` block.

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

The cascade is worked out once, as the layout loads (`Shared/Text/MenuStyles.cs`). The layout
properties go into each frame's `style` for the layout bake; the look is written into elements
the client reads as the screen opens: `<colour>`, `<font>`, `<align>`, `<opacity>`, `<hidden/>`,
`<window/>`, `<panel>`, `<tint>`.
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

The game's backdrops are made of pieces of `files/menu_bg_01.NCGR` (a cell bank), so they aren't whole pictures in the picker. Their pieces can be used as sprites of that sheet.

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

**Expressions** (`bind-visible`, and a class's condition in `bind-class`):
- a path or a literal (a number, `'text'`, `true`, `false`, `null`), with simple sums (`maxHp / 4`, `level + 1`);
- compared with `== != < <= > >=`, turned round by `!`, and joined by `&&` and `||`.

A plain value counts as true when it's true, a non-zero number, a non-empty text or list, or anything else that isn't null.

A class a binding switches on brings its stylesheet rules with it. That's the way to colour a hero red when low, or to grey out a row.

Bindings are worked out every frame (`OpenFF.Engine/MenuBindings.cs`), and only what changed is put on. A text is written only when its value changes, so code that writes a bound frame's text keeps it until the bound value moves.

Steam and GOG builds show the layout's own `<data>` text: bindings are OpenFF's.

In Crystal, the inspector's **Bindings** section edits a frame's data source, text template (**+ path** puts a path in at the cursor), visibility and bound classes. It shows what they come to with a sample of the game's data (Luneth, Lv 12, 12,345 G), and Preview draws bound frames that way too (`menu-bindings.js`, a port kept to the same rules).

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
- **Text:** `Text`, `Colour` (a palette colour), `FontSize`, `Visible`.
- **Classes:** `Classes`, `HasClass`, `AddClass`, `RemoveClass`, `ToggleClass`. A change cascades the sheets again and puts on what it changes: colour, size, opacity, panel, hidden. The windows are made again if a panel changed.
- **Style:** `GetStyle`/`SetStyle` for the frame's own look properties, over every sheet; `Opacity` as a shortcut.
- **Bindings:** `DataSource`, `Bind(property, expression)` (`"text"`, `"visible"` or `"class.<name>"`; null unbinds), `Binding(property)`.
- **Structure:** `Parent`, `Children`, `Id`, `Work`, the rect, and `ScreenRect` for drawing over the frame.

Layout properties (`left`, `width`, `flex-direction`…) are fixed when the screen is built; change them in the layout, not from code. Frames need an id for their classes and style to reach the stylesheets. On one of the game's own screens with no mod layout, `SetStyle` puts colour, font size, opacity and visibility on directly.

`MenuBindings.Format`, `Test` and `Resolve` are public, for code of your own that wants the same paths and templates.

## Screens of a mod's own

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

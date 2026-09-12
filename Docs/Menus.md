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

# Text

Every line in the game - dialogue, menu labels, item and spell names, battle chatter -
lives in 1890 `.msd` files: 55431 messages, ten languages. They decode to JSON and
back, byte for byte.

```bash
dotnet run --project Crystal.Editor -- msd Content/Override/en.lproj/eureka_menu.msd
# edit the .json
dotnet run --project Crystal.Editor -- msd-build Content/Override/en.lproj/eureka_menu.json
```

Point `msd` at a directory to do the lot at once, which is the way to get something
greppable:

```bash
dotnet run --project Crystal.Editor -- extract-archives Content ..\extracted
dotnet run --project Crystal.Editor -- msd ..\extracted ..\text
rg -i "crystal" ..\text\en.lproj
```

## The JSON

```json
{
  "format": "MSDA",
  "version": 65536,
  "messages": [
    { "id": 50003, "pages": [ "Item" ] },
    { "id": 50419, "pages": [ "%player_level1%", "%player_level2%" ] }
  ]
}
```

`id` is what everything else refers to: `MenuDefine.xbn` gives the main menu's Item
command `<parameter>50003</parameter>`, and 50003 here is `"Item"`. Event scripts do
the same. `pages` are shown one at a time, advanced by the player. `%name%` is
substituted at runtime - character names, numbers, item names.

Message ids are not per file; they are a global space carved into ranges (menu text
around 50000, field dialogue in the millions). Adding a line means picking an id in
the file's own range and referencing it from a menu or a script.

## Encodings, and a bug

The text is meant to be UTF-8, and `StringUtil.createString` decodes all of it that
way. 15251 of the 55431 messages are not valid UTF-8:

| Language | Not UTF-8 | Total | What they are |
| --- | ---: | ---: | --- |
| `ja.lproj` | 4974 | 7013 | Shift-JIS |
| `es.lproj` | 3521 | 7015 | windows-1252 |
| `fr.lproj` | 2993 | 5911 | windows-1252 |
| `de.lproj` | 1928 | 5913 | windows-1252 |
| `it.lproj` | 1391 | 5913 | windows-1252 |
| `en.lproj` | 117 | 5922 | stray Shift-JIS |
| `ko`, `zh_CN`, `zh_TW` | 109 each | 5911 | the same stray Shift-JIS |

A .NET UTF-8 decoder turns each bad byte into U+FFFD, so those messages cannot display
correctly: German `Altarhöhle` is stored as `Altarh<F6>hle` and comes out with a hole
in it. English is nearly clean - its 117 are leftover Japanese debug lines - which is
why the game looks fine in English and why this was never noticed.

The tool keeps such messages byte for byte and marks them:

```json
{ "id": 1000550, "encoding": "latin1", "pages": [ "Altarhöhle" ] }
```

`latin1` maps every byte to the code point of the same value, so nothing is lost and a
rebuild is byte identical. It is a container, not a claim about what the bytes mean.

Converting them properly - windows-1252 and Shift-JIS to real UTF-8 - would fix the
display, and the override path means it can ship without repacking. It is a change to
the game's text, so it is a deliberate step rather than something the decoder should
do quietly.

## Not covered

Control codes inside message text (colour, icons, waits) are passed through as the
bytes they are. `.script` event bytecode, which decides *when* a message is shown, is
still opaque.

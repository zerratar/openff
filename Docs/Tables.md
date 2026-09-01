# Tables

Items, weapons, armour, spells, monsters, and the per-map data behind exits,
encounters and cameras. All of it lives in `.pak` files and decodes to JSON and back,
byte for byte.

```bash
dotnet run --project FF3.ContentTool -- pak ..\extracted\files ..\tables --text=..\text\en.lproj
dotnet run --project FF3.ContentTool -- pak-build ..\tables\item_parameter.json
```

`--text` points at decoded `.msd` JSON and writes each record's name in beside it.

```json
{
  "system": 1,
  "itemId": 1015,
  "nameId": 10005,
  "nameText": "Air Knife",
  "captionId": 15005,
  "captionText": "Attack: 89  Deals wind damage.",
  "aggressivity": 89,
  "hitProbability": 100,
  "armsAttribute": 10,
  "buy": 26000,
  "equipJob": 4194818
}
```

`nameText` and `captionText` are annotations, not data. A rebuild writes the id and
never the text, so editing them changes nothing - renaming an item means editing the
message it points at, in `.msd`. See `Docs/Text.md`.

## What is in there

| File | Chains | Contents |
| --- | --- | --- |
| `files/item_parameter.pak` | 5 | 36 consumables, 132 weapons, 73 armour, 127 spells, 41 key items |
| `files/monster.chaindata` | 6 | 255 monsters, their drops, attacks, offsets and effects |
| `files/<stage>.pak` | 7 | per map: exits, land forms, monster parties, sounds, encounters, cameras |

337 map paks, all decoding with the same layouts.

## The format

```
+0   chain count
+8   relocated flag - non zero once the game has fixed up its pointers in memory,
     always zero on disk
+16  chains, 8 bytes each: offset, then size
     chain data follows, each chain aligned to 16 bytes
```

A chain is an array of fixed size records. `FF3.ContentTool/PakRecords.cs` holds those
record layouts and is generated, not written:

```bash
python Tools/gen_records.py
```

It reads each record type's own `parse(ArrayReader)` and records the fields in the
order they are read, so the names in the JSON are the game's names.

A parse method is not always flat. A monster embeds its body, physical attack,
physical defence and magic defence as sub-records, and reads an array of special
actions in a loop, so nested parse calls are followed and constant-bound loops
unrolled. That matters more than it sounds: counting only the direct reads makes a
monster 27 bytes instead of 100, and every monster after the first would then be read
from the wrong offset - while still looking like plausible numbers.

## Is it right?

Two checks, both cheap and both impossible to satisfy by accident.

**Against the game's own arithmetic.** `ItemManager` counts weapons in steps of 56 and
`mon` counts monsters in steps of 100. Every layout with such a stride is compared
against it, and one that does not add up is reported and not emitted rather than
shipped. That is what caught the nested-record problem above.

**Against the shipped data.** `python Tools/check_records.py <files>` checks that every
chain's byte length divides exactly by its record size, across all 339 files that use
a modelled layout. A wrong layout fails this almost immediately.

Then every decode is checked by rebuilding it and comparing to the original bytes.
343 files, all byte-exact - including chain alignment padding, which is not always
zeros, and trailing padding, which is why the file's length is recorded.

## Not modelled yet

- **`player.chaindata`** - 15 chains of player and job parameters. The job tables are
  in here, and they are the obvious next thing to add.
- **`player_world_move_parameter.pak`, `npc_world_move_parameter.pak`** - movement on
  the world map, read by their own managers.
- **`s01_01.pak`** - 5 chains, the older layout. Its `.script` is version 1.0, which
  the game refuses to load, so this looks like a leftover rather than something live.
- **`f03.pak`, `f04.pak`** and two others are not paks at all; the reader refuses them
  rather than inventing a structure.

Chains without a known layout are kept as hex, so a file with one unmodelled chain
still round trips.

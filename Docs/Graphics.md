# Graphics

## The 2D art is already PNG

542 files - 509 `.NCGR` and 33 `.NCBR` - and every one of them is a PNG. The names are
NDS formats (Nitro character graphics, and its bitmap cousin), which hold indexed tiles
and need a palette to mean anything. These do not: the phone port replaced the lot and
kept the extensions.

That was worth checking rather than assuming, so it was: every archived file was tested
for the PNG signature, and those 542 are the only ones that have it.

| | |
| --- | --- |
| 503 | 8-bit palette |
| 28 | 4-bit palette |
| 11 | 8-bit RGBA |
| 316 | 800×480, the full-screen art |

So this is the one kind of art that can be looked at and replaced **today**, with no
decoder in between. The editor's **Images** tab lists all 542, shows each with a
checkerboard behind it so transparency is obvious, and replaces one from a PNG on your
disk. The game reads them through its own PNG path, so a PNG in the override is a PNG
the game will draw.

A replacement is checked before it is written - a file the game cannot read is worse
than no change - and a different size is allowed but called out, because the game lays
some of these out expecting a particular one.

## The 3D data is NitroSDK, wrapped

The `.lz` archive holds 833 `.nmdp` and 756 `.ntxp`, and both use the same `NMDP`
container:

```
NMDP
  +4   4096          version or alignment
  +24  size
  +28  48            where the payload starts
  then a standard NitroSDK block:
    BMD0  -> MDL0 (models) + TEX0 (textures)      .nmdp
    BTX0  -> TEX0 (textures)                      .ntxp
```

`BMD0` and `BTX0` are NSBMD and NSBTX, which are documented formats, and the game's own
reader is right there - `nmdp.SModelFileHeader` and `ds.sys3d.CModelSet.setup` - so the
wrapper needs no guessing either.

Nothing here is decoded yet. The order that gets the most from the least is:

1. **TEX0** - textures on their own. That is what a monster and a character wear, and
   it is the smaller half of the format.
2. **MDL0** - geometry, which the map view needs before it can draw a town rather than
   dots.
3. **NANR / NCER / NSCR** - the 2D animation, cell and screen tables. The pictures are
   already readable; these say which part of a sheet is used and where it goes, which
   is what a menu preview needs to show the real thing.

## What each format is

| Extension | Count | What it holds | Readable |
| --- | ---: | --- | --- |
| `.NCGR`, `.NCBR` | 542 | pictures, as PNG | yes |
| `.nmdp` | 833 | models and their textures (`BMD0`) | no |
| `.ntxp` | 756 | textures (`BTX0`) | no |
| `.ncap` | 221 | motion | no |
| `.namp` | 239 | animation | no |
| `.NANR` | 51 | 2D animation | no |
| `.NCER` | 81 | cells: which part of a sheet is a sprite | no |
| `.NSCR` | 109 | screens: tile maps | no |
| `.mcl` | 322 | map collision (`MCL `) | no |

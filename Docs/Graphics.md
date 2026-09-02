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

## TEX0: the textures

**Done.** All 2828 of them decode. `ff3content tex <dir> <out>` writes the lot as PNG,
and the editor's **Textures** tab shows any package as a gallery.

Of the 1589 packages, 1126 hold a TEX0 - every one of the 756 `.ntxp`, and 370 of the
833 `.nmdp`. The other 463 models carry geometry only and borrow the `.ntxp` of the same
name; 459 of them have one, and the 4 that do not (`o000`, `o024`, `shadow01`,
`shadow02`) are untextured. The tab links a model to its `.ntxp` rather than making you
guess.

### Layout

The block is `NNSG3dResTex`, three near-identical info structs and two dictionaries:

```
+0   "TEX0", size
+8   texInfo    skip, sizeTex>>3, ofsDict, flag, dummy, ofsTex
+24  tex4x4Info the same, then ofsTexPlttIdx
+44  plttInfo   skip, sizePltt>>3, flag, ofsDict, dummy, ofsPlttData
```

A dictionary entry is the texture's `texImageParam`:

| bits | |
| --- | --- |
| 0-19 | where its pixels start, in 8-byte units |
| 20-22 | width = `8 << value` |
| 23-25 | height = `8 << value` |
| 26-28 | format |
| 29 | whether palette entry 0 is transparent |

Names are 16 bytes and mostly ASCII, but not all: `n211` and `n281` were typed with a
Japanese keyboard still in wide mode and begin with a fullwidth `ｎ`. They are read
through the game's own Shift-JIS table, the same one the dialogue uses.

Palettes are paired to textures by name - `sougen01` takes `sougen01_pl` - falling back
to position. The game's own fast path only ever handles one of each, so the convention
is the only rule there is; 1111 of 1126 packages have one palette per texture anyway.

### Checking it

Byte-exact round-trip is not available here: writing a TEX0 means re-quantising, so
there is nothing to compare against. Instead the structure checks itself.

Every texture's extent was computed from its own address, size and format, and compared
with every other texture in the same package:

- **no overlapping pairs**, across all 2828. Getting width, height, format or address
  wrong anywhere would collide with a neighbour.
- **963 packages fit their declared block to the byte**, with nothing left over.
- 599 textures appear to run past `sizeTex` - but every overrun is an exact multiple of
  524288, and `sizeTex` is a `u16` counting 8-byte units, so it simply wraps at 512 kB.
  A wrong reading would not miss by a round number.

The pixel conversion is `GlobalScope.LoadTexture` ported case for case, including the
things that look like mistakes and are not: `pal256` scales colour by `*255/31` while
every other format shifts up by 3 and tops out at 248, and interpolated 4x4 pixels come
out with alpha 248 rather than 255. The game draws it that way, so this does too.

| Format | Count | |
| --- | ---: | --- |
| `pal256` | 1786 | one byte per pixel |
| `4x4` | 464 | 4x4 blocks sharing two or four colours |
| `a5i3` | 294 | 3 bits of index, 5 of alpha - shadows |
| `pal16` | 211 | half a byte per pixel |
| `a3i5` | 40 | 5 bits of index, 3 of alpha |
| `pal4` | 17 | a quarter byte per pixel |
| `rgb555` | 16 | the colour itself, 15 bits, plus 1 of alpha |

Textures are read-only. Putting one back means writing a TEX0 - re-quantising to a
palette, or to 4x4 blocks - which is a larger job than reading one, and the tab says so
rather than offering a button that half works.

## MDL0: the geometry

**Done.** All 833 models decode. `ff3content mdl <dir> <out>` writes them as OBJ with
their textures, and the editor's **Models** tab draws them in the browser - drag to
orbit, wheel to zoom.

A model is three things stacked on each other:

- **Shapes** hold NDS *display lists* - GPU command streams, not vertex buffers.
  Commands come four to a 32-bit word and their parameters follow the word they were
  packed into, which is why walking one needs two pointers rather than one.
- **The SBC** is a small byte code that walks the node tree, builds a matrix for each
  node and says "draw shape 3 with material 1, here". Ten opcodes, all used.
- **Materials** name a texture, resolved through a dictionary that runs backwards:
  each texture name lists the materials that use it.

### Checking it

Two independent checks, and they agree.

Every model records its own `numVertex`, `numTriangle` and `numQuad`, and its own
bounding box. Both are things a decoder cannot fake: lose your place in a display list
and the counts drift, misplace a matrix and the geometry bursts out of the box.

| | |
| --- | ---: |
| counts match the model's own | **832 / 832** |
| geometry fits the declared box | **832 / 832** |

The 833rd is a 332-byte stub with `numShp = 0`, an empty shape dictionary and
uninitialised counts in its header - not a decode failure, just an empty placeholder.

Those two numbers were 830 and 708 before two real bugs were found, and it is worth
recording what found them.

- **4 missing vertices in b32** turned out not to be a bug at all: one shape is switched
  off by its node's visibility flag, so the game never draws it, while the header still
  counts it. The same is true of one shape in t10_02, and those two are the only ones in
  the game. They are decoded and kept, marked hidden, rather than silently dropped.
- **124 models burst out of their box**, which was the real bug. The display list's
  `MTX_RESTORE` picks which stack matrix the following vertices go through, and it was
  being skipped - harmless for a rigid prop, fatal for a character, whose arm then lands
  at the origin instead of on its arm bone. `preBuild` ignores that opcode because the
  game applies it in its render pass instead, which is exactly the sort of thing that
  only shows up when two sources are read against each other. Fixing it took the box
  check from 708 to 832 of 832 - the box had been right all along.

The bind pose is what comes out. The game's animation blend keeps a weight of 1.0 when
nothing is bound, so a static read takes that branch and gets the rest pose. Billboards
keep their base matrix, since which way they face depends on a camera that is not there.

## What is left

**NANR / NCER / NSCR** - the 2D animation, cell and screen tables. The pictures are
already readable; these say which part of a sheet is used and where it goes, which is
what a menu preview needs to show the real thing.

## What each format is

| Extension | Count | What it holds | Readable |
| --- | ---: | --- | --- |
| `.NCGR`, `.NCBR` | 542 | pictures, as PNG | yes |
| `.nmdp` | 833 | models (`BMD0`), 370 with their own textures | yes |
| `.ntxp` | 756 | textures (`BTX0`) | yes |
| `.ncap` | 221 | motion | no |
| `.namp` | 239 | animation | no |
| `.NANR` | 51 | 2D animation | no |
| `.NCER` | 81 | cells: which part of a sheet is a sprite | no |
| `.NSCR` | 109 | screens: tile maps | no |
| `.mcl` | 322 | map collision (`MCL `) | no |

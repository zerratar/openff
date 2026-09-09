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

**Done.** All 2828 of them decode. `crystal tex <dir> <out>` writes the lot as PNG,
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

**Done.** All 833 models decode. `crystal mdl <dir> <out>` writes them as OBJ with
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

### Writing one

`crystal mdl-import <file.glb> <name> [out-dir] [--scale=n]` goes the other way
(`Crystal.Editor/Mdl0Write.cs`): a glTF becomes `<name>.nmdp.lz` - an NMDP wrapping a
BMD0 with one MDL0 - and `<name>.ntxp.lz` with its textures (`Tex0Write.Build`). The
layout is `w005.nmdp`'s, read off the bytes: the 48-byte NMDP head (kind 2, the BMD0's
size at 24, its offset at 28), the model dictionary, then the model's five offsets, the
44-byte info (position scale, counts, the box under its own scale), the node dictionary
with one identity node (flag `0xF807`), the SBC (`NODEDESC`, `NODE`, then `MAT i` / `SHP i`
for each material, `RET`), the material block with its three dictionaries (materials, and
the two that run backwards from texture and palette names to material lists) and 44-byte
material records copied from w005's words, and the shape block - a 16-byte header and a
display list per shape: `BEGIN_VTXS` triangles, then `TEXCOORD` (texels, 12.4), `NORMAL`
(10-bit) and `VTX_16` (1.12 under the position scale) per vertex, `END_VTXS`, commands
four to a word with their parameters after the word. Every glTF material becomes one
material, one shape and one pal256 texture (its picture times the base colour, or 8x8 of
the colour); the total texel size is kept under the 512 KB a texel block's 16-bit size
field can say. Skins, animations and vertex colours are not written; the meshes go in as
the file's node tree places them.

Checking it is the reader above: `mdl-import` reads its own output back and prints the
counts and the box, and `crystal mdl` dumps it as OBJ like any shipped model; the model
viewer draws it; the client plays it (a `w300` in a weapon definition's *Model* shows in
the hero's hand in battle - Testing.md C-79). One thing the writer found in the reader's
neighbour: the dictionaries' patricia tree must number every child after its parent, since
the game's lookup walks while the index grows and treats a step back as the end
(`NNS_G3dGetResDictIdxByName`); `Tex0Write.Dictionary` numbers its nodes by a walk from
the root now, which also fixes a package of three or more textures from *New texture
package…*.

## NCER, NSCR and NANR: which piece goes where

**Done.** `crystal cells <dir> <out>` writes them all as JSON, and the editor's
**Cells** tab composes each one against its sheet and shows the result.

The pictures were already readable, but a picture is a sheet of parts. These are the
tables that turn one back into a window frame, a button, or a whole menu screen.

### The names lie

106 of the 109 `.NSCR` files are **cell banks**, not screens. Only 3 are really screens.
The block tag inside is the only thing that decides, and the game agrees - its own `Nscr`
loader tries `SCRN` first and falls back to `CEBK` when that fails.

| | | |
| --- | ---: | --- |
| `CEBK` cell banks | 187 | 81 `.NCER` + 106 `.NSCR` |
| `SCRN` screens | 3 | the only real ones |
| `ABNK` animation | 51 | all the `.NANR` |

### It is not NDS OAM either

Real OAM packs a sprite into three cryptic 16-bit words. The port threw that out and
wrote seven plain ones, which is far easier to work with:

| | |
| --- | --- |
| 0, 1 | where to put it, relative to the cell's origin |
| 2, 3 | how big it is |
| 4, 5 | where to take it from, in the sheet |
| 6 | flags: 1 flip across, 2 flip down, 4 half size, 8 squash to 0.6 x 2/3 |

That is read straight off the game's own draw call, which passes exactly those values to
`drawImage(x, y, w, h, sx, sy, sw, sh)` - flips included, which it does by moving the
source corner and negating the width. 7853 of the 7906 parts carry the squash flag.

### Finding the sheet

A bank never names the sheet it cuts from; the game names it at the call site:

```csharp
bg.bgLoad("menu_000_main.NSCR", "menu_bg_01.NCGR", "new_menu_bg.NCLR");
sprite.Load("name_i.NCER", null, "name_i.NCGR", "name_i.NCLR");
```

so `Tools/gen_cell_pairs.py` reads those out of the game's 1116 source files rather than
guessing. It follows three shapes: the two above, the arrays the menus index into
(`bgLoad(main_bg_nscr[no], "menu_bg_01.NCGR", ...)`), and one call that picks its bank
with a ternary on the language. Where a file names exactly one sheet anywhere in it,
every bank in that file takes it - and a file naming two is **reported rather than
guessed at**, so the rule cannot be quietly wrong.

Everything else uses the sheet of the same name, trying `.NCBR` when there is no
`.NCGR`, since 33 of the sheets are the other kind.

### Checking it

Every part asks for a rectangle out of a sheet whose size is known, so the check is
whether that rectangle is actually in it. A misread field - a size where an offset
belongs, a stride off by two - sends those coordinates somewhere impossible almost at
once.

| | |
| --- | ---: |
| parts whose source rectangle is inside their sheet | **7890 / 7905** |
| animation banks whose frames add up to their own header | **51 / 51** |

The 15 that fail are all the same thing and none of them is a decode error: the cell asks
for more than the sheet has, because the port replaced that art at a smaller size and
left the table alone. `end_20` wants 800x480 from a 640x480 picture; nine `map_marker_*`
want 32x32 from 24x24. The game would sample past the edge of the texture and get away
with it.

One bank of the 187, `mastercard.NCER`, names a sheet that was never shipped. It has one
cell holding one part.

### Drawing it the way the game does

Decoding the geometry right and *drawing* it right are two jobs, and the second one has
its own set of traps. Four, all settled by reading the game's own draw call rather than
by taste:

- **v = 0 is the top of the texture.** The game decodes a texture top row first and
  uploads it as it is, and the shapes pass v straight through. The viewer was flipping on
  upload, which did far more damage than mirroring each quad: most of these textures are
  **atlases**, so a mirrored v made every quad sample the wrong cell of its own sheet.
  `t29_jimen02` is a 128x128 sheet holding grass, a hedge, a stone wall and a road in its
  four corners - flip it and a town's grass comes out as road, its roofs come out upside
  down, and a riverbank comes out blue. This one bug accounted for nearly everything that
  looked wrong.

- **Two passes, opaque then translucent.** The game draws the whole model twice, and the
  split is its own: alpha of 16 or less out of 31, or a texture in format 1 (`a3i5`) or 6
  (`a5i3`), the two that carry alpha per pixel. Drawn in one pass a half-transparent quad
  writes depth and hides what is behind it, which is where the holes in the terrain came
  from. The second pass leaves the depth buffer alone.

- **Nearest filtering, and repeat on both axes.** The game passes 9728 - `GL_NEAREST` -
  for both filters and 10497 - `GL_REPEAT` - for both axes, and it never reads the repeat
  or flip bits the materials carry (all of which are zero anyway). Nearest matters more
  than it sounds here: with atlases, a linear filter bleeds one cell into the next along
  every seam.

- **A normal throws away the colour before it.** On the hardware, the `NORMAL` command
  recomputes the vertex colour from the lighting equation, overwriting whatever the last
  `COLOR` command set. In these files a `COLOR` is *always* immediately followed by one -
  12027 times - so those colours are working values that were never meant to be seen.
  Taking them at face value painted j301's blue coat solid black, and did the same to
  parts of every character that carries normals. Nothing here evaluates lighting, so the
  material's own colour stands in for its result, which is also what the game loads into
  its colour register when it binds a material. Only 888 of the 8294 shapes carry normals
  at all; the other 424735 `COLOR` commands set a colour and mean it, and are untouched.

- **Billboards turn.** A billboard node has its rotation post-multiplied by the inverse
  camera, so the piece faces the viewer. What the decoder bakes is that step with the
  camera at identity, plus the pivot to turn about; the viewer finishes it, and kind 2
  turns only about the vertical axis so a tree stays upright when you look down on it.
  114 of the 833 models have one.

The UVs themselves needed no correction: the game's texture matrix is `1/origWidth` and
that is what the decoder divides by. Worth noting that `origWidth` is *not* the texture's
size - the port doubled 2071 of the 2833 materials' textures and left `origWidth` at the
original, so the two differ by exactly 2 and dividing by the texture size instead would
tile everything twice over.

## What is left

The NitroSDK animation - `.ncap` motion and `.namp`, 460 files - and `.mcl` collision.
Neither blocks anything the editor does today.

## What each format is

| Extension | Count | What it holds | Readable |
| --- | ---: | --- | --- |
| `.NCGR`, `.NCBR` | 542 | pictures, as PNG | yes |
| `.nmdp` | 833 | models (`BMD0`), 370 with their own textures | yes |
| `.ntxp` | 756 | textures (`BTX0`) | yes |
| `.ncap` | 221 | motion | no |
| `.namp` | 239 | animation | no |
| `.NANR` | 51 | 2D animation | yes |
| `.NCER` | 81 | cells: which part of a sheet is a sprite | yes |
| `.NSCR` | 109 | 106 are cell banks, 3 are really tile maps | yes |
| `.mcl` | 322 | map collision (`MCL `) | no |

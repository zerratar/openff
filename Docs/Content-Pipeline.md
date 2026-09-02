# Content pipeline

The shipped game data comes in two very different forms.

## The editor

Most of what follows has a graphical front end:

```bash
dotnet run --project FF3.ContentTool -- editor --content=Content
```

Scripts, menus and text, edited in a browser and written straight to the override
directory. See `Docs/Editor.md`.

## Repository layout

`Content/` is self-contained: a fresh clone is playable with nothing else on disk.

```
Content/
  *.xnb              shipped audio and font atlases   (389 MB)  - runtime + archival master
  data*.bin, dl.bin  shipped archives                 (144 MB)  - runtime, no pipeline form yet
  Font12.glp, Font16.glp                              (256 KB)  - runtime
  Audio/*.wav        extracted, editable              (346 MB)
  Fonts/*.png +.json extracted, editable              ( 14 MB)
  Content.mgcb       pipeline project for the above
```

The game finds this automatically: `ContentLocator` walks up from the executable
looking for a directory named `Content` that holds `data000.bin`.

### On retiring the original XNBs

Once the pipeline rebuild is verified, it is tempting to stop tracking the shipped
`.xnb` files, since `Audio/` and `Fonts/` duplicate them. Two reasons to keep them
anyway:

- **Deleting them later reclaims nothing.** Git history retains the blobs and the LFS
  objects stay in the remote store; only a history rewrite plus a server-side prune
  actually frees the space. `git rm` just hides them from the working tree.
- **The rebuild is not lossless.** Fonts ship as DXT3 and come back as PNG, which the
  pipeline then re-encodes with a different compressor — not the original bytes. Audio
  extraction *is* lossless (a remux, no transcode), but rebuilding through
  `SoundEffectProcessor` may re-encode. The shipped XNBs are the only exact copy.

So they are better treated as the archival master: the ground truth everything else can
be re-derived from. `Audio/` and `Fonts/` are the editable working copy, and once the
pipeline output is wired up it becomes the thing the game actually loads.

If a lean checkout is ever wanted, that is a clone-time concern rather than a history
one - `git lfs clone --exclude` or a sparse checkout skips the archival files without
losing them.

## What ships where

| Source | Contents | Read by |
| --- | --- | --- |
| `Content/*.xnb` | Audio (BGM, SE) and font atlases | XNA ContentManager |
| `Content/data*.bin` | Maps, sprites, models, scripts — everything else | The game's own archive code, pure C#, no pipeline involved |
| `Content/Font12.glp`, `Font16.glp` | Glyph tables: character → atlas page + vertical shift class | Read directly as bytes |

Only the XNB half touches the content pipeline. The `data*.bin` archives are read by
`ds.CFile` / `ds.Archive` and were never a pipeline concern, which is why they port
without any work.

The XNBs are `XNBm` (Windows Phone), version 5, **uncompressed**, Reach profile. MonoGame
accepts the `m` platform byte directly, so the old platform-byte rewriting tool is not
needed.

## Extracting

```bash
dotnet run --project FF3.ContentTool -- info    <xnb-dir>
dotnet run --project FF3.ContentTool -- extract <xnb-dir> ./Content
```

`extract` produces:

```
Content/
  Fonts/Font12_0.png   glyph atlas, RGBA
  Fonts/Font12_0.json  glyph bounds, cropping, kerning, line spacing
  Audio/BGM00_1.wav    audio, remuxed not transcoded
  Content.mgcb         pipeline project for the above
```

Current inventory: **167 font pages, 475 audio files, 0 skipped.**

### Fonts

Atlases ship as 256×256 **DXT3**. The tool decodes DXT1/DXT3/DXT5 and writes straight
RGBA PNG. Each atlas is a premultiplied white alpha mask — RGB equals alpha at every
pixel — so the glyph colour comes entirely from the tint at draw time.

Fonts come back as *textures plus metrics*, not as `.spritefont` descriptions. The
originals are pre-rendered atlases whose exact glyph boxes the game indexes by page and
character; regenerating them from a system font would not reproduce those. To restyle the
text, edit the PNGs and keep the boxes, or regenerate atlases and the `.glp` tables
together.

### Audio

The XNB stores a `WAVEFORMATEX` block followed by raw sample data, so extraction is a
remux into RIFF/WAVE with no transcoding — including the MS-ADPCM tracks (format tag 2,
22 kHz), which keep their format tail and get the `fact` chunk decoders expect.

## Rebuilding

`Content/Content.mgcb` is generated alongside the extracted files and can be built with
the MonoGame Content Builder:

```bash
dotnet tool install -g dotnet-mgcb
mgcb /@:Content/Content.mgcb
```

This is the path for replacing assets: edit the PNG or WAV, rebuild, and the game picks
up the new XNB. The game currently loads the *original* XNBs from `Unpacked/Content`; to
run against rebuilt content, point `FF3_CONTENT` at the pipeline output directory.

## The data archives

Everything that is not audio or a font atlas - sprites, maps, models, event scripts,
the parameter tables - lives in `Content/data*.bin` and is read by the game's own
code. The format is in `Shared/ArchiveFormat.cs`, compiled into both the game and the
content tool so there is exactly one implementation of it.

See what is in there:

```bash
dotnet run --project FF3.ContentTool -- archives Content
```

```
6962 files across 52 volumes

  2734  .lz          2734 LZ-compressed blobs
  1890  .msd         message/dialogue data
   509  .NCGR        NDS character graphics
   357  .script      event bytecode
   356  .hich
   345  .pak
   ...
```

Extract, all of it or a subset:

```bash
dotnet run --project FF3.ContentTool -- extract-archives Content ..\extracted
dotnet run --project FF3.ContentTool -- extract-archives Content ..\extracted "en.lproj/*" "*.script"
```

Patterns are globs over the archived name. The output mirrors the archive's own
layout, which matters: names are path qualified (`en.lproj/ca_text_01.NCGR`) and the
localised copies share base names, so flattening would lose about 2000 files. A
`manifest.tsv` records name, volume, index and size for every file written.

## Changing content without repacking

Nothing has to be packed back. `GameArchive.Read` looks in `Content/Override/<name>`
before it looks in the archives:

```bash
dotnet run --project FF3.ContentTool -- extract-archives Content Content/Override "files/*.script"
# edit Content/Override/files/whatever.script
FF3.exe
```

`--content-override=<dir>` points somewhere else instead - handy for keeping a whole
extracted tree outside the repository and switching between variants. The log records
which files were served loose:

```
File  content overrides: 6963 loose file(s) in ...\extract-all
File  override in use: files/item_parameter.pak
File  override in use: en.lproj/eureka_menu.msd
```

`Content/Override` is gitignored apart from its README, so experiments stay local.
Content meant to ship goes in deliberately.

## Menu definitions

The `.xbn` files are binary XML and decode to editable XML and back, byte for byte:

```bash
dotnet run --project FF3.ContentTool -- xbn       files/MenuDefine.xbn
dotnet run --project FF3.ContentTool -- xbn-build files/MenuDefine.xml
```

See `Docs/Menus.md`.

## Game text

The `.msd` files hold every line in the game and decode to JSON and back:

```bash
dotnet run --project FF3.ContentTool -- msd       ..\extracted ..	ext
dotnet run --project FF3.ContentTool -- msd-build en.lproj/eureka_menu.json
```

See `Docs/Text.md`, which also covers the 15251 messages that are not the UTF-8 the
game expects.

## Event scripts

The `.script` files are the event bytecode and disassemble, with dialogue written in
beside the instructions that show it:

```bash
dotnet run --project FF3.ContentTool -- script ..\extracted\files ..\scripts --text=..\text\en.lproj
```

They also compile back, from a text language with a real lexer, parser and compiler:

```bash
dotnet run --project FF3.ContentTool -- script-build ..\ffs\d01_02.ffs Content\Override\files
```

See `Docs/Events.md` and `Docs/Script-Language.md`.

## Parameter tables

Items, monsters and per map data decode to JSON and back:

```bash
dotnet run --project FF3.ContentTool -- pak ..\extracted\files ..\tables --text=..\text\en.lproj
```

See `Docs/Tables.md`.

## Compression

2734 archived files are LZ77 compressed and unpack with:

```bash
dotnet run --project FF3.ContentTool -- lz ..\extracted\files ..\unpacked
```

See `Docs/Compression.md`.

## Not decoded yet

What is left is the NitroSDK data itself: `NMDP` models and textures, `NAMP` and
`NCAP` animation, `MCL` collision, and the `.NSCR`/`.NCER` tables. The pictures
themselves turned out to need no decoder at all - all 542 are PNGs. `Docs/Graphics.md`
has what each format holds and which order is worth taking them in.

`--dump=<dir>` is the shortcut in the meantime: the running game writes out every
image blob it decodes, which covers the art without decoding anything by hand.

# Content pipeline

The shipped game data comes in two very different forms.

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

## Not yet extracted

The `data*.bin` archives hold all the sprites, maps, models and event scripts. They are
read by the game's own code and are not decoded by the tool yet. In the meantime,
`FF3_DUMP=<dir>` makes the running game write out every image blob it decodes, which
covers the PNG-backed art without needing an archive reader.

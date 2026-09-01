# Content pipeline

The shipped game data comes in two very different forms.

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

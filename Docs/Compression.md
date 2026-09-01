# Compression

2734 of the 6962 archived files are LZ77 compressed - most of the models, textures,
animation and collision data. They decompress and compress again:

```bash
dotnet run --project FF3.ContentTool -- lz ..\extracted\files ..\unpacked
dotnet run --project FF3.ContentTool -- lz-compress ..\unpacked\b01.nmdp
```

119 MB of `.lz` becomes 238 MB of data.

## The format

NDS LZ77, the same as the game's own `MI_ReadUncompLZ8`:

```
+0  one 32 bit word: low nibble parameter, next nibble type (1 = LZ77),
    top 24 bits the decompressed size
+4  a flag byte, then eight items, repeating:
      flag bit 0   one literal byte
      flag bit 1   two bytes: length = (first >> 4) + 3
                   distance = ((first & 0xF) << 8) + second + 1
```

Worth knowing which routine to mirror: `MI_UncompressLZ8`, the one-shot version, is an
empty stub in this build. The live path is streaming - `prepareReadFile` then
`updateReadFile` - and that one is fully implemented. The decompressor here follows it.

## Is it right?

Every file is checked twice on decompression: the output has to be exactly the size the
header promised, and compressing it again has to decompress back to the same bytes. All
2734 pass both.

The compressor is a plain greedy matcher and will **not** reproduce a shipped `.lz` byte
for byte - the original compressor is not here to copy. That is why the check is
`decompress(compress(x)) == x` rather than `compress(x) == original`. For putting edited
content back, that is the property that matters; for leaving content alone, keep the
original file.

## What is inside

| Extension | Count | Magic | What it is |
| --- | ---: | --- | --- |
| `.nmdp` | 833 | `NMDP` | model data |
| `.ntxp` | 756 | `NMDP` | textures, in the same container |
| `.flsc` | 363 | - | field script/scene data |
| `.mcl` | 322 | `MCL ` | map collision |
| `.namp` | 239 | `NAMP` | animation |
| `.ncap` | 221 | `NCAP` | motion |

These are NitroSDK containers, which the game reads through its `NNSG3d` code. Decoding
them - so a model or a texture can be looked at, let alone edited - is its own project
and is not started. The archive extractor and this decompressor get you the bytes; what
they mean is still the game's business.

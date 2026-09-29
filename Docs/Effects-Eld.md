# The game's effects: eld's formats and runtime

The reference for FF3's effect packs as `OpenFF/GlobalScope/eld` reads and plays them - what
`Shared/Effects/Efp.cs` reads and `Shared/Effects/EffectImport.cs` turns into the new format
(`Docs/Effects-Plan.md`). Read from the port's code and checked against the Steam data with the
script at the end; GUESS marks what neither settles.

Source of truth: the decompiled port in `OpenFF/GlobalScope/eld/*.cs`, plus `ds/ds.Texture.cs`, `ds/ds.pt.cs`
(`PrimitiveDisplay.drawParticles`), `ds/sys3d/sys3d.ParticleElement.cs`, `GlobalScope.Members.cs`
(`LoadTexture`, `G3_*`, `FX_*`, `MTX_*`, `rand`), `btl/btl.BattleEffect.cs`, `btl/btl.TurnSystem.cs`.
Everything below was read from that code and cross-checked against the Steam data files
(`...\Final Fantasy III\files\`) with the Python at the end (section 12).

Conventions used in this document:

* **Byte order** is little-endian everywhere (ArrayReader).
* **fx32** = signed 32-bit fixed point with 12 fractional bits: `4096 = 1.0`. All positions, speeds, sizes,
  scales and radii are fx32. **fx16** = signed 16-bit with 12 fractional bits (vertex offsets, sin/cos).
* **FX_Mul(a,b)** = `(int)(((long)a*b + 2048) >> 12)` (rounded, 64-bit intermediate, arithmetic shift).
  **FX_Div(n,d)** = `(int)(((long)n << 12) / d)` (truncating; returns `n` when `d == 0`).
* **Angles** used at runtime are DS "index" angles: 16-bit binary angle, `65536 = 360 degrees`, taken
  `(ushort)` before use. `FX_SinIdx(i) = round(sin(i * 2*pi/65536) * 4096)` (fx16), same for cos.
  (See 5.3 for the emit-angle unit mismatch.)
* **Time** is in game steps. The game steps 30 times per second (FramePacer: "thirty a second"; one
  `NitroMain` = one step). Every counter below advances by 1 per step.
* **Colour** channels and alpha are integers `0..31` (DS 5-bit). Polygon alpha `31` = opaque, `0` = hidden.
* "Port" = the C# code as it is; "DS" = what the original C++ must have done where the port visibly
  diverges (reference-vs-value bugs). Where they differ it is said explicitly.
* **GUESS** marks anything not established by code or data.

---------------------------------------------------------------------------------------------------------

## 1. effect.efi (index) and template lookup

### 1.1 Layout

The file is loaded whole as a `uint[]` (`ServerFF3.loadID`, `SIDTable.initialize`).

| offset | type | meaning |
|---|---|---|
| 0x00 | u32 | `nbCtgrs` - number of categories (Steam data: 454 = 0x1C6) |
| 0x04 | u32[3] | unused (0) |
| 0x10 + 8*c | u32 `addr`, u32 `nbMem` | category `c`: byte offset (from file start) of its member table, and member count |
| `addr` + 4*m | u32 | template ID of member `m` of that category |

Categories with no effect have `(0, 0)`. Member 0 is always ID 0 (= none); real members start at 1.

### 1.2 (category, member) -> template ID

```
getTemplateID(ctgr, mem):
    if ctgr >= nbCtgrs: return 0
    return pTable[(infoCtgrs[ctgr].addr >> 2) + mem]      // no bounds check on mem
```

The category number is also the pack number: `BattleEffect.addEfp(categoryId)` loads
`/EFFECT/e%03d.efp` (e.g. category 331 -> `e331.efp`). Member 1 of a magic category is (in all packs looked
at) the root SequenceDS that boots the other members; the battle code does `create(category, 1)`
(sometimes 2).

Example (Steam effect.efi):

```
331 (Fire)     : [0, 0xe0d, 0xe08, 0xe06, 0xe09, 0xe0c, 0xe07]
332 (Blizzard) : [0, 0x245, 0xe8c, 0x238, 0x233, 0x2b9, 0x107e]
301 (Cure)     : [0, 0x230, 0x9d5, 0x226, 0x237, 0x232]
407 (black cast): [0, 0xa11, 0xa0c, 0xa91, 0xa92, 0xfef]
408 (white cast): [0, 0xab4, 0xaaf, 0xab6, 0xabb, 0xab1, 0xab0, 0x2c4, 0x2d6, 0x2d2, 0x2d7, 0x2d3, 0x2d8]
```

### 1.3 Template ID -> Template (`Manager.getTemplate(ID)`)

Linear search over all *registered* packs in load order (`_listEfps`: `e201.efp` and `e435.efp` are loaded at
battle setup, then the packs `addEfp` loads), and inside each pack over `pTemplate[0..uiNumTemplate-1]`;
the first template whose `getOwnID()` (the u32 at +0x10 of the template, 2.3) equals ID wins. Template IDs
are global; the same ID can appear in several packs (e.g. 0x2c4..0x2d8 are in both e333 and e408) - first
loaded wins.

`Manager.createObject(category, member)`: `getTemplate(getTemplateID(c, m))` -> the factory whose GUID
equals the template's GUID creates the object (`createObj`: allocate + `prepare()`), it is added to the
object list and to the IGL, `setNumberInformation(c, m)`, then `Start(0)` (which calls `Initialize` at once).

---------------------------------------------------------------------------------------------------------

## 2. .efp pack

### 2.1 SFileHeader (`eld.SFileHeader.cs`)

| offset | type | field | notes |
|---|---|---|---|
| 0x00 | u32 | `uiName` | ASCII `"EFCT"` in all packs |
| 0x04 | u16 | `uiNumTemplate` | |
| 0x06 | u16 | `uiRegistFlag` | 0 in file; set to 1 by `registerEfp` after the templates were initialised (a second register skips `initTemplate`) |
| 0x08 | u32 | `TexAddr` | file offset where the texture/model resource region starts (first `NTPK`/`NMDP`). **Not used by the port.** GUESS: on DS the region after it was freed after VRAM upload. |
| 0x0C | u32 | `version` | observed `(category << 16) | 0x1000` (e331: 0x014B1000, e332: 0x014C1000, e301: 0x012D1000) - observation only, not read |
| 0x10 | u32[n] | `pIndex` | file offset of each template (absolute) |

The first template follows, 16-byte aligned (0x30 for 6 templates, 0x40 for 12) - observation.

All other offsets in the pack (`pAnim`, `pTexture`, `pIndex`) are **absolute file offsets**
(`Template.pAddr` is the whole file; `GetTextureAddr`/`GetAnimAddr` seek to them directly).
`ToAbsoluteAddress` is a no-op in the port.

Rough file layout (e331): header, templates (each: 32-byte head + parameter block + trailing string),
animation blocks (`Eff_AnimationHeader`s, NCAP headers), then from `TexAddr` the textures (`NTPK`) and model
containers (`NMDP` wrappers).

### 2.2 Factory GUIDs and template kinds

GUID = `{u32 Data1, u16 Data2, u16 Data3, u8 Data4[8]}` (16 bytes, little-endian fields), compared field by field.

| kind | factory | GUID Data1-Data2-Data3-Data4 | as bytes in file (first 8) | template version (data) |
|---|---|---|---|---|
| ParticleDS | ImpParticleDSFactory | 91917E57-9AF8-4DA3-{89 98 83 BD B6 27 FA 28} | `57 7e 91 91 f8 9a a3 4d` | 0x1200 (504 templates) |
| ParticleLargeDS | ImpParticleLargeDSFactory | 605FD8B8-992E-41FF-{95 6B F5 85 B0 18 39 B0} | `b8 d8 5f 60 2e 99 ff 41` | 0x1200 (280) |
| ParticleGatherDS | ImpParticleGatherDSFactory | CC29F917-5900-4E67-{96 23 D1 0C BA D0 CC B5} | `17 f9 29 cc 00 59 67 4e` | 0x1100 (14) |
| ModelDS | ImpModelDSFactory | A36D0390-F0AB-417A-{A1 2F BC B9 ED F5 1F 3E} | `90 03 6d a3 ab f0 7a 41` | 0x1010 (85) |
| SequenceDS | ImpSequenceDSFactory | 63511AB8-3F85-4156-{BC C4 8A 91 8E 5A 9D F3} | `b8 1a 51 63 85 3f 56 41` | 0x1000 (231) |

(Decimal Data1 in the C#: 2442231383, 1616894136, 3425302807, 2741830544, 1666259640.) Every template in
every Steam pack is one of these five; all versions are uniform, so a single layout per kind suffices.
Version constants in `eld.Members.cs`: `VER_CURR_PDS/PLDS = 0x1200`, `VER_CURR_GDS = 0x1100`,
`VER_CURR_MDS = 0x1010`, `VER_CURR_SEQ_DS = VER_CURR_PATH_DS = 0x1000`. The port never branches on them.

### 2.3 Template (`eld.Template.cs`)

| offset (from template start) | type | field |
|---|---|---|
| 0x00 | GUID (16) | `_FactoryGUID` |
| 0x10 | u32 | `_uiTemplateID` - the ID matched by `getOwnID()` |
| 0x14 | u32 | `_uiVersion` |
| 0x18 | u32 | `_pAnim` - absolute offset of `Eff_AnimationHeader` (particles) / `ModelDSNcapHeader` (model); 0 for sequences |
| 0x1C | u32 | `_pTexture` - absolute offset of `NTPK` texture (particles) / `ModelDSNmdpHeader` (model); 0 for sequences |
| 0x20 | ... | parameter block (`GetParameter()` returns a reader positioned here) |

Template size is not stored; it is the distance to the next template (or to the anim region).

Parameter block sizes: ParticleDS/LargeDS 144 (0x90), GatherDS 84 (0x54), ModelDS 260 (0x104), SequenceDS
`PathOffset` + path data. After a particle parameter block the files carry a NUL-terminated ASCII source
texture path (e.g. `texture\battle\black_magic\fire\fire_tubu_32bit.tga`) and padding up to the next
16-byte boundary / next template; **nothing reads it** (it is a tool leftover, GUESS on its purpose).

### 2.4 Registration (`Manager.registerEfp`, factory `initTemplate`)

For each template with a matching factory:
* particle kinds: `VramManager.registerTexture(Texture at pTex)` (allocates texel+palette VRAM; if a texture
  with the same 32-byte database name is already registered, it is shared - `Texture.applyShare`), and
  `Eff_AnimationHeader.cast(pAnim)` is parsed.
* ModelDS: marks the NMDP/NCAP headers initialised and registers the model texture container.
* SequenceDS: nothing.

---------------------------------------------------------------------------------------------------------

## 3. Particle parameter blocks

All three particle kinds start with `IParticleSetup`. Offsets below are relative to the parameter block
start (template + 0x20).

### 3.1 IParticleSetup (40 bytes, `eld.IParticleSetup.cs`)

| off | type | field | meaning |
|---|---|---|---|
| 0 | u32 | `version` | = template version |
| 4 | u32 | `flag` | `enFLAG_PDS` bits, below |
| 8 | fx32[3] | `suRange.vRangeBirth` | birth box half-extents (x,y,z) |
| 20 | fx32 | `suSize.nSizeBase` | particle half-size base |
| 24 | fx32 | `suSize.nSizeRand` | + random `[0, nSizeRand)` |
| 28 | u16 | `usTimePlay` | emitter play length (steps) |
| 30 | u16 | `usTimeGroupLife` | life of one group (steps) |
| 32 | u16 | `usTimeInterval` | steps between group spawns |
| 34 | u16 | `nbChilds` | particles per group |
| 36 | u16 | `nbGroups` | number of groups (max spawns per play cycle) |
| 38 | u16 | `_pad0` | |

`flag` bits (`enFLAG_PDS`):

| bit | name | runtime effect |
|---|---|---|
| 0x01 | LOOP | emitter restarts after `usTimePlay` (until stopped) |
| 0x02 | AFTERIMAGE | allocate/draw after-image trail (3.5) |
| 0x04 | FADE | apply `FadeSetup` |
| 0x08 | MOVE_OFFSET | particles are stored relative to the emitter and follow it (6.2) |
| 0x10 | XLU_DEPTH | not used by the port (GUESS: DS "translucent depth update" polygon attr) |
| 0x20/0x40/0x80 | TEXOUT_AUTO / A5I3 / A3I5 | tool export options; ignored (the format comes from the NTPK header) |

Observed flag combinations (798 particle templates): `A3I5` 561, `MOVE_OFFSET|A3I5` 99, `FADE|A3I5` 52,
`LOOP|A3I5` 36, `0` 24, `LOOP|MOVE_OFFSET` 8, `AFTERIMAGE` 5, others <= 4.

### 3.2 Sub-setups

| struct | size | layout |
|---|---|---|
| RangeSetup | 12 | fx32 `vRangeBirth.x, .y, .z` |
| SizeSetup | 8 | fx32 `nSizeBase`, fx32 `nSizeRand` |
| SpeedSetup | 20 | fx32 `vSpeedDir.x,y,z`, fx32 `nSpeedPow`, fx32 `nSpeedRand` |
| GravitySetup | 20 | fx32 `vGravityDir.x,y,z`, fx32 `nGravityPow`, fx32 `nGravityRand` |
| EmmitSetup | 12 | s32 `vEmmitAngle.x,y,z` (angle, see 5.3) |
| AfterImageSetup | 10 | u16 `usNbAfters`, s16 `vColor.r, .g, .b, .a` (colour delta at the trail end, -31..31) |
| FadeSetup | 14 | u16 `usStart`, u16 `usTime`, s16 `vColorSub.r, .g, .b, .a`, u16 `_pad0` |
| CircleSetup | 12 | fx32 `nRadius`, fx32 `nRadiusAdd` (per step), s32 `nAngleAdd` (index angle per step) |
| SizeSpreadSetup | 16 | s32 `nSizeAdd`, s32 `nReserve[3]` - **never used** (all zero in data) |
| GatherSetup | 20 | fx32 `nSpeedPow`, fx32 `nSpeedAdd`, s32 `vRotate.x,y,z` (angle) |

Note the structs are packed exactly as read (no alignment padding between them; the port reads
sequentially and the data sizes add up: e.g. AfterImage 10 + Fade 14 = 24).

Channel order note: AfterImageSetup and FadeSetup are read as (r, g, b, a) and applied to red, green, blue,
alpha by those names. The animation colour table (4.3) is stored (R, B, G, A) - see 4.3. Whether the fade /
after-image vectors are really (R,G,B,A) or also (R,B,G,A) cannot be decided from data (they are almost
always only alpha, e.g. `(0,0,0,-31)`); implement them as the port does (R,G,B,A). GUESS-level caveat.

### 3.3 ParticleDSSetup (ParticleDS and ParticleLargeDS; 144 bytes)

| off | struct |
|---|---|
| 0 | IParticleSetup (40) |
| 40 | SpeedSetup (20) |
| 60 | GravitySetup (20) |
| 80 | EmmitSetup (12) |
| 92 | AfterImageSetup (10) |
| 102 | FadeSetup (14) |
| 116 | CircleSetup (12) - used by ParticleDS only; LargeDS reads and ignores it |
| 128 | SizeSpreadSetup (16) - unused |

### 3.4 ParticleGatherDSSetup (84 bytes)

| off | struct |
|---|---|
| 0 | IParticleSetup (40) |
| 40 | AfterImageSetup (10) |
| 50 | FadeSetup (14) |
| 64 | GatherSetup (20) |

### 3.5 Runtime meaning of each field (summary; formulas in 5)

* Range: birth offset per axis = `-r + rand(2r)` if `r != 0`, else 0 -> uniform in `[-r, r)` (a box, not a sphere).
* Size: `_enSize = base + rand(sizeRand)` (if sizeRand != 0). It is the **half-size** of the quad before animation scale.
* Speed: `v0 = dir * (pow + rand(speedRand))` (fx multiply per component), then rotated by the emit matrix.
* Gravity: per group `G = dir * (pow + rand(gravRand))`; per particle `g = G * (pow + rand(gravRand))`
  (rand part only if gravRand != 0). Note pow is applied twice: with rand = 0, `g = dir * pow * pow`.
  `g` is added to the velocity each step. There is **no drag**.
* Emit: random rotation of the speed vector (5.3).
* Circle (ParticleDS only): each particle gets `radius = nRadius`, `angle = rand(65535)`; each step
  `radius += nRadiusAdd`, `angle += nAngleAdd`; draw position gets `(+r*sin a, 0, +r*cos a)` (world X/Z).
* Fade: colour offset ramp over the group's life (5.7).
* AfterImage: trail of `usNbAfters` copies (5.8).
* Gather: particles fly from the birth box to the emitter centre (5.9).

---------------------------------------------------------------------------------------------------------

## 4. Textures and sprite animation

### 4.1 Where textures live and how a particle picks one

Each particle template points (`_pTexture`, absolute) to one `NTPK` texture block; all particles of that
template use it, the whole texture bound once per element. Several templates may point at the same block
(e331 0xe08 and 0xe07 both use 0xd00). There are no per-particle texture choices; sub-images are selected
with UV cells from the animation (4.4).

### 4.2 NTPK texture block (`ds.Texture.cast`) - 100-byte header

| off | type | field |
|---|---|---|
| 0x00 | char[4] | `"NTPK"` (`TextureCode`) |
| 0x04 | u32 | version (0x1100 in all data) |
| 0x08 | u32 | header size (100) |
| 0x0C | char[32] | database name (e.g. `fire_tubu_32bit.tga`, NUL-terminated, may be truncated to 32 bytes; used for VRAM sharing) |
| 0x2C | u32 | `sizeNtft` texel data size (bytes) |
| 0x30 | u32 | `sizeNtfp` palette size (bytes) |
| 0x34 | u32 | `sizeNtfi` 4x4 index data size (0) |
| 0x38 | u32 | `offNtft` texel offset (from NTPK start; 0x64) |
| 0x3C | u32 | `offNtfp` palette offset (from NTPK start) |
| 0x40 | u32 | `offNtfi` 4x4 index offset (0) |
| 0x44 | u32 | send addr NTFT (ignored) |
| 0x48 | u32 | send addr NTFP |
| 0x4C | u32 | send addr NTFI |
| 0x50 | u32 | flags (`FLAG_NO_NTFI = 8` in all data) |
| 0x54 | u32, u32 | VRAM keys (runtime) |
| 0x5C | u8 | format (`GXTexFmt`) |
| 0x5D | u8 | texgen (1 in data) |
| 0x5E | u8 | sizeS: width = `8 << sizeS` |
| 0x5F | u8 | sizeT: height = `8 << sizeT` |
| 0x60 | u8 | repeat (0 = clamp) |
| 0x61 | u8 | flip (0) |
| 0x62 | u8 | color0 transparent (1 in data) |
| 0x63 | u8 | reserve |

Texel data is row-major, top row first, no swizzle (except 4x4). Palette = u16 BGR555 colours
(`r = c & 31, g = (c>>5) & 31, b = (c>>10) & 31`, bit 15 ignored).

### 4.3 Formats and decoding (`GlobalScope.LoadTexture`, the port's decoder)

| fmt | name | texel | RGB | alpha (port, 0..255) |
|---|---|---|---|---|
| 1 | A3I5 | 1 byte: idx = b & 31, a = b >> 5 | pal[idx] << 3 | `a * 255 / 7` |
| 2 | 4-colour | 2 bits, LSB first (4 per byte) | pal[idx] << 3 | 0 if idx==0 and color0, else 255 |
| 3 | 16-colour | 4 bits, low nibble first | pal[idx] << 3 | 0 if idx==0 and color0, else 255 |
| 4 | 256-colour | 1 byte | pal[idx]*255/31 | 0 if idx==0 and color0, else 255 |
| 5 | 4x4 compressed | - | - | **not supported on the effect path** (the 4x4 index data is passed as null; would crash). Never used. |
| 6 | A5I3 | 1 byte: idx = b & 7, a = b >> 3 | pal[idx] << 3 | `a * 255 / 31` |
| 7 | direct | u16 BGR555 | c << 3 | bit15 ? 255 : 0 |

(DS hardware expands A3 as `a*4 + a/2` to 5 bits; the port's `a*255/7` is equivalent within rounding.)
The GL texture is created with `GL_NEAREST` filtering and `GL_CLAMP_TO_EDGE` wrap for effects.

**Formats present in the Steam data** (798 particle textures): A3I5 774 (8x8 94, 16x16 18, 32x16 4, 32x32 277,
64x16 4, 64x32 24, 64x64 244, 128x64 24, 128x128 81, 256x32 1, 256x128 3) and 256-colour 24 (8x8 21, 32x32 3;
indices only 0..31, 32-entry palette, index 0 transparent). Every palette is 64 bytes (32 colours). No
A5I3, 4/16-colour, 4x4 or direct textures are used by effects.

### 4.4 Sprite animation data (`eld.spr.cs`)

`Eff_AnimationHeader` at `pAnim` (32 bytes):

| off | type | field |
|---|---|---|
| 0 | u32 | `NumUVAnim` - entries in the UV table |
| 4 | u32 | `NumSclAnim` - entries in the scale table |
| 8 | u32 | `NumClrAnim` - entries in the colour table |
| 12 | u32 | reserve |
| 16 | s32 | offset of `Eff_UVAnimation` (from this header's start) |
| 20 | s32 | offset of `Eff_ScaleAnimation` |
| 24 | s32 | offset of `Eff_ColorAnimation` |
| 28 | u32 | reserve |

`Eff_UVAnimation` (32 bytes + 4 per entry; blocks are padded to 16):

| off | type | field |
|---|---|---|
| 0 | u16 | `unSizeU` cell width (texels) |
| 2 | u16 | `unSizeV` cell height |
| 4 | u16 | `unStartU` first cell x (texels) |
| 6 | u16 | `unStartV` first cell y |
| 8 | u16 | `unTextureWidth` (used for the cell grid) |
| 10 | u16 | `unTextureHeight` (unused) |
| 12 | u16 | `ucNumInterpolate` (0 in all data) |
| 14 | u16 | dummy |
| 16 | u32 | `unUvFlag` (bit31 LOOP, bit30 INTERPOLATE) |
| 20 | u32[3] | reserve |
| 32 | {u16 time, u16 pattern}[NumUVAnim] | `unaUvTimeTbl`, `unaUvNoTbl` |

`Eff_ScaleAnimation` (16 + 16 per entry): u32 `unSclFlag`, u32[3] reserve, then entries
`{u32 time, fx32 scaleX, fx32 scaleY, u32 reserve}`.

`Eff_ColorAnimation` (16 + 32 per entry): u32 `unClrFlag`, u32[3] reserve, then entries
`{u32 time, u32 reserve[3], s32 c0, s32 c1, s32 c2, s32 alpha}` with **c0 = red, c1 = BLUE, c2 = green**
(the port reads `nR, nB, nG, nA` in that order and uses them as red, blue, green). The data confirm it:
Thunder tints (0,31,12) -> blue with a blue texture, Blizzard smoke (12,31,24) -> light blue, Sleep stars
(31,12,31) -> yellow. Values 0..31.

Flags (all three tables): `0x80000000` LOOP, `0x40000000` INTERPOLATE. Observed: UV 0 or LOOP (31);
scale INTERP (756) or 0; colour INTERP (785) or 0. No table is empty in the data.

### 4.5 Animation evaluation (identical state machine for the three tables)

Each **group** owns one `EffSprAnim` (UV + scale + colour players); all particles of the group share its
output. It is (re)initialised when the group is created, and advanced once at the start of every group
update (before the output is read), i.e. on every step the group is alive, including its creation step.

State per table: `pos` (entry index), `wait` (int; `END = -1`).

```
init:     pos = 0; wait = (N != 0) ? entry[0].time : END
          (colour/scale with INTERP: inter = FX_Div(4096, entry[0].time))
          UV only: grid = (0,0)                // NOTE: entry[0].pattern is NOT applied at init
update:   if wait == END: return
          if wait <= 0:
              if pos + 1 >= N:
                  if !(flag & LOOP): wait = END; return      // hold the last entry forever
                  pos = 0
              else: pos++
              wait = entry[pos].time; if wait <= 0: wait = 1
              UV: setGrid(entry[pos].pattern)
              colour/scale with INTERP: inter = FX_Div(4096, entry[pos].time)
          wait--
```

So entry *k* is shown for `time_k` updates (the first update already counts), and the UV pattern of
entry 0 only takes effect when the table loops back to it.

Outputs:

* **UV** (`GetData`): with `cols = unTextureWidth / unSizeU` (u16 division), `setGrid(p)`:
  `gx = p % cols, gy = p / cols` (p == 0 or cols == 0 -> (0,0)). Then, in fx32 texels (`<< 12`):
  `u0 = (startU + sizeU*gx) << 12`, `v0 = (startV + sizeV*gy) << 12`, `u1 = u0 + (sizeU << 12)`,
  `v1 = v0 + (sizeV << 12)`. With the UV INTERPOLATE flag the port adds an unshifted `sizeU*(1-wait/T)` to
  u0 (i.e. a fraction of a texel - effectively nothing); unused by data.
* **Scale** (fx32 x, y): if INTERP and `pos+1 < N`: `t = (entry.time != 0) ? 4096 - FX_Mul(wait, inter) : 0`;
  `s = ((next - cur) * t >> 12) + cur` per axis (integer, arithmetic shift). Otherwise `s = entry[pos]`.
* **Colour** (ints R,G,B,A): same interpolation as scale between `entry[pos]` and `entry[pos+1]`; the last
  entry is never interpolated.

`t` after the k-th update of an entry of length T is `4096 - round(( T-k) * 4096/T)`, i.e. `k/T`: the value
reaches the next key exactly on the entry's last update.

---------------------------------------------------------------------------------------------------------

## 5. Particle simulation (per 30 Hz step)

### 5.0 Object life and the frame loop

Per battle step (`BattlePart`): `BattleEffect.execute()` -> `ServerFF3.doExecute()` ->
`Manager.doExecute()` calls `Calculate()` on every object whose status has any of RUN(1) | STOP_WAIT(4) |
STOP(8) (mask 13), in creation order; objects created during this loop (sequence BOOTs) are appended and
calculated in the same pass. Then the battle deletes finished slots. Then `doUpdate()` processes one queued
command per object (1 start-wait, 2 pause, 4 wait-until-!isPlay then `Terminate()`, 8 set STOP, 16 delete).
Then the scene draws (effects after all 3D models, section 6).

`Stop()` queues 8 then 4; `StopToDead()` queues 8, 4, 16; `DeleteObject()` flushes and queues 16.
A particle object's `Terminate()` frees its element (it disappears). STOP status (8) makes a LOOP emitter
finish its current cycle instead of restarting (`isLoop() & CheckStop()==0`).

### 5.1 Allocation (`allocateWork`, at creation)

```
perChild   = 1 + nbAfters                        // main + after-images
nbParticles = AFTERIMAGE ? nbChilds*perChild*nbGroups : nbChilds*nbGroups
```
Particles are laid out group-major: group g owns slots `[g*nbChilds*perChild, ...)`, child c owns
`[main, after_1 .. after_N]`. All primitives start hidden (Disp 0, colour 0). One draw element per object.
(If AFTERIMAGE is off but nbAfters > 0 the port would index out of range; never happens in data.)

### 5.2 Start / Initialize

`Initialize` (called by `Start(0)`; for sequence-booted objects it is called twice, harmless except 5.3):
range/size controllers from the setup, sprite data pointer, `state = PLAY`, `usTimeNextGroup = usTimeInterval`,
`usTimeLife = 0`, `numCurrentGroup = 0`, `bPlay = true`; ParticleDS additionally speed, emit, fade,
after-image, circle controllers; LargeDS the same minus circle; Gather fade, after-image, gather.
The emitter position `posBase` is whatever `SetPosition` last set (fx32 world).

### 5.3 Emitter step (`ImpBaseParticle.statePlay` / `stateWaitEnd`)

```
PLAY:
    usTimeLife++; usTimeNextGroup++
    if usTimeNextGroup >= usTimeInterval and numCurrentGroup < nbGroups:
        group[numCurrentGroup].create(); numCurrentGroup++; usTimeNextGroup = 0
    for each group g (0..nbGroups-1): if g.alive: g.update()
    if usTimeLife >= usTimePlay:
        if LOOP and not stopped: numCurrentGroup = 0; usTimeLife = 0; usTimeNextGroup = 0   // stay PLAY
        else: state = WAITEND
WAITEND:
    for each alive group: update()
    last = (numCurrentGroup != 0) ? numCurrentGroup-1 : 0
    if !group[last].alive: StopToDead(); bPlay = false
```

Consequences: the first group is born on the first step (because NextGroup starts at `interval`). With
interval I, groups are born on steps `1, 1+max(I,1), 1+2*max(I,1), ...` (interval 0 and 1 both give one
group per step) until `nbGroups` are born or `usTimePlay` is reached (unborn groups are skipped). On a loop
restart NextGroup restarts at 0, so the next group comes after `max(I,1)` steps, reusing group slot 0
(recreated even if still alive). The emitter ends when the *last born* group dies.

Every step, if MOVE_OFFSET is set, the draw element's centre is set to `posBase` *before* the step.

### 5.4 Group creation (GroupDS / GroupLargeDS)

```
setSprite(anim)                          // group animation restarts (4.5)
G = GravityController(suGravity)         // per-group: Gdir = dir * (gPow + (gRand? rand(gRand):0))
for each child i:
    p = particle main slot
    p.center = (rx(), ry(), rz())        // Range: axis r!=0 ? -r + rand(2r) : 0
    M = emitMatrix()                     // 5.5
    p.vel = speedDir * (sPow + (sRand? rand(sRand):0))   // FX_Mul per component
    p.vel = p.vel * M                    // row vector times 3x3 (MTX_MultVec43, rounded)
    p.grav = Gdir * (gPow + (gRand? rand(gRand):0))
    if !MOVE_OFFSET: p.center += posBase // world space from now on
    p.baseCenter = p.center              // ParticleDS: position without circle offset
    ParticleDS: p.circleRadius = nRadius; p.circleAngle = rand(65535)
    p.enSize = sizeBase + (sizeRand? rand(sizeRand):0); p.size = (enSize, enSize)
    p.Disp = 3; p.polygonID = next id (22..63, then 21, cycling)
    after-image slots: Disp = 0, new polygon IDs
groupDisp = 3; groupLife = 0; alive = true
```

(Gather: 5.9.)

### 5.5 Emit rotation - IMPORTANT unit and port-bug notes

`EmmitController` builds `M = RotX(ax) * RotY(ay) * RotZ(az)` (applied to a row vector: X first, then Y, then Z;
`RotX` = `[[1,0,0],[0,c,s],[0,-s,c]]`, `RotY` = `[[c,0,-s],[0,1,0],[s,0,c]]`, `RotZ` = `[[c,s,0],[-s,c,0],[0,0,1]]`
with `s,c = FX_SinIdx/CosIdx((ushort)angle)` fx16, row-major `v' = v * M`).

Per axis with `e = vEmmitAngle[axis]`:

* **DS (intended, value semantics):** `angle = (e != 0) ? -e + rand(2e) : 0`, i.e. uniform in `[-e, e)`
  **index units** (`65536 = 360 deg`).
* **Port as written:** `_evBase` and `_evRand` alias the same `Vector3` object (`_evRand = _evBase;` is a
  reference copy), so after `*2` and `neg()` both are `-2e`; `EffRand(-2e)` casts the negative bound to
  `uint` and returns `Random.Next()` unreduced. Result: **any non-zero axis gets a uniformly random full-circle
  angle** in the port. (It also mutates the parsed setup; a second `Initialize` compounds it, which changes
  nothing further.) For a re-implementation that should look like the DS/Steam game, use the DS rule; to look
  like OpenFF today, use full random for non-zero axes. Make it a switch.

**Unit mismatch (applies to DS too):** the authored values are fx32 *radians*: 25736 = 2*pi*4096,
12868 = pi, 6434 = pi/2, 4289 = 60 deg, 2145 = 30 deg, 1430 = 20 deg, 715 = 10 deg, 357 = 5 deg, 71 = 1 deg
(all values found in the data). The runtime feeds them straight into index-angle sin/cos (the `WIN_RAD_TO_DEG`
and `WIN_FX32_TO_F32` macros are identity on DS), so the effective spread is `e * 360/65536` degrees
(2*pi -> +-141.4 deg, 30 deg -> +-11.8 deg). To match the game, reproduce the runtime interpretation, not the
authored meaning. The same holds for `CircleSetup.nAngleAdd` (715 -> 3.93 deg/step) and `GatherSetup.vRotate`.

### 5.6 Group update (GroupDS, per step while alive)

```
anim.update(); form = anim output (uv, scale, colour)                 // 4.5
if groupLife++ > usTimeGroupLife + nbAfters: alive = false; return    // post-increment compare
if groupLife == usTimeGroupLife: groupDisp = 0                        // hide main particles from now
fade = FADE ? fadeColor(groupLife) : (0,0,0,0)                        // 5.7, groupLife is 1-based here
c    = clamp(form.colour + fade, 0, 31) per channel (float)
d    = (c - clamp(c + afterLastColorSub, 0, 31)) / (nbAfters + 1)      // per channel, float
for each child:
    after-image shift (5.8)
    ParticleDS:  radius += nRadiusAdd; angle += nAngleAdd
                 vel += grav; baseCenter += vel
                 drawCenter = baseCenter + (FX_Mul(radius, sinIdx(angle)), 0, FX_Mul(radius, cosIdx(angle)))
                 colour = (short)c; Disp = (alpha != 0) ? 3 : 0
                 size = ((scale.x * enSize) >> 12, (scale.y * enSize) >> 12)   // short for ParticleDS
                 uv = form.uv
    LargeDS:     colour = (short)c; size/uv as above (int size); Disp = alpha ? 3 : 0
                 vel += grav; center += vel                                    // no circle
    main.Disp = groupDisp
```

So a particle is first drawn (on its birth step) at `birth + v0 + g`. Main particles are visible for group
life values 1..usTimeGroupLife-1 (usTimeGroupLife-1 steps); the group lingers nbAfters(+1) more steps so the
trail can drain, then stops updating (its primitives keep their last, hidden, state until the object ends).

### 5.7 Fade (`FadeController`)

```
colSub = vColorSub / usTime (float; = vColorSub if usTime == 0); end = usStart + usTime
fade(t) = t < usStart ? 0 : t >= end ? vColorSub : colSub * (t - usStart)
```
Added to the animation colour before clamping (all four channels, so `(0,0,0,-31)` fades alpha out).
Only when the FADE flag is set - 124 templates carry fade values without the flag (ignored).

### 5.8 After-images (AFTERIMAGE flag, `usNbAfters = N`)

Before moving the main particle, for k = N down to 1: `after_k.{center, size, uv, Disp} = after_{k-1}.{...}`
(`after_0` = the main particle's state from the previous step). Colours are assigned in the same loop in the
opposite direction: main = c, after_1 = c - d, ..., after_{N-1} = c - (N-1)d. **`after_N` is never given a
colour** (stays alpha 0 = invisible) - port behaviour (possibly also the original's); so N-1 trail images are
visible. With `vColor = (-31,-31,-31,-31)` the trail darkens and fades linearly. Used by 20 templates only.

### 5.9 Gather (ParticleGatherDS, GroupGatherDS)

Creation per child:
```
do p.center = range position while center.x + center.y + center.z == 0     // posBase is never added
dir   = normalize(-center)                          // VEC_Normalize: FX_Div by length
limit = |center|                                    // sqrt of 64-bit sum
speed = dir * nSpeedPow; add = dir * nSpeedAdd      // FX_Mul
R     = rotation(rand(vRotate.x), rand(vRotate.y), rand(vRotate.z))   // each only if non-zero; setRotate order X,Y,Z
length = 0; gathering = true; size = enSize; Disp = 2 (cull back), groupDisp = 2
```
Update per step (after the base size/uv update):
```
if gathering:
    center += speed; length += |speed|
    if length >= limit: center = 0; gathering = false
    else: speed += add; speed = speed*R; add = add*R; center = center*R     // swirl about the origin
colour = c; Disp = alpha ? 2 : 0; main.Disp = groupDisp
```
Gather positions are always emitter-relative; they only follow the emitter with MOVE_OFFSET (13 of 14
gather templates have it). No speed/gravity/emit/circle.

### 5.10 Randomness

`ExecRand(n)` -> `EffRand(n)` = `(int)(ds.RandomNumber.rand32((uint)n))` = `(int)((uint)Random.Next() % (uint)n)`,
i.e. uniform integer in `[0, n)` for n > 0. The generator is the single shared `System.Random` behind the C
`rand()` shim, seeded by `srand(time)` (`MATH_InitRand32` ignores its seed argument); effects share it with
camera and field rolls (battle rules use a separate generator). Nothing is reproducible, so a JS version can
use `Math.floor(Math.random()*n)`. Order of draws per particle: range x,y,z, emit x,y,z, speed, gravity,
circle angle, size (group: one gravity roll first).

---------------------------------------------------------------------------------------------------------

## 6. Drawing

### 6.1 Primitive and quad (`ds.pt.Particle.packCommand`, `PrimitiveDisplay.drawParticles`)

Per element (= per particle object), per particle in slot order, skipped when `Color.alpha == 0`:

```
camera = world->view matrix (NNS_G3dGlbGetCameraMtx), T = its translation, Rcam = its rotation
p_world = elementCentre + particle.center                    // elementCentre = posBase if MOVE_OFFSET, else 0
p_view  = p_world * Rcam + T
quad vertices (view space, z = p_view.z):
   (p_view.x - w, p_view.y + h)  tex (u0, v0)     // top-left  -> top of the texture cell
   (p_view.x - w, p_view.y - h)  tex (u0, v1)
   (p_view.x + w, p_view.y - h)  tex (u1, v1)
   (p_view.x + w, p_view.y + h)  tex (u1, v0)
w = Size.x / 4096, h = Size.y / 4096 (world units; Size is the HALF-size)
```

So particles are **screen-aligned billboards** (camera-facing, never rotated, no roll), sized in world
units at the particle's depth (perspective applies). Large particles are identical except the quad is a unit
square scaled by `G3_Scale(Size.x, Size.y, 0)` with int sizes (ParticleDS sizes are `short`: max half-size
~8 units). Object `SetScale`/`SetRotation` are no-ops for particle objects; only `SetPosition` matters.

Texture coordinates: `St` are fx32 texels; `u = (St/4096) / texWidth`, `v = (St/4096) / texHeight`, v = 0
at the top row of the image. Nearest filtering, clamp.

### 6.2 Position spaces

* Without MOVE_OFFSET: `posBase` is added at birth; the particle then lives in world space (moving the
  emitter afterwards affects only new groups; a path-driven emitter leaves a trail).
* With MOVE_OFFSET: particle centres stay relative; the element centre (= current `posBase`, updated each
  step before simulation) is added at draw time, so the whole cloud follows the emitter.
* Gather: always relative (see 5.9).

### 6.3 Colour, blending, depth

* Polygon mode MODULATE: fragment = texture RGBA x vertex colour, where vertex RGB = `Color.rgb * 255/31`
  and vertex alpha = `polygonAlpha * 255/31` with polygonAlpha = `Color.alpha` (0..31).
* `Disp` is passed as the DS cull mode: 0 = CULL_ALL (the port sets alpha 0: invisible), 2 = cull back,
  3 = cull none. In the port both 2 and 3 end up drawing the (front-facing) quad.
* Blending (port): `glBlendFunc(SRC_ALPHA, ONE_MINUS_SRC_ALPHA)` - ordinary alpha blending, **not additive**;
  alpha test `> 0.01`; depth test `LEQUAL` against the scene; **depth writes off** inside every
  `G3_Begin/G3_End` (so particles never occlude each other; submission order decides).
* Order: effect elements are drawn after all 3D layers (`Scene.draw` -> `_graph.drawObjects`), elements in
  object creation order, particles in slot order (group, child, main then after-images).
* DS-only details not emulated by the port: polygon IDs (22..63 cycling; on DS translucent polygons with the
  same ID do not blend over each other), the XLU_DEPTH flag, hardware translucent sort. GUESS: the DS sorted
  translucent polygons by submission order as well (manual sort) - not verifiable here.

### 6.4 World scale

Positions are battle world fx32 (`4096` = 1 unit), set by the battle code via `BattleEffect.setPosition`
(a `VecFx32`). References: a player's hit effect is placed at the character position + 9 units toward the
camera + 5 units (20480) up (`TurnSystem.setHitEffectPosition`); a player steps 8 units forward to attack
(`PLAYER_MOVE_DISTANCE = 32768`). Typical particle half-sizes: 0.5-1.1 (fire droplets), 2-5 (puffs), 5-8
(explosions / flashes).

The port renders at 60 fps by blending captured frames between two 30 Hz steps (`FrameCapture`, keyed by
`Particle.Generation`); the simulation itself is strictly 30 Hz.

---------------------------------------------------------------------------------------------------------

## 7. ModelDS and SequenceDS

### 7.1 ModelDSSetup (260 bytes)

| off | type | field |
|---|---|---|
| 0 | u32 | version (0 in data) |
| 4 | u32 | flag (bit0 LOOP; 2/4/8/16 = anime material / tex SRT / tex pattern / visibility - not consulted) |
| 8 | fx32[3] | `vScale` (e.g. 451 = 0.11, 14336 = 3.5) |
| 20 | s32[3] x4 | `vDiffuse`, `vAmbient`, `vSpecular`, `vEmission` (unused by the port) |
| 68 | char[48] x4 | source names `.ima`, `.ita`, `.itp`, `.iva` (material / texSRT / texPattern / visibility anims; informational) |

`pTexture` -> `ModelDSNmdpHeader`: u32 offModel, u32 offTexture (relative to this header; model =
`[offModel, offTexture)`, texture = `[offTexture, end)`), u32 initialized, u32 reserve. Both parts are
`"NMDP"` containers (Square Enix wrappers around Nitro model / texture resources, `ds.sys3d.nmdp`).
`pAnim` -> `ModelDSNcapHeader`: u32 offMotion (`"NCAP"` motion), u32 offAnimation (`"NAMP"`
material/texture animation, optional), u32 initialized, u32 reserve.

Runtime: builds a skinned model, binds the texture container, scale = `vScale`, starts the NCAP motion
(looping if flag bit0) and the NAMP animation (always looping), registers it as a scene render object
(drawn with the 3D models, LAYER_BOTTOM - not with the particle elements). `Calculate`: `motion.next()`,
`anim.next()`; when the motion ends and not looping -> StopToDead. `SetPosition` / `SetRotationXYZ` (index
angles) apply. Blizzard's ice block (0x107e, `ice_02`) is a ModelDS.

### 7.2 SequenceDS parameter block

| off | type | field |
|---|---|---|
| 0 | u32 | `PathOffset` - offset of the PATH block from the parameter start |
| 4 | u32 | `SeqTime` (total length; not used by the runtime) |
| 8 | u32 | `uiFlag` (bit0 LOOP, bit1 LOOK_CAMERA - the latter unused) |
| 12 | u32 | res |
| 16 | u32 | (unnamed, skipped) |
| 20 | u32[3] | dummy |
| 32 | ... | command stream up to `PathOffset` |
| PathOffset | PATH block | 7.4 |

### 7.3 Commands (first u32 = command id; the dispatcher peeks it, each handler reads its record)

| id | name | size | layout (after the u32 id) | runtime |
|---|---|---|---|---|
| 0 | WAIT | 16 | u32 time, u32 res[2] | `wait_time = time` |
| 1 | POSITION | 16 | f32 x, y, z | sequence base position = xyz*4096; pushes base+offset to all paths. **Port bug:** the reader does not skip the id, so the fields are misread and the stream desyncs by 4 bytes. Never used in the data. |
| 2 | BOOT | 48 | u32 category, u32 member, f32 pos[3], f32 figure[3], u32 id, u32 uiEmitterID, u16 path_id, u8 flag_coord, u8 layer | create (category, member); see below |
| 3 | HALT | 16 | u32 index, u32 id, u32 res | `Stop()` the object booted with that id |
| 4 | MOVE | 16 | u32 pathNo, u32 target_id, u32 res | parsed, does nothing |
| 5 | END | (16) | (not consumed) | end / loop (below) |

Observed in the data: only BOOT (1005), WAIT (517), END (231). `uiEmitterID` equals the booted template ID
(informational); `figure`, `flag_coord`, `layer` are unused (the figure floats are often garbage).

Sequence execution:
```
Initialize: wait_time = 1
each step (Calculate):
    for each path p: if its object stopped playing -> p.DeleteObject()
    if status has STOP: status = STOP_WAIT(4)
    for each path p: p.update(rot, scale); if !p.isPlay: remove p and its boot id
    if wait_time == END(0xFFFFFFFF):
        if status == 4: Stop() every child still RUNning (loops finish their cycle)
        if no path is playing: StopToDead(); bPlay = false
    else:
        wait_time--
        while wait_time == 0: execute next command
END: if status == 4 or !LOOP: wait_time = END; status = 4
     else (LOOP): wait_time = 1; rewind the stream; first = 1
BOOT: if first and the object booted with this id is still playing and is a LOOP object: skip
      obj = createObject(category, member)   (Start(0) inside), remember id
      obj.Start(0) again; obj.SetPosition(offset + pos*4096)
      path = paths[path_id]; path.initialize(obj, base = sequence offset)
      path.updatePositionS(rot); path.updatePositionM(rot, t = 0)   // places the object on the path start
```
So `WAIT n` delays the following commands by n steps, commands between WAITs run in the same step, and
booted children are calculated in the same step they are booted. `SetPosition` on the sequence sets the
offset and re-bases every live path. The sequence's rotation (`SetRotationXYZ`/`SetRotateMatrix`) rotates
path points; its scale is stored but unused; the battle code only calls `SetPosition`.

### 7.4 PATH block (`SEQUENCE_PATH_HEADER`, `SPathBodyHeader`)

Header (at param + PathOffset): char[4] `"PATH"`, u32 version (0x1000), u32 `uiNumPathData`, u32 res,
u32 `pIndex[n]` (offsets from the PATH header start).

Body:

| field | type | notes |
|---|---|---|
| `uiNumArryCount` | u32 | number of Vector4 entries: 1 (single point) or 4*segments |
| `uiFrameTime` | u32 | total time of the path (steps) |
| `uiFlag` | u32 | bits below |
| res | u32 | |
| path | s32[4] x count | fx32 x,y,z,w; for curves grouped per segment as `[P0, P1, T0, T1]` |
| figure | s32[4] x ((count>>2)+1) | per point "figure" (rotation/scale keys); `FigureUpdate` is empty - unused |
| timing | u32 x ((count>>2)+1) | start step of each point. (The port reads `count` entries - over-reads into the next data; only the first `(count>>2)+1` are meaningful.) |

`points = (count >> 2) + 1`, `segments = points - 1`.

Flags: bit0 CURVE (else linear); move type `flag & 0xE`: 2 ITURN (stop at the end), 4 UTURN (ping-pong),
8 LOOP (restart); coordinate `flag & 0x30`: 0x10 LOCAL (add the sequence base), 0x20 WORLD; 0x40 SET_FIGURE,
0x80 ADD_FIGURE (no effect). Observed: 0x12 and 0xd2 single points; 0xd3 / 0x13 curves; 0xd2 / 0x12 lines.

Evaluation:
* **Single point** (count == 1): every step `pos = path[0].xyz * R + base` (base always added).
* **Segments:** segment i uses `G = entries[4i .. 4i+3]`. Linear: `P = P0 + (P1-P0)*t`. Curve (Ferguson /
  Hermite): `P = [t^3, t^2, t, 1] * H * G`, `H = [[2,-2,1,1],[-3,3,-2,-1],[0,0,1,0],[1,0,0,0]]` (fx,
  `t^2 = FX_Mul(t,t)`, `t^3 = FX_Mul(t, FX_Mul(t,t))`; t = 0 and t = 4096 return P0 / P1 exactly).
  Then `P = P * R`; if LOCAL: `P += base`; `obj.SetPosition(P)`.
* Timing: segment duration `T_i = timing[i+1] - timing[i]` for `i+1 < segments`, else `uiFrameTime - timing[i]`.
  Each step: `elapsed++` (not in idle mode), `t = FX_Div(elapsed << 12, T_i*4096)` (= elapsed/T_i),
  evaluate, and if `T_i <= elapsed` advance: next segment (elapsed = 0); past the last segment ->
  ITURN: idle at `t = 1` of the last segment forever; UTURN: run backwards (`t' = 1 - t` on segment i-1,
  reverse durations `timing[i] - timing[i-1]`), then forwards again; LOOP: segment 0; none: freeze.

---------------------------------------------------------------------------------------------------------

## 8. Worked example: e331.efp (Fire)

Header: `EFCT`, 6 templates, TexAddr 0xd00, version 0x014B1000, pIndex 0x30, 0x230, 0x320, 0x400, 0x510, 0x600.
efi category 331 = `[0, 0xe0d, 0xe08, 0xe06, 0xe09, 0xe0c, 0xe07]` (members 1..6 = templates below in order).
player.chaindata: magic 4101 -> category 331, member 1.

### 8.1 Member 1 - 0xe0d SequenceDS (@0x30)

PathOffset 0x140, SeqTime 32, flag 0. Commands:

```
step 1: BOOT 331/2 id1 path0   (0xe08 fire droplets)
        BOOT 331/3 id2 path0   (0xe06 flame puffs)
        BOOT 331/4 id3 path1   (0xe09 explosion, 5 units up)
        BOOT 331/5 id4 path0   (0xe0c sparkle flicker)
        WAIT 2
step 3: BOOT 331/6 id5 path1   (0xe07 swirling droplets, 5 units up)
        WAIT 30
step 33: END (no loop) -> sequence ends when all children are done
```
Paths: path0 flag 0x12, one point (0,0,0), frameTime 60; path1 flag 0xd2, one point (0, 20480, 0) = 5 units
up, figure (0,0,0,4096).

### 8.2 Particle templates (all ParticleDS, flag A3I5 only, no fade/after/loop/offset)

| member | id | range (+-) | size half (base+rand) | play/groupLife/interval | childs x groups | speed | gravity | emit (index) | circle |
|---|---|---|---|---|---|---|---|---|---|
| 2 | 0xe08 | (2,3,2) | 0.5 + [0,0.6) | 30/30/0 | 7 x 1 | (0,1,0)*1.5 | (0,-1,0), pow 0.3 -> g = -0.09/step^2 | (2145, 25736, 2145) | - |
| 3 | 0xe06 | (3,2,5) | 2 + [0,3) | 30/30/2 | 4 x 1 | 0 | 0 | 0 | - |
| 4 | 0xe09 | (3,6,3) | 5 + 0 | 30/30/1 | 1 x 7 | (0,1,0)*0.18 | 0 | 0 | - |
| 5 | 0xe0c | (3,4,6) | 2 + [0,2.5) | 30/30/0 | 8 x 1 | (0,1,0)*(0.3+[0,0.45)) | 0 | 0 | - |
| 6 | 0xe07 | (3,8,6) | 0.5 + [0,0.8) | 30/30/0 | 7 x 1 | (0,1,0)*(0.25+[0,0.5)) | (0,-1,0) pow 0.1 -> -0.01 | (357, 0, 357) | r 1, +0.1/step, +715/step |

Animation / texture per member (colour tuples as stored: R, B, G, A; interpolated unless noted):

* **0xe08** anim @0x6f0: UV 1 entry (20, 0) cell 8x8 of 8x8; scale (8: 1.0) -> (12: 1.0); colour
  (10: R31 B11 G31 A31) -> (9: R31 B12 G31 A31) -> (1: 31,31,31, A0): yellow droplets fading to white and
  transparent during steps 11-19, invisible after. Texture @0xd00 `fire_tubu_32bit.tga` A3I5 8x8 (orange blob,
  palette (31,8,0)..(31,31,29)).
  Behaviour: 7 droplets in a box +-2/3/2 around the target, launched upward at 1.5 units/step in a cone
  (DS: X,Z +-11.8 deg, Y +-141 deg; port: random full rotations - a sphere burst), decelerating by 0.09.
* **0xe06** anim @0x7e0: UV 20 entries of 1 step cycling patterns 0..7 in a 4x2 grid of 32x32 cells
  (texture 128x64); scale 0.6 -> 1.0 over 4 steps then 1.0; colour (3: 31,24,27,A0) -> (4: white A31) ->
  (12: white A31) -> (1: 31,24,27,A0): fade in over 3, hold, fade out. Texture @0xe00 `i_moya_07.tga` A3I5
  128x64 (8 flame-puff frames). One group of 4 static puffs, half-size 2..5.
* **0xe09** anim @0x930: UV 25 entries of 1 step, patterns 0,0,2,4,...,30,31..38 in 16x24 cells of a
  128x128 sheet (8 columns); scale (0.75,1.0) held 15+9 steps then to (0.2,1.0); colour
  A0 -> A31 (5) -> white A31 -> (R31 B6 G24) orange (4..10) -> (R31 B6 G16 A0). Texture @0x2f00
  `power_reactor_explosion_00_00` A3I5 128x128. 7 groups (one per step), one flame column each, rising 0.18/step.
* **0xe0c** anim @0xad0: UV 1 entry (25, 0) 64x64; scale pulses 0.1 <-> 1.0 every 5 steps; colour white A31
  then to A0 in the last step. Texture @0x7000 `cri_kira_fire.tga` A3I5 64x64. 8 twinkling stars rising.
* **0xe07** anim @0xc00 (same tables as 0xe08), texture @0xd00 (shared with 0xe08). Droplets orbiting in XZ
  (radius 1 growing 0.1/step, 715 index = 3.9 deg/step), rising slowly.

Textures of e331: `fire_tubu_32bit` 8x8, `i_moya_07` 128x64, `power_reactor_explosion_00_00` 128x128,
`cri_kira_fire` 64x64 - all A3I5 with 32-colour palettes, color0 transparent flag set, clamp.

### 8.3 e332.efp (Blizzard, magic 4102 -> category 332 member 1)

| member | id | kind | notes |
|---|---|---|---|
| 1 | 0x245 | SequenceDS | BOOT 2,3,4,5 (path0), WAIT 3, BOOT 6 (ModelDS), WAIT 32, END; path0 = single point (0,0,0) flag 0xd2 |
| 2 | 0xe8c | ParticleLargeDS | 1 flash, half-size 8, scale 0 -> 0.25 -> 1.8, 16 steps; `hit_flash_02_32bit` A3I5 64x64 |
| 3 | 0x238 | ParticleLargeDS | 8 smoke puffs, half-size 5, speed (0,1,0)*(0.1+[0,0.15)) emit X 25736; colour to (R12 B31 G24) light blue; `smoke_02` A3I5 32x32 |
| 4 | 0x233 | ParticleDS | 7 groups x 1 sparkle, interval 2; `kira_01_32bit` A3I5 64x64 |
| 5 | 0x2b9 | ParticleDS | FADE (start 5, 15 steps, alpha -31); 7 ice chips, gravity -0.01, UV loop 4 cells 8x8 of 32x16; `stone_02` A3I5 32x16 |
| 6 | 0x107e | ModelDS | ice block model `ice_02.ima`, scale 451 (0.11) |

### 8.4 e301.efp (Cure, magic 4001 -> category 301 member 1)

| member | id | kind | notes |
|---|---|---|---|
| 1 | 0x230 | SequenceDS | BOOT 2 (path0), 3 (path0), 4 (path1), WAIT 2, BOOT 5 (path1), WAIT 28, END |
| 2 | 0x9d5 | ParticleDS | MOVE_OFFSET; 1 big sparkle (half-size 5) riding path0 |
| 3 | 0x226 | ParticleDS | FADE; 22 groups (one per step) of 1 sparkle, falling (0,-1,0)*(0.05+[0,0.1)) - leaves a trail along path0 |
| 4 | 0x237 | ParticleDS | 10 groups (interval 2) of rising sparkles |
| 5 | 0x232 | ParticleDS | 3 groups x 2 orbs (`tama_001_32bit` 32x32), range +-7.25, speed (0,0.35,0)*(0.75+[0,1)) |

All four sparkle templates share one texture @0xf00 `kira_01_32bit` A3I5 64x64 (scale pulses 0.1 <-> 1.0).
Path0: 11 Hermite segments (count 44, flag 0xd3 = CURVE|ITURN|LOCAL|figure bits), frameTime 20, timing
0,1,3,5,7,9,10,12,14,16,18,19: a spiral around the target rising 1 unit per quarter turn, radius 6 -> 8
(P0 (0,0,6), (-6.2,1,-0.2), (0.2,2,-6.4), (6.6,3,0.2), ... (8.2,11,0.2)); ITURN -> the sparkle rests at the
top. Path1: single point (0,0,0).

### 8.5 Kinds summary

* Fire (e331): 1 SequenceDS + 5 ParticleDS.
* Blizzard (e332): 1 SequenceDS + 2 ParticleLargeDS + 2 ParticleDS + 1 ModelDS.
* Cure (e301): 1 SequenceDS + 4 ParticleDS.
* (Casts: e407 = SequenceDS, ModelDS, 3 ParticleDS; e408 = 2 SequenceDS + 10 ParticleDS.)

---------------------------------------------------------------------------------------------------------

## 9. player.chaindata chain 12 (magic -> effect)

File: chain table at 16 + 8*i as (u32 offset, u32 size); chain 12 = 32-byte records: s16 magicId @0,
s16 effect category @10, s16 member @12 (other fields not decoded; e.g. @20 = 1, @22 = 0x104.., @28 = 5,
@30 = a small count, GUESS: timing/sound). Relevant rows:

| magic | category | member |
|---|---|---|
| 4101 (Fire) | 331 | 1 |
| 4102 (Blizzard) | 332 | 1 |
| 4103 | 333 | 1 |
| 4001 (Cure) | 301 | 1 |

---------------------------------------------------------------------------------------------------------

## 10. Checklist for a JS re-implementation

1. Parse efi/efp as above; resolve (category, member) -> template by ID across loaded packs.
2. Decode NTPK A3I5 / 256-colour textures to RGBA (4.3); nearest + clamp.
3. Run at 30 Hz. Keep a creation-ordered object list; sequences before the children they boot; booted
   children simulate in the boot step.
4. Emitters: 5.3; groups: 5.4 / 5.6 with the shared per-group animation player 4.5; after-images 5.8.
5. Use fx32 integer math (FX_Mul rounding) if you want bit-faithful motion; floats are visually fine.
6. Emit angles: index units (5.5); choose DS cone (`[-e, e)`) or port full-random.
7. Draw: view-aligned quads, half-size `Size/4096`, alpha-blend (not additive), no depth write, depth test
   against the scene, draw after the 3D scene in creation/slot order; colour = texture x (rgb/31, alpha/31).
8. Colour table order R, B, G, A.

## 11. Things not determined

* Exact DS behaviour of polygon IDs / translucency sorting and the XLU_DEPTH flag (port ignores them).
* Whether the original DS code had the after-image "last slot never coloured" behaviour (5.8), and the
  emit-angle aliasing (5.5) is certainly port-only (C# reference copy), but not provable from here.
* Channel order of FadeSetup / AfterImageSetup colour vectors (3.2).
* Purpose of `TexAddr`, `SeqTime`, the trailing texture path strings, the SEQ figure fields, and the
  unnamed chaindata fields.
* ModelDS internals (NMDP/NCAP/NAMP) were only identified, not specified.

---------------------------------------------------------------------------------------------------------

## 12. Python used (`eld_dump.py`)

Usage: `python eld_dump.py efi 331 332 301`, `python eld_dump.py efp e331.efp --png outdir`,
`python eld_dump.py kinds e332.efp e301.efp`, `python eld_dump.py chain`, `python eld_dump.py survey`.

```python
#!/usr/bin/env python3
"""Dump FF3 DS "eld" effect packs (.efp) and the index (effect.efi).

Usage:
  python eld_dump.py efi <cat> [<cat> ...]        # list efi members of categories
  python eld_dump.py efp <file.efp> [--png DIR]   # full decode of a pack
  python eld_dump.py kinds <file.efp> ...         # one line per template
  python eld_dump.py chain                        # player.chaindata chain 12 (magic -> effect)
  python eld_dump.py survey                       # versions / kinds over every pack
"""
import os
import struct
import sys
import glob
import zlib

ROOT = r"C:\Program Files (x86)\Steam\steamapps\common\Final Fantasy III\files"

GUIDS = {
    (0x91917E57, 0x9AF8, 0x4DA3, bytes([137, 152, 131, 189, 182, 39, 250, 40])): "ParticleDS",
    (0x605FD8B8, 0x992E, 0x41FF, bytes([149, 107, 245, 133, 176, 24, 57, 176])): "ParticleLargeDS",
    (0xCC29F917, 0x5900, 0x4E67, bytes([150, 35, 209, 12, 186, 208, 204, 181])): "ParticleGatherDS",
    (0xA36D0390, 0xF0AB, 0x417A, bytes([161, 47, 188, 185, 237, 245, 31, 62])): "ModelDS",
    (0x63511AB8, 0x3F85, 0x4156, bytes([188, 196, 138, 145, 142, 90, 157, 243])): "SequenceDS",
}
# sanity: the decimal constants from the C# factories
assert 0x91917E57 == 2442231383 and 0x605FD8B8 == 1616894136 and 0xCC29F917 == 3425302807
assert 0xA36D0390 == 2741830544 and 0x63511AB8 == 1666259640

FMT = {0: "none", 1: "A3I5", 2: "4-colour (2bpp)", 3: "16-colour (4bpp)", 4: "256-colour (8bpp)",
       5: "4x4 compressed", 6: "A5I3", 7: "direct (A1BGR5)"}


class R:
    def __init__(self, d, p=0):
        self.d, self.p = d, p

    def u8(self):
        v = self.d[self.p]; self.p += 1; return v

    def u16(self):
        v = struct.unpack_from("<H", self.d, self.p)[0]; self.p += 2; return v

    def s16(self):
        v = struct.unpack_from("<h", self.d, self.p)[0]; self.p += 2; return v

    def u32(self):
        v = struct.unpack_from("<I", self.d, self.p)[0]; self.p += 4; return v

    def s32(self):
        v = struct.unpack_from("<i", self.d, self.p)[0]; self.p += 4; return v

    def f32(self):
        v = struct.unpack_from("<f", self.d, self.p)[0]; self.p += 4; return v

    def v3(self):
        return (self.s32(), self.s32(), self.s32())


def fx(v):
    return v / 4096.0


def fxv(t):
    return "(" + ", ".join("%g" % fx(x) for x in t) + ")"


# ---------------------------------------------------------------- efi
def load_efi():
    d = open(os.path.join(ROOT, "effect.efi"), "rb").read()
    n = struct.unpack_from("<I", d, 0)[0]
    cats = []
    for i in range(n):
        addr, nb = struct.unpack_from("<II", d, 16 + 8 * i)
        ids = [struct.unpack_from("<I", d, addr + 4 * m)[0] for m in range(nb)] if nb else []
        cats.append(ids)
    return cats


# ---------------------------------------------------------------- efp
def parse_header(d):
    r = R(d)
    name = d[0:4]
    r.p = 4
    ntpl = r.u16(); regist = r.u16(); texaddr = r.u32(); version = r.u32()
    idx = [r.u32() for _ in range(ntpl)]
    return name, ntpl, regist, texaddr, version, idx


def parse_template_head(d, off):
    r = R(d, off)
    g = (r.u32(), r.u16(), r.u16(), bytes(d[r.p:r.p + 8])); r.p += 8
    tid = r.u32(); ver = r.u32(); panim = r.u32(); ptex = r.u32()
    return GUIDS.get(g, "unknown %08x" % g[0]), tid, ver, panim, ptex, r.p


def parse_isetup(r):
    s = {}
    s["version"] = r.u32(); s["flag"] = r.u32()
    s["range"] = r.v3()
    s["sizeBase"] = r.s32(); s["sizeRand"] = r.s32()
    s["timePlay"] = r.u16(); s["groupLife"] = r.u16(); s["interval"] = r.u16()
    s["nbChilds"] = r.u16(); s["nbGroups"] = r.u16(); s["pad"] = r.u16()
    return s


def parse_after(r):
    return {"nbAfters": r.u16(), "color": (r.s16(), r.s16(), r.s16(), r.s16())}


def parse_fade(r):
    return {"start": r.u16(), "time": r.u16(), "colorSub": (r.s16(), r.s16(), r.s16(), r.s16()), "pad": r.u16()}


def parse_pds(r):
    s = {"isetup": parse_isetup(r)}
    s["speed"] = {"dir": r.v3(), "pow": r.s32(), "rand": r.s32()}
    s["gravity"] = {"dir": r.v3(), "pow": r.s32(), "rand": r.s32()}
    s["emit"] = r.v3()
    s["after"] = parse_after(r)
    s["fade"] = parse_fade(r)
    s["circle"] = {"radius": r.s32(), "radiusAdd": r.s32(), "angleAdd": r.s32()}
    s["sizeSpread"] = (r.s32(), r.s32(), r.s32(), r.s32())
    return s


def parse_gds(r):
    s = {"isetup": parse_isetup(r)}
    s["after"] = parse_after(r)
    s["fade"] = parse_fade(r)
    s["gather"] = {"speedPow": r.s32(), "speedAdd": r.s32(), "rotate": r.v3()}
    return s


def cstr(b):
    return b.split(b"\0", 1)[0].decode("latin1")


def parse_mds(r):
    s = {"version": r.u32(), "flag": r.u32(), "scale": r.v3(), "diffuse": r.v3(), "ambient": r.v3(),
         "specular": r.v3(), "emission": r.v3()}
    for k in ("nameIma", "nameIta", "nameItp", "nameIva"):
        s[k] = cstr(r.d[r.p:r.p + 48]); r.p += 48
    return s


SEQ_NAMES = ["WAIT", "POSITION", "BOOT", "HALT", "MOVE", "END"]


def parse_seq(d, off):
    r = R(d, off)
    s = {"PathOffset": r.u32(), "SeqTime": r.u32(), "uiFlag": r.u32(), "res": r.u32(), "u32_4": r.u32(),
         "dummy": (r.u32(), r.u32(), r.u32())}
    end = off + s["PathOffset"]
    cmds = []
    while r.p < end:
        c = struct.unpack_from("<I", d, r.p)[0]
        start = r.p
        if c == 0:
            r.u32(); cmds.append(("WAIT", {"time": r.u32(), "res": (r.u32(), r.u32())}))
        elif c == 1:
            r.u32(); cmds.append(("POSITION", {"pos": (r.f32(), r.f32(), r.f32())}))
        elif c == 2:
            r.u32()
            b = {"category": r.u32(), "member": r.u32(), "pos": (r.f32(), r.f32(), r.f32()),
                 "figure": (r.f32(), r.f32(), r.f32()), "id": r.u32(), "emitterID": r.u32(),
                 "path_id": r.u16(), "flag_coord": r.u8(), "layer": r.u8()}
            cmds.append(("BOOT", b))
        elif c == 3:
            r.u32(); cmds.append(("HALT", {"index": r.u32(), "id": r.u32(), "res": r.u32()}))
        elif c == 4:
            r.u32(); cmds.append(("MOVE", {"pathNo": r.u32(), "target_id": r.u32(), "res": r.u32()}))
        elif c == 5:
            cmds.append(("END", {"raw": d[r.p:r.p + 16].hex()})); r.p += 16
            break
        else:
            cmds.append(("?%d" % c, {"at": hex(start)})); break
    s["cmds"] = cmds
    # path header
    p = R(d, end)
    ph = {"fileType": p.u32(), "version": p.u32(), "num": p.u32(), "res": p.u32()}
    ph["index"] = [p.u32() for _ in range(ph["num"])]
    paths = []
    for i, o in enumerate(ph["index"]):
        q = R(d, end + o)
        b = {"count": q.u32(), "frameTime": q.u32(), "flag": q.u32(), "res": q.u32()}
        n = b["count"]
        b["path"] = [(q.s32(), q.s32(), q.s32(), q.s32()) for _ in range(n)]
        nf = (n >> 2) + 1
        b["figure"] = [(q.s32(), q.s32(), q.s32(), q.s32()) for _ in range(nf)]
        b["timing"] = [q.u32() for _ in range(n)]
        paths.append(b)
    ph["paths"] = paths
    s["pathHeader"] = ph
    return s


def parse_anim(d, off):
    r = R(d, off)
    a = {"NumUV": r.u32(), "NumScl": r.u32(), "NumClr": r.u32(), "res1": r.u32()}
    ouv, oscl, oclr = r.s32(), r.s32(), r.s32()
    a["res2"] = r.u32()
    a["offs"] = (ouv, oscl, oclr)
    r.p = off + ouv
    uv = {"sizeU": r.u16(), "sizeV": r.u16(), "startU": r.u16(), "startV": r.u16(), "texW": r.u16(),
          "texH": r.u16(), "numInterp": r.u16(), "dummy": r.u16(), "flag": r.u32(),
          "res": (r.u32(), r.u32(), r.u32())}
    uv["seq"] = [(r.u16(), r.u16()) for _ in range(a["NumUV"])]  # (time, pattern)
    r.p = off + oscl
    sc = {"flag": r.u32(), "res": (r.u32(), r.u32(), r.u32())}
    sc["seq"] = [(r.u32(), r.s32(), r.s32(), r.u32()) for _ in range(a["NumScl"])]  # time,x,y,res
    r.p = off + oclr
    cl = {"flag": r.u32(), "res": (r.u32(), r.u32(), r.u32())}
    seq = []
    for _ in range(a["NumClr"]):
        t = r.u32(); res = (r.u32(), r.u32(), r.u32())
        c = (r.s32(), r.s32(), r.s32(), r.s32())  # file order as the C# reads it: R, B, G, A  (see spec)
        seq.append((t, c))
    cl["seq"] = seq
    a["uv"], a["scl"], a["clr"] = uv, sc, cl
    return a


def parse_tex(d, off):
    r = R(d, off)
    t = {"code": d[off:off + 4].decode("latin1")}
    r.p = off + 4
    t["version"] = r.u32(); t["headerSize"] = r.u32()
    t["name"] = cstr(d[r.p:r.p + 32]); r.p += 32
    t["sizeNtft"] = r.u32(); t["sizeNtfp"] = r.u32(); t["sizeNtfi"] = r.u32()
    t["offNtft"] = r.u32(); t["offNtfp"] = r.u32(); t["offNtfi"] = r.u32()
    t["sendNtft"] = r.u32(); t["sendNtfp"] = r.u32(); t["sendNtfi"] = r.u32()
    t["flag"] = r.u32(); t["key1"] = r.u32(); t["key2"] = r.u32()
    t["fmt"] = r.u8(); t["gen"] = r.u8(); t["sizeS"] = r.u8(); t["sizeT"] = r.u8()
    t["repeat"] = r.u8(); t["flip"] = r.u8(); t["color0"] = r.u8(); t["reserve"] = r.u8()
    t["w"] = 8 << t["sizeS"]; t["h"] = 8 << t["sizeT"]
    return t


def decode_tex(d, off, t):
    """RGBA8 decode exactly as GlobalScope.LoadTexture does for effect textures (4x4 not supported there)."""
    w, h, fmt = t["w"], t["h"], t["fmt"]
    tex = d[off + t["offNtft"]: off + t["offNtft"] + t["sizeNtft"]] if t["offNtft"] else b""
    pal_b = d[off + t["offNtfp"]: off + t["offNtfp"] + t["sizeNtfp"]] if t["offNtfp"] else b""
    pal = [struct.unpack_from("<H", pal_b, i)[0] for i in range(0, len(pal_b) - 1, 2)]
    c0 = t["color0"]
    out = bytearray()

    def rgb5(c):
        return ((c & 31) << 3, ((c >> 5) & 31) << 3, ((c >> 10) & 31) << 3)

    for i in range(w * h):
        if fmt == 1:
            b = tex[i]; r, g, bl = rgb5(pal[b & 31]); a = (b >> 5) * 255 // 7
        elif fmt == 6:
            b = tex[i]; r, g, bl = rgb5(pal[b & 7]); a = (b >> 3) * 255 // 31
        elif fmt == 2:
            b = (tex[i >> 2] >> ((i & 3) * 2)) & 3; r, g, bl = rgb5(pal[b]); a = 255 if (b or not c0) else 0
        elif fmt == 3:
            b = (tex[i >> 1] >> ((i & 1) * 4)) & 15; r, g, bl = rgb5(pal[b]); a = 255 if (b or not c0) else 0
        elif fmt == 4:
            b = tex[i]; c = pal[b]
            r, g, bl = (c & 31) * 255 // 31, ((c >> 5) & 31) * 255 // 31, ((c >> 10) & 31) * 255 // 31
            a = 255 if (b or not c0) else 0
        elif fmt == 7:
            c = struct.unpack_from("<H", tex, i * 2)[0]; r, g, bl = rgb5(c); a = 255 if c & 0x8000 else 0
        else:
            r = g = bl = a = 0
        out += bytes((r, g, bl, a))
    return bytes(out)


def write_png(path, w, h, rgba):
    raw = b"".join(b"\0" + rgba[y * w * 4:(y + 1) * w * 4] for y in range(h))

    def chunk(t, b):
        c = struct.pack(">I", len(b)) + t + b
        return c + struct.pack(">I", zlib.crc32(t + b) & 0xFFFFFFFF)
    png = b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", struct.pack(">IIBBBBB", w, h, 8, 6, 0, 0, 0)) + \
        chunk(b"IDAT", zlib.compress(raw, 9)) + chunk(b"IEND", b"")
    open(path, "wb").write(png)


def flags_pds(f):
    names = ["LOOP", "AFTERIMAGE", "FADE", "MOVE_OFFSET", "XLU_DEPTH", "TEXOUT_AUTO", "TEXOUT_A5I3", "TEXOUT_A3I5"]
    return "|".join(n for i, n in enumerate(names) if f & (1 << i)) or "0"


def load_pack(path):
    d = open(path, "rb").read()
    name, ntpl, regist, texaddr, version, idx = parse_header(d)
    tpls = []
    for i, off in enumerate(idx):
        kind, tid, ver, panim, ptex, pparam = parse_template_head(d, off)
        nxt = idx[i + 1] if i + 1 < len(idx) else (texaddr if texaddr > off else len(d))
        t = {"off": off, "kind": kind, "id": tid, "ver": ver, "anim": panim, "tex": ptex, "param": pparam,
             "size": nxt - off}
        r = R(d, pparam)
        if kind in ("ParticleDS", "ParticleLargeDS"):
            t["setup"] = parse_pds(r); t["paramEnd"] = r.p
        elif kind == "ParticleGatherDS":
            t["setup"] = parse_gds(r); t["paramEnd"] = r.p
        elif kind == "ModelDS":
            t["setup"] = parse_mds(r); t["paramEnd"] = r.p
        elif kind == "SequenceDS":
            t["setup"] = parse_seq(d, pparam)
        if kind.startswith("Particle"):
            t["trail"] = cstr(d[t["paramEnd"]: off + t["size"]])
            if panim:
                t["animd"] = parse_anim(d, panim)
            if ptex:
                t["texd"] = parse_tex(d, ptex)
        tpls.append(t)
    return d, (name, ntpl, regist, texaddr, version, idx), tpls


def dump_pack(path, png_dir=None, brief=False):
    d, hdr, tpls = load_pack(path)
    name, ntpl, regist, texaddr, version, idx = hdr
    print("== %s  size=%d name=%r nTemplates=%d registFlag=%d TexAddr=0x%x version=0x%x" %
          (os.path.basename(path), len(d), name, ntpl, regist, texaddr, version))
    print("   pIndex=" + ", ".join("0x%x" % i for i in idx))
    for t in tpls:
        print("-- @0x%x %-16s id=0x%x(%d) ver=0x%x pAnim=0x%x pTex=0x%x size=0x%x" %
              (t["off"], t["kind"], t["id"], t["id"], t["ver"], t["anim"], t["tex"], t["size"]))
        if brief:
            s = t.get("setup")
            if t["kind"].startswith("Particle"):
                i = s["isetup"]; tx = t.get("texd")
                print("     flag=%s play=%d groupLife=%d interval=%d childs=%d groups=%d tex=%s" % (
                    flags_pds(i["flag"]), i["timePlay"], i["groupLife"], i["interval"], i["nbChilds"],
                    i["nbGroups"], ("%s %dx%d %s" % (tx["name"], tx["w"], tx["h"], FMT.get(tx["fmt"]))) if tx else "-"))
            elif t["kind"] == "SequenceDS":
                for c, a in s["cmds"]:
                    if c == "BOOT":
                        print("     BOOT %d/%d id=%d path=%d" % (a["category"], a["member"], a["id"], a["path_id"]))
                    elif c == "WAIT":
                        print("     WAIT %d" % a["time"])
                    else:
                        print("     %s %s" % (c, a))
            continue
        s = t.get("setup")
        if t["kind"] in ("ParticleDS", "ParticleLargeDS", "ParticleGatherDS"):
            i = s["isetup"]
            print("   isetup: version=0x%x flag=0x%x(%s) range=%s size=%g+rand(%g) play=%d groupLife=%d interval=%d "
                  "nbChilds=%d nbGroups=%d" % (i["version"], i["flag"], flags_pds(i["flag"]), fxv(i["range"]),
                                               fx(i["sizeBase"]), fx(i["sizeRand"]), i["timePlay"], i["groupLife"],
                                               i["interval"], i["nbChilds"], i["nbGroups"]))
            if "speed" in s:
                sp, gr = s["speed"], s["gravity"]
                print("   speed: dir=%s pow=%g rand=%g | gravity: dir=%s pow=%g rand=%g | emit=%s (65536=turn)" % (
                    fxv(sp["dir"]), fx(sp["pow"]), fx(sp["rand"]), fxv(gr["dir"]), fx(gr["pow"]), fx(gr["rand"]),
                    s["emit"]))
                c = s["circle"]
                print("   circle: radius=%g radiusAdd=%g angleAdd=%d | sizeSpread=%s" % (
                    fx(c["radius"]), fx(c["radiusAdd"]), c["angleAdd"], s["sizeSpread"]))
            if "gather" in s:
                g = s["gather"]
                print("   gather: speedPow=%g speedAdd=%g rotate=%s" % (fx(g["speedPow"]), fx(g["speedAdd"]), g["rotate"]))
            print("   after: %s | fade: %s" % (s["after"], s["fade"]))
            print("   trailing string: %r (param bytes 0x%x..0x%x)" % (t["trail"], t["param"], t["paramEnd"]))
            a = t.get("animd")
            if a:
                uv, sc, cl = a["uv"], a["scl"], a["clr"]
                print("   anim: NumUV=%d NumScl=%d NumClr=%d offs=%s" % (a["NumUV"], a["NumScl"], a["NumClr"], a["offs"]))
                print("     uv: cell=%dx%d start=(%d,%d) tex=%dx%d numInterp=%d flag=0x%x seq(time,pattern)=%s" % (
                    uv["sizeU"], uv["sizeV"], uv["startU"], uv["startV"], uv["texW"], uv["texH"], uv["numInterp"],
                    uv["flag"], uv["seq"]))
                print("     scale: flag=0x%x seq(time,x,y)=%s" % (sc["flag"], [(q[0], fx(q[1]), fx(q[2])) for q in sc["seq"]]))
                print("     colour: flag=0x%x seq(time,(c0,c1,c2,a))=%s" % (cl["flag"], cl["seq"]))
            tx = t.get("texd")
            if tx:
                print("   texture @0x%x: %s v0x%x hdr=%d name=%r fmt=%d(%s) %dx%d gen=%d repeat=%d flip=%d color0=%d "
                      "texel=%d@+0x%x pltt=%d@+0x%x idx=%d flag=0x%x" % (
                          t["tex"], tx["code"], tx["version"], tx["headerSize"], tx["name"], tx["fmt"],
                          FMT.get(tx["fmt"]), tx["w"], tx["h"], tx["gen"], tx["repeat"], tx["flip"], tx["color0"],
                          tx["sizeNtft"], tx["offNtft"], tx["sizeNtfp"], tx["offNtfp"], tx["sizeNtfi"], tx["flag"]))
                if png_dir:
                    os.makedirs(png_dir, exist_ok=True)
                    rgba = decode_tex(d, t["tex"], tx)
                    write_png(os.path.join(png_dir, "%s_%x_%s.png" % (os.path.basename(path)[:-4], t["id"],
                                                                      tx["name"].replace(".tga", ""))),
                              tx["w"], tx["h"], rgba)
        elif t["kind"] == "ModelDS":
            print("   model:", s)
        elif t["kind"] == "SequenceDS":
            print("   seq: PathOffset=0x%x SeqTime=%d uiFlag=%d" % (s["PathOffset"], s["SeqTime"], s["uiFlag"]))
            for c, a in s["cmds"]:
                print("     %-8s %s" % (c, a))
            ph = s["pathHeader"]
            print("   paths: fileType=%r version=0x%x num=%d" % (struct.pack("<I", ph["fileType"]), ph["version"], ph["num"]))
            for k, p in enumerate(ph["paths"]):
                print("     path %d: count=%d frameTime=%d flag=0x%x path=%s figure=%s timing=%s" % (
                    k, p["count"], p["frameTime"], p["flag"], p["path"], p["figure"], p["timing"]))


def chain():
    d = open(os.path.join(ROOT, "player.chaindata"), "rb").read()
    off, size = struct.unpack_from("<II", d, 16 + 8 * 12)
    out = []
    for i in range(size // 32):
        rec = d[off + 32 * i: off + 32 * (i + 1)]
        mid = struct.unpack_from("<h", rec, 0)[0]
        cat = struct.unpack_from("<h", rec, 10)[0]
        mem = struct.unpack_from("<h", rec, 12)[0]
        out.append((mid, cat, mem, rec.hex()))
    return out


def survey():
    from collections import Counter
    kinds = Counter(); vers = Counter(); fmts = Counter(); flags = Counter()
    for p in sorted(glob.glob(os.path.join(ROOT, "e*.efp"))):
        try:
            d, hdr, tpls = load_pack(p)
        except Exception as e:
            print("ERR", p, e); continue
        for t in tpls:
            kinds[t["kind"]] += 1
            vers[(t["kind"], t["ver"], t.get("setup", {}).get("isetup", {}).get("version") if t["kind"].startswith("Particle") else t.get("setup", {}).get("version"))] += 1
            if "texd" in t:
                fmts[(t["texd"]["fmt"], t["texd"]["w"], t["texd"]["h"])] += 1
            if t["kind"].startswith("Particle"):
                flags[flags_pds(t["setup"]["isetup"]["flag"])] += 1
    print("kinds", kinds)
    print("versions", vers)
    print("tex formats", sorted(fmts.items()))
    print("particle flags", flags)


if __name__ == "__main__":
    a = sys.argv[1:]
    if a[0] == "efi":
        cats = load_efi()
        print("nbCategories", len(cats))
        for c in a[1:]:
            print(c, ["0x%x" % x for x in cats[int(c)]])
    elif a[0] == "efp":
        png = a[a.index("--png") + 1] if "--png" in a else None
        dump_pack(a[1] if os.path.isabs(a[1]) else os.path.join(ROOT, a[1]), png)
    elif a[0] == "kinds":
        for f in a[1:]:
            dump_pack(f if os.path.isabs(f) else os.path.join(ROOT, f), brief=True)
    elif a[0] == "chain":
        for r in chain():
            print(r[0], r[1], r[2], r[3])
    elif a[0] == "survey":
        survey()
```

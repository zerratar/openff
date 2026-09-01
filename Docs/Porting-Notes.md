# Porting notes

How this codebase was recovered, and the XNA/MonoGame differences that have bitten so
far. Written down because most of them fail silently.

## Where the source came from

The original sources are lost. The game is recovered by decompiling `syrcusW.dll` from
the unpacked Windows Phone XAP, which also carries a compiled-in Android compatibility
layer (the mobile build was an Android port shimmed onto XNA).

The decisive detail: **decompile with reference assemblies for the XNA namespaces.**
An unreferenced decompile produces code that cannot compile — no `override` keywords,
numeric enum literals, bogus struct casts. MonoGame's public API is namespace-identical
to XNA 4.0, so it can stand in:

```bash
# one copy of MonoGame.Framework.dll per XNA assembly name the DLL references
for n in Microsoft.Xna.Framework Microsoft.Xna.Framework.Game \
         Microsoft.Xna.Framework.Graphics Microsoft.Xna.Framework.GamerServices \
         Microsoft.Xna.Framework.Input.Touch Microsoft.Xna.Framework.Media \
         Microsoft.Xna.Framework.Audio Microsoft.Xna.Framework.Net; do
  cp MonoGame.Framework.dll "$refs/$n.dll"
done

ilspycmd syrcusW.dll -p -o out -r "$refs" --ignore-decompilation-errors
```

That took 257k lines of uncompilable output down to 200k lines with five distinct
problems. `GlobalScope` — one static class with ~350 nested types — is then split into
one file per type under `FF3.Game/GlobalScope/`, everything `partial`, so nothing moves
semantically.

**Caveat that comes with the recipe:** names resolved against MonoGame are wrong wherever
MonoGame numbers an enum differently from XNA. See below.

## Silent XNA/MonoGame differences

### `TouchLocationState` is numbered differently

| | 0 | 1 | 2 | 3 |
| --- | --- | --- | --- | --- |
| XNA 4.0 | Invalid | Released | Pressed | Moved |
| MonoGame | Invalid | Moved | Pressed | Released |

The decompiler turned the IL's raw numbers into MonoGame *names*, which swapped
`Released` and `Moved`. Verified identical: `ButtonState`, `SpriteSortMode`,
`PrimitiveType`, `SurfaceFormat`, `SoundState`, `MediaState`, `DisplayOrientation`.

When an enum looks suspicious, cross-check the raw number in the older dotPeek dump at
`Decompiled/syrcusW/GlobalScope.cs`, which kept the numeric form.

### `Texture2D.GetData` ignores `startIndex`

On DesktopGL, `PlatformGetData` takes a fast path when the requested rectangle covers the
whole texture, and that path calls `GL.GetTexImage(..., data)` — writing from index 0 and
ignoring `startIndex` entirely.

`MainActivity.loadTexture` stores the image width and height in `pixels[0]`/`pixels[1]`
and asks for pixels at offset 2. The header was therefore overwritten by the first two
(transparent, so zero) pixels; `LoadPNG` read the size back as 0×0, `getImageSize`
rounded that up to the 8×8 minimum, and **every background in the game became an 8×8
smear**. Fixed in `android/graphics/Bitmap.getPixels` by reading into a private buffer at
index 0 and placing it at the offset manually.

### `KeyboardInput.Show` leaves `IsVisible` stuck true

DesktopGL's `KeyboardInput.PlatformShow` throws `NotImplementedException`, but `Show()`
is `async` and sets `IsVisible = true` *before* awaiting it — so the exception is captured
in the returned task and the flag is never cleared.

`Guide.IsVisible` used to read that flag, and the game polls `Guide.IsVisible` every
frame: `Game1.Update` skips all input while it is set, and `Game1.Draw` takes the paused
path, which clears to black. One call to character naming therefore froze the game on a
black screen with audio still playing. Replaced by `Compat/TextEntry.cs`.

## Input architecture

The game is an NDS port with touch bolted on. Two input surfaces exist:

- **Touch** — `Android.onTouchDown/Move/Up` → `MainActivity.onTouchEvent`, which
  normalises against the view size that `GLSurfaceView.setRenderer` hard-codes as
  800×480.
- **Pad** — `GlobalScope.cont`, an NDS button bitmask (A=1, B=2, Select=4, Start=8,
  Right=16, Left=32, Up=64, Down=128, R=256, L=512, X=1024, Y=2048), read through
  `PAD_Read()` into `ds.CPad`, which does edge detection and key repeat. Nearly 200 call
  sites poll it, including every menu.

The phone build only ever set the B bit here, from the hardware Back button, so the pad
path was dormant but intact. The Windows port feeds the keyboard into it from
`MainActivity.getKeyEvent`, which means edge and repeat behaviour comes free and matches
the DS original.

MonoGame's `TouchPanel` is not used: `EnableMouseTouchPoint` relies on SDL emitting
synthetic touch events, which it does not do for a plain mouse on Windows. Mouse state is
translated to the touch callbacks directly in `Compat/DesktopInput.cs`.

Do not route keys through `Android.onKeyDown`: it only maps to the Back keycode, and an
unhandled Back calls `Android.finish()`, which quits the game.

## Fast-forward

`GlobalScope.boost` is the game's own speed switch — the frame catch-up loop in
`render()` triples its iteration count when set. It is declared and read but never
assigned anywhere in the decompiled code (the Android build drove it over JNI), so the
port sets it directly while Tab is held.

## Text rendering

Text is one tinted `SpriteBatch.DrawString` per glyph, through `GlobalScope.Font` →
`GlobalScope.Graphics.DrawString`, using `Font12_*` / `Font16_*` SpriteFont pages selected
per character from the `.glp` glyph tables.

The atlases are **premultiplied white alpha masks** — RGB equals A at every pixel, in
three discrete levels — so all colour comes from the tint and the font cannot supply an
outline by itself. The outline is a separate pass in the 2D text renderer:

```csharp
if ((tEXT_DATA.flags & 0x4000) != 0)
    drawString(text, x + 1, y + 1, 255, size);   // 0x000000FF: opaque black, offset 1px
drawString(text, x, y, num11, size);
```

Colours are packed `0xRRGGBBAA`. Note that the tint is *not* premultiplied while the
atlas is, so tints with alpha < 255 come out brighter than they should — this matches
XNA's behaviour, so it is faithful rather than a port bug.

## Compatibility shims

Everything new lives in `FF3.Game/Compat/`; the decompiled sources are kept as close to
the decompiler output as possible so they can be re-derived.

| File | Replaces |
| --- | --- |
| `GamerServices.cs` | `Microsoft.Xna.Framework.GamerServices` — Guide, achievements (local file store) |
| `MicrosoftPhoneTasks.cs` | `WebBrowserTask` → default browser |
| `DesktopInput.cs` | TouchPanel; mouse and keyboard → touch callbacks and pad register |
| `TextEntry.cs` | The phone's system keyboard, for character naming |
| `ContentLocator.cs` / `GameFiles.cs` | `TitleContainer` path resolution |
| `Log.cs` / `Diagnostics.cs` / `ScreenCapture.cs` / `FontDump.cs` | Diagnostics |

Probes inserted into decompiled files are single lines tagged `/*FF3LOG*/` and are
inserted/removed by a script, so they never drift from the decompiled baseline.

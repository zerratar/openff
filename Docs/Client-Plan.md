# The client: OpenFF

Where the game goes now that the editor is in good shape. The aim Karl set: one client
that runs FF3, FF4 (3D) and mods that mix the two, from the content people already own,
rendering natively at desktop quality - TrueType text like the Steam release, not the
phone's 12- and 16-pixel atlases - and moddable all the way down, so that targeting
"our" client can do things the shipped engines cannot.

This continues `Modernisation-Plan.md` (stages 1-2 are done: the phone boot chain is
gone, rendering is native). It is ordered so the game keeps running at every step and
each step is useful on its own.

## Stage A - one content layer, shared with the editor (done)

The editor already reads every shape the content comes in: our `data*.bin` archives,
Steam FF3's loose `files/`, FF4's `SSAM` mass files, all behind `IContentSource`, with
a project's edits layered on top. The client reads only the archives, with one loose
override directory. Moving the sources into `Shared/Content` and reading through a
`ContentChain` (overrides, then project files, then the shipped content whatever its
shape) gives the client:

- `--content=<Steam FF3 install>`: boot from the game people bought. Nothing to extract.
- the editor's projects as the client's mods, with no copy step - the same folder.
- FF4's files readable by the same code, which is the first requirement of running FF4.

The chain has no automatic fallback: what is in `--content` is all the client reads.
Fonts and sound come from the install too (stages B and C), so a Steam install is enough
on its own - see "Steam-only" below.

## Steam-only - nothing of ours needed at runtime (done)

The requirement: people can only play with what they bought, because none of Square's
assets can be distributed with the client. So a boot from `--content=<Steam FF3>` must
need nothing from our `Content/`. Three things stood in the way, all in the 2D layer:

- **44 files Steam does not ship** - the about and repair title sprites, the achievement
  `link_icon_00..37` and the menu's `icon_16dot_2` - all phone-only features. The 2D
  loaders now treat a missing file as an empty bank (`NNS_G2dGetUnpackedBank` and the
  cell/anim/sequence lookups return nothing instead of dereferencing), so those screens
  simply draw nothing there.
- **Steam's cell banks are laid out for Steam's UI.** Its `.NCER` files place and size
  cells for a widescreen desktop over sheets authored at 1x, 1.2x, 1.33x and 2.22x the
  phone's. But both builds keep the same cells in the same order (Steam only appends to
  `m014_button`), and each Steam OAM's sheet rectangle covers the same picture as the
  phone's. So `Tools/gen_steam_cells.py` writes `FF3.Game/Data/ff3-steam-cells.json` -
  the phone's placement (x, y, w, h, flags) for the 29 banks that differ - and
  `Compat/SteamCells.cs` applies it when a bank is loaded from a Steam install, keeping
  the file's own sheet rectangle as the source. `NNSG2dCellOAMAttrData` grew a source
  size (`srcW/srcH`) for this, and `NNS_G2dDrawCell`/the BG path pass it to `drawImage`.
  The table is a few hundred positions of layout metadata we generated, not art.
  `--steam-cells-off` draws Steam's banks as they are.
- **The `.glp` glyph tables** were read at start-up from our XNBs; they are now optional
  (TrueType does not use them) and the atlas text path returns when a size has none.

Verified: from the Steam install alone the client shows the logos, the title (logo,
menu, cursor), naming, the opening, and plays the first battle in the Altar Cave with its
windows, HP bar and targeting cursor. A boot from our archives is unchanged.

## Stage B - TrueType text (done)

The Steam build ships `SE-EYEGLCOB.TTF` (the FF face), `TBUDRGoStd-Bold.otf`, `arial`,
`unifont`, `simsun`, `mona`; the phone build baked two sizes into 256-page atlases and
indexed them through `.glp` tables. The game's text path (`GlobalScope.Graphics`,
`drawString`/`measureString` over `SpriteFont` pages) gets a second implementation over
a TrueType rasteriser (FontStashSharp is the MonoGame-native choice), rendering at the
window's real resolution with the same metrics the atlases had, so layout does not
move. `--text=atlas` keeps the old path for A/B. Fonts are found in the Steam install
or beside our content; a mod can ship its own.

Done in `Compat/TrueTypeText.cs`: every string the game draws goes through
`Graphics.DrawString`/`StringWidth`, and those ask TrueTypeText first. The face is
rasterised at `size x 1.15 x window scale` (the atlas glyphs ran a little large) and
drawn back down, so text is sharp at 1600x960 and lays out where it did. Faces: `--font`,
then the install the content came from (Arial, Arial Unicode, TBUDRGothic, unifont - one
FontSystem, per-glyph fallback), then `Content/Fonts`, then Windows' Arial. The title and
its menu are unaffected - they are baked art, not text.

## Stage C - native sound (done for Ogg)

Steam FF3 ships `sound/BGMnn_0.ogg` + `_1.ogg` (intro and loop) with `.dat` loop points -
the same names our XNBs use. A decoder (NVorbis) into `SoundEffect`/`DynamicSoundEffectInstance`
lets the client play Steam's audio directly, and FF4's `.akb` (an Ogg with a header) with
the same code. `SoundManager` already keys everything by name; only the loader changes.

Done in `Compat/OggSound.cs`: `MediaPlayer.setDataSource` asks the chain for
`sound/<name>.ogg` (or FF4's `files/SOUND/BGM|SE|VOICE/<name>.akb`, skipping to the
`OggS` page) and decodes it with NVorbis into a `SoundEffect`, cached by name; the XNB is
loaded only when the chain has no Ogg. From the Steam install the title theme, the
opening's intro-and-loop pair and the effects all come from Steam's own files.

## Stage D - FF4

FF4 is the same engine with a 500-entry script table, its own tables, `setInsideMapJump`
exits and a few format extensions the editor already handles. Running it in the client
means: the script interpreter takes its table from `ScriptOpTable` (shared with the
editor - the 255 FF4-only handlers are the actual porting work, in order of how often the
scripts use them); the table readers take FF4's layouts; menus read `<layout>/<unit>`.
The measure of progress is maps that play through: Baron castle first (`d01_00`), the
opening (`t01_00`) second.

## Stage E - what "our client" can do that the engines cannot

Once both games run from the same code: higher-resolution textures by name (a mod drops
a 4x PNG next to the model), TrueType at any size, widescreen UI layouts, more casts per
map, longer scripts, new opcodes exposed to the script language, and a mod manifest the
editor's Export already writes. Each is small once A-D exist; none is possible before.

## Working rules

- Keep the game running at every commit; keep the old path behind a flag until the new
  one is proven, then delete the old path.
- Formats live once, in `Shared/`, compiled into both the editor and the client.
- Anything the editor learns about a format (the FF4 tables, the `.ncap` evaluator) is
  already the client's implementation - do not write it twice.
- `Docs/Testing.md` gets a case for every stage; the automated suites cover the formats.

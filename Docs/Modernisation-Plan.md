# Modernisation plan

Where the port goes now that it runs. The aim is a codebase that can absorb new
content — items, enemies, NPCs, quests, menus, dialogue — without fighting it.

## What the survey found

Encouraging: **the game barely touches the Android layer.** Across 200k lines of
`GlobalScope`, there are 14 references to it, spanning 7 types:

| Type | Uses | What for |
| --- | --- | --- |
| `DialogInterface` | 5 | name-entry callbacks |
| `InputFilter` | 3 | name length limit |
| `EditText`, `TextView` | 3 | name entry |
| `FileInputStream`, `File` | 3 | reading `.glp` glyph tables |
| `Activity` | 2 | lifecycle |

Everything else in `android/`, `java/`, `javax/` exists to serve the *port glue* —
`MainActivity`, `SoundManager`, and the boot chain — not the game. So the layer can
be removed by rewriting five files, not by touching the game.

## Stage 1 — remove what Windows has no use for (done)

The phone booted through a Square Enix login and a resource download before it would
start: `BootActivity` → `LoginActivity` → `DLActivity.startDownload` → entitlement
check → `MainActivity`. None of it applies to a build that ships its own data.

- `net.sqexm.*` deleted — 12 files of account, auth, and download plumbing
- `DLActivity` (563 lines) replaced by `Compat/GameArchive.cs`, which keeps only the
  part that matters: the `data*.bin` reader
- the entitlement gate in `MainActivity` removed — it quit the game if the auth
  handshake had not succeeded
- `BootActivity` now loads the archive table and starts the game

The table's "obfuscation" turned out to be a no-op — `MainActivity.encode` reads the
array and returns without touching it — so dropping the key handling loses nothing.

## Stage 2 — retire the remaining shims

Each of the 14 game-side references gets a native replacement, then the folders go.

| Shim | Replacement |
| --- | --- |
| `EditText` / `TextView` / `InputFilter` / `DialogInterface` | `Compat/TextEntry.cs`, already written — the game's calls just need to point at it |
| `FileInputStream` / `File` | `Compat/GameFiles.cs`, already written |
| `Activity` / `Android.cs` lifecycle | direct calls from `Game1`; the broadcast list only ever holds one activity |
| `android.graphics.Bitmap` | `Texture2D` directly — the shim is already only a wrapper |
| `GL10` / `EGLConfig` / `SurfaceView` | delete; `NativeRenderer` owns rendering now |
| `MediaPlayer` / `SoundManager` | a native audio service over MonoGame |

The GL emulation in `GlobalScope.Members.cs` goes the same way: `NativeRenderer`
already intercepts draw and clear, so the `gl*` functions can be deleted a few at a
time, each one verified with `--test=3d` and `--renderer=emulated` as the baseline.

## Stage 3 — structure for new content

Only worth doing once stage 2 is done, because the shims constrain the shape.

**Content.** `GameArchive` reports 6962 files across 52 volumes. The next step is an
extractor (`crystal extract-archives`) so the assets become editable, followed by
an override path — look for a loose file before falling back to the archive — so new
or modified content can be added without repacking.

**Menus and UI.** `menu.MenuBehavior` / `MenuBehaviorFactory` is already a factory
pattern; the work is to make registration data-driven rather than a hard-coded table,
so a new menu is a new file rather than an edit to a switch.

**Dialogue and events.** The event system (`evt`, 113k lines) interprets scripts from
the archives. A text-format script compiler targeting the same bytecode would let new
NPCs and quests be written without touching the interpreter.

**Data tables.** Items, enemies and jobs are fixed-size binary tables. Loading them
from an editable format, with the binary as fallback, makes balance changes and new
entries tractable.

## Where this continues

`Docs/Client-Plan.md` takes over from here: the shared content layer (done - the client
boots from the Steam install), TrueType text, native sound, FF4, and what a client of our
own can do beyond the shipped engines.

## Order

Stage 2 before stage 3, and rendering fully native before either. The principle
throughout: keep the game running at every commit, and keep the old path available
behind a flag until the new one is proven.

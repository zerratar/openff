# Audio

445 sounds: 30 music tracks and the rest effects. They live beside the game as XNBs,
not in the archives, and the editor's **Audio** tab lists them, plays them, and says
which scripts play them.

```bash
dotnet run --project Crystal.Editor -- editor --content=Content
```

## How a script reaches a sound

```
playBGM 1, 100, 30       ->  "BGM01"     ->  BGM01_0.xnb  +  BGM01_1.xnb
playSE 270, 1, 192, 127  ->  "SE270_01"  ->  SE270_01_0.xnb
```

`NNS_SndMain` builds the name from the request - `BGM%.2d` for music,
`SE%.3d_%.2d` for effects - and `SoundManager.playSound` appends `_0` and `_1`,
because a track can come in two parts:

- **`_0`** the intro, played once
- **`_1`** the loop, played forever after it

Where both exist, the loop point is in `sound/<name>.dat` in the archives: one 32-bit
count of milliseconds, and nothing else. That is why there are exactly 30 `.dat` files
and 30 music tracks.

`SoundManager.SoundAssignTable` is the bitmask saying which of the two parts a sound
actually has - bit 0 for the intro, bit 1 for the loop.

## What the editor shows

For each sound: whether it is music or an effect, its length, sample rate and channel
count, which parts it has, and its loop point in seconds.

- **Listen** - a player per part, decoded from the XNB on the fly. Nothing is written
  to disk to do it.
- **Play it from a script** - the exact line, with the numbers filled in:
  `playBGM 1, <volume>, <fadeinFrame>`.
- **What plays it** - every script that plays this sound by number, with how many
  times, and a link that opens it. Built by walking all 356 scripts once and reading
  the operands of `playBGM` and `playSE`.

A sound with nothing listed under **What plays it** is not unused - it means no script
plays it *by number*. Menu sounds, battle sounds and anything the engine starts
directly will not appear, because they never go through a script.

## Replacing a sound

Not yet. Sounds are XNBs loaded by MonoGame's content manager, not archive entries, so
the override directory does not reach them - a replacement means writing an XNB, and
the tool only reads them today. `crystal extract` already pulls all 475 of them out
as WAV, so the missing half is a writer.

Nothing in the game's audio code has been rewritten. It works, and the only thing
touched here is reading it.

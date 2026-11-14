# Command line and diagnostics

`OpenFF.exe` with no arguments finds your Steam or GOG installs, boots FF3 and remembers the
choice in `%LocalAppData%\OpenFF\launch.json`. These switches change that for one run.

## Starting

| Switch | Effect |
| --- | --- |
| `--game=ff4` | Boot FF4 instead (remembered for the next start) |
| `--content=<dir>` | Use an install that is not where Steam or GOG usually puts it |
| `--map=<id> --pos=x,y,z --rot=<deg>` | Start on any map of either game, skipping the title (`d01_05`, `t01_00`, `f00`, `e01_00`) |
| `--party=4:10 --gil=500 --load=<slot>` | FF4 test starts: members by type and level, gil, a saved slot |
| `--drive=<file>` | Play a scripted key drive from inside the game (see [Drives](Drives/)) |
| `--nomods` | Load no mod code |
| `--no-achievements` | Leave Steam alone for this run: no achievements, and Steam doesn't show you in the game (`"achievements": false` in settings.json turns them off for good) |
| `--achievements-dry` | Read Steam's achievements and log what would be earned, sending nothing; for testing a mod or a change without touching your Steam account |

## Display

| Switch | Effect |
| --- | --- |
| `--size=1600x960`, `--fullscreen`, `--borderless`, `--windowed` | Window for this run |
| `--msaa=off\|2\|4\|8`, `--novsync` | Anti-aliasing and vsync for this run |
| `--fps=30\|60\|max` | `30` shows the game's frames as they are. `60` (the default) draws a frame between each two of the game's, with motion and the camera smoothed. `max` smooths at the display's rate. On a display that 60 does not divide, `60` uses the nearest even rate above 60 (72 at 144 Hz). The game's logic always runs 30 steps a second, so nothing it computes changes. Also in the pause menu (Esc) |

The lasting settings (size, window mode, anti-aliasing, vsync, frame rate, the pad's buttons)
are in `%LocalAppData%\OpenFF\settings.json`, written with its defaults on the first start.

## Diagnostics

Every run writes `logs/ff3.log` beside the executable (the previous run is kept as
`ff3.prev.log`; the name is the same for FF4).

| Switch or variable | Effect |
| --- | --- |
| `--log=general,file` (`all`, `gl`, `texture`, `content`, `sound`, `input`, `event`, `firstchance`) | The log channels; `FF3_LOG=all` does the same |
| `--log-file=<path>`, `FF3_LOG_FILE=<path>` | Write the log elsewhere |
| `--screenshot-dir=<dir> --screenshot-every=<s>` | Take a picture every N seconds; with `--drive=` this gives a headless run you can check from its pictures |
| `--trace=<file>` | A parity trace of what the game did (flags, messages, sounds, maps, everyone on the map at each `say` of the drive). `Tools\parity.ps1` runs a drive with and without mods and diffs the two |
| `--debug=all` or `--debug=boxes,labels` | Start with the F1 overlay on |
| `--log-stalls` | Log every frame that took over 50 ms, to find where a hitch comes from |
| `--probe` | A per-frame heartbeat in the log (vertices, bounds, camera, world state) |
| `--ff4table` | List every FF4 script command that is only skipped |
| `--ff4usage=1`, `--ff4ops=<command>,...` , `--ff4ops=@<map>` or `--ff4ops=@*` | Every script command FF4's scripts use (`logs/ff4-commands.txt`), or each use of some commands, or a map's (or every) whole script, with operands (`logs/ff4-ops.txt`) |
| `FF3_DUMP=<dir>`, `FF3_DUMP_FONTS=<dir>` | Dump decoded data and font atlases as they load |
| `FF3_SPEED=<n>` | While fast-forwarding, each game step runs n times |

`firstchance` is worth knowing about: much of the game's own logic swallows exceptions, so a
porting bug usually shows up as "nothing rendered" rather than a crash. On FF4, every scene and
map ends with one log line listing the script commands it skipped.

## The editor's command line

`crystal.exe` alone opens Crystal in the browser. With a command (`extract`, `script`,
`tables`, `msd`, `pak`, `xbn`, `mdl`, `tex`, `cells`, `hich`, `mcl`, `lz` ...) it converts the
games' files on the command line instead; `crystal help` prints the list. See [Editor.md](Editor.md).

## Steam achievements

When the game's files are a Steam install (`steam_api.dll` beside `FF4.exe` or `FF3_Win32.exe`; a GOG copy
has none) and Steam is running, OpenFF connects to your Steam client as that game. Steam then shows you
playing it, counts the playtime, and takes its achievements as the game earns them. OpenFF uses the Steam
client already on your machine (`steamclient64.dll` in Steam's folder) and ships nothing of Valve's.

FF4's achievements follow the game's own rules from `achievement_param.bbd` beside `FF4.exe`: the same
checks (flags, items, augments, summons, levels, gil, the Bestiary, bosses, places) at the same moments
(a scene starting or ending, New Game, entering a map, boarding a vehicle, winning a fight, learning an
augment, a shop). FF4's clear count is kept in the save, as the game keeps it: the ending's save counts the
clear, and New Game+ carries the count. FF3's 18 are awarded where FF3's own code awards them (the bosses,
gil, treasure, the Bestiary, the Ultima Weapon, the Iron Giant, the jobs). They also include any OpenFF
earned before it talked to Steam: those are kept in `%LocalAppData%\FF3\achievements.txt` and sent on
when it connects.

A run with `--drive` (a test drive, which sets flags, gil and parties outright) never sends anything and
doesn't add to that file.

Mods don't turn achievements off. The log says what happened, for example
`achievements: on - Steam counts this as app 312750, 74 rule(s)`,
`achievements: Steam's record in - 12 of 58 earned` and
`achievements: com.square_enix.ff4_achievement042 earned`, or why they are off
(`not a Steam install`, `Steam is not running`, `--no-achievements`).

Earning achievements from a client other than the game's own is between you and Steam: Valve and
Square Enix haven't approved OpenFF. Turn them off with `"achievements": false` if you'd rather not.

# Controls

Keyboard and game pad are fed into the game's own DS-style pad, so they work in every menu and
on the field. Mods read both through `Game.Input`.

| Input | Action |
| --- | --- |
| Arrow keys / WASD | D-pad |
| Z / Space / Enter | A (confirm). On FF4 only Enter, as on Steam |
| X / Backspace | B (cancel). On FF4 only Backspace, as on Steam |
| C / V | X / Y. On FF4, C is auto battle and Z switches a target to all, as on Steam |
| Q / E | L / R |
| Left Shift / Right Shift | Start / Select |
| Mouse | Touch (click, drag) |
| Tab | FF4's menu on the field; held, it fast-forwards fights and scenes |
| M | FF4's field map |
| Esc | The client's menu: resume, settings, exit. Works anywhere, the title included |
| Alt+Enter | Switch between windowed and full screen |
| F1 | Debug overlay (F2 menu frames, F3 ids, F4 sprites, F5 world text, F6 stats) |
| F12 | Screenshot |

## Game pads

XInput, DualShock, DualSense, Switch Pro and others work, plugged in at any time.

- The d-pad or left stick moves. The stick's push sets the pace: a walk part way, a run all
  the way (`"run": "stick"` in `settings.json`; `"hold"` makes a button the run instead).
- By default Cross/Circle/Square/Triangle are the DS's A/B/X/Y, L1/R1 are L/R, Options is
  Start, Share is Select, R2 runs and L2 fast-forwards. Rebind them in the client's menu
  (Settings) or in `settings.json` under `pad`.
- Hold Start (Options) to open the client's menu; a tap is the game's own Start.
- A pad SDL does not know can be described in a `gamecontrollerdb.txt` beside the executable.

## A few FF3 details

- **Name entry:** type, or with a pad: Up/Down between the name and OK, A opens the on-screen
  keys (d-pad picks, A types, B deletes, Start is Done).
- **Menu tabs:** X / Y (C / V) turn a menu's tabs, such as Items / Key Items or Config 1 / 2.
  L / R do too where they are not the character switch.

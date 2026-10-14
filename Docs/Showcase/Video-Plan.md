# Video plan

Two videos first: a short teaser now, and an FF4 progress video when FF4 is further along (or
done). Everything below is about what OpenFF does that the original games don't, plus proof
that FF3 is complete and FF4 is close.

## How to capture (repeatable)

Record from **drive files** instead of playing by hand, so any shot can be re-taken
identically after a fix:

- `--drive=<file>` plays the keys; `shots N` writes N consecutive frames;
  `--screenshot-dir` and `--size=1920x1080` set where and how big.
- `ffmpeg -framerate 60 -i shot%04d.png -c:v libx264 -crf 16 -pix_fmt yuv420p clip.mp4` turns
  the frames into a clip. Sound is added in the edit, or recorded live with OBS on a second pass.
- For side-by-side shots with the original, `Tools/ff4hook` records Steam FF4 with the same
  timing.

## 1. Teaser: "Two games. One engine. Yours to change." (60-90 s)

| Time | Shot | What it shows |
| --- | --- | --- |
| 0-5 s | Black, the FF3 crystal chime; FF3's title fades in | It's FF3, as you remember it |
| 5-10 s | Hard cut on a beat to FF4's Red Wings deck | ...and FF4, in the same program |
| 10-20 s | The window dragged from small to 4K-sharp; the text stays crisp | Native rendering, any size |
| 20-28 s | Split screen: the same walk at 30 fps (left) and smoothed 60 fps (right) | Smoother than the original |
| 28-40 s | An FF4 battle. Mid-fight a mod is rebuilt and the enemy turns into an FF3 boss, still fighting | Hot-reload mods; both games' content in one battle |
| 40-48 s | FF4's menu, one key toggles the Starlit restyle | Menus are layouts anyone can restyle |
| 48-56 s | Gambits running a battle on their own; fast-forward on | New features the originals never had |
| 56-66 s | Quick cuts: the Survivors mod swarm, two players together over LAN (Fellowship), FF3 with FF5's jobs (Mastery) | Mods can make whole new games |
| 66-78 s | Crystal: a map edited in 3D, *Run in OpenFF*, the change is in the game | Anyone can make these |
| 78-90 s | The Lunar Whale lifting off over the Moon; logo; "Bring your own copy. Free and open source." | End card |

## 2. FF4 progress video (3-4 min)

A tour that ends on "and it all runs from your own copy":

1. **Frame by frame with Steam**: a split screen of the opening scene in Steam FF4 and OpenFF,
   cut so the movements match exactly. This shows FF4 is built from the game's own code.
2. **Cutscenes**: Mysidia, the Lunar Whale rising from the sea, the ending's staff roll.
3. **Battles**: Jump, Twincast, a summon, a boss with its scripted turns, a back attack.
4. **Vehicles**: the Enterprise's hook lifting the Hovercraft, diving through the hole to the
   Underworld, the black chocobo flying home, the Whale's bridge.
5. **Extras**: the Fat Chocobo's Bestiary and Music Box.
6. **Mods on FF4**: the battle HUD restyled; an enemy swapped.

## More ideas

- **"Enemy swap" short (15-30 s, for social media)**: the same FF4 fight looped, the enemy
  swapped each loop (an FF3 boss, a Survivors monster, a mod's own model). One caption:
  "Every enemy here is a mod."
- **"Same game, side by side" short**: Steam on the left, OpenFF on the right, a scene played
  in sync, then OpenFF's window stretched to 4K. Credibility in 20 seconds.
- **"Mod in 60 seconds"**: a timelapse of Crystal from empty project to a new weapon showing in
  the shop, then bought and equipped in game.
- **"Cross-over party"**: Cecil and Kain in FF3's world, or FF3's Onion Knights fighting in an
  FF4 battle. Shows the shared engine better than any explanation.
- **Speed-up montage**: a whole dungeon cleared on fast-forward with Gambits, set to a remix
  of the battle theme. The "quality of life" pitch.
- **Developer diary series**: short videos per milestone (vehicles, battles, the menu
  rebuild) showing the Steam frame next to ours, how it was matched, and what changed.

## Notes

- Show only footage made from our own copies; don't publish ripped music on its own. Game
  audio under gameplay is fine for showing the client working.
- End every video with the requirement: you need your own copy of the games.

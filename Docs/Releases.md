# Releases

What each release carries, and how one is made. The download is one zip,
`OpenFF-<version>-win-x64.zip`: unzip anywhere, run `OpenFF.exe` to play, `crystal.exe` to
edit and make mods. It needs the Steam copies of the games on the machine (Final Fantasy III
and/or Final Fantasy IV, the 3D remakes); none of their data is in the zip or in this
repository. Windows 10/11, x64; the .NET runtime is inside, nothing to install. How a release
is made is in `Docs/Releasing.md`; each version's section below is its release's description.

## 0.1.0 - the first release (2026-09-09)

**The client.** Final Fantasy III from its Steam install, title to credits, as shipped -
rendered natively on the desktop, TrueType text at the window's resolution, music and
effects from the Steam build's Ogg files. Final Fantasy IV through the same client behind a
`GameProfile` seam: Baron and the overworld, the opening scenes, battles on FF4's stages
with its HUD, the menu, shops, inns, saves, encounters - a work in progress; what is still
missing is in `Docs/Client-Plan.md`.

**Mods, with Crystal.** A `mods/` folder beside the client, one mod per subfolder, made in
Crystal (the editor, in your browser):

- *Replace anything of the game's*: maps and their characters, exits, encounters; scripts
  in a readable language; text in every language; menus; tables (items, monsters, jobs …);
  models, textures, pictures, sounds - each in its own editor, saved into the project, never
  into the game, exported as a mod.
- *Add what the game never had*: items, monsters, formations, characters and text of the
  mod's own (definitions the client adds to the game's tables); **maps of the mod's own** -
  a scene file with a glTF for the ground and the look, exits, music, a sky and a camera;
  **glTF models** from Blender, static, animated (skins and clips) and solid (ground and
  walls); **music and sound effects** as Ogg Vorbis or WAV; **a font** for all the game's
  text; C# behaviours and services on the engine's API (`OpenFF.Engine`), hot-reloaded while
  the game runs.
- *Built-in components* placed from the editor, Unity-style: Chest, Talk, Encounter, Trigger,
  Exit, Music, Sound, Mesh, Roam, Wander, Motion, GameCast, CastScript, WhenFlags,
  MapSettings - with an inspector, a hierarchy, undo, autosave and *Play here*.

`Modding.md` in the zip walks through it; `Docs/API.md` in the repository is the API
reference; `Docs/Testing.md` is what was tried.

**Known gaps.** FF4 is not finished (see the plan). A map of the mod's own has no name on
the menu and no map screen. Mods that must also install into the Steam games (the *steam*
target) keep to the game's own formats: glTF, Ogg and TTF are for the OpenFF target.
Sound import for a Steam target (an XNB writer) is not there. There is no Linux or macOS
build yet, though nothing in the client is Windows-only but the build.

# Goblin Survivors

A real-time roguelite on the field, made only of parts the game already has. It is here to
show that OpenFF mods are not limited to the game's kind of play - and it is fun.

**Play.** On any map press **T**. Goblins - the game's own monster, model, stats and attack
motion - come at the hero in waves and swing when they reach them (the battle's damage
flash and floating numbers). The hero turns to the nearest goblin and throws a spell at it
on their own, with the battle's casting motion; walk to keep out of reach. Kills give the
goblins' own experience and gil (the party levels as it would) and this run's experience.
A run level deals **three cards**: pick with Left/Right and A, or 1 2 3 - faster bolts,
more power, longer reach, a second bolt, a new spell (Blizzard, Thunder...), a stat, a heal,
regeneration, greed. Goblins sometimes leave a **chest** with a piece of equipment the hero
can wear (walk into it; it is worn when it is better). When a wave is down a **trader**
appears: talk to open the game's own shop (weapons, armour, items, magic in turn), talk
again to start the next wave. The leader falling ends the run; T starts another.

**Install.** Double-click `install.cmd` (it builds into the client's mods folder), or

    dotnet build Samples/Survivors -o "<client>/mods/survivors"

**Read.** `SurvivorsService.cs` is the whole mod: a `GameService` with the run's state
(public fields, so a hot reload keeps them), a `Foe : Behaviour` per goblin. The API it
leans on: `Game.Monsters`, `Game.Magic` (data and formulas), `Game.Items` (what the chests
hold), `Game.Npcs.SpawnModel/Spawn` with `BindMotions`/`PlayMotion` for the battle's
motions on the field, `Game.Hero.BindMotions("b_b01")` + `HeroMotion.MagicShot`,
`Game.Party.Hurt/Heal/GiveExperience/SetStat/AddItem/Equip`, `Game.Screen.PopNumber/Flash`,
`Game.Camera.WorldToScreen` with `Game.Draw`, `Game.Input.Capture` while a card is chosen,
`Game.Dialogue.Ask` through the trader, `Game.Shops.Open(index, "t01")`, `Game.Field.OnGround`
and `Encounters`, coroutines for the death fade.

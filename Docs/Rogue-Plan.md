# Rogue Mode - plan

A run-based roguelite mode for FF3, as a sample mod (`Samples/Rogue`), in the shape of the
Mastery, Fellowship and Survivors samples: FF3's battles, jobs, magic, equipment, monsters
and bosses, with Rogue Mode deciding only what happens *between* battles. The campaign is
not touched: the mode lives in its own save profile, and a game started from New Game never
sees it.

```
Title ▸ Rogue Mode ──▶ the mod's screens (menus/) ──▶ Game.Battle.Start ──▶ FF3's battle
                          ▲                                                    │
                          └──────── BattleEnded, the battle hooks ◀───────────┘
```

## What it stands on (as found in the code)

| Need | What exists | Where |
| --- | --- | --- |
| An entry apart from the campaign | `Game.Title.AddEntry`, `Game.Title.NewGame(hero, map, pos, saveProfile)` - a profile keeps its saves and mod chunks apart (Fellowship's "fellowship") | Docs/API.md ITitle; Samples/Fellowship/Journey.cs |
| A place between battles | a battle background loads as a map (`b16`, Fellowship's shrine): the biome's own battlefield is the run's backdrop | Journey.ShrineMap |
| Screens | mod menus: an XML layout drawn in Crystal's Menus tab, a `.json` definition, `MenuBehaviour`s in C#, `MenuList`, bindings, stylesheets (Starlit Menu) | Samples/Mastery, Samples/StarlitMenu |
| Battles | `Game.Battle.Start(formation, battleMap)`, `BattleStarting`, `BattleEnded { Result }`, `EscapeAllowed` | Compat/EngineApi.cs LegacyBattle |
| The party | `Game.Party`: Reset, AddMember, SetJob/ChangeJob, SetLevel, GiveExperience, GiveAbp, Equip/Unequip, CanEquip, LearnSpell/ForgetSpell, SetCharges, Heal/HealAll/Hurt, Cure, Gil, AddItem, **Export/Import** (a hero's whole record as text) | Api.cs IParty |
| FF3's data | `Game.Monsters` (records, groups), `Game.Items` (categories, prices, who equips), `Game.Magic` (spells, levels, schools) | Api.cs |
| Saves | `ISaveable` chunks per save profile | Api.cs SaveChunks |
| Content of a mod's own | Crystal: formations, monsters, items, spells and their looks, effects, summons, menus, scenes | Docs/Editor.md |

What is missing is inside the battle: a mod hears it start and end, nothing between. The
battle code has the places (found in `btl/`):

| Hook | Place |
| --- | --- |
| a battle's monsters, stats settable (elites, scaling) | after `BattleMonsterParty.registerParty`, BattleSystem.cs:95 - the battle's copies are per monster, the shared records untouched |
| a physical hit's damage | between `calcNormalAttackDamage` and `setNormalAttackDamage` (PlayerTurnSystem.cs:579-583, where `BattleCommands.Adjust` already sits; MonsterTurnSystem.cs:284-292), crit in `PF_CRITICAL`, Jump in `PF_JUMP` |
| a spell's damage and healing | `BattleCalculation.calcAttackMagic` (before `subNow`, :173) and `calcRecoveryMagic` (:193) - spell id and element known |
| a turn starting | `TurnSystem.initializeTurn` (:318) |
| someone falling | `BattleCalculation.damageCharacter` (:690), `TurnSystem.deadMonster` (:2402) |
| a charge spent | `BattlePlayer.deleteItemOrMagicNumber` (:1218) |
| the winnings | `BattleCharacterManager.getTrueExp` (Qol.Exp already there), `BattleWin.windowOpenPhase` (gil, :205), `PlayerJobManager.addJobSkillExp` (Qol.JobExp) |
| a loss back to the field | `OutsideToBattle.onRestart` - the game's own "a battle you may lose" (BattlePart.cs:240) |
| a formation of the mod's making | `MonsterPartyManager.monsterParty(id)` - the table already takes formations appended past the game's |

## Engine API it adds (general, for any mod)

1. `Game.Battle.Start(MonsterGroup group, int battleMap, BattleOptions options)` - a
   formation made at run time (a reserved record of the table filled before the start), with
   options: `LossReturns` (a wipe ends on the field with `BattleEnded { Lost }`, not the
   title), `EscapeAllowed`, the battle's music.
2. `BattleMonstersReady` - every monster of the battle as a live `BattleUnit`: MaxHp/Hp,
   Attack, Defense, Magic, MagicDefense, Agility, Level settable, before the first turn.
3. `BattleDamage` - each hit, spell and heal as it lands: Attacker, Target (BattleUnit), Kind
   (Physical, Magic, Healing, Ability), SpellId, Element, Critical, Jump; `Amount` settable.
4. `BattleTurnStarting { Unit }`, `BattleUnitFell { Unit, By }`, and `BattleUnit.Revive(hp)`.
5. `BattleRewards { Exp, Gil, JobExp }` settable at the victory, and `ChargeSpending`
   (cancel to keep the charge).

These are the primitives the modifiers are made of: ModifyDamage, ModifyHealing, ModifyStat,
ModifyCritical, ModifyExperience/JobExperience/Gil, ModifySpellCharge, OnKill,
OnBattleStart, OnTurnStart, OnLowHealth, OnCast, OnPhysicalAttack, OnDamageTaken.

## The mod (Samples/Rogue)

- `Core/` - the run, with no engine in it, so it can be checked without the game:
  `RogueRandom` (seeded, named streams: map, encounter, reward, event, shop; its state
  saved), `RunState`, `RouteGenerator`, `EncounterGenerator` (danger budgets),
  `RewardGenerator`, `Scaling`, `Content` (the JSON below, loaded and checked).
- `Rogue*.cs` - the engine side: the title entry, the screens' behaviours, the battle
  hooks applying the run's modifiers, the save chunk.
- `menus/` - every screen, drawn in Crystal's Menus tab: Rogue Mode, New Run, Party Setup,
  Route, Victory (rewards), Shop, Rest, Event, Crystal, Active Effects, Run Summary.
- `data/` - the content, by FF3's ids - nothing of FF3's copied:
  `acts.json`, `biomes/*.json` (battlefield, music, encounter pools, elites, bosses, events,
  next biomes), `jobs.json` (costs, which crystal opens them), `rewards.json` (rarity bands
  from the items' own prices, overrides by id), `passives.json` and `elites.json` (made of
  the primitives above), `events/*.json` (requirements, choices, outcomes), `rules.json`
  (challenge rules). A monster's danger is worked out from its own record (level, HP) unless
  a biome says otherwise.
- `defs/` - what Crystal makes: formations (authored encounters and bosses), effects (the
  crystals, an elite's aura, a reward's reveal), spells.
- `Tests/` - a console checker in the repo's way (`crystal effect-cases`): seed determinism,
  map generation, weighted choice, encounter budgets, job costs, rerolls, boss placement,
  save and load equal.

The party between battles is the game's own (levels, job levels, equipment, spells, charges
and HP are the game's state); the run's save chunk keeps everything else - seed and RNG
states, the route, the act and node, the modifiers, the unlocked jobs, the run's statistics.
Meta-progression (mastery, unlocks, history) is one file of the mod's own, kept across runs.

## Phases

1. **A playable run.** Title entry; New Run with a seed; Party Setup with the job budget;
   a line of battles on act 1's biome from encounter budgets, every few an elite (scaled by
   BattleMonstersReady); a victory's 1-of-3 rewards (equipment, a spell, a modifier, gil, a
   heal) with rerolls; modifiers through BattleDamage; a boss at the act's end; a wipe or the
   boss ends the run with its summary. Saved between battles; Continue Run.
2. **The route.** A node graph per act, branches, battle / elite / boss / rest / shop nodes.
3. **Biomes.** Several, each its battlefield, music, pools and bosses; the choice after a boss.
4. **Jobs and crystals.** Crystal events open jobs; job changes at rest and crystal nodes;
   mastery-driven job costs.
5. **Events.** The generic event system: requirements, choices, chances, costs, rewards.
6. **Meta-progression.** Unlock currency, job mastery, unlocks, history, statistics.
7. **Bosses and Endless.** Boss phases (BattleDamage and BattleTurnStarting are enough for
   thresholds), stronger elites, Endless, daily seeds, challenge rules.

A Crystal page for the content (pools, rewards, passives, events with pickers of FF3's
monsters, items and spells, as the summon editor's) comes after phase 3, once the shapes have
settled; until then the JSON is edited in Crystal's code editor.

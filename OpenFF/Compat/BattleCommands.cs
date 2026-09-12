// Battle commands of the mods' own (OpenFF.Engine/BattleCommands.cs) in FF3's battle.
//
// A ladder step with "command": true carries an id of 64..99 (ModJobs); set into a free
// slot it reaches the battle's command window like any command id. Three touches on the
// game's code play it as the hero's plain attack with the mod's damage:
//
//   - menu.CommandWindow.createCommandMessage: a name for an id past the game's table.
//   - btl.BattleSetupPlayer.commandAction: the id chosen - the enemy pick, every enemy, or the
//     hero themselves, as the command's Target says - with the plain attack's action; the
//     command is remembered for the hero until it lands.
//   - btl.PlayerTurnSystem.initializeNormalAttack: between the attack's damage calculation and
//     its application, each target's damage is replaced by what the command's Damage says;
//     a negative one heals (the game's PF_RECOVER path shows it green).

using System;
using System.Collections.Generic;
using OpenFF.Content;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal static class BattleCommands
	{
		private static readonly Dictionary<int, int> _pending = new Dictionary<int, int>();

		/// <summary>The command a ladder id names, or null (no ladder step, or no class of that word in the mods' code).</summary>
		public static BattleCommand Of(int id)
		{
			if (!ModJobs.IsOwnCommand(id)) return null;
			ModJobAbility step = ModJobs.Step(ProgressionLayer.Ladders, id);
			if (step == null) return null;
			BattleCommand command = BattleCommandLoader.Find(step.Ability);
			if (command == null && _missingNoted.Add(id)) Log.Write(LogChannel.General, "battle: the ladder names a command '" + step.Ability + "' (" + step.ShownName + ") but no BattleCommand class of that word is in the mods' code; it strikes as a plain attack");
			return command;
		}

		private static readonly HashSet<int> _missingNoted = new HashSet<int>();

		/// <summary>What the battle's window shows for a command id of the mods' own.</summary>
		public static string NameOf(int id) => ModJobs.Step(ProgressionLayer.Ladders, id)?.ShownName ?? ("command " + id);

		/// <summary>The command chosen in the window: targets and action set as its Target asks. Returns the setup's "done choosing" (true when no target pick follows), or null for an id that is none of the mods'.</summary>
		public static bool? Select(GlobalScope.btl.BattleSetupPlayer setup, GlobalScope.btl.BattlePlayer player, GlobalScope.btl.BattleSystem battle, int id)
		{
			if (!ModJobs.IsOwnCommand(id)) return null;
			BattleCommand command = Of(id);
			CommandTarget target = command?.Target ?? CommandTarget.Enemy;
			_pending[player.playerId()] = id;
			switch (target)
			{
				case CommandTarget.Enemies:
					player.setActionId(1);
					battle.characterManager().setMonsterAllTarget(player);
					GlobalScope.btl.Battle2DManager.instance().cursor().nondisplayAll();
					return true;
				case CommandTarget.Self:
					player.setActionId(1);
					player.clearTargetId();
					player.setTargetIdMyself();
					GlobalScope.btl.Battle2DManager.instance().cursor().nondisplayAll();
					return true;
				default:
					setup.commandAttack(player, battle.characterManager().monsterParty(), 0);
					return false;
			}
		}

		/// <summary>The attack's damage is worked out: the pending command of the mods' own, if any, has its say per target.</summary>
		public static void Adjust(GlobalScope.btl.TurnSystem turn, GlobalScope.btl.BattlePlayer player)
		{
			try
			{
				if (player == null || !_pending.TryGetValue(player.playerId(), out int id)) return;
				_pending.Remove(player.playerId());
				BattleCommand command = Of(id);
				if (command == null) return;
				BattleActor actor = ActorOf(player);
				string line = null;
				OpenFF.Game.Guard(command.Name + ".Announce", () => line = command.Announce(actor));
				if (!string.IsNullOrEmpty(line)) Notices.Post(line);
				for (int i = 0; i < 12; i++)
				{
					if (player.targetId(i) < 0) continue;
					GlobalScope.btl.BaseBattleCharacter target = turn.characterManager().getBaseBattleCharacterFromBreed(player.targetId(i));
					if (target == null) continue;
					int before = turn.calc_.damage(target.battleCharacterId());
					int after = before;
					BattleActor targetActor = ActorOf(target);
					OpenFF.Game.Guard(command.Name + ".Damage", () => after = command.Damage(actor, targetActor, before));
					if (after < 0)
					{
						target.setFlag(GlobalScope.btl.PLAYER_FLAG.PF_RECOVER);
						after = -after;
					}
					if (after > 0) target.clearFlag(GlobalScope.btl.PLAYER_FLAG.PF_MISS);
					turn.calc_.setDamage(target.battleCharacterId(), Math.Min(9999, after));
				}
				Log.Write(LogChannel.File, "battle: " + player.player().name() + " used " + command.Name);
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "battle: command: " + ex.Message); }
		}

		private static Dictionary<uint, string> _battleNames;

		/// <summary>A name from eureka_battle.msd (monsters', abilities'), read once; null when it has none.</summary>
		public static string BattleName(uint id)
		{
			try { _battleNames ??= TableFiles.ReadNames(GameArchive.Chain, "eureka_battle.msd", new GameTables()); } catch (Exception) { _battleNames = new Dictionary<uint, string>(); }
			return _battleNames.TryGetValue(id, out string s) ? s : null;
		}

		private static BattleActor ActorOf(GlobalScope.btl.BaseBattleCharacter c)
		{
			BattleActor a = new BattleActor { Id = c.battleCharacterId(), IsMonster = c.breed() != 0 };
			try { a.Hp = c.hp().getNow(); a.MaxHp = c.hp().getLimit(); a.Level = c.level(); } catch (Exception) { }
			try
			{
				GlobalScope.ys.BodyParameter b = c.bodyAndBonus();
				a.Stats.Strength = b.strength().get(); a.Stats.Vitality = b.vitality().get(); a.Stats.Agility = b.dexterity().get(); a.Stats.Intellect = b.intelligence().get(); a.Stats.Mind = b.mind().get();
			}
			catch (Exception) { }
			if (c is GlobalScope.btl.BattlePlayer p)
			{
				try
				{
					a.Name = p.player().name();
					a.Member = OpenFF.Game.Party?.Member(p.playerId());
				}
				catch (Exception) { }
			}
			else if (c is GlobalScope.btl.BattleMonster m)
			{
				try { a.Name = BattleName((uint)m.monster().nameId()) ?? ("monster " + m.monsterId()); } catch (Exception) { a.Name = "monster"; }
			}
			return a;
		}
	}
}

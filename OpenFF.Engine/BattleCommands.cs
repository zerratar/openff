// Battle commands of a mod's own, for heroes on the mastery progression.
//
// A job ladder step that says "command": true names a BattleCommand class in the mod's code
// by its word:
//
//   { "abp": 40, "ability": "zeninage", "command": true, "name": "Zeninage" }
//
//   public sealed class Zeninage : BattleCommand
//   {
//       public override string Name => "Zeninage";
//       public override CommandTarget Target => CommandTarget.Enemies;
//       public override int Damage(BattleActor actor, BattleActor target, int attackDamage)
//       {
//           int gil = Math.Min(Game.Party.Gil, actor.Level * 50);
//           Game.Party.Gil -= gil;
//           return attackDamage + gil / 2;
//       }
//   }
//
// Learned, it goes into a free command slot like any command, shows in the battle's window
// under its Name, and is played as the hero's plain attack - the swing, the hit, the number -
// whose damage the class decides, target by target, from what the plain attack would have
// done: a formula of the mod's own, an element, a cost taken from the party. Target says who
// is struck: one enemy the player picks (the default), every enemy, or the hero (a negative
// Damage heals). What is not here: a command with its own animation, or one that casts,
// steals or inflicts - those are the game's own commands, which a ladder can teach as they are.

using System;
using System.Linq;

namespace OpenFF
{
	/// <summary>Who a battle command of a mod's own strikes.</summary>
	public enum CommandTarget
	{
		/// <summary>One enemy, picked by the player.</summary>
		Enemy,
		/// <summary>Every enemy standing.</summary>
		Enemies,
		/// <summary>The hero themselves (a negative Damage heals).</summary>
		Self
	}

	/// <summary>Someone in the battle, as a command sees them: a hero (Member set) or a monster.</summary>
	public sealed class BattleActor
	{
		/// <summary>The battle's id for them.</summary>
		public int Id { get; set; }
		public string Name { get; set; }
		public bool IsMonster { get; set; }
		public int Hp { get; set; }
		public int MaxHp { get; set; }
		public int Level { get; set; }
		/// <summary>Stats as the formulas read them (a monster's from its record).</summary>
		public Stats Stats { get; set; } = new Stats();
		/// <summary>The party member, for a hero; null for a monster.</summary>
		public PartyMember Member { get; set; }
		public override string ToString() => Name + " (" + Hp + "/" + MaxHp + ")";
	}

	/// <summary>A battle command of a mod's own; see the file's header. One instance serves every use.</summary>
	public abstract class BattleCommand
	{
		/// <summary>The word a ladder names it by: the class name in lower-kebab (ZenInage -> zen-inage) unless overridden.</summary>
		public virtual string Word => Slug(GetType().Name);
		/// <summary>What the battle's window shows.</summary>
		public abstract string Name { get; }
		public virtual CommandTarget Target => CommandTarget.Enemy;
		/// <summary>The damage to a target, from what the hero's plain attack would have done (0 for a miss); negative heals. Called once per target as the swing lands.</summary>
		public abstract int Damage(BattleActor actor, BattleActor target, int attackDamage);
		/// <summary>A line for the notices as the command is used, or null.</summary>
		public virtual string Announce(BattleActor actor) => null;

		public static string Slug(string s)
		{
			if (string.IsNullOrEmpty(s)) return "";
			System.Text.StringBuilder b = new System.Text.StringBuilder();
			for (int i = 0; i < s.Length; i++)
			{
				char c = s[i];
				if (char.IsUpper(c) && i > 0 && (char.IsLower(s[i - 1]) || (i + 1 < s.Length && char.IsLower(s[i + 1]) && char.IsUpper(s[i - 1])))) b.Append('-');
				b.Append(char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : '-');
			}
			return b.ToString().Trim('-');
		}
	}

	/// <summary>The mods' battle commands, found in their code.</summary>
	public static class BattleCommandLoader
	{
		/// <summary>A command by word across the loaded mods, or null.</summary>
		public static BattleCommand Find(string word)
		{
			if (string.IsNullOrWhiteSpace(word)) return null;
			string slug = BattleCommand.Slug(word);
			foreach (Modding.LoadedMod mod in Game.Mods)
			{
				foreach (BattleCommand c in mod.BattleCommands)
				{
					if (string.Equals(c.Word, slug, StringComparison.OrdinalIgnoreCase) || string.Equals(BattleCommand.Slug(c.GetType().Name), slug, StringComparison.OrdinalIgnoreCase)) return c;
				}
			}
			return null;
		}
	}
}

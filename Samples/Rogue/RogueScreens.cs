// Rogue Mode's screens, in the game's own menu system (menus/*.xml, drawn in Crystal's Menus tab; the
// frames with <window/> are the game's window art). Each screen's logic is a MenuBehaviour here:
//
//   rogue           the mode's own menu: New Run, Continue Run
//   rogue-party     a new run: each hero's starting job (left / right), the cost against the budget, the seed
//   rogue-camp      between battles: the act and the battle ahead, the party, gil; Fight, Active Effects, Abandon
//   rogue-reward    a victory's three choices side by side, and a reroll for gil
//   rogue-effects   the modifiers held, in full
//   rogue-summary   a run's end: how far it went and what it did
//
// Everything they show comes from RogueService (the run) and Game.Party (the heroes).

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OpenFF;
using Rogue.Core;

namespace Rogue
{
	internal static class Text
	{
		/// <summary>Words into lines of about this many characters (the menus' frames are one line each).</summary>
		public static List<string> Wrap(string text, int width, int lines)
		{
			List<string> result = new List<string>();
			string line = "";
			foreach (string word in (text ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries))
			{
				if (line.Length > 0 && line.Length + 1 + word.Length > width) { result.Add(line); line = word; }
				else line = line.Length == 0 ? word : line + " " + word;
			}
			if (line.Length > 0) result.Add(line);
			while (result.Count < lines) result.Add("");
			if (result.Count > lines) { result = result.Take(lines).ToList(); result[lines - 1] = result[lines - 1].TrimEnd('.') + "..."; }
			return result;
		}

		public static string Gil(int n) => n.ToString("N0", CultureInfo.InvariantCulture);
	}

	/// <summary>The mode's own menu.</summary>
	public sealed class RogueHub : MenuBehaviour
	{
		public override void OnOpen()
		{
			Game.Log("rogue: screen " + Menu.Id);
			RogueService s = RogueService.Instance;
			Menu.SetText("title", "Rogue Mode");
			Menu.SetText("sub", s == null ? "" : s.Content.Run.Acts.Count + " acts, from " + s.Content.Run.Acts.First().Name + " to " + s.Content.Run.Acts.Last().Name + ".");
			Menu.Widget("continue").Enabled = RogueService.HasSavedRun;
			Menu.Focus(RogueService.HasSavedRun ? "continue" : "new");
			Describe();
		}

		public override void OnFocus() => Describe();

		private void Describe()
		{
			switch (Menu.Focused)
			{
				case "new": Desc("Four heroes, a job each within the budget, and a run of FF3's battles.", "Win to choose rewards; fall, and the run is over."); break;
				case "continue": Desc(RogueService.HasSavedRun ? "The run you left, where you left it." : "No run waits to be continued.", ""); break;
				default: Desc("", ""); break;
			}
		}

		private void Desc(string a, string b) { Menu.SetText("desc1", a); Menu.SetText("desc2", b); }

		public override bool OnPress()
		{
			RogueService s = RogueService.Instance;
			switch (Menu.Focused)
			{
				case "new": Menu.SoundDecide(); Menu.Open("rogue-party"); return true;
				case "continue":
					if (!RogueService.HasSavedRun) { Menu.SoundBeep(); return true; }
					Menu.SoundDecide(); Menu.Close(); s.Continue(); return true;
			}
			return false;
		}

		// There is nowhere else to go in Rogue Mode: the screen stays.
		public override bool OnCancel() { Menu.SoundBeep(); return true; }
	}

	/// <summary>A new run: a starting job for each hero within the budget, and the seed.</summary>
	public sealed class RogueParty : MenuBehaviour
	{
		private static readonly string[] Defaults = { "warrior", "black-mage", "white-mage", "freelancer" };
		private readonly int[] _pick = new int[4];
		private List<JobDef> _jobs;
		private ulong _seed;

		public override void OnOpen()
		{
			Game.Log("rogue: screen " + Menu.Id);
			RogueService s = RogueService.Instance;
			_jobs = s.Content.Jobs.Jobs.Where(j => j.Starting).ToList();
			for (int h = 0; h < 4; h++) _pick[h] = Math.Max(0, _jobs.FindIndex(j => j.Word == Defaults[h]));
			_seed = NewSeed();
			Menu.SetText("title", "New Run");
			Show();
			Menu.Focus("begin");
		}

		private static ulong NewSeed() => RogueService.TestSeed != 0 ? RogueService.TestSeed : (ulong)(DateTime.Now.Ticks % 1000000000L);

		private int Cost => Enumerable.Range(0, 4).Sum(h => _jobs[_pick[h]].Cost);
		private int Budget => RogueService.Instance.Content.Jobs.Budget;

		private void Show()
		{
			for (int h = 0; h < 4; h++)
			{
				JobDef j = _jobs[_pick[h]];
				Menu.SetText("hero" + h, RogueService.HeroNames[h].PadRight(9) + "< " + j.Name + " >");
				Menu.SetText("cost" + h, j.Cost.ToString(CultureInfo.InvariantCulture));
			}
			Menu.SetText("total", "Cost " + Cost + " / " + Budget);
			Menu.Widget("total").Colour = Cost > Budget ? MenuColour.Red : MenuColour.White;
			Menu.SetText("seed", "Seed  " + _seed);
			Menu.Widget("begin").Enabled = Cost <= Budget;
			Describe();
		}

		public override void OnFocus() => Describe();

		private void Describe()
		{
			string f = Menu.Focused ?? "";
			if (f.StartsWith("hero"))
			{
				JobDef j = _jobs[_pick[f[4] - '0']];
				Menu.SetText("desc1", j.Name + " - costs " + j.Cost + " of the budget.  Left / Right to change.");
				Menu.SetText("desc2", Starts(j));
			}
			else if (f == "seed") { Menu.SetText("desc1", "The run's seed: the same seed, the same battles and rewards."); Menu.SetText("desc2", "Press for another."); }
			else if (f == "begin") { Menu.SetText("desc1", Cost <= Budget ? "Begin the run with this party." : "Over the budget: choose cheaper jobs."); Menu.SetText("desc2", ""); }
		}

		/// <summary>What a job starts with, in words.</summary>
		private static string Starts(JobDef j)
		{
			RogueService s = RogueService.Instance;
			GameData d = s.GameData();
			int bit = 1 << j.Job;
			var gear = d.Items.Where(i => (i.Jobs & bit) != 0 && i.Price > 0 && (i.Kind == "weapon" || i.Kind == "armour")).GroupBy(i => i.Kind).Select(g => g.OrderBy(i => i.Price).First().Name);
			var spells = j.SpellCount > 0 ? d.Spells.Where(sp => sp.Level == 1 && sp.InBattle && (sp.Jobs & bit) != 0 && sp.School != "summon").OrderBy(sp => sp.Id).Take(j.SpellCount).Select(sp => sp.Name) : Enumerable.Empty<string>();
			string all = string.Join(", ", gear.Concat(spells));
			return all.Length > 0 ? "Starts with " + all + "." : "";
		}

		public override bool OnKey(MenuKey key)
		{
			string f = Menu.Focused ?? "";
			if (!f.StartsWith("hero") || (key != MenuKey.Left && key != MenuKey.Right)) return false;
			int h = f[4] - '0';
			_pick[h] = (_pick[h] + (key == MenuKey.Right ? 1 : _jobs.Count - 1)) % _jobs.Count;
			Menu.SoundDecide();
			Show();
			return true;
		}

		public override bool OnPress()
		{
			switch (Menu.Focused)
			{
				case "seed": _seed = NewSeed(); Menu.SoundDecide(); Show(); return true;
				case "begin":
					if (Cost > Budget) { Menu.SoundBeep(); return true; }
					Menu.SoundDecide();
					RogueService.Instance.StartRun(_seed, Enumerable.Range(0, 4).Select(h => _jobs[_pick[h]].Word).ToList());
					Menu.Close();
					RogueService.Instance.After("rogue-camp");
					return true;
				default:
					// A hero's row: a press steps the job on, as Right does.
					if ((Menu.Focused ?? "").StartsWith("hero")) return OnKey(MenuKey.Right);
					return false;
			}
		}

		public override bool OnCancel() { Menu.SoundCancel(); Menu.Open("rogue"); return true; }
	}

	/// <summary>Between battles.</summary>
	public sealed class RogueCamp : MenuBehaviour
	{
		private bool _abandoning;

		public override void OnOpen()
		{
			Game.Log("rogue: screen " + Menu.Id);
			_abandoning = false;
			RogueService s = RogueService.Instance;
			RunState run = s.Run;
			if (run == null) { Menu.Open("rogue"); return; }
			ActDef act = s.Act;
			Menu.SetText("title", "Act " + (run.Act + 1) + ": " + (act?.Name ?? ""));
			string kind = run.Next?.Kind ?? "battle";
			Menu.SetText("step", kind == "boss" ? "The act's boss" : "Battle " + (run.Step + 1) + " of " + (act?.Encounters ?? 0) + (kind == "elite" ? "  -  an elite" : ""));
			Menu.SetText("next", "Next: " + (run.Next?.Name ?? "?"));
			string elite = run.Next == null || run.Next.Elite.Count == 0 ? "" : string.Join(", ", run.Next.Elite.Select(id => s.Content.Elite(id)?.Name ?? id));
			Menu.SetText("elite", elite.Length > 0 ? "Elite: " + elite : "");
			for (int h = 0; h < 4; h++)
			{
				PartyMember m = Game.Party.Member(h);
				if (m == null) { foreach (string f in new[] { "p", "job", "lv", "hp" }) Menu.SetText(f + h, ""); continue; }
				Menu.SetText("p" + h, m.Name);
				Menu.SetText("job" + h, m.JobTitle);
				Menu.SetText("lv" + h, "Lv " + m.Level);
				Menu.SetText("hp" + h, "HP " + m.Hp + " / " + m.MaxHp);
			}
			Menu.SetText("gil", "Gil " + Text.Gil(Game.Party.Gil) + "     Effects " + run.Passives.Sum(p => p.Stacks));
			Menu.SetText("abandon", "Abandon the run");
			Menu.Focus("fight");
			Describe();
		}

		public override void OnFocus() { if (Menu.Focused != "abandon" && _abandoning) { _abandoning = false; Menu.SetText("abandon", "Abandon the run"); } Describe(); }

		private void Describe()
		{
			RunState run = RogueService.Instance.Run;
			switch (Menu.Focused)
			{
				case "fight": Menu.SetText("desc1", run?.Next?.Kind == "boss" ? "Face the act's boss. Win, and the next act opens." : "Into battle. No running away."); Menu.SetText("desc2", "Win to choose a reward; a wipe ends the run."); break;
				case "effects": Menu.SetText("desc1", "The modifiers this run has gathered."); Menu.SetText("desc2", ""); break;
				case "abandon": Menu.SetText("desc1", _abandoning ? "Press again to give the run up." : "Give up this run and see its summary."); Menu.SetText("desc2", ""); break;
			}
		}

		public override bool OnPress()
		{
			switch (Menu.Focused)
			{
				case "fight": Menu.SoundDecide(); Menu.Close(); RogueService.Instance.Fight(); return true;
				case "effects": Menu.SoundDecide(); Menu.Open("rogue-effects"); return true;
				case "abandon":
					if (!_abandoning) { _abandoning = true; Menu.SetText("abandon", "Really abandon?"); Menu.SoundBeep(); Describe(); return true; }
					Menu.SoundDecide(); Menu.Close(); RogueService.Instance.Abandon(); return true;
			}
			return false;
		}

		public override bool OnCancel() { Menu.SoundBeep(); return true; }
	}

	/// <summary>A victory's choices.</summary>
	public sealed class RogueReward : MenuBehaviour
	{
		public override void OnOpen()
		{
			Game.Log("rogue: screen " + Menu.Id);
			Show();
			Menu.Focus("card0");
		}

		private void Show()
		{
			RogueService s = RogueService.Instance;
			RunState run = s.Run;
			if (run == null || run.Pending.Count == 0) { Menu.Open("rogue-camp"); return; }
			Menu.SetText("title", run.Next?.Kind == "boss" ? "The act is won!" : run.Next?.Kind == "elite" ? "The elites are beaten!" : "Victory!");
			Menu.SetText("sub", "Choose one");
			for (int i = 0; i < 3; i++)
			{
				RewardOption o = i < run.Pending.Count ? run.Pending[i] : null;
				Menu.SetText("card" + i, o?.Name ?? "");
				Menu.SetText("label" + i, o == null ? "" : o.Label);
				IMenuWidget label = Menu.Widget("label" + i);
				if (label != null && o != null) label.Colour = RarityColour(s.Content, o.Rarity);
				List<string> lines = Text.Wrap(o?.Text ?? "", 22, 4);
				for (int l = 0; l < 4; l++) Menu.SetText("text" + i + "_" + l, lines[l]);
			}
			int cost = s.RerollCost;
			Menu.SetText("reroll", "Reroll  -  " + Text.Gil(cost) + " Gil");
			Menu.Widget("reroll").Enabled = Game.Party.Gil >= cost;
			Menu.SetText("gil", "You have " + Text.Gil(Game.Party.Gil) + " Gil");
			Describe();
		}

		private static MenuColour RarityColour(RogueContent c, string rarity)
		{
			switch (c.RarityIndex(rarity))
			{
				case 0: return MenuColour.White;
				case 1: return MenuColour.PaleBlue;
				case 2: return MenuColour.Yellow;
				default: return MenuColour.Red;
			}
		}

		public override void OnFocus() => Describe();

		private void Describe()
		{
			RunState run = RogueService.Instance.Run;
			string f = Menu.Focused ?? "";
			if (f.StartsWith("card") && run != null)
			{
				int i = f[4] - '0';
				RewardOption o = i < run.Pending.Count ? run.Pending[i] : null;
				List<string> lines = Text.Wrap(o == null ? "" : o.Name + " - " + o.Text, 70, 2);
				Menu.SetText("desc1", lines[0]); Menu.SetText("desc2", lines[1]);
			}
			else if (f == "reroll") { Menu.SetText("desc1", "Three new choices for gil; each reroll here costs twice the last."); Menu.SetText("desc2", ""); }
		}

		public override bool OnPress()
		{
			RogueService s = RogueService.Instance;
			string f = Menu.Focused ?? "";
			if (f == "reroll")
			{
				if (!s.Reroll()) { Menu.SoundBeep(); return true; }
				Menu.SoundDecide(); Show(); return true;
			}
			if (f.StartsWith("card"))
			{
				int i = f[4] - '0';
				if (s.Run == null || i >= s.Run.Pending.Count) { Menu.SoundBeep(); return true; }
				Menu.SoundDecide();
				string said = s.Take(i);
				if (!string.IsNullOrEmpty(said)) Game.Screen.Notice(said);
				Menu.Close();
				return true;
			}
			return false;
		}

		public override bool OnCancel() { Menu.SoundBeep(); return true; }
	}

	/// <summary>The modifiers held.</summary>
	public sealed class RogueEffects : MenuBehaviour
	{
		private MenuList _list;

		public override void OnOpen()
		{
			Game.Log("rogue: screen " + Menu.Id);
			RogueService s = RogueService.Instance;
			RunState run = s.Run;
			Menu.SetText("title", "Active Effects");
			_list = new MenuList(Menu, "row", 6, "page") { SubPrefix = "sub" };
			List<OwnedPassive> held = run?.Passives ?? new List<OwnedPassive>();
			_list.Items = held.Select(p => { PassiveDef d = s.Content.Passive(p.Id); return d == null ? p.Id : d.Name + (d.MaxStacks > 1 ? " " + RewardGenerator.Roman(p.Stacks) : ""); }).ToList();
			_list.SubItems = held.Select(p => { PassiveDef d = s.Content.Passive(p.Id); return d == null ? "" : Modifiers.Describe(d, p.Stacks); }).ToList();
			_list.Show();
			Menu.SetText("none", held.Count == 0 ? "None yet - victories offer them." : "");
			if (held.Count > 0) Menu.Focus("row0");
		}

		public override void OnFocus() => _list?.OnFocus();
		public override bool OnKey(MenuKey key) => _list != null && _list.OnKey(key);
		public override bool OnCancel() { Menu.SoundCancel(); Menu.Open("rogue-camp"); return true; }
		public override bool OnPress() => true;
	}

	/// <summary>A run's end.</summary>
	public sealed class RogueSummary : MenuBehaviour
	{
		public override void OnOpen()
		{
			Game.Log("rogue: screen " + Menu.Id);
			RogueService s = RogueService.Instance;
			RunState r = s.Ended;
			if (r == null) { Menu.Open("rogue"); return; }
			Menu.SetText("title", r.Outcome == "won" ? "The run is won!" : r.Outcome == "abandoned" ? "The run is given up" : "The party has fallen");
			ActDef act = s.Content.Act(Math.Min(r.Act, s.Content.Run.Acts.Count - 1));
			(string, string)[] lines =
			{
				("Reached", "Act " + (r.Act + 1) + ": " + (act?.Name ?? "") + (r.Outcome == "won" ? "" : ", battle " + (r.Step + 1))),
				("Battles won", r.Tally.Battles + "   (elites " + r.Tally.Elites + ", bosses " + r.Tally.Bosses + ")"),
				("Monsters felled", r.Tally.Kills.ToString()),
				("Gil earned", Text.Gil(r.Tally.GilEarned)),
				("Rewards taken", r.Tally.Rewards + "   (rerolls " + r.Tally.Rerolls + ", rare finds " + r.Tally.RareItems + ")"),
				("Jobs", string.Join(", ", r.Jobs.Select(j => s.Content.Job(j)?.Name ?? j))),
				("Effects", r.Passives.Count == 0 ? "none" : string.Join(", ", r.Passives.Select(p => s.Content.Passive(p.Id)?.Name ?? p.Id))),
				("Seed", r.Seed.ToString()),
			};
			for (int i = 0; i < lines.Length; i++)
			{
				Menu.SetText("s" + i, lines[i].Item1);
				Menu.SetText("v" + i, lines[i].Item2.Length > 46 ? lines[i].Item2.Substring(0, 43) + "..." : lines[i].Item2);
			}
			Menu.Focus("ok");
		}

		public override bool OnPress()
		{
			if (Menu.Focused != "ok") return false;
			Menu.SoundDecide();
			Menu.Open("rogue");
			return true;
		}

		public override bool OnCancel() { Menu.Open("rogue"); return true; }
	}
}

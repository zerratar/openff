// Rogue Mode's screens, in the game's own menu system: menus/*.xml placed by tools/menus.py, the look in
// menus/styles/rogue.css over the act's backdrop (menus/images). Each screen's logic is a MenuBehaviour here:
//
//   rogue           the mode's own menu: New Run, Continue Run
//   rogue-party     a new run: each hero's starting job (left / right), the cost against the budget, the seed
//   rogue-camp      between battles: the act's road, the party, the battle ahead; Fight, Active Effects, Abandon
//   rogue-reward    a victory's three choices side by side, and a reroll for gil
//   rogue-effects   the modifiers held, in full
//   rogue-summary   a run's end: how far it went and what it did
//
// Everything they show comes from RogueService (the run) and Game.Party (the heroes). What changes with the
// run - the act's backdrop, a card's rarity, a pip on the road - is a class the sheet styles.

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
		/// <summary>Words into at most this many lines that fit a frame this wide (menu units) at this size.</summary>
		public static List<string> Wrap(string text, float width, int size, int lines)
		{
			List<string> result = new List<string>();
			string line = "";
			foreach (string word in (text ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries))
			{
				string tried = line.Length == 0 ? word : line + " " + word;
				if (line.Length > 0 && Width(tried, size) > width) { result.Add(line); line = word; }
				else line = tried;
			}
			if (line.Length > 0) result.Add(line);
			if (result.Count > lines)
			{
				result = result.Take(lines).ToList();
				string last = result[lines - 1];
				while (last.Length > 1 && Width(last + "...", size) > width) last = last.Substring(0, last.LastIndexOf(' ') > 0 ? last.LastIndexOf(' ') : last.Length - 1);
				result[lines - 1] = last.TrimEnd('.', ',', ' ') + "...";
			}
			while (result.Count < lines) result.Add("");
			return result;
		}

		/// <summary>A text's width in menu units at a size, by the client's TrueType face (a guess when it cannot say).</summary>
		public static float Width(string text, int size)
		{
			float w = 0;
			try { w = Game.Draw.MeasureText(text, size) * 480f / 800f; } catch (Exception) { }   // Game.Draw's units are the 800-wide screen's
			return w > 0 ? w : text.Length * size * 0.52f;
		}

		public static string Gil(int n) => n.ToString("N0", CultureInfo.InvariantCulture);
	}

	/// <summary>What the screens share: the act's backdrop, the help, a class picked from a set.</summary>
	internal static class Look
	{
		private static readonly string[] Backdrops = { "hub", "cave", "woods", "tower" };

		/// <summary>The backdrop of the act the run is in, or the mode's own.</summary>
		public static void Backdrop(IMenuScreen menu, RunState run)
		{
			RogueService s = RogueService.Instance;
			string look = run == null ? "hub" : s.Content.BiomeOf(s.Content.Act(Math.Min(run.Act, s.Content.Run.Acts.Count - 1)))?.Look ?? "cave";
			One(menu.Widget("bg"), Backdrops, look);
		}

		/// <summary>Of a set of classes, only this one on the frame (none for null).</summary>
		public static void One(IMenuWidget w, IEnumerable<string> set, string on)
		{
			if (w == null) return;
			foreach (string c in set) w.ToggleClass(c, c == on);
		}

		public static void Help(IMenuScreen menu, string a, string b = "")
		{
			menu.SetText("desc1", a ?? "");
			menu.SetText("desc2", b ?? "");
		}

		/// <summary>The help in two lines, broken at the help's width.</summary>
		public static void HelpWrapped(IMenuScreen menu, string text)
		{
			List<string> l = Text.Wrap(text, 440, 11, 2);
			Help(menu, l[0], l[1]);
		}

		/// <summary>A hero's face in a job, as a background image says it.</summary>
		public static string Face(int hero, int job) => "resource(\"files/pc" + (hero + 1) + "_" + (job + 1).ToString("00", CultureInfo.InvariantCulture) + ".NCGR\")";

		public static readonly string[] Icons = { "none", "weapon", "shield", "helmet", "armour", "gloves", "white", "black", "magic", "gil", "passive", "heal", "experience", "boss", "elite", "reroll" };

		/// <summary>The icon a reward is shown with: its kind of gear, its school of magic, or what it is.</summary>
		public static string IconOf(RewardOption o)
		{
			if (o == null) return "none";
			RogueService s = RogueService.Instance;
			switch (o.Kind)
			{
				case "equipment":
					string kind = s.GameData().Items.FirstOrDefault(i => i.Id.ToString(CultureInfo.InvariantCulture) == o.Ref)?.Kind;
					return kind == "weapon" || kind == "shield" || kind == "helmet" || kind == "armour" || kind == "gloves" ? kind : "weapon";
				case "spell":
					string school = s.GameData().Spells.FirstOrDefault(sp => sp.Id.ToString(CultureInfo.InvariantCulture) == o.Ref)?.School;
					return school == "white" ? "white" : school == "black" ? "black" : "magic";
				case "gil": return "gil";
				case "heal": return "heal";
				case "experience": return "experience";
				default: return "passive";
			}
		}
	}

	/// <summary>The mode's own menu.</summary>
	public sealed class RogueHub : MenuBehaviour
	{
		public override void OnOpen()
		{
			Game.Log("rogue: screen " + Menu.Id);
			RogueService s = RogueService.Instance;
			Look.Backdrop(Menu, null);
			Menu.SetText("sub", string.Join("   ·   ", s.Content.Run.Acts.Select(a => a.Name)));
			RunState saved = RogueService.SavedRun();
			Menu.SetText("runinfo", saved == null ? "" : "Saved run: act " + (saved.Act + 1) + ", battle " + (saved.Step + 1) + "   ·   seed " + saved.Seed);
			Menu.Widget("continue").Enabled = saved != null;
			Menu.Focus(saved != null ? "continue" : "new");
			Describe();
		}

		public override void OnFocus() => Describe();

		private void Describe()
		{
			switch (Menu.Focused)
			{
				case "new": Look.Help(Menu, "Four heroes, a job each within the budget, and a run of FF3's battles.", "Win to choose rewards; fall, and the run is over."); break;
				case "continue": Look.Help(Menu, RogueService.HasSavedRun ? "Pick the run up where you left it." : "No run waits to be continued.", RogueService.HasSavedRun ? "A new run in its place ends it." : ""); break;
				default: Look.Help(Menu, ""); break;
			}
		}

		public override bool OnPress()
		{
			RogueService s = RogueService.Instance;
			switch (Menu.Focused)
			{
				case "new": Menu.SoundDecide(); Menu.Open("rogue-party"); return true;
				case "continue":
					if (!RogueService.HasSavedRun || !s.Continue()) { Menu.SoundBeep(); return true; }
					Menu.SoundDecide();
					if (s.Warping) Menu.Close(); else Menu.Open(s.Run.Pending.Count > 0 ? "rogue-reward" : "rogue-camp");
					return true;
			}
			return false;
		}

		// Back: out of Rogue Mode to the title (a run in progress is saved after every step; Continue Run picks it up).
		public override bool OnCancel() { Menu.SoundCancel(); Game.Title.Return(); return true; }
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
			Look.Backdrop(Menu, null);
			_jobs = s.Content.Jobs.Jobs.Where(j => j.Starting).ToList();
			for (int h = 0; h < 4; h++) _pick[h] = Math.Max(0, _jobs.FindIndex(j => j.Word == Defaults[h]));
			_seed = NewSeed();
			Show();
			Menu.Focus("begin");
			Describe();
		}

		private static ulong NewSeed() => RogueService.TestSeed != 0 ? RogueService.TestSeed : (ulong)(DateTime.Now.Ticks % 1000000000L);

		private int Cost => Enumerable.Range(0, 4).Sum(h => _jobs[_pick[h]].Cost);
		private int Budget => RogueService.Instance.Content.Jobs.Budget;

		private void Show()
		{
			for (int h = 0; h < 4; h++)
			{
				JobDef j = _jobs[_pick[h]];
				Menu.SetText("name" + h, RogueService.HeroNames[h]);
				Menu.SetText("job" + h, j.Name);
				Menu.SetText("cost" + h, j.Cost.ToString(CultureInfo.InvariantCulture));
				Menu.Widget("face" + h)?.SetStyle("background-image", Look.Face(h, j.Job));
			}
			bool over = Cost > Budget;
			Menu.SetText("hright", "Budget  " + Cost + " / " + Budget);
			Menu.Widget("hright")?.ToggleClass("over", over);
			Menu.SetText("seed", "Seed  " + _seed);
			Menu.Widget("begin").Enabled = !over;
		}

		public override void OnFocus() => Describe();

		private void Describe()
		{
			string f = Menu.Focused ?? "";
			if (f.StartsWith("hero"))
			{
				JobDef j = _jobs[_pick[f[4] - '0']];
				Look.Help(Menu, j.Name + " costs " + j.Cost + " of the budget's " + Budget + ".   Left / Right: another job.", Starts(j));
			}
			else if (f == "seed") Look.Help(Menu, "The run's seed: the same seed, the same battles and rewards.", "Press for another.");
			else if (f == "begin") Look.Help(Menu, Cost <= Budget ? "Begin the run with this party." : "Over the budget: choose cheaper jobs.", "");
		}

		/// <summary>What a job starts with, in words.</summary>
		private static string Starts(JobDef j)
		{
			GameData d = RogueService.Instance.GameData();
			var gear = RogueService.Kit(j, d).Select(id => d.Items.FirstOrDefault(i => i.Id == id)?.Name).Where(n => n != null);
			var spells = RogueService.StartingSpells(j, d).Select(id => d.Spells.FirstOrDefault(sp => sp.Id == id)?.Name).Where(n => n != null);
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
			Describe();
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
					Menu.Open("rogue-camp");
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
		private static readonly string[] PipStates = { "off", "done", "now", "next" };
		private bool _abandoning;
		/// <summary>What the reward just taken did ("Arc learns Sleep."), said in the help as the camp opens, till the cursor moves.</summary>
		public static string News;
		private string _news;
		/// <summary>The row to come back to (the one a screen was opened from); Fight when none.</summary>
		public static string Return;

		public override void OnOpen()
		{
			Game.Log("rogue: screen " + Menu.Id);
			_abandoning = false;
			RogueService s = RogueService.Instance;
			RunState run = s.Run;
			if (run == null) { Menu.Open("rogue"); return; }
			Look.Backdrop(Menu, run);
			ActDef act = s.Act;
			string kind = run.Next?.Kind ?? "battle";
			Menu.SetText("title", "Act " + (run.Act + 1) + "  ·  " + (act?.Name ?? ""));
			Menu.SetText("hright", kind == "boss" ? "The act's boss" : "Battle " + (run.Step + 1) + " of " + (act?.Encounters ?? 0));
			Road(run, act);

			Look.One(Menu.Widget("nkind"), new[] { "elite", "boss" }, kind);
			Menu.SetText("nkind", kind == "boss" ? "BOSS" : kind == "elite" ? "ELITE BATTLE" : "NEXT BATTLE");
			string elite = run.Next == null || run.Next.Elite.Count == 0 ? "" : string.Join(", ", run.Next.Elite.Select(id => s.Content.Elite(id)?.Name ?? id));
			List<string> lines = Text.Wrap((run.Next?.Name ?? "?") + (elite.Length > 0 ? "  -  " + elite : ""), 150, 11, 2);
			Menu.SetText("next", lines[0]);
			Menu.SetText("next2", lines[1]);

			Menu.SetText("gil", Text.Gil(Game.Party.Gil));
			int fx = run.Passives.Sum(p => p.Stacks);
			Menu.SetText("fx", fx.ToString(CultureInfo.InvariantCulture));
			Menu.SetText("abandon", "Abandon the run");
			_news = News;
			News = null;
			// Back from Party or Active Effects, on the row that went there; after a battle or a reward, on Fight.
			Menu.Focus(Return ?? "fight");
			Return = null;
			Describe();
		}

		/// <summary>The act's road: a pip a battle (an elite's marked, the boss's last), the ones won lit, the next one beating.</summary>
		private void Road(RunState run, ActDef act)
		{
			int steps = (act?.Encounters ?? 0) + 1;
			for (int i = 0; i < 10; i++)
			{
				IMenuWidget pip = Menu.Widget("pip" + i);
				if (pip == null) continue;
				string kind = i < steps && act != null ? RunState.KindAt(act, i) : null;
				Look.One(pip, PipStates, i >= steps ? "off" : i < run.Step ? "done" : i == run.Step ? "now" : "next");
				pip.ToggleClass("first", i == 0);
				pip.ToggleClass("elite", kind == "elite");
				pip.ToggleClass("boss", kind == "boss");
				Look.One(Menu.Widget("pi" + i), Look.Icons, i >= steps || i < run.Step ? "none" : kind == "boss" ? "boss" : kind == "elite" ? "elite" : "none");
			}
		}

		public override void OnFocus()
		{
			if (Menu.Focused != "abandon" && _abandoning) { _abandoning = false; Menu.SetText("abandon", "Abandon the run"); }
			if (Menu.Focused != "fight") _news = null;
			Describe();
		}

		private void Describe()
		{
			RunState run = RogueService.Instance.Run;
			Menu.Widget("desc1")?.ToggleClass("news", _news != null);
			if (_news != null)
			{
				// A line of its own when it fits, else the news over both (a crystal's says more than a reward's).
				List<string> lines = Text.Wrap(_news, 440, 11, 2);
				Look.Help(Menu, lines[0], lines[1].Length > 0 ? lines[1] : run?.Next?.Kind == "boss" ? "Next: the act's boss." : "Fight on when you are ready.");
				return;
			}
			switch (Menu.Focused)
			{
				case "fight": Look.Help(Menu, run?.Next?.Kind == "boss" ? "Face the act's boss. Win, and the next act opens." : "Into battle - there is no running away.", "Win to choose a reward; a wipe ends the run."); break;
				case "party":
					Look.Help(Menu, "Each hero in full: job, stats, magic and equipment.",
						(run?.Act ?? 0) > 0 ? "Change jobs there, among those the crystals have opened." : "Job changes open with the crystal the act's boss guards.");
					break;
				case "effects": Look.Help(Menu, "The modifiers this run has gathered, in full."); break;
				case "abandon": Look.Help(Menu, _abandoning ? "Press again to give the run up." : "Give this run up and see how far it went."); break;
			}
		}

		public override bool OnPress()
		{
			switch (Menu.Focused)
			{
				case "fight": Menu.SoundDecide(); Menu.Close(); RogueService.Instance.Fight(); return true;
				case "party": Menu.SoundDecide(); RogueHeroes.Focus = 0; Return = "party"; Menu.Open("rogue-heroes"); return true;
				case "effects": Menu.SoundDecide(); Return = "effects"; Menu.Open("rogue-effects"); return true;
				case "abandon":
					if (!_abandoning) { _abandoning = true; Menu.SetText("abandon", "Really abandon?"); Menu.SoundBeep(); Describe(); return true; }
					Menu.SoundDecide(); RogueService.Instance.Abandon(); Menu.Open("rogue-summary"); return true;
			}
			return false;
		}

		public override bool OnCancel() { Menu.SoundBeep(); return true; }
	}

	/// <summary>A victory's choices.</summary>
	public sealed class RogueReward : MenuBehaviour
	{
		private static readonly string[] Rarities = { "common", "uncommon", "rare", "epic", "legendary", "empty" };

		public override void OnOpen()
		{
			Game.Log("rogue: screen " + Menu.Id);
			Show();
			Menu.Focus("card0");
			Describe();
		}

		private void Show()
		{
			RogueService s = RogueService.Instance;
			RunState run = s.Run;
			if (run == null || run.Pending.Count == 0) { Menu.Open("rogue-camp"); return; }
			Look.Backdrop(Menu, run);
			Menu.SetText("title", run.Next?.Kind == "boss" ? "The act is won!" : run.Next?.Kind == "elite" ? "The elites are beaten!" : "Victory!");
			Menu.SetText("hright", "Choose one");
			for (int i = 0; i < 3; i++)
			{
				RewardOption o = i < run.Pending.Count ? run.Pending[i] : null;
				Look.One(Menu.Widget("card" + i), Rarities, o == null ? "empty" : Rarities.Contains(o.Rarity) ? o.Rarity : "common");
				Look.One(Menu.Widget("icon" + i), Look.Icons, Look.IconOf(o));
				Menu.SetText("rarity" + i, o == null ? "" : o.Rarity);
				Menu.SetText("label" + i, o == null ? "" : o.Label);
				List<string> name = Text.Wrap(o?.Name ?? "", 120, 13, 1);
				Menu.SetText("name" + i, name[0]);
				List<string> lines = Text.Wrap(o?.Text ?? "", 120, 10, 7);
				for (int l = 0; l < 7; l++) Menu.SetText("text" + i + "_" + l, lines[l]);
			}
			int cost = s.RerollCost;
			Menu.SetText("reroll", "Reroll the choices  -  " + Text.Gil(cost) + " gil");
			Menu.Widget("reroll").Enabled = Game.Party.Gil >= cost;
			Menu.SetText("gil", Text.Gil(Game.Party.Gil) + " gil");
		}

		public override void OnFocus() => Describe();

		private void Describe()
		{
			RogueService s = RogueService.Instance;
			RunState run = s.Run;
			string f = Menu.Focused ?? "";
			if (f.StartsWith("card") && run != null)
			{
				int i = f[4] - '0';
				RewardOption o = i < run.Pending.Count ? run.Pending[i] : null;
				Look.HelpWrapped(Menu, o == null ? "" : o.Name + " (" + RewardGenerator.Title(o.Rarity) + " " + (o.Label ?? "").ToLowerInvariant() + ") - press to take it.");
			}
			else if (f == "reroll") Look.Help(Menu, Game.Party.Gil >= s.RerollCost ? "Three new choices for gil." : "Not enough gil for a reroll.", "Each reroll after a battle costs twice the last.");
		}

		public override bool OnPress()
		{
			RogueService s = RogueService.Instance;
			string f = Menu.Focused ?? "";
			if (f == "reroll")
			{
				if (!s.Reroll()) { Menu.SoundBeep(); return true; }
				Menu.SoundDecide(); Show(); Describe(); return true;
			}
			if (f.StartsWith("card"))
			{
				int i = f[4] - '0';
				if (s.Run == null || i >= s.Run.Pending.Count) { Menu.SoundBeep(); return true; }
				Menu.SoundDecide();
				string said = s.Take(i);
				// On to the camp in the menus, which says what came of it; after a boss, out to the field for the warp to the next act.
				if (s.Warping || s.Run == null) { if (!string.IsNullOrEmpty(said)) Game.Screen.Notice(said); Menu.Close(); }
				else { RogueCamp.News = said; Menu.Open("rogue-camp"); }
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
			Look.Backdrop(Menu, run);
			List<OwnedPassive> held = run?.Passives ?? new List<OwnedPassive>();
			Menu.SetText("hright", held.Count == 0 ? "" : held.Sum(p => p.Stacks) + " held");
			_list = new MenuList(Menu, "row", 6, "page") { SubPrefix = "sub" };
			_list.Items = held.Select(p => { PassiveDef d = s.Content.Passive(p.Id); return d == null ? p.Id : d.Name + (d.MaxStacks > 1 ? " " + RewardGenerator.Roman(p.Stacks) : ""); }).ToList();
			_list.SubItems = held.Select(p => { PassiveDef d = s.Content.Passive(p.Id); return d == null ? "" : Text.Wrap(Modifiers.Describe(d, p.Stacks), 370, 11, 1)[0]; }).ToList();
			_list.Show();
			Stars();
			// Nothing held: the first row says so (the hand has to stand somewhere).
			Menu.Widget("row0")?.ToggleClass("vacant", held.Count == 0);
			if (held.Count == 0) Menu.SetText("row0", "None yet - victories offer them.");
			Menu.Focus("row0");
			Describe();
		}

		/// <summary>A star by each row that holds an effect.</summary>
		private void Stars()
		{
			for (int r = 0; r < 6; r++)
				Look.One(Menu.Widget("star" + r), Look.Icons, _list.Top + r < _list.Items.Count ? "passive" : "none");
		}

		private void Describe()
		{
			RogueService s = RogueService.Instance;
			List<OwnedPassive> held = s.Run?.Passives ?? new List<OwnedPassive>();
			int at = _list == null ? -1 : _list.IndexAt(Menu.Focused ?? "");
			if (at < 0 || at >= held.Count) { Look.Help(Menu, held.Count == 0 ? "Win battles to be offered effects; they last the whole run." : "", "Back: to the camp."); return; }
			PassiveDef d = s.Content.Passive(held[at].Id);
			Look.HelpWrapped(Menu, d == null ? held[at].Id : Modifiers.Describe(d, held[at].Stacks) + (d.MaxStacks > 1 ? "  (" + held[at].Stacks + " of " + d.MaxStacks + ")" : ""));
		}

		public override void OnFocus() { _list?.OnFocus(); Stars(); Describe(); }
		public override bool OnKey(MenuKey key) { bool done = _list != null && _list.OnKey(key); Stars(); Describe(); return done; }
		public override bool OnCancel() { Menu.SoundCancel(); Menu.Open("rogue-camp"); return true; }
		public override bool OnPress() => true;
	}

	/// <summary>The party in full: a hero picked on the left, everything about them on the right; A for a job change.</summary>
	public sealed class RogueHeroes : MenuBehaviour
	{
		/// <summary>The hero to start on (after a job change, the one who changed); what to say first, if anything.</summary>
		public static int Focus;
		public static string News;
		private static readonly string[] Labels = { "Strength", "Agility", "Vitality", "Intellect", "Mind", "Attack", "Defense", "M. Def." };
		private string _news;

		public override void OnOpen()
		{
			Game.Log("rogue: screen " + Menu.Id);
			Look.Backdrop(Menu, RogueService.Instance.Run);
			Menu.SetText("hright", "A: change job");
			Menu.Focus("hero" + Math.Clamp(Focus, 0, 3));
			// After the focus is set (OnFocus clears the news): what the job change did, till the cursor moves.
			_news = News;
			News = null;
			Show();
		}

		private int Hero => (Menu.Focused ?? "").StartsWith("hero") ? Menu.Focused[4] - '0' : 0;

		private void Show()
		{
			int h = Hero;
			PartyMember m = Game.Party.Member(h);
			if (m == null) return;
			Menu.Widget("dface")?.SetStyle("background-image", m.Face ?? Look.Face(h, m.Job));
			Menu.SetText("dname", m.Name);
			Menu.SetText("djob", m.JobTitle + "   ·   job level " + m.JobSkill);
			Menu.SetText("dlv", "Lv " + m.Level);
			Menu.SetText("dnext", m.ExpToNext > 0 ? "next level " + Text.Gil(m.ExpToNext) : "");
			Menu.Widget("dexpfill")?.SetStyle("width", m.ExpPercent + "%");
			Menu.SetText("dhpv", m.Hp + " / " + m.MaxHp);
			Menu.Widget("dhpfill")?.SetStyle("width", m.HpPercent + "%");
			Menu.Widget("dhpfill")?.ToggleClass("low", m.HpPercent < 30);
			List<string> charges = new List<string>();
			for (int l = 0; m.MaxCharges != null && l < 8 && l < m.MaxCharges.Length; l++)
				if (m.MaxCharges[l] > 0) charges.Add("L" + (l + 1) + " " + m.Charges[l] + "/" + m.MaxCharges[l]);
			Menu.SetText("dmp", charges.Count == 0 ? "No magic" : "Magic   " + string.Join("    ", charges));
			Stats st = m.Stats ?? new Stats();
			int[] values = { st.Strength, st.Agility, st.Vitality, st.Intellect, st.Mind, st.Attack, st.Defense, st.MagicDefense };
			for (int i = 0; i < 8; i++) { Menu.SetText("sl" + i, Labels[i]); Menu.SetText("sv" + i, values[i].ToString(CultureInfo.InvariantCulture)); }
			GameData d = RogueService.Instance.GameData();
			for (int s = 0; s < 5; s++)
			{
				int id = Game.Party.Equipped(h, (EquipSlot)s);
				ItemInfo item = id > 0 ? d.Items.FirstOrDefault(i => i.Id == id) : null;
				Menu.SetText("ev" + s, item?.Name ?? "-");
				Menu.Widget("ev" + s)?.ToggleClass("dim", item == null);
				string kind = item?.Kind;
				Look.One(Menu.Widget("ei" + s), Look.Icons, kind == "weapon" || kind == "shield" || kind == "helmet" || kind == "armour" || kind == "gloves" ? kind : "none");
			}
			Describe(m);
		}

		private void Describe(PartyMember m)
		{
			if (_news != null) { List<string> l = Text.Wrap(_news, 440, 11, 2); Look.Help(Menu, l[0], l[1]); return; }
			bool jobs = Game.Party.OpenJobs(m.Id).Count > 1;
			Look.Help(Menu, m.Name + ", " + m.JobTitle + ".   Up / Down: another hero.",
				jobs ? "A: change " + m.Name + "'s job." : "Job changes open with the crystal the act's boss guards.");
		}

		public override void OnFocus() { _news = null; Show(); }

		public override bool OnPress()
		{
			int h = Hero;
			if (Game.Party.OpenJobs(h).Count <= 1) { Menu.SoundBeep(); return true; }
			Menu.SoundDecide();
			RogueJob.Hero = h;
			Menu.Open("rogue-job");
			return true;
		}

		public override bool OnCancel() { Menu.SoundCancel(); Menu.Open("rogue-camp"); return true; }
	}

	/// <summary>A hero's job, among those the crystals have opened; the game's own job change scene plays over it.</summary>
	public sealed class RogueJob : MenuBehaviour
	{
		public static int Hero;
		private List<string> _words = new List<string>();
		private bool _changing;

		public override void OnOpen()
		{
			Game.Log("rogue: screen " + Menu.Id);
			Look.Backdrop(Menu, RogueService.Instance.Run);
			PartyMember m = Game.Party.Member(Hero);
			Menu.SetText("hright", m?.Name ?? "");
			Menu.Widget("jface")?.SetStyle("background-image", m?.Face ?? Look.Face(Hero, m?.Job ?? 0));
			Menu.SetText("jwho", m == null ? "" : m.Name + "   ·   Lv " + m.Level);
			Menu.SetText("jnow", m == null ? "" : "Now a " + m.JobTitle + ", job level " + m.JobSkill);
			_words = Game.Party.OpenJobs(Hero).Take(16).ToList();
			int held = 0;
			for (int i = 0; i < 16; i++)
			{
				string word = i < _words.Count ? _words[i] : null;
				JobInfo info = word == null ? null : Game.Party.JobInfo(Hero, word);
				Menu.SetText("j" + i, info?.Title ?? "");
				Menu.Widget("j" + i)?.ToggleClass("blank", info == null);
				Menu.Widget("j" + i)?.ToggleClass("held", info != null && info.Held);
				if (info != null && info.Held) held = i;
			}
			Menu.Focus("j" + held);
			Describe();
		}

		private int At => (Menu.Focused ?? "").StartsWith("j") && int.TryParse(Menu.Focused.Substring(1), out int i) ? i : -1;

		public override void OnFocus()
		{
			// A blank cell is passed over: back to the last job there is.
			if (At >= _words.Count && _words.Count > 0) { Menu.Focus("j" + (_words.Count - 1)); return; }
			Describe();
		}

		private void Describe()
		{
			int i = At;
			PartyMember m = Game.Party.Member(Hero);
			JobInfo info = i >= 0 && i < _words.Count ? Game.Party.JobInfo(Hero, _words[i]) : null;
			if (info == null || m == null) { Look.Help(Menu, ""); return; }
			if (info.Held) Look.Help(Menu, m.Name + " is a " + info.Title + " now.", "Back: to the party.");
			else Look.Help(Menu, "A: " + m.Name + " becomes a " + info.Title + ".", "Gear a " + info.Title + " cannot wear goes to the bag.");
		}

		public override bool OnPress()
		{
			int i = At;
			if (_changing || i < 0 || i >= _words.Count) { Menu.SoundBeep(); return true; }
			JobInfo info = Game.Party.JobInfo(Hero, _words[i]);
			if (info == null || info.Held) { Menu.SoundBeep(); return true; }
			int hero = Hero;
			_changing = true;
			// The game's own job change scene, the job taken at its flash; then the gear the job cannot wear off, the run saved.
			bool started = Game.Party.ChangeJob(hero, _words[i], ok =>
			{
				_changing = false;
				RogueHeroes.Focus = hero;
				RogueHeroes.News = ok ? RogueService.Instance.AfterJobChange(hero) : null;
				Menu.Open("rogue-heroes");
			});
			if (!started) { _changing = false; Menu.SoundBeep(); }
			return true;
		}

		public override bool OnCancel() { if (_changing) return true; Menu.SoundCancel(); RogueHeroes.Focus = Hero; Menu.Open("rogue-heroes"); return true; }
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
			Look.Backdrop(Menu, r);
			Menu.SetText("title", r.Outcome == "won" ? "The run is won!" : r.Outcome == "abandoned" ? "The run is given up" : "The party has fallen");
			Look.One(Menu.Widget("title"), new[] { "lost", "given" }, r.Outcome == "lost" ? "lost" : r.Outcome == "abandoned" ? "given" : null);
			ActDef act = s.Content.Act(Math.Min(r.Act, s.Content.Run.Acts.Count - 1));
			Menu.SetText("reached", r.Outcome == "won" ? "All " + s.Content.Run.Acts.Count + " acts cleared   ·   seed " + r.Seed
				: "Act " + (r.Act + 1) + " · " + (act?.Name ?? "") + ", battle " + (r.Step + 1) + "   ·   seed " + r.Seed);
			(string, string)[] tiles =
			{
				("Battles won", r.Tally.Battles.ToString(CultureInfo.InvariantCulture)),
				("Elites  ·  bosses", r.Tally.Elites + "  ·  " + r.Tally.Bosses),
				("Monsters felled", r.Tally.Kills.ToString(CultureInfo.InvariantCulture)),
				("Gil earned", Text.Gil(r.Tally.GilEarned)),
				("Rewards  ·  rerolls", r.Tally.Rewards + "  ·  " + r.Tally.Rerolls),
				("Rare finds", r.Tally.RareItems.ToString(CultureInfo.InvariantCulture)),
			};
			for (int i = 0; i < tiles.Length; i++) { Menu.SetText("tl" + i, tiles[i].Item1); Menu.SetText("tv" + i, tiles[i].Item2); }
			for (int h = 0; h < 4; h++)
			{
				JobDef j = h < r.Jobs.Count ? s.Content.Job(r.Jobs[h]) : null;
				Menu.SetText("sjob" + h, j?.Name ?? "");
				if (j != null) Menu.Widget("sface" + h)?.SetStyle("background-image", Look.Face(h, j.Job));
			}
			string fx = r.Passives.Count == 0 ? "No effects gathered." : "Effects: " + string.Join(", ", r.Passives.Select(p => { PassiveDef d = s.Content.Passive(p.Id); return (d?.Name ?? p.Id) + (d != null && d.MaxStacks > 1 ? " " + RewardGenerator.Roman(p.Stacks) : ""); })) + ".";
			List<string> lines = Text.Wrap(fx, 444, 11, 2);
			Menu.SetText("fx1", lines[0]);
			Menu.SetText("fx2", lines[1]);
			Menu.Focus("ok");
			Look.Help(Menu, r.Outcome == "won" ? "Every act cleared. Another run, another seed?" : "Every run is a seed: the same seed plays the same battles again.", "");
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

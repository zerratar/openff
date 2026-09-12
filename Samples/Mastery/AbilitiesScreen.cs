// The Mastery sample's Abilities screen, in the game's own menu system.
//
// Two screens of the mod's own (menus/abilities.json, menus/ability-pick.json), their
// layouts drawn in Crystal's Menus tab, their logic here: MenuBehaviours attached to the
// screens. AbilitiesScreen fills the hero's commands, innate passives, stat modifiers and
// every job's ladder standing, and acts on a press - a free slot opens the pick, a job is
// taken. AbilityPick lists what the hero has learned and sets the chosen one. Everything
// they show comes from Game.Party (PartyMember, JobInfo); nothing here reads the game's
// own tables. Copy the folder into mods/ - or open it as a project in Crystal, Build, Run.

using System;
using System.Collections.Generic;
using System.Linq;
using OpenFF;

namespace Mastery
{
	/// <summary>The Abilities screen: the hero picked, their commands and free slots, their jobs.</summary>
	public sealed class AbilitiesScreen : MenuBehaviour
	{
		/// <summary>The hero the screens are about: picked on this screen, remembered for the pick.</summary>
		public static int Hero = -1;
		/// <summary>The free slot the pick fills, 0-based.</summary>
		public static int Slot;
		private static int _page;
		private const int JobRows = 8;

		private List<string> _jobs = new List<string>();

		public override void OnOpen()
		{
			if (Menu.Hero >= 0) Hero = Menu.Hero;
			Fill();
		}

		private void Fill()
		{
			PartyMember m = Game.Party.Member(Hero);
			if (m == null) { Menu.SetText("hero", "No hero"); return; }
			Menu.SetText("hero", m.Name + " - " + m.JobTitle + (m.Progression == "mastery" ? "  Lv " + (Game.Party.JobInfo(Hero, m.JobWord)?.Level ?? 0) : ""));
			if (m.Progression != "mastery")
			{
				Menu.SetText("commands_title", m.Name + " grows by " + m.Progression + ", not by abilities");
				for (int i = 0; i < 4; i++) { Menu.SetText("cmd" + i, ""); }
				Menu.SetText("innate", ""); Menu.SetText("stats", "");
			}
			else
			{
				Menu.SetText("commands_title", "Commands as " + m.JobTitle);
				int free = 0;
				for (int i = 0; i < 4; i++)
				{
					AbilityInfo c = i < m.Commands.Count ? m.Commands[i] : null;
					IMenuWidget w = Menu.Widget("cmd" + i);
					if (c == null) { Menu.SetText("cmd" + i, ""); continue; }
					if (c.Id >= 0)
					{
						Menu.SetText("cmd" + i, Shown(c));
						if (w != null) w.Colour = MenuColour.Disabled;   // the job's own: greyed, not for setting
					}
					else
					{
						int set = free < m.Slots.Length ? m.Slots[free] : 0;
						AbilityInfo ability = set > 0 ? m.Learned.FirstOrDefault(a => a.Id == set) : null;
						Menu.SetText("cmd" + i, "Free slot " + (free + 1) + ": " + (ability != null ? Shown(ability) : "- none -"));
						if (w != null) w.Colour = MenuColour.White;
						free++;
					}
				}
				List<AbilityInfo> innate = m.Abilities.Where(a => a.Passive && !m.Slots.Contains(a.Id)).ToList();
				Menu.SetText("innate", "Innate: " + (innate.Count > 0 ? string.Join(", ", innate.Select(a => a.Name)) : "-"));
				Menu.SetText("stats", "Learned: " + m.Learned.Count + "   Str " + m.Stats.Strength + "  Agi " + m.Stats.Agility + "  Vit " + m.Stats.Vitality + "  Int " + m.Stats.Intellect + "  Mnd " + m.Stats.Mind);
			}
			// The jobs: every one in play, a page at a time; the held one marked, closed ones greyed.
			_jobs = Game.Party.AllJobs.ToList();
			int pages = Math.Max(1, (_jobs.Count + JobRows - 1) / JobRows);
			_page = Math.Clamp(_page, 0, pages - 1);
			Menu.SetText("jobs_title", "Jobs  " + (_page + 1) + " / " + pages);
			for (int i = 0; i < JobRows; i++)
			{
				int index = _page * JobRows + i;
				IMenuWidget w = Menu.Widget("job" + i);
				if (index >= _jobs.Count) { Menu.SetText("job" + i, ""); continue; }
				JobInfo job = Game.Party.JobInfo(Hero, _jobs[index]);
				if (job == null) { Menu.SetText("job" + i, _jobs[index]); continue; }
				string standing = !job.HasLadder ? "" : job.Mastered ? "  mastered" : job.Steps == 0 ? "" : "  Lv " + job.Level + "  " + job.Abp + "/" + job.AbpToNext;
				Menu.SetText("job" + i, (job.Held ? "> " : "  ") + job.Title + standing + (job.Open ? "" : "  (closed)"));
				if (w != null) w.Colour = job.Held ? MenuColour.Yellow : job.Open ? MenuColour.White : MenuColour.Disabled;
			}
			Menu.SetText("page", pages > 1 ? "L / R: more jobs" : "");
			Menu.SetText("hint", "A: set a free slot, take a job     B: back");
		}

		private static string Shown(AbilityInfo a) => (a.Passive ? "" : "!") + a.Name;

		public override bool OnPress()
		{
			string id = Menu.Focused ?? "";
			PartyMember m = Game.Party.Member(Hero);
			if (m == null || m.Progression != "mastery") { Menu.SoundBeep(); return true; }
			if (id.StartsWith("cmd"))
			{
				int i = id[3] - '0';
				AbilityInfo c = i < m.Commands.Count ? m.Commands[i] : null;
				if (c == null || c.Id >= 0) { Menu.SoundBeep(); return true; }   // the job's own command is not for setting
				Slot = m.Commands.Take(i).Count(x => x.Id < 0);
				Menu.SoundDecide();
				Menu.Open("ability-pick");
				return true;
			}
			if (id.StartsWith("job"))
			{
				int index = _page * JobRows + (id[3] - '0');
				if (index >= _jobs.Count) { Menu.SoundBeep(); return true; }
				JobInfo job = Game.Party.JobInfo(Hero, _jobs[index]);
				if (job == null || job.Held || !job.Open || !Game.Party.ChangeJob(Hero, job.Word)) { Menu.SoundBeep(); return true; }
				Menu.SoundDecide();
				Fill();
				return true;
			}
			return false;
		}

		public override bool OnKey(MenuKey key)
		{
			if (key != MenuKey.L && key != MenuKey.R) return false;
			int pages = Math.Max(1, (_jobs.Count + JobRows - 1) / JobRows);
			_page = (_page + (key == MenuKey.R ? 1 : pages - 1)) % pages;
			Fill();
			return true;
		}
	}

	/// <summary>The pick for a free slot: what the hero has learned, ten a page; a press sets it and goes back.</summary>
	public sealed class AbilityPick : MenuBehaviour
	{
		private const int Rows = 10;
		private static int _page;
		private List<AbilityInfo> _options = new List<AbilityInfo>();

		public override void OnOpen()
		{
			Fill();
		}

		private void Fill()
		{
			PartyMember m = Game.Party.Member(AbilitiesScreen.Hero);
			_options = new List<AbilityInfo> { new AbilityInfo { Id = 0, Name = "- none -" } };
			if (m != null)
			{
				_options.AddRange(m.Learned.Where(a => !a.Passive));
				_options.AddRange(m.Learned.Where(a => a.Passive));
			}
			int pages = Math.Max(1, (_options.Count + Rows - 1) / Rows);
			_page = Math.Clamp(_page, 0, pages - 1);
			Menu.SetText("title", (m?.Name ?? "?") + " - free slot " + (AbilitiesScreen.Slot + 1) + "   (" + (_options.Count - 1) + " learned)");
			for (int i = 0; i < Rows; i++)
			{
				int index = _page * Rows + i;
				if (index >= _options.Count) { Menu.SetText("opt" + i, ""); continue; }
				AbilityInfo a = _options[index];
				bool set = m != null && a.Id > 0 && m.Slots.Contains(a.Id);
				Menu.SetText("opt" + i, a.Id == 0 ? a.Name : (a.Passive ? "" : "!") + a.Name + (a.Passive ? "   passive" : "   command") + (set ? "   (set)" : ""));
			}
			Menu.SetText("page", pages > 1 ? "L / R: more   " + (_page + 1) + " / " + pages : "");
		}

		public override bool OnPress()
		{
			string id = Menu.Focused ?? "";
			if (!id.StartsWith("opt")) return false;
			int index = _page * Rows + (id[3] - '0');
			if (index >= _options.Count) { Menu.SoundBeep(); return true; }
			if (!Game.Party.SetAbility(AbilitiesScreen.Hero, AbilitiesScreen.Slot, _options[index].Id)) { Menu.SoundBeep(); return true; }
			Menu.SoundDecide();
			Menu.Open("abilities");
			return true;
		}

		public override bool OnCancel()
		{
			Menu.SoundCancel();
			Menu.Open("abilities");
			return true;
		}

		public override bool OnKey(MenuKey key)
		{
			if (key != MenuKey.L && key != MenuKey.R) return false;
			int pages = Math.Max(1, (_options.Count + Rows - 1) / Rows);
			_page = (_page + (key == MenuKey.R ? 1 : pages - 1)) % pages;
			Fill();
			return true;
		}
	}
}

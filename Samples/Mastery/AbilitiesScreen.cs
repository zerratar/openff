// The Mastery sample's Abilities and Jobs screens, in the game's own menu system, after FF5's.
//
// Three screens of the mod's own (menus/abilities, menus/jobs, menus/job-confirm), their
// layouts drawn in Crystal's Menus tab - frames with <window/> are the game's window art -
// and their logic here: MenuBehaviours attached to the screens.
//
//   Abilities  the hero's header; Job Commands (the job's own) and the free slots on the
//              left; everything learned on the right, two columns, a page at a time (L / R);
//              a description window. Pick a slot, then an ability (or Remove); B steps back.
//   Jobs       every job in play in a grid with its ladder's standing; a press asks.
//   Change?    Yes takes the job, No returns.
//
// Everything shown comes from Game.Party (PartyMember, JobInfo); nothing here reads the
// game's own tables. Copy the folder into mods/ - or open it as a project in Crystal, Build, Run.

using System;
using System.Collections.Generic;
using System.Linq;
using OpenFF;

namespace Mastery
{
	/// <summary>What the three screens share: the hero they are about, the header, the description window.</summary>
	internal static class Screens
	{
		/// <summary>The hero: picked on Abilities or Jobs (the game's character pick), remembered for the screens after.</summary>
		public static int Hero = -1;

		public static void Header(IMenuScreen menu)
		{
			PartyMember m = Game.Party.Member(Hero);
			if (m == null) { menu.SetText("hero", "No hero"); return; }
			JobInfo job = Game.Party.JobInfo(Hero, m.JobWord);
			menu.SetText("hero", m.Name + "   Lv. " + m.Level);
			menu.SetText("job", m.JobTitle + (job != null && job.HasLadder ? "   Lv. " + job.Level + (job.Mastered ? "   Mastered!" : "") : ""));
			menu.SetText("abp_label", m.Progression == "mastery" ? "ABP" : "");
			Colour(menu, "abp_label", MenuColour.PaleBlue);   // a colour goes on after the text: setting the text draws it afresh
			menu.SetText("abp", m.Progression != "mastery" ? m.Name + " grows by " + m.Progression : job == null || !job.HasLadder ? "-" : job.Mastered ? "mastered" : job.Abp + " / " + job.AbpToNext);
		}

		public static void Describe(IMenuScreen menu, string line1, string line2)
		{
			menu.SetText("desc1", line1 ?? "");
			menu.SetText("desc2", line2 ?? "");
		}

		public static void Colour(IMenuScreen menu, string id, MenuColour colour)
		{
			IMenuWidget w = menu.Widget(id);
			if (w != null) w.Colour = colour;
		}

		public static string Shown(AbilityInfo a) => a == null ? "" : (a.Passive ? "" : "!") + a.Name;

		/// <summary>Where an ability is learned, for the description: "Knight: Lv. 2" (the first ladder that teaches it).</summary>
		public static string Source(AbilityInfo a) => a?.TaughtBy.FirstOrDefault();
	}

	/// <summary>The Abilities screen.</summary>
	public sealed class AbilitiesScreen : MenuBehaviour
	{
		private const int Rows = 7, Columns = 2, PerPage = Rows * Columns;
		private static int _page;
		private int _picking = -1;                       // the free slot being filled, or -1
		private List<AbilityInfo> _options = new List<AbilityInfo>();
		private List<int> _slotIndex = new List<int>();  // slot row -> index into PartyMember.Slots
		private int _slots;                              // slot rows in use

		public override void OnOpen()
		{
			if (Menu.Hero >= 0) Screens.Hero = Menu.Hero;
			_picking = -1;
			Fill();
			Menu.Focus("slot0");
		}

		private void Fill()
		{
			Screens.Header(Menu);
			PartyMember m = Game.Party.Member(Screens.Hero);
			for (int i = 0; i < 3; i++) { Menu.SetText("cmd" + i, ""); Menu.SetText("slot" + i, ""); }
			_slotIndex.Clear();
			if (m == null || m.Progression != "mastery")
			{
				Menu.SetText("cmd_title", m == null ? "" : m.Name + " grows by " + m.Progression);
				Menu.SetText("slot_title", "");
				Screens.Describe(Menu, m == null ? "" : "Only a hero on the mastery progression sets abilities.", "");
			}
			else
			{
				Menu.SetText("cmd_title", "Job Commands");
				Menu.SetText("slot_title", "Abilities");
				Screens.Colour(Menu, "cmd_title", MenuColour.PaleBlue);
				Screens.Colour(Menu, "slot_title", MenuColour.PaleBlue);
				int row = 0, free = 0;
				foreach (AbilityInfo c in m.Commands)
				{
					if (c.Id >= 0) { if (row < 3) Menu.SetText("cmd" + row++, Screens.Shown(c)); }
					else
					{
						if (free < 3)
						{
							int set = free < m.Slots.Length ? m.Slots[free] : 0;
							AbilityInfo a = set > 0 ? m.Learned.FirstOrDefault(x => x.Id == set) : null;
							Menu.SetText("slot" + free, a != null ? Screens.Shown(a) : "None");
							_slotIndex.Add(free);
						}
						free++;
					}
				}
				_slots = Math.Min(3, free);
			}
			// The learned list: Remove first, then commands, then passives.
			_options = new List<AbilityInfo> { new AbilityInfo { Id = 0, Name = "Remove" } };
			if (m != null) { _options.AddRange(m.Learned.Where(a => !a.Passive)); _options.AddRange(m.Learned.Where(a => a.Passive)); }
			int pages = Math.Max(1, (_options.Count + PerPage - 1) / PerPage);
			_page = Math.Clamp(_page, 0, pages - 1);
			for (int i = 0; i < PerPage; i++)
			{
				int index = _page * PerPage + i;
				AbilityInfo a = index < _options.Count ? _options[index] : null;
				Menu.SetText("opt" + i, a == null ? "" : a.Id == 0 ? "  " + a.Name : Screens.Shown(a));
				IMenuWidget w = Menu.Widget("opt" + i);
				if (w != null) w.Colour = a != null && a.Id > 0 && m != null && m.Slots.Contains(a.Id) ? MenuColour.Yellow : MenuColour.White;
			}
			Menu.SetText("page", pages > 1 ? "Page " + (_page + 1) + " / " + pages + "   L / R (Q / E)" : "");
			DescribeFocused();
		}

		private void DescribeFocused()
		{
			string id = Menu.Focused ?? "";
			PartyMember m = Game.Party.Member(Screens.Hero);
			if (m == null) return;
			if (id.StartsWith("slot"))
			{
				int row = id[4] - '0';
				int set = row < _slotIndex.Count && _slotIndex[row] < m.Slots.Length ? m.Slots[_slotIndex[row]] : 0;
				AbilityInfo a = set > 0 ? m.Learned.FirstOrDefault(x => x.Id == set) : null;
				if (a == null) Screens.Describe(Menu, "Free slot " + (row + 1), _picking >= 0 ? "Choose an ability on the right." : "A: choose an ability learned from any job to set here.");
				else Describe(a);
			}
			else if (id.StartsWith("opt"))
			{
				int index = _page * PerPage + int.Parse(id.Substring(3));
				AbilityInfo a = index < _options.Count ? _options[index] : null;
				if (a == null) Screens.Describe(Menu, "", "");
				else if (a.Id == 0) Screens.Describe(Menu, "Remove", "Empties the slot; it behaves as Guard.");
				else Describe(a);
			}
		}

		private void Describe(AbilityInfo a)
		{
			string from = Screens.Source(a);
			Screens.Describe(Menu, (from != null ? from + " Abilities  -  " : "") + Screens.Shown(a),
				a.Passive ? "A support ability: it works while set here or innate to the job held." : "A command: set here, it is in the battle's window in the slot's place.");
		}

		/// <summary>The cursor stays on rows that mean something: an unused slot row or an empty list entry sends it back.</summary>
		public override void OnFocus()
		{
			string id = Menu.Focused ?? "";
			if (id.StartsWith("slot") && id[4] - '0' >= _slots) { Menu.Focus("slot" + Math.Max(0, _slots - 1)); return; }
			if (id.StartsWith("opt"))
			{
				int last = Math.Min(PerPage, _options.Count - _page * PerPage) - 1;
				if (int.Parse(id.Substring(3)) > last) { Menu.Focus("opt" + Math.Max(0, last)); return; }
			}
			DescribeFocused();
		}

		public override bool OnPress()
		{
			string id = Menu.Focused ?? "";
			PartyMember m = Game.Party.Member(Screens.Hero);
			if (m == null || m.Progression != "mastery") { Menu.SoundBeep(); return true; }
			if (id.StartsWith("slot"))
			{
				int row = id[4] - '0';
				if (row >= _slotIndex.Count) { Menu.SoundBeep(); return true; }
				_picking = row;
				Menu.SoundDecide();
				Menu.Focus("opt0");
				DescribeFocused();
				return true;
			}
			if (id.StartsWith("opt"))
			{
				int index = _page * PerPage + int.Parse(id.Substring(3));
				if (_picking < 0 || index >= _options.Count) { Menu.SoundBeep(); return true; }
				if (!Game.Party.SetAbility(Screens.Hero, _slotIndex[_picking], _options[index].Id)) { Menu.SoundBeep(); return true; }
				Menu.SoundDecide();
				string back = "slot" + _picking;
				_picking = -1;
				Fill();
				Menu.Focus(back);
				DescribeFocused();
				return true;
			}
			return false;
		}

		public override bool OnCancel()
		{
			if (_picking < 0) return false;
			Menu.SoundCancel();
			Menu.Focus("slot" + _picking);
			_picking = -1;
			DescribeFocused();
			return true;
		}

		public override bool OnKey(MenuKey key)
		{
			if (key != MenuKey.L && key != MenuKey.R) return false;
			int pages = Math.Max(1, (_options.Count + PerPage - 1) / PerPage);
			if (pages < 2) return true;
			_page = (_page + (key == MenuKey.R ? 1 : pages - 1)) % pages;
			Fill();
			return true;
		}
	}

	/// <summary>The Jobs screen: a grid of every job in play; a press asks to change.</summary>
	public sealed class JobsScreen : MenuBehaviour
	{
		private const int Columns = 4, Rows = 5, PerPage = Columns * Rows;
		private static int _page;
		private List<string> _jobs = new List<string>();

		public override void OnOpen()
		{
			if (Menu.Hero >= 0) Screens.Hero = Menu.Hero;
			Fill();
			Menu.Focus("job0");
			DescribeFocused();
		}

		private void Fill()
		{
			Screens.Header(Menu);
			_jobs = Game.Party.AllJobs.ToList();
			int pages = Math.Max(1, (_jobs.Count + PerPage - 1) / PerPage);
			_page = Math.Clamp(_page, 0, pages - 1);
			for (int i = 0; i < PerPage; i++)
			{
				int index = _page * PerPage + i;
				JobInfo job = index < _jobs.Count ? Game.Party.JobInfo(Screens.Hero, _jobs[index]) : null;
				Menu.SetText("job" + i, job == null ? "" : job.Title);
				Menu.SetText("lv" + i, job == null ? "" : !job.HasLadder ? "" : job.Mastered ? "Mastered!" : "Lv. " + job.Level + "  " + job.Abp + " / " + job.AbpToNext);
				IMenuWidget w = Menu.Widget("job" + i);
				if (w != null) w.Colour = job == null ? MenuColour.White : job.Held ? MenuColour.Yellow : job.Open ? MenuColour.White : MenuColour.Disabled;
				IMenuWidget l = Menu.Widget("lv" + i);
				if (l != null) l.Colour = job != null && job.Open ? MenuColour.PaleBlue : MenuColour.Disabled;
				if (w != null && job != null && job.Held) w.Colour = MenuColour.Yellow;
			}
			Menu.SetText("page", pages > 1 ? "Page " + (_page + 1) + " / " + pages + "   L / R (Q / E)" : "");
		}

		private JobInfo Focused()
		{
			string id = Menu.Focused ?? "";
			if (!id.StartsWith("job")) return null;
			int index = _page * PerPage + int.Parse(id.Substring(3));
			return index < _jobs.Count ? Game.Party.JobInfo(Screens.Hero, _jobs[index]) : null;
		}

		private void DescribeFocused()
		{
			JobInfo job = Focused();
			if (job == null) { Screens.Describe(Menu, "", ""); return; }
			string standing = !job.HasLadder ? "no ladder" : job.Mastered ? "Mastered!" : "Lv. " + job.Level + "   " + job.Abp + " / " + job.AbpToNext + " ABP" + (job.Next != null ? "   next: " + Screens.Shown(job.Next) : "");
			Screens.Describe(Menu, job.Title + (job.Own ? "  (a job of this mod's)" : "") + "   " + standing,
				job.Held ? "The job held now." : job.Open ? "A: change to this job. No penalty; the ladder's standing is kept." : "Not open yet - the crystals have not granted it.");
		}

		public override void OnFocus()
		{
			string id = Menu.Focused ?? "";
			int last = Math.Min(PerPage, _jobs.Count - _page * PerPage) - 1;
			if (id.StartsWith("job") && int.Parse(id.Substring(3)) > last) { Menu.Focus("job" + Math.Max(0, last)); return; }
			DescribeFocused();
		}

		public override bool OnPress()
		{
			JobInfo job = Focused();
			PartyMember m = Game.Party.Member(Screens.Hero);
			if (job == null || m == null || m.Progression != "mastery" || job.Held || !job.Open) { Menu.SoundBeep(); return true; }
			JobConfirm.Pending = job.Word;
			Menu.SoundDecide();
			Menu.Open("job-confirm");
			return true;
		}

		public override bool OnKey(MenuKey key)
		{
			if (key != MenuKey.L && key != MenuKey.R) return false;
			int pages = Math.Max(1, (_jobs.Count + PerPage - 1) / PerPage);
			if (pages < 2) return true;
			_page = (_page + (key == MenuKey.R ? 1 : pages - 1)) % pages;
			Fill();
			DescribeFocused();
			return true;
		}
	}

	/// <summary>"Are you sure you want to change jobs?" - Yes takes the job the Jobs screen picked.</summary>
	public sealed class JobConfirm : MenuBehaviour
	{
		public static string Pending;

		public override void OnOpen()
		{
			Screens.Header(Menu);
			JobInfo job = Pending != null ? Game.Party.JobInfo(Screens.Hero, Pending) : null;
			Menu.SetText("ask_job", job?.Title ?? "?");
			Screens.Colour(Menu, "ask_job", MenuColour.Yellow);
			Screens.Describe(Menu, job == null ? "" : job.Title + (job.HasLadder ? "   Lv. " + job.Level : ""), job == null ? "" : job.Mastered ? "Mastered!" : job.HasLadder ? job.Abp + " / " + job.AbpToNext + " ABP" : "");
			Menu.Focus("no");
		}

		public override bool OnPress()
		{
			if (Menu.Focused == "yes" && Pending != null && Game.Party.ChangeJob(Screens.Hero, Pending))
			{
				Menu.SoundDecide();
				Pending = null;
				Menu.Open("jobs");
				return true;
			}
			Menu.SoundCancel();
			Pending = null;
			Menu.Open("jobs");
			return true;
		}

		public override bool OnCancel()
		{
			Menu.SoundCancel();
			Pending = null;
			Menu.Open("jobs");
			return true;
		}
	}
}

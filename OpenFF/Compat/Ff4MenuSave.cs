// FF4's Save and Load screens as Steam's (captured at the Underground Waterway's save point, and from the title's LOAD
// GAME): Slot 1..3 and Title Menu down the right, what the lit slot holds on the left - its party by places (face, name,
// level, HP, MP), the place, the play time and the gil - or "No save data found."; the hand starts on the slot written
// last. Saving, Enter on a slot asks "Save data to Slot N?" and Yes writes it; Enter on Title Menu asks "Return to the
// title menu?". Loading (the title's), Enter on a held slot asks "Load data from Slot N?", on an empty one does nothing;
// Title Menu and Back go back to the title. The menu offers Save only on the world map and at save points
// (Ff4Menu.SaveAllowed).

using System;
using System.Collections.Generic;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Menu
	{
		private int _questionSlot;          // a slot's "Save data to Slot N?" (1..3), 0 for none
		private int _questionLoad;          // a slot's "Load data from Slot N?" (1..3), 0 for none
		private bool _questionTitle;        // "Return to the title menu?"
		private bool _fromTitle;            // the Load screen is the title's: drawn by it, over it
		private bool _loadOpening;          // the step it opened in: the title's Enter is not the screen's

		/// <summary>The menu the game registered (the title opens its Load screen).</summary>
		internal static Ff4Menu Instance { get; private set; }

		public Ff4Menu() { Instance = this; }

		/// <summary>What the title's Load screen came to: 0 while it is up, the slot read (1..3), or -1 for back to the title.</summary>
		internal int LoadChosen { get; private set; }

		/// <summary>The title's LOAD GAME: the Load screen over the title, the hand on the slot written last.</summary>
		internal void OpenLoad()
		{
			_open = true;
			_mode = Mode.Browse;
			_question = false;
			_questionSlot = _questionLoad = 0;
			_questionTitle = false;
			LoadChosen = 0;
			_fromTitle = _loadOpening = true;
			Game.Input.Capture = true;
			OpenScreen(Screen.Load);
		}

		/// <summary>The title draws its Load screen over itself (the title is drawn after the menu would be).</summary>
		internal void DrawOverTitle()
		{
			if (_open && _fromTitle) Draw();
		}

		/// <summary>The slot written last, 0-based (0 when none is).</summary>
		private static int LatestSlot()
		{
			int best = 0;
			string at = null;
			for (int slot = 1; slot <= Ff4Saves.SlotCount; slot++)
			{
				string written = Game.Saves.WrittenAt(slot);
				if (written != null && (at == null || string.CompareOrdinal(written, at) > 0)) { at = written; best = slot - 1; }
			}
			return best;
		}

		private void UpdateSaveLayout(InputState input)
		{
			if (_question) { UpdateQuestion(input); return; }
			if (input.Pressed(Pad.B)) { Back(); return; }
			if (input.Pressed(Pad.Up)) _cursor = (_cursor + 3) % 4;
			if (input.Pressed(Pad.Down)) _cursor = (_cursor + 1) % 4;
			if (!input.Pressed(Pad.A)) return;
			_question = true;
			_questionYes = false;
			_questionQuit = false;
			_questionLoad = 0;
			_questionSlot = _cursor < Ff4Saves.SlotCount ? _cursor + 1 : 0;
			_questionTitle = _cursor == Ff4Saves.SlotCount;
		}

		private void UpdateLoadLayout(InputState input)
		{
			if (_loadOpening) { _loadOpening = false; return; }
			if (_question) { UpdateQuestion(input); return; }
			bool back = input.Pressed(Pad.B) || input.KeyPressed("Escape");
			if (input.Pressed(Pad.Up)) _cursor = (_cursor + 3) % 4;
			if (input.Pressed(Pad.Down)) _cursor = (_cursor + 1) % 4;
			if (input.Pressed(Pad.A))
			{
				if (_cursor == Ff4Saves.SlotCount) back = true;
				else if (Ff4Saves.Exists(_cursor + 1))
				{
					_question = true;
					_questionYes = false;
					_questionQuit = _questionTitle = false;
					_questionSlot = 0;
					_questionLoad = _cursor + 1;
				}
			}
			if (back) { LoadChosen = -1; Close(); }
		}

		/// <summary>The Save or Load screen's question answered Yes: the slot written or read, or the title.</summary>
		private bool AnswerSaveQuestion()
		{
			if (_questionSlot > 0)
			{
				int slot = _questionSlot;
				_questionSlot = 0;
				if (!Ff4Saves.Save(slot)) Notice("Could not save here.");
				return true;
			}
			if (_questionLoad > 0)
			{
				LoadChosen = _questionLoad;
				_questionLoad = 0;
				Close();
				return true;
			}
			if (_questionTitle)
			{
				_questionTitle = false;
				Close();
				try { GlobalScope.wld.CBaseSystem.setTitle(true); } catch (Exception ex) { Log.Write(LogChannel.General, "menu: title menu: " + ex.Message); }
				return true;
			}
			return false;
		}

		private static string SaveSlotName(int slot) => T((uint)(50799 + slot), "Slot " + slot);

		private void FillSave(Ff4MenuHud.Data h)
		{
			h.Title = _screen == Screen.Load ? T(50008, "Load") : T(50007, "Save");
			for (int i = 0; i < h.SaveRow.Count; i++)
			{
				h.SaveRow[i].Present = true;
				h.SaveRow[i].Name = i < Ff4Saves.SlotCount ? SaveSlotName(i + 1) : T(50831, "Title Menu");
				h.SaveRow[i].Lit = i == _cursor;
			}
			(Ff4Party.Saved Party, Ff4FieldState.Data Field)? held = _cursor < Ff4Saves.SlotCount ? Ff4Saves.Peek(_cursor + 1) : null;
			h.SlotFilled = held.HasValue && held.Value.Party != null;
			h.NoDataLabel = T(50810, "No save data found.");
			for (int p = 0; p < h.SaveMember.Count; p++)
			{
				Ff4MenuHud.MemberRow row = h.SaveMember[p];
				Ff4Party.SavedCharacter c = h.SlotFilled ? held.Value.Party.Roster.Find(r => r.Slot >= 0 && r.Position == p) : null;
				row.Present = c != null;
				row.Lit = false;
				if (c == null) { row.Name = row.Level = row.Hp = row.MaxHp = row.Mp = row.MaxMp = ""; row.Dim = row.Low = row.Back = false; continue; }
				row.Name = c.Name;
				row.Level = c.Level.ToString();
				row.Hp = c.Hp.ToString();
				row.MaxHp = c.MaxHp.ToString();
				row.Mp = c.Mp.ToString();
				row.MaxMp = c.MaxMp.ToString();
				row.Face = c.Id;
				row.Low = c.Hp > 0 && c.Hp * 4 <= c.MaxHp;
				row.Dim = c.Hp <= 0;
				row.Back = Ff4Party.RowOf(p, held.Value.Party.Formation) == 1;
			}
			Ff4FieldState.Data field = held?.Field;
			h.SavePlace = field?.Place ?? "";
			h.SaveTime = h.SlotFilled ? Ff4Saves.PlayTimeText(field?.PlaySeconds ?? 0) : "";
			h.SaveGil = h.SlotFilled ? held.Value.Party.Gil + T(50446, "Gil") : "";
			h.Question = _question;
			h.Yes = _question && _questionYes;
			h.No = _question && !_questionYes;
			h.QuestionText = _questionTitle ? T(50830, "Return to the title menu?")
				: _questionLoad > 0 ? T(50806, "Load data from %SCC30%?").Replace("%SCC30%", SaveSlotName(_questionLoad))
				: T(50804, "Save data to %SCC30%?").Replace("%SCC30%", SaveSlotName(_questionSlot > 0 ? _questionSlot : _cursor + 1));
		}
	}
}

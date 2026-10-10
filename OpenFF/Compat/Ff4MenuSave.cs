// FF4's Save screen as Steam's (captured at the Underground Waterway's save point): Slot 1..3 and Title Menu down the right,
// what the lit slot holds on the left - its party by places (face, name, level, HP, MP), the place, the play time and the
// gil - or "No save data found."; Enter on a slot asks "Save data to Slot N?" and Yes writes it, Enter on Title Menu asks
// "Return to the title menu?". The menu offers Save only on the world map and at save points (Ff4Menu.SaveAllowed).

using System;
using System.Collections.Generic;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Menu
	{
		private int _questionSlot;          // a slot's "Save data to Slot N?" (1..3), 0 for none
		private bool _questionTitle;        // "Return to the title menu?"

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
			_questionSlot = _cursor < Ff4Saves.SlotCount ? _cursor + 1 : 0;
			_questionTitle = _cursor == Ff4Saves.SlotCount;
		}

		/// <summary>The Save screen's question answered Yes: the slot written, or the title.</summary>
		private bool AnswerSaveQuestion()
		{
			if (_questionSlot > 0)
			{
				int slot = _questionSlot;
				_questionSlot = 0;
				if (!Ff4Saves.Save(slot)) Notice("Could not save here.");
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
			h.Title = T(50007, "Save");
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
				: T(50804, "Save data to %SCC30%?").Replace("%SCC30%", SaveSlotName(_questionSlot > 0 ? _questionSlot : _cursor + 1));
		}
	}
}

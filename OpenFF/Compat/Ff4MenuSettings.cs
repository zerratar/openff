// FF4's Settings screen as Steam's (measured off its draws): nine lines, five in view - Music, Sound Effects and Voices
// (sliders 0..10), Battle Mode (Active / Wait), Battle Speed (Fast 1..6 Slow), Subtitles (On / Off), Window Design (1..6),
// Help and Quit. Up and down move the hand (round from the last to the first), left and right change the line's value at
// once; Quit asks "Do you wish to quit the game ?" and goes to the title. Backspace leaves, and the settings are written
// (Ff4Settings). Help has no screen of its own yet.

using System;
using System.Collections.Generic;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Menu
	{
		private enum SettingLine { Music, SoundEffects, Voices, BattleMode, BattleSpeed, Subtitles, WindowDesign, Help, Quit }

		private const int SettingLines = 9, SettingsShown = 5;
		private bool _questionQuit;
		private static bool _settingsApplied;

		/// <summary>The settings' volumes into the game's sound, once, as the menu first runs.</summary>
		private static void ApplySettingsOnce()
		{
			if (_settingsApplied) return;
			_settingsApplied = true;
			Ff4Settings.Current.Apply();
		}

		private void UpdateSettings(InputState input)
		{
			if (_question) { UpdateQuestion(input); return; }
			Ff4Settings s = Ff4Settings.Current;
			if (input.Pressed(Pad.B)) { s.Save(); Back(); return; }
			if (input.Pressed(Pad.Up)) _cursor = (_cursor + SettingLines - 1) % SettingLines;
			if (input.Pressed(Pad.Down)) _cursor = (_cursor + 1) % SettingLines;
			if (_cursor < _scroll) _scroll = _cursor;
			if (_cursor >= _scroll + SettingsShown) _scroll = _cursor - SettingsShown + 1;
			int step = input.Pressed(Pad.Right) ? 1 : input.Pressed(Pad.Left) ? -1 : 0;
			SettingLine line = (SettingLine)_cursor;
			if (step != 0)
			{
				switch (line)
				{
					case SettingLine.Music: s.Music = Math.Clamp(s.Music + step, 0, 10); s.Apply(); break;
					case SettingLine.SoundEffects: s.SoundEffects = Math.Clamp(s.SoundEffects + step, 0, 10); s.Apply(); break;
					case SettingLine.Voices: s.Voices = Math.Clamp(s.Voices + step, 0, 10); break;
					case SettingLine.BattleMode: s.BattleMode = Math.Clamp(s.BattleMode + step, 0, 1); break;
					case SettingLine.BattleSpeed: s.BattleSpeed = Math.Clamp(s.BattleSpeed + step, 1, 6); break;
					case SettingLine.Subtitles: s.Subtitles = step < 0; break;
					case SettingLine.WindowDesign: s.WindowDesign = Math.Clamp(s.WindowDesign + step, 1, 6); break;
				}
				return;
			}
			if (input.Pressed(Pad.A) && line == SettingLine.Quit) { _question = true; _questionQuit = true; _questionSlot = 0; _questionTitle = false; _questionYes = false; }
		}

		private void FillSettings(Ff4MenuHud.Data h)
		{
			Ff4Settings s = Ff4Settings.Current;
			h.Title = T(50006, "Settings");
			(uint Help, string Fallback)[] helps =
			{
				(50759, "Adjust music volume."), (50760, "Adjust sound effects volume."), (50761, "Adjust volume of character dialogue."),
				(50750, "Toggle battle mode."), (50751, "Select a smaller number to increase speed of battles."),
				(50755, "Toggle the display of subtitles during events."), (50754, "Change the design used for windows."),
				(0, ""), (60272, "Quit Game"),
			};
			h.Help = helps[_cursor].Help != 0 ? T(helps[_cursor].Help, helps[_cursor].Fallback) : "";
			(uint Text, string Fallback)[] labels =
			{
				(50730, "Music"), (50731, "Sound Effects"), (50727, "Voices"), (50701, "Battle Mode"), (50702, "Battle Speed"),
				(50726, "Subtitles"), (50704, "Window Design"), (50733, "Help"), (60271, "Quit"),
			};
			for (int k = 0; k < h.Setting.Count; k++)
			{
				int i = _scroll + k;
				Ff4MenuHud.SettingRow row = h.Setting[k];
				row.Present = i < SettingLines;
				if (!row.Present) continue;
				SettingLine line = (SettingLine)i;
				row.Label = T(labels[i].Text, labels[i].Fallback);
				row.Lit = i == _cursor && !_question;
				row.Slider = line <= SettingLine.Voices;
				row.Pair = line == SettingLine.BattleMode || line == SettingLine.Subtitles;
				row.Steps = line == SettingLine.BattleSpeed;
				row.Design = line == SettingLine.WindowDesign;
				row.Button = line >= SettingLine.Help;
				foreach (Ff4MenuHud.CellRow box in row.Box) { box.Present = false; box.Lit = false; box.Name = ""; }
				if (row.Slider)
				{
					int v = line == SettingLine.Music ? s.Music : line == SettingLine.SoundEffects ? s.SoundEffects : s.Voices;
					row.Number = v.ToString();
					row.Marker = 15.3f + 863.7f * v / 10f;   // the marker's left: its middle at v tenths along Steam's 863.7-pixel bar, 55 in
				}
				else if (row.Pair)
				{
					bool first = line == SettingLine.BattleMode ? s.BattleMode == 0 : s.Subtitles;
					SetBox(row.Box[0], line == SettingLine.BattleMode ? T(50705, "Active") : T(50725, "On"), first);
					SetBox(row.Box[1], line == SettingLine.BattleMode ? T(50706, "Wait") : T(50724, "Off"), !first);
				}
				else if (row.Steps || row.Design)
				{
					int set = row.Steps ? s.BattleSpeed : s.WindowDesign;
					for (int b = 0; b < 6; b++) SetBox(row.Box[b], (b + 1).ToString(), b + 1 == set);
					row.FastLabel = T(50709, "Fast");
					row.SlowLabel = T(50707, "Slow");
				}
			}
			Ff4MenuHud.Scroll(h.SettingScroll, SettingLines, SettingsShown, _scroll, 688.5f, true);
			h.SettingConfirm = !_question && _cursor >= (int)SettingLine.Help;
			h.Question = _question;
			h.Yes = _question && _questionYes;
			h.No = _question && !_questionYes;
			h.QuestionText = T(60273, "Do you wish to quit the game ?");
		}

		private static void SetBox(Ff4MenuHud.CellRow box, string name, bool lit)
		{
			box.Present = true;
			box.Name = name;
			box.Lit = lit;
		}
	}
}

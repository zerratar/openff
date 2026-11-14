// Settings' Help, as Steam's FF4.exe draws it (its own PC screens - MENU_LAYOUT has none; measured off Steam's frames at
// 1920 x 1080). Enter on Help lists the ten topics of babil_menu.msd (50900 Controls, 50902 Saving and Suspending ... 50918
// Known Abilities; 50920, the mobile build's cloud saves, is not offered) in one window under the title: 135 a row, the
// strip moved by the pixel just enough to keep the lit row whole (the last lit, Omnicasting is cut at the top), Up and
// Down going round the ends. Enter opens the topic's page (its text the id after its name) in the same window: its lines 54
// apart, cut at the window's edges, moved 67.5 a step for as long as Up or Down is held, no further than its end (the
// lines' height and an 80 margin less the window's 864 but a step). %key_assignNN% in a line is the key's cap (Enter, Backspace,
// Tab, C, M). Backspace goes back a screen; the list keeps its lit row.

using System;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Menu
	{
		private enum HelpView { None, List, Page }

		private HelpView _help;
		private int _helpCursor;
		private float _helpListTop, _helpTextTop;

		private const int HelpTopics = 10;
		private const float HelpRow = 135f, HelpWindow = 864f, HelpLine = 54f, HelpMargin = 80f, HelpStep = 67.5f, HelpTrack = 796.5f;

		/// <summary>Where the page's first line's capitals stand under the window's top (Steam's 80), as the text frame is drawn.</summary>
		private const float HelpTextOrigin = 80f - 5.5f;

		private void OpenHelp()
		{
			Game.Audio.PlaySe(0, 1);
			_help = HelpView.List;
			_helpCursor = 0;
			_helpListTop = 0f;
		}

		private void UpdateHelp(InputState input)
		{
			if (_help == HelpView.Page)
			{
				if (input.Pressed(Pad.B)) { Game.Audio.PlaySe(0, 2); _help = HelpView.List; return; }
				float most = HelpMost(_helpCursor);
				if (input.IsHeld(Pad.Down)) _helpTextTop = Math.Min(most, _helpTextTop + HelpStep);
				else if (input.IsHeld(Pad.Up)) _helpTextTop = Math.Max(0f, _helpTextTop - HelpStep);
				return;
			}
			if (input.Pressed(Pad.B)) { Game.Audio.PlaySe(0, 2); _help = HelpView.None; return; }
			int was = _helpCursor;
			if (input.Pressed(Pad.Up)) _helpCursor = (_helpCursor + HelpTopics - 1) % HelpTopics;
			if (input.Pressed(Pad.Down)) _helpCursor = (_helpCursor + 1) % HelpTopics;
			if (_helpCursor != was) Game.Audio.PlaySe(0, 3);
			// The strip moved just enough to show the lit row whole.
			float top = _helpCursor * HelpRow, bottom = top + HelpRow;
			if (top < _helpListTop) _helpListTop = top;
			if (bottom > _helpListTop + HelpWindow) _helpListTop = bottom - HelpWindow;
			if (input.Pressed(Pad.A))
			{
				Game.Audio.PlaySe(0, 1);
				_help = HelpView.Page;
				_helpTextTop = 0f;
			}
		}

		private static uint HelpTitleId(int topic) => (uint)(50900 + 2 * topic);

		/// <summary>How far a page moves at most: its lines' height and the 80 margin, less the window but a step (Steam's Status
		/// Effects, 96 lines, stops 4467.5 down - its last line 829 down the screen).</summary>
		private static float HelpMost(int topic) => Math.Max(0f, HelpLines(topic) * HelpLine + HelpMargin - (HelpWindow - HelpStep));

		/// <summary>A topic's page as babil_menu.msd has it, its line breaks kept.</summary>
		private static string HelpPage(int topic) => Ff4Layouts.MenuPage(HelpTitleId(topic) + 1) ?? "";

		private static int HelpLines(int topic) => HelpPage(topic).Split('\n').Length;

		private void FillHelp(Ff4MenuHud.Data h)
		{
			h.NoTitle = false;
			if (_help == HelpView.List)
			{
				h.Title = T(50733, "Help");
				for (int k = 0; k < h.HelpRow.Count; k++)
				{
					h.HelpRow[k].Present = k < HelpTopics;
					h.HelpRow[k].Name = k < HelpTopics ? T(HelpTitleId(k), "") : "";
					h.HelpRow[k].Lit = k == _helpCursor;
				}
				h.HelpListTop = -_helpListTop;
				Ff4MenuHud.Scroll(h.HelpScroll, (int)(HelpTopics * HelpRow), (int)HelpWindow, (int)_helpListTop, HelpTrack, true);
				return;
			}
			h.Title = T(HelpTitleId(_helpCursor), "");
			h.HelpText = WithKeyCaps(HelpPage(_helpCursor));
			h.HelpTextTop = HelpTextOrigin - _helpTextTop;
			int total = (int)(HelpMost(_helpCursor) + HelpWindow);
			Ff4MenuHud.Scroll(h.HelpPageScroll, Math.Max(total, (int)HelpWindow), (int)HelpWindow, (int)_helpTextTop, HelpTrack, true);
		}

		/// <summary>A help text's %key_assignNN% as the key's cap (LayoutScreen.KeyChar + its icon_keyboard_64 cell): the keys
		/// Steam's key hints name the same way - 01 Backspace, 02 Enter, 09 M, 11 Tab, 12 C. Any other is left out.</summary>
		private static string WithKeyCaps(string text) => System.Text.RegularExpressions.Regex.Replace(text, "%key_assign(\\d+)%", m =>
		{
			int cell = int.Parse(m.Groups[1].Value) switch { 1 => 15, 2 => 43, 9 => 52, 11 => 16, 12 => 48, _ => -1 };
			return cell < 0 ? "" : ((char)(LayoutScreen.KeyChar + cell)).ToString();
		});
	}
}

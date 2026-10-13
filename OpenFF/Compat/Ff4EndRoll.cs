// FF4's staff roll: callEndRoll's world::EndingPlayer over the ending's scenes.
//
// ending_page_file.epf: a u32 count, then 0x1C8-byte pages ("PA02"): +4 the screens it draws on (bit 0 the main screen,
// bit 1 the second), +6 frames hidden, +8 frames shown, +0xa frames fading in, +0xc fading out; +0xe a column count for
// each of 22 lines (11 a screen), +0x24 each line's placing (0 centring: the columns spread evenly; 1 and 2 pulled about
// the middle - an odd count with one in the middle, an even one without), +0x3a / +0x45 the screens' first line (+0x44)
// and the gaps after each line (+8), +0x50 each line's x shift, +0x7c the columns' colours (1 white, 10 the titles' blue),
// +0xc0 the columns' message ids - the ending map's own messages (t24_00.msd, 900500...). A page on the main screen fades
// the screen itself out and in about it (CFade); one on the second blends its text in (G2S_SetBlendAlpha, 16 steps).
// The text canvas is the phone's 480 x 320; Steam lays it over its 1920 x 1080 as its menus, x 3.375 from 150 across, and
// draws the main screen's and the second's in the same place (measured off its frames: the first page's centre at 656,
// its lines' capitals at 464 and 581 - lines 4 and 6).

using System;
using System.Collections.Generic;
using OpenFF;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal static class Ff4EndRoll
	{
		private const int PageSize = 0x1C8;
		private const float Scale = 3.375f, Left = 150f;

		private static byte[] _file;
		private static int _pages, _page, _tick;
		private static Dictionary<uint, string> _texts;
		private enum Phase { Check, Hide, FadeIn, Show, FadeOut }
		private static Phase _phase;
		private static int _alpha;   // the second screen's blend, 0..16

		public static bool Running { get; private set; }

		/// <summary>callEndRoll(?, ?, ?): EndingPlayer::initialize - the pages from the first, the text from the map's messages.</summary>
		public static void CallEndRoll(GlobalScope.ScriptEngine engine)
		{
			engine.getDword();
			engine.getDword();
			engine.getDword();
			Ff4Cutscene.Guard("end roll", Start);
		}

		private static void Start()
		{
			if (!TableFiles.ReadAny(GameArchive.Chain, "ending_page_file.epf", out _file) || _file == null || _file.Length < 4)
			{
				Log.Write(LogChannel.General, "end roll: no ending_page_file.epf");
				return;
			}
			_pages = Math.Min(BitConverter.ToInt32(_file, 0), (_file.Length - 4) / PageSize);
			_texts = MapTexts(GlobalScope.stg.CStageMng.CurrentName);
			// The ending's map (t24_00) holds the roll's lines; elsewhere (a test, a mod's call) its file is read for them.
			if (_texts == null || !_texts.ContainsKey(900500)) _texts = MapTexts("t24_00");
			_page = 0;
			_phase = Phase.Check;
			_tick = 0;
			_alpha = 0;
			Running = _pages > 0;
			Log.Write(LogChannel.General, "end roll: " + _pages + " page(s), " + (_texts?.Count ?? 0) + " line(s) of " + GlobalScope.stg.CStageMng.CurrentName);
		}

		/// <summary>The map's message file (CAST_EVENT_MSD's &lt;map&gt;.msd, UTF-16 in the Steam build), every message's first page.</summary>
		private static Dictionary<uint, string> MapTexts(string map)
		{
			Dictionary<uint, string> texts = new Dictionary<uint, string>();
			try
			{
				if (map != null && TableFiles.ReadAny(GameArchive.Chain, map + ".msd", out byte[] raw))
				{
					if (Ff4Text.IsWide(raw)) raw = Ff4Text.DecodeWide(raw);
					foreach (OpenFF.Content.MsdMessage m in OpenFF.Content.Msd.Read(raw).Messages)
					{
						if (m.Pages.Count > 0 && !texts.ContainsKey(m.Id)) texts[m.Id] = m.Pages[0];
					}
				}
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "end roll: " + map + ".msd: " + ex.Message); }
			return texts;
		}

		private static ushort U16(int at) => BitConverter.ToUInt16(_file, at);

		private static int Base => 4 + _page * PageSize;

		/// <summary>Each step (30 a second): EndingPlayer::update's phases.</summary>
		public static void Tick()
		{
			if (!Running) return;
			if (_page >= _pages) { Running = false; Log.Write(LogChannel.General, "end roll: over"); return; }
			int b = Base;
			int flags = U16(b + 4), hide = U16(b + 6), show = U16(b + 8), fadeIn = U16(b + 10), fadeOut = U16(b + 12);
			switch (_phase)
			{
				case Phase.Check:
					// pageDataCheckPhase: the main screen black at once for a page on it; then hidden.
					if ((flags & 1) != 0) GlobalScope.dgs.CFade.Main().fadeOut(0, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
					_alpha = 0;
					_phase = Phase.Hide;
					_tick = 0;
					break;
				case Phase.Hide:
					if (_tick == hide) { _phase = Phase.FadeIn; _tick = 0; if ((flags & 1) != 0) GlobalScope.dgs.CFade.Main().fadeIn(fadeIn); }
					else _tick++;
					break;
				case Phase.FadeIn:
				{
					int t = fadeIn != 0 ? (fadeIn - _tick) * 16 / fadeIn : 0;
					_alpha = (flags & 1) != 0 ? 16 : t < 17 ? Math.Min(16, 16 - t) : 0;
					if (_tick == fadeIn) { _phase = Phase.Show; _tick = 0; _alpha = 16; }
					else _tick++;
					break;
				}
				case Phase.Show:
					if (_tick == show) { _phase = Phase.FadeOut; _tick = 0; if ((flags & 1) != 0) GlobalScope.dgs.CFade.Main().fadeOut(fadeOut, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK); }
					else _tick++;
					break;
				case Phase.FadeOut:
				{
					int t = fadeOut != 0 ? (fadeOut - _tick) * 16 / fadeOut : 0;
					_alpha = (flags & 1) != 0 ? 16 : Math.Clamp(t, 0, 16);
					if (_tick == fadeOut) { _alpha = 0; _page++; _phase = Phase.Check; _tick = 0; }
					else _tick++;
					break;
				}
			}
		}

		public static void Stop() => Running = false;

		// ---- the text ----

		/// <summary>One placed column: its text, the colour index, and where (the canvas's x, y) and how it hangs (0 left of x,
		/// 1 about x, 2 right of x - DGSTextDrawECC's 0x88, 0x90, 0xa0).</summary>
		private struct Placed
		{
			public string Text;
			public int Colour, X, Y, Align;
		}

		private static string Line(uint id) => id != 0 && _texts != null && _texts.TryGetValue(id, out string s) ? s : null;

		/// <summary>DGSTextLengthECC's characters x 8: the canvas's width for a text.</summary>
		private static int Width(uint id) => (Line(id)?.Length ?? 0) * 8;

		private static List<Placed> PlacePage()
		{
			List<Placed> placed = new List<Placed>();
			int b = Base;
			int flags = U16(b + 4);
			if (_phase == Phase.Check || _phase == Phase.Hide) return placed;
			// showMessage: the main screen's eleven lines, then the second's. Centring's halves: the main screen's lines the
			// left half when both screens show, else the whole width; the second's the right half beside the main, else the left.
			for (int screen = 0; screen < 2; screen++)
			{
				if ((flags & (1 << screen)) == 0) continue;
				int first = _file[b + 0x3a + 11 * screen];
				if (first >= 0xFC) continue;
				int half = screen == 0 ? ((flags & 3) == 3 ? 1 : 0) : ((flags & 1) != 0 ? 2 : 1);
				int y = first + 0x44;
				for (int line = 0; line < 11; line++)
				{
					if (line > 0)
					{
						y += _file[b + 0x3b + 11 * screen + line - 1] + 8;
						if (y > 0x13F) break;
					}
					int li = screen * 11 + line;
					int mode = _file[b + 0x24 + li];
					if (mode == 0) Centering(placed, b, li, y, half);
					else if (mode == 1) PullOdd(placed, b, li, y);
					else if (mode == 2) PullEven(placed, b, li, y);
				}
			}
			return placed;
		}

		private static uint Id(int b, int li, int col) => BitConverter.ToUInt32(_file, b + 0xc0 + li * 12 + col * 4);

		private static int Colour(int b, int li, int col) => _file[b + 0x7c + li * 3 + col];

		/// <summary>EndingPlayer::centering: the line's columns spread over its half (or the whole width), each about its spot.</summary>
		private static void Centering(List<Placed> placed, int b, int li, int y, int half)
		{
			int count = _file[b + 0xe + li];
			int width = half != 0 ? 0xF0 : 0x1E0;
			int origin = half == 2 ? 0xD2 : half == 1 ? 0x1E : 0;
			int shift = BitConverter.ToInt16(_file, b + 0x50 + li * 2);
			int step = width / (count + 1);
			int n = 0;
			for (int col = 0; col < 3 && n < count; col++)
			{
				uint id = Id(b, li, col);
				int colour = Colour(b, li, col);
				if (id == 0 || colour == 0) continue;
				placed.Add(new Placed { Text = Line(id), Colour = colour, X = shift + origin + step + step * n, Y = y, Align = 1 });
				n++;
			}
		}

		/// <summary>pullCenterOdd: the middle column about the centre, the others pulled out from it in pairs.</summary>
		private static void PullOdd(List<Placed> placed, int b, int li, int y)
		{
			int count = _file[b + 0xe + li];
			if (count == 0) return;
			int mid = count >> 1;
			int shift = BitConverter.ToInt16(_file, b + 0x50 + li * 2);
			uint id = Id(b, li, mid);
			int done = 0;
			if (id != 0 && Colour(b, li, mid) != 0)
			{
				placed.Add(new Placed { Text = Line(id), Colour = Colour(b, li, mid), X = shift + 0x80, Y = y, Align = 1 });
				done++;
				if (done >= count) return;
			}
			int gap = id == 0 ? 0x18 : Width(id) + 0x18;
			PullMessage(placed, b, li, y, gap, mid - 1, mid + 1, count, done);
		}

		/// <summary>pullCenterEven: the columns pulled out from the centre in pairs, nothing in the middle.</summary>
		private static void PullEven(List<Placed> placed, int b, int li, int y)
		{
			int count = _file[b + 0xe + li];
			if (count == 0) return;
			int mid = count >> 1;
			PullMessage(placed, b, li, y, 0xC, mid - 1, mid, count, 0);
		}

		/// <summary>pullCenterMessage: from the centre (0x80 and the line's shift) out, a column ending at -gap/2 on the left
		/// and one starting at +gap/2 on the right, each pair's widths widening the next.</summary>
		private static void PullMessage(List<Placed> placed, int b, int li, int y, int gap, int left, int right, int count, int done)
		{
			int centre = BitConverter.ToInt16(_file, b + 0x50 + li * 2) + 0x80;
			int l = -(gap >> 1), r = gap >> 1;
			for (int guard = 0; guard < 4 && done < count; guard++)
			{
				if (left >= 0 && left < 3)
				{
					uint id = Id(b, li, left);
					if (id != 0 && Colour(b, li, left) != 0)
					{
						placed.Add(new Placed { Text = Line(id), Colour = Colour(b, li, left), X = centre + l, Y = y, Align = 2 });
						if (++done >= count) break;
					}
				}
				if (right >= 0 && right < 3)
				{
					uint id = Id(b, li, right);
					if (id != 0 && Colour(b, li, right) != 0)
					{
						placed.Add(new Placed { Text = Line(id), Colour = Colour(b, li, right), X = centre + r, Y = y, Align = 0 });
						if (++done >= count) break;
					}
				}
				l -= left >= 0 && left < 3 && Id(b, li, left) != 0 ? Width(Id(b, li, left)) + 0xC : 0xC;
				r += right >= 0 && right < 3 && Id(b, li, right) != 0 ? Width(Id(b, li, right)) + 0xC : 0xC;
				left++;
				right++;
			}
		}

		/// <summary>The roll's text for the layout (menu.roll[0..65]).</summary>
		public static void Fill(Ff4MenuHud.Data h)
		{
			h.Roll = Running;
			foreach (Ff4MenuHud.RollRow row in h.RollRow) row.Present = false;
			if (!Running) return;
			List<Placed> placed = PlacePage();
			float opacity = Math.Clamp(_alpha, 0, 16) / 16f;
			for (int i = 0; i < placed.Count && i < h.RollRow.Count; i++)
			{
				Placed p = placed[i];
				Ff4MenuHud.RollRow row = h.RollRow[i];
				row.Present = !string.IsNullOrEmpty(p.Text);
				row.Text = p.Text ?? "";
				row.Blue = p.Colour == 10;
				// The frame is 1000 wide: hung left of, about or right of the spot.
				float x = Left + p.X * Scale;
				row.Left = p.Align == 1 ? x - 500f : p.Align == 2 ? x - 1000f : x;
				row.Centre = p.Align == 1;
				row.Right = p.Align == 2;
				row.Top = p.Y * Scale;
				row.Opacity = opacity;
			}
		}
	}
}

// FF4's scenes' own 2D programs: ce_CallProgram(n) and ce_CallProgParam(...) - EventConteManager::setSPFunc /
// setSPFuncParam, the program then run once a scene frame (executeSPFunc) - as Steam's FF4.exe has them (its table at
// +0xc6c: FUN_004fb1b0, FUN_004faa40, FUN_004fad50, LAB_004fb0b0, FUN_004fa8d0, LAB_004faa20; libff4's SPFunc_*):
//   0 event2_art   e03_00's drawn sequence (event2d_pack's conte_01_01 art, its sprites and conte_font_01..06's lines)
//   1 event19_art  e19_00: event2d_pack's e19 (or e19_02) over the screen for <param 1> frames
//   2 event26      e26_00 / e26_02: conte_26's lines (17, centred, with flag 895; else conte_26_half's 10 at x 150)
//                  rising from below the screen one pixel a frame, fading in and out over the top and bottom 80
//   3 event26_3    e26_03: the original development staff, by (screen, line, entry, shown, x, y) - FF4.exe's own list
//                  (blue titles, white names); a button skips the scene
//   4 drawMsg      the e19 / e19_02 picture and a scene message centred over it
//   5 drawMsg_Moon a scene message alone
// Both commands do nothing while the scene is skipped. Steam on a screen wider than 3:2 takes the plain pictures, not
// the _ipad ones. Places are the phone's 480 x 320 (Steam's 1080p: x 150 + 3.375 x, y 3.375 y); the pictures and cells
// are plates as the scenes' others (half size), the texts the staff roll's rows (ff4_roll.xml).

using System;
using System.Collections.Generic;
using System.IO;

namespace OpenFF.Client
{
	internal static class Ff4ScenePrograms
	{
		private static int _program = -1, _step, _count;
		private static readonly int[] _param = new int[6];
		private static bool _running;

		private static GlobalScope.sys2d.Sprite3d _picture;

		private sealed class Riser
		{
			public GlobalScope.sys2d.Sprite3d Sprite;
			public bool Moving;
			public short Speed, Pos;
			public int X;
		}

		private static readonly List<Riser> _risers = new List<Riser>();

		private sealed class Line
		{
			public string Text;
			public bool Blue;
			public int X, Y;
		}

		private static readonly SortedDictionary<int, Line> _lines = new SortedDictionary<int, Line>();
		private static string _message;
		private static Dictionary<uint, string> _texts;
		private static string _textsOf;

		/// <summary>Texts up for the layout (the staff, a message).</summary>
		public static bool Showing => _lines.Count > 0 || !string.IsNullOrEmpty(_message);

		public static void CallProgram(GlobalScope.ScriptEngine engine)
		{
			int n = (int)engine.getDword();
			if (Ff4Cutscene.EventSkipping || n < 0 || n >= 6) return;
			_program = n;
			_step = _count = 0;
			_running = true;
			Log.Write(LogChannel.File, "script: FF4 scene program " + n);
		}

		public static void CallProgParam(GlobalScope.ScriptEngine engine)
		{
			int[] p = new int[6];
			for (int i = 0; i < 6; i++) p[i] = (int)engine.getDword();
			if (Ff4Cutscene.EventSkipping) return;
			if (p[0] == 500) return;   // setSubPlaneVisiblity(BG0 only): the second screen's 3D - one screen here
			switch (_program)
			{
				case 4:
				case 5:
					// DGSTextErase, then the message drawn about the middle (DGSTextDraw2(128, 96)).
					_message = SceneText((uint)p[0]);
					break;
				case 2:
					if (p[0] >= 0 && p[0] < _risers.Count) { _risers[p[0]].Moving = true; _risers[p[0]].Speed = (short)p[1]; }
					break;
				case 3:
				{
					int slot = (p[0] != 0 ? 11 : 0) + p[1];
					_lines.Remove(slot);
					if (p[3] == 0) break;
					(int colour, string text) = Staff((p[0] != 0 ? 19 : 0) + p[2]);
					if (text != null) _lines[slot] = new Line { Text = text, Blue = colour == 10, X = p[4], Y = p[5] };
					break;
				}
				default:
					Array.Copy(p, _param, 6);
					break;
			}
		}

		/// <summary>The scene's frame: the program running steps once (executeSPFunc).</summary>
		public static void Tick()
		{
			if (!_running) return;
			try
			{
				switch (_program)
				{
					case 0: Event2Art(); break;
					case 1: Event19Art(); break;
					case 2: Event26(); break;
					case 3: Event26Staff(); break;
					case 4: if (_step == 0) { ShowPicture(_param[1] == 0 ? "e19" : "e19_02"); _step = 1; } break;
				}
			}
			catch (Exception ex)
			{
				Log.First(LogChannel.General, "scene-program-" + _program, 3, () => "script: FF4 scene program " + _program + ": " + ex.Message);
				_running = false;
			}
		}

		/// <summary>The scene is over (EventConteManager's cleanup): its pictures, cells and texts go.</summary>
		public static void Clear()
		{
			_running = false;
			_program = -1;
			ReleasePicture();
			foreach (Riser r in _risers) Release(r.Sprite);
			_risers.Clear();
			_lines.Clear();
			_message = null;
		}

		// ---- the programs ----

		private static void Event2Art()
		{
			if (_step != 0) return;
			_step = 1;
			Log.First(LogChannel.General, "scene-event2-art", 1, () => "script: FF4 scene program 0 (event2_art, e03_00's drawn sequence) is not in yet");
		}

		private static void Event19Art()
		{
			if (_step == 0)
			{
				ShowPicture(_param[1] == 0 ? "e19" : "e19_02");
				_step = 1;
				return;
			}
			if (_step == 1 && _count++ >= _param[0])
			{
				// The 3D back on the main screen, the message window set up again.
				ReleasePicture();
				_running = false;
				_step = 2;
			}
		}

		private static void Event26()
		{
			if (_step == 0)
			{
				bool full = GlobalScope.FlagManager.singleton().get(0, 0x37f) != 0;
				int count = full ? 17 : 10, x = full ? 0xf0 : 0x96;
				string cells = full ? "conte_26" : "conte_26_half";
				for (int i = 0; i < count; i++)
				{
					GlobalScope.sys2d.Sprite3d sprite = new GlobalScope.sys2d.Sprite3d();
					sprite.Load(GlobalScope.sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, cells + ".NCER", cells + ".NANR", "conte_26.NCGR", null);
					sprite.SetCell((ushort)i);
					sprite.SetAnimation(true);
					sprite.SetShow(false);
					sprite.SetScaleF(2048, 2048);
					sprite.SetPositionI(x, 0x150);
					GlobalScope.sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(sprite);
					_risers.Add(new Riser { Sprite = sprite, Pos = 0xd20, X = x });
				}
				_step = 1;
				return;
			}
			foreach (Riser r in _risers)
			{
				if (!r.Moving) continue;
				r.Pos += r.Speed;
				int y = r.Pos / 10;
				r.Sprite.SetPositionI(r.X, y);
				int alpha = 0x1f;
				if (y < 0x50) alpha = Math.Clamp((y - 0x50) / 2 + 0x1f, 0, 0x1f);
				else if (y > 0xf0) alpha = Math.Clamp((0xf0 - y) / 2 + 0x1f, 0, 0x1f);
				r.Sprite.SetAlpha((byte)alpha);
				if (r.Pos < -0xa0) { r.Moving = false; r.Sprite.SetShow(false); }
				else if (alpha > 0) r.Sprite.SetShow(true);
			}
		}

		private static void Event26Staff()
		{
			if (_step == 0) { _step = 1; return; }
			InputState input = Game.Input;
			if (Ff4Cutscene.EventSkip == 3 || !(input.Pressed(Pad.A) || input.Pressed(Pad.B))) return;
			// SPFunc_event26_3: a button ends the staff page - the scene skipped, both screens out over 15 frames.
			Ff4Cutscene.EventSkip = 3;
			GlobalScope.dgs.CFade.Main().fadeOut(15, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
			GlobalScope.dgs.CFade.Sub().fadeOut(15, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
			Ff4Cutscene.StopSceneVoice();
		}

		// ---- pictures and sprites ----

		/// <summary>sys2d::Bg::bgLoad of an event2d_pack picture: Steam keeps its screen as a cell and its characters as a
		/// picture - a plate at half size about the middle, as the field's bgLoad.</summary>
		private static void ShowPicture(string name)
		{
			ReleasePicture();
			GlobalScope.sys2d.Sprite3d sprite = new GlobalScope.sys2d.Sprite3d();
			sprite.Load(GlobalScope.sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, name + ".NSCR", null, name + ".NCGR", null);
			sprite.SetCell(0);
			sprite.SetScaleF(2048, 2048);
			sprite.SetPositionI(240, 160);
			GlobalScope.sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(sprite);
			_picture = sprite;
			Log.Write(LogChannel.File, "script: FF4 scene program picture " + name);
		}

		private static void ReleasePicture()
		{
			if (_picture == null) return;
			Release(_picture);
			_picture = null;
		}

		private static void Release(GlobalScope.sys2d.Sprite3d sprite)
		{
			try
			{
				GlobalScope.sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(sprite);
				sprite.Release();
			}
			catch (Exception) { }
		}

		// ---- texts ----

		private static string SceneText(uint id)
		{
			string scene = Ff4Cutscene.SceneStage ?? GlobalScope.stg.CStageMng.CurrentName;
			if (_texts == null || _textsOf != scene) { _texts = Ff4EndRoll.MapTexts(scene); _textsOf = scene; }
			return _texts.TryGetValue(id, out string s) ? s : null;
		}

		private static List<(int Colour, string Text)> _staff;

		/// <summary>The original development staff, as FF4.exe lists them (colour and text pairs at 0x5a9068: 0..18 the
		/// first screen's, 19 on the second's).</summary>
		private static (int Colour, string Text) Staff(int index)
		{
			_staff ??= ReadStaff();
			return index >= 0 && index < _staff.Count ? _staff[index] : (1, null);
		}

		private static List<(int, string)> ReadStaff()
		{
			List<(int, string)> staff = new List<(int, string)>();
			try
			{
				string path = Ff4Title.SteamFile("FF4.exe");
				if (path == null) return staff;
				byte[] exe = File.ReadAllBytes(path);
				int pe = BitConverter.ToInt32(exe, 0x3c);
				int sections = BitConverter.ToUInt16(exe, pe + 6), optional = BitConverter.ToUInt16(exe, pe + 20);
				uint imageBase = BitConverter.ToUInt32(exe, pe + 24 + 28);
				int Offset(uint va)
				{
					uint rva = va - imageBase;
					for (int s = 0; s < sections; s++)
					{
						int at = pe + 24 + optional + 40 * s;
						uint virtualSize = BitConverter.ToUInt32(exe, at + 8), virtualAddress = BitConverter.ToUInt32(exe, at + 12);
						uint rawSize = BitConverter.ToUInt32(exe, at + 16), raw = BitConverter.ToUInt32(exe, at + 20);
						if (rva >= virtualAddress && rva < virtualAddress + Math.Max(virtualSize, rawSize)) return (int)(raw + rva - virtualAddress);
					}
					return -1;
				}
				int table = Offset(0x5a9068);
				for (int i = 0; table >= 0 && i < 64; i++)
				{
					int colour = BitConverter.ToInt32(exe, table + 8 * i);
					int text = Offset(BitConverter.ToUInt32(exe, table + 8 * i + 4));
					if ((colour != 1 && colour != 10) || text < 0) break;
					int end = Array.IndexOf(exe, (byte)0, text);
					staff.Add((colour, System.Text.Encoding.UTF8.GetString(exe, text, end - text)));
				}
				if (staff.Count == 0 || staff[0].Item2 != "Original Development Staff") Log.Write(LogChannel.General, "script: FF4.exe's staff list not where this build keeps it");
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "script: FF4.exe's staff list: " + ex.Message); }
			return staff;
		}

		/// <summary>The texts for the layout (the staff roll's rows): each staff line centred on its spot, a message's lines
		/// centred about the middle.</summary>
		public static void Fill(Ff4MenuHud.Data h)
		{
			const float Scale = 3.375f, Left = 150f, LineHeight = 18f;
			h.Roll = Showing;
			foreach (Ff4MenuHud.RollRow row in h.RollRow) row.Present = false;
			int k = 0;
			void Put(string text, bool blue, float x, float y)
			{
				if (k >= h.RollRow.Count) return;
				Ff4MenuHud.RollRow row = h.RollRow[k++];
				row.Present = !string.IsNullOrEmpty(text);
				row.Text = text ?? "";
				row.Blue = blue;
				row.Centre = true;
				row.Right = false;
				row.Left = Left + x * Scale - 500f;
				row.Top = (y - LineHeight / 2) * Scale;
				row.Opacity = 1f;
			}
			foreach (Line l in _lines.Values) Put(l.Text, l.Blue, l.X, l.Y);
			if (!string.IsNullOrEmpty(_message))
			{
				string[] lines = _message.Replace("\r", "").Split('\n');
				for (int i = 0; i < lines.Length; i++) Put(lines[i], false, 240, 160 + (i - (lines.Length - 1) / 2f) * LineHeight);
			}
		}
	}
}

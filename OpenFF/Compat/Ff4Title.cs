// FF4's company logos and title screen, as the Steam FF4.exe has them.
//
// FF4 runs on FF3's part loop in this client, so its start is two parts in FF3's slots - the company
// logo (GAMEPART_CAMPANY_LOGO) and the title (GAMEPART_TITLE) - timed by the same CFade, read from the same
// pad and heard through the same sound calls; New Game and Load hand over to the jump part (JumpPart) as
// --map and --load did, so the world is entered the way it always was. What they show is FF4's own art,
// drawn by the client over the DS screens (Ff4Ui's sheets and cell banks), faded with the screens'
// brightness (CFade.Level). The game-over and "return to title" paths, which go to GAMEPART_TITLE, land
// here too.
//
// From FF4.exe (Ghidra, Reference/ff4exe; the Android libff4.so's names for the same code):
// - CompanyLogoSubState (FUN_005269d0 setup, FUN_00526b00 update): TITLE_Localize_Common.dat's se_logo
//   (entries 18/19) and mt_logo (12/13), each faded in over the default 15 frames, held 120 frames once
//   clear, faded out to black; then the movie (opening.mkv, beside FF4.exe), the game held under it (the main loop draws
//   the movie in place of the game, FUN_00441d30), and the title after it. The movie ends early on the Pause key (Esc),
//   a press of A, B, X or Y, any of the pad's buttons, or a click. Here MoviePlayer plays it.
// - Title2Ds::setup (FUN_00527c10): title_bg_00 (entries 22/23) as the background; TITLE_Localize.dat's
//   title_obj_00 cells - 2 the FINAL FANTASY IV logo, shown at (240, 94) of the 480 x 320 logical screen,
//   4 the copyright line at (240, LCD_HEIGHT / 2 + 148).
// - TitleContents::setup (FUN_005282e0): the commands CONTINUE (cell 3, when there is a suspend save),
//   NEW GAME (cell 0), LOAD GAME (cell 1, when there is a save) and QUIT GAME (cell 5), centred at x 240
//   from y (LCD_HEIGHT + 960) / 4 - 165 down in rows of 40; a missing LOAD GAME takes no row, a missing
//   CONTINUE keeps its own (empty) one - as the Steam title shows them (checked against the game at 1920 x 1080).
//   The title's BGM is 1.
// - TitleContents::update (FUN_00528120): Up/Down move through the shown commands, wrapping, with SE 0:3;
//   NEW GAME plays SE 0:1, stops the BGM over 20 frames and fades out over 30 to the new game; LOAD GAME
//   opens the load screen; QUIT GAME quits.
//
// Places: the Steam shell's logical screen is 480 x 320 (the phone's; the sheets are at 2x), widened for
// a wide window with the 480 in the middle. The port's 800 x 480 is that at 1.5: x = 40 + 1.5 lx, y = 1.5 ly,
// and a sheet pixel is 0.75 (Ff4Ui.Scale).
//
// The load screen is the port's for now - three slot windows over the title (Steam's LoadDisplayPart is
// not read yet) - and CONTINUE never shows: this client has no FF4 suspend save.

using System;
using System.Collections.Generic;

namespace OpenFF.Client
{
	internal static class Ff4Title
	{
		// By pack and entry, as FF4.exe opens them: a different title_obj_00.NCER lies loose beside the packs.
		private const string Common = "TITLE_Localize_Common.dat/", Localize = "TITLE_Localize.dat/";
		private const string ObjSheet = Localize + "title_obj_00.NCBR", ObjBank = Localize + "title_obj_00.NCER";
		private const string BgSheet = Common + "title_bg_00.NCGR", BgBank = Common + "title_bg_00.NSCR";
		private const int TitleBgm = 1;
		private const int DefaultFade = 15, LogoHold = 120;

		private const int Continue = 0, NewGame = 1, Load = 2, Quit = 3;
		/// <summary>The commands' cells in title_obj_00 (FF4.exe's table at 0x5b0900).</summary>
		private static readonly int[] CommandCells = { 3, 0, 1, 5 };

		/// <summary>A point of the 480 x 320 logical screen in the port's 800 x 480.</summary>
		private static float X(float lx) => 40f + 1.5f * lx;
		private static float Y(float ly) => 1.5f * ly;

		// What the parts put on the screen, read by Screen as it draws.
		private enum Showing { Nothing, Logo, Movie, Title }
		private static Showing _showing;
		private static string _logo;
		private static readonly bool[] _shown = new bool[4];
		private static readonly float[] _rowY = new float[4];
		private static int _cursor = -1;
		private static bool _loadOpen;
		private static int _loadCursor;

		public static void registerParts()
		{
			if (!GameProfile.IsFf4) return;
			GlobalScope.sys.GGlobal.registerPart(GlobalScope.GAMEPART.GAMEPART_CAMPANY_LOGO, LogoPart.Instance);
			GlobalScope.sys.GGlobal.registerPart(GlobalScope.GAMEPART.GAMEPART_TITLE, TitlePart.Instance);
		}

		private static void FadeIn(int frames)
		{
			GlobalScope.dgs.CFade.Main().fadeIn(frames);
			GlobalScope.dgs.CFade.Sub().fadeIn(frames);
		}

		private static void FadeOut(int frames, GlobalScope.dgs.CFade.FADE_TYPE type = GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK)
		{
			GlobalScope.dgs.CFade.Main().fadeOut(frames, type);
			GlobalScope.dgs.CFade.Sub().fadeOut(frames, type);
		}

		private static bool Faded => GlobalScope.dgs.CFade.Main().isFaded() && GlobalScope.dgs.CFade.Sub().isFaded();
		private static bool Cleared => GlobalScope.dgs.CFade.Main().isCleared() && GlobalScope.dgs.CFade.Sub().isCleared();

		private static void Se(int number)
		{
			try { GlobalScope.MatrixSound.MtxSENDS_Play(0, number, 127, 64); } catch (Exception) { }
		}

		/// <summary>CompanyLogoSubState: the Square Enix logo, then Matrix's, then the title.</summary>
		private sealed class LogoPart : GlobalScope.sys.FF3GamePart
		{
			public static readonly LogoPart Instance = new LogoPart();
			private int _state, _frames;
			private MoviePlayer _movie;
			private bool _mouseWas, _escWas;

			protected override void doInitialize()
			{
				GlobalScope.ds.CDevice.singleton().setFPS(GlobalScope.ds.CDevice.enFPS.enFPS_30);
				FadeOut(0);
				_state = 0;
				_frames = 0;
				// The logo comes up only once the screens are black: fadeOut(0) takes hold on the fade's next step, and
				// the first frames would show it at full brightness before its fade-in.
				_logo = null;
				_showing = Showing.Logo;
			}

			protected override void doUninitialize()
			{
				_movie?.Dispose();
				_movie = null;
				_showing = Showing.Nothing;
			}

			private void ToTitle()
			{
				_showing = Showing.Nothing;
				GlobalScope.sys.GGlobal.setNextPart(GlobalScope.GAMEPART.GAMEPART_TITLE);
				abort();
				_state = 6;
			}

			/// <summary>What ends the movie in FF4.exe: the Pause key (Esc), A / B / X / Y as the game reads them (0xC03), any
			/// of the pad's buttons, a click - each on its press.</summary>
			private bool SkipPressed()
			{
				bool esc = Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape);
				bool mouse = Microsoft.Xna.Framework.Input.Mouse.GetState().LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
				// A click counts only on the game's own window, it in front: one on a window over it is not the game's.
				bool active = true;
				try
				{
					Microsoft.Xna.Framework.Game game = GlobalScope.m_Graphics.getGame();
					Microsoft.Xna.Framework.Input.MouseState m = Microsoft.Xna.Framework.Input.Mouse.GetState();
					Microsoft.Xna.Framework.Rectangle client = game.Window.ClientBounds;
					active = game.IsActive && m.X >= 0 && m.Y >= 0 && m.X < client.Width && m.Y < client.Height;
				}
				catch (Exception) { }
				int pad = 0;
				try { pad = GlobalScope.ds.g_Pad.edge() & 0xC03; } catch (Exception) { }
				bool skip = (esc && !_escWas) || (active && mouse && !_mouseWas) || pad != 0;
				if (skip) Log.Write(LogChannel.General, "movie: skip by " + (pad != 0 ? "pad 0x" + pad.ToString("x") : esc && !_escWas ? "Esc" : "a click"));
				_escWas = esc;
				_mouseWas = mouse;
				return skip;
			}

			protected override void onExecutePart()
			{
				switch (_state)
				{
					case 0:
						if (Faded) { _logo = "se_logo"; FadeIn(DefaultFade); _state = 1; }
						break;
					case 1:
					case 3:
						if (Cleared && ++_frames >= LogoHold) { _frames = 0; FadeOut(DefaultFade); _state++; }
						break;
					case 2:
						if (Faded) { _logo = "mt_logo"; FadeIn(DefaultFade); _state = 3; }
						break;
					case 4:
						if (Faded)
						{
							string movie = SteamFile("opening.mkv");
							if (movie != null)
							{
								_movie = new MoviePlayer(movie);
								_movie.Start();
								_showing = Showing.Movie;
								try { GlobalScope.ds.g_Pad.enable(); } catch (Exception) { }
								_mouseWas = Microsoft.Xna.Framework.Input.Mouse.GetState().LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
								_escWas = Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape);
								_state = 5;
							}
							else ToTitle();
						}
						break;
					case 5:
						_movie.Update();
						if (_movie.Done || SkipPressed())
						{
							if (!_movie.Done) Log.Write(LogChannel.General, "movie: skipped");
							_movie.Dispose();
							_movie = null;
							ToTitle();
						}
						break;
				}
			}
		}

		/// <summary>A file of the Steam install beside FF4.exe (the content's files are two folders under it); null when it is not there.</summary>
		private static string SteamFile(string name)
		{
			try
			{
				string dir = System.IO.Path.GetFullPath(ContentLocator.FindContentRoot() ?? "");
				for (int up = 0; up < 4 && !string.IsNullOrEmpty(dir); up++)
				{
					string path = System.IO.Path.Combine(dir, name);
					if (System.IO.File.Exists(path)) return path;
					dir = System.IO.Path.GetDirectoryName(dir);
				}
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "ff4 title: looking for " + name + ": " + ex.Message); }
			return null;
		}

		/// <summary>TitleSubState and TitleContents: the background, the logo, the commands, and what a command does.</summary>
		private sealed class TitlePart : GlobalScope.sys.FF3GamePart
		{
			public static readonly TitlePart Instance = new TitlePart();
			private enum State { FadingIn, Menu, Leaving }
			private State _state;
			private GlobalScope.GAMEPART _next;

			protected override void doInitialize()
			{
				GlobalScope.ds.CDevice.singleton().setFPS(GlobalScope.ds.CDevice.enFPS.enFPS_30);
				FadeOut(0);
				// Come from a game's end (the field ended into it), the pad and the touch panel may still be held.
				try { GlobalScope.ds.g_Pad.enable(); GlobalScope.ds.g_TouchPanel.enable(); } catch (Exception) { }
				OpenFF.Game.Input.Capture = false;
				_shown[Continue] = false;          // no FF4 suspend save in this client
				_shown[NewGame] = true;
				_shown[Load] = AnySave();
				_shown[Quit] = true;
				// (LCD_HEIGHT + 960) / 4 - 165 for the first row; a missing LOAD GAME takes no row.
				float y = (320 + 960) / 4 - 165;
				for (int i = 0; i < 4; i++)
				{
					_rowY[i] = y;
					if (_shown[i] || i != Load) y += 40;
				}
				_cursor = Array.IndexOf(_shown, true);
				_loadOpen = false;
				_showing = Showing.Title;
				_state = State.FadingIn;
				try
				{
					GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().play(TitleBgm, 127, 0, GlobalScope.MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
				}
				catch (Exception ex) { Log.Write(LogChannel.General, "ff4 title: bgm " + TitleBgm + ": " + ex.Message); }
				Log.Write(LogChannel.General, "ff4 title: up" + (_shown[Load] ? ", a save to load" : ""));
			}

			protected override void doUninitialize()
			{
				_showing = Showing.Nothing;
				_loadOpen = false;
			}

			private static bool AnySave()
			{
				try
				{
					for (int slot = 1; slot <= Ff4Saves.SlotCount; slot++) if (Ff4Saves.Exists(slot)) return true;
				}
				catch (Exception) { }
				return false;
			}

			protected override void onExecutePart()
			{
				switch (_state)
				{
					case State.FadingIn:
						if (Faded) { FadeIn(DefaultFade); _state = State.Menu; }
						break;
					case State.Menu:
						if (_loadOpen) LoadInput();
						else MenuInput();
						break;
					case State.Leaving:
						if (Faded)
						{
							GlobalScope.sys.GGlobal.setNextPart(_next);
							abort();
						}
						break;
				}
			}

			private static ushort Repeat => GlobalScope.ds.g_Pad.repeat();
			private static ushort Edge => GlobalScope.ds.g_Pad.edge();

			private void MenuInput()
			{
				ushort repeat = Repeat, edge = Edge;
				if ((repeat & 0xC0) != 0 && _cursor >= 0)
				{
					int step = (repeat & 0x40) != 0 ? 3 : 1, at = _cursor;
					do at = (at + step) % 4; while (!_shown[at]);
					if (at != _cursor) { _cursor = at; Se(3); }
					return;
				}
				if ((edge & 1) == 0 || _cursor < 0) return;
				switch (_cursor)
				{
					case NewGame:
						Se(1);
						// A new game starts from nothing: the save file the engine read at boot (its last slot, a test's party
						// in it) is not this game's. FF4's initForNewgame: Cecil alone, no flags.
						Ff4Saves.Pending = null;
						Ff4Party.NewGame();
						if (GlobalScope.flags != null) Array.Clear(GlobalScope.flags);
						Leave(GlobalScope.GAMEPART.GAMEPART_DEBUG_MENU, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
						break;
					case Load:
						Se(1);
						_loadOpen = true;
						_loadCursor = 0;
						while (_loadCursor < Ff4Saves.SlotCount - 1 && !Ff4Saves.Exists(_loadCursor + 1)) _loadCursor++;
						break;
					case Quit:
						Se(1);
						Log.Write(LogChannel.General, "ff4 title: quit");
						try { GlobalScope.m_Graphics.getGame().Exit(); } catch (Exception) { Environment.Exit(0); }
						break;
				}
			}

			private void LoadInput()
			{
				ushort repeat = Repeat, edge = Edge;
				if ((repeat & 0xC0) != 0)
				{
					int at = (_loadCursor + ((repeat & 0x40) != 0 ? Ff4Saves.SlotCount - 1 : 1)) % Ff4Saves.SlotCount;
					if (at != _loadCursor) { _loadCursor = at; Se(3); }
					return;
				}
				if ((edge & 2) != 0) { _loadOpen = false; Se(2); return; }
				if ((edge & 1) == 0) return;
				int slot = _loadCursor + 1;
				if (!Ff4Saves.Exists(slot) || !Ff4Saves.Load(slot, true)) { Se(4); return; }
				Se(1);
				Leave(GlobalScope.GAMEPART.GAMEPART_DEBUG_MENU, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
			}

			/// <summary>
			/// Into the game: the BGM stopped over 20 frames, the screens faded over 30 - to white for a new game, as the Steam
			/// title has it (fadeOut(30, 1)) - then the jump part (a new game, or the slot just read). The fades are left as
			/// they are: the world fades a map in, or its event does (Ff4MapChange) - a new game's opening scene from white.
			/// </summary>
			private void Leave(GlobalScope.GAMEPART next, GlobalScope.dgs.CFade.FADE_TYPE fade)
			{
				_next = next;
				try { GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().stop(20, GlobalScope.MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0); } catch (Exception) { }
				FadeOut(30, fade);
				_state = State.Leaving;
				Log.Write(LogChannel.General, "ff4 title: " + (Ff4Saves.Pending != null ? "load, to " + Ff4Saves.Pending.Map : "new game"));
			}
		}

		/// <summary>Draws what the logo and title parts show, every step, faded with the screens.</summary>
		internal sealed class Screen : GameService
		{
			public override bool WantsUpdate => true;

			public override void OnUpdate()
			{
				if (_showing == Showing.Nothing) return;
				DrawList d = Game.Draw;
				if (_showing == Showing.Movie)
				{
					MoviePlayer.Current?.Draw(d);
					return;
				}
				d.Rect(0, 0, DrawList.ScreenWidth, DrawList.ScreenHeight, Color.Black);
				if (_showing == Showing.Logo) DrawLogo(d);
				else DrawTitle(d);
				int level = GlobalScope.dgs.CFade.Main().Level;
				if (level != 0)
				{
					byte a = (byte)Math.Min(255, Math.Abs(level) * 255 / 16);
					d.Rect(0, 0, DrawList.ScreenWidth, DrawList.ScreenHeight, level < 0 ? Color.Black.WithAlpha(a) : Color.White.WithAlpha(a));
				}
			}

			private static void DrawLogo(DrawList d)
			{
				if (_logo == null) return;
				Ff4Ui.Cell(d, Common + _logo + ".NSCR", Common + _logo + ".NCGR", 0, X(240), Y(160));
			}

			private static void DrawTitle(DrawList d)
			{
				Ff4Ui.Cell(d, BgBank, BgSheet, 0, X(240), Y(160));
				Ff4Ui.Cell(d, ObjBank, ObjSheet, 2, X(240), Y(94));
				Ff4Ui.Cell(d, ObjBank, ObjSheet, 4, X(240), Y(320 / 2 + 148));
				for (int i = 0; i < 4; i++)
				{
					if (_shown[i]) Ff4Ui.Cell(d, ObjBank, ObjSheet, CommandCells[i], X(240), Y(_rowY[i]));
				}
				// The glove at the command's left end (the cell's 144 sheet pixels left of its middle), level with it.
				if (_cursor >= 0 && !_loadOpen) Ff4Ui.Glove(d, X(240) - 144f * Ff4Ui.Scale, Y(_rowY[_cursor]));
				if (_loadOpen) DrawLoad(d);
			}

			private static void DrawLoad(DrawList d)
			{
				const float w = 520f, h = 80f, gap = 12f;
				float x = (DrawList.ScreenWidth - w) / 2f, top = (DrawList.ScreenHeight - (h * Ff4Saves.SlotCount + gap * (Ff4Saves.SlotCount - 1))) / 2f;
				d.Rect(0, 0, DrawList.ScreenWidth, DrawList.ScreenHeight, new Color(0, 0, 0, 110));
				for (int i = 0; i < Ff4Saves.SlotCount; i++)
				{
					float y = top + i * (h + gap);
					if (!Ff4Ui.Window(d, x, y, w, h)) d.Rect(x, y, w, h, Ff4Ui.Fill);
					bool exists = Ff4Saves.Exists(i + 1);
					Color text = exists ? Color.White : new Color(150, 150, 170);
					Shadowed(d, "File " + (i + 1), x + 24, y + 12, text);
					Shadowed(d, Ff4Saves.Describe(i + 1), x + 24, y + 42, text, 14);
					if (i == _loadCursor) Ff4Ui.Glove(d, x + 6, y + h / 2);
				}
			}

			private static void Shadowed(DrawList d, string text, float x, float y, Color color, int size = 16)
			{
				d.Text(text, x + 1, y + 1, new Color(0, 0, 0, 200), size);
				d.Text(text, x, y, color, size);
			}
		}
	}
}

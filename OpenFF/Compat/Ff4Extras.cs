// FF4's extras, as Steam has them: the Fat Chocobo's window (shiftState chokobo_menu - world::WSChokoboMenu with
// WSCMWindow over the field) and the parts it calls - the Bestiary (part 14, also call_monster_ref), the Event Theatre (15,
// call_theater) and the Music Box (22, call_music_player). Drawn through the menu's layout (Ff4MenuHud: menu.launcher,
// menu.extrasList), the field held under them as the menu holds it.
//
// The window's three rows are open once their flags are (0/750 Bestiary, 751 Event Theatre, 752 Music Box), "??????" and
// shut until then (WSCMWindow::openedCallback: babil_menu.msd 51501..51503, 51599). The Music Box (music::MPSubState)
// lists the songs music_player.bbd holds whose flag is up - 12-byte records: the BGM, its flag (601...), the name's
// babil_music.msd id, the length in seconds - a song's flag set as its BGM first loads (setFlagOnLoadBGMHook). A song chosen
// plays for its length; then Play one stops it (faded over 15 frames), Play all goes on down the list, and Looping starts
// the one again or the list over (settingBGM, updatePlaying).
//
// The Event Theatre (et::ETSubState) lists theater_data.bbd's scenes whose flag is up - 8-byte records: the event and its
// part (eNN_PP), the flag (701...; each scene sets its own as it plays), the name's babil_theater.msd id. One chosen plays
// with flag 0/700 up (the scenes' scripts pass over their story's business on it); its end comes back here rather than
// going on with the story (ContEventPart::eventEnd), the party where it stood. Back puts 700 down.

using System;
using System.Collections.Generic;
using OpenFF;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed class Ff4Extras : GameService
	{
		private enum Page { None, Launcher, Music, Theatre, Bestiary }

		private static Ff4Extras _instance;
		private static Page _pending;

		private Page _page;
		private int _cursor, _scroll;
		private readonly Ff4MenuHud.Data _hud = new Ff4MenuHud.Data();

		public Ff4Extras() { _instance = this; }

		public override bool WantsUpdate => true;

		/// <summary>The extras are up, or one is waiting for the event to let go.</summary>
		public static bool Busy => _pending != Page.None || (_instance != null && _instance._page != Page.None);

		/// <summary>shiftState's names for them; false for another state.</summary>
		public static bool Request(string state)
		{
			switch (state)
			{
				case "chokobo_menu": _pending = Page.Launcher; return true;
				case "call_music_player": _pending = Page.Music; return true;
				case "call_theater": _pending = Page.Theatre; return true;
				case "call_monster_ref": _pending = Page.Bestiary; return true;
				default: return false;
			}
		}

		// ---- the songs ----

		private sealed class Song
		{
			public int Bgm, Flag, Seconds;
			public uint Name;
		}

		private static List<Song> _songs;

		private static List<Song> Songs
		{
			get
			{
				if (_songs != null) return _songs;
				_songs = new List<Song>();
				try
				{
					if (GameArchive.Chain != null && TableFiles.ReadAny(GameArchive.Chain, "music_player.bbd", out byte[] d))
					{
						for (int at = 0; at + 10 <= d.Length; at += 12)
						{
							_songs.Add(new Song
							{
								Bgm = BitConverter.ToUInt16(d, at),
								Flag = BitConverter.ToUInt16(d, at + 2),
								Name = BitConverter.ToUInt32(d, at + 4),
								Seconds = BitConverter.ToUInt16(d, at + 8),
							});
						}
					}
				}
				catch (Exception ex) { Log.Write(LogChannel.General, "extras: music_player.bbd: " + ex.Message); }
				return _songs;
			}
		}

		private static int _lastBgm = -1;

		/// <summary>setFlagOnLoadBGMHook: a BGM starting sets its song's flag (the Music Box lists it from then on).</summary>
		public static void BgmStarted(int bgm)
		{
			_lastBgm = bgm;
			foreach (Song s in Songs)
			{
				if (s.Bgm != bgm) continue;
				SetFlag(s.Flag);
				return;
			}
		}

		internal static bool Flag(int index)
		{
			byte[,] flags = GlobalScope.flags;
			return flags != null && index >= 0 && index < flags.GetLength(1) && flags[0, index] != 0;
		}

		private static void SetFlag(int index)
		{
			byte[,] flags = GlobalScope.flags;
			if (flags != null && index >= 0 && index < flags.GetLength(1)) flags[0, index] = 1;
		}

		private static void Se(int number) => Game.Audio.PlaySe(0, number);   // playSECommon: 0 buzzer, 1 decide, 2 cancel, 3 cursor

		// ---- the scenes ----

		private sealed class Scene
		{
			public int Event, Part, Flag;
			public uint Name;
		}

		private static List<Scene> _scenes;

		private static List<Scene> Scenes
		{
			get
			{
				if (_scenes != null) return _scenes;
				_scenes = new List<Scene>();
				try
				{
					if (GameArchive.Chain != null && TableFiles.ReadAny(GameArchive.Chain, "theater_data.bbd", out byte[] d))
					{
						for (int at = 0; at + 8 <= d.Length; at += 8)
						{
							_scenes.Add(new Scene { Event = d[at], Part = d[at + 1], Flag = BitConverter.ToUInt16(d, at + 2), Name = BitConverter.ToUInt32(d, at + 4) });
						}
					}
				}
				catch (Exception ex) { Log.Write(LogChannel.General, "extras: theater_data.bbd: " + ex.Message); }
				return _scenes;
			}
		}

		private const int TheatreFlag = 700;
		private List<Scene> _seen = new List<Scene>();
		private static int _theatreCursor, _theatreScroll;
		private static string _theatreMap;
		private static GlobalScope.VecFx32 _theatrePosition;

		/// <summary>A scene ends: one the Event Theatre played goes back to where the party stood and the list comes up again
		/// (ContEventPart::eventEnd with flag 700 up: part 15, no battle after it). False when the theatre is not playing.</summary>
		public static bool TheatreSceneEnds()
		{
			if (!Flag(TheatreFlag) || _theatreMap == null) return false;
			Log.Write(LogChannel.General, "extras: the theatre's scene is over - back to " + _theatreMap);
			Ff4Cutscene.ReturnFromTheatre(_theatreMap, _theatrePosition);
			_theatreMap = null;
			_pending = Page.Theatre;
			return true;
		}

		private void PlayScene(Scene scene)
		{
			string name = "e" + scene.Event.ToString("00") + "_" + scene.Part.ToString("00");
			_theatreCursor = _cursor;
			_theatreScroll = _scroll;
			_theatreMap = Game.Field.Map;
			try { GlobalScope.VecFx32 at = EngineApi.HeroPlayer?.getPosition(); _theatrePosition = at == null ? null : new GlobalScope.VecFx32(at.x, at.y, at.z); }
			catch (Exception) { _theatrePosition = null; }
			SetFlag(TheatreFlag);
			_page = Page.None;
			Game.Input.Capture = false;
			Log.Write(LogChannel.General, "extras: the theatre plays " + name + " (from " + _theatreMap + ")");
			Ff4Cutscene.PlayFromTheatre(name);
		}

		// ---- opening and closing ----

		private int _fieldBgm = -1;
		private List<Song> _heard = new List<Song>();
		private bool _loop, _playAll;
		private int _playing = -1;          // the index in _heard sounding, -1 none
		private double _playedFor;          // seconds into it
		private DateTime _lastTick;

		private void Open(Page page)
		{
			_page = page;
			_cursor = _scroll = 0;
			Game.Input.Capture = true;
			if (page == Page.Music)
			{
				_fieldBgm = _lastBgm;
				_heard = Songs.FindAll(s => Flag(s.Flag));
				_playing = -1;
				// MPSubState unloads the field's sound: its tune stops as the Music Box comes up.
				try { GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().stop(0, GlobalScope.MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0); } catch (Exception) { }
			}
			if (page == Page.Theatre)
			{
				_seen = Scenes.FindAll(s => Flag(s.Flag));
				_cursor = Math.Min(_theatreCursor, Math.Max(0, _seen.Count - 1));
				_scroll = Math.Min(_theatreScroll, _cursor);
				_theatreCursor = _theatreScroll = 0;
			}
			Log.Write(LogChannel.File, "extras: " + page);
		}

		private void Close()
		{
			Page was = _page;
			_page = Page.None;
			Game.Input.Capture = false;
			if (was == Page.Music)
			{
				StopSong(0);
				// Back on the field: its tune again (the part going back sets the map's up).
				if (_fieldBgm > 0) { try { GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().play(_fieldBgm, 127, 0, GlobalScope.MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0); } catch (Exception) { } }
			}
			Log.Write(LogChannel.File, "extras: closed " + was);
		}

		// ---- each frame ----

		public override void OnUpdate()
		{
			if (_page == Page.None)
			{
				if (_pending == Page.None || !EngineApi.InWorld || Ff4Menu.EventBusy() || Ff4Cutscene.Active) return;
				Page p = _pending;
				_pending = Page.None;
				Open(p);
			}
			InputState input = Game.Input;
			switch (_page)
			{
				case Page.Launcher: UpdateLauncher(input); break;
				case Page.Music: UpdateMusic(input); break;
				case Page.Theatre: UpdateTheatre(input); break;
				default:
					if (input.Pressed(Pad.B)) { Se(2); Close(); }
					break;
			}
			if (_page != Page.None) Draw();
		}

		private void UpdateLauncher(InputState input)
		{
			if (input.Pressed(Pad.Up)) { _cursor = (_cursor + 2) % 3; Se(3); }
			if (input.Pressed(Pad.Down)) { _cursor = (_cursor + 1) % 3; Se(3); }
			if (input.Pressed(Pad.B)) { Se(2); Close(); return; }
			if (input.Pressed(Pad.A))
			{
				// WSCMWindow::update: a row whose flag is down only buzzes.
				if (!Flag(750 + _cursor)) { Se(0); return; }
				Se(1);
				Page next = _cursor == 0 ? Page.Bestiary : _cursor == 1 ? Page.Theatre : Page.Music;
				Close();
				Open(next);
			}
		}

		private void UpdateMusic(InputState input)
		{
			int n = _heard.Count;
			if (n > 0)
			{
				if (input.Pressed(Pad.Up) && _cursor > 0) { _cursor--; Se(3); }
				if (input.Pressed(Pad.Down) && _cursor < n - 1) { _cursor++; Se(3); }
				if (_cursor < _scroll) _scroll = _cursor;
				if (_cursor >= _scroll + 8) _scroll = _cursor - 7;
			}
			if (input.Pressed(Pad.Y)) { _loop = !_loop; Se(1); }                 // Z: Looping
			if (input.KeyPressed("M")) { _playAll = !_playAll; Se(1); }          // M: Mode
			if (input.KeyPressed("Tab") && _playing >= 0) { StopSong(15); Se(1); }
			if (input.Pressed(Pad.B)) { Se(2); Close(); return; }
			if (input.Pressed(Pad.A) && n > 0) { Se(1); PlaySong(_cursor); }
			TickSong();
		}

		private void UpdateTheatre(InputState input)
		{
			int n = _seen.Count;
			if (n > 0)
			{
				if (input.Pressed(Pad.Up) && _cursor > 0) { _cursor--; Se(3); }
				if (input.Pressed(Pad.Down) && _cursor < n - 1) { _cursor++; Se(3); }
				if (_cursor < _scroll) _scroll = _cursor;
				if (_cursor >= _scroll + 8) _scroll = _cursor - 7;
			}
			if (input.Pressed(Pad.B))
			{
				// ETSubState: Back puts the theatre's flag down and goes back to the field.
				byte[,] flags = GlobalScope.flags;
				if (flags != null) flags[0, TheatreFlag] = 0;
				Se(2);
				Close();
				return;
			}
			if (input.Pressed(Pad.A) && n > 0)
			{
				if (!Flag(_seen[_cursor].Flag)) { Se(0); return; }
				Se(1);
				PlayScene(_seen[_cursor]);
			}
		}

		private void PlaySong(int index)
		{
			_playing = index;
			_playedFor = 0;
			_lastTick = DateTime.UtcNow;
			try { GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().play(_heard[index].Bgm, 127, 0, GlobalScope.MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0); } catch (Exception) { }
		}

		private void StopSong(int fade)
		{
			if (_playing < 0) return;
			_playing = -1;
			try { GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().stop(fade, GlobalScope.MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0); } catch (Exception) { }
		}

		/// <summary>The song's time run out: Play one stops (looping, starts it again); Play all goes to the next (looping, from
		/// the top after the last).</summary>
		private void TickSong()
		{
			if (_playing < 0) return;
			DateTime now = DateTime.UtcNow;
			_playedFor += (now - _lastTick).TotalSeconds;
			_lastTick = now;
			if (_playedFor < _heard[_playing].Seconds) return;
			if (!_playAll)
			{
				if (_loop) PlaySong(_playing); else StopSong(15);
				return;
			}
			int next = _playing + 1;
			if (next >= _heard.Count)
			{
				if (!_loop) { StopSong(15); return; }
				next = 0;
			}
			PlaySong(next);
		}

		// ---- drawing ----

		private static string Music(uint id, string fallback) => Ff4Layouts.Page("babil_music.msd", id, fallback);

		private void Draw()
		{
			Ff4MenuHud.Data h = _hud;
			h.Launcher = _page == Page.Launcher;
			h.Full = _page == Page.Music || _page == Page.Theatre || _page == Page.Bestiary;
			h.ExtrasList = _page == Page.Music || _page == Page.Theatre;
			h.MusicIdle = _page == Page.Music && _playing < 0;
			h.MusicPlaying = _page == Page.Music && _playing >= 0;
			h.Theatre = _page == Page.Theatre;
			if (h.Launcher)
			{
				for (int k = 0; k < 3; k++)
				{
					Ff4MenuHud.CommandRow row = h.Extra[k];
					row.Present = true;
					row.Lit = k == _cursor;
					row.Name = Flag(750 + k) ? Ff4Layouts.Text((uint)(51501 + k), k == 0 ? "Bestiary" : k == 1 ? "Event Theatre" : "Music Box") : Ff4Layouts.Text(51599, "??????");
				}
			}
			if (_page == Page.Music)
			{
				h.Title = Music(80000, "Music Box");
				h.LoopLabel = Music(_loop ? 81004u : 81003u, _loop ? "Looping [On]" : "Looping [Off]");
				h.ModeLabel = Music(_playAll ? 81006u : 81005u, _playAll ? "Mode [Play all]" : "Mode [Play one]");
				h.StopLabel = Music(81001, "Stop");
				for (int k = 0; k < 8; k++)
				{
					int i = _scroll + k;
					Ff4MenuHud.CommandRow row = h.ListRow[k];
					row.Present = i < _heard.Count;
					row.Lit = i == _cursor;
					row.Name = row.Present ? Music(_heard[i].Name, "") : "";
				}
				Ff4MenuHud.Scroll(h.ListScroll, _heard.Count, 8, _scroll, 864 - 67.5f, always: true);
			}
			if (_page == Page.Theatre)
			{
				h.Title = Ff4Layouts.Text(51502, "Event Theatre");
				for (int k = 0; k < 8; k++)
				{
					int i = _scroll + k;
					Ff4MenuHud.CommandRow row = h.ListRow[k];
					row.Present = i < _seen.Count;
					row.Lit = i == _cursor;
					row.Name = row.Present ? Ff4Layouts.Page("babil_theater.msd", _seen[i].Name, "") : "";
				}
				Ff4MenuHud.Scroll(h.ListScroll, _seen.Count, 8, _scroll, 864 - 67.5f, always: true);
			}
			Ff4MenuHud.Draw(h);
		}
	}
}

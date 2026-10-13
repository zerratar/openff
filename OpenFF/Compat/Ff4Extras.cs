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
		private enum Page { None, Launcher, Music, Theatre, Bestiary, BestiaryPage }

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
			if (page == Page.Bestiary) OpenBestiary();
			Log.Write(LogChannel.File, "extras: " + page);
		}

		private static GlobalScope.VecFx32 _savedPos, _savedTrg;

		/// <summary>saveCameraPosAndTarget (WSCMenu::wscmSaveCameraLocation): the camera as it stands, before the script turns it
		/// on the Fat Chocobo - where the window's closing takes it back to.</summary>
		public static void SaveCamera(GlobalScope.ScriptEngine engine)
		{
			engine.getDword();
			engine.getDword();
			GlobalScope.VecFx32 p = GlobalScope.NNS_G3dGlb.camPos, t = GlobalScope.NNS_G3dGlb.camTarget;
			_savedPos = p == null ? null : new GlobalScope.VecFx32(p.x, p.y, p.z);
			_savedTrg = t == null ? null : new GlobalScope.VecFx32(t.x, t.y, t.z);
		}

		private void Close()
		{
			Page was = _page;
			if (was == Page.Launcher && _savedPos != null) { Ff4EventCamera.EaseBack(_savedPos, _savedTrg); _savedPos = _savedTrg = null; }
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
				case Page.Bestiary: UpdateBestiary(input); break;
				case Page.BestiaryPage: UpdatePage(input); break;
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
				// The part change takes the field down; coming back sets its camera up again (no easing back).
				_savedPos = _savedTrg = null;
				Ff4EventCamera.Release();
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

		// ---- the Bestiary ----

		private List<Ff4Bestiary.Entry> _listed = new List<Ff4Bestiary.Entry>();

		private static int _bestCursor, _bestScroll;

		private void OpenBestiary()
		{
			_listed = Ff4Bestiary.Book.FindAll(e => Ff4Bestiary.Listed(e.Monster));
			_cursor = Math.Min(_bestCursor, Math.Max(0, _listed.Count - 1));
			_scroll = Math.Min(_bestScroll, _cursor);
			_bestCursor = _bestScroll = 0;
		}

		// The page: the monster on its battle stage (MRStageLoader, MCBattleMonster), Steam's camera for it (measured: at
		// (0, 60, 196) looking at (0, 0, -34), a 16-degree field of view), its battle BGM (MCBattleBGM, the record's +4).
		private int _viewing = -1;          // the index in _listed shown
		private Npc _viewNpc;
		private bool _viewWaiting, _viewAttacking;
		private int _viewBgm = -1;

		private void OpenPage(int index)
		{
			Ff4Bestiary.Entry e = _listed[index];
			_viewing = index;
			_page = Page.BestiaryPage;
			RemoveViewNpc();
			_viewWaiting = true;
			if (!Ff4BattleStage.Viewing)
			{
				_fieldBgm = _lastBgm;
				_bestCursor = index;
				_bestScroll = _scroll;
				if (!Ff4BattleStage.BeginView(e.Stage)) { _viewWaiting = false; Log.Write(LogChannel.General, "extras: no stage b" + e.Stage.ToString("00") + " for the bestiary"); }
			}
			else if (Ff4BattleStage.BattleMap != e.Stage) Ff4BattleStage.SwitchView(e.Stage);
			else _viewWaiting = false;
			if (!_viewWaiting) SetUpView();
			Log.Write(LogChannel.File, "extras: bestiary page " + e.Monster + " on b" + e.Stage.ToString("00"));
		}

		private void SetUpView()
		{
			Ff4Bestiary.Entry e = _listed[_viewing];
			MonsterDefinition m = Ff4Bestiary.Definition(e.Monster);
			try { EngineApi.HeroPlayer?.setHidden(true); } catch (Exception) { }
			// MCBattleMonster::setup: the record's turn (+8, degrees) and place (+0xc less 12, +0x10, +0x14).
			Vector3 at = new Vector3((e.Camera[1] - 0xC000) / 4096f, e.Camera[2] / 4096f, e.Camera[3] / 4096f);
			float facing = e.Camera[0] / 4096f;
			if (m != null) _viewNpc = Ff4Battle.Instance?.SpawnForView(m, at, facing);
			_viewAttacking = false;
			Ff4EventCamera.MoveTo(0, 60 * 4096, 196 * 4096, 0, false);
			Ff4EventCamera.LookAt(0, 0, -34 * 4096, 0);
			Ff4EventCamera.SetFov(16.04f);
			Ff4EventCamera.SetClip(Ff4BattleStage.ClipNear, Ff4BattleStage.ClipFar);
			if (e.Bgm != _viewBgm)
			{
				_viewBgm = e.Bgm;
				try { GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().play(e.Bgm, 127, 0, GlobalScope.MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0); } catch (Exception) { }
			}
			GlobalScope.dgs.CFade.Main().fadeIn(5);
			GlobalScope.dgs.CFade.Sub().fadeIn(5);
		}

		private void RemoveViewNpc()
		{
			if (_viewNpc == null) return;
			try { _viewNpc.Remove(); } catch (Exception) { }
			_viewNpc = null;
		}

		private void UpdatePage(InputState input)
		{
			if (_viewWaiting)
			{
				if (Ff4BattleStage.Pending && Ff4BattleStage.Arrived) { Ff4BattleStage.Arrive(); _viewWaiting = false; SetUpView(); }
				return;
			}
			if (input.Pressed(Pad.B))
			{
				Se(2);
				RemoveViewNpc();
				Ff4EventCamera.Release();
				Ff4BattleStage.Leave();
				_viewBgm = -1;
				_page = Page.None;
				Game.Input.Capture = false;
				if (_fieldBgm > 0) { try { GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().play(_fieldBgm, 127, 0, GlobalScope.MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0); } catch (Exception) { } }
				_pending = Page.Bestiary;   // the list again, once the field is back
				return;
			}
			// Z and M: the previous and the next monster met.
			int step = input.Pressed(Pad.Y) ? -1 : input.KeyPressed("M") ? 1 : 0;
			if (step != 0)
			{
				for (int k = 1; k < _listed.Count; k++)
				{
					int i = ((_viewing + step * k) % _listed.Count + _listed.Count) % _listed.Count;
					if (!Ff4Bestiary.Met(_listed[i].Monster)) continue;
					if (i != _viewing) { Se(3); Ff4Bestiary.Seen(_listed[i].Monster); _bestCursor = i; OpenPage(i); }
					break;
				}
				return;
			}
			// Enter: MCBattleMonster::startAttackMotion - motion 201, unless the record's flag 0 is up; then the stance again.
			if (input.Pressed(Pad.A) && _viewNpc != null && !_viewAttacking && (_listed[_viewing].Flags & 1) == 0)
			{
				_viewAttacking = true;
				try { _viewNpc.PlayMotion(201, false, 5); Log.Write(LogChannel.File, "extras: bestiary action 201"); } catch (Exception ex) { _viewAttacking = false; Log.Write(LogChannel.File, "extras: bestiary action: " + ex.Message); }
			}
			if (_viewAttacking && _viewNpc != null && _viewNpc.MotionDone)
			{
				_viewAttacking = false;
				try { _viewNpc.PlayMotion(101, true, 10); Log.Write(LogChannel.File, "extras: bestiary action done"); } catch (Exception) { }
			}
		}

		private void DrawPage(Ff4MenuHud.Data h)
		{
			Ff4Bestiary.Entry e = _listed[_viewing];
			MonsterDefinition m = Ff4Bestiary.Definition(e.Monster);
			int mania = Ff4Bestiary.Mania(e.Monster);
			bool felled = mania > 0xF;
			string unknown = MonsterRef(60011, "??????"), little = MonsterRef(60010, "???");
			h.BestName = m?.Name ?? "";
			h.LvLabel = Ff4Layouts.Text(50401, "Lv");
			h.SlayLabel = MonsterRef(60005, "Slay Count");
			h.ExpWord = Ff4Layouts.Text(50451, "EXP");
			h.GilLabel = Ff4Layouts.Text(50446, "Gil");
			h.TreasureLabel = MonsterRef(60009, "Treasure");
			h.AreaLabel = MonsterRef(61000, "Area");
			h.ChangeMonstersLabel = Ff4Menu.KeyText(60214, "Change Monsters");
			h.PlayActionLabel = Ff4Menu.KeyText(60215, "Play Action");
			// su_AfterLoad: the figures once one has been felled ("???" before; a record's flag 1 hides the level).
			h.BestLv = felled ? ((e.Flags & 2) != 0 ? little : (m?.Level ?? 0).ToString()) : little;
			h.BestSlay = felled ? (mania >> 4).ToString() : little;
			h.BestExp = felled ? (m == null || m.Experience < 0 ? "------" : m.Experience.ToString()) : little;
			h.BestGil = felled ? (m == null || m.Gil < 0 ? "-------" : m.Gil.ToString()) : unknown;
			int line = 0;
			for (int k = 0; k < 4; k++) h.BestItem[k].Name = felled ? "" : unknown;
			if (felled && m?.Raw != null)
			{
				for (int d = 0; d < 4 && line < 4; d++)
				{
					int item = BitConverter.ToInt16(m.Raw, 0x6C + 4 * d);
					if (item <= 0) continue;
					string text = Ff4Chests.ItemString(item);
					if (!string.IsNullOrEmpty(text)) h.BestItem[line++].Name = text;
				}
			}
			h.BestArea = felled ? MonsterRef(e.Area, "") : unknown;
		}

		private void UpdateBestiary(InputState input)
		{
			int n = _listed.Count;
			if (n > 0)
			{
				if (input.Pressed(Pad.Up) && _cursor > 0) { _cursor--; Se(3); }
				if (input.Pressed(Pad.Down) && _cursor < n - 1) { _cursor++; Se(3); }
				if (_cursor < _scroll) _scroll = _cursor;
				if (_cursor >= _scroll + 5) _scroll = _cursor - 4;
			}
			if (input.Pressed(Pad.B)) { Se(2); Close(); return; }
			if (input.Pressed(Pad.A) && n > 0)
			{
				// decideMonster: a monster not met yet only buzzes.
				if (!Ff4Bestiary.Met(_listed[_cursor].Monster)) { Se(0); return; }
				Se(1);
				Ff4Bestiary.Seen(_listed[_cursor].Monster);
				OpenPage(_cursor);
			}
		}

		private static string MonsterRef(uint id, string fallback) => Ff4Layouts.Page("babil_monster_ref.msd", id, fallback);

		private void DrawBestiary(Ff4MenuHud.Data h)
		{
			h.Title = Ff4Layouts.Text(51501, "Bestiary");
			// 60002 "%SCC0%%percent% complete": the rate in standard code 0 (setCompleteRate), then the sign.
			h.CompleteText = MonsterRef(60002, "%SCC0%%percent% complete").Replace("%SCC0%", Ff4Bestiary.Complete().ToString()).Replace("%percent%", "%");
			string unknown = MonsterRef(60011, "??????");
			for (int k = 0; k < 6; k++)
			{
				int i = _scroll + k;
				Ff4MenuHud.BestRow row = h.BestRow[k];
				row.Present = i < _listed.Count;
				row.Lit = i == _cursor;
				if (!row.Present) continue;
				int id = _listed[i].Monster;
				row.Number = (i + 1).ToString();
				row.Name = Ff4Bestiary.Met(id) ? (Ff4Bestiary.Definition(id)?.Name ?? "") : unknown;
				row.Fresh = Ff4Bestiary.New(id);
			}
			Ff4MenuHud.Scroll(h.BestScroll, _listed.Count, 5, _scroll, 756 - 67.5f, always: true);
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
			h.Bestiary = _page == Page.Bestiary;
			if (h.Bestiary) DrawBestiary(h);
			h.BestPage = _page == Page.BestiaryPage && !_viewWaiting;
			if (h.BestPage) DrawPage(h);
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

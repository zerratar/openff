// The engine API on the legacy game.
//
// OpenFF.Engine/Api.cs says what a script may do; these services do it by calling what
// the FF3 script command handlers call (GlobalScope.Members.cs, ff3Command_*): the
// player manager for characters, the field's message window for text, FlagManager for
// flags, PlayerParty for money and items, MatrixSound for sound, CFade for fades, the
// cast transit's map jump for warps. Everything is reached through
// CCastCommandTransit, which is only wired while a map is up, so each verb is a no-op
// (returning nothing, logging once) outside one.
//
// Registered before the mods load, so a mod can Get<T>() them at OnGameStart; a mod
// registering its own IDialogue later in the order replaces this one for everybody.

using System;
using System.Collections.Generic;
using System.Linq;
using OpenFF;

namespace OpenFF.Client
{
	internal static class EngineApi
	{
		public static readonly LegacyDialogue Dialogue = new LegacyDialogue();
		public static readonly LegacyNpcs Npcs = new LegacyNpcs();
		public static readonly LegacyHero Hero = new LegacyHero();

		public static void Register()
		{
			OpenFF.Game.Services.Register(Dialogue);
			OpenFF.Game.Services.Register(Hero);
			OpenFF.Game.Services.Register(Npcs);
			OpenFF.Game.Services.Register(new LegacyFlags());
			// FF4's party is the unified OpenFF.Data.Party (Ff4Party); FF3's is still pl.PlayerParty.
			if (GameProfile.IsFf4)
			{
				Ff4PartyService ff4Party = new Ff4PartyService();
				OpenFF.Game.Services.Register(ff4Party);
				// FF4 saves through the engine's chunks (Ff4Saves): the party and the field state.
				OpenFF.Game.Saves.Register(ff4Party);
				OpenFF.Game.Saves.Register(new Ff4FieldState());
			}
			else OpenFF.Game.Services.Register(new LegacyParty());
			OpenFF.Game.Services.Register(new LegacyAudio());
			OpenFF.Game.Services.Register(Screen);
			OpenFF.Game.Services.Register(new LegacyField());
			OpenFF.Game.Services.Register(new LegacyCamera());
			OpenFF.Game.Services.Register(Effects);
			OpenFF.Game.Services.Register(new LegacyBattle());
			OpenFF.Game.Services.Register(Magic);
			if (GameProfile.IsFf4) OpenFF.Game.Services.Register(new Ff4Monsters());
			else OpenFF.Game.Services.Register(new LegacyMonsters());
			if (GameProfile.IsFf4) OpenFF.Game.Services.Register(new Ff4Items());
			else OpenFF.Game.Services.Register(new LegacyItems());
			// FF3's menu part cannot read FF4's party; the engine draws a status menu for FF4 (Ff4Menu).
			if (GameProfile.IsFf4) OpenFF.Game.Services.Register(new Ff4Menu());
			// ... and the OpenFF battle for FF4 (Ff4Battle), on the unified party and monsters.
			if (GameProfile.IsFf4) OpenFF.Game.Services.Register(new Ff4Battle());
			// FF4's shops are drawn by the engine from the unified tables (Ff4Shop); FF3 keeps its own screen.
			if (GameProfile.IsFf4) OpenFF.Game.Services.Register(new Ff4Shop());
			else OpenFF.Game.Services.Register(new LegacyShops());
		}

		public static readonly LegacyScreen Screen = new LegacyScreen();
		public static readonly LegacyEffects Effects = new LegacyEffects();
		public static readonly LegacyMagic Magic = new LegacyMagic();

		/// <summary>Once per frame, after the legacy tick: the message window, the hero's scripted walk, and spawned characters.</summary>
		public static void Tick()
		{
			Dialogue.Tick();
			Hero.Tick();
			Npcs.Tick();
			Effects.Tick();
			Screen.Tick();
			Magic.Tick();
			Ff4FieldCommands.Tick();
			Ff4CameraMotion.Tick();
			Ff4EventCamera.Tick();
			Ff4Cutscene.Tick();
			Ff4Saves.Tick();
		}

		// ---- reaching the legacy world ----

		internal static bool InWorld
		{
			get
			{
				GlobalScope.CCastCommandTransit transit = GlobalScope.CCastCommandTransit.getInstance();
				if (transit.cast_BaseSystem() == null)
				{
					return false;
				}
				try { return transit.cast_PlayerMng() != null; }
				catch (Exception) { return false; }
			}
		}

		internal static GlobalScope.pl.CPlayerManager Players => GlobalScope.CCastCommandTransit.getInstance().cast_PlayerMng();

		/// <summary>
		/// bootCharacterImp's last word: the map's script booted cast <paramref name="cast"/>
		/// into player slot <paramref name="slot"/>. Published as Events.CastBooted so a mod's
		/// stand-in for the cast can take over; nothing when the engine is not up. Never throws
		/// into the script.
		/// </summary>
		internal static void CastBooted(int cast, int slot)
		{
			if (!OpenFF.Game.Started || cast <= 0 || slot < 0) return;
			try
			{
				Npc character = Npcs.Existing(slot);
				Log.Write(LogChannel.File, "engine api: cast " + cast + " booted into slot " + slot + " on " + GlobalScope.stg.CStageMng.CurrentName + (character == null ? " (no handle)" : ""));
				if (character == null) return;
				OpenFF.Game.Events.Publish(new OpenFF.Events.CastBooted { Cast = cast, Character = character, Map = GlobalScope.stg.CStageMng.CurrentName });
			}
			catch (Exception ex)
			{
				Warn("cast-booted", "CastBooted " + cast + ": " + ex.Message);
			}
		}

		internal static int HeroIndex => GlobalScope.wld.CWorldOutSideData.getInstance().PlayerData().getPlayCharacterIndex();

		internal static GlobalScope.pl.CBasePlayer HeroPlayer
		{
			get
			{
				try
				{
					if (!InWorld) return null;
					GlobalScope.pl.CBasePlayer hero = Players.Player(HeroIndex);
					return hero != null && hero.getCharacterId() >= 0 ? hero : null;
				}
				catch (Exception) { return null; }
			}
		}

		internal const float Fx = 4096f;

		internal static Vector3 ToUnits(GlobalScope.VecFx32 v) => v == null ? Vector3.Zero : new Vector3(v.x / Fx, v.y / Fx, v.z / Fx);

		internal static GlobalScope.VecFx32 ToFx(Vector3 v)
		{
			GlobalScope.VecFx32 r = new GlobalScope.VecFx32();
			r.set((int)Math.Round(v.X * Fx), (int)Math.Round(v.Y * Fx), (int)Math.Round(v.Z * Fx));
			return r;
		}

		/// <summary>The legacy rotation word for a yaw in degrees: the scripts' 4096 * FX_DEG_TO_IDX(deg) * -1.</summary>
		internal static int YawToRot(float yaw)
		{
			int deg = ((int)Math.Round(yaw) % 360 + 360) % 360;
			return -4096 * GlobalScope.FX_DEG_TO_IDX(deg);
		}

		internal static float RotToYaw(int rot)
		{
			// FX_DEG_TO_IDX maps 360 degrees onto 65536; undo it.
			long idx = -(long)rot / 4096;
			float deg = (float)(idx * 360.0 / 65536.0);
			return ((deg % 360f) + 360f) % 360f;
		}

		internal static float YawBetween(Vector3 from, Vector3 to)
		{
			return (float)(Math.Atan2(to.X - from.X, to.Z - from.Z) * 180.0 / Math.PI);
		}

		/// <summary>A flat direction for a yaw in degrees (0 along +z, 90 along +x), in fx.</summary>
		internal static GlobalScope.VecFx32 Direction(float yaw)
		{
			double r = yaw * Math.PI / 180.0;
			GlobalScope.VecFx32 d = new GlobalScope.VecFx32();
			d.set((int)Math.Round(Math.Sin(r) * 4096), 0, (int)Math.Round(Math.Cos(r) * 4096));
			return d;
		}

		/// <summary>
		/// Turns a character toward a direction the way the game's own talk does: the
		/// normalised direction, scaled down by 682, handed to the turn system, which
		/// rotates the model over the next frames. Setting the rotation directly is undone
		/// by that system, and turned the model about the wrong axis.
		/// </summary>
		internal static void TurnToward(GlobalScope.pl.CBasePlayer p, GlobalScope.VecFx32 direction)
		{
			if (p == null || direction == null || (direction.x == 0 && direction.z == 0)) return;
			GlobalScope.VecFx32 d = new GlobalScope.VecFx32();
			d.set(direction.x, 0, direction.z);
			GlobalScope.VEC_Normalize(d, d);
			d.x /= 682;
			d.y /= 682;
			d.z /= 682;
			p.setTargetDirection(d);
		}

		// The motion sets a character's model was not loaded with: the battle binds "b_b01" (and a
		// job's own) onto the same party models it uses on the field, and "b_f<family>" onto a
		// monster's; the field can do the same. Eight slots per character; a set already bound is
		// not bound twice.
		private static readonly Dictionary<int, HashSet<string>> _boundMotions = new Dictionary<int, HashSet<string>>();

		// A motion a mod asked for, held against the field's own motion control: the turn system
		// and the walk restart the wait/walk motions at every transition, which would cut a cast
		// or a swing short. While the request stands, a replaced motion is started again (a
		// few times), until it ends or the mod asks for another.
		internal sealed class MotionHold
		{
			public int Want = -1;
			public bool Loop;
			public uint Blend;
			public int Restarts;
			public long Since;

			public void Ask(GlobalScope.pl.CBasePlayer player, int index, bool loop, uint blend, string who)
			{
				int id = player.getCharacterId();
				if (!GlobalScope.characterMng.isMotion(id, index))
				{
					EngineApi.Warn("motion-" + index, who + ".PlayMotion: motion " + index + " is not in the model's sets - BindMotions first (b_b01 for a party member's model, the monster's MotionSet for its)");
					Want = -1;
					return;
				}
				Want = index;
				Loop = loop;
				Blend = blend;
				Restarts = 0;
				Since = OpenFF.Game.Time.Frame;
				player.startMotion(index, loop, blend);
			}

			public void Tick(GlobalScope.pl.CBasePlayer player)
			{
				if (Want < 0 || player == null) return;
				try
				{
					uint now = player.getMotionIndex();
					if (now == (uint)Want)
					{
						if (!Loop && player.isEndOfMotion()) Want = -1;
						return;
					}
					// The field put its own motion on: ours again, a few times at most, and never
					// after it would have ended anyway.
					if (Restarts >= 3 || OpenFF.Game.Time.Frame - Since > 240)
					{
						Want = -1;
						return;
					}
					Restarts++;
					player.startMotion(Want, Loop, Blend);
				}
				catch (Exception) { Want = -1; }
			}

			public bool Done(GlobalScope.pl.CBasePlayer player)
			{
				if (player == null) return true;
				if (Want < 0) return true;
				try { return !Loop && player.isEndOfMotion(); } catch (Exception) { return true; }
			}
		}

		internal static void BindMotions(GlobalScope.pl.CBasePlayer player, string set)
		{
			int id = player.getCharacterId();
			if (!_boundMotions.TryGetValue(id, out HashSet<string> sets))
			{
				_boundMotions[id] = sets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			}
			if (!sets.Add(set)) return;
			GlobalScope.characterMng.addMotion(id, set);
		}

		internal static void Warn(string key, string message)
		{
			Log.First(LogChannel.General, "api-" + key, 3, () => "engine api: " + message);
		}
	}

	// ---- dialogue ----

	internal sealed class LegacyDialogue : GameService, IDialogue
	{
		private bool _shown;
		// Text waiting for the window to finish opening: the scripts open the window, wait,
		// then set the text; set at once it shows over the half-drawn frame.
		private string _pending;
		// A question in progress: the box is up, the answer goes here.
		private Action<bool> _answer;
		private bool _yes = true;

		public bool IsOpen => _shown || _pending != null;
		public bool IsAsking => _answer != null;
		public event Action Closed;

		private GlobalScope.wld.CMessageWindow Window
		{
			get
			{
				// Outside a map the transit has no world system, and cast_Field2D() would
				// dereference it; ask only when there is one.
				GlobalScope.CCastCommandTransit transit = GlobalScope.CCastCommandTransit.getInstance();
				if (transit.cast_BaseSystem() == null)
				{
					return null;
				}
				try { return transit.cast_Field2D()?.MessageWindow(); }
				catch (Exception) { return null; }
			}
		}

		/// <summary>--debug=dialogue: every step of the window's life in the log.</summary>
		internal static readonly bool Trace = Environment.GetCommandLineArgs().Any(a => a.StartsWith("--debug=", StringComparison.OrdinalIgnoreCase) && (a.Contains("dialogue") || a.Contains("all")));

		private static void TraceLine(string what, GlobalScope.wld.CMessageWindow window)
		{
			if (!Trace) return;
			string state = "no window";
			try { if (window != null) state = "made " + window.isMadeWindow() + " open " + window.isWindowOpen() + " msg " + window.isMadeMessage() + " next " + window.isNextPageButton(); } catch (Exception) { }
			OpenFF.Game.Log("dialogue f" + OpenFF.Game.Time.Frame + ": " + what + " (" + state + ")");
		}

		public void Say(string text, string speaker = null)
		{
			GlobalScope.wld.CMessageWindow window = Window;
			TraceLine("Say \"" + (text ?? "").Substring(0, Math.Min(20, (text ?? "").Length)) + "\"", window);
			if (window == null)
			{
				EngineApi.Warn("say", "Say: no message window (not on a map)");
				return;
			}
			if (!window.isMadeWindow())
			{
				// A fresh window: the text waits for it to finish opening (Tick shows it). The
				// window reports open for a frame after a release, so asking it here is no good -
				// a Say on the frame a message was dismissed put its text on a closing window.
				window.createWindow(1);
				_createdFrame = OpenFF.Game.Time.Frame;
				_pending = text ?? "";
				return;
			}
			if (_shown && window.isWindowOpen() && ClosedFrame != OpenFF.Game.Time.Frame)
			{
				// Our own window is up: the text changes in place.
				Show(window, text ?? "");
			}
			else
			{
				_pending = text ?? "";
			}
		}

		public void Ask(string question, Action<bool> answered)
		{
			if (Window == null)
			{
				EngineApi.Warn("ask", "Ask: not on a map");
				Game.Guard("Dialogue.Ask", () => answered?.Invoke(false));
				return;
			}
			if (_answer != null)
			{
				// One question at a time: the earlier one is answered "no".
				Action<bool> earlier = _answer;
				_answer = null;
				Game.Guard("Dialogue.Ask", () => earlier(false));
			}
			// The answer is recorded first: Show decides by it whether to put the box up.
			_answer = answered ?? (_ => { });
			_yes = true;
			Say(question ?? "");
		}

		/// <summary>
		/// A text of the form "@1000142" is one of the game's own messages, by its id in the
		/// .msd, shown through createMessage so its control codes (the item, the gold, the
		/// hero's name) expand as the game's do; "@1000142 item=5001" or "gold=250" sets the
		/// codes first, as the game's chest does before its "The chest contained ..." line;
		/// "color=9" is the window's text colour (dgs.TXT_COLOR; 9 is the chests' gold).
		/// </summary>
		private static bool TryMessageId(string text, out uint id, out int itemId, out int gold, out int color)
		{
			id = 0; itemId = 0; gold = 0; color = -1;
			if (string.IsNullOrEmpty(text) || text[0] != '@') return false;
			string[] parts = text.Substring(1).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 0 || !uint.TryParse(parts[0], out id)) return false;
			foreach (string part in parts.Skip(1))
			{
				int eq = part.IndexOf('=');
				if (eq <= 0 || !int.TryParse(part.Substring(eq + 1), out int value)) continue;
				string key = part.Substring(0, eq).ToLowerInvariant();
				if (key == "item") itemId = value; else if (key == "gold" || key == "gil") gold = value; else if (key == "color") color = value;
			}
			return true;
		}

		private void Show(GlobalScope.wld.CMessageWindow window, string text)
		{
			TraceLine("Show", window);
			if (TryMessageId(text, out uint id, out int itemId, out int gold, out int color))
			{
				try
				{
					if (color >= 0) window.setMessageColor(color);
					if (gold != 0) GlobalScope.dgs.CCtrlCodeInterface.instance().setGold(gold);
					if (itemId != 0) GlobalScope.dgs.CCtrlCodeInterface.instance().setItemId(GlobalScope.itm.ItemManager.instance().itemParameter((short)itemId).nameId());
					window.createMessage((int)id, 0, 0);
				}
				catch (Exception ex)
				{
					EngineApi.Warn("say-id", "Say @" + id + ": " + ex.Message);
					window.createText(text, 0);
				}
			}
			else
			{
				window.createText(text, 0);
			}
			TraceLine("Show done", window);
			// A question keeps its text up until answered: no tap mark, no dismissal.
			window.setProgressIconActivity(_SendMessage: _answer == null);
			_pending = null;
			_shown = true;
		}

		public void Close()
		{
			GlobalScope.wld.CMessageWindow window = Window;
			bool wasOpen = _shown || _pending != null;
			TraceLine("Close (wasOpen " + wasOpen + ")", window);
			if (window != null && wasOpen)
			{
				window.release();
				TraceLine("released", window);
			}
			_pending = null;
			if (_answer != null)
			{
				Action<bool> answer = _answer;
				_answer = null;
				Game.Guard("Dialogue.Ask", () => answer(false));
			}
			if (wasOpen)
			{
				_shown = false;
				Game.Guard("Dialogue.Closed", () => Closed?.Invoke());
			}
		}

		/// <summary>The frame the window closed on; a press that dismissed it must not also talk to someone.</summary>
		internal long ClosedFrame = -1;
		/// <summary>The frame Say created the window on; it reports open for a frame after a release, so pending text waits past that.</summary>
		private long _createdFrame = -10;

		/// <summary>
		/// Pending text goes up once the window is open; a question reads the Yes/No box
		/// (tap on an answer, or up/down and A; B is no); a plain message comes down when
		/// the player taps past it (isNextPageButton, what WaitInputSendMessage waits for).
		/// </summary>
		internal void Tick()
		{
			GlobalScope.wld.CMessageWindow window = Window;
			if (_pending != null)
			{
				if (window == null || !window.isMadeWindow())
				{
					_pending = null;
					return;
				}
				if (window.isWindowOpen() && OpenFF.Game.Time.Frame - _createdFrame >= 2)
				{
					Show(window, _pending);
				}
				return;
			}
			if (!_shown) return;
			if (window == null || !window.isMadeWindow())
			{
				Close();
				ClosedFrame = OpenFF.Game.Time.Frame;
				return;
			}
			if (_answer != null)
			{
				TickQuestion();
				return;
			}
			if (window.isNextPageButton())
			{
				Close();
				ClosedFrame = OpenFF.Game.Time.Frame;
			}
		}

		// The box: drawn each frame by the engine's own draw layer, over the message window's
		// right end, in screen units (800x480).
		private const float BoxX = 610f, BoxY = 262f, BoxW = 150f, BoxH = 74f, RowH = 30f;

		private void TickQuestion()
		{
			OpenFF.InputState input = OpenFF.Game.Input;
			int decided = -1;
			if (input.Pressed(OpenFF.Pad.Up) || input.Pressed(OpenFF.Pad.Down))
			{
				_yes = !_yes;
			}
			if (input.Pressed(OpenFF.Pad.A)) decided = _yes ? 1 : 0;
			if (input.Pressed(OpenFF.Pad.B)) decided = 0;
			if (decided < 0 && input.PointerReleased)
			{
				float x = input.PointerX, y = input.PointerY;
				if (x >= BoxX && x <= BoxX + BoxW)
				{
					if (y >= BoxY + 6 && y < BoxY + 6 + RowH) decided = 1;
					else if (y >= BoxY + 6 + RowH && y < BoxY + BoxH) decided = 0;
				}
			}
			if (decided < 0)
			{
				DrawBox();
				return;
			}
			bool yes = decided == 1;
			Log.Write(LogChannel.General, "engine api: Ask answered " + (yes ? "yes" : "no"));
			Action<bool> answer = _answer;
			_answer = null;
			GlobalScope.wld.CMessageWindow window = Window;
			if (window != null)
			{
				window.release();
			}
			_shown = false;
			ClosedFrame = OpenFF.Game.Time.Frame;
			OpenFF.Game.Events.Publish(new OpenFF.Events.Answered { Yes = yes });
			Game.Guard("Dialogue.Ask", () => answer(yes));
			Game.Guard("Dialogue.Closed", () => Closed?.Invoke());
		}

		private void DrawBox()
		{
			OpenFF.DrawList draw = OpenFF.Game.Draw;
			draw.Rect(BoxX, BoxY, BoxW, BoxH, new OpenFF.Color(24, 40, 96, 235));
			draw.Rect(BoxX, BoxY, BoxW, BoxH, new OpenFF.Color(230, 230, 240), filled: false);
			draw.Rect(BoxX + 1, BoxY + 1, BoxW - 2, BoxH - 2, new OpenFF.Color(120, 130, 170), filled: false);
			float yesY = BoxY + 6, noY = BoxY + 6 + RowH;
			draw.Rect(BoxX + 6, (_yes ? yesY : noY) + 2, BoxW - 12, RowH - 4, new OpenFF.Color(255, 255, 255, 40));
			draw.Text(">", BoxX + 14, (_yes ? yesY : noY) + 6, OpenFF.Color.Yellow, 16);
			draw.Text("Yes", BoxX + 40, yesY + 6, _yes ? OpenFF.Color.White : new OpenFF.Color(200, 200, 210), 16);
			draw.Text("No", BoxX + 40, noY + 6, _yes ? new OpenFF.Color(200, 200, 210) : OpenFF.Color.White, 16);
		}
	}

	// ---- a walk the host steps itself ----

	/// <summary>
	/// A walk over frames, stepped by the host: the legacy MoveSys leaves a character
	/// without a script cast standing still, so the API moves the position itself and
	/// lets the WALK action play the motion.
	/// </summary>
	internal sealed class HostWalk
	{
		private GlobalScope.VecFx32 _target;
		private GlobalScope.VecFx32 _step;
		private int _frames;

		public bool Moving => _frames > 0;

		public void Begin(GlobalScope.pl.CBasePlayer p, int humanIndex, Vector3 position, int frames)
		{
			_target = EngineApi.ToFx(position);
			GlobalScope.VecFx32 from = p.getPosition();
			_step = new GlobalScope.VecFx32();
			_step.set((_target.x - from.x) / frames, (_target.y - from.y) / frames, (_target.z - from.z) / frames);
			_frames = frames;
			EngineApi.TurnToward(p, _step);
			SetAction(humanIndex, GlobalScope.pl.CPlayerHuman.ACTION_ID.ACTION_ID_WALK);
		}

		public void Stop(int humanIndex)
		{
			if (_frames <= 0) return;
			_frames = 0;
			SetAction(humanIndex, GlobalScope.pl.CPlayerHuman.ACTION_ID.ACTION_ID_WAIT);
		}

		public void Advance(GlobalScope.pl.CBasePlayer p, int humanIndex)
		{
			if (_frames <= 0) return;
			if (p == null)
			{
				_frames = 0;
				return;
			}
			_frames--;
			GlobalScope.VecFx32 next = new GlobalScope.VecFx32();
			if (_frames == 0)
			{
				next.copy(_target);
			}
			else
			{
				GlobalScope.VecFx32 now = p.getPosition();
				next.set(now.x + _step.x, now.y + _step.y, now.z + _step.z);
			}
			p.getPrePosition_set(p.getPosition());
			p.setPosition(next);
			EngineApi.TurnToward(p, _step);
			if (_frames == 0)
			{
				SetAction(humanIndex, GlobalScope.pl.CPlayerHuman.ACTION_ID.ACTION_ID_WAIT);
			}
		}

		private static void SetAction(int humanIndex, GlobalScope.pl.CPlayerHuman.ACTION_ID action)
		{
			try
			{
				EngineApi.Players.PlayerHuman(humanIndex)?.setAction(action);
			}
			catch (Exception) { }
		}
	}

	// ---- the hero ----

	internal sealed class LegacyHero : GameService, IHero
	{
		private bool _frozen;
		private readonly HostWalk _walk = new HostWalk();

		public bool Present => EngineApi.HeroPlayer != null;
		public Vector3 Position => EngineApi.ToUnits(EngineApi.HeroPlayer?.getPosition());
		public float Yaw => EngineApi.HeroPlayer == null ? 0f : EngineApi.RotToYaw(EngineApi.HeroPlayer.getRotation().y);
		public string Model => EngineApi.HeroPlayer?.getModelName();
		public bool Frozen => _frozen;
		public bool Moving => _walk.Moving && EngineApi.HeroPlayer != null;

		public void Teleport(Vector3 position)
		{
			GlobalScope.pl.CBasePlayer hero = EngineApi.HeroPlayer;
			if (hero == null) return;
			_walk.Stop(EngineApi.HeroIndex);
			GlobalScope.VecFx32 pos = EngineApi.ToFx(position);
			hero.setPosition(pos);
			hero.getPrePosition_set(hero.getPosition());
		}

		public void Face(float yaw)
		{
			GlobalScope.pl.CBasePlayer hero = EngineApi.HeroPlayer;
			if (hero == null) return;
			EngineApi.TurnToward(hero, EngineApi.Direction(yaw));
		}

		public void LookAt(Vector3 point)
		{
			GlobalScope.pl.CBasePlayer hero = EngineApi.HeroPlayer;
			if (hero == null) return;
			GlobalScope.VecFx32 to = EngineApi.ToFx(point);
			GlobalScope.VecFx32 from = hero.getPosition();
			GlobalScope.VecFx32 d = new GlobalScope.VecFx32();
			d.set(to.x - from.x, 0, to.z - from.z);
			EngineApi.TurnToward(hero, d);
		}

		public void MoveTo(Vector3 position, int frames)
		{
			GlobalScope.pl.CBasePlayer hero = EngineApi.HeroPlayer;
			if (hero == null) return;
			if (frames <= 0)
			{
				Teleport(position);
				return;
			}
			hero.setAutoPilot(_AutoPilot: true);
			_walk.Begin(hero, EngineApi.HeroIndex, position, frames);
		}

		public void Stop()
		{
			_walk.Stop(EngineApi.HeroIndex);
		}

		private readonly EngineApi.MotionHold _motion = new EngineApi.MotionHold();

		public void PlayMotion(int index, bool loop = false, int blendFrames = 5)
		{
			GlobalScope.pl.CBasePlayer hero = EngineApi.HeroPlayer;
			if (hero == null) return;
			Game.Guard("Hero.PlayMotion", () => _motion.Ask(hero, index, loop, (uint)Math.Max(0, blendFrames), "Hero"));
		}

		public void BindMotions(string set = "b_b01")
		{
			GlobalScope.pl.CBasePlayer hero = EngineApi.HeroPlayer;
			if (hero == null || string.IsNullOrEmpty(set)) return;
			Game.Guard("Hero.BindMotions", () => EngineApi.BindMotions(hero, set));
		}

		// What btl.BattlePlayer binds when a fight starts: the common set, the magic set
		// ("ADD PLAYER MAGIC MOTION"), the job's set and b_b04_002. The weapon's swing set is
		// the one thing left to the mod, since it depends on what is in the hand.
		public void BindBattleMotions()
		{
			GlobalScope.pl.CBasePlayer hero = EngineApi.HeroPlayer;
			if (hero == null) return;
			Game.Guard("Hero.BindBattleMotions", () =>
			{
				EngineApi.BindMotions(hero, "b_b01");
				EngineApi.BindMotions(hero, "b_b02_040");
				int job = 0;
				try { job = GlobalScope.pl.PlayerParty.instance().player((byte)EngineApi.HeroIndex)?.jobManager().nowJob() ?? 0; } catch (Exception) { }
				EngineApi.BindMotions(hero, "b_b03_" + GlobalScope.btl.BattlePlayer.jobMotionFileId(job).ToString("D3"));
				EngineApi.BindMotions(hero, "b_b04_002");
			});
		}

		public bool MotionDone => _motion.Done(EngineApi.HeroPlayer);

		public bool Balloon
		{
			get => EngineApi.HeroPlayer?.isBalloon() == true;
			set { GlobalScope.pl.CBasePlayer hero = EngineApi.HeroPlayer; if (hero != null) hero.setBalloon(value); }
		}

		public void Freeze()
		{
			if (!EngineApi.InWorld) return;
			int index = EngineApi.HeroIndex;
			EngineApi.Players.setPlayerStop(index);
			GlobalScope.dv.CDeviceManager.getInstance().Pad().setActivity(b: false);
			_frozen = true;
		}

		public void Unfreeze()
		{
			if (!EngineApi.InWorld) { _frozen = false; return; }
			int index = EngineApi.HeroIndex;
			_walk.Stop(index);
			GlobalScope.pl.CBasePlayer hero = EngineApi.HeroPlayer;
			hero?.setAutoPilot(_AutoPilot: false);
			EngineApi.Players.setPlayerStart(index);
			GlobalScope.dv.CDeviceManager.getInstance().Pad().setActivity(b: true);
			_frozen = false;
		}

		/// <summary>Once per frame: a scripted walk in progress; when it ends and the hero is not frozen, control returns.</summary>
		internal void Tick()
		{
			_motion.Tick(EngineApi.HeroPlayer);
			if (!_walk.Moving) return;
			GlobalScope.pl.CBasePlayer hero = EngineApi.HeroPlayer;
			_walk.Advance(hero, EngineApi.HeroIndex);
			if (!_walk.Moving && !_frozen && hero != null)
			{
				hero.setAutoPilot(_AutoPilot: false);
			}
		}
	}

	// ---- characters ----

	internal sealed class LegacyNpc : Npc
	{
		internal int Index;
		internal string Map;
		private bool _removed;
		private readonly HostWalk _walk = new HostWalk();

		public LegacyNpc(int index, string model, string map)
		{
			Index = index;
			Model = model;
			Map = map;
		}

		private GlobalScope.pl.CBasePlayer Player
		{
			get
			{
				try
				{
					if (_removed || !EngineApi.InWorld || Map != GlobalScope.stg.CStageMng.CurrentName) return null;
					GlobalScope.pl.CBasePlayer p = EngineApi.Players.Player(Index);
					return p != null && p.getCharacterId() >= 0 ? p : null;
				}
				catch (Exception) { return null; }
			}
		}

		public override bool Alive => Player != null;
		public override Vector3 Position => EngineApi.ToUnits(Player?.getPosition());
		public override float Yaw => Player == null ? 0f : EngineApi.RotToYaw(Player.getRotation().y);
		public override bool Moving => _walk.Moving && Player != null;

		public override void Teleport(Vector3 position)
		{
			GlobalScope.pl.CBasePlayer p = Player;
			if (p == null) return;
			_walk.Stop(Index);
			p.setPosition(EngineApi.ToFx(position));
			p.getPrePosition_set(p.getPosition());
		}

		public override void MoveTo(Vector3 position, int frames)
		{
			GlobalScope.pl.CBasePlayer p = Player;
			if (p == null) return;
			if (frames <= 0)
			{
				Teleport(position);
				return;
			}
			_walk.Begin(p, Index, position, frames);
		}

		public override void Stop() => _walk.Stop(Index);

		/// <summary>One frame of a walk in progress; called by the host each tick.</summary>
		internal void Advance()
		{
			GlobalScope.pl.CBasePlayer p = Player;
			_walk.Advance(p, Index);
			_motion.Tick(p);
		}

		private readonly EngineApi.MotionHold _motion = new EngineApi.MotionHold();

		public override void PlayMotion(int index, bool loop = false, int blendFrames = 5)
		{
			GlobalScope.pl.CBasePlayer p = Player;
			if (p == null) return;
			// A map object (a chest, a sign: o/w models) has its own motions - the lid's 1002/1003 -
			// and plays them itself (map.CMapObject.startMotion), not through the walkers' sets.
			GlobalScope.map.CMapObject box = AsMapObject();
			if (box != null)
			{
				Game.Guard("Npc.PlayMotion", () => box.startMotion(index, loop, (uint)Math.Max(0, blendFrames)));
				return;
			}
			Game.Guard("Npc.PlayMotion", () => _motion.Ask(p, index, loop, (uint)Math.Max(0, blendFrames), "Npc"));
		}

		/// <summary>The character as the game's map object, when its slot is one of those (the o/w models); null for a walker.</summary>
		private GlobalScope.map.CMapObject AsMapObject()
		{
			try
			{
				int num = Index - (int)(GlobalScope.pl.FIELD_CHARACTER_NUM - GlobalScope.pl.MAP_OBJECT_NUM);
				if (num < 0 || num >= (int)GlobalScope.pl.MAP_OBJECT_NUM) return null;
				return GlobalScope.CCastCommandTransit.getInstance().cast_PlayerMng().MapObject(num);
			}
			catch (Exception) { return null; }
		}

		public override void BindMotions(string set)
		{
			GlobalScope.pl.CBasePlayer p = Player;
			if (p == null || string.IsNullOrEmpty(set)) return;
			Game.Guard("Npc.BindMotions", () => EngineApi.BindMotions(p, set));
		}

		public override bool MotionDone
		{
			get
			{
				// A map object (a chest) plays its own motions: ask it, not the walkers' bookkeeping.
				GlobalScope.map.CMapObject box = AsMapObject();
				if (box != null) { try { return box.isEndOfMotion(); } catch (Exception) { return true; } }
				return _motion.Done(Player);
			}
		}

		public override int Alpha
		{
			get { GlobalScope.pl.CBasePlayer p = Player; return p == null ? 0 : p.getTransparencyRate(); }
			set { GlobalScope.pl.CBasePlayer p = Player; if (p != null) p.setTransparencyRate(Math.Clamp(value, 0, 100)); }
		}

		public override bool Hidden
		{
			get => Player?.isHidden() == true;
			set { GlobalScope.pl.CBasePlayer p = Player; if (p != null) p.setHidden(value); }
		}

		public override bool Balloon
		{
			get => Player?.isBalloon() == true;
			set { GlobalScope.pl.CBasePlayer p = Player; if (p != null) p.setBalloon(value); }
		}

		public override float Scale
		{
			get { GlobalScope.pl.CBasePlayer p = Player; return p == null ? 1f : p.getScale().x / 4096f; }
			set
			{
				GlobalScope.pl.CBasePlayer p = Player;
				if (p == null) return;
				int s = (int)Math.Round(Math.Max(0.01f, value) * 4096);
				GlobalScope.VecFx32 v = new GlobalScope.VecFx32();
				v.set(s, s, s);
				p.setScale(v);
				p.setShadowScale(v);
			}
		}

		public override void Face(float yaw)
		{
			GlobalScope.pl.CBasePlayer p = Player;
			if (p == null) return;
			EngineApi.TurnToward(p, EngineApi.Direction(yaw));
		}

		public override void LookAt(Vector3 point)
		{
			GlobalScope.pl.CBasePlayer p = Player;
			if (p == null) return;
			GlobalScope.VecFx32 to = EngineApi.ToFx(point);
			GlobalScope.VecFx32 from = p.getPosition();
			GlobalScope.VecFx32 d = new GlobalScope.VecFx32();
			d.set(to.x - from.x, 0, to.z - from.z);
			EngineApi.TurnToward(p, d);
		}

		public override void SetAi(NpcAi ai)
		{
			try
			{
				GlobalScope.pl.CPlayerHuman human = EngineApi.Players.PlayerHuman(Index);
				if (human == null || Player == null) return;
				human.NPCAiManager().AiKind_set((GlobalScope.pl.CNPCAiManager.AI_KIND)(int)ai);
			}
			catch (Exception ex)
			{
				EngineApi.Warn("ai", "SetAi: " + ex.Message);
			}
		}

		/// <summary>ff3Command_MoveCharacter_StartRandom on this character: no autopilot, no operator, the random-move AI, the gait.</summary>
		public override void StartWander(WanderGait gait)
		{
			try
			{
				GlobalScope.pl.CPlayerHuman human = EngineApi.Players.PlayerHuman(Index);
				if (human == null || Player == null) return;
				human.setAutoPilot(_AutoPilot: false);
				human.setOperater(_Operater: false);
				human.NPCAiManager().AiKind_set(GlobalScope.pl.CNPCAiManager.AI_KIND.AI_KIND_RANDOM_MOVE);
				human.NPCRandomMoveType_set((GlobalScope.pl.NPC_RANDOM_MOVE_TYPE)(int)gait);
			}
			catch (Exception ex)
			{
				EngineApi.Warn("wander", "StartWander: " + ex.Message);
			}
		}

		public override bool RunScript(string line)
		{
			if (Player == null) return false;
			if (ScriptSnippet.Run(line, out string error))
			{
				Log.Write(LogChannel.File, "engine api: " + Model + " (character " + Index + ") ran " + line.Trim());
				return true;
			}
			EngineApi.Warn("run-script", "RunScript on " + Model + ": " + error + " - " + line.Trim());
			return false;
		}

		/// <summary>ff3Command_MoveCharacter_EndRandom: no operator, the default AI.</summary>
		public override void EndWander()
		{
			try
			{
				GlobalScope.pl.CPlayerHuman human = EngineApi.Players.PlayerHuman(Index);
				if (human == null || Player == null) return;
				human.setOperater(_Operater: false);
				human.NPCAiManager().AiKind_set(GlobalScope.pl.CNPCAiManager.AI_KIND.AI_KIND_DEFAULT);
			}
			catch (Exception ex)
			{
				EngineApi.Warn("wander", "EndWander: " + ex.Message);
			}
		}

		private bool _solid;

		public override bool Solid
		{
			get => _solid;
			set
			{
				_solid = value;
				GlobalScope.pl.CBasePlayer p = Player;
				if (p == null) return;
				// The scripts' SetCharacter_CharaCollision: bit 4 of the collision flags.
				if (value) p.getColFlag_or(4); else p.getColFlag_not_and(4);
			}
		}

		public override void Remove()
		{
			GlobalScope.pl.CBasePlayer p = Player;
			_removed = true;
			if (p != null)
			{
				Game.Guard("Npc.Remove", p.terminate);
			}
		}

		internal void Interact() => RaiseInteracted();

		internal bool IsPlayer(GlobalScope.chr.CCharacterEureka character) => character != null && ReferenceEquals(character, Player);

		/// <summary>
		/// What bootCharacterImp does for the game's own: the logic index is the cast number
		/// (talking to the character runs cast&lt;N&gt;_main), and the .hich row's character index
		/// points here, so every command the script addresses to that cast - changeHichNumber -
		/// lands on this character from now on. The stand-in is the original, as far as the
		/// script can tell. Solid, as booted characters are.
		/// </summary>
		public override void RunCast(int cast)
		{
			GlobalScope.pl.CBasePlayer p = Player;
			if (p == null || cast <= 0) return;
			try
			{
				p.LogicIndex_set((uint)cast);
				int row = GlobalScope.evt.CHichParameterManager.getInstance().getManCastIndex((uint)cast);
				if (row >= 0) GlobalScope.evt.CHichParameterManager.getInstance().setCharaIndex(row, Index);
				p.flagOff(GlobalScope.pl.CBasePlayer.CBP_FLAG.NPC_NOT_TURN_TALKED);
				Solid = true;
				Log.Write(LogChannel.General, "engine api: " + Model + " (character " + Index + ") runs cast " + cast + (row >= 0 ? " (row " + row + ")" : " (no row)") + " on " + Map);
			}
			catch (Exception ex)
			{
				EngineApi.Warn("run-cast", "RunCast " + cast + " on " + Model + " failed: " + ex.Message);
			}
		}

		/// <summary>
		/// ff3Command_SetTreasureItem / SetTreasureMoney on this character: the map object keeps
		/// the flag, the item (or gold) and its count; an opened chest (flag set) shows the open
		/// lid, a closed one shuts. The chest then opens the game's own way when talked to -
		/// sound, lid, message, flag, treasure count - since it is a map object like any other.
		/// </summary>
		public override void OwnChest()
		{
			// The model's number is its type to the game (o001 = TREASURE_BOX, o000 = INVISIBLE, an item spot): CPlayerHumanCheck
			// runs the game's own opening for one on A. As a plain object the talk goes the
			// characters' way (turn, startLogic of nothing) and reaches Interacted.
			GlobalScope.map.CMapObject box = AsMapObject();
			if (box == null) return;
			GlobalScope.map.MAP_OBJECT_TYPE type = box.MapObjType();
			if (type != GlobalScope.map.MAP_OBJECT_TYPE.TREASURE_BOX && type != GlobalScope.map.MAP_OBJECT_TYPE.INVISIBLE) return;
			try { box.setMapObjType(GlobalScope.map.MAP_OBJECT_TYPE.MAP_OBJECT_TYPE_ERR); }
			catch (Exception ex) { EngineApi.Warn("chest", "OwnChest: " + ex.Message); }
		}

		public override void SetTreasure(int itemId, int gil, int flagGroup, int flagIndex)
		{
			if (Player == null) return;
			try
			{
				GlobalScope.map.CMapObject box = AsMapObject();
				if (box == null)
				{
					EngineApi.Warn("treasure", "SetTreasure: " + Model + " (character " + Index + ") is not a map object - a chest wants an o or w model spawned as a character");
					return;
				}
				box.setFlag((uint)flagGroup, (uint)flagIndex);
				if (GlobalScope.FlagManager.singleton().get((uint)flagGroup, (uint)flagIndex) == 1)
				{
					box.setNowAct(6);
					return;
				}
				box.setEnCountIndex(0);
				if (gil > 0)
				{
					box.setGold(gil);
				}
				else
				{
					box.setItemId((uint)itemId);
					int count = 1;
					if (GlobalScope.itm.ItemManager.instance().itemCategory((short)itemId) == GlobalScope.itm.CATEGORY.CATEGORY_WEAPON
						&& GlobalScope.itm.ItemManager.instance().itemParameter((short)itemId).system() == 8)
					{
						count = 20;
					}
					box.setItemNum(count);
				}
				box.startMotion(1003, _Loop: false, 5u);
				Log.Write(LogChannel.General, "engine api: " + Model + " (character " + Index + ") is a chest: " + (gil > 0 ? gil + " gil" : "item " + itemId) + ", flag " + flagGroup + ":" + flagIndex);
			}
			catch (Exception ex)
			{
				EngineApi.Warn("treasure", "SetTreasure on " + Model + " failed: " + ex.Message);
			}
		}

		/// <summary>ff3Command_ChangeColorCharacter: the texture set &lt;model&gt;_&lt;variant&gt; in place of the model's own.</summary>
		public override void Recolour(string variant)
		{
			GlobalScope.pl.CBasePlayer p = Player;
			if (p == null || string.IsNullOrWhiteSpace(variant)) return;
			try
			{
				int characterId = p.getCharacterId();
				string name = Model + "_" + variant.Trim();
				GlobalScope.characterMng.releaseTex(characterId);
				GlobalScope.characterMng.bindReplaceTex(characterId, name);
				GlobalScope.TexDivideLoader.getSingleton().tdlForceLoad();
				GlobalScope.characterMng.setupReplaceTex(characterId);
				Log.Write(LogChannel.General, "engine api: " + Model + " (character " + Index + ") recoloured " + name);
			}
			catch (Exception ex)
			{
				EngineApi.Warn("recolour", "Recolour " + variant + " on " + Model + " failed: " + ex.Message);
			}
		}
	}

	internal sealed class LegacyNpcs : GameService, INpcs
	{
		private readonly List<LegacyNpc> _spawned = new List<LegacyNpc>();
		// The map's own characters a mod asked for by index: not ours to place or remove with the
		// map, but talked to and moved like ours while the map is up.
		private readonly List<LegacyNpc> _wrapped = new List<LegacyNpc>();

		public IReadOnlyList<Npc> Spawned => _spawned;

		private IEnumerable<LegacyNpc> Tracked => _spawned.Concat(_wrapped);

		/// <summary>
		/// The character the map's .hich row <paramref name="row"/> was booted into, or null
		/// while nothing has booted it (a scene's actor before its scene, an opened chest). The
		/// row is what Crystal shows and a scene file's object:&lt;n&gt; means; the slot is the
		/// hich table's business (bootCharacterImp's setCharaIndex).
		/// </summary>
		public Npc ByRow(int row)
		{
			if (!EngineApi.InWorld || row < 0 || row >= 48) return null;
			try
			{
				int slot = GlobalScope.evt.CHichParameterManager.getInstance().CharaIndex(row);
				return slot >= 0 ? Existing(slot) : null;
			}
			catch (Exception ex) { EngineApi.Warn("by-row", "Npcs.ByRow " + row + ": " + ex.Message); return null; }
		}

		public Npc Existing(int index)
		{
			if (!EngineApi.InWorld || index < 0) return null;
			string map = GlobalScope.stg.CStageMng.CurrentName;
			LegacyNpc have = _wrapped.FirstOrDefault(n => n.Index == index && n.Map == map);
			if (have != null) return have;
			try
			{
				GlobalScope.pl.CBasePlayer player = EngineApi.Players.Player(index);
				if (player == null) return null;
				LegacyNpc npc = new LegacyNpc(index, "object:" + index, map);
				_wrapped.Add(npc);
				return npc;
			}
			catch (Exception ex) { EngineApi.Warn("existing", "Npcs.Existing: " + ex.Message); return null; }
		}

		public Npc Spawn(string model, Vector3 position, float yaw = 0f)
		{
			if (!EngineApi.InWorld || string.IsNullOrEmpty(model))
			{
				EngineApi.Warn("spawn", "Spawn: not on a map");
				return null;
			}
			try
			{
				string map = GlobalScope.stg.CStageMng.CurrentName;
				GlobalScope.VecFx32 pos = EngineApi.ToFx(position);
				GlobalScope.VecFx32 rot = new GlobalScope.VecFx32();
				rot.set(0, EngineApi.YawToRot(yaw), 0);
				GlobalScope.VecFx32 scale = new GlobalScope.VecFx32();
				scale.set(4096, 4096, 4096);
				GlobalScope.VecFx32 shadow = new GlobalScope.VecFx32();
				shadow.set(4915, 4096, 4915);
				// The same steps as the BootCharacter command's bootCharacterImp, without a
				// script cast behind the character.
				GlobalScope.TexDivideLoader.getSingleton().tdlForceLoad();
				GlobalScope.changeGlobalDirectory();
				int index = EngineApi.Players.setUpWorldCharacter(pos, rot, scale, shadow, model, _AutoPilot: false, _Operater: false);
				if (index < 0)
				{
					EngineApi.Warn("spawn-full", "Spawn: no character slot free for " + model);
					return null;
				}
				GlobalScope.TexDivideLoader.getSingleton().tdlForceLoad();
				GlobalScope.pl.CBasePlayer player = EngineApi.Players.Player(index);
				GlobalScope.characterMng.setupOrgTex(player.getCharacterId());
				player.into();
				player.setAutoPilot(_AutoPilot: true);
				player.setHidden(false);
				// No script cast stands behind this character: the legacy talk finds nothing to run.
				player.LogicIndex_set(GlobalScope.CastInfo.INVALID_SCRIPT);
				LegacyNpc npc = new LegacyNpc(index, model, map);
				npc.Solid = false;
				_spawned.Add(npc);
				Log.Write(LogChannel.General, "engine api: spawned " + model + " as character " + index + " at " + position + " on " + map
					+ " (chr " + player.getCharacterId() + ", hidden " + player.isHidden() + ")");
				// Diagnostic: everyone on the map, to place the new one among them.
				for (int i = 0; i < 24; i++)
				{
					GlobalScope.pl.CBasePlayer other = EngineApi.Players.Player(i);
					if (other != null && other.getCharacterId() >= 0)
					{
						Log.Write(LogChannel.File, "engine api:   character " + i + " " + other.getModelName() + " at " + EngineApi.ToUnits(other.getPosition())
							+ " chr " + other.getCharacterId() + " hidden " + other.isHidden() + " auto " + other.isAutoPilot());
					}
				}
				return npc;
			}
			catch (Exception ex)
			{
				EngineApi.Warn("spawn-fail", "Spawn " + model + " failed: " + ex.GetType().Name + ": " + ex.Message);
				return null;
			}
		}

		/// <summary>
		/// The map scripts' bootPlainCharacter, without a cast: the light walker
		/// (setupPlainCharacter) with the model's own scale and human type - the children's
		/// n031/n041 at 0.8, the chocobo, the frog, the fairy - through the very code the
		/// command runs (bootPlainCharacterImp). What a stand-in for a plain-booted character
		/// must be to look and move as the original did.
		/// </summary>
		public Npc SpawnPlain(string model, Vector3 position, float yaw = 0f)
		{
			if (!EngineApi.InWorld || string.IsNullOrEmpty(model))
			{
				EngineApi.Warn("spawn", "SpawnPlain: not on a map");
				return null;
			}
			try
			{
				string map = GlobalScope.stg.CStageMng.CurrentName;
				GlobalScope.VecFx32 pos = EngineApi.ToFx(position);
				GlobalScope.VecFx32 rot = new GlobalScope.VecFx32();
				rot.set(0, EngineApi.YawToRot(yaw), 0);
				GlobalScope.VecFx32 scale = new GlobalScope.VecFx32();
				scale.set(4096, 4096, 4096);
				GlobalScope.VecFx32 shadow = new GlobalScope.VecFx32();
				shadow.set(4915, 4096, 4915);
				int index = GlobalScope.bootPlainCharacterImp(GlobalScope.CastInfo.INVALID_SCRIPT, model, pos, rot, scale, shadow);
				if (index < 0)
				{
					EngineApi.Warn("spawn-full", "SpawnPlain: no character slot free for " + model);
					return null;
				}
				GlobalScope.pl.CBasePlayer player = EngineApi.Players.Player(index);
				player.setHidden(false);
				LegacyNpc npc = new LegacyNpc(index, model, map);
				_spawned.Add(npc);
				Log.Write(LogChannel.General, "engine api: spawned " + model + " as plain character " + index + " at " + position + " on " + map
					+ " (chr " + player.getCharacterId() + ")");
				return npc;
			}
			catch (Exception ex)
			{
				EngineApi.Warn("spawn-fail", "SpawnPlain " + model + " failed: " + ex.GetType().Name + ": " + ex.Message);
				return null;
			}
		}

		public Npc SpawnModel(string model, Vector3 position, float yaw = 0f, float scale = 1f)
		{
			if (!EngineApi.InWorld || string.IsNullOrEmpty(model))
			{
				EngineApi.Warn("spawn-model", "SpawnModel: not on a map");
				return null;
			}
			try
			{
				string map = GlobalScope.stg.CStageMng.CurrentName;
				int s = (int)Math.Round(Math.Max(0.01f, scale) * 4096);
				GlobalScope.VecFx32 scl = new GlobalScope.VecFx32();
				scl.set(s, s, s);
				GlobalScope.VecFx32 shadow = new GlobalScope.VecFx32();
				shadow.set(4915, 4096, 4915);
				GlobalScope.TexDivideLoader.getSingleton().tdlForceLoad();
				GlobalScope.changeGlobalDirectory();
				int index = EngineApi.Players.setupPlainCharacter(model, scl, shadow);
				if (index < 0)
				{
					EngineApi.Warn("spawn-model-full", "SpawnModel: " + model + " did not load or no slot is free");
					return null;
				}
				GlobalScope.TexDivideLoader.getSingleton().tdlForceLoad();
				GlobalScope.pl.CBasePlayer player = EngineApi.Players.Player(index);
				if (player == null || player.getCharacterId() < 0)
				{
					EngineApi.Warn("spawn-model-fail", "SpawnModel: " + model + " did not load");
					return null;
				}
				GlobalScope.characterMng.setupOrgTex(player.getCharacterId());
				player.setPosition(EngineApi.ToFx(position));
				GlobalScope.VecFx32 rot = new GlobalScope.VecFx32();
				rot.set(0, EngineApi.YawToRot(yaw), 0);
				player.setRotation(rot);
				player.into();
				player.setAutoPilot(_AutoPilot: true);
				player.setHidden(false);
				player.LogicIndex_set(GlobalScope.CastInfo.INVALID_SCRIPT);
				LegacyNpc npc = new LegacyNpc(index, model, map);
				npc.Solid = false;
				_spawned.Add(npc);
				Log.Write(LogChannel.General, "engine api: placed model " + model + " as character " + index + " at " + position + " on " + map);
				return npc;
			}
			catch (Exception ex)
			{
				EngineApi.Warn("spawn-model-ex", "SpawnModel " + model + " failed: " + ex.GetType().Name + ": " + ex.Message);
				return null;
			}
		}

		private LegacyNpc _legacyTalkTarget;

		/// <summary>
		/// Talking, two ways: the game's own talk action aimed at a spawned character (a tap
		/// on it, or A while facing it) fires once per talk; and a press of A within reach
		/// fires for the nearest character that listens. Dead handles are dropped.
		/// </summary>
		internal void Tick()
		{
			foreach (LegacyNpc npc in Tracked.ToArray())
			{
				npc.Advance();
			}
			string current = GlobalScope.stg.CStageMng.CurrentName;
			_spawned.RemoveAll(n => !n.Alive && n.Map != current);
			_wrapped.RemoveAll(n => n.Map != current);
			if ((_spawned.Count == 0 && _wrapped.Count == 0) || !EngineApi.InWorld) return;
			GlobalScope.pl.CBasePlayer hero = EngineApi.HeroPlayer;
			if (hero == null) return;

			// The game's talk: the hero's target is one of ours and the hero is in its TALK action (4).
			LegacyNpc talking = null;
			try
			{
				if (hero.getNowAct() == 4)
				{
					GlobalScope.chr.CCharacterEureka target = hero.getTarget();
					talking = Tracked.FirstOrDefault(n => n.IsPlayer(target));
				}
			}
			catch (Exception) { }
			if (talking != _legacyTalkTarget)
			{
				_legacyTalkTarget = talking;
				if (talking != null)
				{
					Log.Write(LogChannel.File, "engine api: the hero talks to " + talking.Model + " (character " + talking.Index + ")" + (talking.HasInteractHandler ? "" : " - nothing listens"));
					// The game has the character's attention now: a walk in progress ends here.
					talking.Stop();
					if (talking.HasInteractHandler && !EngineApi.Dialogue.IsOpen)
					{
						talking.Interact();
						return;
					}
				}
			}

			bool pressed;
			try { pressed = (GlobalScope.ds.g_Pad.edge() & 1) != 0; }
			catch (Exception) { return; }
			if (!pressed || talking != null || EngineApi.Dialogue.IsOpen || EngineApi.Dialogue.ClosedFrame == OpenFF.Game.Time.Frame) return;
			Log.Write(LogChannel.File, "engine api: A pressed near " + Tracked.Count(n => n.Alive) + " character(s) of ours" + (hero.getNowAct() == 4 ? " (hero in talk)" : ""));
			Vector3 at = EngineApi.ToUnits(hero.getPosition());
			LegacyNpc nearest = null;
			float best = float.MaxValue;
			foreach (LegacyNpc npc in Tracked)
			{
				if (!npc.Alive || !npc.HasInteractHandler) continue;
				Vector3 p = npc.Position;
				float dx = p.X - at.X, dz = p.Z - at.Z;
				float d = (float)Math.Sqrt(dx * dx + dz * dz);
				if (d <= npc.InteractRadius && d < best)
				{
					best = d;
					nearest = npc;
				}
			}
			nearest?.Interact();
		}
	}

	// ---- flags, party, audio, screen, field ----

	internal sealed class LegacyFlags : GameService, IFlags
	{
		public bool Get(uint group, uint index) => GlobalScope.FlagManager.singleton().get(group, index) == 1;

		public void Set(uint group, uint index, bool value)
		{
			if (value) GlobalScope.FlagManager.singleton().set(group, index);
			else GlobalScope.FlagManager.singleton().reset(group, index);
		}
	}

	internal sealed class LegacyParty : GameService, IParty
	{
		public int Gil
		{
			get => GlobalScope.pl.PlayerParty.instance().gold().get();
			set => GlobalScope.pl.PlayerParty.instance().gold().set(Math.Max(0, value));
		}

		public void AddItem(int itemId, int count)
		{
			if (count > 0) GlobalScope.pl.PlayerParty.instance().addItem(itemId, count);
		}

		public int ItemCount(int itemId)
		{
			try { return GlobalScope.pl.PlayerParty.instance().item().serchNormalItem((short)itemId)?.itemNumber() ?? 0; }
			catch (Exception) { return 0; }
		}

		public IReadOnlyList<PartyMember> Members
		{
			get
			{
				List<PartyMember> members = new List<PartyMember>();
				try
				{
					for (byte slot = 0; slot < 4; slot++)
					{
						GlobalScope.pl.Player player = GlobalScope.pl.PlayerParty.instance().player(slot);
						if (player != null && player.isEnable())
						{
							members.Add(Describe(player, slot));
						}
					}
				}
				catch (Exception ex) { EngineApi.Warn("party", "Members: " + ex.Message); }
				return members;
			}
		}

		public PartyMember Member(int id)
		{
			try
			{
				GlobalScope.pl.Player player = GlobalScope.pl.PlayerParty.instance().playerForId((byte)id);
				if (player == null) return null;
				int slot = -1;
				for (byte s = 0; s < 4; s++)
				{
					if (ReferenceEquals(GlobalScope.pl.PlayerParty.instance().player(s), player)) { slot = s; break; }
				}
				return Describe(player, slot);
			}
			catch (Exception) { return null; }
		}

		private static PartyMember Describe(GlobalScope.pl.Player player, int slot)
		{
			PartyMember m = new PartyMember { Id = player.playerId(), Slot = slot, Name = player.name() };
			try { m.Level = player.level().get(); } catch (Exception) { }
			try { m.Experience = player.exp().get(); } catch (Exception) { }
			try { m.Hp = player.hp().getNow(); m.MaxHp = player.hp().getLimit(); } catch (Exception) { }
			try
			{
				for (int level = 0; level < 8; level++)
				{
					m.Charges[level] = player.mp(level).getNow();
					m.MaxCharges[level] = player.mp(level).getLimit();
				}
				m.Mp = m.Charges[0];
			}
			catch (Exception) { }
			try { m.Job = player.jobManager().nowJob(); } catch (Exception) { }
			try { m.JobSkill = player.jobManager().nowJobParameter().skill().skillLevel().get(); } catch (Exception) { }
			try
			{
				GlobalScope.ys.BodyParameter body = player.bodyAndBonus();
				m.Stats.Strength = body.strength().get();
				m.Stats.Vitality = body.vitality().get();
				m.Stats.Agility = body.dexterity().get();
				m.Stats.Intellect = body.intelligence().get();
				m.Stats.Mind = body.mind().get();
				m.Stats.JobSkill = m.JobSkill;
				m.Stats.MagicDefense = player.magicDefense().magicPhylacticPower();
				m.Stats.Weakness = (Element)player.magicDefense().weakType();
				m.Stats.Resist = (Element)player.physicsDefense().antiType();
			}
			catch (Exception) { }
			try { m.Conditions = (Condition)player.condition().normalCondition(); } catch (Exception) { }
			try
			{
				for (int level = 0; level < 8; level++)
				{
					GlobalScope.pl.EquipmentMagic equipped = player.equipParameter().equipMagic((GlobalScope.pl.MAGIC_LEVEL)level);
					for (int i = 0; i < GlobalScope.pl.MAGIC_ONCE_LEVEL_EQUIP_MAX; i++)
					{
						int id = equipped.magicId(i);
						if (id > 0) m.Spells.Add(id);
					}
				}
			}
			catch (Exception) { }
			m.Alive = m.Hp > 0 && (m.Conditions & Condition.Death) == 0;
			return m;
		}

		private static GlobalScope.pl.Player PlayerOf(int id)
		{
			try { return GlobalScope.pl.PlayerParty.instance().playerForId((byte)id); }
			catch (Exception) { return null; }
		}

		public int Hurt(int id, int amount, bool canKill = false)
		{
			GlobalScope.pl.Player player = PlayerOf(id);
			if (player == null) return 0;
			try
			{
				int now = Math.Max(canKill ? 0 : 1, player.hp().getNow() - Math.Max(0, amount));
				player.hp().setNow(now);
				if (now == 0) player.condition().onDeath();
				player.updateCondition();
				return now;
			}
			catch (Exception ex) { EngineApi.Warn("party-hurt", "Hurt: " + ex.Message); return 0; }
		}

		public int Heal(int id, int amount, bool revive = false)
		{
			GlobalScope.pl.Player player = PlayerOf(id);
			if (player == null) return 0;
			try
			{
				if (player.condition().isDeath())
				{
					if (!revive) return 0;
					player.condition().offDeath();
				}
				player.hp().addNow(Math.Max(0, amount));
				int now = player.hp().getNow();
				if (now >= player.hp().getLimit() * 25 / 100) player.condition().offNearDeath();
				return now;
			}
			catch (Exception ex) { EngineApi.Warn("party-healone", "Heal: " + ex.Message); return 0; }
		}

		public void SetHp(int id, int now, int max = -1)
		{
			GlobalScope.pl.Player player = PlayerOf(id);
			if (player == null) return;
			try
			{
				if (max > 0) player.hp().setLimit(Math.Min(max, 999999));
				player.hp().setNow(Math.Max(0, now));
				if (player.hp().getNow() == 0) player.condition().onDeath();
				else if (player.condition().isDeath()) player.condition().offDeath();
				player.updateCondition();
			}
			catch (Exception ex) { EngineApi.Warn("party-sethp", "SetHp: " + ex.Message); }
		}

		public void SetCharges(int id, int level, int now, int max = -1)
		{
			GlobalScope.pl.Player player = PlayerOf(id);
			if (player == null) return;
			try
			{
				int index = Math.Clamp(level, 1, 8) - 1;
				if (max >= 0) player.mp(index).setLimit(Math.Min(max, 99));
				player.mp(index).setNow(Math.Max(0, now));
				player.setJobChangeMp(index, (byte)player.mp(index).getNow());
			}
			catch (Exception ex) { EngineApi.Warn("party-charges", "SetCharges: " + ex.Message); }
		}

		public bool GiveExperience(int id, int amount)
		{
			GlobalScope.pl.Player player = PlayerOf(id);
			if (player == null) return false;
			try { return player.levelUp(Math.Max(0, amount)); }
			catch (Exception ex) { EngineApi.Warn("party-exp", "GiveExperience: " + ex.Message); return false; }
		}

		public void SetJob(int id, Job job)
		{
			GlobalScope.pl.Player player = PlayerOf(id);
			if (player == null) return;
			try
			{
				player.changeJob((GlobalScope.pl.JOB_TYPE)(int)job);
				RefreshDisplay();
			}
			catch (Exception ex) { EngineApi.Warn("party-job", "SetJob: " + ex.Message); }
		}

		public void SetStat(int id, Stat stat, int value)
		{
			GlobalScope.pl.Player player = PlayerOf(id);
			if (player == null) return;
			try
			{
				GlobalScope.ys.BodyParameter body = player.body();
				value = Math.Clamp(value, 0, 99);
				switch (stat)
				{
					case Stat.Strength: body.strength().set(value); break;
					case Stat.Vitality: body.vitality().set(value); break;
					case Stat.Agility: body.dexterity().set(value); break;
					case Stat.Intellect: body.intelligence().set(value); break;
					case Stat.Mind: body.mind().set(value); break;
				}
				player.updateParameter();
			}
			catch (Exception ex) { EngineApi.Warn("party-stat", "SetStat: " + ex.Message); }
		}

		public bool LearnSpell(int id, int spellId)
		{
			GlobalScope.pl.Player player = PlayerOf(id);
			if (player == null) return false;
			try
			{
				GlobalScope.itm.MagicParameter p = GlobalScope.itm.ItemManager.instance().magicParameter((short)spellId);
				if (p == null) return false;
				GlobalScope.pl.EquipmentMagic slots = player.equipParameter().equipMagic((GlobalScope.pl.MAGIC_LEVEL)p.magicClass());
				return slots.equip(spellId) != GlobalScope.pl.EquipmentMagic.NO_MAGIC_ID;
			}
			catch (Exception ex) { EngineApi.Warn("party-learn", "LearnSpell: " + ex.Message); return false; }
		}

		public bool ForgetSpell(int id, int spellId)
		{
			GlobalScope.pl.Player player = PlayerOf(id);
			if (player == null) return false;
			try
			{
				for (int level = 0; level < 8; level++)
				{
					GlobalScope.pl.EquipmentMagic slots = player.equipParameter().equipMagic((GlobalScope.pl.MAGIC_LEVEL)level);
					for (int i = 0; i < GlobalScope.pl.MAGIC_ONCE_LEVEL_EQUIP_MAX; i++)
					{
						if (slots.magicId(i) == spellId) return slots.release(i);
					}
				}
				return false;
			}
			catch (Exception ex) { EngineApi.Warn("party-forget", "ForgetSpell: " + ex.Message); return false; }
		}

		public IReadOnlyList<ItemStack> Items
		{
			get
			{
				List<ItemStack> list = new List<ItemStack>();
				try
				{
					GlobalScope.itm.PossessionItemManager bag = GlobalScope.pl.PlayerParty.instance().item();
					for (int i = 0; i < 384; i++)
					{
						GlobalScope.itm.PossessionItem entry = bag.normalItem(i);
						if (entry != null && entry.itemId() > 0 && entry.itemNumber() > 0)
						{
							list.Add(new ItemStack { ItemId = entry.itemId(), Count = entry.itemNumber() });
						}
					}
					for (int i = 0; i < 64; i++)
					{
						GlobalScope.itm.PossessionItem entry = bag.importantItem(i);
						if (entry != null && entry.itemId() > 0)
						{
							list.Add(new ItemStack { ItemId = entry.itemId(), Count = Math.Max(1, (int)entry.itemNumber()) });
						}
					}
				}
				catch (Exception ex) { EngineApi.Warn("bag", "Items: " + ex.Message); }
				return list;
			}
		}

		public bool RemoveItem(int itemId, int count)
		{
			if (count <= 0) return true;
			try
			{
				GlobalScope.itm.PossessionItem entry = GlobalScope.pl.PlayerParty.instance().item().serchNormalItem((short)itemId);
				if (entry == null || entry.itemNumber() < count) return false;
				int left = entry.itemNumber() - count;
				entry.setItemNumber(left);
				if (left <= 0) entry.setItemId(-1);
				return true;
			}
			catch (Exception ex) { EngineApi.Warn("bag-remove", "RemoveItem: " + ex.Message); return false; }
		}

		internal static EquipSlot SlotOf(int itemId)
		{
			GlobalScope.itm.ItemManager items = GlobalScope.itm.ItemManager.instance();
			if (items.weaponParameter((short)itemId) != null) return EquipSlot.RightHand;
			GlobalScope.itm.ProtectionParameter armour = items.protectionParameter((short)itemId);
			if (armour == null) return EquipSlot.None;
			switch (armour.system())
			{
				case 0: return EquipSlot.LeftHand;   // shield
				case 1: return EquipSlot.Head;
				case 2: return EquipSlot.Body;
				case 3: return EquipSlot.Arm;
			}
			return EquipSlot.None;
		}

		public bool Equip(int id, int itemId, EquipSlot slot = EquipSlot.Auto)
		{
			GlobalScope.pl.Player player = PlayerOf(id);
			if (player == null) return false;
			try
			{
				if (slot == EquipSlot.Auto) slot = SlotOf(itemId);
				if (slot < 0) return false;
				GlobalScope.itm.PossessionItem entry = GlobalScope.pl.PlayerParty.instance().item().serchNormalItem((short)itemId);
				if (entry == null || entry.itemNumber() <= 0) return false;
				return player.doEquip((int)slot, (short)itemId, sort: true);
			}
			catch (Exception ex) { EngineApi.Warn("equip", "Equip: " + ex.Message); return false; }
		}

		public void Unequip(int id, EquipSlot slot)
		{
			GlobalScope.pl.Player player = PlayerOf(id);
			if (player == null || slot < 0) return;
			try { player.releaseEquipItem((int)slot, sort: true); }
			catch (Exception ex) { EngineApi.Warn("unequip", "Unequip: " + ex.Message); }
		}

		public int Equipped(int id, EquipSlot slot)
		{
			GlobalScope.pl.Player player = PlayerOf(id);
			if (player == null || slot < 0) return 0;
			try
			{
				GlobalScope.pl.EquipmentItem point = player.equipParameter().equipPoint((int)slot);
				int item = point?.equipItemInfo()?.itemId_ ?? 0;
				return item > 0 ? item : 0;
			}
			catch (Exception) { return 0; }
		}

		public void Inflict(int id, Condition conditions) => SetConditions(id, conditions, true);
		public void Cure(int id, Condition conditions) => SetConditions(id, conditions, false);

		private static void SetConditions(int id, Condition conditions, bool on)
		{
			GlobalScope.pl.Player player = PlayerOf(id);
			if (player == null) return;
			try
			{
				GlobalScope.ys.Condition c = player.condition();
				if ((conditions & Condition.Death) != 0) { if (on) { c.onDeath(); player.hp().setNow(0); } else { c.offDeath(); if (player.hp().getNow() == 0) player.hp().setNow(1); } }
				if ((conditions & Condition.Stone) != 0) { if (on) c.onStone(); else c.offStone(); }
				if ((conditions & Condition.Frog) != 0) { if (on) c.onFrog(); else c.offFrog(); }
				if ((conditions & Condition.Silence) != 0) { if (on) c.onSilence(); else c.offSilence(); }
				if ((conditions & Condition.Mini) != 0) { if (on) c.onLilliput(); else c.offLilliput(); }
				if ((conditions & Condition.Blind) != 0) { if (on) c.onDarkness(); else c.offDarkness(); }
				if ((conditions & Condition.Poison) != 0) { if (on) c.onPoison(); else c.offPoison(); }
				if ((conditions & Condition.NearDeath) != 0) { if (on) c.onNearDeath(); else c.offNearDeath(); }
				player.updateParameter();
			}
			catch (Exception ex) { EngineApi.Warn("party-condition", (on ? "Inflict: " : "Cure: ") + ex.Message); }
		}

		public bool AddMember(int id)
		{
			try
			{
				bool added = GlobalScope.pl.PlayerParty.instance().addPlayer((byte)id);
				if (added)
				{
					GlobalScope.pl.PlayerParty.instance().clearMemory();
					RefreshDisplay();
				}
				return added;
			}
			catch (Exception ex) { EngineApi.Warn("party-add", "AddMember: " + ex.Message); return false; }
		}

		public bool RemoveMember(int id)
		{
			try
			{
				bool removed = GlobalScope.pl.PlayerParty.instance().releasePlayer((byte)id);
				if (removed) RefreshDisplay();
				return removed;
			}
			catch (Exception ex) { EngineApi.Warn("party-remove", "RemoveMember: " + ex.Message); return false; }
		}

		public void SetLevel(int id, int level)
		{
			try { GlobalScope.pl.PlayerParty.instance().playerForId((byte)id)?.growParameter((byte)Math.Clamp(level, 1, 99)); }
			catch (Exception ex) { EngineApi.Warn("party-level", "SetLevel: " + ex.Message); }
		}

		public void HealAll()
		{
			try { GlobalScope.pl.PlayerParty.instance().fineAll(); }
			catch (Exception ex) { EngineApi.Warn("party-heal", "HealAll: " + ex.Message); }
		}

		private static void RefreshDisplay()
		{
			if (!EngineApi.InWorld) return;
			try { GlobalScope.CCastCommandTransit.getInstance().cast_BaseSystem().changePlayerCharDisplay(); }
			catch (Exception) { }
		}
	}

	internal sealed class LegacyAudio : GameService, IAudio
	{
		public void PlaySe(int archive, int number, int volume = 127, int pan = 64)
		{
			GlobalScope.MatrixSound.MtxSENDS_Play(archive, number, volume, pan);
		}

		public void PlayBgm(int number, int volume = 127, int fadeInFrames = 0)
		{
			GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().stop(0, GlobalScope.MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
			GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().play(number, volume, fadeInFrames, GlobalScope.MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
		}

		public void StopBgm(int fadeOutFrames = 15)
		{
			GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().stop(fadeOutFrames, GlobalScope.MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
		}
	}

	internal sealed class LegacyScreen : GameService, IScreen
	{
		public void FadeOut(int frames, bool white = false)
		{
			GlobalScope.dgs.CFade.FADE_TYPE type = white ? GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE : GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK;
			GlobalScope.dgs.CFade.Main().fadeOut(frames, type);
			GlobalScope.dgs.CFade.Sub().fadeOut(frames, type);
		}

		public void FadeIn(int frames)
		{
			GlobalScope.dgs.CFade.Main().fadeIn(frames);
			GlobalScope.dgs.CFade.Sub().fadeIn(frames);
		}

		public bool Faded => GlobalScope.dgs.CFade.Main().isFaded();

		// The field's own flash (a damage floor's red), a 15-bit colour.
		public void Flash(Color color, int frames = 8, int interval = 2)
		{
			if (!EngineApi.InWorld) return;
			try
			{
				GlobalScope.wld.CBaseSystem world = GlobalScope.wld.CBaseSystem.Current;
				if (world == null) return;
				ushort rgb = GlobalScope.GX_RGB(color.R >> 3, color.G >> 3, color.B >> 3);
				world.ScrFlash().setFlash((short)Math.Max(1, frames), (byte)Math.Clamp(interval, 1, 255), rgb);
			}
			catch (Exception ex) { EngineApi.Warn("flash", "Screen.Flash: " + ex.Message); }
		}

		// The battle's pop-up numbers, drawn by the 2D manager the field runs too. Their sprite
		// sheet is the battle's; it is loaded here on first use on a map and released when the
		// map or the world part goes, since the battle loads its own copy.
		private GlobalScope.btl.Damage _numbers;
		private GlobalScope.btl.Hit _hits;
		private bool _sheetLoaded;
		private int _slot;

		private bool EnsureSheet()
		{
			if (!EngineApi.InWorld) return false;
			if (!_sheetLoaded)
			{
				GlobalScope.u2d.PopUp.puInitializeSystem();
				_sheetLoaded = true;
				_numbers = new GlobalScope.btl.Damage();
				_hits = new GlobalScope.btl.Hit();
			}
			return true;
		}

		public void PopNumber(Vector3 at, int value, bool heal = false)
		{
			try
			{
				if (!EnsureSheet()) return;
				value = Math.Clamp(Math.Abs(value), 0, 9999);
				_numbers.create(_slot, value, EngineApi.ToFx(at), heal ? 1 : 0);
				_slot = (_slot + 1) % 12;
			}
			catch (Exception ex) { EngineApi.Warn("pop", "Screen.PopNumber: " + ex.Message); }
		}

		public void PopMiss(Vector3 at)
		{
			try
			{
				if (!EnsureSheet()) return;
				_hits.create(0, EngineApi.ToFx(at), GlobalScope.u2d.PopUpHitNumber.puhnKIND.puhnkMISS);
			}
			catch (Exception ex) { EngineApi.Warn("pop-miss", "Screen.PopMiss: " + ex.Message); }
		}

		internal void Tick()
		{
			if (_sheetLoaded && !EngineApi.InWorld)
			{
				try { GlobalScope.u2d.PopUp.puReleaseSystem(); } catch (Exception) { }
				_sheetLoaded = false;
				_numbers = null;
				_hits = null;
			}
		}
	}

	internal sealed class LegacyCamera : GameService, ICamera
	{
		private GlobalScope.cmr.CWorldCamera Camera
		{
			get
			{
				if (!EngineApi.InWorld) return null;
				try { return GlobalScope.CCastCommandTransit.getInstance().cast_FieldCamera(); }
				catch (Exception) { return null; }
			}
		}

		public Vector3 Position => EngineApi.ToUnits(Camera?.Pos());
		public Vector3 Target => EngineApi.ToUnits(Camera?.Trg());

		public void MoveTo(Vector3 position)
		{
			GlobalScope.cmr.CWorldCamera cam = Camera;
			if (cam == null) return;
			cam.Mode_set(GlobalScope.cmr.CWorldCamera.MODE.MODE_FREE);
			cam.Pos_set(EngineApi.ToFx(position));
			GlobalScope.VEC_Set(cam.PosOffset(), 0, 0, 0);
		}

		public void LookAt(Vector3 target)
		{
			GlobalScope.cmr.CWorldCamera cam = Camera;
			if (cam == null) return;
			cam.Mode_set(GlobalScope.cmr.CWorldCamera.MODE.MODE_FREE);
			cam.setTrg(EngineApi.ToFx(target));
		}

		public void Follow()
		{
			GlobalScope.cmr.CWorldCamera cam = Camera;
			GlobalScope.pl.CBasePlayer hero = EngineApi.HeroPlayer;
			if (cam == null || hero == null) return;
			cam.Mode_set(GlobalScope.cmr.CWorldCamera.MODE.MODE_AUTOFOLLOW);
			GlobalScope.chr.CBaseCharacter.setLookIndex(EngineApi.HeroIndex);
			cam.setTrg(hero.getPosition());
		}

		public void Shake(int frames, float strength = 1f, int speed = 2)
		{
			GlobalScope.cmr.CWorldCamera cam = Camera;
			if (cam == null) return;
			int amount = (int)Math.Round(Math.Max(0f, strength) * 4096);
			Game.Guard("Camera.Shake", () => cam.composit2.startVibration(GlobalScope.cmr.CCameraVibration.VIBRATION_STATE.VIBRATION_EXE_1,
				Math.Max(1, frames), Math.Max(1, speed), amount, amount, 0, false));
		}

		public void Zoom(int degrees)
		{
			GlobalScope.cmr.CWorldCamera cam = Camera;
			if (cam == null) return;
			Game.Guard("Camera.Zoom", () => cam.composit.setZoom(4096 * degrees));
		}

		public void Reset()
		{
			if (!EngineApi.InWorld) return;
			Game.Guard("Camera.Reset", () => GlobalScope.CCastCommandTransit.getInstance().cast_BaseSystem().setupCamera());
		}

		// The renderer's own matrices: the field camera writes the camera matrix (world -> eye)
		// and the perspective matrix (eye -> clip) into NNS_G3dGlb each frame it executes
		// (sys3d.CCamera.execute -> NNS_G3dGlbLookAt / NNS_G3dGlbPerspective), both in fx32.
		// A 180-degree flip is applied when the port flips the screen. The 3D view fills the
		// window, which is the 800x480 text space Game.Draw uses.
		public Vector2? WorldToScreen(Vector3 world)
		{
			if (!EngineApi.InWorld) return null;
			try
			{
				GlobalScope.NNSG3dGlb glb = GlobalScope.NNS_G3dGlb;
				GlobalScope.VecFx32 eye = new GlobalScope.VecFx32();
				GlobalScope.MTX_MultVec43(EngineApi.ToFx(world), glb.cameraMtx, eye);
				double ex = eye.x / 4096.0, ey = eye.y / 4096.0, ez = eye.z / 4096.0;
				GlobalScope.MtxFx44 p = glb.projMtx;
				// _ij is column i, row j (GL order), so clip_j = sum_i _ij * v_i.
				double cx = (p._00 * ex + p._10 * ey + p._20 * ez + p._30) / 4096.0;
				double cy = (p._01 * ex + p._11 * ey + p._21 * ez + p._31) / 4096.0;
				double cw = (p._03 * ex + p._13 * ey + p._23 * ez + p._33) / 4096.0;
				if (cw <= 1e-6) return null;
				double nx = cx / cw, ny = cy / cw;
				if (GlobalScope.flipScreen != 0) { nx = -nx; ny = -ny; }
				return new Vector2((float)((nx + 1.0) * 0.5 * 800.0), (float)((1.0 - ny) * 0.5 * 480.0));
			}
			catch (Exception ex) { EngineApi.Warn("w2s", "Camera.WorldToScreen: " + ex.Message); return null; }
		}

		public bool OnScreen(Vector3 world)
		{
			Vector2? s = WorldToScreen(world);
			return s.HasValue && s.Value.X >= 0 && s.Value.X <= 800 && s.Value.Y >= 0 && s.Value.Y <= 480;
		}
	}

	internal sealed class LegacyEffects : GameService, IEffects
	{
		// Effect packs loaded through the API on this map (the game frees them all with the map).
		private readonly HashSet<int> _loaded = new HashSet<int>();
		private string _loadedMap;
		private readonly List<Follower> _followers = new List<Follower>();

		private sealed class Follower
		{
			public int Id;
			public Npc Target;
			public bool Hero;
			public Vector3 Offset;
		}

		private GlobalScope.eff.CEffectMng Manager => GlobalScope.eff.CEffectMng.instance();

		public int Spawn(int category, int member, Vector3 position)
		{
			if (!EngineApi.InWorld) return -1;
			try
			{
				int id = Manager.create(category, member);
				if (id != -1)
				{
					Manager.setPosition(id, EngineApi.ToFx(position));
				}
				return id;
			}
			catch (Exception ex) { EngineApi.Warn("effect", "Effects.Spawn: " + ex.Message); return -1; }
		}

		public void Remove(int id)
		{
			try
			{
				if (Manager.isEffectObject(id))
				{
					Manager.deleteEffect(id);
				}
				_followers.RemoveAll(f => f.Id == id);
			}
			catch (Exception) { }
		}

		public bool Alive(int id)
		{
			try { return Manager.isEffectObject(id); }
			catch (Exception) { return false; }
		}

		public bool Load(int category)
		{
			if (!EngineApi.InWorld) return false;
			ForgetOldMap();
			if (_loaded.Contains(category)) return true;
			try
			{
				if (Manager.getLoadedEfpNum() >= 5) return false;
				int before = Manager.getLoadedEfpNum();
				Manager.loadEfp("/EFFECT/e" + category.ToString("D3") + ".efp");
				bool ok = Manager.getLoadedEfpNum() > before;
				if (ok) _loaded.Add(category);
				else EngineApi.Warn("efp-" + category, "Effects.Load: pack e" + category.ToString("D3") + " did not load");
				return ok;
			}
			catch (Exception ex) { EngineApi.Warn("efp", "Effects.Load: " + ex.Message); return false; }
		}

		public bool Loaded(int category)
		{
			ForgetOldMap();
			// 102 is w_common, which every map carries.
			return category == 102 || _loaded.Contains(category);
		}

		private void ForgetOldMap()
		{
			string map = GlobalScope.stg.CStageMng.CurrentName;
			if (map != _loadedMap)
			{
				_loaded.Clear();
				_followers.Clear();
				_loadedMap = map;
			}
		}

		public void Move(int id, Vector3 position)
		{
			try { if (Manager.isEffectObject(id)) Manager.setPosition(id, EngineApi.ToFx(position)); }
			catch (Exception) { }
		}

		public void Scale(int id, float scale)
		{
			try
			{
				if (!Manager.isEffectObject(id)) return;
				int s = (int)(Math.Max(0.01f, scale) * 4096f);
				Manager.setScale(id, new GlobalScope.VecFx32(s, s, s));
			}
			catch (Exception) { }
		}

		public void Pause(int id, bool paused)
		{
			try { if (Manager.isEffectObject(id)) Manager.setPause(id, paused); }
			catch (Exception) { }
		}

		public void Follow(int id, Npc target, Vector3 offset = default)
		{
			if (target == null || id < 0) return;
			_followers.RemoveAll(f => f.Id == id);
			_followers.Add(new Follower { Id = id, Target = target, Offset = offset });
		}

		public void FollowHero(int id, Vector3 offset = default)
		{
			if (id < 0) return;
			_followers.RemoveAll(f => f.Id == id);
			_followers.Add(new Follower { Id = id, Hero = true, Offset = offset });
		}

		internal void Tick()
		{
			if (_followers.Count == 0) return;
			if (!EngineApi.InWorld) { _followers.Clear(); return; }
			for (int i = _followers.Count - 1; i >= 0; i--)
			{
				Follower f = _followers[i];
				bool alive;
				try { alive = Manager.isEffectObject(f.Id); } catch (Exception) { alive = false; }
				if (!alive || (!f.Hero && (f.Target == null || !f.Target.Alive)))
				{
					_followers.RemoveAt(i);
					continue;
				}
				Vector3 at = (f.Hero ? OpenFF.Game.Hero.Position : f.Target.Position) + f.Offset;
				Move(f.Id, at);
			}
		}
	}

	internal sealed class LegacyBattle : GameService, IBattle
	{
		public bool InBattle
		{
			get
			{
				try { return ((GlobalScope.GAMEPART)GlobalScope.sys.FF3PartSys.getCurrentPart()) == GlobalScope.GAMEPART.GAMEPART_BATTLE; }
				catch (Exception) { return false; }
			}
		}

		public void Start(int monsterParty, int battleMap = 0)
		{
			if (GameProfile.IsFf4)
			{
				// FF4's battles are the OpenFF battle over the unified tables.
				if (Ff4Battle.Instance == null || !Ff4Battle.Instance.StartParty(monsterParty)) EngineApi.Warn("battle", "Battle.Start: no FF4 encounter group " + monsterParty);
				return;
			}
			if (!EngineApi.InWorld)
			{
				EngineApi.Warn("battle", "Battle.Start: not on a map");
				return;
			}
			Game.Guard("Battle.Start", () =>
			{
				GlobalScope.btl.OutsideToBattle.getInstance().initializeMonster().setMonsterPartyId((short)monsterParty);
				GlobalScope.btl.OutsideToBattle.getInstance().initializeBattleMap().setBattleMapId((byte)battleMap);
				GlobalScope.wld.CBaseSystem.setBattle(b: true);
			});
		}

		private bool _escape = true;

		public bool EscapeAllowed
		{
			get => _escape;
			set
			{
				_escape = value;
				Game.Guard("Battle.EscapeAllowed", () =>
				{
					if (value) GlobalScope.btl.OutsideToBattle.getInstance().onEscape();
					else GlobalScope.btl.OutsideToBattle.getInstance().offEscape();
				});
			}
		}
	}

	internal sealed class LegacyField : GameService, IField
	{
		/// <summary>The stage's name; on the overworld, whose stage has none, the chip's stage (f00 from f00_48).</summary>
		public string Map
		{
			get
			{
				string name = GlobalScope.stg.CStageMng.CurrentName;
				if (!string.IsNullOrEmpty(name)) return name;
				try
				{
					string chip = GlobalScope.stageMng?.getChipName();
					return !string.IsNullOrEmpty(chip) && chip.Length >= 3 ? chip.Substring(0, 3) : name;
				}
				catch (Exception) { return name; }
			}
		}

		public bool Encounters
		{
			get
			{
				try { return GlobalScope.wld.CWorldOutSideData.getInstance().canEncount(); }
				catch (Exception) { return false; }
			}
			set
			{
				Game.Guard("Field.Encounters", () => GlobalScope.wld.CWorldOutSideData.getInstance().setCanEncount(value));
			}
		}

		// The ground query the characters make each frame (chr.CCharacterEureka.calculateBottom):
		// an arrow straight down from a little above the point, through every active collision
		// restrictor (the stage's chips), in the stage's own space (its world matrix inverted),
		// against material attribute 1 (walkable ground). The nearest hit wins.
		private const int AboveFx = 28672;          // 7 units up, as the characters start
		private const int ReachFx = 28672 + 81920;  // to 20 units below the point

		public float? GroundHeight(Vector3 at)
		{
			GlobalScope.VecFx32 hit = GroundHit(at);
			return hit == null ? (float?)null : hit.y / 4096f;
		}

		public Vector3 OnGround(Vector3 at)
		{
			float? y = GroundHeight(at);
			return y.HasValue ? new Vector3(at.X, y.Value, at.Z) : at;
		}

		public bool Walkable(Vector3 at)
		{
			return GroundHit(at) != null;
		}

		private GlobalScope.VecFx32 GroundHit(Vector3 at)
		{
			try { return GroundHitFx(EngineApi.ToFx(at)); }
			catch (Exception ex) { EngineApi.Warn("ground", "Field.GroundHeight: " + ex.Message); return null; }
		}

		/// <summary>The same query in engine units; the FF4 scene shadows' ground callback (ds.sys3d.CShadowObject.GroundQuery) uses it.</summary>
		internal static GlobalScope.VecFx32 GroundHitFx(GlobalScope.VecFx32 at)
		{
			if (!EngineApi.InWorld) return null;
			{
				GlobalScope.stg.CStageMng stage = GlobalScope.stageMng;
				if (stage == null) return null;
				GlobalScope.MtxFx43 inv = new GlobalScope.MtxFx43();
				GlobalScope.MtxFx43 wld = new GlobalScope.MtxFx43();
				stage.getInvWldMtx(inv);
				stage.getWldMtx(wld);
				GlobalScope.VecFx32 start = new GlobalScope.VecFx32(at.x, at.y, at.z);
				start.y += AboveFx;
				GlobalScope.MTX_MultVec43(start, inv, start);
				GlobalScope.VecFx32 down = new GlobalScope.VecFx32(0, -4096, 0);
				GlobalScope.mcl.CollisionResult result = new GlobalScope.mcl.CollisionResult();
				GlobalScope.VecFx32 best = null;
				int bestLength = int.MaxValue;
				for (GlobalScope.dgs.CRestrictor r = (GlobalScope.dgs.CRestrictor)GlobalScope.dgs.DGSLinkedList<GlobalScope.dgs.CRestrictor>.dgsllBase(); r != null; r = (GlobalScope.dgs.CRestrictor)r.dgsllNext())
				{
					if (!r.rorActivity()) continue;
					result.clean();
					if (!r.rorEvaluateArrow(start, down, ReachFx, 1, result)) continue;
					if (result.length < bestLength)
					{
						bestLength = result.length;
						best = new GlobalScope.VecFx32(result.pos);
					}
				}
				if (best == null) return null;
				GlobalScope.MTX_MultVec43(best, wld, best);
				return best;
			}
		}

		public void Warp(string map, Vector3 position, int facing = 0)
		{
			if (!EngineApi.InWorld || string.IsNullOrEmpty(map))
			{
				EngineApi.Warn("warp", "Warp: not on a map");
				return;
			}
			GlobalScope.VecFx32 rot = new GlobalScope.VecFx32();
			rot.set(0, (facing & 7) * 8192, 0);
			GlobalScope.CCastCommandTransit transit = GlobalScope.CCastCommandTransit.getInstance();
			transit.castParam_MapJump().initialize();
			transit.castParam_MapJump().setUp(map, 0, EngineApi.ToFx(position), rot, _Flag: true);
			transit.cast_BaseSystem().setMapJump(b: true);
		}
	}
}

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

namespace FF3
{
	internal static class EngineApi
	{
		public static readonly LegacyDialogue Dialogue = new LegacyDialogue();
		public static readonly LegacyNpcs Npcs = new LegacyNpcs();

		public static void Register()
		{
			OpenFF.Game.Services.Register(Dialogue);
			OpenFF.Game.Services.Register(new LegacyHero());
			OpenFF.Game.Services.Register(Npcs);
			OpenFF.Game.Services.Register(new LegacyFlags());
			OpenFF.Game.Services.Register(new LegacyParty());
			OpenFF.Game.Services.Register(new LegacyAudio());
			OpenFF.Game.Services.Register(new LegacyScreen());
			OpenFF.Game.Services.Register(new LegacyField());
		}

		/// <summary>Once per frame, after the legacy tick: the message window's end, and talking to spawned characters.</summary>
		public static void Tick()
		{
			Dialogue.Tick();
			Npcs.Tick();
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

		public bool IsOpen => _shown || _pending != null;
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

		public void Say(string text, string speaker = null)
		{
			GlobalScope.wld.CMessageWindow window = Window;
			if (window == null)
			{
				EngineApi.Warn("say", "Say: no message window (not on a map)");
				return;
			}
			if (!window.isMadeWindow())
			{
				window.createWindow(1);
			}
			if (window.isWindowOpen())
			{
				Show(window, text ?? "");
			}
			else
			{
				_pending = text ?? "";
			}
		}

		private void Show(GlobalScope.wld.CMessageWindow window, string text)
		{
			window.createText(text, 0);
			window.setProgressIconActivity(_SendMessage: true);
			_pending = null;
			_shown = true;
		}

		public void Close()
		{
			GlobalScope.wld.CMessageWindow window = Window;
			bool wasOpen = _shown || _pending != null;
			if (window != null && wasOpen)
			{
				window.release();
			}
			_pending = null;
			if (wasOpen)
			{
				_shown = false;
				Game.Guard("Dialogue.Closed", () => Closed?.Invoke());
			}
		}

		/// <summary>The frame the window closed on; a press that dismissed it must not also talk to someone.</summary>
		internal long ClosedFrame = -1;

		/// <summary>The player has tapped past the text (isNextPageButton, what WaitInputSendMessage waits for): take the window down.</summary>
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
				if (window.isWindowOpen())
				{
					Show(window, _pending);
				}
				return;
			}
			if (!_shown) return;
			if (window == null || !window.isMadeWindow() || window.isNextPageButton())
			{
				Close();
				ClosedFrame = OpenFF.Game.Time.Frame;
			}
		}
	}

	// ---- the hero ----

	internal sealed class LegacyHero : GameService, IHero
	{
		private bool _frozen;

		public bool Present => EngineApi.HeroPlayer != null;
		public Vector3 Position => EngineApi.ToUnits(EngineApi.HeroPlayer?.getPosition());
		public float Yaw => EngineApi.HeroPlayer == null ? 0f : EngineApi.RotToYaw(EngineApi.HeroPlayer.getRotation().y);
		public string Model => EngineApi.HeroPlayer?.getModelName();
		public bool Frozen => _frozen;

		public void Teleport(Vector3 position)
		{
			GlobalScope.pl.CBasePlayer hero = EngineApi.HeroPlayer;
			if (hero == null) return;
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
			EngineApi.Players.setPlayerStart(index);
			GlobalScope.dv.CDeviceManager.getInstance().Pad().setActivity(b: true);
			_frozen = false;
		}
	}

	// ---- characters ----

	internal sealed class LegacyNpc : Npc
	{
		internal int Index;
		internal string Map;
		private bool _removed;
		// A walk in progress: the host steps the position itself each frame. The legacy
		// MoveSys route (MoveCharaImp) leaves a character that has no script cast behind it
		// standing still - its acceleration is reset the frame after - so the API walks the
		// character by hand and lets the human's WALK action play the motion.
		private GlobalScope.VecFx32 _walkTarget;
		private GlobalScope.VecFx32 _walkStep;
		private int _walkFrames;

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
		public override bool Moving => _walkFrames > 0 && Player != null;

		public override void Teleport(Vector3 position)
		{
			GlobalScope.pl.CBasePlayer p = Player;
			if (p == null) return;
			_walkFrames = 0;
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
			_walkTarget = EngineApi.ToFx(position);
			GlobalScope.VecFx32 from = p.getPosition();
			_walkStep = new GlobalScope.VecFx32();
			_walkStep.set((_walkTarget.x - from.x) / frames, (_walkTarget.y - from.y) / frames, (_walkTarget.z - from.z) / frames);
			_walkFrames = frames;
			EngineApi.TurnToward(p, _walkStep);
			try
			{
				EngineApi.Players.PlayerHuman(Index)?.setAction(GlobalScope.pl.CPlayerHuman.ACTION_ID.ACTION_ID_WALK);
			}
			catch (Exception) { }
		}

		public override void Stop()
		{
			if (_walkFrames <= 0) return;
			_walkFrames = 0;
			try
			{
				EngineApi.Players.PlayerHuman(Index)?.setAction(GlobalScope.pl.CPlayerHuman.ACTION_ID.ACTION_ID_WAIT);
			}
			catch (Exception) { }
		}

		/// <summary>One frame of a walk in progress; called by the host each tick.</summary>
		internal void Advance()
		{
			if (_walkFrames <= 0) return;
			GlobalScope.pl.CBasePlayer p = Player;
			if (p == null)
			{
				_walkFrames = 0;
				return;
			}
			_walkFrames--;
			GlobalScope.VecFx32 next = new GlobalScope.VecFx32();
			if (_walkFrames == 0)
			{
				next.copy(_walkTarget);
			}
			else
			{
				GlobalScope.VecFx32 now = p.getPosition();
				next.set(now.x + _walkStep.x, now.y + _walkStep.y, now.z + _walkStep.z);
			}
			p.getPrePosition_set(p.getPosition());
			p.setPosition(next);
			EngineApi.TurnToward(p, _walkStep);
			if (_walkFrames == 0)
			{
				try
				{
					EngineApi.Players.PlayerHuman(Index)?.setAction(GlobalScope.pl.CPlayerHuman.ACTION_ID.ACTION_ID_WAIT);
				}
				catch (Exception) { }
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
	}

	internal sealed class LegacyNpcs : GameService, INpcs
	{
		private readonly List<LegacyNpc> _spawned = new List<LegacyNpc>();

		public IReadOnlyList<Npc> Spawned => _spawned;

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

		private LegacyNpc _legacyTalkTarget;

		/// <summary>
		/// Talking, two ways: the game's own talk action aimed at a spawned character (a tap
		/// on it, or A while facing it) fires once per talk; and a press of A within reach
		/// fires for the nearest character that listens. Dead handles are dropped.
		/// </summary>
		internal void Tick()
		{
			foreach (LegacyNpc npc in _spawned)
			{
				npc.Advance();
			}
			_spawned.RemoveAll(n => !n.Alive && n.Map != GlobalScope.stg.CStageMng.CurrentName);
			if (_spawned.Count == 0 || !EngineApi.InWorld) return;
			GlobalScope.pl.CBasePlayer hero = EngineApi.HeroPlayer;
			if (hero == null) return;

			// The game's talk: the hero's target is one of ours and the hero is in its TALK action (4).
			LegacyNpc talking = null;
			try
			{
				if (hero.getNowAct() == 4)
				{
					GlobalScope.chr.CCharacterEureka target = hero.getTarget();
					talking = _spawned.FirstOrDefault(n => n.IsPlayer(target));
				}
			}
			catch (Exception) { }
			if (talking != _legacyTalkTarget)
			{
				_legacyTalkTarget = talking;
				if (talking != null)
				{
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
			Vector3 at = EngineApi.ToUnits(hero.getPosition());
			LegacyNpc nearest = null;
			float best = float.MaxValue;
			foreach (LegacyNpc npc in _spawned)
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
	}

	internal sealed class LegacyField : GameService, IField
	{
		public string Map => GlobalScope.stg.CStageMng.CurrentName;

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

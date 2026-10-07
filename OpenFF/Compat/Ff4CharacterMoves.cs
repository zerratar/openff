// FF4's moves and turns of a scene's characters, as libff4 makes them. FF3's handlers move a character through its
// MoveSys (an acceleration, a top speed, the turn system easing it round, the map's walls); FF4's register an object
// strategy on the character instead (its OSDriver, CharacterObject +0x2a0), run once a frame:
//
// - OSLinerMoveByFrame (MoveCharacter_AbsoluteCoordination2 / _RelativeCoordination): the way to the target split into
//   equal steps (integer division of the difference by the frames), one a frame, the target itself on the last; no
//   frames puts it there at once. Unless the character's turn is fixed (SetCharacter_FixedTurn, behaviour flag 1), an
//   OSRotationByFrame comes with it: a turn toward the target in 5 frames.
// - OSRotationByFrame (TurnCharacter_AbsoluteAngle2 / _RelativeAngle2 / _AbsoluteCoordination2 / _LookCharacter2):
//   the yaw stepped by the difference over the frames, the short way round, then the target; the turns play motion
//   1005 while turning and 1000 after (their last operand 0), if the model has them.
//
// MoveCharacter_EndAutoIdle waits while a move is running, TurnCharacter_EndAutoIdle while a turn is (FF3's passed).
// A character takes four at once (OSDriver::osdRegisterOS, slot 7: the first free of 0..3).

using System;
using System.Collections.Generic;

namespace OpenFF.Client
{
	internal static class Ff4CharacterMoves
	{
		private abstract class Strategy
		{
			public GlobalScope.pl.CBasePlayer Player;
			public bool Done;
			public string Stage;   // the map it was made on
			public abstract void Update();
		}

		private sealed class LinerMove : Strategy
		{
			public readonly GlobalScope.VecFx32 Target = new GlobalScope.VecFx32();
			public int Frames, Dx, Dy, Dz;

			public override void Update()
			{
				if (Frames < 2)
				{
					Place(Player, Target);
					Done = true;
					return;
				}
				Frames--;
				GlobalScope.VecFx32 at = Player.getPosition();
				Place(Player, new GlobalScope.VecFx32(at.x + Dx, at.y + Dy, at.z + Dz));
			}
		}

		private sealed class Rotation : Strategy
		{
			public int Yaw, Frames, Step, EndMotion;
			public bool Negative;

			public override void Update()
			{
				int f = Frames--;
				GlobalScope.VecFx32 rot = new GlobalScope.VecFx32(Player.getRotation());
				if (f < 1)
				{
					rot.y = Yaw & 0xFFFF;
					if (EndMotion != -1 && Has(Player, EndMotion)) Start(Player, EndMotion, 2);
					Done = true;
				}
				else
				{
					rot.y = (rot.y + (Negative ? -Step : Step)) & 0xFFFF;
				}
				Face(Player, rot);
			}
		}

		private static readonly List<Strategy> _running = new List<Strategy>();

		// ---- the commands ----

		/// <summary>(cast, x, y, z, frames): straight to the point.</summary>
		public static void MoveAbsolute2(GlobalScope.ScriptEngine engine)
		{
			int cast = engine.getWord();
			GlobalScope.VecFx32 target = new GlobalScope.VecFx32((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
			int frames = engine.getWord();
			GlobalScope.pl.CBasePlayer player = PlayerOf(cast);
			if (player != null) Move(player, target, frames);
		}

		/// <summary>(cast, to, dx, dy, dz, frames): straight to a point beside another.</summary>
		public static void MoveRelative(GlobalScope.ScriptEngine engine)
		{
			int cast = engine.getWord();
			int to = engine.getWord();
			int dx = (int)engine.getDword(), dy = (int)engine.getDword(), dz = (int)engine.getDword();
			int frames = engine.getWord();
			GlobalScope.pl.CBasePlayer player = PlayerOf(cast);
			GlobalScope.pl.CBasePlayer other = PlayerOf(to);
			if (player == null || other == null) return;
			GlobalScope.VecFx32 at = other.getPosition();
			Move(player, new GlobalScope.VecFx32(at.x + dx, at.y + dy, at.z + dz), frames);
		}

		/// <summary>(cast, degrees fx32, frames, ?, keep motion): to the angle.</summary>
		public static void TurnAbsoluteAngle2(GlobalScope.ScriptEngine engine)
		{
			int cast = engine.getWord();
			uint degrees = engine.getDword();
			int frames = engine.getWord();
			engine.getDword();
			bool keepMotion = engine.getByte() != 0;
			GlobalScope.pl.CBasePlayer player = PlayerOf(cast);
			if (player != null) Turn(player, DegreesToIdx(degrees), frames, keepMotion);
		}

		/// <summary>(cast, degrees fx32, frames, ?, keep motion): by the angle from where it faces.</summary>
		public static void TurnRelativeAngle2(GlobalScope.ScriptEngine engine)
		{
			int cast = engine.getWord();
			uint degrees = engine.getDword();
			int frames = engine.getWord();
			engine.getDword();
			bool keepMotion = engine.getByte() != 0;
			GlobalScope.pl.CBasePlayer player = PlayerOf(cast);
			if (player != null) Turn(player, player.getRotation().y + DegreesToIdx(degrees), frames, keepMotion);
		}

		/// <summary>(cast, x, y, z, frames, ?, keep motion): toward the point.</summary>
		public static void TurnAbsoluteCoordination2(GlobalScope.ScriptEngine engine)
		{
			int cast = engine.getWord();
			GlobalScope.VecFx32 point = new GlobalScope.VecFx32((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
			int frames = engine.getWord();
			engine.getDword();
			bool keepMotion = engine.getByte() != 0;
			GlobalScope.pl.CBasePlayer player = PlayerOf(cast);
			if (player != null) Turn(player, YawBetween(player.getPosition(), point), frames, keepMotion);
		}

		/// <summary>(cast, at, frames, ?, keep motion): toward another.</summary>
		public static void TurnLookCharacter2(GlobalScope.ScriptEngine engine)
		{
			int cast = engine.getWord();
			int at = engine.getWord();
			int frames = engine.getWord();
			engine.getDword();
			bool keepMotion = engine.getByte() != 0;
			GlobalScope.pl.CBasePlayer player = PlayerOf(cast);
			GlobalScope.pl.CBasePlayer other = PlayerOf(at);
			if (player != null && other != null) Turn(player, YawBetween(player.getPosition(), other.getPosition()), frames, keepMotion);
		}

		/// <summary>(cast): holds while one of its moves runs (and FF3's MoveSys, for the moves still made its way).</summary>
		public static void MoveEndAutoIdle(GlobalScope.ScriptEngine engine)
		{
			GlobalScope.pl.CBasePlayer player = PlayerOf(engine.getWord());
			if (player == null) return;
			if (Running<LinerMove>(player) || player.MoveSys().isFlag()) engine.suspendRedo();
		}

		/// <summary>(cast): holds while one of its turns runs.</summary>
		public static void TurnEndAutoIdle(GlobalScope.ScriptEngine engine)
		{
			GlobalScope.pl.CBasePlayer player = PlayerOf(engine.getWord());
			if (player != null && Running<Rotation>(player)) engine.suspendRedo();
		}

		// ---- the strategies ----

		private static void Move(GlobalScope.pl.CBasePlayer player, GlobalScope.VecFx32 target, int frames)
		{
			// The move is the strategy's from here: FF3's MoveSys would pull it its own way.
			try { player.MoveSys().setFlag(_Flag: false); } catch (Exception) { }
			GlobalScope.VecFx32 from = new GlobalScope.VecFx32(player.getPosition());
			LinerMove move = new LinerMove { Player = player, Frames = frames };
			move.Target.copy(target);
			if (frames < 1)
			{
				Place(player, target);
				move.Done = true;
			}
			else
			{
				move.Dx = (target.x - from.x) / frames;
				move.Dy = (target.y - from.y) / frames;
				move.Dz = (target.z - from.z) / frames;
			}
			Register(move);
			if (player.TurnSys().isFixed()) return;
			// From where it stands now: a move of no frames has put it there already, and it does not turn.
			int yaw = YawBetween(player.getPosition(), target);
			if (yaw != -1) Register(NewRotation(player, yaw, 5, -1, -1));
		}

		private static void Turn(GlobalScope.pl.CBasePlayer player, int yaw, int frames, bool keepMotion)
		{
			Register(NewRotation(player, yaw & 0xFFFF, frames, keepMotion ? -1 : 1005, keepMotion ? -1 : 1000));
		}

		private static Rotation NewRotation(GlobalScope.pl.CBasePlayer player, int yaw, int frames, int startMotion, int endMotion)
		{
			Rotation r = new Rotation { Player = player, Yaw = yaw, Frames = frames, EndMotion = endMotion };
			if (frames < 1)
			{
				GlobalScope.VecFx32 rot = new GlobalScope.VecFx32(0, yaw & 0xFFFF, 0);
				Face(player, rot);
			}
			else
			{
				int current = player.getRotation().y;
				uint diff = (uint)(yaw - current);
				r.Negative = (diff & 0x8000) != 0;
				if (r.Negative) diff = (uint)(current - yaw);
				r.Step = (int)(diff & 0xFFFF) / frames;
				if (startMotion != -1 && Has(player, startMotion)) Start(player, startMotion, 5);
			}
			// A model without motions, or without the one to end on, is not turned (OSRotationByFrame's constructor).
			int ctrl = player.getCharacterId();
			if (GlobalScope.characterMng.getMotionNum(ctrl) < 1 || (endMotion != -1 && !Has(player, endMotion))) r.Done = true;
			return r;
		}

		private static void Register(Strategy s)
		{
			if (s.Done) return;
			int count = 0;
			foreach (Strategy r in _running)
				if (r.Player == s.Player) count++;
			if (count >= 4) return;
			s.Stage = StageNow();
			_running.Add(s);
		}

		/// <summary>Once a step, after its scripts: each strategy runs, in the order they were made, and the finished go.</summary>
		public static void Tick()
		{
			if (_running.Count == 0) return;
			for (int i = 0; i < _running.Count; i++)
			{
				Strategy s = _running[i];
				try
				{
					if (s.Player.getCharacterId() < 0) s.Done = true;
					else s.Update();
				}
				catch (Exception) { s.Done = true; }
			}
			_running.RemoveAll(s => s.Done);
		}

		/// <summary>A map left: its characters go, and what moved them - not what the next map's script has already set going
		/// (it can run before the host sees the stage change: the castle's corridor walks Cecil and Baigan from its first step).</summary>
		public static void MapLeft()
		{
			string stage = StageNow();
			_running.RemoveAll(s => !string.Equals(s.Stage, stage, StringComparison.OrdinalIgnoreCase));
		}

		private static string StageNow()
		{
			try { return GlobalScope.stg.CStageMng.CurrentName; } catch (Exception) { return null; }
		}

		// ---- helpers ----

		private static bool Running<T>(GlobalScope.pl.CBasePlayer player) where T : Strategy
		{
			foreach (Strategy s in _running)
				if (s.Player == player && s is T && !s.Done) return true;
			return false;
		}

		private static GlobalScope.pl.CBasePlayer PlayerOf(int cast)
		{
			int index = GlobalScope.CCastCommandTransit.getInstance().changeHichNumber((uint)cast);
			if (index == -1) return null;
			try { return GlobalScope.CCastCommandTransit.getInstance().cast_PlayerMng().Player(index); }
			catch (Exception) { return null; }
		}

		private static void Place(GlobalScope.pl.CBasePlayer player, GlobalScope.VecFx32 at)
		{
			GlobalScope.VecFx32 p = new GlobalScope.VecFx32(at);
			player.setPosition(p);
			player.getPrePosition_set(p);
		}

		/// <summary>The yaw set, and the direction the turn system eases toward with it - so it does not turn the
		/// character back to where FF3's last move pointed.</summary>
		private static void Face(GlobalScope.pl.CBasePlayer player, GlobalScope.VecFx32 rot)
		{
			player.setRotation(rot);
			GlobalScope.VecFx32 dir = new GlobalScope.VecFx32(GlobalScope.FX_SinIdx(rot.y & 0xFFFF), 0, GlobalScope.FX_CosIdx(rot.y & 0xFFFF));
			player.setDirection(dir);
			player.setTargetDirection(dir);
		}

		/// <summary>utl::computeYaw2Vectors: the yaw from one point to another, -1 when they are the same.</summary>
		private static int YawBetween(GlobalScope.VecFx32 from, GlobalScope.VecFx32 to)
		{
			GlobalScope.VecFx32 d = new GlobalScope.VecFx32(to.x - from.x, to.y - from.y, to.z - from.z);
			if (GlobalScope.VEC_Mag(d) == 0) return -1;
			GlobalScope.VEC_Normalize(d, d);
			return GlobalScope.FX_Atan2Idx(d.x, d.z) & 0xFFFF;
		}

		/// <summary>An angle's whole degrees (fx32) as a 65536-step index, as the Turn commands read it.</summary>
		private static int DegreesToIdx(uint degrees)
		{
			int whole = (int)((degrees & 0xFFFF000) << 4);
			return (whole / 360) & 0xFFFF;
		}

		private static bool Has(GlobalScope.pl.CBasePlayer player, int motion) =>
			GlobalScope.characterMng.isMotion(player.getCharacterId(), (int)GameProfile.FieldMotionId((uint)motion));

		private static void Start(GlobalScope.pl.CBasePlayer player, int motion, uint blend) =>
			GlobalScope.characterMng.startMotion(player.getCharacterId(), (int)GameProfile.FieldMotionId((uint)motion), true, blend);
	}
}

// FF4's field event camera: the camera the map scripts drive with moveCamera_*,
// setCamera_*Gaze and setCameraOffset.
//
// FF4's world::EventCamera keeps a position and a target and moves each linearly over a
// number of frames (setPositionLinerMove / setTargetLinerMove); setCameraOffset puts it
// in a follow mode a fixed offset from the party's leader (CUFollowCamera::set with a
// position offset and, from there, a target offset). FF3's handlers for the same commands
// set the world camera's position and target in MODE_FREE, whose controller rebuilds the
// position from a distance and an angle every frame - a distance FF4's maps never set, so
// every scripted shot ended up at the target with the party's legs filling the screen.
// This drives the camera directly through CWorldCamera.ExternalDrive, as the scene camera
// motions do, and lets go when the script hands the camera back (changeCamera_Mode,
// setCamera_BeforeEvent, cancelCameraControl, a LookPlayer, a map change).

using System;

namespace FF3
{
	internal static class Ff4EventCamera
	{
		private static bool _active;
		private static GlobalScope.VecFx32 _pos = new GlobalScope.VecFx32(0, 0, 0);
		private static GlobalScope.VecFx32 _trg = new GlobalScope.VecFx32(0, 0, 0);

		private static GlobalScope.VecFx32 _posFrom = new GlobalScope.VecFx32(0, 0, 0), _posTo = new GlobalScope.VecFx32(0, 0, 0);
		private static int _posFrames, _posTick;
		private static GlobalScope.VecFx32 _trgFrom = new GlobalScope.VecFx32(0, 0, 0), _trgTo = new GlobalScope.VecFx32(0, 0, 0);
		private static int _trgFrames, _trgTick;

		private static bool _follow;
		private static GlobalScope.VecFx32 _followPos = new GlobalScope.VecFx32(0, 0, 0), _followTrg = new GlobalScope.VecFx32(0, 0, 0);

		public static bool Active => _active;

		private static GlobalScope.cmr.CWorldCamera Camera
		{
			get
			{
				try { return GlobalScope.CCastCommandTransit.getInstance().cast_FieldCamera(); }
				catch (Exception) { return null; }
			}
		}

		/// <summary>Takes the camera over where it stands, unless a scene camera motion has it.</summary>
		private static bool Take()
		{
			if (_active) return true;
			GlobalScope.cmr.CWorldCamera camera = Camera;
			if (camera == null || Ff4CameraMotion.Playing) return false;
			_pos = Copy(camera.getPosition());
			_trg = Copy(camera.getTarget());
			_posFrames = _trgFrames = 0;
			_follow = false;
			_active = true;
			GlobalScope.cmr.CWorldCamera.ExternalDrive = Drive;
			return true;
		}

		/// <summary>Hands the camera back to the field's own controller.</summary>
		public static void Release()
		{
			if (!_active) return;
			_active = false;
			_follow = false;
			_posFrames = _trgFrames = 0;
			if (GlobalScope.cmr.CWorldCamera.ExternalDrive == (Action<GlobalScope.cmr.CWorldCamera>)Drive)
			{
				GlobalScope.cmr.CWorldCamera.ExternalDrive = null;
			}
		}

		private static GlobalScope.VecFx32 Copy(GlobalScope.VecFx32 v) => new GlobalScope.VecFx32(v.x, v.y, v.z);

		// ---- the script commands ----

		/// <summary>moveCamera_AbsoluteCoordination(x, y, z, frames, alsoTarget, ?).</summary>
		public static void MoveTo(int x, int y, int z, int frames, bool alsoTarget)
		{
			if (!Take()) return;
			_follow = false;
			GlobalScope.VecFx32 to = new GlobalScope.VecFx32(x, y, z);
			if (alsoTarget)
			{
				// The target keeps its bearing from the camera: it moves by the same delta.
				GlobalScope.VecFx32 delta = new GlobalScope.VecFx32(to.x - _pos.x, to.y - _pos.y, to.z - _pos.z);
				StartTarget(new GlobalScope.VecFx32(_trg.x + delta.x, _trg.y + delta.y, _trg.z + delta.z), frames);
			}
			StartPosition(to, frames);
		}

		/// <summary>moveCamera_RelativeCoordination(dx, dy, dz, frames, alsoTarget, ?).</summary>
		public static void MoveBy(int dx, int dy, int dz, int frames, bool alsoTarget)
		{
			if (!Take()) return;
			MoveTo(_pos.x + dx, _pos.y + dy, _pos.z + dz, frames, alsoTarget);
		}

		/// <summary>setCamera_AbsoluteGaze(x, y, z, frames, ?).</summary>
		public static void LookAt(int x, int y, int z, int frames)
		{
			if (!Take()) return;
			_follow = false;
			StartTarget(new GlobalScope.VecFx32(x, y, z), frames);
		}

		/// <summary>setCamera_RelativeGaze(dx, dy, dz, frames, ?).</summary>
		public static void LookBy(int dx, int dy, int dz, int frames)
		{
			if (!Take()) return;
			LookAt(_trg.x + dx, _trg.y + dy, _trg.z + dz, frames);
		}

		/// <summary>setCameraOffset: the camera at leader + posOffset, looking at that point + trgOffset, every frame.</summary>
		public static void Follow(GlobalScope.VecFx32 posOffset, GlobalScope.VecFx32 trgOffset)
		{
			if (!Take()) return;
			_follow = true;
			_followPos = Copy(posOffset);
			_followTrg = Copy(trgOffset);
			_posFrames = _trgFrames = 0;
		}

		private static void StartPosition(GlobalScope.VecFx32 to, int frames)
		{
			if (frames <= 0)
			{
				_pos = to;
				_posFrames = 0;
				return;
			}
			_posFrom = Copy(_pos);
			_posTo = to;
			_posFrames = frames;
			_posTick = 0;
		}

		private static void StartTarget(GlobalScope.VecFx32 to, int frames)
		{
			if (frames <= 0)
			{
				_trg = to;
				_trgFrames = 0;
				return;
			}
			_trgFrom = Copy(_trg);
			_trgTo = to;
			_trgFrames = frames;
			_trgTick = 0;
		}

		private static GlobalScope.VecFx32 Lerp(GlobalScope.VecFx32 a, GlobalScope.VecFx32 b, int tick, int frames)
		{
			return new GlobalScope.VecFx32(
				a.x + (int)((long)(b.x - a.x) * tick / frames),
				a.y + (int)((long)(b.y - a.y) * tick / frames),
				a.z + (int)((long)(b.z - a.z) * tick / frames));
		}

		/// <summary>Each frame: the moves advance; the follow mode reads the leader.</summary>
		public static void Tick()
		{
			if (!_active) return;
			if (_posFrames > 0)
			{
				_posTick++;
				_pos = _posTick >= _posFrames ? Copy(_posTo) : Lerp(_posFrom, _posTo, _posTick, _posFrames);
				if (_posTick >= _posFrames) _posFrames = 0;
			}
			if (_trgFrames > 0)
			{
				_trgTick++;
				_trg = _trgTick >= _trgFrames ? Copy(_trgTo) : Lerp(_trgFrom, _trgTo, _trgTick, _trgFrames);
				if (_trgTick >= _trgFrames) _trgFrames = 0;
			}
			if (_follow)
			{
				GlobalScope.pl.CBasePlayer hero = EngineApi.HeroPlayer;
				if (hero != null)
				{
					GlobalScope.VecFx32 at = hero.getPosition();
					_pos = new GlobalScope.VecFx32(at.x + _followPos.x, at.y + _followPos.y, at.z + _followPos.z);
					_trg = new GlobalScope.VecFx32(_pos.x + _followTrg.x, _pos.y + _followTrg.y, _pos.z + _followTrg.z);
				}
			}
		}

		private static void Drive(GlobalScope.cmr.CWorldCamera camera)
		{
			if (!_active) return;
			try
			{
				GlobalScope.VecFx32 pos = Copy(_pos);
				GlobalScope.VecFx32 trg = Copy(_trg);
				if (pos.x == trg.x && pos.y == trg.y && pos.z == trg.z)
				{
					trg.z -= 4096;
				}
				camera.Pos_set(pos);
				camera.setPrePos(pos);
				camera.Trg_set(trg);
				camera.setPreTrg(trg);
				camera.setPosition(pos);
				camera.setTarget(trg);
				camera.setCamUp(0, 4096, 0);
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "script: FF4 event camera: " + ex.Message);
				Release();
			}
		}
	}
}

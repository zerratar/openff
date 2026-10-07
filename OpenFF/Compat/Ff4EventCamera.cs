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
// setCamera_BeforeEvent, cancelCameraControl, a LookPlayer, a map change). It takes the camera
// where it stands, so taking it over is no cut; putting it somewhere at once is, and the frame
// capture is told so (a move over frames is motion, blended as ever).

using System;

namespace OpenFF.Client
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
		private static bool _followStarts;   // the follow has not put the camera at the leader yet: a cut when it does
		private static GlobalScope.VecFx32 _followPos = new GlobalScope.VecFx32(0, 0, 0), _followTrg = new GlobalScope.VecFx32(0, 0, 0);
		// CUFollowCamera: whom it follows (moveCamera_LookPlayer2 names them; setCameraOffset keeps them; the hero when none),
		// and its way there - over frames from where the camera stands to the point it was set to (the character's place then
		// and the offset), a step a frame; at the end set there, and from then on the character's place and the offset.
		private static GlobalScope.pl.CBasePlayer _followChar;
		private static int _followFrames = -1;
		private static GlobalScope.VecFx32 _followAt = new GlobalScope.VecFx32(0, 0, 0), _followStep = new GlobalScope.VecFx32(0, 0, 0);

		// The field of view: the camera keeps sin and cos of half the vertical angle (fx32); the
		// field's own is about 30 degrees. Set for the battle stage, put back on Release.
		private static bool _fovSet;
		private static int _fovSin, _fovCos, _fovSavedSin, _fovSavedCos;
		private static bool _clipSet;
		private static int _clipNear, _clipFar;

		public static bool Active => _active;

		private static GlobalScope.cmr.CWorldCamera Camera
		{
			get
			{
				try { return GlobalScope.CCastCommandTransit.getInstance().cast_FieldCamera(); }
				catch (Exception) { return null; }
			}
		}

		// The map whose script last drove the camera: leaving another (the host sees a map change after the next map's
		// first steps have run - the castle corridor sets its camera there) keeps it (MapLeft).
		private static string _stage;

		/// <summary>A map left: the camera goes back to the field's controller - unless the next map's script has it already.</summary>
		public static void MapLeft()
		{
			string now = null;
			try { now = GlobalScope.stg.CStageMng.CurrentName; } catch (Exception) { }
			if (_active && _stage != null && string.Equals(_stage, now, StringComparison.OrdinalIgnoreCase)) return;
			Release();
			_followChar = null;
		}

		/// <summary>Takes the camera over where it stands, unless a scene camera motion has it.</summary>
		private static bool Take()
		{
			try { _stage = GlobalScope.stg.CStageMng.CurrentName; } catch (Exception) { }
			if (_active) return true;
			GlobalScope.cmr.CWorldCamera camera = Camera;
			if (camera == null || Ff4CameraMotion.Playing) return false;
			_pos = Copy(camera.getPosition());
			_trg = Copy(camera.getTarget());
			_posFrames = _trgFrames = 0;
			_follow = _followStarts = false;
			_fovSet = false;
			try { camera.getFOV(out _fovSavedSin, out _fovSavedCos); } catch (Exception) { _fovSavedSin = 0; }
			_active = true;
			GlobalScope.cmr.CWorldCamera.ExternalDrive = Drive;
			return true;
		}

		/// <summary>Hands the camera back to the field's own controller.</summary>
		public static void Release()
		{
			if (!_active) return;
			_active = false;
			_follow = _followStarts = false;
			_posFrames = _trgFrames = 0;
			if (_fovSet && _fovSavedSin > 0)
			{
				try { Camera?.setFOV(_fovSavedSin, _fovSavedCos); } catch (Exception) { }
			}
			_fovSet = false;
			_clipSet = false;
			if (GlobalScope.cmr.CWorldCamera.ExternalDrive == (Action<GlobalScope.cmr.CWorldCamera>)Drive)
			{
				GlobalScope.cmr.CWorldCamera.ExternalDrive = null;
			}
		}

		private static GlobalScope.VecFx32 Copy(GlobalScope.VecFx32 v) => new GlobalScope.VecFx32(v.x, v.y, v.z);

		/// <summary>A vertical field of view in degrees while the event camera drives; the field's own returns on Release.</summary>
		public static void SetFov(float degrees)
		{
			if (!Take()) return;
			double half = Math.Clamp(degrees, 5f, 120f) * Math.PI / 360.0;
			_fovSin = (int)Math.Round(Math.Sin(half) * 4096);
			_fovCos = (int)Math.Round(Math.Cos(half) * 4096);
			_fovSet = true;
		}

		/// <summary>Near and far clip planes (world units) while the event camera drives; the field's map setup restores its own on the next map.</summary>
		public static void SetClip(float near, float far)
		{
			if (!Take()) return;
			_clipNear = (int)Math.Round(near * 4096);
			_clipFar = (int)Math.Round(far * 4096);
			_clipSet = true;
		}

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

		/// <summary>
		/// The camera put at a place looking at a point, as a step of a move the caller drives a frame at a time (the battle's
		/// boss entrance and invoke close-up): no cut declared, so the frames between the steps are smoothed.
		/// </summary>
		public static void Place(int x, int y, int z, int tx, int ty, int tz)
		{
			if (!Take()) return;
			_follow = false;
			_pos = new GlobalScope.VecFx32(x, y, z);
			_trg = new GlobalScope.VecFx32(tx, ty, tz);
			_posFrames = _trgFrames = 0;
		}

		/// <summary>setCamera_RelativeGaze(dx, dy, dz, frames, ?).</summary>
		public static void LookBy(int dx, int dy, int dz, int frames)
		{
			if (!Take()) return;
			LookAt(_trg.x + dx, _trg.y + dy, _trg.z + dz, frames);
		}

		/// <summary>setCameraOffset (EventCamera::setFollow with no character): the camera at the followed character + posOffset,
		/// looking at that point + trgOffset, every frame - reached over <paramref name="frames"/>, or at once.</summary>
		public static void Follow(GlobalScope.VecFx32 posOffset, GlobalScope.VecFx32 trgOffset, int frames = 0) => Follow(null, posOffset, trgOffset, frames);

		/// <summary>moveCamera_LookPlayer2: the character the follow follows from now on (the camera not taken).</summary>
		public static void FollowWhom(GlobalScope.pl.CBasePlayer character)
		{
			if (character != null) _followChar = character;
		}

		/// <summary>CUFollowCamera::set: <paramref name="character"/> followed (null: the one followed already), the camera
		/// put at its place + posOffset (looking at that + trgOffset) at once, or moved there over the frames.</summary>
		public static void Follow(GlobalScope.pl.CBasePlayer character, GlobalScope.VecFx32 posOffset, GlobalScope.VecFx32 trgOffset, int frames)
		{
			if (!Take()) return;
			if (character != null) _followChar = character;
			_follow = true;
			_followPos = Copy(posOffset);
			_followTrg = Copy(trgOffset);
			_posFrames = _trgFrames = 0;
			GlobalScope.VecFx32 at = Followed()?.getPosition();
			if (at == null || frames < 1)
			{
				_followFrames = -1;
				_followStarts = true;
				return;
			}
			_followAt = new GlobalScope.VecFx32(at.x + posOffset.x, at.y + posOffset.y, at.z + posOffset.z);
			_followStarts = false;
			_followFrames = frames;
			_followStep = new GlobalScope.VecFx32((_followAt.x - _pos.x) / frames, (_followAt.y - _pos.y) / frames, (_followAt.z - _pos.z) / frames);
		}

		private static GlobalScope.pl.CBasePlayer Followed()
		{
			GlobalScope.pl.CBasePlayer c = _followChar;
			try { if (c != null && c.getCharacterId() >= 0) return c; } catch (Exception) { }
			return EngineApi.HeroPlayer;
		}

		private static void StartPosition(GlobalScope.VecFx32 to, int frames)
		{
			if (frames <= 0)
			{
				FrameCapture.CameraCut();   // put there at once: a cut, unless it was there already
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
				FrameCapture.CameraCut();   // turned there at once: a cut, unless it looked there already
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
			if (_follow && _followFrames > 0)
			{
				// On the way: a step a frame; the last frame sets it on the point (CUFollowCamera::update_).
				_followFrames--;
				_pos = new GlobalScope.VecFx32(_pos.x + _followStep.x, _pos.y + _followStep.y, _pos.z + _followStep.z);
				_trg = new GlobalScope.VecFx32(_pos.x + _followTrg.x, _pos.y + _followTrg.y, _pos.z + _followTrg.z);
			}
			else if (_follow && _followFrames == 0)
			{
				_followFrames = -1;
				_pos = Copy(_followAt);
				_trg = new GlobalScope.VecFx32(_pos.x + _followTrg.x, _pos.y + _followTrg.y, _pos.z + _followTrg.z);
			}
			else if (_follow)
			{
				GlobalScope.pl.CBasePlayer hero = Followed();
				if (hero != null)
				{
					GlobalScope.VecFx32 at = hero.getPosition();
					_pos = new GlobalScope.VecFx32(at.x + _followPos.x, at.y + _followPos.y, at.z + _followPos.z);
					_trg = new GlobalScope.VecFx32(_pos.x + _followTrg.x, _pos.y + _followTrg.y, _pos.z + _followTrg.z);
					if (_followStarts)
					{
						// The camera goes to the leader at once (it is set there at the next update): a cut, unless it was there already.
						_followStarts = false;
						FrameCapture.CameraCut();
					}
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
				if (_fovSet) camera.setFOV(_fovSin, _fovCos);
				if (_clipSet) camera.setClip(_clipNear, _clipFar);
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "script: FF4 event camera: " + ex.Message);
				Release();
			}
		}
	}
}
